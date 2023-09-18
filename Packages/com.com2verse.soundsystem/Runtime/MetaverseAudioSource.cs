/*===============================================================
* Product:    Com2Verse
* File Name:  MetaverseAudioSource.cs
* Developer:  yangsehoon
* Date:       2022-04-11 09:40
* History:    
* Documents:  Unity AudioSource wrapper
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;
using System.Threading;
using Com2Verse.AssetSystem;
using Com2Verse.Extension;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Com2Verse.Sound
{
    [AddComponentMenu("Audio/Metaverse Audio Source")]
    public class MetaverseAudioSource : MonoBehaviour
    {
        public enum eAudioClipType
        {
            ASSET_REFERENCE,
            NORMAL_CLIP,
            NORMAL_SOURCE,
        }
        
        [SerializeField][HideInInspector] private AudioSource _audioSource;
        [SerializeField] private eAudioClipType _audioClipType;
        [SerializeField] private AssetReference _audioFile;
        [SerializeField][Tooltip("Load AssetReference on Awake?")] private bool _bindOnLoad = true;
        [SerializeField] private bool _doNotDestroyOnLoad;
        [SerializeField] private bool _playOnAwake;

        private AudioSource _lastFadeSource;
        private C2VAsyncOperationHandle _lastCrossFadeAssetHandle = null;
        private UniTask _crossFadeTask;
        private CancellationTokenSource _crossFadeCancelSource;
        private readonly float _maxFadeDeltaTime = 0.016f;
        
        public AudioSource AudioSource
        {
            get => _audioSource;
#if UNITY_EDITOR
            set => _audioSource = value;
#endif
        }

        public eAudioClipType AudioClipType
        {
            get => _audioClipType;
            set => _audioClipType = value;
        }

        public int TargetMixerGroup
        {
            get
            {
                return SoundManager.Instance.GetMixerGroup(_audioSource.outputAudioMixerGroup);
            }
            set
            {
                _audioSource.outputAudioMixerGroup = SoundManager.Instance.GetMixerGroup(value);
            }
        }

#region Wrapped Properties
        public float SpatialBlend
        {
            get => _audioSource.spatialBlend;
            set => _audioSource.spatialBlend = value;
        }
        public bool IsPlaying
        {
            get => _audioSource.isPlaying;
        }
        public bool Mute
        {
            get => _audioSource.mute;
            set => _audioSource.mute = value;
        }
        public bool BypassEffects
        {
            get => _audioSource.bypassEffects;
            set => _audioSource.bypassEffects = value;
        }
        public bool BypassListenerEffects
        {
            get => _audioSource.bypassListenerEffects;
            set => _audioSource.bypassListenerEffects = value;
        }
        public bool BypassReverbZones
        {
            get => _audioSource.bypassReverbZones;
            set => _audioSource.bypassReverbZones = value;
        }
        public bool Loop
        {
            get => _audioSource.loop;
            set => _audioSource.loop = value;
        }
        public int Priority
        {
            get => _audioSource.priority;
            set => _audioSource.priority = value;
        }
        public float Volume
        {
            get => _audioSource.volume;
            set => _audioSource.volume = value;
        }
        public float Pitch
        {
            get => _audioSource.pitch;
            set => _audioSource.pitch = value;
        }
        public AudioRolloffMode RolloffMode
        {
            get => _audioSource.rolloffMode;
            set => _audioSource.rolloffMode = value;
        }
        public float MinDistance
        {
            get => _audioSource.minDistance;
            set => _audioSource.minDistance = value;
        }
        public float MaxDistance
        {
            get => _audioSource.maxDistance;
            set => _audioSource.maxDistance = value;
        }

        /// <param name="sampleData">Result array</param>
        /// <param name="offset">Data offset in ms (only works with AudioClip)</param>
        /// <param name="channel">Target channel</param>
        /// <param name="useClipData">Set true to get absolute samples</param>
        /// <param name="applySourceVolume">Apply source volume to clip</param>
        public void GetData(float[] sampleData, int offset = 0, int channel = 0, bool useClipData = true, bool applySourceVolume = false)
        {
            if (useClipData && _audioClipType == eAudioClipType.NORMAL_CLIP)
            {
                var offsetSec     = MathUtil.ToSeconds(offset);
                var samplesPerSec = _audioSource!.clip!.frequency;
                var offsetSamples = Mathf.RoundToInt(offsetSec * samplesPerSec);
                
                var positionSamples      = _audioSource.timeSamples;
                var seekPosition  = positionSamples + offsetSamples;
                var maxSamples    = _audioSource!.clip!.samples - 1;

                if (seekPosition < 0)
                {
                    seekPosition += maxSamples;
                }

                if (seekPosition > maxSamples)
                {
                    seekPosition -= maxSamples;
                }

                seekPosition = Math.Clamp(seekPosition, 0, maxSamples);
                _audioSource.clip.GetData(sampleData, seekPosition);

                if (applySourceVolume)
                {
                    for (int i = 0; i < sampleData.Length; i++)
                    {
                        sampleData[i] *= _audioSource.volume;
                    }
                }
            }
            else
            {
                _audioSource.GetOutputData(sampleData, channel);
            }
        }
#endregion

        private MetaverseAudioSource()
        {
            // prevent default constructor (Use CreateNew)
        }

        private void Awake()
        {
            if (_doNotDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            AsyncJob();
        }

        private async void AsyncJob()
        {
            if (_bindOnLoad && !_playOnAwake)
            {
                _audioSource.clip = await SoundManager.Instance.GetClip(_audioFile);
            }

            if (_playOnAwake)
            {
                Play();
            }
        }

        private void OnEnable()
        {
            if (_audioSource != null)
            {
                _audioSource.enabled = true;
            }
        }

        private void OnDisable()
        {
            _audioSource.enabled = false;
        }

        private void OnDestroy()
        {
            if (!_audioSource.IsUnityNull())
                Destroy(_audioSource);
        }

        private async UniTask CrossFadeCoroutine(AudioSource origin, AudioSource target, C2VAsyncOperationHandle assetHandle, AssetReference targetReference, float fadeDuration, float fadeInDelay, eFadingMode mode, float targetVolume, bool unloadSoundClipAfterFadeOut, CancellationToken cancelToken)
        {
            float fadeOutRemTime = fadeDuration;
            float fadeInRemTime = fadeDuration + fadeInDelay;
            float originVolume = origin.volume;
            while ((!cancelToken.IsCancellationRequested && fadeInRemTime > 0) || (cancelToken.IsCancellationRequested && !Mathf.Approximately(fadeOutRemTime, 0)))
            {
                if (!SoundManager.InstanceExists)
                {
                    return;
                }

                float elapsedTime = Math.Min(Time.deltaTime, _maxFadeDeltaTime);
                fadeOutRemTime = Math.Max(0, fadeOutRemTime - elapsedTime);
                fadeInRemTime = Math.Max(0, fadeInRemTime - elapsedTime);
                float fadeInRatio = 1;
                if (fadeInRemTime < fadeDuration)
                {
                    fadeInRatio = SoundUtil.CalculateFadeRatio(mode, fadeInRemTime, fadeDuration);
                }
                float fadeOutRatio = SoundUtil.CalculateFadeRatio(mode, fadeOutRemTime, fadeDuration);
                origin.volume = originVolume * fadeOutRatio;
                if (!cancelToken.IsCancellationRequested)
                    target.volume = targetVolume * (1 - fadeInRatio);
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            }
            
            if (assetHandle != null && unloadSoundClipAfterFadeOut)
            {
                SoundManager.Instance.UnLoadSoundClip(assetHandle);
            }
            
            SoundManager.Instance.Stop(origin);
            if (!cancelToken.IsCancellationRequested)
            {
                _audioSource = target;
                _audioFile = targetReference;
                _lastFadeSource = null;
                _crossFadeCancelSource = null;
            }
            Destroy(origin);
        }

#region Public Method
        public static MetaverseAudioSource CreateNew(GameObject targetObject)
        {
            AudioSource newAudioSource = targetObject.AddComponent<AudioSource>();
            MetaverseAudioSource newMetaverseAudioSource = targetObject.AddComponent<MetaverseAudioSource>();
            newAudioSource.playOnAwake = false;
            newMetaverseAudioSource._audioSource = newAudioSource;
            newAudioSource.outputAudioMixerGroup = SoundManager.Instance.GetMixerGroup(0);
            newMetaverseAudioSource._bindOnLoad = false;

            return newMetaverseAudioSource;
        }

        public static MetaverseAudioSource CreateWithSource(GameObject targetObject, AudioSource source)
        {
            MetaverseAudioSource newMetaverseAudioSource = targetObject.AddComponent<MetaverseAudioSource>();
            newMetaverseAudioSource._audioSource = source;
            newMetaverseAudioSource._bindOnLoad = false;
            newMetaverseAudioSource.AudioClipType = eAudioClipType.NORMAL_SOURCE;

            return newMetaverseAudioSource;
        }

        public AudioClip GetClip()
        {
            return _audioSource.clip;
        }

        public void SetClip(AudioClip clip)
        {
            _audioSource.clip = clip;
            _audioClipType = eAudioClipType.NORMAL_CLIP;
        }

        public void Play(ulong delay = 0)
        {
            switch (_audioClipType)
            {
                case eAudioClipType.ASSET_REFERENCE:
                    _ = SoundManager.Instance.Play(_audioSource, _audioFile, delay);
                    break;
                case eAudioClipType.NORMAL_CLIP:
                case eAudioClipType.NORMAL_SOURCE:
                    SoundManager.Instance.Play(_audioSource, delay);
                    break;
            }
        }

        public void PlayOneShot(AssetReference reference, float volumeScale = 1)
        {
            SoundManager.Instance.PlayOneShot(_audioSource, reference, volumeScale);
        }

        public void PlayOneShot(string path, float volumeScale = 1)
        {
            SoundManager.Instance.PlayOneShot(_audioSource, path, volumeScale);
        }
        
        public void Stop()
        {
            if (SoundManager.InstanceExists)
                SoundManager.Instance.Stop(_audioSource);
        }

        public void Pause()
        {
            SoundManager.Instance.Pause(_audioSource);
        }

        public void UnPause()
        {
            SoundManager.Instance.UnPause(_audioSource);
        }

        public async UniTask CrossFadeTo(AssetReference reference, float fadeDuration, float fadeInDelay, eFadingMode mode, float targetVolume, bool unloadSoundClipAfterFadeOut)
        {
            if (_crossFadeCancelSource != null)
                _crossFadeCancelSource.Cancel();
            _crossFadeCancelSource = new CancellationTokenSource();
            
            var fadeSource = gameObject.AddComponent<AudioSource>();
            var targetSource = _lastFadeSource;
            if (_lastFadeSource.IsUnityNull())
                targetSource = _audioSource;

            _lastFadeSource = fadeSource;

            // copy settings from origin AudioSource
            fadeSource.loop = targetSource.loop;
            fadeSource.priority = targetSource.priority;
            fadeSource.volume = 0;
            fadeSource.pitch = targetSource.pitch;
            fadeSource.mute = targetSource.mute;
            fadeSource.spatialBlend = targetSource.spatialBlend;
            fadeSource.bypassEffects = targetSource.bypassEffects;
            fadeSource.bypassListenerEffects = targetSource.bypassListenerEffects;
            fadeSource.bypassReverbZones = targetSource.bypassReverbZones;
            fadeSource.rolloffMode = targetSource.rolloffMode;
            fadeSource.minDistance = targetSource.minDistance;
            fadeSource.maxDistance = targetSource.maxDistance;
            fadeSource.outputAudioMixerGroup = targetSource.outputAudioMixerGroup;

            var originClipHandle = _lastCrossFadeAssetHandle;
            if (reference != null)
            {
                if (unloadSoundClipAfterFadeOut)
                {
                    var clipAssetHandle = C2VAddressables.LoadAssetAsync<AudioClip>(reference);
                    if (clipAssetHandle != null)
                    {
                        var targetClip = await clipAssetHandle.ToUniTask();

                        SoundManager.Instance.Play(fadeSource, targetClip);
                        _lastCrossFadeAssetHandle = clipAssetHandle;
                    }
                }
                else
                {
                    _ = SoundManager.Instance.Play(fadeSource, reference);
                }
            }

            _crossFadeTask = CrossFadeCoroutine(targetSource, fadeSource, originClipHandle, reference, fadeDuration, fadeInDelay, mode, targetVolume, unloadSoundClipAfterFadeOut, _crossFadeCancelSource.Token);
            await _crossFadeTask;
        }

#endregion
    }
}
