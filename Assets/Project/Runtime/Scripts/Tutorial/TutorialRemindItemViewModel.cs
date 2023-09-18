/*===============================================================
* Product:		Com2Verse
* File Name:	TutorialRemindItemViewModel.cs
* Developer:	ydh
* Date:			2023-08-25 13:59
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using Com2Verse.Tutorial;

namespace Com2Verse.UI
{
	[ViewModelGroup("Tutorial")]
	public sealed class TutorialRemindItemViewModel : ViewModel
	{
		private string _desc;
		public string Desc
		{
			get => _desc;
			set => SetProperty(ref _desc, value);
		}

		public eTutorialGroup TutorialGroup { get; set; }

		public CommandHandler Command_BtnClick { get; }

		public TutorialRemindItemViewModel()
		{
			Command_BtnClick = new CommandHandler(OnCommandBtnClick);
		}

		private void OnCommandBtnClick()
		{
			TutorialManager.Instance.TutorialPlay(TutorialGroup, true);
		}
	}
}
