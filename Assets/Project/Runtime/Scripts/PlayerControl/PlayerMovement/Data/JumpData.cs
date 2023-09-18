/*===============================================================
* Product:		Com2Verse
* File Name:	JumpData.cs
* Developer:	eugene9721
* Date:			2023-02-03 14:50
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
	public sealed class JumpData : IMovementData
	{
		[field: SerializeField]
		public float JumpInterval { get; private set; } = 1.2f;

		[field: SerializeField]
		public float JumpDelay { get; private set; } = 0.1f;

		[field: SerializeField]
		public float JumpPower { get; private set; } = 3f;

		[field: SerializeField]
		public float MaxJumpHeight { get; private set; } = 3f;

		[field: SerializeField]
		public float Gravity { get; private set; } = -9.8f;

		[field: SerializeField]
		public bool IsApplyGravityWeightInAir { get; private set; } = true;

		[field: SerializeField]
		public float SqrtGravityWeightInFalling { get; private set; } = 1f;

		[field: SerializeField]
		public float GravityTimeWeightInFalling { get; private set; } = 2f;

		[field: SerializeField]
		public float GravityCumulativeWeight { get; private set; } = 1f;

		[field: SerializeField]
		public float InAirThresholdWhenYUp { get; private set; } = 0.7f;

		[field: SerializeField]
		public float InAirThresholdWhenYDown { get; private set; } = -1f;

		[field: SerializeField]
		public float JumpDownHeight { get; private set; } = 0.7f;

		[field: SerializeField]
		public float MaximumJumpStartTime { get; private set; } = 0.7f;

		[field: SerializeField]
		public bool NeedJumpReady { get; private set; } = true;

		public void SetData(AvatarControl data)
		{
			if (data == null)
			{
				C2VDebug.LogErrorCategory(nameof(JumpData), "data is null");
				return;
			}

			JumpInterval               = data.JumpInterval;
			JumpDelay                  = data.JumpDelay;
			JumpPower                  = data.JumpPower;
			MaxJumpHeight              = data.MaxJumpHeight;
			Gravity                    = data.Gravity;
			IsApplyGravityWeightInAir  = data.IsApplyGravityWeightInAir;
			SqrtGravityWeightInFalling = data.SqrtGravityWeightInFalling;
			GravityTimeWeightInFalling = data.GravityTimeWeightInFalling;
			GravityCumulativeWeight    = data.GravityCumulativeWeight;
			InAirThresholdWhenYUp      = data.InAirThresholdWhenYUp;
			InAirThresholdWhenYDown    = data.InAirThresholdWhenYDown;
			JumpDownHeight             = data.JumpDownHeight;
			MaximumJumpStartTime       = data.MaximumJumpStartTime;
			NeedJumpReady              = data.NeedJumpReady;
		}
	}
}
