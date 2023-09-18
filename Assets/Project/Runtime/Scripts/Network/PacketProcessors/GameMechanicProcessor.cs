/*===============================================================
* Product:		Com2Verse
* File Name:	GameMechanicProcessor.cs
* Developer:	haminjeong
* Date:			2022-05-16 12:23
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using JetBrains.Annotations;
using Protocols.GameMechanic;

namespace Com2Verse.Network
{
	[UsedImplicitly]
	[Channel(Protocols.Channels.GameMechanic)]
	public sealed class GameMechanicProcessor : BaseMessageProcessor
	{
		public override void Initialize()
		{
			SetMessageProcessCallback((int)MessageTypes.CheckCollisionRequest,
			                          static payload => CheckCollisionRequest.Parser?.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is CheckCollisionRequest response)
				                          {
					                          GameMechanic.PacketReceiver.Instance.RaiseCollisionResponse(response);
				                          }
			                          });
		}
	}
}
