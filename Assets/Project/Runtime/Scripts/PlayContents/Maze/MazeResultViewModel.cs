/*===============================================================
* Product:		Com2Verse
* File Name:	MazeResultViewModel.cs
* Developer:	haminjeong
* Date:			2023-06-12 15:00
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;

namespace Com2Verse.UI
{
	[ViewModelGroup("PlayContents")]
	public class MazeResultViewModel : ViewModelBase
	{
		public GUIView CurrentView { private get; set; }

		private string _recordTimeString;
		public string RecordTimeString
		{
			get => _recordTimeString;
			set => SetProperty(ref _recordTimeString, value);
		}

		private string _bestTimeString;
		public string BestTimeString
		{
			get => _bestTimeString;
			set => SetProperty(ref _bestTimeString, value);
		}

		private bool _isNewRecord;
		public bool IsNewRecord
		{
			get => _isNewRecord;
			set => SetProperty(ref _isNewRecord, value);
		}

		private string _rankingText;
		public string RankingText
		{
			get => _rankingText;
			set => SetProperty(ref _rankingText, value);
		}

		public CommandHandler CloseButtonCommandHandler { get; }

		public MazeResultViewModel()
		{
			CloseButtonCommandHandler = new CommandHandler(OnCloseButton);
		}

		private void OnCloseButton()
		{
			if (!CurrentView.IsUnityNull())
				CurrentView!.Hide();
		}
	}
}
