// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	PacketReceiver_EventTrigger.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-22 오전 10:35
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using Com2Verse.EventTrigger;
using Com2Verse.Logger;
using Protocols.GameMechanic;

namespace Com2Verse.Network.GameMechanic
{
	public partial class PacketReceiver
	{
		public event Action<CheckCollisionRequest> CheckCollisionRequest;
		
		public void RaiseCollisionResponse(CheckCollisionRequest response)
		{
			CheckCollisionRequest?.Invoke(response);
		}

		private void DisposeEventTrigger()
		{
			CheckCollisionRequest = null;
		}
	}
}
