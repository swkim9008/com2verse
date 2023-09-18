// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	TextBoardProcessor.cs
//  * Developer:	yangsehoon
//  * Date:		2023-05-26 오후 2:52
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using Com2Verse.Data;
using Com2Verse.UI;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.TEXT_BOARD)]
	public class TextBoardProcessor : BaseLogicTypeProcessor
	{
		public override void OnInteraction(TriggerInEventParameter triggerInParameter)
		{
			base.OnInteraction(triggerInParameter);

			var textBoard = triggerInParameter.SourceTrigger.transform.GetComponentInParent<TextBoard>();
			textBoard.OpenTextBoard();
		}
	}
}
