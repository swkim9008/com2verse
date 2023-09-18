/*===============================================================
 * Product:		Com2Verse
 * File Name:	ToolBarViewModel.cs
 * Developer:	urun4m0r1
 * Date:		2022-12-13 14:57
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Com2Verse.Extension;
using Com2Verse.InputSystem;
using Com2Verse.Mice;
using Com2Verse.Notification;
using Com2Verse.PlayerControl;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("ToolBar")]
	public class ToolBarViewModel : ViewModelBase, IDisposable
	{
		[UsedImplicitly] public CommandHandler<bool> SetMicLayout          { get; }
		[UsedImplicitly] public CommandHandler<bool> SetCameraLayout       { get; }
		[UsedImplicitly] public CommandHandler<bool> SetPortalLayout       { get; }
		[UsedImplicitly] public CommandHandler<bool> SetParticipantLayout  { get; }
		[UsedImplicitly] public CommandHandler<bool> SetEmotionLayout      { get; }
		[UsedImplicitly] public CommandHandler<bool> SetMoreLayout         { get; }
		[UsedImplicitly] public CommandHandler<bool> SetNotificationLayout { get; }
		[UsedImplicitly] public CommandHandler<bool> SetMyPadLayout        { get; }
		[UsedImplicitly] public CommandHandler<bool> SetMinimapLayout      { get; }

		[UsedImplicitly] public CommandHandler ToggleMicLayout          { get; }
		[UsedImplicitly] public CommandHandler ToggleCameraLayout       { get; }
		[UsedImplicitly] public CommandHandler TogglePortalLayout       { get; }
		[UsedImplicitly] public CommandHandler ToggleParticipantLayout  { get; }
		[UsedImplicitly] public CommandHandler ToggleEmotionLayout      { get; }
		[UsedImplicitly] public CommandHandler ToggleMoreLayout         { get; }
		[UsedImplicitly] public CommandHandler ToggleNotificationLayout { get; }
		[UsedImplicitly] public CommandHandler ToggleMyPadLayout        { get; }
		[UsedImplicitly] public CommandHandler ToggleMinimapLayout      { get; }

		[UsedImplicitly] public CommandHandler ClosePopupLayouts { get; }

		private bool _isMicLayout;
		private bool _isCameraLayout;
		private bool _isPortalLayout;
		private bool _isParticipantLayout;
		private bool _isEmotionLayout;
		private bool _isMoreLayout;
		private bool _isNotificationLayout;
		private bool _isMyPadLayout;
		private bool _isMinimapLayout;

		private bool _isClosingPopupLayouts;

		public ToolBarViewModel()
		{
			SetMicLayout          = new CommandHandler<bool>(value => IsMicLayout          = value);
			SetCameraLayout       = new CommandHandler<bool>(value => IsCameraLayout       = value);
			SetPortalLayout       = new CommandHandler<bool>(value => IsPortalLayout       = value);
			SetParticipantLayout  = new CommandHandler<bool>(value => IsParticipantLayout  = value);
			SetEmotionLayout      = new CommandHandler<bool>(value => IsEmotionLayout      = value);
			SetMoreLayout         = new CommandHandler<bool>(value => IsMoreLayout         = value);
			SetNotificationLayout = new CommandHandler<bool>(value => IsNotificationLayout = value);
			SetMyPadLayout        = new CommandHandler<bool>(value => IsMyPadLayout        = value);
			SetMinimapLayout      = new CommandHandler<bool>(value => IsMinimapLayout      = value);

			ToggleMicLayout          = new CommandHandler(() => IsMicLayout          ^= true);
			ToggleCameraLayout       = new CommandHandler(() => IsCameraLayout       ^= true);
			TogglePortalLayout       = new CommandHandler(() => IsPortalLayout       ^= true);
			ToggleParticipantLayout  = new CommandHandler(() => IsParticipantLayout  ^= true);
			ToggleEmotionLayout      = new CommandHandler(() => IsEmotionLayout      ^= true);
			ToggleMoreLayout         = new CommandHandler(() => IsMoreLayout         ^= true);
			ToggleNotificationLayout = new CommandHandler(() => IsNotificationLayout ^= true);
			ToggleMyPadLayout        = new CommandHandler(() => IsMyPadLayout        ^= true);
			ToggleMinimapLayout      = new CommandHandler(() => IsMinimapLayout      ^= true);

			ClosePopupLayouts = new CommandHandler(() => ClosePopupLayoutsOnAny(string.Empty));

			MyPadManager.Instance.OnMyPadOpenedEvent += OnMyPadOpened;
			MyPadManager.Instance.OnMyPadClosedEvent += OnMyPadClosed;

			var notificationManager = NotificationManager.InstanceOrNull;
			if (notificationManager != null)
			{
				NotificationManager.Instance.OnNotificationPopUpOpenedEvent += OnNotificaionOpend;
				NotificationManager.Instance.OnNotificationPopUpClosedEvent += OnNotificaionClosed;
			}

			var playerController = PlayerController.InstanceOrNull;
			if (!playerController.IsUnityNull())
			{
				playerController!.GestureHelper.OnUIOpenedEvent += OnEmotionUIOpened;
				playerController.GestureHelper.OnUIClosedEvent  += OnEmotionUIClosed;
				playerController.OnMinimapUIOpenedEvent         += OnMinimapUIOpened;
				playerController.OnMinimapUIClosedEvent         += OnMinimapUIClosed;
			}
		}

		public void Dispose()
		{
			var myPadManager = MyPadManager.InstanceOrNull;
			if (myPadManager != null)
			{
				myPadManager.OnMyPadOpenedEvent -= OnMyPadOpened;
				myPadManager.OnMyPadClosedEvent -= OnMyPadClosed;
			}

			var notificationManager = NotificationManager.InstanceOrNull;
			if (notificationManager != null)
			{
				notificationManager.OnNotificationPopUpOpenedEvent -= OnNotificaionOpend;
				notificationManager.OnNotificationPopUpClosedEvent -= OnNotificaionClosed;
			}

			var playerController = PlayerController.InstanceOrNull;
			if (!playerController.IsUnityNull())
			{
				playerController!.GestureHelper.OnUIOpenedEvent -= OnEmotionUIOpened;
				playerController.GestureHelper.OnUIClosedEvent  -= OnEmotionUIClosed;
				playerController.OnMinimapUIOpenedEvent         -= OnMinimapUIOpened;
				playerController.OnMinimapUIClosedEvent         -= OnMinimapUIClosed;
			}
		}

		private void OnMyPadOpened()       => IsMyPadLayout = true;
		private void OnMyPadClosed()       => IsMyPadLayout = false;
		private void OnNotificaionOpend()  => IsNotificationLayout = true;
		private void OnNotificaionClosed() => IsNotificationLayout = false;
		private void OnEmotionUIOpened()   => IsEmotionLayout = true;
		private void OnEmotionUIClosed()   => IsEmotionLayout = false;
		private void OnMinimapUIOpened()   => IsMinimapLayout = true;
		private void OnMinimapUIClosed()   => IsMinimapLayout = false;

#region ViewModelProperties
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
		/// 포탈 바로가기 레이아웃이 열려있는지 여부
		/// </summary>
		public bool IsPortalLayout
		{
			get => _isPortalLayout;
			set => UpdateProperty(ref _isPortalLayout, value);
		}

		/// <summary>
		/// 참여자 목록 레이아웃이 열려있는지 여부
		/// </summary>
		public bool IsParticipantLayout
		{
			get => _isParticipantLayout;
			set => UpdateProperty(ref _isParticipantLayout, value);
		}

		/// <summary>
		/// 감정표현 레이아웃이 열려있는지 여부
		/// </summary>
		public bool IsEmotionLayout
		{
			get => _isEmotionLayout;
			set => UpdateProperty(ref _isEmotionLayout, value);
		}

		/// <summary>
		/// 더보기 레이아웃이 열려있는지 여부
		/// </summary>
		public bool IsMoreLayout
		{
			get => _isMoreLayout;
			set => UpdateProperty(ref _isMoreLayout, value);
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
		/// 미니맵 레이아웃이 열려있는지 여부
		/// </summary>
		public bool IsMinimapLayout
		{
			get => _isMinimapLayout;
			set => UpdateProperty(ref _isMinimapLayout, value);
		}

		/// <summary>
		/// 사이드 레이아웃이 열려있는지 여부 (=전체화면 레이아웃 사이즈 조절 여부)
		/// </summary>
		public bool IsSideLayout => false;

		/// <summary>
		/// 배경을 클릭해 닫을 수 있는 레이아웃이 열려있는지 여부 (= 투명 닫기 버튼 표시 여부)
		/// </summary>
		public bool IsBlockingLayout => IsMicLayout || IsCameraLayout || IsMoreLayout;
#endregion // ViewModelProperties

		private void UpdateProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "") where T : unmanaged, IConvertible
		{
			if (EqualityComparer<T>.Default.Equals(storage, value))
				return;

			SetProperty(ref storage, value, propertyName);

			ClosePopupLayoutsOnAny(propertyName);

			UpdateLayoutDependentProperty();
			RefreshLayoutDependencies();
		}

		private void UpdateLayoutDependentProperty()
		{
			InvokePropertyValueChanged(nameof(IsSideLayout),       IsSideLayout);
			InvokePropertyValueChanged(nameof(IsBlockingLayout),   IsBlockingLayout);
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
				if (IsMinimapLayout)
					playerController.OnMiniMapOpen();
				else
					playerController.OnMinimapClose();
			}

			if (IsMicLayout)
			{
				var audioRecordingQuality = ViewModelManager.Instance.Get<AudioRecordingQualityViewModel>();
				audioRecordingQuality?.InitVariables();
			}

			if (IsMyPadLayout)
				MyPadManager.Instance.OpenMyPad();
			else
				MyPadManager.Instance.CheckCloseMyPad();

			if (IsNotificationLayout)
				NotificationManager.Instance.NotificationPopUpOpen();
			else
				NotificationManager.Instance.ClosePopUp();

			// TODO: ParticipantLayout 닫기, 닫기/열기 콜백 구현
			if (IsParticipantLayout)
				MiceParticipantListManager.Instance.ShowAsNormal();
			else
				MiceParticipantListManager.Instance.Hide();

			// TODO: PortalLayout 동작 구현
		}

		/// <summary>
		/// 팝업 레이아웃은 다른 동작 시 닫히도록 한다.
		/// </summary>
		private void ClosePopupLayoutsOnAny(string propertyName)
		{
			if (_isClosingPopupLayouts)
				return;

			_isClosingPopupLayouts = true;
			{
				if (propertyName != nameof(IsMicLayout)) IsMicLayout                   = false;
				if (propertyName != nameof(IsCameraLayout)) IsCameraLayout             = false;
				if (propertyName != nameof(IsMoreLayout)) IsMoreLayout                 = false;
                if (propertyName != nameof(IsParticipantLayout)) IsParticipantLayout   = false;
                if (propertyName != nameof(IsEmotionLayout)) IsEmotionLayout           = false;
                if (propertyName != nameof(IsNotificationLayout)) IsNotificationLayout = false;
                if (propertyName != nameof(IsMinimapLayout)) IsMinimapLayout           = false;
            }
			_isClosingPopupLayouts = false;
		}


	}
}

