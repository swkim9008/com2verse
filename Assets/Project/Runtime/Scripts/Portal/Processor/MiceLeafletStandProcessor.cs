/*===============================================================
* Product:		Com2Verse
* File Name:	LeafletScreenProcessor.cs
* Developer:	ikyoung
* Date:			2023-06-13 11:46
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com2Verse.Interaction;
using Com2Verse.Data;
using Com2Verse.Mice;
using Com2Verse.Network;
using Com2Verse.UI;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.MICE_LEAFLET_STAND)]
	public sealed class MiceLeafletStandProcessor : BaseLogicTypeProcessor
	{
		public override void OnInteraction(TriggerInEventParameter triggerInParameter)
		{
			base.OnInteraction(triggerInParameter);

			triggerInParameter.ParentMapObject.GetComponent<LeafletScreenObject>()?.StartLeafletStandInteraction(triggerInParameter);
        }
		public override void OnTriggerEnter(TriggerInEventParameter triggerInParameter)
		{
			var monoObj = triggerInParameter.ParentMapObject.GetComponent<LeafletScreenObject>();
			if (monoObj.HasValidTagValue())
			{
				base.OnTriggerEnter(triggerInParameter);	
			}
		}
	}
}
