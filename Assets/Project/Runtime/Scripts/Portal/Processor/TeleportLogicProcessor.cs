// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	TeleportLogicProcessor.cs
//  * Developer:	yangsehoon
//  * Date:		2023-05-15 오전 11:26
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using Com2Verse.Network;
using Com2Verse.PlayerControl;

namespace Com2Verse.EventTrigger
{
	public abstract class TeleportLogicProcessor : BaseLogicTypeProcessor
	{
		protected void PreventUserInput()
		{
			// 이동중 OSR/Input 막기
			PlayerController.Instance.SetStopAndCannotMove(true);
			User.Instance.DiscardPacketBeforeStandBy();
		}

		public override void OnInteraction(TriggerInEventParameter triggerInParameter)
		{
			base.OnInteraction(triggerInParameter);
			
			base.OnTriggerExit(new TriggerOutEventParameter()
			{
				CallbackIndex = triggerInParameter.CallbackIndex,
				SourceTrigger = triggerInParameter.SourceTrigger,
				TriggerIndex = triggerInParameter.TriggerIndex
			});
		}
	}
}
