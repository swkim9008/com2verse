/*===============================================================
* Product:		Com2Verse
* File Name:	MiceBusinessCardBookViewModel.cs
* Developer:	wlemon
* Date:			2023-04-06 12:55
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.Threading;
using Com2Verse.Mice;
using Cysharp.Threading.Tasks;

namespace Com2Verse.UI
{
	[ViewModelGroup("Mice")]
	public partial class MiceBusinessCardBookViewModel : ViewModelBase, INestedViewModel
	{
		public static readonly string ResNameCreate = "UI_Popup_BusinessCard_Create";

		public enum eTabType
		{
			NONE,
			MYCARD,
			CARDLIST,
		}

#region Variables
		private eTabType                  _tabType;
		private bool                      _setVisible;
		private MiceBusinessCardViewModel _miceBusinessCardViewModel;
		private bool                      _isCardViewVisible;
		private CancellationTokenSource   _ctsUserList;

		public CommandHandler TabMyCard   { get; }
		public CommandHandler TabCardList { get; }

		public bool IsTabMyCard   => _tabType == eTabType.MYCARD;
		public bool IsTabCardList => _tabType == eTabType.CARDLIST;
#endregion

#region Properties
		public eTabType TabType
		{
			get => _tabType;
			set
			{
				SetProperty(ref _tabType, value);
				InvokePropertyValueChanged(nameof(IsTabMyCard),   IsTabMyCard);
				InvokePropertyValueChanged(nameof(IsTabCardList), IsTabCardList);
			}
		}

		public bool SetVisible
		{
			get => _setVisible;
			set => SetProperty(ref _setVisible, value);
		}

		public bool IsCardViewVisible
		{
			get => _isCardViewVisible;
			set => SetProperty(ref _isCardViewVisible, value);
		}
#endregion

		public IList<ViewModel> NestedViewModels { get; } = new List<ViewModel>();

#region Initialize
		public MiceBusinessCardBookViewModel()
		{
			TabMyCard   = new CommandHandler(OnTabMyCard);
			TabCardList = new CommandHandler(OnTabCardList);
			TabType     = eTabType.MYCARD;

			_miceBusinessCardViewModel = new MiceBusinessCardViewModel();
			NestedViewModels.Add(_miceBusinessCardViewModel);

			InitializeMyUserInfo();
			InitializeUserList();
		}
#endregion

		public override void OnInitialize()
		{
			base.OnInitialize();

			SetMyUserInfo(MiceInfoManager.Instance.MyUserInfo);
			SyncCardList().Forget();

			TabType           = eTabType.MYCARD;
			_setVisible       = true;
			IsCardViewVisible = false;
		}

		public override void OnRelease()
		{
			base.OnRelease();
			_ctsUserList?.Cancel();
		}

		private void OnTabMyCard()
		{
			TabType           = eTabType.MYCARD;
			IsCardViewVisible = false;
		}

		private void OnTabCardList()
		{
			TabType           = eTabType.CARDLIST;
			IsCardViewVisible = false;
		}

		private async UniTask SyncCardList()
		{
			try
			{
				if (_ctsUserList != null) return;

				_ctsUserList = new CancellationTokenSource();

				var cancellationToken = _ctsUserList.Token;
				await MiceInfoManager.Instance.SyncCardList();
				cancellationToken.ThrowIfCancellationRequested();

				SetUserList(MiceInfoManager.Instance.UserInfoList);
			}
			finally
			{
				_ctsUserList?.Dispose();
				_ctsUserList = null;
			}
		}

		public static void ShowView()
		{
			UIManager.Instance.CreatePopup(ResNameCreate, async (guiView) =>
			{
				// NestedViewModel 처리를 위해 미리 등록
				guiView.ViewModelContainer.GetOrAddViewModel<MiceBusinessCardBookViewModel>();
				guiView.OnClosedEvent += OnClosedEvent;
				guiView.Show();

				void OnClosedEvent(GUIView view)
				{
					view.OnClosedEvent -= OnClosedEvent;
				}
			}).Forget();
		}
	}
}
