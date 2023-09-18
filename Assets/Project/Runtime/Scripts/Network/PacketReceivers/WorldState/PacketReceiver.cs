// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	PacketReceiver.cs
//  * Developer:	yangsehoon
//  * Date:		2023-06-15 오후 2:34
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using Protocols.WorldState;

namespace Com2Verse.Network.WorldState
{
	public class PacketReceiver : Singleton<PacketReceiver>, IDisposable
	{
		private PacketReceiver() { }

		public void Dispose()
		{
			DisposeZone();
		}

		public event Action<NearZoneNotify> NearZoneNotify;

		public void RaiseNearZoneNotify(NearZoneNotify notify)
		{
			NearZoneNotify?.Invoke(notify);
		}

		private void DisposeZone()
		{
			NearZoneNotify = null;
		}
	}
}
