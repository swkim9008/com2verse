using Com2Verse.Data;
using Com2Verse.EventListen;
using Com2Verse.EventTrigger;
using Com2Verse.Network;
using Protocols.GameLogic;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.BOARD__AI)]
	public class BoardAIReadProcessor : BaseLogicTypeProcessor
	{
		public override void OnInteraction(TriggerInEventParameter triggerInParameter)
		{
			base.OnInteraction(triggerInParameter);
			Commander.Instance.RequestAIBotString();
		}

		public override void OnTriggerEnter(TriggerInEventParameter triggerInParameter)
		{
			base.OnTriggerEnter(triggerInParameter);
		}
	}
}
