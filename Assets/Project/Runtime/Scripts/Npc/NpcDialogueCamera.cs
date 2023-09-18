/*===============================================================
* Product:		Com2Verse
* File Name:	NpcDialogueCamera.cs
* Developer:	eugene9721
* Date:			2023-09-11 13:45
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.AssetSystem;
using UnityEngine;
using Com2Verse.CameraSystem;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Network;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Contents
{
	public sealed class NpcDialogueCamera
	{
		private const string CameraKey         = "NpcDialogueCamera";
		private const string CameraAddressable = "VCamera_NpcDialogue.prefab";

		private GameObject? _cameraJigPrefab;
		private GameObject? _cameraJigObject;

		// Table Data
		private float _cameraDistance          = 4.5f;
		private float _forceRotateYawThreshold = 20f;
		private float _pitchThreshold          = 20f;
		private float _heightRatio             = 0.33f;
		private float _distanceRatio           = 0.5f;
		private float _inCameraBlendTime       = 2f;
		private float _outCameraBlendTime      = 2f;

		// TODO: 테이블 데이터 값 이용, 매개변수 타입 변경
		public void SetData(float cameraDistance, float forceRotateYawThreshold, float pitchThreshold, float heightRatio, float distanceRatio, float inCameraBlendTime, float outCameraBlendTime)
		{
			_cameraDistance          = cameraDistance;
			_forceRotateYawThreshold = forceRotateYawThreshold;
			_pitchThreshold          = pitchThreshold;
			_heightRatio             = heightRatio;
			_distanceRatio           = distanceRatio;
			_inCameraBlendTime       = inCameraBlendTime;
			_outCameraBlendTime      = outCameraBlendTime;
		}

		public enum eCameraTarget
		{
			DEFAULT,
		}

		public void Initialize()
		{
			LoadCameraPrefab().Forget();
		}

		private async UniTask LoadCameraPrefab()
		{
			var handle = C2VAddressables.LoadAssetAsync<GameObject>(CameraAddressable);
			if (handle == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"Failed to load camera asset : {CameraAddressable}");
				return;
			}

			var result = await handle.ToUniTask();
			if (result.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"Failed to load camera asset : {CameraAddressable}");
				return;
			}

			_cameraJigPrefab = result;
		}

		public void SetCharacterCamera()
		{
			var cameraManager = CameraManager.InstanceOrNull;
			if (cameraManager == null)
				return;

			cameraManager.SetCameraBlendTime(_outCameraBlendTime);
			cameraManager.ChangeState(eCameraState.FOLLOW_CAMERA);
			var metaverseCamera = cameraManager.MetaverseCamera;
			if (!metaverseCamera.IsUnityNull())
				metaverseCamera!.OnBlending(SetDefaultBlendTime);
		}

		private void SetDefaultBlendTime()
		{
			CameraManager.InstanceOrNull?.SetDefaultBlendTime();
		}

		public void SetCamera(eCameraTarget target, ActiveObject character, ActiveObject npc)
		{
			CameraManager.InstanceOrNull?.SetCameraBlendTime(_inCameraBlendTime);

			switch (target)
			{
				case eCameraTarget.DEFAULT:
					SetDefaultCamera(character, npc);
					break;
			}
		}

		private void SetDefaultCamera(ActiveObject character, ActiveObject npc)
		{
			var cameraManager = CameraManager.InstanceOrNull;
			var camera        = cameraManager?.MainCamera;
			if (cameraManager == null || camera.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "camera is null");
				return;
			}

			var cameraPosition    = camera!.transform.position;
			var characterPosition = character.transform.position;
			var npcPosition       = npc.transform.position;

			var cameraToCharacterDir = (characterPosition - cameraPosition).normalized;
			var cameraToNpcDir       = (npcPosition       - cameraPosition).normalized;
			var yawAngle             = Vector3.SignedAngle(cameraToCharacterDir, cameraToNpcDir, Vector3.up);

			var centerHeight      = (character.Height + npc.Height) * _heightRatio;
			var characterToNpc   = npcPosition - characterPosition;

			var center            = characterPosition + characterToNpc * _distanceRatio + Vector3.up * centerHeight;
			var centerToCameraDir = (cameraPosition - center).normalized;

			if (!TryRefreshCameraJig())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "Failed to refresh camera jig");
				return;
			}

			_cameraJigObject!.transform.position = center + centerToCameraDir * _cameraDistance;
			if (Mathf.Abs(yawAngle) < _forceRotateYawThreshold)
			{
				var correctionAngle = yawAngle > 0 ? yawAngle - _forceRotateYawThreshold : yawAngle + _forceRotateYawThreshold;
				_cameraJigObject!.transform.RotateAround(center, Vector3.up, correctionAngle);
				centerToCameraDir = (_cameraJigObject.transform.position - center).normalized;
			}

			var rightVector   = Vector3.Cross(Vector3.up,  centerToCameraDir);
			var forwardVector = Vector3.Cross(rightVector, Vector3.up);
			var pitchAngle    = Vector3.SignedAngle(forwardVector, centerToCameraDir, rightVector);

			if (Mathf.Abs(pitchAngle) > _pitchThreshold)
			{
				var correctionAngle = pitchAngle > 0 ? -(pitchAngle + _pitchThreshold - pitchAngle) : -pitchAngle + _pitchThreshold + pitchAngle;
				_cameraJigObject!.transform.RotateAround(center, rightVector, correctionAngle);
			}
			_cameraJigObject!.transform.LookAt(center);

			cameraManager.ChangeState(eCameraState.FIXED_CAMERA, CameraKey);
		}

		private bool TryRefreshCameraJig()
		{
			if (!_cameraJigObject.IsUnityNull()) 
				return _cameraJigObject!;

			_cameraJigObject = Object.Instantiate(_cameraJigPrefab);
			return !_cameraJigObject.IsReferenceNull();
		}
	}
}
