/*===============================================================
* Product:    Com2Verse
* File Name:  SoundManager.cs
* Developer:  yangsehoon
* Date:       2022-04-08 09:53
* History:
* Documents:  Sound Manager
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.Threading;
using Com2Verse.AssetSystem;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.Video;

namespace Com2Verse.Sound
{
    public enum eFadingMode
    {
        LINEAR,
        QUADRATIC,
        SQUAREROOT,
        SMOOTHSTEP
    }
    public sealed class SoundManager : MonoSingleton<SoundManager>
    {
        private struct FadeInfo
        {
            public float FadeRemTime;
            public float FromVolume;
            public float ToVolume;
        }

        private AudioMixer _mainMixer;

        private readonly int MaxDecibel = 0;
        private readonly int MinDecibel = -60;
        private readonly int MaxPriority = 255;
        private readonly int MinPriority = 0;

        public const float BGMDefaultVolume = 0.3f;
        public const float UIDefaultVolume = 0.3f;

        private MetaverseAudioSource _uiAudioSource;
        private MetaverseAudioSource _bgmAudioSource;
        private FadeInfo _fadeInfo;

        private readonly Dictionary<int, AudioMixerGroup>         _mixerGroupMap                  = new();
        private readonly Dictionary<int, string>                  _volumeExposedFloatName         = new();
        private readonly Dictionary<int, CancellationTokenSource> _volumeSmoothCancellationTokens = new();

        private int _uiGroupIndex;
        private int _bgmGroupIndex;
        private int _videoGroupIndex;

        // local  variables
        private readonly AudioMixerSnapshot[] _transitionGroup = {null};
        private readonly float[] _transitionWeight = {1};

        private List<C2VAsyncOperationHandle> _assetHandles = new ();

        public int BGMGroupIndex => _bgmGroupIndex;
        public int VideoGroupIndex => _videoGroupIndex;

        [NotNull] private readonly Events _events = Events.Empty;
        public class Events
        {
            public delegate void VolumeChanged(int mixerGroup, float volume);
            public VolumeChanged OnVolumeChanged;
            [NotNull] public static Events Empty { get; } = new() {OnVolumeChanged = (_, _) => { }};
        }
        public void Initialize(AudioMixer mainMixer, int groupCount, int ui, int bgm, int video)
        {
            _uiGroupIndex = ui;
            _bgmGroupIndex = bgm;
            _videoGroupIndex = video;

            LoadData(mainMixer);

            SetUpUIAudioSource();
            SetUpBGMSource();
            SetUpVolumeParams(groupCount);
        }

        private void SetUpVolumeParams(int groupCount)
        {
            for (int i = 0; i <= groupCount; i++)
            {
                _volumeExposedFloatName.Add(i, $"Volume{i}");
            }
        }

 #region Internal
        private void SetUpUIAudioSource()
        {
            _uiAudioSource = MetaverseAudioSource.CreateNew(gameObject);
            _uiAudioSource.Mute = false;
            _uiAudioSource.BypassEffects = true;
            _uiAudioSource.BypassListenerEffects = true;
            _uiAudioSource.BypassReverbZones = true;
            _uiAudioSource.TargetMixerGroup = _uiGroupIndex;
            _uiAudioSource.Priority = MaxPriority;
            _uiAudioSource.SpatialBlend = 0;
            _uiAudioSource.Volume = UIDefaultVolume;
        }

        private void SetUpBGMSource()
        {
            _bgmAudioSource = MetaverseAudioSource.CreateNew(gameObject);
            _bgmAudioSource.Mute = false;
            _bgmAudioSource.Loop = true;
            _bgmAudioSource.TargetMixerGroup = _bgmGroupIndex;
            _bgmAudioSource.Priority = MinPriority;
            _bgmAudioSource.SpatialBlend = 0;
            _bgmAudioSource.Volume = BGMDefaultVolume;
        }
        private void LoadData(AudioMixer mainMixer)
        {
            _mainMixer = mainMixer;

            if (_mainMixer != null)
            {
                _mixerGroupMap.Clear();
                var groups = _mainMixer.FindMatchingGroups("Master");
                var masterGroup = groups[0];
                _mixerGroupMap.Add(0, masterGroup);

                foreach (var group in groups)
                {
                    if (!group.name.StartsWith('*') && group != groups[0])
                    {
                        string groupIndex = group.name.Split("_")[^1];
                        if (int.TryParse(groupIndex, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int index))
                        {
                            _mixerGroupMap.Add(index, group);
                        }
                    }
                }
            }
            else
            {
                C2VDebug.LogError("Failed to load main mixer");
            }
        }

        private void RegisterHandle(C2VAsyncOperationHandle handle)
        {
            _assetHandles.Add(handle);
        }

        public void ClearAssetHandle()
        {
            foreach (var handle in _assetHandles)
            {
                if (handle.IsValid())
                {
                    handle.Release();
                }
            }
            
            _assetHandles.Clear();
        }

        private UniTask<T> LoadClip<T>(AssetReference reference, bool doNotReleaseHandle = false) where T : UnityEngine.Object
        {
            return LoadClipWithHandle(C2VAddressables.LoadAssetAsync<T>(reference), doNotReleaseHandle);
        }

        private UniTask<T> LoadClip<T>(string addressableName, bool doNotReleaseHandle = false) where T : UnityEngine.Object
        {
            return LoadClipWithHandle(C2VAddressables.LoadAssetAsync<T>(addressableName), doNotReleaseHandle);
        }

        private UniTask<T> LoadClipWithHandle<T>(C2VAsyncOperationHandle<T> assetLoadHandle, bool doNotReleaseHandle = false) where T : UnityEngine.Object
        {
            if (assetLoadHandle == null) return UniTask.FromResult<T>(null);
            else if (!doNotReleaseHandle) RegisterHandle(assetLoadHandle);

            return assetLoadHandle.ToUniTask();
        }

        private void FadeInternal(eFadingMode mode, float duration, bool isIn)
        {
            bool startNewCoroutine = _fadeInfo.FadeRemTime == 0;

            _fadeInfo.FadeRemTime = duration;
            _fadeInfo.FromVolume = AudioListener.volume;
            _fadeInfo.ToVolume = isIn ? 1 : 0;
            if (startNewCoroutine)
            {
                FadeCoroutine(mode, duration).Forget();
            }
        }
#endregion

#region Public Method
        public AudioMixerGroup GetMixerGroup(int groupIndex)
        {
            return _mixerGroupMap.TryGetValue(groupIndex, out AudioMixerGroup group) ? group : null;
        }

        public int GetMixerGroup(AudioMixerGroup group)
        {
            foreach (var pair in _mixerGroupMap)
            {
                if (pair.Value == group)
                {
                    return pair.Key;
                }
            }

            return 0;
        }

        public async UniTask PlayOneShot(AudioSource source, AssetReference reference, float volumeScale = 1.0f)
        {
            AudioClip clip = await LoadClip<AudioClip>(reference);
            source.PlayOneShot(clip, volumeScale);
        }

        public async UniTask PlayOneShot(AudioSource source, string path, float volumeScale = 1.0f)
        {
            AudioClip clip = await LoadClip<AudioClip>(path);
            source.PlayOneShot(clip, volumeScale);
        }

        public void PlayOneShot(AudioSource source, AudioClip clip, float volumeScale = 1.0f)
        {
            source.PlayOneShot(clip, volumeScale);
        }

        public void PlayUISound(AssetReference reference, float volumeScale = 1.0f)
        {
            if (!_uiAudioSource.IsUnityNull())
                _uiAudioSource!.PlayOneShot(reference, volumeScale);
        }

        public void PlayBGM(AssetReference reference, float fadeDuration, float fadeInDelay, float targetVolume = BGMDefaultVolume)
        {
            if (!_bgmAudioSource.IsUnityNull())
                _bgmAudioSource!.CrossFadeTo(reference, fadeDuration, fadeInDelay, eFadingMode.SMOOTHSTEP, targetVolume, true).Forget();
        }

        public void StopBGM()
        {
            if (_bgmAudioSource.IsUnityNull()) return;
            _bgmAudioSource.Stop();
        }

        public void PlayUISound(string path, float volumeScale = 1.0f)
        {
            if (!_uiAudioSource.IsUnityNull())
                _uiAudioSource!.PlayOneShot(path, volumeScale);
        }

        public async UniTask<AudioClip> GetClip(AssetReference reference)
        {
            return await LoadClip<AudioClip>(reference);
        }

        public async UniTask<AudioClip> GetClip(string path)
        {
            return await LoadClip<AudioClip>(path);
        }

        public void UnLoadSoundClip(C2VAsyncOperationHandle handle)
        {
            handle.Release();
        }

        public void Play(AudioSource source, ulong delay = 0)
        {
            if (source.clip != null)
            {
                source.Play(delay);
            }
        }

        public void Play(VideoPlayer source)
        {
            if (source.clip != null)
            {
                source.Play();
            }
        }

        public async UniTask Play(AudioSource source, AssetReference reference, ulong delay = 0, bool doNotReleaseHandle = false)
        {
            AudioClip clip = await LoadClip<AudioClip>(reference, doNotReleaseHandle);
            Play(source, clip, delay);
        }

        public async UniTask Play(VideoPlayer source, AssetReference reference, bool doNotReleaseHandle = false)
        {
            VideoClip clip = await LoadClip<VideoClip>(reference);
            Play(source, clip);
        }

        public void Play(AudioSource source, AudioClip clip, ulong delay = 0)
        {
            source.clip = clip;
            Play(source, delay);
        }

        public void Play(VideoPlayer source, VideoClip clip)
        {
            source.clip = clip;
            Play(source);
        }

        public void Stop(AudioSource source)
        {
            source.Stop();
        }

        public void Pause(AudioSource source)
        {
            source.Pause();
        }

        public void UnPause(AudioSource source)
        {
            source.UnPause();
        }

        public float GetVolume(int mixerGroup)
        {
            if (_volumeExposedFloatName.TryGetValue(mixerGroup, out string parameterName) && _mixerGroupMap.ContainsKey(mixerGroup))
            {
                _mainMixer.GetFloat(parameterName, out float volume);
                return SoundUtil.NormalizeVolume(volume, MaxDecibel, MinDecibel);
            }

            return 0;
        }

        public void SetVolume(int mixerGroup, float volumeNormalized)
        {
            float volume = SoundUtil.DeNormalizeVolume(volumeNormalized, MaxDecibel, MinDecibel);
            if (_volumeExposedFloatName.TryGetValue(mixerGroup, out string parameterName) && _mixerGroupMap.ContainsKey(mixerGroup))
            {
                _mainMixer.SetFloat(parameterName, volume);
                _events.OnVolumeChanged?.Invoke(mixerGroup, volume);
            }
        }

        public void SetVolumeSmooth(int mixerGroup, float volumeNormalized, float duration)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            if (_volumeSmoothCancellationTokens.TryGetValue(mixerGroup, out var tokenSource))
            {
                tokenSource.Cancel();
                _volumeSmoothCancellationTokens[mixerGroup] = source;
            }
            else
            {
                _volumeSmoothCancellationTokens.Add(mixerGroup, source);
            }

            SetlVolumeSmoothTask(mixerGroup, volumeNormalized, duration, source.Token).Forget();
        }

        private async UniTask SetlVolumeSmoothTask(int mixerGroup, float targetVolume, float duration, CancellationToken cancel)
        {
            float currentVolume = GetNormalizedVolume(mixerGroup);
            float elapsed = 0;

            while (elapsed < duration)
            {
                if (cancel.IsCancellationRequested)
                {
                    return;
                }

                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                elapsed = Mathf.Min(elapsed + Time.deltaTime, duration);
                SetVolume(mixerGroup, (targetVolume - currentVolume) * elapsed / duration + currentVolume);
            }

            if (_volumeSmoothCancellationTokens.Remove((int) mixerGroup, out var source))
            {
                source.Dispose();
            }
        }

        public float GetNormalizedVolume(int mixerGroup)
        {
            if (_volumeExposedFloatName.TryGetValue(mixerGroup, out string parameterName) && _mainMixer.GetFloat(parameterName, out float value))
            {
                return SoundUtil.NormalizeVolume(value, MaxDecibel, MinDecibel);
            }

            return 0;
        }

        public void SnapshotTransition(string snapshotName, float transitionDuration)
        {
            AudioMixerSnapshot snapshot = _mainMixer.FindSnapshot(snapshotName);
            if (snapshot != null)
            {
                _transitionGroup[0] = snapshot;
                _mainMixer.TransitionToSnapshots(_transitionGroup, _transitionWeight, transitionDuration);
            }
        }

        public void FadeIn(eFadingMode mode, float duration)
        {
            FadeInternal(mode, duration, true);
        }

        public void FadeOut(eFadingMode mode, float duration)
        {
            FadeInternal(mode, duration, false);
        }

        private async UniTask FadeCoroutine(eFadingMode mode, float duration)
        {
            while (_fadeInfo.FadeRemTime > 0)
            {
                _fadeInfo.FadeRemTime -= Time.deltaTime;
                float ratio = SoundUtil.CalculateFadeRatio(mode, duration - _fadeInfo.FadeRemTime, duration);
                AudioListener.volume = _fadeInfo.FromVolume + (_fadeInfo.ToVolume - _fadeInfo.FromVolume) * ratio;
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            }

            AudioListener.volume = _fadeInfo.ToVolume;
            _fadeInfo.FadeRemTime = 0;
        }

        public void SetOnVolumeChanged(Events.VolumeChanged onVolumeChanged) => _events.OnVolumeChanged = onVolumeChanged;
#endregion
    }
}
