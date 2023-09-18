/*===============================================================
* Product:		Com2Verse
* File Name:	FollowCameraTargetProxy.cs
* Developer:	eugene9721
* Date:			2022-06-20 15:51
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Data;
using Com2Verse.Extension;
using UnityEngine;

namespace Com2Verse.CameraSystem
{
	public sealed class FollowCameraTargetProxy : MonoBehaviour
	{
		[SerializeField] private float _smoothFactor     = 6f;
		[SerializeField] private float _teleportDistance = 10f;
		[SerializeField] private float _skipDistance     = 0.1f;

		private float _height;

		private Transform? _targetTransform;

		public void SetTarget(Transform? targetTransform)
		{
			_targetTransform = targetTransform;
		}

		public void SetTarget(Transform? targetTransform, float height)
		{
			_targetTransform = targetTransform;
			_height          = height;
		}

		public void SetHeight(float height)
		{
			_height = height;
		}

		public void SetTableData(Data.FollowCameraTargetProxy data)
		{
			_smoothFactor     = data.SmoothFactor;
			_teleportDistance = data.TeleportDistance;
			_skipDistance     = data.SkipDistance;
		}

		public void SetTableData(FollowCamera.CameraTargetProxySetting data)
		{
			_smoothFactor     = data.SmoothFactor;
			_teleportDistance = data.TeleportDistance;
			_skipDistance     = data.SkipDistance;
		}

		private void Update()
		{
			if (CameraManager.Instance.CurrentState is not eCameraState.FOLLOW_CAMERA) return;
			if (_targetTransform.IsUnityNull()) return;

			var   targetPosition = _targetTransform!.position + Vector3.up * _height;
			float distance       = Vector3.Distance(transform.position, targetPosition);

			if (distance > _teleportDistance)
			{
				transform.position = targetPosition;
				return;
			}

			if (distance < _skipDistance) return;

			var currentPosition = Vector3.Lerp(transform.position, targetPosition,
			                                   _smoothFactor * Time.deltaTime);
			transform.position = currentPosition;
		}
	}
}
