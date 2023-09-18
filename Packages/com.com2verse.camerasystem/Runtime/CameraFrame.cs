/*===============================================================
* Product:		Com2Verse
* File Name:	CameraFrame.cs
* Developer:	eugene9721
* Date:			2022-05-18 16:15
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.UIExtension;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Com2Verse.CameraSystem
{
	public sealed class CameraFrame : IUpdatableCamera, IDisposable
	{
		public Camera        SourceCamera  { get; }
		public RenderTexture TargetTexture { get; private set; }

		private CameraBase?  _currentCamera;
		private eCameraState _currentState;

		private readonly VariableResolutionRenderTexture      _component;
		private readonly Dictionary<eCameraState, CameraBase> _stateMap = new();

		public static CameraFrame Create(RenderTextureFormat format, int defaultWidth = 0, int defaultHeight = 0, float renderScale = 1f, GameObject? cameraPrefab = null)
		{
			var cameraFrame = new CameraFrame(format, defaultWidth, defaultHeight, renderScale, cameraPrefab);
			cameraFrame.Initialize();
			return cameraFrame;
		}

		private CameraFrame(RenderTextureFormat format, int defaultWidth = 0, int defaultHeight = 0, float renderScale = 1f, GameObject? cameraPrefab = null)
		{
			var cameraHolder = cameraPrefab.IsUnityNull() ? new GameObject() : Object.Instantiate(cameraPrefab)!;
			cameraHolder.SetActive(false);

			TargetTexture = RenderTextureHelper.CreateRenderTexture(format, defaultWidth, defaultHeight, renderScale);
			SourceCamera  = cameraHolder.GetOrAddComponent<Camera>()!;
			_component    = cameraHolder.GetOrAddComponent<VariableResolutionRenderTexture>()!;

			_component.RenderTextureResized += OnRenderTextureResized;
		}

		public void Initialize()
		{
			var fixedCamera   = new FixedCamera(SourceCamera);
			var forwardCamera = ForwardCamera.Create(SourceCamera);

			_stateMap.Add(eCameraState.FIXED_CAMERA,   fixedCamera);
			_stateMap.Add(eCameraState.FORWARD_CAMERA, forwardCamera);
		}

		private void OnRenderTextureResized(RenderTexture renderTexture)
		{
			TargetTexture = renderTexture;
		}

		public void Activate(RawImage? renderTarget = null)
		{
			if (!TargetTexture.IsCreated()) TargetTexture.Create();
			SourceCamera.gameObject.SetActive(true);
			CameraManager.Instance.UpdatableCameras?.Add(this);

			var go = SourceCamera.gameObject;
			go.SetActive(false);
			go.SetActive(true);

			if (!renderTarget.IsUnityNull())
			{
				_component.Initialize(TargetTexture, SourceCamera, renderTarget!);
			}
		}

		public void Deactivate()
		{
			if (!TargetTexture.IsUnityNull()) TargetTexture.Release();
			SourceCamera.gameObject.SetActive(false);
			CameraManager.InstanceOrNull?.UpdatableCameras.Remove(this);
		}

		public void Reset()
		{
			Deactivate();
			ChangeTarget(null);
			ChangeName();
		}

		public void Dispose()
		{
			_component.RenderTextureResized -= OnRenderTextureResized;
			Reset();
			_stateMap.DisposeAndClear();
			Object.Destroy(TargetTexture);
			SourceCamera.DestroyGameObject();
		}

		public void ChangeState(eCameraState nextState)
		{
			if (!_stateMap.ContainsKey(nextState))
			{
				C2VDebug.LogErrorCategory(nameof(CameraFrame), "This CameraState is null");
				return;
			}

			var currentTarget = _currentCamera?.TargetObject;
			_currentCamera?.OnStateExit();
			_currentState = nextState;
			_stateMap.TryGetValue(_currentState, out _currentCamera);
			_currentCamera?.OnStateEnter();

			ChangeTarget(currentTarget);
		}

		public void ChangeTarget(Transform? cameraTarget)
		{
			_currentCamera?.OnChangeTarget(cameraTarget);
		}

		public void ChangeName(string? name = "null")
		{
			var textureName = $"@{nameof(CameraFrame)}_{_currentState}_{name}";
			TargetTexture.name           = textureName;
			SourceCamera.gameObject.name = textureName;
		}

		public void OnUpdate()
		{
			_currentCamera?.OnUpdate();
		}

		public void OnLateUpdate()
		{
			_currentCamera?.OnLateUpdate();
		}
	}
}
