using System;
using System.IO;
using System.Linq;
using Com2Verse.Logger;
using Com2Verse.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[Obsolete("this script is now obsolete. please use C2VCaptureTools instead")]
public class TransparentCapture
{
	private static readonly string _directoryName = "TransparentCaptures";
	private static readonly string _basicFileName = "TransparentCapture";
	private static readonly string _uiLayerName = "UI";

	private static string CapturePath => DirectoryUtil.DataRoot.Replace("Assets", _directoryName);

	[UnityEditor.MenuItem("Com2Verse/Transparent Capture (Obsolete) &c", false)]
	static void OnTransparentCapture()
	{
		var captureCamera = GetCaptureCamera();

		if (captureCamera == null)
		{
			C2VDebug.LogError("not found activated camera.");
			return;
		}

		InactiveAllCameras(captureCamera);

		var captureInfo = Capture(captureCamera);

		Save(captureInfo);
	}

	private static bool CreateDirectory()
	{
		if (!Directory.Exists(CapturePath))
		{
			Directory.CreateDirectory(CapturePath);
			return false;
		}

		return true;
	}

	private static string GetLayerName()
	{
		var activeObject = Selection.activeGameObject;

		var layerName = "Default";
		if (activeObject != null)
			layerName = LayerMask.LayerToName(activeObject.layer);

		return layerName;
	}

	/// <summary>
	/// 현재 카메라 중 "CaptureCamera" 라는 이름을 가진 카메라를 검색해서 리턴
	/// "CaptureCamera"가 없다면 Active 상태로 되어 있는 첫 번째 카메라 리턴
	/// </summary>
	/// <returns></returns>
	private static Camera GetCaptureCamera()
	{
		var mainCameraName = "CaptureCamera";
		var allCameras = Camera.allCameras;

		if (allCameras != null && allCameras.Length > 0)
		{
			var captureCamera = Camera.allCameras.FirstOrDefault((camera) => camera.gameObject.name.ToLower().Equals(mainCameraName.ToLower()));

			if (captureCamera == null)
				captureCamera = Camera.allCameras.FirstOrDefault((camera) => camera.gameObject.activeInHierarchy);

			return captureCamera;
		}

		return null;
	}

	private static void InactiveAllCameras(Camera activeCamera)
	{
		foreach (var camera in Camera.allCameras)
		{
			if (!ReferenceEquals(activeCamera, null) && !camera.Equals(activeCamera))
				camera.gameObject.SetActive(false);
		}
	}

	private static (byte[] image, string extension, string layerName, int width, int height) Capture(Camera captureCamera)
	{
		var layerName = GetLayerName();
		string extension = layerName != _uiLayerName ? ".png" : ".jpg";

		var width = captureCamera.pixelWidth;
		var height = captureCamera.pixelHeight;

		var renderTexture = RenderTexture.GetTemporary(width, height, 32, RenderTextureFormat.ARGB32);
		renderTexture.antiAliasing = 8;

		// disabled post processing
		var urpCameraData = captureCamera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
		if (urpCameraData != null)
			urpCameraData.renderPostProcessing = false;

		var beforeClearFlags = captureCamera.clearFlags;
		captureCamera.clearFlags = CameraClearFlags.Depth;
		captureCamera.targetTexture = renderTexture;
		captureCamera.Render();
		captureCamera.targetTexture = null;
		captureCamera.clearFlags = beforeClearFlags;

		RenderTexture.active = renderTexture;
		var copyTexture = new Texture2D(width, height, TextureFormat.ARGB32, false) { alphaIsTransparency = true };
		copyTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
		copyTexture.Apply();
		RenderTexture.active = null;

		var bytes = layerName == _uiLayerName ? copyTexture.EncodeToJPG(100) : copyTexture.EncodeToPNG();

		RenderTexture.ReleaseTemporary(renderTexture);
		Object.DestroyImmediate(copyTexture);

		return (bytes, extension, layerName, width, height);
	}

	private static string GetFileName(GameObject activeGameObject, (string extension, string layerName) captureInfo)
	{
		var fileName = string.Empty;

		if (activeGameObject != null)
			fileName = Path.Combine(_directoryName, activeGameObject.name + captureInfo.extension);

		if (string.IsNullOrEmpty(fileName))
			fileName = Path.Combine(_directoryName, _basicFileName + captureInfo.extension);

		return fileName;
	}

	private static void Save((byte[] image, string extension, string layerName, int width, int height) captureInfo)
	{
		try
		{
			CreateDirectory();

			var fileName = GetFileName(Selection.activeGameObject, (captureInfo.extension, captureInfo.layerName));
			File.WriteAllBytes(fileName, captureInfo.image);

			var result = string.Format("Transparent Capture [{0}\\{1}] || image size [{2},{3}]", Directory.GetCurrentDirectory(), fileName, captureInfo.width, captureInfo.height);

			System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
			if (T != null)
			{
				EditorWindow gameView = EditorWindow.GetWindow(T);
				gameView?.ShowNotification(new GUIContent(result), 3f);
			}

			C2VDebug.Log(result);
		}
		catch (Exception e)
		{
			C2VDebug.Log(e.ToString());
		}
	}
}
