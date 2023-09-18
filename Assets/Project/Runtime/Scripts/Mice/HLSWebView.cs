/*===============================================================
* Product:		Com2Verse
* File Name:	HLSWebView.cs
* Developer:	ikyoung
* Date:			2023-05-30 19:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Threading;
using Com2Verse.Extension;
using Com2Verse.Mice;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using Vuplex.WebView;
using Com2Verse.Logger;
using Com2Verse.Network;
using Protocols.Mice;

namespace Com2Verse.UI
{
	public sealed class HLSWebView : MonoBehaviour
		, IMapObjectPoolingEvent
	{
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private static void Log(string msg) => C2VDebug.LogCategory(nameof(HLSWebView), msg);

        public enum eScreenState
		{
			NONE,
			STREAMING,
			VOD,
			STANDBY_IMAGE
		}

		[SerializeField] private RectTransform _canvasWebViewRoot;
		[SerializeField] private GameObject _videoRenderer;
		[SerializeField] private SpriteRenderer _pictureRenderer;
		[SerializeField] private MetaverseButton _screenMouseEvent;
		[SerializeField] private MetaverseButton _screenZoomEvent;
		[SerializeField] private GameObject _screenZoomRoot;

		private Vector3 _pictureRendererSize;
		private CanvasWebViewWrapper _canvasWebViewWrapper;
		/// <summary>
		/// 강제 이동 중 인지 여부.
		/// </summary>
		private bool isTeleporting = false;

		private bool _screenZoomVisible
		{
			get => (_screenZoomRoot != null && _screenZoomRoot) ? _screenZoomRoot.activeSelf : false;
			set
			{
				if (_screenZoomRoot != null && _screenZoomRoot)
				{
					_screenZoomRoot.SetActive(value);
				}
			}
		}

		public eScreenState CurrentEScreenState { get; set; }

        public void Awake()
		{
			WebHelper.TrySetAutoplayEnabled(true);

            this.isTeleporting = false;

            _pictureRendererSize = _pictureRenderer.transform.localScale;

			if (!_pictureRenderer.IsUnityNull()) _pictureRenderer.enabled = false;

			this.CreateCanvasWebViewWrapperIfNotExists();

			_screenMouseEvent.OnHighlightedEvent += this.OnScreenHighlighted;
			_screenMouseEvent.OnNormalEvent += this.OnScreenNormal;

			_screenZoomEvent.onClick.AddListener(this.OnScreenZoom);

			_screenZoomVisible = false;

			_videoRenderer.SetActive(false);
			_pictureRenderer.enabled = false;

			this.SubscribeMapObjectPoolingEvent();

			PacketReceiver.Instance.OnMiceRoomNotifyEvent += this.OnMiceRoomNotify;
        }

        private void OnMiceRoomNotify(MiceRoomNotify response)
		{
			if (response.MiceType == MiceType.ConferenceSession && response.NotiEvent == NotifyEvent.Close && !this.isTeleporting)
			{
				this.CloseZoomUI();
				this.isTeleporting = true;
			}                
        }

        private void OnDestroy()
		{
            HLSWebView.Log("[OnDestroy]");

            this.Stop();
            this.DestroyHLS();
			this.DestroyCanvasWebViewWrapper();

            this.UnsubscribeMapObjectPoolingEvent();

            PacketReceiver.Instance.OnMiceRoomNotifyEvent -= this.OnMiceRoomNotify;
        }

		void IMapObjectPoolingEvent.OnAllocMapObject()
		{
			HLSWebView.Log("[OnAllocMapObject]");

            this.CreateCanvasWebViewWrapperIfNotExists();

			// 새로 시작했는데 플레이 중 일 경우 웹뷰의 비디오 플레이를 호출 해 준다(세션 전환 시점)
			if (MiceService.Instance.StreamingState != null && MiceService.Instance.StreamingState.State == StreamingState.Play)
			{
				this._canvasWebViewWrapper.PlayVideo().Forget();
            }

			this.isTeleporting = false;
        }

		void IMapObjectPoolingEvent.OnFreeMapObject()
		{
            HLSWebView.Log("[OnFreeMapObject]");

			this.Stop();
        }

		private void CreateCanvasWebViewWrapperIfNotExists()
		{
			if (_canvasWebViewWrapper != null && _canvasWebViewWrapper.IsWebViewVaild) return;

            _canvasWebViewWrapper = new CanvasWebViewWrapper();
            _canvasWebViewWrapper.Init(_canvasWebViewRoot, _videoRenderer.transform).Forget();

            HLSWebView.Log("CanvasWebViewWrapper created!");
        }
		
		private void DestroyCanvasWebViewWrapper()
		{
			if (_canvasWebViewWrapper == null) return;

            _canvasWebViewWrapper.Dispose();

            HLSWebView.Log("CanvasWebViewWrapper destroyed!");
        }
		
		private void DestroyHLS()
		{
            if (_canvasWebViewWrapper != null && _canvasWebViewWrapper.IsWebViewVaild)
            {
                _canvasWebViewWrapper.HLSDestroy().Forget();

                HLSWebView.Log("HLS destroyed!");
            }
        }

		/// <summary>
        /// 스트리밍 또는 VOD 플레이 중일 경우 중단한다.
        /// </summary>
        public void Stop()
		{
            if (_canvasWebViewWrapper != null && _canvasWebViewWrapper.IsWebViewVaild)
			{
				_canvasWebViewWrapper.StopVideo().Forget();

                HLSWebView.Log("Stopped!");
            }
        }

        /// <summary>
        /// 영상 확대 출력 UI가 표시 중 일 경우 닫는다.
        /// </summary>
        public void CloseZoomUI()
		{
            MiceUIConferenceScreenZoomViewModel.HideView().Forget();
        }

        public async UniTask SetScreenState(eScreenState state, string screenContentUrl, CancellationToken cancellationToken = default)
		{
			_videoRenderer.gameObject.SetActive(false);
			_pictureRenderer.enabled = false;

			try
			{
                HLSWebView.Log($"[SetScreenState]({state}) URL='{screenContentUrl}'");

				if (state == eScreenState.STREAMING || state == eScreenState.VOD)
				{
					this.CreateCanvasWebViewWrapperIfNotExists();

                    var mediaType = MiceService.MEDIA_TYPE_CUSTOM_HLS;

					if (state == eScreenState.VOD)
					{
						var strs = screenContentUrl.Split('.');
						var type = strs[strs.Length - 1].ToLower();

						mediaType = type switch
						{
							"ogg" => MiceService.MEDIA_TYPE_VIDEO_OGG,
							"mp4" => MiceService.MEDIA_TYPE_VIDEO_MP4,
							"webm" => MiceService.MEDIA_TYPE_VIDEO_WEBM,
							_ => MiceService.MEDIA_TYPE_VIDEO_MP4
						};
					}

                    HLSWebView.Log($"[SetScreenState]({state}) MediaType={mediaType}");

                    await _canvasWebViewWrapper.LoadUrl("about:blank").WithCancellationToken(cancellationToken);

                    var url = MiceService.GetHLSVideoWebPage(screenContentUrl, mediaType, true, true);

                    await _canvasWebViewWrapper.LoadUrl(url).WithCancellationToken(cancellationToken);

					// VOD 일 경우에만 Timeline을 표시한다.
					await _canvasWebViewWrapper.SetVideoTimelineVisible(state == eScreenState.VOD);

                    _videoRenderer.gameObject.SetActive(true);
				}
				else if (state == eScreenState.STANDBY_IMAGE)
				{
                    if (!string.IsNullOrEmpty(screenContentUrl))
					{
						var result = await TextureCache.Instance.GetOrDownloadTextureAsync(screenContentUrl).WithCancellationToken(cancellationToken);

						_pictureRenderer.enabled = true;
						_pictureRenderer.sprite = Sprite.Create(result, new Rect(0, 0, result.width, result.height), new Vector2(0.5f, 0.5f), 1);
						_pictureRenderer.flipY = false;
						_pictureRenderer.transform
										.localScale = new
										(
											_pictureRendererSize.x / result.width,
											_pictureRendererSize.y / result.height
										);

						_pictureRenderer.enabled = true;
					}
				}
			}
			finally
			{
				CurrentEScreenState = state;
			}
		}

		public void OnScreenZoom()
		{
			if (this.isTeleporting) return;

			MiceUIConferenceScreenZoomViewModel
				.ShowView
				(
					transform => _canvasWebViewWrapper.Output(transform),
					() => _canvasWebViewWrapper.Output(_videoRenderer.transform)
				);
		}

		private void OnScreenHighlighted()
		{
			_screenZoomVisible = !this.isTeleporting &&
								 (
								  	this.CurrentEScreenState != eScreenState.NONE &&
								  	this.CurrentEScreenState != eScreenState.STANDBY_IMAGE
								 );

		}

		private void OnScreenNormal()
		{
			_screenZoomVisible = false;
		}
	}
}

namespace Com2Verse.Network
{ 
	public interface IMapObjectPoolingEvent
	{
		public void Subscribe()
		{
            MapController.Instance.OnMapObjectCreate += this.OnMapObjectCreate;
            MapController.Instance.OnMapObjectRemove += this.OnMapObjectRemove;
        }

		public void Unsubscribe()
		{
            MapController.Instance.OnMapObjectCreate -= this.OnMapObjectCreate;
            MapController.Instance.OnMapObjectRemove -= this.OnMapObjectRemove;
        }

		private GameObject gameObject => (this is MonoBehaviour mono) ? mono.gameObject : null;

        private void OnMapObjectCreate(Protocols.ObjectState state, BaseMapObject obj)
		{
            if (obj.gameObject == this.gameObject)
            {
                this.OnAllocMapObject();
            }
        }

        private void OnMapObjectRemove(BaseMapObject obj)
		{
            if (obj.gameObject == this.gameObject)
            {
                this.OnFreeMapObject();
            }
        }

        /// <summary>
        /// BaseMapObject가 Pool에서 나갈 때(할당) 호출됨.
        /// </summary>
        void OnAllocMapObject();

        /// <summary>
        /// BaseMapObject가 Pool에 들어갈 때(해제) 호출됨.
        /// </summary>
        void OnFreeMapObject();
    }

	public static partial class MapObjectPoolingEventExtensions
	{
		/// <summary>
		/// MapObject Pooling 이벤트를 구독한다.
		/// </summary>
		/// <param name="value"></param>
		public static void SubscribeMapObjectPoolingEvent(this IMapObjectPoolingEvent value) => value.Subscribe();
        /// <summary>
        /// MapObject Pooling 이벤트를 구독 해제한다.
        /// </summary>
        /// <param name="value"></param>
        public static void UnsubscribeMapObjectPoolingEvent(this IMapObjectPoolingEvent value) => value.Unsubscribe();
    }
}

