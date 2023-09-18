/*===============================================================
* Product:		Com2Verse
* File Name:	EnterMazeProcessor.cs
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
	[LogicType(eLogicType.MAZE_ENTER)]
	public sealed class EnterMazeProcessor : BaseLogicTypeProcessor
	{
		public override void OnTriggerEnter(TriggerInEventParameter triggerInParameter)
		{
			if (PlayContentsManager.Instance.CurrentContentsType == PlayContentsManager.eContentsType.MAZE) return;
			PlayContentsManager.Instance.InitializeContents(PlayContentsManager.eContentsType.MAZE);
			if (PlayContentsManager.Instance.CurrentContents is not MazeController mazeController) return;
			mazeController.SetGates(triggerInParameter);
		}

		public override void OnTriggerExit(TriggerOutEventParameter triggerOutParameter)
		{
			if (PlayContentsManager.Instance.CurrentContents is not MazeController mazeController) return;
			if (!mazeController.IsStartGate(triggerOutParameter.ParentMapObject)) return;
			if (mazeController.PlayState != MazeController.ePlayState.PLAYING)
				PlayContentsManager.Instance.ContentsEnd();
		}
	}
}
