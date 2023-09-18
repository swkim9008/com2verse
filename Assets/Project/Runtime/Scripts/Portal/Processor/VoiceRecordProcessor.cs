/*===============================================================
* Product:		Com2Verse
* File Name:	VoiceRecordProcessor.cs
* Developer:	jhkim
* Date:			2023-05-19 12:28
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.AudioRecord;
using Com2Verse.Data;
using Com2Verse.Interaction;
using Com2Verse.Office;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.VOICE_RECORD)]
	public sealed class VoiceRecordProcessor : BaseLogicTypeProcessor
	{
		public override void OnTriggerEnter(TriggerInEventParameter triggerInParameter)
		{
			if (OfficeService.Instance.IsModelHouse) return;

			base.OnTriggerEnter(triggerInParameter);
		}

		public override void OnInteraction(TriggerInEventParameter triggerInParameter)
		{
			base.OnInteraction(triggerInParameter);
			// TODO : 음성 녹음 UI 열기

			var audioRecordId = InteractionManager.Instance.GetInteractionValue(triggerInParameter.ParentMapObject.InteractionValues, triggerInParameter.TriggerIndex, triggerInParameter.CallbackIndex, 0);
			if (string.IsNullOrEmpty(audioRecordId))
				return;
				
			AudioRecordManager.Instance.OpenAudioRecord(audioRecordId);
		}
	}
}