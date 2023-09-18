// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	PacketReceiver_ServiceChange.cs
//  * Developer:	yangsehoon
//  * Date:		2023-04-27 오후 1:06
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using Protocols.GameLogic;

namespace Com2Verse.Network.GameLogic
{
	public partial class PacketReceiver
	{
		public event Action<ServiceChangeResponse> ServiceChangeResponse;
		public event Action<LeaveBuildingResponse> LeaveBuildingResponse;

		public void RaiseServiceChangeResponse(ServiceChangeResponse response)
		{
			ServiceChangeResponse?.Invoke(response);
		}

		public void RaiseLeaveBuildingResponse(LeaveBuildingResponse response)
		{
			LeaveBuildingResponse?.Invoke(response);
		}

		private void DisposeServiceChange()
		{
			ServiceChangeResponse = null;
			LeaveBuildingResponse = null;
		}
	}
}
