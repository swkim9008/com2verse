/*===============================================================
* Product:		Com2Verse
* File Name:	ChairLogicTypeProcessor.cs
* Developer:	tlghks1009
* Date:			2022-09-29 10:10
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Interaction;
using Com2Verse.Network;
using Com2Verse.PlayerControl;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.CHAIR)]
	public sealed class ChairProcessor : BaseLogicTypeProcessor
	{
		public override void OnInteraction(TriggerInEventParameter triggerInParameter)
		{
			base.OnInteraction(triggerInParameter);

			PlayerController.Instance.SetNavigationMode(false);
			Commander.Instance.ChairCommand(triggerInParameter.ParentMapObject.ObjectID, User.Instance.CurrentUserData.ObjectID);

			// Hide interaction ui
			base.OnTriggerExit(new TriggerOutEventParameter()
			{
				SourceTrigger = triggerInParameter.SourceTrigger,
				TriggerIndex = triggerInParameter.TriggerIndex
			});
		}

		public override void OnTriggerExit(TriggerOutEventParameter triggerOutParameter)
		{
			base.OnTriggerExit(triggerOutParameter);
			if (!User.Instance.CharacterObject.IsUnityNull() && User.Instance.CharacterObject!.CharacterState == (int)Protocols.CharacterState.Sit)
				SeatManager.Instance.StandUp();
		}
	}
}
