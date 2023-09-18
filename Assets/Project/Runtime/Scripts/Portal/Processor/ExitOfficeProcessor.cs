// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	ExitOfficeProcessor.cs
//  * Developer:	yangsehoon
//  * Date:		2023-04-27 오전 11:02
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using Com2Verse.Data;
using Com2Verse.Network;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.OFFICE_EXIT)]
	public class ExitOfficeProcessor : TeleportLogicProcessor
	{
		public override void OnInteraction(TriggerInEventParameter triggerInParameter)
		{
			base.OnInteraction(triggerInParameter);

			PreventUserInput();
			Commander.Instance.LeaveBuildingRequest();
		}
	}
}
