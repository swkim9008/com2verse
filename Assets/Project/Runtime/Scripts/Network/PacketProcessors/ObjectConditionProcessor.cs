/*===============================================================
* Product:		Com2Verse
* File Name:	ObjectConditionProcessor.cs
* Developer:	haminjeong
* Date:			2022-06-09 13:40
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using JetBrains.Annotations;

namespace Com2Verse.Network
{
	[UsedImplicitly]
	[Channel(Protocols.Channels.ObjectCondition)]
	public sealed class ObjectConditionProcessor : BaseMessageProcessor
	{
		public override void Initialize() { }
	}
}
