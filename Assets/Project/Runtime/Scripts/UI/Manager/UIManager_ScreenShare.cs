/*===============================================================
* Product:		Com2Verse
* File Name:	UIManager_ScreenShare.cs
* Developer:	urun4m0r1
* Date:			2022-12-26 12:18
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.CameraSystem;
using Com2Verse.Data;
using Com2Verse.Project.CameraSystem;
using Com2Verse.ScreenShare;

namespace Com2Verse.UI
{
	public partial class UIManager
	{
		private void OnScreenShareSignalChanged(eScreenShareSignal type)
		{
			switch (type)
			{
				case eScreenShareSignal.NONE:
				case eScreenShareSignal.IGNORE:
				case eScreenShareSignal.CAPTURE_CHANGED_BY_USER:
					break;
				case eScreenShareSignal.CAPTURE_STARTED_BY_USER:
					SwitchToScreenCamera();
					SwitchToImmersiveUi();
					break;
				case eScreenShareSignal.CAPTURE_STOPPED_BY_USER:
					SwitchToNormalUi();
					SwitchToFollowCamera();
					break;
				case eScreenShareSignal.CAPTURE_STOPPED_BY_SYSTEM:
				case eScreenShareSignal.CAPTURE_STOPPED_BY_SCREEN_REMOVED:
				case eScreenShareSignal.CAPTURE_STOPPED_BY_VISIBILITY:
				case eScreenShareSignal.CAPTURE_STOPPED_BY_REMOTE:
					if (ScreenCaptureManager.InstanceOrNull?.Controller.IsCaptureRequestedOrCapturing is true)
					{
						ViewModelManager.InstanceOrNull.Get<ScreenShareViewModel>()?.StopShare.Invoke(null!);

						SwitchToNormalUi();
						SwitchToFollowCamera();

						ShowPopupCommon(Utils.Define.String.UI_MeetingRoomSharing_SharingStopped);
					}

					break;
				case eScreenShareSignal.RECEIVE_STARTED_BY_REMOTE:
					SwitchToScreenCamera();
					break;
				case eScreenShareSignal.RECEIVE_STOPPED_BY_REMOTE:
					SwitchToFollowCamera();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null!);
			}
		}

		private static void SwitchToImmersiveUi()
		{
			return; // TODO: 화면 공유 시 컴투버스 최소화 기능이 필요한 경우 구현
		}

		private static void SwitchToNormalUi()
		{
			return; // TODO: 화면 공유 시 컴투버스 최소화 기능이 필요한 경우 구현
		}

		private static void SwitchToScreenCamera()
		{
			if (CanChangeCameraJig && IsAnyScreenAvailable)
				CameraManager.Instance.ChangeState(eCameraState.FIXED_CAMERA, CameraJigKey.MeetingScreenView);
		}

		private static void SwitchToFollowCamera()
		{
			if (CanChangeCameraJig && !IsAnyScreenAvailable)
				CameraManager.Instance.ChangeState(eCameraState.FOLLOW_CAMERA);
		}

		private static bool IsAnyScreenAvailable => ScreenCaptureManager.InstanceOrNull?.RemoteSharingUsers.Count > 0 || ScreenCaptureManager.InstanceOrNull?.Controller.IsCaptureRequested is true;
		private static bool CanChangeCameraJig   => ViewModelManager.Instance.Get<UserCharacterStateViewModel>()?.UserCharacterStateSit is true;
	}
}
