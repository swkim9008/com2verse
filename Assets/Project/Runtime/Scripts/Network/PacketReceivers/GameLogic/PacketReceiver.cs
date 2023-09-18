// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	PacketReceiver.cs
//  * Developer:	yangsehoon
//  * Date:		2023-04-27 오후 1:05
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;

namespace Com2Verse.Network.GameLogic
{
	public partial class PacketReceiver : Singleton<PacketReceiver>, IDisposable
	{
		private PacketReceiver() { }

		public void Dispose()
		{
			DisposeServiceChange();
			DisposeAreaChange();
			DisposeChat();
			DisposeSmallTalkObject();
			DisposeAudience();
			DisposeCommon();
		}
	}
}
