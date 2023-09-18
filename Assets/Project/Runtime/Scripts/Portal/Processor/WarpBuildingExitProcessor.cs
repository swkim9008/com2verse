/*===============================================================
* Product:		Com2Verse
* File Name:	WarpBuildingExitProcessor.cs
* Developer:	jhkim
* Date:			2023-05-19 17:28
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using Com2Verse.Network;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.WARP__BUILDING__EXIT)]
	public sealed class WarpBuildingExitProcessor : TeleportLogicProcessor
	{
		public override void OnInteraction(TriggerInEventParameter triggerInParameter)
		{
			base.OnInteraction(triggerInParameter);

			PreventUserInput();
			Commander.Instance.LeaveBuildingRequest();
		}
	}
}
