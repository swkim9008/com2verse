// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	PacketReceiver_SmallTalkObject.cs
//  * Developer:	yangsehoon
//  * Date:		2023-06-28 오전 10:33
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
		public event Action<EnterObjectInteractionSmallTalkNotify> EnterObjectInteractionSmalltalkNotify;

		public void RaiseObjectSmallTalkNotify(EnterObjectInteractionSmallTalkNotify response)
		{
			EnterObjectInteractionSmalltalkNotify?.Invoke(response);
		}

		private void DisposeSmallTalkObject()
		{
			EnterObjectInteractionSmalltalkNotify = null;
		}
	}
}
