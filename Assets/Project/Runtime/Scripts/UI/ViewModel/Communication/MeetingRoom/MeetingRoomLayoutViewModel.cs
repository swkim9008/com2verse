/*===============================================================
 * Product:		Com2Verse
 * File Name:	MeetingRoomLayoutViewModel.cs
 * Developer:	urun4m0r1
 * Date:		2022-12-13 14:57
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Runtime.CompilerServices;
using Com2Verse.Chat;
using Com2Verse.Communication;
using Com2Verse.Data;
using Com2Verse.MeetingReservation;
using Com2Verse.Extension;
using Com2Verse.InputSystem;
using Com2Verse.Notification;
using Com2Verse.PlayerControl;
using Com2Verse.Tutorial;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using User = Com2Verse.Network.User;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("MeetingRoom")]
	public sealed partial class MeetingRoomLayoutViewModel : ViewModelBase, IDisposable
	{
		public event Action<string, bool>? LayoutChanged;

		[UsedImplicitly] public CommandHandler<bool> SetExitLayout           { get; }
		[UsedImplicitly] public CommandHandler<bool> SetUseInfoLayout           { get; }
		[UsedImplicitly] public CommandHandler<bool> SetMicLayout            { get; }
		[UsedImplicitly] public CommandHandler<bool> SetCameraLayout         { get; }
		[UsedImplicitly] public CommandHandler<bool> SetHandLayout           { get; }
		[UsedImplicitly] public CommandHandler<bool> SetEmotionLayout        { get; }
		[UsedImplicitly] public CommandHandler<bool> SetShareLayout          { get; }
		[UsedImplicitly] public CommandHandler<bool> SetScreenLayout         { get; }
		[UsedImplicitly] public CommandHandler<bool> SetMinimizedLayout      { get; }
		[UsedImplicitly] public CommandHandler<bool> SetGridLayout           { get; }
		[UsedImplicitly] public CommandHandler<bool> SetMoreLayout           { get; }
		[UsedImplicitly] public CommandHandler<bool> SetChatLayout           { get; }
		[UsedImplicitly] public CommandHandler<bool> SetUserLayout           { get; }
		[UsedImplicitly] public CommandHandler<bool> SetInfoLayout           { get; }
		[UsedImplicitly] public CommandHandler<bool> SetNotificationLayout   { get; }
		[UsedImplicitly] public CommandHandler<bool> SetMyPadLayout          { get; }
		[UsedImplicitly] public CommandHandler<bool> SetMeetingMinutesLayout { get; }

		[UsedImplicitly] public CommandHandler ToggleExitLayout           { get; }
		[UsedImplicitly] public CommandHandler ToggleUseInfoLayout           { get; }
		[UsedImplicitly] public CommandHandler ToggleMicLayout            { get; }
		[UsedImplicitly] public CommandHandler ToggleCameraLayout         { get; }
		[UsedImplicitly] public CommandHandler ToggleHandLayout           { get; }
		[UsedImplicitly] public CommandHandler ToggleEmotionLayout        { get; }
		[UsedImplicitly] public CommandHandler ToggleShareLayout          { get; }
		[UsedImplicitly] public CommandHandler ToggleScreenLayout         { get; }
		[UsedImplicitly] public CommandHandler ToggleMinimizedLayout      { get; }
		[UsedImplicitly] public CommandHandler ToggleGridLayout           { get; }
		[UsedImplicitly] public CommandHandler ToggleMoreLayout           { get; }
		[UsedImplicitly] public CommandHandler ToggleChatLayout           { get; }
		[UsedImplicitly] public CommandHandler ToggleUserLayout           { get; }
		[UsedImplicitly] public CommandHandler ToggleInfoLayout           { get; }
		[UsedImplicitly] public CommandHandler ToggleNotificationLayout   { get; }
		[UsedImplicitly] public CommandHandler ToggleMyPadLayout          { get; }
		[UsedImplicitly] public CommandHandler ToggleMeetingMinutesLayout { get; }

		[UsedImplicitly] public CommandHandler CommandOpenProfile { get; }

		private bool _isDebugLayout;

		private bool _isExitLayout;
		private bool _isUseInfoLayout;
		private bool _isMicLayout;
		private bool _isCameraLayout;
		private bool _isHandLayout;
		private bool _isEmotionLayout;
		private bool _isShareLayout;
		private bool _isScreenLayout;
		private bool _isMinimizedLayout;
		private bool _isGridLayout;
		private bool _isMoreLayout;
		private bool _isChatLayout;
		private bool _isUserLayout;
		private bool _isInfoLayout;
		private bool _isNotificationLayout;
		private bool _isMyPadLayout;
		private bool _isProfileLayout;
		private bool _isMeetingMinutesLayout;

		private readonly SharedScreen _sharedScreen;


		private bool _isClosingPopupLayouts;

		private partial void Initialize();


		public MeetingRoomLayoutViewModel()
		{
			SetExitLayout           = new CommandHandler<bool>(value => IsExitLayout           = value);
			SetUseInfoLayout        = new CommandHandler<bool>(value => IsUseInfoLayout        = value);
			SetMicLayout            = new CommandHandler<bool>(value => IsMicLayout            = value);
			SetCameraLayout         = new CommandHandler<bool>(value => IsCameraLayout         = value);
			SetHandLayout           = new CommandHandler<bool>(value => IsHandLayout           = value);
			SetEmotionLayout        = new CommandHandler<bool>(value => IsEmotionLayout        = value);
			SetShareLayout          = new CommandHandler<bool>(value => IsShareLayout          = value);
			SetScreenLayout         = new CommandHandler<bool>(value => IsScreenLayout         = value);
			SetMinimizedLayout      = new CommandHandler<bool>(value => IsMinimizedLayout      = value);
			SetGridLayout           = new CommandHandler<bool>(value => IsGridLayout           = value);
			SetMoreLayout           = new CommandHandler<bool>(value => IsMoreLayout           = value);
			SetChatLayout           = new CommandHandler<bool>(value => IsChatLayout           = value);
			SetUserLayout           = new CommandHandler<bool>(value => IsUserLayout           = value);
			SetInfoLayout           = new CommandHandler<bool>(value => IsInfoLayout           = value);
			SetNotificationLayout   = new CommandHandler<bool>(value => IsNotificationLayout   = value);
			SetMyPadLayout          = new CommandHandler<bool>(value => IsMyPadLayout          = value);
			SetMeetingMinutesLayout = new CommandHandler<bool>(value => IsMeetingMinutesLayout = value);

			ToggleExitLayout           = new CommandHandler(() => IsExitLayout           ^= true);
			ToggleUseInfoLayout        = new CommandHandler(() => IsUseInfoLayout        ^= true);
			ToggleMicLayout            = new CommandHandler(() => IsMicLayout            ^= true);
			ToggleCameraLayout         = new CommandHandler(() => IsCameraLayout         ^= true);
			ToggleHandLayout           = new CommandHandler(() => IsHandLayout           ^= true);
			ToggleEmotionLayout        = new CommandHandler(() => IsEmotionLayout        ^= true);
			ToggleShareLayout          = new CommandHandler(() => IsShareLayout          ^= true);
			ToggleScreenLayout         = new CommandHandler(() => IsScreenLayout         ^= true);
			ToggleMinimizedLayout      = new CommandHandler(() => IsMinimizedLayout      ^= true);
			ToggleGridLayout           = new CommandHandler(() => IsGridLayout           ^= true);
			ToggleMoreLayout           = new CommandHandler(() => IsMoreLayout           ^= true);
			ToggleChatLayout           = new CommandHandler(() => IsChatLayout           ^= true);
			ToggleUserLayout           = new CommandHandler(() => IsUserLayout           ^= true);
			ToggleInfoLayout           = new CommandHandler(() => IsInfoLayout           ^= true);
			ToggleNotificationLayout   = new CommandHandler(() => IsNotificationLayout   ^= true);
			ToggleMyPadLayout          = new CommandHandler(() => IsMyPadLayout          ^= true);
			ToggleMeetingMinutesLayout = new CommandHandler(() => IsMeetingMinutesLayout ^= true);

			CommandOpenProfile = new CommandHandler(OnCommandOpenProfile);

			_sharedScreen = new SharedScreen();

			_sharedScreen.UserChanged += OnScreenSharingUserChanged;

			var screenShareViewModel = ViewModelManager.Instance.GetOrAdd<ScreenShareViewModel>();
			screenShareViewModel.ListToggled += OnScreenShareListToggled;
			OnScreenShareListToggled(screenShareViewModel.IsListEnabled);

			var meetingMinutesRecordViewModel = ViewModelManager.Instance.GetOrAdd<MeetingMinutesRecordViewModel>();
			meetingMinutesRecordViewModel.ListToggled += OnMeetingMinutesRecordListToggled;
			OnMeetingMinutesRecordListToggled(meetingMinutesRecordViewModel.IsOpen);

			MyPadManager.Instance.OnMyPadOpenedEvent += OnMyPadOpened;
			MyPadManager.Instance.OnMyPadClosedEvent += OnMyPadClosed;

			NotificationManager.Instance.OnNotificationPopUpOpenedEvent += OnNotificationPopUpOpened;
			NotificationManager.Instance.OnNotificationPopUpClosedEvent += OnNotificationPopUpClosed;

			TutorialManager.Instance.OnTutorialOpenedEvent += OnTutorialOpened;
			TutorialManager.Instance.OnTutorialClosedEvent += OnTutorialClosed;

			var playerController = PlayerController.InstanceOrNull;
			if (!playerController.IsUnityNull())
			{
				playerController!.GestureHelper.OnUIOpenedEvent += OnEmotionUIOpened;
				playerController.GestureHelper.OnUIClosedEvent  += OnEmotionUIClosed;
			}

			Initialize();
		}

		public void Dispose()
		{
			_sharedScreen.UserChanged -= OnScreenSharingUserChanged;

			_sharedScreen.Dispose();

			var screenShareViewModel = ViewModelManager.InstanceOrNull?.Get<ScreenShareViewModel>();
			if (screenShareViewModel != null)
				screenShareViewModel.ListToggled -= OnScreenShareListToggled;

			var meetingMinutesRecordViewModel = ViewModelManager.InstanceOrNull?.Get<MeetingMinutesRecordViewModel>();
			if (meetingMinutesRecordViewModel != null)
				meetingMinutesRecordViewModel.ListToggled -= OnMeetingMinutesRecordListToggled;

			var myPadManager = MyPadManager.InstanceOrNull;
			if (myPadManager != null)
			{
				myPadManager.OnMyPadOpenedEvent -= OnMyPadOpened;
				myPadManager.OnMyPadClosedEvent -= OnMyPadClosed;
			}

			var notificationManager = NotificationManager.InstanceOrNull;
			if (notificationManager != null)
			{
				notificationManager.OnNotificationPopUpOpenedEvent -= OnNotificationPopUpOpened;
				notificationManager.OnNotificationPopUpClosedEvent -= OnNotificationPopUpClosed;
			}

			var playerController = PlayerController.InstanceOrNull;
			if (!playerController.IsUnityNull())
			{
				playerController!.GestureHelper.OnUIOpenedEvent -= OnEmotionUIOpened;
				playerController.GestureHelper.OnUIClosedEvent  -= OnEmotionUIClosed;
			}
			
			var tutorialManager = TutorialManager.InstanceOrNull;
			if (tutorialManager != null)
			{
				TutorialManager.Instance.OnTutorialOpenedEvent += OnTutorialOpened;
				TutorialManager.Instance.OnTutorialClosedEvent += OnTutorialClosed;
			}
		}

		private void OnScreenShareListToggled(bool isEnabled) => IsShareLayout = isEnabled;
		private void OnMeetingMinutesRecordListToggled(bool isEnabled) => IsMeetingMinutesLayout = isEnabled;

		private void OnMyPadOpened()             => IsMyPadLayout = true;
		private void OnMyPadClosed()             => IsMyPadLayout = false;
		private void OnNotificationPopUpOpened() => IsNotificationLayout = true;
		private void OnNotificationPopUpClosed() => IsNotificationLayout = false;
		private void OnEmotionUIOpened()         => IsEmotionLayout = true;
		private void OnEmotionUIClosed()         => IsEmotionLayout = false;
		private void OnTutorialOpened()          => IsUseInfoLayout = true;
		private void OnTutorialClosed()          => IsUseInfoLayout = false;
		/// <summary>
		/// 화면 공유 상태가 변경되었을 때 호출.
		/// </summary>
		private void OnScreenSharingUserChanged(IViewModelUser? prevUser, IViewModelUser? user)
		{
			var isLocal  = user is ILocalUser;
			var isShared = user != null;

			IsMinimizedLayout = (isShared, isLocal, IsFullscreenLayout) switch
			{
				(true, true, false) => true,  // 내가 공유를 시작할 때, 전체화면이 아니었던 경우만 최소화 모드로 공유 화면 표시
				(true, true, true)  => false, // 내가 공유를 시작할 때, 전체화면이었던 경우는 일반 공유 화면 표시
				(true, false, _)    => false, // 다른 사람이 공유를 시작한 경우, 전체화면 여부와 상관없이 일반 공유 화면 표시
				(false, _, _)       => false, // 공유가 종료된 경우 기본 상태 복구
			};

			IsScreenLayout = isShared;
		}

#region ViewModelProperties
		/// <summary>
		/// 디버그 레이아웃이 열려있는지 여부
		/// </summary>
		public bool IsDebugLayout
		{
			get => _isDebugLayout;
			set => UpdateProperty(ref _isDebugLayout, value);
		}

		/// <summary>
		/// 닫기 드롭다운 메뉴가 열려있는지 여부
		/// </summary>
		public bool IsExitLayout
		{
			get => _isExitLayout;
			set => UpdateProperty(ref _isExitLayout, value);
		}
		
		/// <summary>
		/// 사용안내 메뉴가 열려있는지 여부
		/// </summary>
		public bool IsUseInfoLayout
		{
			get => _isUseInfoLayout;
			set => UpdateProperty(ref _isUseInfoLayout, value);
		}

		/// <summary>
		/// 마이크 설정 창이 열려있는지 여부
		/// </summary>
		public bool IsMicLayout
		{
			get => _isMicLayout;
			set => UpdateProperty(ref _isMicLayout, value);
		}

		/// <summary>
		/// 카메라 설정 창이 열려있는지 여부
		/// </summary>
		public bool IsCameraLayout
		{
			get => _isCameraLayout;
			set => UpdateProperty(ref _isCameraLayout, value);
		}

		/// <summary>
		/// 손들기 기능이 사용중인지 여부
		/// </summary>
		public bool IsHandLayout
		{
			get => _isHandLayout;
			set
			{
				ChatManager.Instance.BroadcastCustomNotify(value ? ChatManager.CustomDataType.HANDS_UP : ChatManager.CustomDataType.HANDS_DOWN);
				UpdateProperty(ref _isHandLayout, value);
			}
		}

		/// <summary>
		/// 감정 표현 레이아웃이 열려있는지 여부
		/// </summary>
		public bool IsEmotionLayout
		{
			get => _isEmotionLayout;
			set => UpdateProperty(ref _isEmotionLayout, value);
		}

		/// <summary>
		/// 화면 공유 창 선택 팝업이 열려있는지 여부
		/// </summary>
		public bool IsShareLayout
		{
			get => _isShareLayout;
			set => UpdateProperty(ref _isShareLayout, value);
		}

		/// <summary>
		/// 공유중인 화면 레이아웃이 열려있는지 여부
		/// </summary>
		public bool IsScreenLayout
		{
			get => _isScreenLayout;
			set => UpdateProperty(ref _isScreenLayout, value);
		}

		/// <summary>
		/// 공유중인 화면 레이아웃이 최소화되어있는지 여부
		/// </summary>
		public bool IsMinimizedLayout
		{
			get => _isMinimizedLayout;
			set => UpdateProperty(ref _isMinimizedLayout, value);
		}

		/// <summary>
		/// 화상화면 격자 레이아웃이 열려있는지 여부
		/// </summary>
		public bool IsGridLayout
		{
			get => _isGridLayout;
			set => UpdateProperty(ref _isGridLayout, value);
		}

		/// <summary>
		/// 더보기 레이아웃이 열려있는지 여부
		/// </summary>
		public bool IsMoreLayout
		{
			get => _isMoreLayout;
			set
			{
				if (value)
				{
					if (MeetingReservationProvider.IsGuest())
					{
						UIManager.Instance.SendToastMessage("UI_Guest_MeetingRoom_GuestNotUse_Toast");
						return;
					}
				}
				UpdateProperty(ref _isMoreLayout, value);
			}
		}

		/// <summary>
		/// 채팅 레이아웃이 열려있는지 여부
		/// </summary>
		public bool IsChatLayout
		{
			get => _isChatLayout;
			set => UpdateProperty(ref _isChatLayout, value);
		}

		/// <summary>
		/// 사용자 목록 레이아웃이 열려있는지 여부
		/// </summary>
		public bool IsUserLayout
		{
			get => _isUserLayout;
			set => UpdateProperty(ref _isUserLayout, value);
		}

		/// <summary>
		/// 회의 정보 레이아웃이 열려있는지 여부
		/// </summary>
		public bool IsInfoLayout
		{
			get => _isInfoLayout;
			set => UpdateProperty(ref _isInfoLayout, value);
		}

		/// <summary>
		/// 알림 레이아웃이 열려있는지 여부
		/// </summary>
		public bool IsNotificationLayout
		{
			get => _isNotificationLayout;
			set => UpdateProperty(ref _isNotificationLayout, value);
		}

		/// <summary>
		/// 마이패드 레이아웃이 열려있는지 여부
		/// </summary>
		public bool IsMyPadLayout
		{
			get => _isMyPadLayout;
			set => UpdateProperty(ref _isMyPadLayout, value);
		}

		/// <summary>
		/// 회의록 레이아웃이 열려있는지 여부
		/// </summary>
		public bool IsMeetingMinutesLayout
		{
			get => _isMeetingMinutesLayout;
			set
			{
				if (value)
				{
					if (MeetingReservationProvider.IsGuest())
					{
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_Guest_MeetingRoom_GuestNotUse_Toast"));
						return;
					}
					if (!MeetingReservationProvider.IsOrganizer(User.Instance.CurrentUserData.ID))
					{
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_OtherMenu_Stt_RecordingFailed_Authority_Toast"));
						return;
					}
					IsMoreLayout = false;
					
					
					if (_isMeetingMinutesLayout != value)
					{
						UIManager.Instance.ShowPopupConfirm(Localization.Instance.GetString("UI_ConnectingApp_Detail_Stt_STTNoteTitle_Text"), Localization.Instance.GetString("UI_MeetingRoom_OtherMenu_Stt_PrivacyNotiPopup_Text"));
					}
				}
				else
				{
					if (MeetingReservationProvider.IsRecording)
					{
						var meetingMinuteViewModel = ViewModelManager.Instance.GetOrAdd<MeetingMinutesRecordViewModel>();
						meetingMinuteViewModel.IsOpen = false;
						return;
					}
				}

				UpdateProperty(ref _isMeetingMinutesLayout, value);
			}
		}


		/// <summary>
		/// 전체화면 레이아웃이 열려있는지 여부 (=화면 공유 시작 UI 결정)
		/// </summary>
		public bool IsFullscreenLayout => IsGridLayout || (IsScreenLayout && !IsMinimizedLayout);

		/// <summary>
		/// 사이드 레이아웃이 열려있는지 여부 (=전체화면 레이아웃 사이즈 조절 여부)
		/// </summary>
		public bool IsSideLayout => IsChatLayout || IsUserLayout || IsInfoLayout;

		/// <summary>
		/// 배경을 클릭해 닫을 수 있는 레이아웃이 열려있는지 여부 (= 투명 닫기 버튼 표시 여부)
		/// </summary>
		public bool IsBlockingLayout => false;

		/// <summary>
		/// 공유중인 화면 정보 레이아웃이 열려있는지 여부, 화면 공유 창 선택 레이아웃이 열린 경우 제외
		/// </summary>
		public bool IsScreenInfoLayout => IsScreenLayout && !IsShareLayout;
#endregion // ViewModelProperties

		private bool UpdateProperty(ref bool storage, bool value, [CallerMemberName] string propertyName = "")
		{
			if (storage == value)
				return false;

			SetProperty(ref storage, value, propertyName);
			LayoutChanged?.Invoke(propertyName, value);

			ClosePopupLayoutsOnOthers(propertyName);
			CloseSideLayoutsOnOthers(propertyName);
			CloseGridOrMinimizedLayoutOnOthers(propertyName);

			UpdateLayoutDependentProperty();
			RefreshLayoutDependencies();

			return true;
		}

		private void RaisePropertyValueChanged(string propertyName, bool value)
		{
			InvokePropertyValueChanged(propertyName, value);
			LayoutChanged?.Invoke(propertyName, value);
		}

		private void UpdateLayoutDependentProperty()
		{
			RaisePropertyValueChanged(nameof(IsFullscreenLayout), IsFullscreenLayout);
			RaisePropertyValueChanged(nameof(IsSideLayout),       IsSideLayout);
			RaisePropertyValueChanged(nameof(IsBlockingLayout),   IsBlockingLayout);
			RaisePropertyValueChanged(nameof(IsScreenInfoLayout), IsScreenInfoLayout);
		}

		private void RefreshLayoutDependencies()
		{
			var playerController = PlayerController.InstanceOrNull;
			if (!playerController.IsUnityNull())
			{
				if (IsEmotionLayout)
					playerController!.GestureHelper.OpenEmotionUI();
				else
					playerController!.GestureHelper.CloseEmotionUI();
			}

			var shareViewModel = ViewModelManager.Instance.GetOrAdd<ScreenShareViewModel>();
			shareViewModel.IsListEnabled = IsShareLayout;

			var meetingMinuteViewModel = ViewModelManager.Instance.GetOrAdd<MeetingMinutesRecordViewModel>();
			meetingMinuteViewModel.IsOpen = IsMeetingMinutesLayout;

			if (IsMyPadLayout)
				MyPadManager.Instance.OpenMyPad();
			else
				MyPadManager.Instance.CheckCloseMyPad();

			if (IsNotificationLayout)
				NotificationManager.Instance.NotificationPopUpOpen();
			else
				NotificationManager.Instance.ClosePopUp();

			if(IsUseInfoLayout)
				TutorialManager.Instance.TutorialPlay(eTutorialGroup.OFFICE__MEETING, true).Forget();
			else
				TutorialManager.Instance.TutorialClose();
		}

		/// <summary>
		/// 팝업 레이아웃은 동시에 하나만 열릴 수 있도록 한다.
		/// </summary>
		private void ClosePopupLayoutsOnOthers(string propertyName)
		{
			if (_isClosingPopupLayouts)
				return;

			_isClosingPopupLayouts = true;
			{
				if (propertyName != nameof(IsMicLayout)) IsMicLayout                   = false;
				if (propertyName != nameof(IsCameraLayout)) IsCameraLayout             = false;
				if (propertyName != nameof(IsMoreLayout)) IsMoreLayout                 = false;
				if (propertyName != nameof(IsExitLayout)) IsExitLayout                 = false;
				if (propertyName != nameof(IsEmotionLayout)) IsEmotionLayout           = false;
				if (propertyName != nameof(IsNotificationLayout)) IsNotificationLayout = false;
				if (propertyName != nameof(IsShareLayout)) IsShareLayout               = false;
			}
			_isClosingPopupLayouts = false;
			
		}

		/// <summary>
		/// 사이드 레이아웃 또는 알림 레이아웃은 동시에 하나만 열릴 수 있도록 한다.
		/// </summary>
		private void CloseSideLayoutsOnOthers(string propertyName)
		{
			if (_isClosingPopupLayouts)
				return;

			_isClosingPopupLayouts = true;
			{
				if (propertyName != nameof(IsChatLayout)) IsChatLayout                 = false;
				if (propertyName != nameof(IsUserLayout)) IsUserLayout                 = false;
				if (propertyName != nameof(IsInfoLayout)) IsInfoLayout                 = false;
				if (propertyName != nameof(IsNotificationLayout)) IsNotificationLayout = false;
			}
			_isClosingPopupLayouts = false;
		}

		/// <summary>
		/// 최소화 레이아웃은 격자 레이아웃과 동시에 열릴 수 없도록 한다.
		/// <br/>공유 화면 레이아웃이 닫힐 때 최소화 레이아웃도 닫히도록 한다.
		/// </summary>
		private void CloseGridOrMinimizedLayoutOnOthers(string propertyName)
		{
			switch (propertyName)
			{
				case nameof(IsMinimizedLayout) when IsMinimizedLayout:
					IsGridLayout = false;
					break;
				case nameof(IsGridLayout) when IsGridLayout:
					IsMinimizedLayout = false;
					break;
				case nameof(IsScreenLayout) when !IsScreenLayout:
					IsMinimizedLayout = false;
					break;
			}
		}

		private void OnCommandOpenProfile()
		{
			IsMoreLayout = false;
			UIManager.Instance.CreatePopup("UI_Connecting_UserProfilePopup", guiView =>
			{
				guiView.Show();
				var viewModel = guiView.ViewModelContainer.GetViewModel<MeetingRoomProfileViewModel>();
				viewModel.GUIView = guiView;
				viewModel.SetSelfProfile().Forget();
			}).Forget();
		}
	}
}
