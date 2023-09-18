/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingRoomExitViewModel.cs
* Developer:	urun4m0r1
* Date:			2023-04-06 15:05
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Net;
using Com2Verse.Communication;
using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Com2Verse.PlayerControl;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Protocols.OfficeMeeting;
using User = Com2Verse.Network.User;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("MeetingRoom")]
	public sealed class MeetingRoomExitViewModel : ViewModelBase, IDisposable
	{
		[UsedImplicitly] public CommandHandler ExitRoom    { get; }
		[UsedImplicitly] public CommandHandler LeaveRoom   { get; }
		[UsedImplicitly] public CommandHandler DestroyRoom { get; }

		private DateTime _lastLeaveRoomRequestTime   = DateTime.MinValue;
		private DateTime _lastDestroyRoomRequestTime = DateTime.MinValue;

		private static readonly TimeSpan LeaveRoomRequestDelay   = TimeSpan.FromSeconds(3);
		private static readonly TimeSpan DestroyRoomRequestDelay = TimeSpan.FromSeconds(3);

		private bool _isExitLayoutAvailable;
		private bool _canDestroyRoom;

		public MeetingRoomExitViewModel()
		{
			ExitRoom    = new CommandHandler(OnExitRoom);
			LeaveRoom   = new CommandHandler(OnLeaveRoom);
			DestroyRoom = new CommandHandler(OnDestroyRoom);


			MeetingReservationProvider.OnMeetingInfoChanged                += OnMeetingInfoChanged;
			MeetingReservationProvider.OnDisconnectRequestFromMediaChannel += ReceivedDisconnectRequest;
			OnSelfAuthorityChanged();
		}

		public void Dispose()
		{
			MeetingReservationProvider.OnMeetingInfoChanged                -= OnMeetingInfoChanged;
			MeetingReservationProvider.OnDisconnectRequestFromMediaChannel -= ReceivedDisconnectRequest;
		}

		private void OnMeetingInfoChanged()
		{
			OnSelfAuthorityChanged();
		}

		private void OnSelfAuthorityChanged()
		{
			var isOrganizer = MeetingReservationProvider.IsOrganizer(User.Instance.CurrentUserData.ID);

			IsExitLayoutAvailable = isOrganizer;
			CanDestroyRoom        = isOrganizer;

			if (!IsExitLayoutAvailable)
				CloseExitLayout();
		}

		private void ReceivedDisconnectRequest()
		{
			C2VDebug.Log("ReceivedDisconnectRequest");
			ChannelManager.Instance.LeaveAllChannels();
			PlayerController.Instance.SetStopAndCannotMove(true);
			User.Instance.DiscardPacketBeforeStandBy();
		}

#region ViewModelProperties
		public bool IsExitLayoutAvailable
		{
			get => _isExitLayoutAvailable;
			set => SetProperty(ref _isExitLayoutAvailable, value);
		}

		public bool CanDestroyRoom
		{
			get => _canDestroyRoom;
			set => SetProperty(ref _canDestroyRoom, value);
		}
#endregion // ViewModelProperties

		private void OnExitRoom()
		{
			if (IsExitLayoutAvailable)
				ToggleExitLayout();
			else
				LeaveRoomImpl();
		}

		private void OnLeaveRoom()
		{
			LeaveRoomImpl();
		}

		private void OnDestroyRoom()
		{
			UIManager.Instance.ShowPopupYesNo(
				Localization.Instance.GetString("UI_Common_Popup_Title_Text")
			  , Localization.Instance.GetString("UI_MeetingRoom_ToolBar_Exit_DestroyConfirm_Popup_Text")
			  , OnDestroyRoomConfirm,
				yes: Localization.Instance.GetString("UI_Common_Btn_Yes"), no: Localization.Instance.GetString("UI_Common_Btn_No"));
		}

		private void OnDestroyRoomConfirm(GUIView _)
		{
			DestroyRoomImpl();
		}

		private void ToggleExitLayout()
		{
			ViewModelManager.Instance.GetOrAdd<MeetingRoomLayoutViewModel>().IsExitLayout ^= true;
		}

		private void CloseExitLayout()
		{
			ViewModelManager.Instance.GetOrAdd<MeetingRoomLayoutViewModel>().IsExitLayout = false;
		}

		private void LeaveRoomImpl()
		{
			if (!CheckRequestDelay(ref _lastLeaveRoomRequestTime, LeaveRoomRequestDelay))
				return;

			RequestLeaveConnecting();
		}

		private void DestroyRoomImpl()
		{
			if (!CheckRequestDelay(ref _lastDestroyRoomRequestTime, DestroyRoomRequestDelay))
				return;

			if (!CanDestroyRoom)
			{
				C2VDebug.LogErrorMethod(nameof(MeetingRoomExitViewModel), "You don't have permission to destroy room.");
				return;
			}

			RequestConnectingEnd();
		}

		private static bool CheckRequestDelay(ref DateTime lastRequestTime, TimeSpan requestDelay)
		{
			var now = DateTime.Now;
			if (now - lastRequestTime < requestDelay)
				return false;

			lastRequestTime = now;
			return true;
		}

#region Request
		private void RequestConnectingEnd()
		{
			if (MeetingReservationProvider.EnteredMeetingInfo == null)
			{
				C2VDebug.LogErrorMethod(nameof(MeetingRoomExitViewModel), "EnteredMeetingInfo is null.");
				return;
			}
			Commander.Instance.RequestMeetingEndAsync(MeetingReservationProvider.EnteredMeetingInfo.MeetingId).Forget();
		}

		private void RequestLeaveConnecting()
		{
			if (MeetingReservationProvider.EnteredMeetingInfo == null)
			{
				C2VDebug.LogErrorMethod(nameof(MeetingRoomExitViewModel), "EnteredMeetingInfo is null.");
				return;
			}
			// 이동중 OSR/Input 막기
			PlayerController.Instance.SetStopAndCannotMove(true);
			User.Instance.DiscardPacketBeforeStandBy();
			Commander.Instance.RequestRoomLeaveAsync(MeetingReservationProvider.EnteredMeetingInfo.MeetingId, MeetingReservationProvider.RoomId).Forget();
		}
#endregion
	}
}
