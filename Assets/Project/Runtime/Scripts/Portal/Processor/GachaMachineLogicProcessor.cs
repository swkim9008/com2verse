/*===============================================================
* Product:		Com2Verse
* File Name:	GachaMachineLogicProcessor.cs
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
using Cysharp.Threading.Tasks;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.GACHA)]
	public sealed class GachaMachineLogicProcessor : BaseLogicTypeProcessor
	{
		public override void OnInteraction(TriggerInEventParameter triggerInParameter)
		{
			base.OnInteraction(triggerInParameter);

			var go = triggerInParameter.ParentMapObject.GetComponent<GachaMachineObject>();
			
			if(go != null)
			{
				go.StartInteraction(triggerInParameter).Forget();
            }
		}
	}
}
