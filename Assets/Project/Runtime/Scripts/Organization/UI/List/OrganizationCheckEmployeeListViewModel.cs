/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationCheckEmployeeListViewModel.cs
* Developer:	jhkim
* Date:			2022-07-20 17:35
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/


using System;
using System.Collections.Generic;
using Com2Verse.Organization;
using UnityEngine;

namespace Com2Verse.UI
{
	[ViewModelGroup("Organization")]
	public sealed class OrganizationCheckEmployeeListViewModel : ViewModelBase, INestedViewModel
	{
#region Variables
		private CheckMemberListModel _model;
		private bool _isChecked;
		private bool _uiIsChecked;
		private bool _isInteractable;
		private Texture _profileImage;
		private string _name;
		private string _jobInfo;
		private string _phone;
		private string _email;
		private string _duties;

		public CommandHandler ClickCheck { get; }
		public CommandHandler Profile { get; }
		private Action<CheckMemberListModel> _onEmployeeCheckClicked = isOn => { };
		private RectTransform _employeePopupRectTransform;
#endregion // Variables

#region Properties
		public IList<ViewModel> NestedViewModels { get; } = new List<ViewModel>();
		public bool IsChecked
		{
			get => _isChecked;
			set
			{
				UiIsChecked = value;
				if (!IsInteractable) return;
				SetChecked(value);
				_onEmployeeCheckClicked(_model);
				InvokePropertyValueChanged(nameof(IsChecked), value);
			}
		}

		public bool UiIsChecked
		{
			get => _uiIsChecked;
			set
			{
				_uiIsChecked = value;
				CheckAndUpdateModel(value);
				InvokePropertyValueChanged(nameof(UiIsChecked), value);
			}
		}

		public bool IsInteractable
		{
			get => _isInteractable;
			set
			{
				_isInteractable = value;
				InvokePropertyValueChanged(nameof(IsInteractable), value);
			}
		}
		public Texture ProfileImage
		{
			get => _profileImage;
			set
			{
				_profileImage = value;
				InvokePropertyValueChanged(nameof(ProfileImage), value);
			}
		}
		public string Name
		{
			get => _name;
			set
			{
				_name = value;
				InvokePropertyValueChanged(nameof(Name), value);
			}
		}
		public string JobInfo
		{
			get => _jobInfo;
			set
			{
				_jobInfo = value;
				InvokePropertyValueChanged(nameof(JobInfo), value);
			}
		}

		public string Phone
		{
			get => _phone;
			set
			{
				_phone = value;
				InvokePropertyValueChanged(nameof(Phone), value);
			}
		}

		public string Email
		{
			get => _email;
			set
			{
				_email = value;
				InvokePropertyValueChanged(nameof(Email), value);
			}
		}

		public string Duties
		{
			get => _duties;
			set
			{
				_duties = value;
				InvokePropertyValueChanged(nameof(Duties), value);
			}
		}
		public bool IsMine => _model.Info.IsMine();
		public string ID
		{
			get => $"{Convert.ToString(_model.Info.Member.AccountId)})";
			set => InvokePropertyValueChanged(nameof(ID), value);
		}
		public CheckMemberListModel Model => _model;
#endregion // Properties

#region Initialize
		public OrganizationCheckEmployeeListViewModel() { }

		public OrganizationCheckEmployeeListViewModel(CheckMemberListModel model, RectTransform employeePopupRectTransform, Action<CheckMemberListModel> onEmployeeCheckClicked)
		{
			_model = model;
			_employeePopupRectTransform = employeePopupRectTransform;
			_onEmployeeCheckClicked = onEmployeeCheckClicked;

			NestedViewModels.Add(new OrganizationProfileIconViewModel(_model.Info, OnProfile) {IsShowBadge = true});

			ClickCheck = new CommandHandler(OnClickCheck);
			Profile = new CommandHandler(OnProfile);

			IsInteractable = model.IsInteractable;
			UiIsChecked = model.IsChecked;

			Name = model.Info.Member.MemberName;
			JobInfo = model.Info.GetPositionLevelTeamStr();
			Phone = model.Info.GetFormattedTelNo();
			Email = model.Info.Member.MailAddress;

			Duties = model.Info.Member.Task;
		}
#endregion // Initialize

#region Binding Events
		private void OnClickCheck()
		{
			UiIsChecked = !UiIsChecked;
			_onEmployeeCheckClicked(_model);
		}
		private void OnProfile()
		{
			OrganizationEmployeeDetailViewModel.SetModel(_model.Info);
			OrganizationEmployeeDetailViewModel.ShowView(() =>
			{
				OrganizationEmployeeDetailViewModel.SetPosition(_employeePopupRectTransform);
			});
		}
#endregion // Binding Events

		// 전체선택 버튼용 체크 처리
		public void SetChecked(bool value)
		{
			if (_model.Info.IsMine()) return;
			_isChecked = value;
			_model.IsChecked = value;
			CheckAndUpdateModel(value);
			UiIsChecked = value;
		}
		private void CheckAndUpdateModel(bool value)
		{
			if (_model.IsChecked != value)
				_model.IsChecked = value;
		}
	}
}
