// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	SmallTalkObjectProcessor.cs
//  * Developer:	yangsehoon
//  * Date:		2023-06-23 오후 12:56
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using Com2Verse.Data;
using Com2Verse.Interaction;
using Com2Verse.Network;
using Com2Verse.SmallTalk.SmallTalkObject;
using Com2Verse.UI;

namespace Com2Verse.EventTrigger
{
	public class SmallTalkObjectProcessor : BaseLogicTypeProcessor
	{
		protected virtual string UserLimit => "0";
		
		public override void OnTriggerEnter(TriggerInEventParameter triggerInParameter)
		{
			if (!SmallTalkObjectManager.Instance.ParameterSourcesMap.TryGetValue(triggerInParameter.ParentMapObject, out var source))
			{
				source = new InteractionStringParameterSource(2);
				source.SetParameter(0, "0");
				SmallTalkObjectManager.Instance.ParameterSourcesMap.Add(triggerInParameter.ParentMapObject, source);
			}
			source.SetParameter(1, UserLimit);
			
			InteractionManager.Instance.SetInteractionUI(triggerInParameter.SourceTrigger, triggerInParameter.CallbackIndex, () => { OnInteraction(triggerInParameter); }, source);
		}
		
		public override void OnInteraction(TriggerInEventParameter triggerInParameter)
		{
			base.OnInteraction(triggerInParameter);
			base.OnTriggerExit(new TriggerOutEventParameter()
			{
				SourceTrigger = triggerInParameter.SourceTrigger,
				CallbackIndex = triggerInParameter.CallbackIndex
			});

			if (SmallTalkObjectManager.Instance.ParameterSourcesMap.TryGetValue(triggerInParameter.ParentMapObject, out var source))
			{
				int currentUserCount = int.Parse(source.GetParameter(0));
				int maxUserCount = InteractionManager.Instance.GetUserCountLimit(LogicType);

				if (currentUserCount < maxUserCount)
				{
					long interactionLinkId = InteractionManager.GetInteractionLinkId(triggerInParameter.ParentMapObject.ObjectTypeId, triggerInParameter.TriggerIndex, triggerInParameter.CallbackIndex);
					Commander.Instance.RequestEnterObjectInteraction(triggerInParameter.ParentMapObject.ObjectID, interactionLinkId);
					SmallTalkObjectManager.Instance.LastRequestedInteractionLink = InteractionManager.Instance.GetInteractionLink(interactionLinkId);
				}
				else
				{
					OnEnterFail(null);
				}
			}
		}

		public override void OnTriggerExit(TriggerOutEventParameter triggerOutParameter)
		{
			base.OnTriggerExit(triggerOutParameter);

			if (SmallTalkObjectManager.Instance.IsConnected)
			{
				SmallTalkObjectManager.Instance.RemoveChannel();
				UIManager.Instance.SendToastMessage(Com2Verse.UI.Localization.Instance.GetString("UI_SmallTalkVoice_Disconnect_Msg"));
			}

			Commander.Instance.RequestExitObjectInteraction(triggerOutParameter.ParentMapObject.ObjectID, InteractionManager.GetInteractionLinkId(triggerOutParameter.ParentMapObject.ObjectTypeId, triggerOutParameter.TriggerIndex,triggerOutParameter.CallbackIndex));
		}
	}

	[LogicType(eLogicType.SMALL_TALK_VOICE2)]
	public class SmallTalkVoice2Processor : SmallTalkObjectProcessor
	{
		private readonly string _userLimit = InteractionManager.Instance.GetUserCountLimit(eLogicType.SMALL_TALK_VOICE2).ToString();
		protected override string UserLimit => _userLimit;
	}

	[LogicType(eLogicType.SMALL_TALK_VOICE3)]
	public class SmallTalkVoice3Processor : SmallTalkObjectProcessor
	{
		private readonly string _userLimit = InteractionManager.Instance.GetUserCountLimit(eLogicType.SMALL_TALK_VOICE3).ToString();
		protected override string UserLimit => _userLimit;
	}

	[LogicType(eLogicType.SMALL_TALK_VOICE4)]
	public class SmallTalkVoice4Processor : SmallTalkObjectProcessor
	{
		private readonly string _userLimit = InteractionManager.Instance.GetUserCountLimit(eLogicType.SMALL_TALK_VOICE4).ToString();
		protected override string UserLimit => _userLimit;
	}
}
