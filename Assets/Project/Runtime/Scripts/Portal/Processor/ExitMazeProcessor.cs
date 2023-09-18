/*===============================================================
* Product:		Com2Verse
* File Name:	ExitMazeProcessor.cs
* Developer:	haminjeong
* Date:			2023-05-25 17:32
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Contents;
using Com2Verse.Data;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.MAZE_EXIT)]
	public sealed class ExitMazeProcessor : BaseLogicTypeProcessor
	{
		public override void OnTriggerEnter(TriggerInEventParameter triggerInParameter)
		{
			if (PlayContentsManager.Instance.CurrentContents is not MazeController mazeController) return;
			if (!mazeController.IsGoalGate(triggerInParameter.ParentMapObject)) return;
			if (mazeController.PlayState == MazeController.ePlayState.PLAYING)
				mazeController.PlayEnd();
		}

		public override void OnTriggerExit(TriggerOutEventParameter triggerOutParameter)
		{
			if (PlayContentsManager.Instance.CurrentContents is not MazeController mazeController) return;
			if (!mazeController.IsGoalGate(triggerOutParameter.ParentMapObject)) return;
			if (mazeController.PlayState == MazeController.ePlayState.VALID_CHECK)
				mazeController.RestorePlaying();
			else if (mazeController.PlayState != MazeController.ePlayState.RESULT)
				PlayContentsManager.Instance.ContentsEnd();
		}
	}
}
