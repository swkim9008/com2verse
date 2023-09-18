/*===============================================================
* Product:		Com2Verse
* File Name:	MiceUICapture.cs
* Developer:	wlemon
* Date:			2023-04-10 11:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using Com2Verse.Extension;
using Com2Verse.UIExtension;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Com2Verse.Mice
{
	public sealed class MiceUICapture : MonoBehaviour
	{
		public static readonly string ResName = "MiceUICapture.prefab";

		public enum eLayoutType
		{
			DEFAULT,
			FILL,
			EXPAND
		}
		
		[SerializeField] private Camera _camera;
		[SerializeField] private Canvas _canvas;
		[SerializeField] private RawImage _rawImage;

		private Texture2D CaptureInternal(GameObject ui, string captureNodeName)
		{
			var target = ui.transform;
			if (!string.IsNullOrEmpty(captureNodeName))
			{
				target = ui.transform.FindRecursive(captureNodeName);
			}

			if (target == null) return null;

			var targetRectTransform = target.GetComponent<RectTransform>();
			var targetWidth = targetRectTransform.rect.width;
			var targetHeight = targetRectTransform.rect.height;
			var targetParent = target.transform.parent;

			var targetAnchoredPosition = targetRectTransform.anchoredPosition;
			var targetAnchorMin        = targetRectTransform.anchorMin;
			var targetAnchorMax        = targetRectTransform.anchorMax;
			var targetPivot            = targetRectTransform.pivot;
			var targetLocalScale       = targetRectTransform.localScale;

			targetRectTransform.SetParent(_canvas.transform, false);
			targetRectTransform.anchoredPosition = Vector2.zero;
			targetRectTransform.anchorMin        = new Vector2(0.5f, 0.5f);
			targetRectTransform.anchorMax        = new Vector2(0.5f, 0.5f);
			targetRectTransform.pivot            = new Vector2(0.5f, 0.5f);
			targetRectTransform.localScale       = Vector3.one;
			_rawImage.gameObject.SetActive(false);

			var generatedTexture = CaptureInternal((int)targetWidth, (int)targetHeight);
			target.transform.SetParent(targetParent, false);
			targetRectTransform.anchoredPosition = targetAnchoredPosition;
			targetRectTransform.anchorMin        = targetAnchorMin;
			targetRectTransform.anchorMax        = targetAnchorMax;
			targetRectTransform.pivot            = targetPivot;
			targetRectTransform.localScale       = targetLocalScale;
			return generatedTexture;
		}

		private Texture2D CaptureInternal(Texture2D texture, int width, int height, eLayoutType layoutType)
		{
			_rawImage.gameObject.SetActive(true);
			_rawImage.texture = texture;
			
			var textureWidth = texture.width;
			var textureHeight = texture.height;
			var imageSize = new Vector2((float)textureWidth, (float)textureHeight);
			switch (layoutType)
			{
				case eLayoutType.DEFAULT:
					if (textureWidth > textureHeight)
						imageSize = new Vector2(width, width * (float)textureHeight / (float)textureWidth);
					else
						imageSize = new Vector2(height * (float)textureWidth / (float)textureHeight, height);
					break;
				case eLayoutType.FILL:
					imageSize = new Vector2(width, height);
					break;
				case eLayoutType.EXPAND:
					if (textureWidth > textureHeight)
						imageSize = new Vector2(height * (float)textureWidth / (float)textureHeight, height);
					else
						imageSize = new Vector2(width, width * (float)textureHeight / (float)textureWidth);
					break;
			}
			_rawImage.rectTransform.sizeDelta = imageSize;
			return CaptureInternal(width, height);
		}

		private Texture2D CaptureInternal(int width, int height)
		{
			var renderTexture = RenderTextureHelper.CreateRenderTexture(RenderTextureFormat.Default, width, (int)height);
			_camera.targetTexture = renderTexture;
			_camera.Render();

			var activeRenderTexture = RenderTexture.active;
			RenderTexture.active = renderTexture;
			var generatedTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
			generatedTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			generatedTexture.Apply();
			RenderTexture.active = activeRenderTexture;
			return generatedTexture;
		}
		
		
		public static async UniTask<Texture2D> Capture(GameObject ui, string captureNodeName = default(string))
		{
			var uiCaptureHandle = Addressables.InstantiateAsync(ResName);
			await uiCaptureHandle;
		
			var uiCapture = uiCaptureHandle.Result.GetComponent<MiceUICapture>();
			var texture = uiCapture.CaptureInternal(ui, captureNodeName);

			//Destroy(uiCapture.gameObject);
			return texture;
		}

		public static async UniTask<Texture2D> Capture(Texture2D texture, int width, int height, eLayoutType layoutType)
		{
			var uiCaptureHandle = Addressables.InstantiateAsync(ResName);
			await uiCaptureHandle;

			var uiCapture = uiCaptureHandle.Result.GetComponent<MiceUICapture>();
			var generatedTexture = uiCapture.CaptureInternal(texture, width, height, layoutType);

			Destroy(uiCapture.gameObject);
			return generatedTexture;
		}
	}
}

