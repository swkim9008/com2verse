/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationEmployeeDetailViewModel.cs
* Developer:	jhkim
* Date:			2022-07-21 16:04
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Extension;
using Com2Verse.Organization;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Com2Verse.UI
{
	[ViewModelGroup("Organization")]
	public sealed class OrganizationEmployeeDetailViewModel : OrganizationBaseViewModel
	{
#region Variables
		private static readonly Color ActiveTextColor = new(0.3764706f, 0.2117647f, 0.9254903f);
		private static readonly Color DeactivateTextColor = new(0.6156863f, 0.6470588f, 0.7215686f);

		private static readonly string ResName = "UI_Connecting_UserProfilePopup";
		private static OrganizationEmployeeDetailViewModel _viewModel;
		// private static EmployeePayload _employee;
		private static MemberModel _memberModel;
		private OrganizationProfileIconViewModel _profileIconViewModel = new();
		private OrganizationEmployeeInfoViewModel _employeeInfoViewModel = new();

		// Profile
		private string _statusMessage;
		private string _affiliation;
		private string _jobInfo;
		private string _phone;
		private string _email;
		private string _placeOfWork;
		private string _placeOfWorkMeta;
		private string _duties;
		private bool _isMine;

		// 메뉴가 더 추가될 경우 별도 클래스로 분리
		private bool _isCallAvailable;
		private bool _isWhisperAvailable;
		private bool _isFollowAvailable;
		private Color _callAvailableTextColor = new();
		private Color _whisperAvailableTextColor = new();
		private Color _followAvailableTextColor = new();

		private Vector3 _popupPosition = new();

		public CommandHandler Call    { get; }
		public CommandHandler Whisper { get; }
		public CommandHandler Follow  { get; }
		public CommandHandler Close   { get; }

		private static GUIView _view;
		private static bool _requestShow;

		// CHEAT
		private string _cheatAccountId;
		private string _cheatEmployeeNo;
#endregion // Variables

#region Properties
		public OrganizationProfileIconViewModel ProfileIconViewModel
		{
			get => _profileIconViewModel;
			set
			{
				_profileIconViewModel = value;
				InvokePropertyValueChanged(nameof(ProfileIconViewModel), value);
			}
		}

		public OrganizationEmployeeInfoViewModel EmployeeInfoViewModel
		{
			get => _employeeInfoViewModel;
			set
			{
				_employeeInfoViewModel = value;
				InvokePropertyValueChanged(nameof(EmployeeInfoViewModel), value);
			}
		}

		public string StatusMessage
		{
			get => _statusMessage;
			set
			{
				_statusMessage = value;
				InvokePropertyValueChanged(nameof(StatusMessage), value);
			}
		}
		public string Affiliation
		{
			get => _affiliation;
			set
			{
				_affiliation = value;
				InvokePropertyValueChanged(nameof(Affiliation), value);
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

		public string PlaceOfWork
		{
			get => _placeOfWork;
			set
			{
				_placeOfWork = value;
				InvokePropertyValueChanged(nameof(PlaceOfWork), value);
			}
		}

		public string PlaceOfWorkMeta
		{
			get => _placeOfWorkMeta;
			set
			{
				_placeOfWorkMeta = value;
				InvokePropertyValueChanged(nameof(PlaceOfWorkMeta), value);
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

		public bool IsMine
		{
			get => _isMine;
			set
			{
				_isMine = value;
				InvokePropertyValueChanged(nameof(IsMine), value);
			}
		}

		public bool IsCallAvailable
		{
			get => _isCallAvailable;
			set
			{
				_isCallAvailable = value;
				InvokePropertyValueChanged(nameof(IsCallAvailable), value);
			}
		}

		public bool IsWhisperAvailable
		{
			get => _isWhisperAvailable;
			set
			{
				_isWhisperAvailable = value;
				InvokePropertyValueChanged(nameof(IsWhisperAvailable), value);
			}
		}

		public bool IsFollowAvailable
		{
			get => _isFollowAvailable;
			set
			{
				_isFollowAvailable = value;
				InvokePropertyValueChanged(nameof(IsFollowAvailable), value);
			}
		}

		public Color CallAvailableTextColor
		{
			get => _callAvailableTextColor;
			set
			{
				_callAvailableTextColor = value;
				InvokePropertyValueChanged(nameof(CallAvailableTextColor), value);
			}
		}

		public Color WhisperAvailableTextColor
		{
			get => _whisperAvailableTextColor;
			set
			{
				_whisperAvailableTextColor = value;
				InvokePropertyValueChanged(nameof(WhisperAvailableTextColor), value);
			}
		}

		public Color FollowAvailableTextColor
		{
			get => _followAvailableTextColor;
			set
			{
				_followAvailableTextColor = value;
				InvokePropertyValueChanged(nameof(FollowAvailableTextColor), value);
			}
		}

		public Vector3 PopupPosition
		{
			get => _popupPosition;
			set
			{
				_popupPosition = value == Vector3.zero ? Constant.DefaultPopupPosition : value;
				InvokePropertyValueChanged(nameof(PopupPosition), PopupPosition);
			}
		}

		// CHEAT
		public string CheatEmployeeNo
		{
			get => _cheatEmployeeNo;
			set
			{
				_cheatEmployeeNo = value;
				InvokePropertyValueChanged(nameof(CheatEmployeeNo), value);
			}
		}

		public string CheatAccountID
		{
			get => _cheatAccountId;
			set
			{
				_cheatAccountId = value;
				InvokePropertyValueChanged(nameof(CheatAccountID), value);
			}
		}
#endregion // Properties

#region View
		public static void ShowView(Action onLoaded) => ShowView(ResName, _memberModel, onLoaded);
		public static void HideView() => HideView(ResName);
#endregion // View

#region Initialize
		public OrganizationEmployeeDetailViewModel() : base(ResName)
		{
			_viewModel = this;
			Whisper = new CommandHandler(OnWhisper);
			Follow = new CommandHandler(OnFollow);
			Close = new CommandHandler(OnClose);
			ProfileIconViewModel = new OrganizationProfileIconViewModel();
			EmployeeInfoViewModel = new OrganizationEmployeeInfoViewModel();
		}

		// public static void SetModel(EmployeePayload employeePayload)
		// {
		// 	_employee = employeePayload;
		// 	_viewModel?.RefreshUI();
		// }

		public static void SetModel(MemberModel memberModel)
		{
			_memberModel = memberModel;
			_viewModel?.RefreshUI();
		}
		public static void SetPosition(RectTransform rt)
		{
			if (_viewModel == null || rt.IsReferenceNull()) return;
			_viewModel.PopupPosition = Constant.DefaultPopupPosition;
		}
#endregion // Initialize

#region Binding Events
		private void OnWhisper() => UIManager.Instance.ShowPopupCommon("미구현 기능입니다.");
		private void OnFollow() => UIManager.Instance.ShowPopupCommon("미구현 기능입니다.");
		private void OnClose()
		{
			HideView();
		}
#endregion // Binding Events

		private void RefreshUI()
		{
			StatusMessage = "";
			Affiliation = _memberModel.Affiliation;
			JobInfo = _memberModel.GetPositionLevelStr();
			Phone = _memberModel.GetFormattedTelNo();
			Email = _memberModel.Member.MailAddress;
			PlaceOfWork = _memberModel.TeamName;
			PlaceOfWorkMeta = "";
			Duties = _memberModel.Member.Task;
			IsMine = _memberModel.IsMine();

			// TODO : 사용가능 상태에 따른 처리 필요
			IsCallAvailable = true;
			IsWhisperAvailable = false;
			IsFollowAvailable = false;

			CallAvailableTextColor = IsCallAvailable ? ActiveTextColor : DeactivateTextColor;
			WhisperAvailableTextColor = IsWhisperAvailable ? ActiveTextColor : DeactivateTextColor;
			FollowAvailableTextColor = IsFollowAvailable ? ActiveTextColor : DeactivateTextColor;

			ProfileIconViewModel.SetInfo(_memberModel);
			EmployeeInfoViewModel.SetInfo(_memberModel.Member.AccountId, false);

			// CHEAT
			CheatEmployeeNo = Convert.ToString(_memberModel.Member.AccountId);
			CheatAccountID = Convert.ToString(_memberModel.Member.AccountId);
		}
	}
}
