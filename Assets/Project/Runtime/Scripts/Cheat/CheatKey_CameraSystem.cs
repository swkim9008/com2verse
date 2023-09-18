#if ENABLE_CHEATING

/*===============================================================
* Product:		Com2Verse
* File Name:	CheatKey_CameraSystem.cs
* Developer:	eugene9721
* Date:			2023-01-04 18:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Diagnostics.CodeAnalysis;
using Com2Verse.CameraSystem;
using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.Utils;
using Cysharp.Text;

namespace Com2Verse.Cheat
{
	[SuppressMessage("ReSharper", "UnusedMember.Local")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static partial class CheatKey
	{
		[MetaverseCheat("Cheat/CameraSystem/VirtualCameraSetting")]
		private static void VirtualCameraSetting(string verticalFOV = "40", string nearClipPlane = "0.3",
		                                         string farClipPlane = "5000", string defaultPriority = "10")
		{
			var verticalFOVFloat   = Convert.ToSingle(verticalFOV);
			var nearClipPlaneFloat = Convert.ToSingle(nearClipPlane);
			var farClipPlaneFloat  = Convert.ToSingle(farClipPlane);
			var defaultPriorityInt = Convert.ToInt32(defaultPriority);

			CameraManager.Instance.SetVirtualCameraSetting(verticalFOVFloat, nearClipPlaneFloat, farClipPlaneFloat, defaultPriorityInt);

			var stringBuilder = ZString.CreateStringBuilder();
			stringBuilder.AppendLine($"ApplyCheat! : {nameof(VirtualCameraSetting)}");
			stringBuilder.AppendLine("-----");
			stringBuilder.AppendLine($"{nameof(verticalFOV)}: {verticalFOV}");
			stringBuilder.AppendLine($"{nameof(nearClipPlane)}: {nearClipPlane}");
			stringBuilder.AppendLine($"{nameof(farClipPlane)}: {farClipPlane}");
			stringBuilder.AppendLine($"{nameof(defaultPriority)}: {defaultPriority}");
			C2VDebug.Log(stringBuilder.ToString());
		}

		[MetaverseCheat("Cheat/CameraSystem/FollowCameraZoom")]
		private static void FollowCameraZoom(string scaleFactorCameraDistance = "1", string zoomSmoothFactor = "0.14",
		                                     string zoomThreshold = "0.1", string minCameraDistance = "1", string maxCameraDistance = "9")
		{
			if (CameraManager.Instance.StateMap[eCameraState.FOLLOW_CAMERA] is not CameraSystem.FollowCamera followCamera) return;

			var scaleFactorCameraDistanceFloat = Convert.ToSingle(scaleFactorCameraDistance);
			var zoomSmoothFactorFloat          = Convert.ToSingle(zoomSmoothFactor);
			var zoomThresholdFloat             = Convert.ToSingle(zoomThreshold);
			var minCameraDistanceFloat         = Convert.ToSingle(minCameraDistance);
			var maxCameraDistanceFloat         = Convert.ToSingle(maxCameraDistance);

			followCamera.SetCameraZoomSetting(scaleFactorCameraDistanceFloat, zoomSmoothFactorFloat, zoomThresholdFloat, minCameraDistanceFloat, maxCameraDistanceFloat);

			var stringBuilder = ZString.CreateStringBuilder();
			stringBuilder.AppendLine($"ApplyCheat! : {nameof(FollowCameraZoom)}");
			stringBuilder.AppendLine("-----");
			stringBuilder.AppendLine($"{nameof(scaleFactorCameraDistance)}: {scaleFactorCameraDistance}");
			stringBuilder.AppendLine($"{nameof(zoomSmoothFactor)}: {zoomSmoothFactor}");
			stringBuilder.AppendLine($"{nameof(zoomThreshold)}: {zoomThreshold}");
			stringBuilder.AppendLine($"{nameof(minCameraDistance)}: {minCameraDistance}");
			stringBuilder.AppendLine($"{nameof(maxCameraDistance)}: {maxCameraDistance}");
			C2VDebug.Log(stringBuilder.ToString());
		}

		[MetaverseCheat("Cheat/CameraSystem/FollowCameraRotation")]
		private static void FollowCameraRotation(string scaleFactorMouseRotation, string cameraRotationSmoothFactor, string yawCinemachineTargetDefault, string pitchCinemachineTargetDefault,
		                                         string rotationVelocityYawByKeyboard, string rotationYawWeightByWaypoint, string rotationPerSecondDueToWaypoint, string topClamp, string bottomClamp,
		                                         string cameraAngleOverride)
		{
			if (CameraManager.Instance.StateMap[eCameraState.FOLLOW_CAMERA] is not CameraSystem.FollowCamera followCamera) return;

			var scaleFactorMouseRotationFloat       = Convert.ToSingle(scaleFactorMouseRotation);
			var cameraRotationSmoothFactorFloat     = Convert.ToSingle(cameraRotationSmoothFactor);
			var yawCinemachineTargetDefaultFloat    = Convert.ToSingle(yawCinemachineTargetDefault);
			var pitchCinemachineTargetDefaultFloat  = Convert.ToSingle(pitchCinemachineTargetDefault);
			var rotationVelocityYawByKeyboardFloat  = Convert.ToSingle(rotationVelocityYawByKeyboard);
			var rotationYawWeightByWaypointFloat    = Convert.ToSingle(rotationYawWeightByWaypoint);
			var rotationPerSecondDueToWaypointFloat = Convert.ToSingle(rotationPerSecondDueToWaypoint);

			var topClampFloat              = Convert.ToSingle(topClamp);
			var bottomClampFloat           = Convert.ToSingle(bottomClamp);
			var cameraAngleOverrideFloat   = Convert.ToSingle(cameraAngleOverride);

			followCamera.SetCameraRotationBaseSetting(scaleFactorMouseRotationFloat, cameraRotationSmoothFactorFloat, yawCinemachineTargetDefaultFloat, pitchCinemachineTargetDefaultFloat,
			                                          rotationVelocityYawByKeyboardFloat, rotationYawWeightByWaypointFloat, rotationPerSecondDueToWaypointFloat);

			followCamera.SetCameraRotationLimitSetting(topClampFloat, bottomClampFloat, cameraAngleOverrideFloat);

			var stringBuilder = ZString.CreateStringBuilder();
			stringBuilder.AppendLine($"ApplyCheat! : {nameof(FollowCameraRotation)}");
			stringBuilder.AppendLine("-----");
			stringBuilder.AppendLine($"{nameof(scaleFactorMouseRotation)}: {scaleFactorMouseRotation}");
			stringBuilder.AppendLine($"{nameof(cameraRotationSmoothFactor)}: {cameraRotationSmoothFactor}");
			stringBuilder.AppendLine($"{nameof(yawCinemachineTargetDefault)}: {yawCinemachineTargetDefault}");
			stringBuilder.AppendLine($"{nameof(pitchCinemachineTargetDefault)}: {pitchCinemachineTargetDefault}");
			stringBuilder.AppendLine($"{nameof(rotationVelocityYawByKeyboard)}: {rotationVelocityYawByKeyboard}");
			stringBuilder.AppendLine($"{nameof(rotationYawWeightByWaypoint)}: {rotationYawWeightByWaypoint}");
			stringBuilder.AppendLine($"{nameof(rotationPerSecondDueToWaypoint)}: {rotationPerSecondDueToWaypoint}");
			stringBuilder.AppendLine("");
			stringBuilder.AppendLine($"{nameof(topClamp)}: {topClamp}");
			stringBuilder.AppendLine($"{nameof(bottomClamp)}: {bottomClamp}");
			stringBuilder.AppendLine($"{nameof(cameraAngleOverride)}: {cameraAngleOverride}");
			C2VDebug.Log(stringBuilder.ToString());
		}

		[MetaverseCheat("Cheat/CameraSystem/FollowCameraCinemachine")]
		private static void FollowCameraCinemachine(string thirdFieldOfViewDefault = "40", string thirdCameraDistanceDefault = "4",
		                                            string shoulderOffset = "(0,1.8,0)", string verticalArmLength = "-0.4")
		{
			if (CameraManager.Instance.StateMap[eCameraState.FOLLOW_CAMERA] is not CameraSystem.FollowCamera followCamera) return;

			var thirdFieldOfViewDefaultFloat    = Convert.ToSingle(thirdFieldOfViewDefault);
			var thirdCameraDistanceDefaultFloat = Convert.ToSingle(thirdCameraDistanceDefault);
			var shoulderOffsetVector3           = Util.StringToVector3(shoulderOffset);
			var verticalArmLengthFloat          = Convert.ToSingle(verticalArmLength);

			followCamera.SetCameraCinemachineSetting(thirdFieldOfViewDefaultFloat, thirdCameraDistanceDefaultFloat,
			                                         shoulderOffsetVector3, verticalArmLengthFloat);

			var stringBuilder = ZString.CreateStringBuilder();
			stringBuilder.AppendLine($"ApplyCheat! : {nameof(FollowCameraCinemachine)}");
			stringBuilder.AppendLine("-----");
			stringBuilder.AppendLine($"{nameof(thirdFieldOfViewDefault)}: {thirdFieldOfViewDefault}");
			stringBuilder.AppendLine($"{nameof(thirdCameraDistanceDefault)}: {thirdCameraDistanceDefault}");
			stringBuilder.AppendLine($"{nameof(shoulderOffset)}: {shoulderOffset}");
			stringBuilder.AppendLine($"{nameof(verticalArmLength)}: {verticalArmLength}");
			C2VDebug.Log(stringBuilder.ToString());
		}

		[MetaverseCheat("Cheat/CameraSystem/ForwardCameraBody")]
		private static void ForwardCameraBody(string bodyOffset = "(0,0,1.2)", string bodyDamping = "(0,0,0)", string bodyYamDamping = "0")
		{
			if (CameraManager.Instance.StateMap[eCameraState.FORWARD_CAMERA] is not CameraSystem.ForwardCamera forwardCamera) return;

			var bodyOffsetVector3   = Util.StringToVector3(bodyOffset);
			var bodyDampingVector3  = Util.StringToVector3(bodyDamping);
			var bodyYamDampingFloat = Convert.ToSingle(bodyYamDamping);

			forwardCamera.ForwardCameraBodySetting(bodyOffsetVector3, bodyDampingVector3, bodyYamDampingFloat);
			forwardCamera.Initialize();

			var stringBuilder = ZString.CreateStringBuilder();
			stringBuilder.AppendLine($"ApplyCheat! : {nameof(ForwardCameraBody)}");
			stringBuilder.AppendLine("-----");
			stringBuilder.AppendLine($"{nameof(bodyOffset)}: {bodyOffset}");
			stringBuilder.AppendLine($"{nameof(bodyDamping)}: {bodyDamping}");
			stringBuilder.AppendLine($"{nameof(bodyYamDamping)}: {bodyYamDamping}");
			C2VDebug.Log(stringBuilder.ToString());
		}

		[MetaverseCheat("Cheat/CameraSystem/ForwardCameraAim")]
		private static void ForwardCameraAim(string aimOffset = "(0,0,0)", string aimDamping = "(0,0)", string aimSoftZoneSize = "(0,0)")
		{
			if (CameraManager.Instance.StateMap[eCameraState.FORWARD_CAMERA] is not CameraSystem.ForwardCamera forwardCamera) return;

			var aimOffsetVector3       = Util.StringToVector3(aimOffset);
			var aimDampingVector2      = Util.StringToVector2(aimDamping);
			var aimSoftZoneSizeVector2 = Util.StringToVector2(aimSoftZoneSize);

			forwardCamera.ForwardCameraAimSetting(aimOffsetVector3, aimDampingVector2, aimSoftZoneSizeVector2);
			forwardCamera.Initialize();

			var stringBuilder = ZString.CreateStringBuilder();
			stringBuilder.AppendLine($"ApplyCheat! : {nameof(ForwardCameraAim)}");
			stringBuilder.AppendLine("-----");
			stringBuilder.AppendLine($"{nameof(aimOffset)}: {aimOffset}");
			stringBuilder.AppendLine($"{nameof(aimDamping)}: {aimDamping}");
			stringBuilder.AppendLine($"{nameof(aimSoftZoneSize)}: {aimSoftZoneSize}");
			C2VDebug.Log(stringBuilder.ToString());
		}

		[MetaverseCheat("Cheat/CameraSystem/MainCameraStateChange")] [HelpText("(int)eCameraState")]
		private static void MainCameraStateChange(string state)
		{
			var stateInt = Convert.ToInt32(state);
			foreach (eCameraState value in Enum.GetValues(typeof(eCameraState)))
			{
				if (value != (eCameraState)stateInt) continue;
				CameraManager.Instance.ChangeState((eCameraState)stateInt);
				C2VDebug.Log($"[CameraSystem] {value.ToString()}이 적용되었습니다.");
				break;
			}
		}
	}
}
#endif
