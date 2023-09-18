// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	EnterOfiiceProcessor.cs
//  * Developer:	yangsehoon
//  * Date:		2023-04-27 오전 11:01
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using Com2Verse.Data;
using Com2Verse.Interaction;
using Com2Verse.Network;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.OFFICE_ENTER)]
	public class EnterOfficeProcessor : BaseLogicTypeProcessor
	{
		public override void OnInteraction(TriggerInEventParameter triggerInParameter)
		{
			base.OnInteraction(triggerInParameter);
			string buildingId = InteractionManager.Instance.GetInteractionValue(triggerInParameter.ParentMapObject.InteractionValues, triggerInParameter.TriggerIndex, triggerInParameter.CallbackIndex, 0);
			Commander.Instance.RequestServiceChange(long.Parse(buildingId));
		}
	}
}
