/*===============================================================
* Product:		Com2Verse
* File Name:	MetaverseOptionViewModel.cs
* Developer:	tlghks1009
* Date:			2022-10-04 15:58
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Option;

namespace Com2Verse.UI
{
	[ViewModelGroup("Option")]
	public partial class MetaverseOptionViewModel : ViewModelBase, IDisposable
	{
		private StackRegisterer _guiViewRegisterer;
		private bool _scrollRectEnable = false;
		public CommandHandler Command_CloseButtonClick { get; }

		public StackRegisterer GuiViewRegisterer
		{
			get => _guiViewRegisterer;
			set
			{
				_guiViewRegisterer = value;
				_guiViewRegisterer.WantsToQuit += CloseView;
			}
		}
		
		public float ResetScroll
		{
			get => 0;
			set { }
		}

		public bool ScrollRectEnable
		{
			get => _scrollRectEnable;
			set => SetProperty(ref _scrollRectEnable, value);
		}

		public MetaverseOptionViewModel()
		{
			Command_CloseButtonClick = new CommandHandler(OnCommand_CloseButtonClicked);

			ResetVolumeSettingsCommand = new CommandHandler(ResetVolumeSettings);
			ResetChatSettingsCommand = new CommandHandler(ResetChatSettings);

			InitializeVolumeOption();
			InitializeGraphicsOption();
			InitializeChatOption();
			// InitializeOrganizationAsync().Forget();
			InitializeLanguage();
			InitializeControlOption();
			InitializeAccountOption();
		}

		public override void OnInitialize()
		{
			base.OnInitialize();
			base.InvokePropertyValueChanged(nameof(ResetScroll), ResetScroll);
		}
		
		public override void OnLanguageChanged()
		{
			base.OnLanguageChanged();
			LanguageChangeGraphicOption();
			LanguageChangeLanguageOption();
		}

		private void OnCommand_CloseButtonClicked()
		{
			OptionController.Instance.ApplyAll();
			OptionController.Instance.SaveAll();
		}

		private void CloseView()
		{
			OnCommand_CloseButtonClicked();
			_guiViewRegisterer.HideComplete();
		}

		public void Dispose()
		{
			DisposeGraphicOption();
		}
	}
}
