// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	PacketReceiver.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-23 오전 11:23
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;

namespace Com2Verse.Network.GameMechanic
{
	public partial class PacketReceiver : Singleton<PacketReceiver>, IDisposable
	{
		private PacketReceiver() { }
		
		public void Dispose()
		{
			DisposeEventTrigger();
		}
	}
}
