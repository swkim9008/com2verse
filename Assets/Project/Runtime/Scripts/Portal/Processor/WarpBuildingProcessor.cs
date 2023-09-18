// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	WarpBuildingProcessor.cs
//  * Developer:	yangsehoon
//  * Date:		2023-05-12 오후 4:16
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using Com2Verse.Data;
using Com2Verse.Interaction;
using Com2Verse.Network;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.WARP__BUILDING)]
	public class WarpBuildingProcessor : TeleportLogicProcessor
	{
		public override void OnInteraction(TriggerInEventParameter triggerInParameter)
		{
			base.OnInteraction(triggerInParameter);

			string buildingIdStr = InteractionManager.Instance.GetInteractionValue(triggerInParameter.ParentMapObject.InteractionValues, triggerInParameter.TriggerIndex,
			                                                                       triggerInParameter.CallbackIndex, 0);
			if (!long.TryParse(buildingIdStr, out var buildingId))
				return;

			PreventUserInput();

			Commander.Instance.RequestServiceChange(buildingId);
		}
	}
}
