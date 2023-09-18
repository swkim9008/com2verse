/*===============================================================
* Product:		Com2Verse
* File Name:	FixedCamera.cs
* Developer:	eugene9721
* Date:			2022-05-18 11:56
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Cinemachine;
using Com2Verse.Extension;
using UnityEngine;

namespace Com2Verse.CameraSystem
{
	public sealed class FixedCamera : CameraBase
	{
		public FixedCamera(CinemachineVirtualCamera? virtualCamera, Camera? camera) : base(virtualCamera, camera, true) { }

		public FixedCamera(Camera? camera) : base(null, camera, false) { }

		public override void OnStateEnter()
		{
			if (!CheckHasVirtualCamera()) return;
			FixedCameraManager.Instance.Initialize();
			if (_isMainCamera) _virtualCamera!.Priority = CameraManager.Instance.DefaultPriority;
		}

		public override void OnStateExit()
		{
			if (!CheckHasVirtualCamera()) return;
			FixedCameraManager.Instance.Disable();
			if (_isMainCamera) _virtualCamera!.Priority = CameraManager.Instance.DefaultPriority;
		}

		public override void Dispose()
		{
			var fixedCameraManager = FixedCameraManager.InstanceOrNull;
			fixedCameraManager?.Disable();
		}

		public override void OnChangeTarget(Transform? cameraTarget)
		{
			base.OnChangeTarget(cameraTarget);

			if (!_isMainCamera)
			{
				if (cameraTarget.IsUnityNull()) return;
				SetVirtualCameraTransform(cameraTarget!);
			}
			else
			{
				if (!CheckHasVirtualCamera()) return;
				_virtualCamera!.Follow = null;
				_virtualCamera.LookAt  = null;
			}
		}

		private void SetVirtualCameraTransform(Transform targetTransform)
		{
			_camera!.transform.SetPositionAndRotation(
				targetTransform.position,
				targetTransform.rotation
			);
		}
	}
}
