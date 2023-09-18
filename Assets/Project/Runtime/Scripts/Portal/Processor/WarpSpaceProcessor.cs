/*===============================================================
* Product:		Com2Verse
* File Name:	WarpSpaceProcessor.cs
* Developer:	jhkim
* Date:			2023-05-22 11:37
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using Com2Verse.Interaction;
using Com2Verse.Logger;
using Com2Verse.Network;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.WARP__SPACE)]
	public sealed class WarpSpaceProcessor : TeleportLogicProcessor
	{
		public override void OnInteraction(TriggerInEventParameter triggerInParameter)
		{
			base.OnInteraction(triggerInParameter);
			var spaceId = InteractionManager.Instance.GetInteractionValue(triggerInParameter.ParentMapObject.InteractionValues, triggerInParameter.TriggerIndex, triggerInParameter.CallbackIndex,
			                                                              0);
			if (string.IsNullOrWhiteSpace(spaceId)) return;

			PreventUserInput();
			Commander.Instance.TeleportUserSpaceRequest(spaceId);
		}
	}
}
