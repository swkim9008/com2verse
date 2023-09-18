/*===============================================================
* Product:		Com2Verse
* File Name:	ForwardCheckData.cs
* Developer:	eugene9721
* Date:			2023-03-10 19:53
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
	public sealed class ForwardCheckData : IMovementData
	{
		[field: SerializeField]
		public float CheckCapsuleRadius { get; private set; } = 0.3f;

		[field: SerializeField]
		public float CheckDistance { get; private set; } = 0.5f;

		[field: SerializeField]
		public LayerMask LayerMask { get; private set; } = -1;

		[field: SerializeField]
		[Tooltip("Ground 레이어와 별도로, 책상이나 가구 위에 올라가는 경우를 체크하기 위해 사용")]
		public float GroundCheckAngle { get; private set; } = 80f;

		public void SetData(AvatarControl data)
		{
			if (data == null)
			{
				C2VDebug.LogErrorCategory(nameof(JumpData), "data is null");
				return;
			}

			CheckCapsuleRadius = data.CheckCapsuleRadius;
			CheckDistance      = data.CheckDistance;
			LayerMask          = data.LayerMask;
			GroundCheckAngle   = data.GroundCheckAngle;
		}
	}
}
