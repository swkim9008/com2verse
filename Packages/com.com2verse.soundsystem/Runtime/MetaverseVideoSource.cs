/*===============================================================
* Product:    Com2Verse
* File Name:  MetaverseVideoSource.cs
* Developer:  yangsehoon
* Date:       2022-04-11 09:40
* History:    
* Documents:  Unity VideoPlayer wrapper
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using Com2Verse.AssetSystem;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Video;

namespace Com2Verse.Sound
{
    [System.Serializable]
    public struct RenderTextureSize
    {
        public int width;
        public int height;
        public int depth;
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("Video/Metaverse Video Source")]
    public class MetaverseVideoSource : MonoBehaviour
    {
        [SerializeField][HideInInspector] private AudioSource _audioSource;
        [SerializeField][HideInInspector] private VideoPlayer _videoSource;

#if UNITY_EDITOR
        /// <summary>
        /// 에디터 참조 전용 프로퍼티
        /// </summary>
        public AudioSource AudioSource
        {
            get => _audioSource;
            set => _audioSource = value;
        }

        /// <summary>
        /// 에디터 참조 전용 프로퍼티
        /// </summary>
        public VideoPlayer VideoPlayer
        {
            get => _videoSource;
            set => _videoSource = value;
        }
#endif

        [SerializeField] private AssetReference _mediaFile;
        [SerializeField] private int _targetMixerGroup;
        [SerializeField][Tooltip("Load AssetReference on Awake?")] private bool _bindOnLoad = true;
        [SerializeField] private bool _createRenderTextureAtRuntime = false;
        [SerializeField] private bool _playOnAwake;
        
        [SerializeField] private RenderTextureSize _RenderTextureSize;
        [SerializeField] private RenderTextureFormat _renderTextureFormat;
        public RenderTexture TargetTexture
        {
            get
            {
                if (_createRenderTextureAtRuntime && _videoSource.targetTexture == null)
                {
                    _videoSource.targetTexture = UIExtension.RenderTextureHelper.CreateRenderTexture(_renderTextureFormat, _RenderTextureSize.width, _RenderTextureSize.height, _RenderTextureSize.depth);
                    if (!_videoSource.targetTexture.Create())
                    {
                        C2VDebug.LogError("Render texture generation failed");
                    }
                }

                return _videoSource.targetTexture;
            }
        }

#region Wrapped Properties
        public float SpatialBlend
        {
            get => _audioSource.spatialBlend;
            set => _audioSource.spatialBlend = value;
        }
        public int TargetMixerGroup
        {
            get => _targetMixerGroup;
            set { _audioSource.outputAudioMixerGroup = SoundManager.Instance.GetMixerGroup(value); _targetMixerGroup = value; }
        }
        public VideoSource Source
        {
            get => _videoSource.source;
            set => _videoSource.source = value;
        }
        public string Url
        {
            get => _videoSource.url;
            set => _videoSource.url = value;
        }
        public bool Loop
        {
            get => _videoSource.isLooping;
            set => _videoSource.isLooping = value;
        }
        public bool SkipOnDrop
        {
            get => _videoSource.skipOnDrop;
            set => _videoSource.skipOnDrop = value;
        }
        public float Volume
        {
            get => _audioSource.volume;
            set => _audioSource.volume = value;
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
        public float PlaybackSpeed
        {
            get => _videoSource.playbackSpeed;
            set => _videoSource.playbackSpeed = value;
        }
        public VideoRenderMode RenderMode
        {
            get => _videoSource.renderMode;
            set => _videoSource.renderMode = value;
        }
        public Camera RenderCamera
        {
            get => _videoSource.targetCamera;
            set => _videoSource.targetCamera = value;
        }
        public float CameraAlpha
        {
            get => _videoSource.targetCameraAlpha;
            set => _videoSource.targetCameraAlpha = value;
        }
        public Video3DLayout Camera3DLayout
        {
            get => _videoSource.targetCamera3DLayout;
            set => _videoSource.targetCamera3DLayout = value;
        }
        public VideoAspectRatio AspectRatio
        {
            get => _videoSource.aspectRatio;
            set => _videoSource.aspectRatio = value;
        }

        public bool IsPrepared
        {
            get => _videoSource.isPrepared;
        }

        public bool IsPlaying
        {
            get => _videoSource.isPlaying;
        }
#endregion

        private MetaverseVideoSource()
        {
            // prevent default constructor (Use CreateNew)
        }
        private void Start()
        {
            AsyncJob();
        }

        private void OnDestroy()
        {
            if (_createRenderTextureAtRuntime)
            {
                if (_videoSource.targetTexture != null)
                {
                    _videoSource.targetTexture.Release();
                    _videoSource.targetTexture = null;
                }
            }
            Destroy(_videoSource);
            Destroy(_audioSource);
        }

        private void OnEnable()
        {
            Initialize();
            _audioSource.enabled = true;
            _videoSource.enabled = true;
        }

        private void OnDisable()
        {
            _audioSource.enabled = false;
            _videoSource.enabled = false;
        }

        private void Initialize()
        {
            if (_videoSource.IsReferenceNull())
            {
                _videoSource = Util.GetOrAddComponent<VideoPlayer>(gameObject);
            }

            if (_audioSource.IsReferenceNull())
            {
                _audioSource = Util.GetOrAddComponent<AudioSource>(gameObject);
            }
            
            _videoSource!.audioOutputMode = VideoAudioOutputMode.AudioSource;
            _videoSource.SetTargetAudioSource(0, _audioSource);
        }

        private async void AsyncJob()
        {
            if (Source == VideoSource.Url)
            {
                if (_playOnAwake)
                {
                    Play();
                }
            }
            else if (Source == VideoSource.VideoClip)
            {
                if (_bindOnLoad && !_playOnAwake)
                {
                    _videoSource.clip = await C2VAddressables.LoadAssetAsync<VideoClip>(_mediaFile).ToUniTask();
                }

                if (_playOnAwake)
                {
                    Play();
                }
            }
        }
        
#region Public Method
        public static MetaverseVideoSource CreateNew(GameObject targetObject)
        {
            MetaverseVideoSource newMetaverseVideoSourceSource = Util.GetOrAddComponent<MetaverseVideoSource>(targetObject);
            newMetaverseVideoSourceSource._audioSource     = Util.GetOrAddComponent<AudioSource>(targetObject);
            newMetaverseVideoSourceSource._videoSource     = Util.GetOrAddComponent<VideoPlayer>(targetObject);
            newMetaverseVideoSourceSource._bindOnLoad      = false;
            newMetaverseVideoSourceSource.TargetMixerGroup = SoundManager.Instance.VideoGroupIndex;

            return newMetaverseVideoSourceSource;
        }

        public void Prepare()
        {
            _videoSource.Prepare();
        }
        
        public void Play(ulong delay = 0)
        {
            _videoSource.Play();
        }

        public void Pause()
        {
            _videoSource.Pause();
        }
#endregion
    }
}
