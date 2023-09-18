/*===============================================================
* Product:		Com2Verse
* File Name:	MiceLectureScreenProcessor.cs
* Developer:	ikyoung
* Date:			2023-06-14 12:50
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
using Com2Verse.UI;
using Com2Verse.Logger;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.MICE_LECTURE_SCREEN)]
	public sealed class MiceLectureScreenProcessor : BaseLogicTypeProcessor
	{
		public override void OnTriggerClick(TriggerEventParameter triggerParameter)
		{
			base.OnTriggerClick(triggerParameter);
		}
	}
}
