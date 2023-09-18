/*===============================================================
* Product:		Com2Verse
* File Name:	CameraManager_Event.cs
* Developer:	eugene9721
* Date:			2023-02-07 12:05
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Extension;
using UnityEngine;

namespace Com2Verse.CameraSystem
{
	public partial class CameraManager
	{
		private Action<CameraBase?, CameraBase>? _onCameraStateChange;

		/// <summary>
		/// 카메라의 상태가 변경됨을 알리는 이벤트
		/// 첫번째 CameraBase : Exit되는 카메라의 상태
		/// 두번째 CameraBase : Enter되는 카메라의 상태
		/// </summary>
		public event Action<CameraBase?, CameraBase> OnCameraStateChange
		{
			add
			{
				_onCameraStateChange -= value;
				_onCameraStateChange += value;
			}
			remove => _onCameraStateChange -= value;
		}

		private Action<int>? _onUpdateCameraView;

		public event Action<int> OnUpdateCameraView
		{
			add
			{
				_onUpdateCameraView -= value;
				_onUpdateCameraView += value;
			}
			remove => _onUpdateCameraView -= value;
		}

		private Action<int>? _onCameraRendererChange;

		public event Action<int> OnCameraRendererChange
		{
			add
			{
				_onCameraRendererChange -= value;
				_onCameraRendererChange += value;
			}
			remove => _onCameraRendererChange -= value;
		}

		private float          _prevAspectRatio;
		private Action<float>? _onChangeAspectRatio;

		public event Action<float> OnChangeAspectRatio
		{
			add
			{
				_onChangeAspectRatio -= value;
				_onChangeAspectRatio += value;
			}
			remove => _onChangeAspectRatio -= value;
		}

		private float          _prevFov;
		private Action<float>? _onChangeFov;

		public event Action<float> OnChangeFov
		{
			add
			{
				_onChangeFov -= value;
				_onChangeFov += value;
			}
			remove => _onChangeFov -= value;
		}

		private float          _prevNearClipPlane;
		private Action<float>? _onChangeNearClipPlane;

		public event Action<float> OnChangeNearClipPlane
		{
			add
			{
				_onChangeNearClipPlane -= value;
				_onChangeNearClipPlane += value;
			}
			remove => _onChangeNearClipPlane -= value;
		}

		private float          _prevFarClipPlane;
		private Action<float>? _onChangeFarClipPlane;

		public event Action<float> OnChangeFarClipPlane
		{
			add
			{
				_onChangeFarClipPlane -= value;
				_onChangeFarClipPlane += value;
			}
			remove => _onChangeFarClipPlane -= value;
		}

		private void InvokeCameraStateChange(CameraBase? exitCamera, CameraBase enterCamera)
		{
			_onCameraStateChange?.Invoke(exitCamera, enterCamera);
		}

		private void InvokeUpdateCameraView(int cameraViewIndex)
		{
			_onUpdateCameraView?.Invoke(cameraViewIndex);
		}

		private void InvokeCameraRendererChange(int renderer)
		{
			_onCameraRendererChange?.Invoke(renderer);
		}

		private void InvokeLensEvents()
		{
			var mainCamera = MainCamera;
			if (mainCamera.IsUnityNull())
				return;

			var currentFov = mainCamera!.fieldOfView;
			if (!Mathf.Approximately(currentFov, _prevFov))
			{
				_onChangeFov?.Invoke(mainCamera.fieldOfView);
				_prevFov = currentFov;
			}

			var currentNearClipPlane = mainCamera.nearClipPlane;
			if (!Mathf.Approximately(currentNearClipPlane, _prevNearClipPlane))
			{
				_onChangeNearClipPlane?.Invoke(mainCamera.nearClipPlane);
				_prevNearClipPlane = currentNearClipPlane;
			}

			var currentFarClipPlane = mainCamera.farClipPlane;
			if (!Mathf.Approximately(currentFarClipPlane, _prevFarClipPlane))
			{
				_onChangeFarClipPlane?.Invoke(mainCamera.farClipPlane);
				_prevFarClipPlane = currentFarClipPlane;
			}
		}

		private void OnScreenResized(int width, int height)
		{
			var mainCamera = MainCamera;
			if (mainCamera.IsUnityNull())
				return;

			var aspectRatio = mainCamera!.aspect;
			_onChangeAspectRatio?.Invoke(aspectRatio);
			_prevAspectRatio = aspectRatio;
		}

		private void ClearEvents()
		{
			_onCameraStateChange    = null;
			_onUpdateCameraView     = null;
			_onCameraRendererChange = null;
			_onChangeFov            = null;
			_onChangeNearClipPlane  = null;
			_onChangeFarClipPlane   = null;
		}
	}
}
