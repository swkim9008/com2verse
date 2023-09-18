/*===============================================================
* Product:		Com2Verse
* File Name:	TutorialRemindViewModel.cs
* Developer:	ydh
* Date:			2023-08-25 13:59
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;

namespace Com2Verse.UI
{
	[ViewModelGroup("Tutorial")]
	public sealed class TutorialRemindViewModel : ViewModelBase, IDisposable
	{
		private Collection<TutorialRemindItemViewModel> _tutorialRemindItemCollection;
		public Collection<TutorialRemindItemViewModel> TutorialRemindItemCollection
		{
			get => _tutorialRemindItemCollection;
		}

		public CommandHandler Command_ExitBtnClick { get; }
		public Action ExitAction { get; set; }
		public Action ChangeLanguage { get; set; }

		public TutorialRemindViewModel()
		{
			Command_ExitBtnClick = new CommandHandler(OnCommandExitBtnClick);

			_tutorialRemindItemCollection ??= new();
		}

		private void OnCommandExitBtnClick()
		{
			ExitAction?.Invoke();
		}

		public void Dispose()
		{
			_tutorialRemindItemCollection.DestroyAll();
			_tutorialRemindItemCollection = null;
		}
		
		public override void OnLanguageChanged()
		{
			base.OnLanguageChanged();
			ChangeLanguage?.Invoke();
		}
	}
}