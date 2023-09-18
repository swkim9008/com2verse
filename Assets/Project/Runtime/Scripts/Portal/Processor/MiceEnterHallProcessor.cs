/*===============================================================
* Product:		Com2Verse
* File Name:	EnterMiceHallProcessor.cs
* Developer:	ikyoung
* Date:			2023-06-13 18:49
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
using Cysharp.Threading.Tasks;
using Com2Verse.Logger;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.MICE_HALL_ENTER)]
	public sealed class MiceEnterHallProcessor : BaseLogicTypeProcessor
	{
		public override void OnInteraction(TriggerInEventParameter triggerInParameter)
		{
			C2VDebug.LogMethod(GetType().Name);
			
			base.OnInteraction(triggerInParameter);
			MiceService.Instance.ShowUIPopupSessionList(triggerInParameter).Forget();
		}
	}
}
