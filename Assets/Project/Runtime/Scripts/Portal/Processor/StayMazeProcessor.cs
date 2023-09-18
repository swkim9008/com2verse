/*===============================================================
* Product:		Com2Verse
* File Name:	StayMazeProcessor.cs
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
	[LogicType(eLogicType.MAZE_STAY)]
	public sealed class StayMazeProcessor : BaseLogicTypeProcessor
	{
		public override void OnZoneEnter(ServerZone zone, int callbackIndex)
		{
			if (PlayContentsManager.Instance.CurrentContents is not MazeController mazeController) return;
			if (mazeController.PlayState == MazeController.ePlayState.READY)
				mazeController.PlayStart();
			else if (mazeController.PlayState == MazeController.ePlayState.PAUSE)
				mazeController.SetPause(false);
		}

		public override void OnZoneExit(ServerZone zone, int callbackIndex)
		{
			if (PlayContentsManager.Instance.CurrentContents is not MazeController mazeController) return;
			if (mazeController.PlayState == MazeController.ePlayState.VALID_CHECK)
				mazeController.GoToResult();
			else
				mazeController.SetPause(true);
		}
	}
}
