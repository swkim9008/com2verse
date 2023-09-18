/*===============================================================
* Product:		Com2Verse
* File Name:	ParameterSyncFilter.cs
* Developer:	eugene9721
* Date:			2023-04-03 16:27
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Utils;
using UnityEngine;

namespace Com2Verse.AvatarAnimation
{
	/// <summary>
	/// 서버와 자연스러운 애니메이션 동기화를 위한 애니메이션 값 동기화 필터
	/// </summary>
	[Serializable]
	public sealed class ParameterSyncFilter
	{
		private eAnimationState _prevAnimationState = eAnimationState.STAND;

		private bool _isEnable = true;

		private int _throttleFrames = 7;
		private int _changedFrame   = 7;

		private float _othersTargetVelocity;
		private int   _distinctUntilChangedCount;
		private int   _distinctUntilChangedTarget = 3;

		private float _movingThresholdVelocity = 0.1f;
		[SerializeField]
		private float _runThresholdVelocity    = 4.2f;

		private static readonly float ChangeVelocityThreshold = 3f;

		[SerializeField, ReadOnly]
		private float _prevVelocity;
		private bool  _isIncrease;

		[SerializeField]
		private float _velocitySmoothFactor = 1f;

		private float _avatarWalkSpeed = 3f;
		private float _avatarRunSpeed  = 5.5f;

		public void SetData(bool isEnable, int throttleFrames, int changedFrame, int distinctUntilChangedTarget, float movingThresholdVelocity,
		                    float runThresholdVelocity, float velocitySmoothFactor, float avatarWalkSpeed, float avatarRunSpeed)
		{
			_isEnable                   = isEnable;
			_throttleFrames             = throttleFrames;
			_changedFrame               = changedFrame;
			_distinctUntilChangedTarget = distinctUntilChangedTarget;
			_movingThresholdVelocity    = movingThresholdVelocity;
			_runThresholdVelocity       = runThresholdVelocity;
			_velocitySmoothFactor       = velocitySmoothFactor;
			_avatarWalkSpeed            = avatarWalkSpeed;
			_avatarRunSpeed             = avatarRunSpeed;

			var avatarControlData = AnimationManager.Instance.AvatarControlData;
			if (avatarControlData != null)
			{
				_avatarWalkSpeed = avatarControlData.Speed;
				_avatarRunSpeed  = avatarControlData.SprintSpeed;
			}
		}

		public float GetSyncVelocity(float targetVelocity, float deltaTime)
		{
			if (!_isEnable)
				return targetVelocity;

			if (targetVelocity < _movingThresholdVelocity)
			{
				_prevVelocity = 0f;
				return _prevVelocity;
			}

			if (Mathf.Approximately(_prevVelocity, 0f) && targetVelocity > 0f)
				_isIncrease = true;
			else
			{
				var currentIncrease = targetVelocity > _prevVelocity;
				if (currentIncrease != _isIncrease && Mathf.Abs(targetVelocity - _prevVelocity) > ChangeVelocityThreshold)
					_isIncrease = currentIncrease;
			}

			if (_isIncrease)
				_prevVelocity = targetVelocity >= _runThresholdVelocity ? _avatarRunSpeed : _avatarWalkSpeed;
			else
				_prevVelocity = 0f;

			return _prevVelocity;
		}
	}
}
