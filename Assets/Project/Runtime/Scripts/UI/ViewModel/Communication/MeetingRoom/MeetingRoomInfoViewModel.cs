/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingRoomInfoViewModel.cs
* Developer:	urun4m0r1
* Date:			2022-06-20 12:45
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Chat;
using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Com2Verse.Utils;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;
namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("MeetingRoom")]
	public sealed class MeetingRoomInfoViewModel : InitializableViewModel<MeetingInfoType>
	{
		[UsedImplicitly] public CommandHandler CommandCopyMeetingCode { get; }
		[UsedImplicitly] public CommandHandler CommandCopyInviteCode  { get; }
		[UsedImplicitly] public CommandHandler CommandOpenCreditPopup { get; }

		private DateTime _endDateTime;
		private TimeSpan _remainingTime;
		private string   _remainingTimeText = string.Empty;
		private bool     _sendRemainToast   = false;
		private UIInfo   _uiInfo = new();

		public UIInfo UIInfoScreenShareViewModel
		{
			get
			{
				if (_uiInfo == null)
				{
					InitScreenShareInfo();
				}

				return _uiInfo!;
			}
		}

		public MeetingRoomInfoViewModel()
		{
			CommandCopyMeetingCode = new CommandHandler(OnClickCopyMeetingCode);
			CommandCopyInviteCode  = new CommandHandler(OnClickCopyInviteCode);
			CommandOpenCreditPopup = new CommandHandler(OnClickOpenCreditPopup);

			ResetRemainingTime();
			InitScreenShareInfo();

			ChatManager.Instance.OnTimeExtensionNotify += OnMeetingExtensionTimeNotify;
		}

		private void InitScreenShareInfo()
		{
			var titleString   = Define.String.Information_Title_ScreenSharing;
			var messageString = Define.String.Information_Text_ScreenSharing;

			_uiInfo = new UIInfo(true);
			_uiInfo.Set(UIInfo.eInfoType.INFO_TYPE_SCREEN_SHARING, UIInfo.eInfoLayout.INFO_LAYOUT_UP, titleString, messageString);
		}

		private void OnClickCopyMeetingCode()
		{
			GUIUtility.systemCopyBuffer = MeetingCode;
			UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_Common_UserList_Invitation_SpaceCodeCopied_Toast"));
		}

		private void OnClickCopyInviteCode()
		{
			GUIUtility.systemCopyBuffer = Localization.Instance.GetString("UI_MeetingRoom_Common_ConnectingInfo_Invitation_Content_Text",
			                                                              MeetingReservationProvider.GetOrganizerName(),
			                                                              GetMeetingDateString(), MeetingName, MeetingCode);
			UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_Common_UserList_Invitation_InvitationCopied_Toast"));
		}

		private void OnClickOpenCreditPopup()
		{
			if (!MeetingReservationProvider.IsOrganizer(User.Instance.CurrentUserData.ID))
			{
				UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_OtherMenu_Stt_RecordingFailed_Authority_Toast"));
				return;
			}
			UIManager.Instance.CreatePopup("UI_Credit_AdditionalPayment_Popup", (guiView) =>
			{
				guiView.Show();

				var meetingCreditViewModel = guiView.ViewModelContainer.GetViewModel<MeetingRoomCreditViewModel>();
				meetingCreditViewModel.SetCreditPopup(guiView);
			}).Forget();
		}

		private string GetMeetingDateString() =>
			ZString.Format("{0}.{1:00}.{2:00} {3:00}:{4:00} ~ {5}.{6:00}.{7:00} {8:00}:{9:00}", Value?.StartDateTime.Year, Value?.StartDateTime.Month, Value?.StartDateTime.Day,
			               Value?.StartDateTime.Hour, Value?.StartDateTime.Minute, Value?.EndDateTime.Year, Value?.EndDateTime.Month,
			               Value?.EndDateTime.Day, Value?.EndDateTime.Hour, Value?.EndDateTime.Minute);

		private void ResetRemainingTime()
		{
			_endDateTime      = System.DateTime.MinValue;
			_remainingTime    = TimeSpan.Zero;
			RemainingTimeText = "00:00";
		}

		protected override void OnPrevValueUnassigned(MeetingInfoType value)
		{
			UIManager.InstanceOrNull?.RemoveUpdateListener(OnUpdate);
			ResetRemainingTime();
		}

		protected override void OnCurrentValueAssigned(MeetingInfoType value)
		{
			if (value.EndDateTime == null)
			{
				C2VDebug.LogError(nameof(MeetingRoomInfoViewModel), "Invalid Protobuf: EndDateTime is null");
				return;
			}

			_endDateTime = value.EndDateTime;

			UIManager.Instance.AddUpdateListener(OnUpdate);
			OnUpdate();
		}

		private void OnUpdate()
		{
			_remainingTime    = _endDateTime - System.DateTime.Now;
			if (Math.Abs(_remainingTime.TotalSeconds - 300) < 1)
			{
				if (!_sendRemainToast)
				{
					UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_MeetingRoom_Time_EndTime_Text"));
					_sendRemainToast = true;
				}
			}

			if (_remainingTime.TotalMinutes < 0)
			{
				RemainingTimeText = ZString.Format("<color=#FF0000>{0:00}:{1:00}</color>", 0, 0);
				return;
			}

			if (_remainingTime.TotalMinutes < 5)
			{
				RemainingTimeText = ZString.Format("<color=#FF0000>{0:00}:{1:00}</color>", _remainingTime.TotalHours, _remainingTime.Minutes);
			}
			else
			{
				RemainingTimeText = _remainingTime.TotalHours switch
				{
					< 100  => ZString.Format("{0:00}:{1:00}",   (int)_remainingTime.TotalHours, _remainingTime.Minutes),
					< 1000 => ZString.Format("{0:000}:{1:00}",  (int)_remainingTime.TotalHours, _remainingTime.Minutes),
					_      => ZString.Format("{0:0000}:{1:00}", (int)_remainingTime.TotalHours, _remainingTime.Minutes)
				};
			}
		}

		public override void RefreshViewModel()
		{
			InvokePropertyValueChanged(nameof(MeetingName), MeetingName);
			InvokePropertyValueChanged(nameof(Capacity), Capacity);
			InvokePropertyValueChanged(nameof(Description), Description);
			InvokePropertyValueChanged(nameof(StartDateTime), StartDateTime);
			InvokePropertyValueChanged(nameof(EndDateTime), EndDateTime);
			InvokePropertyValueChanged(nameof(DateTime), DateTime);

			InvokePropertyValueChanged(nameof(IsVisibleUserPermissionSettingToggle), IsVisibleUserPermissionSettingToggle);
			InvokePropertyValueChanged(nameof(IsVisibleUserInvitationToggle), IsVisibleUserInvitationToggle);
		}

		public override void OnLanguageChanged()
		{
			base.OnLanguageChanged();
			InitScreenShareInfo();
		}

		private void OnMeetingExtensionTimeNotify()
		{
			Commander.Instance.RequestMeetingInfoAsync(MeetingReservationProvider.EnteredMeetingInfo.MeetingId, response =>
			{
				MeetingReservationProvider.SetMeetingInfo(response.Value.Data);
				_endDateTime     = response.Value.Data.EndDateTime;
				_sendRemainToast = false;
				RefreshViewModel();
			}).Forget();
		}

#region ViewModelProperties
		public string MeetingName        => Value?.MeetingName               ?? "{MeetingName}";
		public int    Capacity           => Value?.MaxUsersLimit             ?? 30;
		public string Description        => Value?.MeetingDescription        ?? "{Description}";
		public string StartDateTime      => Value?.StartDateTime.ToString() ?? "{StartDateTime}";
		public string EndDateTime        => Value?.EndDateTime.ToString()   ?? "{EndDateTime}";
		public string MeetingDescription => Value?.MeetingDescription        ?? "{MeetingDescription}";

		public string MeetingAccess => Value?.PublicYn == "Y"
			? Localization.Instance.GetString("UI_ConnectingApp_Card_Privacy_Text_1")
			: Localization.Instance.GetString("UI_ConnectingApp_Card_Privacy_Text_2");

		public string? MeetingCode => Value?.MeetingCode;

		public string DateTime
		{
			get
			{
				var start = Value?.StartDateTime;
				var end   = Value?.EndDateTime;

				return Localization.InstanceOrNull?.GetString(
					Define.TextKey.UI_MeetingRoomCommon_DateTime_Format
				  , start?.Year, start?.Month, start?.Day, start?.Hour, start?.Minute, end?.Hour, end?.Minute)!;
			}
		}

		public string RemainingTimeText
		{
			get => _remainingTimeText;
			set => SetProperty(ref _remainingTimeText, value);
		}

		//Setting permissions
		public bool IsVisibleUserPermissionSettingToggle => MeetingReservationProvider.IsOrganizer(Value?.MeetingMembers);
		public bool IsVisibleUserInvitationToggle        => MeetingReservationProvider.IsOrganizer(Value?.MeetingMembers);
#endregion // ViewModelProperties

#region IDisposable
		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				if (ChatManager.InstanceExists)
					ChatManager.Instance.OnTimeExtensionNotify -= OnMeetingExtensionTimeNotify;
			}

			// Uncomment this line in inherited class to implement standard disposing pattern.
			base.Dispose(disposing);

			_disposed = true;
		}
#endregion
	}
}
