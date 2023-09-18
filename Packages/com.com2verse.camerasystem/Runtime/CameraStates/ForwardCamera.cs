/*===============================================================
* Product:		Com2Verse
* File Name:	ForwardCamera.cs
* Developer:	eugene9721
* Date:			2022-07-29 14:02
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Cinemachine;
using Com2Verse.Data;
using Com2Verse.Extension;
using UnityEngine;

namespace Com2Verse.CameraSystem
{
	public sealed class ForwardCamera : CameraBase
	{
		private IAvatarTarget? _avatarTarget;

#region MainCamera Cinemachine
		private CinemachineTransposer? _transposer;
		private CinemachineComposer?   _composer;
#endregion MainCamera Cinemachine

		private TableForwardCamera? _tableForwardCamera;

		private Vector3 _bodyOffset;
		private Vector3 _bodyDamping;
		private float   _bodyYawDamping;

		private Vector3 _aimOffset;
		private Vector2 _aimDamping;
		private Vector2 _aimSoftZoneSize;

		private float _height;

		public static ForwardCamera Create(CinemachineVirtualCamera? virtualCamera, Camera? camera)
		{
			var forwardCamera = new ForwardCamera(virtualCamera, camera);
			forwardCamera.LoadSingleDataTable();
			return forwardCamera;
		}

		public static ForwardCamera Create(Camera? camera)
		{
			var forwardCamera = new ForwardCamera(camera);
			forwardCamera.LoadSingleDataTable();
			return forwardCamera;
		}

		private ForwardCamera(CinemachineVirtualCamera? virtualCamera, Camera? camera) : base(virtualCamera, camera, true) { }

		private ForwardCamera(Camera? camera) : base(null, camera, false) { }

#region Ovverides
		public override void OnStateEnter() { }

		public override void OnStateExit()
		{
			_avatarTarget = null;

			if (!CheckHasVirtualCamera()) return;

			_transposer = null;
			_composer   = null;

			_virtualCamera!.DestroyCinemachineComponent<CinemachineComponentBase>();
		}

		public override void Dispose() { }

		public override void OnChangeTarget(Transform? cameraTarget)
		{
			base.OnChangeTarget(cameraTarget);
			TargetObject = cameraTarget;
			Initialize();
		}

		public override void OnLateUpdate()
		{
			if (_avatarTarget == null) return;
			_height = _avatarTarget.Height;

			if (CheckHasVirtualCamera())
			{
				if (_transposer.IsReferenceNull() || _composer.IsReferenceNull())
					InitializeVirtualCamera();

				_transposer!.m_FollowOffset      = _bodyOffset + Vector3.up * _height;
				_composer!.m_TrackedObjectOffset = _aimOffset + Vector3.up * _height;

				return;
			}

			if (_camera.IsReferenceNull()) return;
			if (TargetObject.IsReferenceNull()) return;

			SetCameraFrameBody();
			SetCameraFrameAim();
		}
#endregion Ovverides

#region CameraFrame Update
		private void SetCameraFrameBody()
		{
			var targetObjectTr = TargetObject!.transform;
			var bodyOffset = targetObjectTr.forward * _bodyOffset.z +
			                 targetObjectTr.up * _bodyOffset.y +
			                 targetObjectTr.right * _bodyOffset.x;
			var followOffset   = bodyOffset + Vector3.up * _height;
			var targetPosition = targetObjectTr.position + followOffset;
			_camera!.transform.position = targetPosition;
		}

		private void SetCameraFrameAim()
		{
			var targetObjectTr = TargetObject!.transform;
			var aimOffset = targetObjectTr.forward * _aimOffset.z +
			                targetObjectTr.up * _aimOffset.y +
			                targetObjectTr.right * _aimOffset.x;
			var trackedObjectOffset  = aimOffset + Vector3.up * _height;
			var targetLookAtPosition = targetObjectTr.position + trackedObjectOffset;

			_camera!.transform.LookAt(targetLookAtPosition);
		}
#endregion CameraFrame Update

#region Initialize
		public void Initialize()
		{
			_height = 0f;
			if (!TargetObject.IsUnityNull() && TargetObject!.TryGetComponent(out IAvatarTarget avatarTarget))
			{
				_avatarTarget = avatarTarget;
				_height       = _avatarTarget!.Height;
			}
			else
			{
				_avatarTarget = null;
			}

			if (_isMainCamera) InitializeVirtualCamera();
		}

		private void InitializeVirtualCamera()
		{
			if (!CheckHasVirtualCamera()) return;
			if (TargetObject.IsUnityNull())
			{
				_virtualCamera!.Follow = null;
				_virtualCamera.LookAt  = null;
				return;
			}

			var targetTransform = TargetObject!.transform;
			_virtualCamera!.Follow = targetTransform;
			_virtualCamera.LookAt  = targetTransform;

			SetTransposer();
			SetComposer();
		}
#endregion Initialize

#region MainCamera Cinemachine Setting
		private void SetTransposer()
		{
			_transposer = _virtualCamera!.AddCinemachineComponent<CinemachineTransposer>()!;

			_transposer.m_XDamping = _bodyDamping.x;
			_transposer.m_YDamping = _bodyDamping.y;
			_transposer.m_ZDamping = _bodyDamping.z;

			_transposer.m_YawDamping   = _bodyYawDamping;
			_transposer.m_FollowOffset = _bodyOffset + Vector3.up * _height;
		}

		private void SetComposer()
		{
			_composer = _virtualCamera!.AddCinemachineComponent<CinemachineComposer>()!;

			_composer.m_HorizontalDamping   = _aimDamping.x;
			_composer.m_VerticalDamping     = _aimDamping.y;
			_composer.m_SoftZoneHeight      = _aimSoftZoneSize.x;
			_composer.m_SoftZoneWidth       = _aimSoftZoneSize.y;
			_composer.m_TrackedObjectOffset = _aimOffset + Vector3.up * _height;
		}
#endregion MainCamera Cinemachine Setting

#region TableData
		private void LoadSingleDataTable()
		{
			_tableForwardCamera = TableDataManager.Instance.Get<TableForwardCamera>();
			SetForwardCameraTable();
		}

		private void SetForwardCameraTable()
		{
			if (_tableForwardCamera == null) return;
			var currentTableData = _tableForwardCamera.Datas![CameraDefine.DEFAULT_TABLE_INDEX]!;

			ForwardCameraBodySetting(
				currentTableData.BodyOffset,
				currentTableData.BodyDamping,
				currentTableData.BodyYawDamping
			);

			ForwardCameraAimSetting(
				currentTableData.AimOffset,
				currentTableData.AimDamping,
				currentTableData.AimSoftZoneSize
			);
		}

		public void ForwardCameraBodySetting(Vector3 bodyOffset, Vector3 bodyDamping, float bodyYamDamping)
		{
			_bodyOffset     = bodyOffset;
			_bodyDamping    = bodyDamping;
			_bodyYawDamping = bodyYamDamping;
		}

		public void ForwardCameraAimSetting(Vector3 aimOffset, Vector2 aimDamping, Vector2 aimSoftZoneSize)
		{
			_aimOffset       = aimOffset;
			_aimDamping      = aimDamping;
			_aimSoftZoneSize = aimSoftZoneSize;
		}
#endregion TableData
	}
}
