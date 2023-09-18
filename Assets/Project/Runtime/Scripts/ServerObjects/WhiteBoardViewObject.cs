/*===============================================================
* Product:		Com2Verse
* File Name:	WhiteBoardViewObject.cs
* Developer:	jhkim
* Date:			2023-07-18 20:49
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Extension;
using Com2Verse.Interaction;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Vuplex.WebView;

namespace Com2Verse.Network
{
	public sealed class WhiteBoardViewObject : MonoBehaviour, IDisposable
	{
#region Variables
		private static readonly int MinWhiteBoardIdLength = 5;

		private bool _isInit;
		private bool _isWebViewLoaded;
		[SerializeField] private Renderer _renderer;
		private Material _prevMaterial;
		private IWebView _webView;

		private static readonly int DelayFrame = 30;
		private static readonly int RetryCount = 5;
		private bool _isLoadRequested = false;

#endregion // Variables
		/* 초기화 과정
		 * 1. BaseMapObject 컴포넌트 찾기 (최초 로드시 컴포넌트가 늦게 붙어 반복체크)
		 * 2. InteractionValueChanged 이벤트 등록 (오브젝트가 풀링되기 때문에 해제 후 등록)
		 * 2-1. 이벤트 등록 시점에 로드 시도 (이벤트 등록 전에 InteractionValue가 Set 되는 상황이 있음)
		 */

		private void OnEnable() => InitializeAsync().Forget();
		private void OnDisable() => Dispose();
		private async UniTaskVoid InitializeAsync()
		{
			if (_isInit) return;
			if (!await TryRegisterEventAsync()) return;

			_isInit = true;
		}

#region Interaction Event
		private async UniTask<bool> TryRegisterEventAsync()
		{
			var retry = 0;
			while (retry < RetryCount)
			{
				var mapObject = transform.GetComponentInChildren<BaseMapObject>();
				if (!mapObject.IsReferenceNull())
				{
					mapObject.InteractionValueChanged -= OnInteractionValueChanged;
					mapObject.InteractionValueChanged += OnInteractionValueChanged;
					if (mapObject.InteractionValues.Count > 0)
						CheckAndLoadAsync();
					return true;
				}
				await UniTask.DelayFrame(DelayFrame);
				retry++;
			}
			return false;
		}

		private void UnregisterEvent()
		{
			var mapObject = transform.GetComponentInChildren<BaseMapObject>();
			if (mapObject.IsReferenceNull()) return;
			mapObject.InteractionValueChanged -= OnInteractionValueChanged;
			mapObject.InteractionValues.Clear();
		}

		private void OnInteractionValueChanged(BaseMapObject mapObject) => CheckAndLoadAsync().Forget();
#endregion // Interaction Event

#region Load
		private async UniTaskVoid CheckAndLoadAsync()
		{
			if (_isLoadRequested) return;

			_isLoadRequested = true;
			var mapObject = transform.GetComponentInChildren<BaseMapObject>();
			if (!mapObject.IsReferenceNull() && !_isWebViewLoaded)
			{
				TryGetBoardId(mapObject, out var boardId);
				await LoadAsync(boardId);
			}

			_isLoadRequested = false;
		}

		private bool TryGetBoardId(BaseMapObject mapObject, out string boardId)
		{
			boardId = string.Empty;
			if (mapObject == null) return false;

			boardId = InteractionManager.Instance.GetInteractionValue(mapObject.InteractionValues, 0, 0, 0);
			boardId = RemoveUrl(boardId);

			return IsValidBoardId(boardId);

			string RemoveUrl(string boardId)
			{
				if (string.IsNullOrWhiteSpace(boardId)) return boardId;

				var idx = boardId.LastIndexOf("/");

				return idx == -1 ? boardId : boardId.Substring(idx);
			}

			bool IsValidBoardId(string boardId)
			{
				if (string.IsNullOrWhiteSpace(boardId)) return false;

				return boardId.Length >= MinWhiteBoardIdLength;
			}
		}

		private async UniTask<bool> LoadAsync(string boardId, Vector2Int? sizeObj = null)
		{
			if (_isWebViewLoaded)
				Dispose();

			if (string.IsNullOrWhiteSpace(boardId)) return false;

			_isWebViewLoaded = true;

			var offSet = new Vector2Int(15, 0);
			sizeObj ??= WhiteBoardWebView.DefaultSize;

			var size = sizeObj.Value;
			var htmlStr = string.Format(WhiteBoardWebView.MiroHtmlViewFormat, Convert.ToString(size.x - offSet.x), Convert.ToString(size.y - offSet.y), boardId);

			_webView = Web.CreateWebView();
			await _webView.Init(size.x, size.y);
			_webView.LoadHtml(htmlStr);
			await _webView.WaitForNextPageLoadToFinish();
			var material = _webView.CreateMaterial();

			SetMaterial(material);
			return true;
		}
#endregion // Load

		private void SetMaterial(Material material)
		{
			if (_renderer == null) return;

			_prevMaterial = _renderer.material;
			_renderer.material = material;
		}

#region Dispose
		public void Dispose()
		{
			if (!_renderer.IsUnityNull() && _prevMaterial != null)
				_renderer.material = _prevMaterial;

			_prevMaterial = null;
			_webView?.Dispose();
			_webView = null;
			_isWebViewLoaded = false;
			_isLoadRequested = false;
			_isInit = false;
			UnregisterEvent();
		}
#endregion // Dispose
	}
}
