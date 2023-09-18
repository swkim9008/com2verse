/*===============================================================
* Product:		Com2Verse
* File Name:	WebViewExtensions.cs
* Developer:	jhkim
* Date:			2022-11-21 16:41
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using Vuplex.WebView;

namespace Com2Verse.WebView
{
	public static class WebViewExtension
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void Initialize()
		{
			BaseWebViewPrefab.OnTerminateWebView += OnTerminateWebViewWebView;
		}

		private static void OnTerminateWebViewWebView()
		{
			// Prefab 외에 WebView를 사용하는 곳에서 이슈가 발생하여 주석처리
			// StandaloneWebView.TerminateBrowserProcess();
		}
	}
}
