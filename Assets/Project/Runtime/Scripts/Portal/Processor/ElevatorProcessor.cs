/*===============================================================
* Product:		Com2Verse
* File Name:	ElevatorProcessor.cs
* Developer:	tlghks1009
* Date:			2022-09-28 13:12
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using Com2Verse.Interaction;
using Com2Verse.Network;
using Com2Verse.Office;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.ELEVATOR)]
	public sealed class ElevatorProcessor : TeleportLogicProcessor
	{
		public override void OnInteraction(TriggerInEventParameter triggerInParameter)
		{
			base.OnInteraction(triggerInParameter);
			string spaceId = InteractionManager.Instance.GetInteractionValue(triggerInParameter.ParentMapObject.InteractionValues, triggerInParameter.TriggerIndex,
			                                                                 triggerInParameter.CallbackIndex, 0);

			if (!string.IsNullOrWhiteSpace(spaceId) && OfficeService.Instance.IsModelHouse)
			{
				PreventUserInput();
				Commander.Instance.TeleportUserSpaceRequest(spaceId);
			}
			else
			{
				WarpSpaceViewModel.ShowAsync().Forget();
			}
		}
	}
}
