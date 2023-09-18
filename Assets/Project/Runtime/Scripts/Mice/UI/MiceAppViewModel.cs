/*===============================================================
* Product:		Com2Verse
* File Name:	MiceAppViewModel.cs
* Developer:	klizzard
* Date:			2023-07-17 14:30
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.Mice;
using Com2Verse.Network;
using Cysharp.Threading.Tasks;

namespace Com2Verse.UI
{
	[ViewModelGroup("Mice")]
	public partial class MiceAppViewModel : ViewModelBase, INestedViewModel
	{
		partial void InitBusinessCardView();
		partial void InvokeBusinessCardView();
		partial void InitNoticeView();
		partial void InvokeNoticeView();
		partial void InitTicketView();
		partial void InvokeTicketView();

		public enum eViewMode
		{
			BACK = -1,

			NONE,
			TICKET_LIST,
			TICKET_DETAIL,
			BUSINESS_MYCARD,
			BUSINESS_CARDLIST,
			NOTICE_LIST,
			NOTICE_DETAIL,

			DEFAULT = NONE,
		}

#region INestedViewModel
		public IList<ViewModel> NestedViewModels { get; } = new List<ViewModel>();
#endregion

#region Variables
		private eViewMode _viewMode = eViewMode.DEFAULT;
		private string    _welcomeMessage;
#endregion

#region Properties
		public eViewMode ViewMode
		{
			get => _viewMode;
			set => SetProperty(ref _viewMode, value);
		}

		public string WelcomeMessage
		{
			get => _welcomeMessage;
			set => SetProperty(ref _welcomeMessage, value);
		}

		public CommandHandler HideView             { get; }
		public CommandHandler BackView             { get; }
		public CommandHandler ShowBusinessCardView { get; }
		public CommandHandler ShowNoticeView       { get; }
		public CommandHandler ShowTicketView       { get; }
#endregion

		public MiceAppViewModel()
		{
			NestedViewModels.Clear();

			InitBusinessCardView();
			InitNoticeView();
			InitTicketView();

			HideView             = new CommandHandler(() => SetViewMode(eViewMode.NONE));
			BackView             = new CommandHandler(() => SetViewMode(eViewMode.BACK));
			ShowBusinessCardView = new CommandHandler(() => SetViewMode(eViewMode.BUSINESS_MYCARD));
			ShowNoticeView = new CommandHandler(() =>
			{
				if (ViewMode == eViewMode.NOTICE_LIST) return;

				MiceInfoManager.Instance.SyncNoticeInfo().ContinueWith(() => { SetViewMode(eViewMode.NOTICE_LIST); }).Forget();
			});
			ShowTicketView = new CommandHandler(() =>
			{
				if (ViewMode == eViewMode.TICKET_LIST) return;

				MiceInfoManager.Instance.SyncMyPackages().ContinueWith(() => { SetViewMode(eViewMode.TICKET_LIST); }).Forget();
			});
		}

		public void SetViewMode(eViewMode viewMode)
		{
			if (ViewMode == viewMode)
			{
				return;
			}

			if (viewMode == eViewMode.BACK)
			{
				viewMode = ViewMode switch
				{
					eViewMode.NOTICE_DETAIL => eViewMode.NOTICE_LIST,
					eViewMode.TICKET_DETAIL => eViewMode.TICKET_LIST,
					_                       => eViewMode.NONE
				};
			}

			ViewMode = viewMode;

			InvokeBusinessCardView();
			InvokeNoticeView();
			InvokeTicketView();
		}

		public override void OnInitialize()
		{
			base.OnInitialize();

			// Welcome 메시지.
			WelcomeMessage = Data.Localization.eKey.MICE_UI_CCApp_Popup_Msg_Wellcome
			                     .ToLocalizationString(User.Instance.CurrentUserData.UserName);
			
			InitTicketOptions();
		}
	}
}
