/*===============================================================
* Product:		Com2Verse
* File Name:	GroundCheckData.cs
* Developer:	eugene9721
* Date:			2023-02-16 10:30
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
	public sealed class GroundCheckData : IMovementData
	{
		[field: SerializeField]
		public float CheckSphereRadius { get; private set; } = 0.1f;

		[field: SerializeField]
		public float CheckSphereOffset { get; private set; } = 0.12f;

		[Tooltip("지면에 있다고 인식하는 거리")]
		[field: SerializeField]
		public float GroundCheckDistance { get; private set; } = 0.1f;

		[field: SerializeField]
		public float MaxDistance { get; private set; } = 100f;

		[field: SerializeField]
		public LayerMask IgnoreLayerMask { get; private set; } = 0;

		public void SetData(AvatarControl data)
		{
			if (data == null)
			{
				C2VDebug.LogErrorCategory(nameof(GroundCheckData), "data is null");
				return;
			}

			CheckSphereRadius   = data.CheckSphereRadius;
			CheckSphereOffset   = data.CheckSphereOffset;
			GroundCheckDistance = data.GroundCheckDistance;
			MaxDistance         = data.MaxDistance;
			IgnoreLayerMask     = data.IgnoreLayerMask;
		}
	}
}
