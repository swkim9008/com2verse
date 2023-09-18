/*===============================================================
* Product:		Com2Verse
* File Name:	BaseLogicTypeProcessor.cs
* Developer:	tlghks1009
* Date:			2022-09-28 13:12
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Data;
using Com2Verse.Interaction;
using Com2Verse.Tutorial;
using Com2Verse.UI;
using Cysharp.Text;
using Protocols.GameLogic;
using Localization = Com2Verse.UI.Localization;

namespace Com2Verse.EventTrigger
{
	[AttributeUsage(AttributeTargets.Class)]
	public class LogicTypeAttribute : Attribute
	{
		public eLogicType LogicType { get; }

		public LogicTypeAttribute(eLogicType logicType)
		{
			LogicType = logicType;
		}
	}

	public abstract class BaseLogicTypeProcessor
	{
		public eLogicType LogicType { get; set; }
		public eActionType ActionType { get; set; }
		public string SoundEffectPath { get; set; }
		public bool ShowInteractionUI => ActionType == eActionType.ICON_ACTION;

		public virtual void OnTriggerClick(TriggerEventParameter triggerParameter)
		{
			
		}
		
		public virtual void OnTriggerEnter(TriggerInEventParameter triggerInParameter)
		{
			if (ShowInteractionUI)
			{
				if (triggerInParameter.SourcePacket == null)
				{
					InteractionManager.Instance.SetInteractionUI(triggerInParameter.SourceTrigger, triggerInParameter.CallbackIndex, () => { OnInteraction(triggerInParameter); });
				}
			}
		}

		public virtual void OnZoneEnter(ServerZone zone, int callbackIndex) { }
		public virtual void OnZoneExit(ServerZone zone, int callbackIndex) { }

		public virtual void OnInteraction(TriggerInEventParameter triggerInParameter)
		{
			TutorialManager.Instance.TutorialObjectInteractionCheck(triggerInParameter);
		}

		public virtual void OnTriggerExit(TriggerOutEventParameter triggerOutParameter)
		{
			if (triggerOutParameter.SourcePacket == null)
			{
				InteractionManager.Instance.UnsetInteractionUI(triggerOutParameter.SourceTrigger, triggerOutParameter.CallbackIndex);
			}
		}

		public virtual void OnEnterFail(ObjectInteractionEnterFailNotify failNotify)
		{
			UIManager.Instance.SendToastMessage(
				ZString.Format(Localization.Instance.GetString("UI_Interaction_Zone_NotAvailable_Toast"), Localization.Instance.GetString(InteractionManager.Instance.GetInteractionNameKey(LogicType))), 3f,
				UIManager.eToastMessageType.WARNING);
		}
	}
}
