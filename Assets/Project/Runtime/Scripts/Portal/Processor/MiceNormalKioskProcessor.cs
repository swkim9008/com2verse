/*===============================================================
* Product:		Com2Verse
* File Name:	KioskNormalProcessor.cs
* Developer:	ikyoung
* Date:			2023-06-13 18:51
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com2Verse.Data;
using Com2Verse.Mice;
using Com2Verse.Network;
using Cysharp.Threading.Tasks;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.MICE_KIOSK)]
	public sealed class MiceNormalKioskProcessor : BaseLogicTypeProcessor
	{
		public override void OnInteraction(TriggerInEventParameter triggerInParameter)
		{
			base.OnInteraction(triggerInParameter);
			var kiosk = triggerInParameter.ParentMapObject.GetComponent<KioskObject>();
			MiceService.Instance.ShowKioskMenu(kiosk).Forget();
		}

		public override void OnTriggerEnter(TriggerInEventParameter triggerInParameter)
		{
			var kiosk = triggerInParameter.ParentMapObject.GetComponent<KioskObject>();
			if (kiosk.HasValidTagValue())
			{
				base.OnTriggerEnter(triggerInParameter);	
			}
		}
	}
}
