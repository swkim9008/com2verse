// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	Commander_SmallTalk.cs
//  * Developer:	yangsehoon
//  * Date:		2023-06-28 오전 10:13
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using Com2Verse.Logger;
using Protocols.GameLogic;

namespace Com2Verse.Network
{
	public partial class Commander
	{
		public void RequestEnterObjectInteraction(long objectId, long interactionLinkId)
		{
			EnterObjectInteractionRequest request = new()
			{
				ObjectId = objectId,
				InteractionLinkId = interactionLinkId
			};
			C2VDebug.LogCategory("ObjectInteraction", "RequestEnterObjectInteraction");
			NetworkManager.Instance.Send(request, MessageTypes.EnterObjectInteractionRequest);
		}

		public void RequestExitObjectInteraction(long objectId, long interactionLinkId)
		{
			ExitObjectInteractionRequest request = new()
			{
				ObjectId = objectId,
				InteractionLinkId = interactionLinkId
			};
			C2VDebug.LogCategory("ObjectInteraction", "RequestExitObjectInteraction");
			NetworkManager.Instance.Send(request, MessageTypes.ExitObjectInteractionRequest);
		}
	}
}
