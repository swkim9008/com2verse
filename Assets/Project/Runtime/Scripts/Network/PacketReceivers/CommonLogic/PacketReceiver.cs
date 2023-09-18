// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	PacketReceiver.cs
//  * Developer:	haminjeong
//  * Date:		2023-07-01 오후 6:05
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;

namespace Com2Verse.Network.CommonLogic
{
	public partial class PacketReceiver : Singleton<PacketReceiver>, IDisposable
	{
		private PacketReceiver() { }

		public void Dispose()
		{
			DisposeServiceChange();
			DisposeConnectQueue();
		}
	}
}
