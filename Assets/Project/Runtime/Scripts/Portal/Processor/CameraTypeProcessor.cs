/*===============================================================
* Product:		Com2Verse
* File Name:	CameraTypeProcessor.cs
* Developer:	haminjeong
* Date:			2023-06-30 17:02
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.CameraSystem;
using Com2Verse.Data;
using FollowCamera = Com2Verse.CameraSystem.FollowCamera;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.CAMERA_CHANGE)]
	public sealed class CameraTypeProcessor : BaseLogicTypeProcessor
	{
		public override void OnZoneEnter(ServerZone zone, int callbackIndex)
		{
			if (CameraManager.Instance.CurrentState != eCameraState.FOLLOW_CAMERA) return;
			var    callback    = zone.Callback[callbackIndex];
			string typeString = null;
			if (callback is { InteractionValue: { Count: > 0 } })
				typeString = callback.InteractionValue[0];
			if (!string.IsNullOrEmpty(typeString) && int.TryParse(typeString, out var cameraType))
				SetFollowCameraData(cameraType);
		}

		public override void OnZoneExit(ServerZone zone, int callbackIndex)
		{
			if (CameraManager.Instance.CurrentState != eCameraState.FOLLOW_CAMERA) return;
			SetFollowCameraData(CameraDefine.DEFAULT_TABLE_INDEX);
		}

		private void SetFollowCameraData(int index)
		{
			var cameraManager = CameraManager.InstanceOrNull;
			var followCamera  = cameraManager?.StateMap[eCameraState.FOLLOW_CAMERA] as FollowCamera;
			followCamera?.SetFollowCameraTable(index);
			followCamera?.SetClampZoomFactor();
		}
	}
}
