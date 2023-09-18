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
using Com2Verse.Logger;
using Protocols.GameLogic;

namespace Com2Verse.Network.GameLogic
{
	public partial class PacketReceiver
	{
		public event Action<ObjectInteractionEnterFailNotify> ObjectInteractionEnterFailNotify;

		public void RaiseObjectInteractionEnterFailNotify(ObjectInteractionEnterFailNotify response)
		{
			C2VDebug.LogCategory("ObjectInteraction", $"ObjectInteractionEnterFailNotify {response.InteractionId}");
			ObjectInteractionEnterFailNotify?.Invoke(response);
		}

		private void DisposeCommon()
		{
			ObjectInteractionEnterFailNotify = null;
		}
	}
}
