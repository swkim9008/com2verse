/*===============================================================
* Product:		Com2Verse
* File Name:	VideoLogicProcessor.cs
* Developer:	haminjeong
* Date:			2023-05-21 00:13
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Network;
using Com2Verse.Office;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.SCREEN_AD)]
	public sealed class VideoLogicProcessor : BaseLogicTypeProcessor
	{
		public override void OnTriggerEnter(TriggerInEventParameter triggerInParameter)
		{
			if (OfficeService.Instance.IsModelHouse) return;
			
			base.OnTriggerEnter(triggerInParameter);

			var adsObject = triggerInParameter.SourceTrigger.transform.GetComponentInParent<AdsObject>();
			if (adsObject.IsUnityNull()) return;
			adsObject!.SetPlaying(true);
		}

		public override void OnTriggerExit(TriggerOutEventParameter triggerOutParameter)
		{
			if (OfficeService.Instance.IsModelHouse) return;
			
			base.OnTriggerExit(triggerOutParameter);

			var adsObject = triggerOutParameter.SourceTrigger.transform.GetComponentInParent<AdsObject>();
			if (adsObject.IsUnityNull()) return;
			adsObject!.SetPlaying(false);
		}
	}
}
