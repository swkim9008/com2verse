/*===============================================================
* Product:		Com2Verse
* File Name:	ObjectWebView.cs
* Developer:	mikeyid77
* Date:			2022-08-05 09:46
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using Vuplex.WebView;

namespace Com2Verse.WebView
{
	public sealed class ObjectWebViewController : MonoBehaviour
	{
		public enum eWebViewType
		{
			NONE,
			VIDEO_AD,
			PDF_VIEWER,
			WEB_BROWSER
		}

		public eWebViewType _webViewType;
		public string _url;
		public float _resolution = 200f;
		public Transform _webViewRoot;
		public SpriteRenderer _spriteMask;
		
		private WebViewPrefab _webView;
		private Transform _charaPos;
		private float _distance = 4f;
		private bool _showWebView = false;
		private float x, y;
		
		
		
		private void Start()
		{
			_charaPos = GameObject.Find("[Test Chara]").GetComponent<Transform>();
			x = _spriteMask.sprite.rect.width / 100f;
			y = _spriteMask.sprite.rect.height / 100f;
		}
		
		private void Update()
		{
			var charaPos = new Vector3(_charaPos.position.x, 0, _charaPos.position.z);
			var targetPos = new Vector3(transform.position.x, 0, transform.position.z);
			
			if (!_showWebView && Vector3.Distance(charaPos, targetPos) < _distance)
			{
				_showWebView = true;
				InitializeView();
			}
			else if (_showWebView && Vector3.Distance(charaPos, targetPos) > _distance)
			{
				_showWebView = false;
				DisposeView();
			}
		}

		private async void InitializeView()
		{
			_webView = WebViewPrefab.Instantiate(x, y);
			_webView.transform.SetParent(_webViewRoot, false);
			_webView.transform.localPosition = new Vector3(0, y / 2, 0);
			_webView.Resolution = _resolution;

			await _webView.WaitUntilInitialized();
			_webView.WebView.LoadUrl(_url);

			if (_webViewType == eWebViewType.PDF_VIEWER)
			{
				_spriteMask.gameObject.SetActive(false);
			}
			else
			{
				_webView.WebView.TitleChanged += (obj, args) =>
				{
					if (_webViewType == eWebViewType.VIDEO_AD)
					{
						_webView.ClickingEnabled = false;
						_webView.WebView.Click(new Vector2(0f, 0f));
					}
					_spriteMask.gameObject.SetActive(false);
				};
			}
		}

		private void DisposeView()
		{
			_spriteMask.gameObject.SetActive(true);
			_webView.Destroy();
		}
	}
}
