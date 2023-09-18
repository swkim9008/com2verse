/*===============================================================
* Product:		Com2Verse
* File Name:	SpeechMicProcessor.cs
* Developer:	haminjeong
* Date:			2023-06-02 16:21
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Communication;
using Com2Verse.Data;
using Com2Verse.Interaction;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.PhysicsAssetSerialization;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.SPEECH)]
	public sealed class SpeechMicProcessor : BaseLogicTypeProcessor
	{
		private static readonly string PopupSoundQualityType = "UI_Popup_SoundQualityType";

		private bool IsInTrigger(C2VEventTrigger trigger) => TriggerEventManager.Instance.IsInTrigger(trigger, 0);
		
		public override void OnTriggerEnter(TriggerInEventParameter triggerInParameter)
		{
			if (!AuditoriumController.Instance.ParameterSourcesMap!.TryGetValue(triggerInParameter.ParentMapObject, out var source))
			{
				source = new InteractionStringParameterSource(2);
				source.SetParameter(0, "0");
				AuditoriumController.Instance.ParameterSourcesMap.Add(triggerInParameter.ParentMapObject, source);
			}

			source!.SetParameter(1, InteractionManager.Instance.GetUserCountLimit(LogicType).ToString());

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

			if (AuditoriumController.Instance.ParameterSourcesMap!.TryGetValue(triggerInParameter.ParentMapObject, out var source))
			{
				int currentUserCount = int.Parse(source!.GetParameter(0));
				int maxUserCount     = InteractionManager.Instance.GetUserCountLimit(LogicType);
			
				if (currentUserCount < maxUserCount)
				{
					UIManager.Instance.CreatePopup(PopupSoundQualityType, (view) =>
					{
						var trigger = triggerInParameter.ParentMapObject.GetComponentInChildren<C2VEventTrigger>();
						if (!IsInTrigger(trigger))
						{
							C2VDebug.LogWarningCategory("Auditorium", "Out of Trigger");
							view.Hide();
							return;
						}
						view.Show();
						var viewModel = view.ViewModelContainer.GetViewModel<VoiceQualitySelectViewModel>();
						viewModel.InitVariables();
						viewModel.OnConfirmAction = () =>
						{
							if (!IsInTrigger(trigger))
							{
								C2VDebug.LogWarningCategory("Auditorium", "Out of Trigger");
								view.Hide();
								return;
							}
							AuditoriumController.Instance.CurrentMicTrigger = trigger;
							AuditoriumController.Instance.IsHighQuality     = viewModel.UseVoiceRecordingQuality == (int)eSoundQualityType.HIGH_QUALITY;
							Commander.Instance.RequestEnterObjectInteraction(triggerInParameter.ParentMapObject.ObjectID,
							                                                 InteractionManager.GetInteractionLinkId(triggerInParameter.ParentMapObject.ObjectTypeId, triggerInParameter.TriggerIndex,
							                                                                                         triggerInParameter.CallbackIndex));
							view.Hide();
						};
					}).Forget();
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
			if (AuditoriumController.Instance.CurrentGroupChannel is { IsSpeech: true })
				AuditoriumController.Instance.LeaveChannel();

			Commander.Instance.RequestExitObjectInteraction(triggerOutParameter.ParentMapObject.ObjectID,
			                                                InteractionManager.GetInteractionLinkId(triggerOutParameter.ParentMapObject.ObjectTypeId, triggerOutParameter.TriggerIndex,
			                                                                                        triggerOutParameter.CallbackIndex));
		}
	}
}
