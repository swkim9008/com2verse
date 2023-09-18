/*===============================================================
* Product:		Com2Verse
* File Name:	UIInfo.cs
* Developer:	jhkim
* Date:			2022-12-12 13:57
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.UI
{
	public sealed class UIInfo : ViewModelBase
	{
		// !!! 새로운 항목은 마지막에 추가, 순서변경 X !!!
		// https://jira.com2us.com/wiki/pages/viewpage.action?pageId=319012712
		// https://jira.com2us.com/wiki/pages/viewpage.action?pageId=316073740
		public enum eInfoType
		{
			NONE,
			INFO_TYPE_MY_DESK,
			INFO_TYPE_BOARD_WRITE,
			INFO_TYPE_MEETING_INFO,
			INFO_TYPE_PARTY_TALK_REQUEST,
			INFO_TYPE_PARTY_TALK_LIST,
			INFO_TYPE_SCREEN_SHARING,
			INFO_TYPE_ACCESS_INFO,
			INFO_TYPE_RESERVATION_UNIT_INFO,
			INFO_TYPE_RESERVATION_CONFIRM_INFO,
			INFO_TYPE_MICE_PROGRAM_TYPE,
		}

		public enum eInfoLayout
		{
			NONE,
			INFO_LAYOUT_UP,   // 아이콘 위로 붙음
			INFO_LAYOUT_DOWN, // 아이콘 아래로 붙음
		}
#region Variables
		private static UIInfo _viewModel;

		private eInfoType _infoType;
		private eInfoLayout _infoLayout;
		private string _title;
		private string _message;
		private bool _isVisible;
		private bool _playOpenAnimation;
		private bool _playCloseAnimation;
		public CommandHandler Confirm { get; private set; }
		public CommandHandler OpenInfoPopup { get; }
		public CommandHandler CloseInfoPopup { get; }
#endregion // Variables

#region Properties
		public eInfoType InfoType
		{
			get => _infoType;
			set
			{
				_infoType = value;
				InvokePropertyValueChanged(nameof(InfoType), value);
			}
		}

		public eInfoLayout InfoLayout
		{
			get => _infoLayout;
			set
			{
				_infoLayout = value;
				InvokePropertyValueChanged(nameof(InfoLayout), value);
			}
		}

		public string Title
		{
			get => _title;
			set
			{
				_title = value;
				InvokePropertyValueChanged(nameof(Title), value);
			}
		}

		public string Message
		{
			get => _message;
			set
			{
				_message = value;
				InvokePropertyValueChanged(nameof(Message), value);
			}
		}

		public bool IsVisiblePopup
		{
			get => _isVisible;
			set
			{
				_isVisible = value;
				base.InvokePropertyValueChanged(nameof(IsVisiblePopup), value);

				PlayVisibleAnimation();
			}
		}

		public bool PlayOpenAnimation
		{
			get => _playOpenAnimation;
			set
			{
				_playOpenAnimation = value;
				if (_playOpenAnimation)
					base.InvokePropertyValueChanged(nameof(PlayOpenAnimation), value);
			}
		}

		public bool PlayCloseAnimation
		{
			get => _playCloseAnimation;
			set
			{
				_playCloseAnimation = value;
				if (_playCloseAnimation)
					base.InvokePropertyValueChanged(nameof(PlayCloseAnimation), value);
			}
		}
#endregion // Properties

#region Initialize
		public UIInfo()
		{
			_viewModel = this;

			Confirm = new CommandHandler(OnConfirm, null);
			OpenInfoPopup = new CommandHandler(OnOpenInfoPopup);
			CloseInfoPopup = new CommandHandler(OnCloseInfoPopup);
		}

		public UIInfo(bool withoutViewModel)
		{
			if (!withoutViewModel)
				_viewModel = this;

			Confirm = new CommandHandler(OnConfirm, null);
			OpenInfoPopup = new CommandHandler(OnOpenInfoPopup);
			CloseInfoPopup = new CommandHandler(OnCloseInfoPopup);
		}
#endregion // Initialize

#region Binding Events
		private void OnConfirm() => ClearInfo();

		private void OnOpenInfoPopup()
		{
			IsVisiblePopup = true;

			base.InvokePropertyValueChanged(nameof(InfoLayout), InfoLayout);
			base.InvokePropertyValueChanged(nameof(Message), Message);
			base.InvokePropertyValueChanged(nameof(Title), Title);
		}

		private void OnCloseInfoPopup()
		{
			IsVisiblePopup = false;
		}

#endregion // Binding Events

		public static void ShowInfo(eInfoType type, eInfoLayout layout, string title, string message)
		{
			_viewModel?.SetInfo(type, layout, title, message);
		}

		private void SetInfo(eInfoType infoType, eInfoLayout infoLayout, string title, string message)
		{
			InfoType = infoType;
			InfoLayout = infoLayout;
			Title = title;
			Message = message;

			PlayOpenAnimation = true;
		}

		public void Set(eInfoType infoType, eInfoLayout infoLayout, string title, string message)
		{
			_infoType = infoType;
			_infoLayout = infoLayout;
			_title = title;
			_message = message;
		}

		private void ClearInfo()
		{
			InfoType = eInfoType.NONE;
			InfoLayout = eInfoLayout.NONE;
		}

		private void PlayVisibleAnimation()
		{
			if (_isVisible)
			{
				PlayOpenAnimation = true;
				PlayCloseAnimation = false;
			}
			else
			{
				PlayCloseAnimation = true;
				PlayOpenAnimation = false;
			}
		}

		public static void CloseInfo(eInfoType infoType)
		{
			if (_viewModel.InfoType == infoType)
				_viewModel?.ClearInfo();
		}
	}
}
