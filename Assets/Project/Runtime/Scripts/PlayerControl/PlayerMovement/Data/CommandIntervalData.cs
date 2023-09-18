/*===============================================================
* Product:		Com2Verse
* File Name:	CommandIntervalData.cs
* Developer:	eugene9721
* Date:			2023-02-03 12:33
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;

namespace Com2Verse.PlayerControl
{
	[Serializable]
	public sealed class CommandIntervalData : IMovementData
	{
		[field: SerializeField]
		public float MoveToInterval { get; private set; } = 0.7f;

		[field: SerializeField]
		public float MoveCommandInterval { get; private set; } = 0.1f;

		[field: SerializeField]
		public float SprintInterval { get; private set; } = 0.2f;
	}
}
