/*===============================================================
* Product:		Com2Verse
* File Name:	CameraBase.cs
* Developer:	eugene9721
* Date:			2022-05-17 12:20
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Cinemachine;
using Com2Verse.Extension;
using Com2Verse.Logger;
using UnityEngine;

namespace Com2Verse.CameraSystem
{
	public abstract class CameraBase : IUpdatableCamera, IDisposable
	{
		protected readonly CinemachineVirtualCamera? _virtualCamera;
		protected readonly bool                      _isMainCamera;
		protected          Camera?                   _camera;

		public Transform? TargetObject { protected set; get; }

		protected CameraBase(CinemachineVirtualCamera? virtualCamera, Camera? camera, bool isMainCamera)
		{
			_virtualCamera = virtualCamera;
			_camera        = camera;
			_isMainCamera  = isMainCamera;

			// Main Camera인 경우에만 Virtual Camera가 필요하다.
			if (isMainCamera && virtualCamera.IsUnityNull())
				C2VDebug.LogError("[CameraSystem] Main Camera의 Virtual Camera가 없습니다.");
		}

		public void SetCamera(Camera? camera) => _camera = camera;

		public abstract void OnStateEnter();
		public abstract void OnStateExit();
		public abstract void Dispose();

		public virtual void OnChangeTarget(Transform? cameraTarget) => TargetObject = cameraTarget;

		public virtual void OnUpdate()     { }
		public virtual void OnLateUpdate() { }
		public virtual void InitializeValue() { }

		protected bool CheckHasVirtualCamera()
		{
			if (_isMainCamera) return true;
			if (!_virtualCamera.IsUnityNull()) return true;
			C2VDebug.LogErrorCategory(nameof(CameraSystem), "virtualCamera is null");
			return false;
		}
	}
}
