/*===============================================================
* Product:		Com2Verse
* File Name:	WhiteboardWebView.cs
* Developer:	jhkim
* Date:			2023-04-28 09:32
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Extension;
using Com2Verse.UI;
using UnityEngine;

namespace Com2Verse.UI
{
	public static class WhiteBoardWebView
	{
		public static readonly string MiroHtmlEditFormat = "<body style='overflow:hidden'><iframe width='{0}' height='{1}' src='https://miro.com/app/live-embed/{2}?&autoplay=true' frameborder='0' scrolling='no' allow='fullscreen; clipboard-read; clipboard-write' allowfullscreen></iframe></body>";
		public static readonly string MiroHtmlViewFormat = "<body style='overflow:hidden'><iframe width='{0}' height='{1}' src='https://miro.com/app/live-embed/{2}?&autoplay=true&embedMode=view_only_without_ui' frameborder='0' scrolling='no' allow='fullscreen; clipboard-read; clipboard-write' allowfullscreen></iframe></body>";
		public static readonly Vector2Int DefaultSize = new Vector2Int(1300, 800);
		public static readonly Vector2Int DefaultOffset = new Vector2Int(15, 68);
		private static bool _isOpen = false;
		private static GUIView _view = null;

		public static void Show(string boardId, Vector2Int? sizeObj = null, Vector2Int? offsetObj = null, Action<GUIView> onLoadCompleted = null)
		{
			if (_isOpen) return;

			sizeObj ??= DefaultSize;
			offsetObj ??= DefaultOffset;

			var size = sizeObj.Value;
			var offset = offsetObj.Value;
			var html = string.Format(MiroHtmlEditFormat, Convert.ToString(size.x - offset.x), Convert.ToString(size.y - offset.y), boardId);

			UIManager.Instance.ShowPopupWebView(new UIManager.WebViewData
			{
				WebViewSize = size,
				Html = html,
				OnTerminated = OnTerminated,
			}, onLoadCompleted);
			_isOpen = true;
		}

		private static void OnTerminated()
		{
			_isOpen = false;
			_view?.Hide();
			_view = null;
		}

		public static bool Show()
		{
			if (_view.IsUnityNull()) return false;
			_view.gameObject.SetActive(true);
			_view.Show();
			return true;
		}
#if UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Reset() => _isOpen = false;
#endif // UNITY_EDITOR
	}
}