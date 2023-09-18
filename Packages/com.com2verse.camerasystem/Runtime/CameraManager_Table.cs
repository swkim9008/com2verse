/*===============================================================
* Product:		Com2Verse
* File Name:	CameraManager_Table.cs
* Developer:	eugene9721
* Date:			2022-10-13 20:20
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;

namespace Com2Verse.CameraSystem
{
	public sealed partial class CameraManager
	{
		private TableCameraCommon         _tableCameraCommon;
		private TableCullingGroupDistance _tableCullingGroupDistance;

		public float BlendTime          { get; private set; } = 2f;
		public float VerticalFOV        { get; private set; } = 40f;
		public float NearClipPlane      { get; private set; } = 0.3f;
		public float FarClipPlane       { get; private set; } = 5000f;
		public int   DefaultPriority    { get; private set; } = 10;

		private void LoadTable()
		{
			_tableCameraCommon         = TableDataManager.Instance.Get<TableCameraCommon>();
			_tableCullingGroupDistance = TableDataManager.Instance.Get<TableCullingGroupDistance>();

			ApplyTable();
			SetVirtualCameraSetting(VerticalFOV, NearClipPlane, FarClipPlane, DefaultPriority);
			SetStateDict();
		}

		private void ApplyTable()
		{
			ApplyCameraCommonTable();
			ApplyCullingGroupDistanceTable();
		}

		private void ApplyCameraCommonTable()
		{
			if (_tableCameraCommon?.Datas[CameraDefine.DEFAULT_TABLE_INDEX] == null)
			{
				C2VDebug.LogError($"[{nameof(CameraManager)}] CameraCommon Table is null");
				return;
			}

			BlendTime       = _tableCameraCommon.Datas[CameraDefine.DEFAULT_TABLE_INDEX].BlendTime;
			VerticalFOV     = _tableCameraCommon.Datas[CameraDefine.DEFAULT_TABLE_INDEX].VerticalFOV;
			NearClipPlane   = _tableCameraCommon.Datas[CameraDefine.DEFAULT_TABLE_INDEX].NearClipPlane;
			FarClipPlane    = _tableCameraCommon.Datas[CameraDefine.DEFAULT_TABLE_INDEX].FarClipPlane;
			DefaultPriority = _tableCameraCommon.Datas[CameraDefine.DEFAULT_TABLE_INDEX].DefaultPriority;
		}

		private void ApplyCullingGroupDistanceTable()
		{
			if (_tableCullingGroupDistance?.Datas[CameraDefine.DEFAULT_TABLE_INDEX] == null)
			{
				C2VDebug.LogError($"[{nameof(CameraManager)}] ChatSetting Table is null");
				return;
			}

			var hudBoundingDistances = _tableCullingGroupDistance.Datas[CameraDefine.DEFAULT_TABLE_INDEX].BoundingDistances;
			SetCullingGroupBoundingDistance(eCullingGroupType.HUD, hudBoundingDistances);
		}

		public void SetVirtualCameraSetting(float verticalFOV, float nearClipPlane, float farClipPlane, int defaultPriority)
		{
			if (MainVirtualCamera.IsUnityNull())
			{
				C2VDebug.LogErrorMethod(nameof(CameraManager), "Failed to set virtual camera setting. MainVirtualCamera is null.");
				return;
			}

			MainVirtualCamera.m_Lens.FieldOfView   = verticalFOV;
			MainVirtualCamera.m_Lens.NearClipPlane = nearClipPlane;
			MainVirtualCamera.m_Lens.FarClipPlane  = farClipPlane;
			MainVirtualCamera.Priority             = defaultPriority;
		}

		private void SetStateDict()
		{
			var fixedCamera   = new FixedCamera(MainVirtualCamera, MainCamera);
			var forwardCamera = ForwardCamera.Create(MainVirtualCamera, MainCamera);
			var followCamera  = FollowCamera.Create(MainVirtualCamera, MainCamera);

			StateMap.Add(eCameraState.FIXED_CAMERA,   fixedCamera);
			StateMap.Add(eCameraState.FORWARD_CAMERA, forwardCamera);
			StateMap.Add(eCameraState.FOLLOW_CAMERA,  followCamera);
		}
	}
}
