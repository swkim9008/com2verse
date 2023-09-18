/*===============================================================
* Product:		Com2Verse
* File Name:	LectureScreen.cs
* Developer:	ikyoung
* Date:			2023-05-25 16:06
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Threading;
using Com2Verse.Mice;
using Cysharp.Threading.Tasks;
using Protocols.Mice;
using Com2Verse.Logger;

namespace Com2Verse.Network
{
	public sealed class LectureScreenObject : MonoBehaviour
	{
        private MiceWebView _miceWebView;
		private CancellationTokenSource _cts;

		private void Awake()
		{
			_miceWebView = this.GetComponent<MiceWebView>();
			_cts = new CancellationTokenSource();
			MiceService.Instance.OnMiceScreenStateChanged += OnMiceScreenStateChanged;
		}

		private void OnDestroy()
		{
			C2VDebug.LogMethod(GetType().Name);
			MiceService.Instance.OnMiceScreenStateChanged -= OnMiceScreenStateChanged;
			_cts?.Cancel();
		}

		void OnMiceScreenStateChanged(MiceStreamingNoti streamingState)
		{
			C2VDebug.LogMethod(GetType().Name);
			SetScreenState(streamingState);
		}

		private void SetScreenState(MiceStreamingNoti streamingState)
		{
			Mice.NamedLoggerTag.Sprite.Log($"<LectureScreenObject.SetScreenState> State={streamingState.State}, SourceType={streamingState.SourceType}, Source='{streamingState.Source}'");

            if (streamingState.State == StreamingState.Play)
			{
				(MiceWebView.eScreenState state, string url) screen = streamingState.SourceType switch
				{
					2 or 3 or 5 or 6 => (MiceWebView.eScreenState.STREAMING, streamingState.Source),
					4 => (MiceWebView.eScreenState.VOD, streamingState.Source),
					7 => (MiceWebView.eScreenState.YOUTUBE, streamingState.Source),
					_ => (MiceWebView.eScreenState.STANDBY_IMAGE, streamingState.IdleImageUrl)
				};
				_miceWebView.SetScreenState(screen.state, screen.url).Forget();

            }
			else if (streamingState.State == StreamingState.Stop)
			{
                _miceWebView.SetScreenState(MiceWebView.eScreenState.STANDBY_IMAGE, streamingState.IdleImageUrl).Forget();
            }
		}

		private void Start()
		{
			C2VDebug.LogMethod(GetType().Name);
			SetScreenState(MiceService.Instance.StreamingState);
		}
	}
}
