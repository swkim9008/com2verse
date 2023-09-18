/*===============================================================
* Product:		Com2Verse
* File Name:	AnimationParameterSyncData.cs
* Developer:	eugene9721
* Date:			2023-05-20 17:17
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Data;
using Com2Verse.Logger;
using UnityEngine;

namespace Com2Verse.PlayerControl
{
	[Serializable]
	public sealed class AnimationParameterSyncData : IMovementData
	{
		[field: SerializeField]
		public bool IsEnable { get; private set; } = true;

		[field: SerializeField]
		public int ThrottleFrames { get; private set; } = 7;

		[field: SerializeField]
		public int ChangedFrame { get; private set; } = 7;

		[field: SerializeField]
		public int DistinctUntilChangedTarget { get; private set; } = 3;

		[field: SerializeField]
		public float MovingThresholdVelocity { get; private set; } = 1.0f;

		[field: SerializeField]
		public float RunThresholdVelocity { get; private set; } = 4.2f;

		[field: SerializeField]
		public float VelocitySmoothFactor { get; private set; } = 1f;

		[field: SerializeField]
		public float AvatarWalkSpeed { get; private set; } = 3.0f;

		[field: SerializeField]
		public float AvatarRunSpeed { get; private set; } = 5.5f;

		public void SetData(AnimationParameterSync data)
		{
			if (data == null)
			{
				C2VDebug.LogErrorCategory(nameof(JumpData), "data is null");
				return;
			}
			IsEnable                   = data.IsEnable;
			ThrottleFrames             = data.ThrottleFrames;
			ChangedFrame               = data.ChangedFrame;
			DistinctUntilChangedTarget = data.DistinctUntilChangedTarget;
			MovingThresholdVelocity    = data.MovingThresholdVelocity;
			RunThresholdVelocity       = data.RunThresholdVelocity;
			VelocitySmoothFactor       = data.VelocitySmoothFactor;
			AvatarWalkSpeed            = data.AvatarWalkSpeed;
			AvatarRunSpeed             = data.AvatarRunSpeed;
		}
	}
}
