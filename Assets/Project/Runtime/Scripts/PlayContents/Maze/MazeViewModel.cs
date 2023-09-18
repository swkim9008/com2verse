/*===============================================================
* Product:		Com2Verse
* File Name:	MazeViewModel.cs
* Developer:	haminjeong
* Date:			2023-05-26 15:08
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Network;

namespace Com2Verse.UI
{
	[ViewModelGroup("PlayContents")]
	public class MazeViewModel : ViewModelBase
	{
		public CommandHandler GiveUpButtonCommandHandler { get; }

		public MazeViewModel()
		{
			GiveUpButtonCommandHandler = new CommandHandler(OnGiveUpButton);
		}

		private void OnGiveUpButton()
		{
			UIManager.Instance.ShowPopupYesNo(Localization.Instance.GetString("UI_Maze_Exit_Popup_Title"),
			                                  Localization.Instance.GetString("UI_Maze_Exit_Popup_Desc"),
			                                  _ => Commander.Instance.EscapeMaze(),
			                                  allowCloseArea: false);
		}
	}
}
