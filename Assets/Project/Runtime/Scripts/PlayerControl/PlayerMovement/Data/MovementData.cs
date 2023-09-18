/*===============================================================
* Product:		Com2Verse
* File Name:	MovementData.cs
* Developer:	eugene9721
* Date:			2023-02-03 16:53
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
	public sealed class MovementData : IMovementData
	{
		[field: SerializeField]
		public float Speed { get; set; } = 5f;

		[field: SerializeField]
		public float SprintSpeed { get; set; } = 10f;

		[field: SerializeField]
		public float MoveSpeedAccelerationFactor { get; set; } = 3;

		[field: SerializeField]
		public float MoveSpeedAccelerationTimeFactor { get; set; } = 7;

		[field: SerializeField]
		public float SpeedRatioInAir { get; set; } = 0.8f;

		[field: SerializeField]
		public float MoveAnimValueSmoothFactor { get; set; } = 12;

		[field: SerializeField]
		public float MoveCommandRotationSmoothFactor { get; set; } = 8f;

		[field: SerializeField]
		public float MoveToRotationSmoothFactor { get; set; } = 15;

		public void SetData(AvatarControl data)
		{
			if (data == null)
			{
				C2VDebug.LogErrorCategory(nameof(MovementData), "data is null");
				return;
			}

			Speed                           = data.Speed;
			SprintSpeed                     = data.SprintSpeed;
			MoveSpeedAccelerationFactor     = data.MoveSpeedAccelerationFactor;
			MoveSpeedAccelerationTimeFactor = data.MoveSpeedAccelerationTimeFactor;
			SpeedRatioInAir                 = data.SpeedRatioInAir;
			MoveAnimValueSmoothFactor       = data.MoveAnimValueSmoothFactor;
			MoveCommandRotationSmoothFactor = data.MoveCommandRotationSmoothFactor;
			MoveToRotationSmoothFactor      = data.MoveToRotationSmoothFactor;
		}
	}
}
