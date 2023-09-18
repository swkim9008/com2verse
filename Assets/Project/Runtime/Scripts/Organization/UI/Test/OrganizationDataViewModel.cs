/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationDataViewModel.cs
* Developer:	jhkim
* Date:			2022-08-31 14:51
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Organization;
using Cysharp.Threading.Tasks;

namespace Com2Verse.UI
{
	[ViewModelGroup("Organization")]
	public partial class OrganizationDataViewModel : OrganizationBaseViewModel
	{
#region Variables
		private static readonly string ResName = "UI_OrganizationDataView";

		private string         _title;
		private bool           _isVisibleHierarchy;
		private bool           _isVisibleTeamWork ;
		public  CommandHandler ResetSelect           { get; }
		public  CommandHandler ShowOrganizationPopup { get; }
		public  CommandHandler ShowOrganization      { get; }
		public  CommandHandler ShowTeamWork          { get; }
		public  CommandHandler Close                 { get; }

		public Collection<OrganizationDataButtonListViewModel> _menuButtons = new();

#endregion // Variables

#region Properties
		public string Title
		{
			get => _title;
			set
			{
				_title = value;
				InvokePropertyValueChanged(nameof(Title), value);
			}
		}
		public bool IsVisibleOrganization
		{
			get => _isVisibleHierarchy;
			set
			{
				_isVisibleHierarchy = value;
				InvokePropertyValueChanged(nameof(IsVisibleOrganization), value);
			}
		}

		public bool IsVisibleTeamWork
		{
			get => _isVisibleTeamWork;
			set
			{
				_isVisibleTeamWork = value;
				InvokePropertyValueChanged(nameof(IsVisibleTeamWork), value);
			}
		}
		public Collection<OrganizationDataButtonListViewModel> MenuButtons
		{
			get => _menuButtons;
			set
			{
				_menuButtons = value;
				InvokePropertyValueChanged(nameof(MenuButtons), value);
			}
		}
#endregion // Properties

#region View
		public static void ShowView() => ShowView(ResName);
		private static void HideView() => HideView(ResName);
#endregion // View

#region Initialize
		public OrganizationDataViewModel() : base(ResName)
		{
			IsVisibleOrganization = true;

			ResetSelect = new CommandHandler(OnResetSelect);
			ShowOrganizationPopup = new CommandHandler(OnShowOrganizationPopup);
			ShowOrganization = new CommandHandler(OnShowOrganization);
			ShowTeamWork = new CommandHandler(OnShowTeamWork);
			Close = new CommandHandler(OnClose);

			InitAsync().Forget();
		}

		private async UniTask InitAsync()
		{
			if (!DataManager.Instance.IsReady)
				await DataManager.SendOrganizationChartRequestAsync(DataManager.Instance.GroupID);

			SetHierarchyItem();
			InitMenuButtons();
			InitSearchField();
			OnResetSelect();

			InitTeamWork();
			RefreshTitle();
		}
#endregion // Initialize

#region Binding Events
		private void OnResetSelect()
		{
			ClearAllInfos();
			var groupModel = DataManager.Instance.GetGroupModel();
			SetTeam(groupModel);
		}

		private void OnShowOrganizationPopup()
		{
		}

		private void OnShowOrganization()
		{
			IsVisibleOrganization = true;
			IsVisibleTeamWork = false;
			RefreshTitle();
		}

		private void OnShowTeamWork()
		{
			IsVisibleOrganization = false;
			IsVisibleTeamWork = true;
			RefreshTitle();
		}

		private void OnClose() => HideView();
#endregion // Binding Events

		private void InitMenuButtons()
		{
			MenuButtons.Reset();

			// Add("데이터 갱신", Organization.DataManager.SendOrganizationChartRequest);
			// void Add(string text, Action onClick) => MenuButtons.AddItem(new OrganizationDataButtonListViewModel(text, onClick));
		}

		private void RefreshTitle() => Title = IsVisibleOrganization ? "조직도 데이터 뷰어" : "팀워크 네트워크 테스트";
		void ClearAllInfos()
		{
			ClearSubDepartmentList();
			ClearDepartmentInfo();
			ClearMemberList();
			ClearEmployeeInfo();
		}
	}
}
