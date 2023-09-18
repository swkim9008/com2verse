/*===============================================================
 * Product:		Com2Verse
 * File Name:	MeetingRoomUserListViewModel.cs
 * Developer:	urun4m0r1
 * Date:		2022-12-13 14:57
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using JetBrains.Annotations;
using Protocols.OfficeMeeting;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("Communication")]
	public partial class MeetingRoomUserListViewModel : ViewModelBase
	{
		[UsedImplicitly] public CommandHandler<bool> SetJoinUserLayout       { get; }
		[UsedImplicitly] public CommandHandler<bool> SetWaitingUserLayout    { get; }
		[UsedImplicitly] public CommandHandler<bool> SetOrganizerFold        { get; }
		[UsedImplicitly] public CommandHandler<bool> SetPresenterFold        { get; }
		[UsedImplicitly] public CommandHandler<bool> SetMemberFold           { get; }
		[UsedImplicitly] public CommandHandler<bool> SetWaitInviteAcceptFold { get; }
		[UsedImplicitly] public CommandHandler<bool> SetNotJoinedFold        { get; }

		[UsedImplicitly] public CommandHandler ToggleJoinUserLayout       { get; }
		[UsedImplicitly] public CommandHandler ToggleWaitingUserLayout    { get; }
		[UsedImplicitly] public CommandHandler ToggleOrganizerFold        { get; }
		[UsedImplicitly] public CommandHandler TogglePresenterFold        { get; }
		[UsedImplicitly] public CommandHandler ToggleMemberFold           { get; }
		[UsedImplicitly] public CommandHandler ToggleWaitInviteAcceptFold { get; }
		[UsedImplicitly] public CommandHandler ToggleNotJoinedFold        { get; }

		private bool _isJoinUserLayout       = true;
		private bool _isWaitingUserLayout    = false;
		private bool _isOrganizerFold        = false;
		private bool _isPresenterFold        = false;
		private bool _isMemberFold           = false;
		private bool _isWaitInviteAcceptFold = false;
		private bool _isNotJoinedFold        = false;
		private bool _isUserLayout;

		public MeetingRoomUserListViewModel()
		{
			SetJoinUserLayout       = new CommandHandler<bool>(value => IsJoinUserLayout       = value);
			SetWaitingUserLayout    = new CommandHandler<bool>(value => IsWaitingUserLayout    = value);
			SetOrganizerFold        = new CommandHandler<bool>(value => IsOrganizerFold        = value);
			SetPresenterFold        = new CommandHandler<bool>(value => IsPresenterFold        = value);
			SetMemberFold           = new CommandHandler<bool>(value => IsMemberFold           = value);
			SetWaitInviteAcceptFold = new CommandHandler<bool>(value => IsWaitInviteAcceptFold = value);
			SetNotJoinedFold        = new CommandHandler<bool>(value => IsNotJoinedFold        = value);

			ToggleJoinUserLayout       = new CommandHandler(() => IsJoinUserLayout       = !IsJoinUserLayout);
			ToggleWaitingUserLayout    = new CommandHandler(() => IsWaitingUserLayout    = !IsWaitingUserLayout);
			ToggleOrganizerFold        = new CommandHandler(() => IsOrganizerFold        = !IsOrganizerFold);
			TogglePresenterFold        = new CommandHandler(() => IsPresenterFold        = !IsPresenterFold);
			ToggleMemberFold           = new CommandHandler(() => IsMemberFold           = !IsMemberFold);
			ToggleWaitInviteAcceptFold = new CommandHandler(() => IsWaitInviteAcceptFold = !IsWaitInviteAcceptFold);
			ToggleNotJoinedFold        = new CommandHandler(() => IsNotJoinedFold        = !IsNotJoinedFold);
		}

#region ViewModelProperties
		public bool IsJoinUserLayout
		{
			get => _isJoinUserLayout;
			set
			{
				var prevValue = _isJoinUserLayout;
				if (prevValue == value)
					return;
				
				SetProperty(ref _isJoinUserLayout, value);
				IsWaitingUserLayout = !value;
			}
		}

		public bool IsWaitingUserLayout
		{
			get => _isWaitingUserLayout;
			set
			{
				var prevValue = _isWaitingUserLayout;
				if (prevValue == value)
					return;
				if (value)
				{
					if (MeetingReservationProvider.IsGuest())
					{
						UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_Guest_MeetingRoom_GuestNotUse_Toast"));
						return;
					}

					RequestMeetingInfo();
				}

				SetProperty(ref _isWaitingUserLayout, value);
				IsJoinUserLayout = !value;
			}
		}

		public bool IsOrganizerFold
		{
			get => _isOrganizerFold;
			set => SetProperty(ref _isOrganizerFold, value);
		}

		public bool IsPresenterFold
		{
			get => _isPresenterFold;
			set => SetProperty(ref _isPresenterFold, value);
		}

		public bool IsMemberFold
		{
			get => _isMemberFold;
			set => SetProperty(ref _isMemberFold, value);
		}

		public bool IsWaitInviteAcceptFold
		{
			get => _isWaitInviteAcceptFold;
			set => SetProperty(ref _isWaitInviteAcceptFold, value);
		}

		public bool IsNotJoinedFold
		{
			get => _isNotJoinedFold;
			set => SetProperty(ref _isNotJoinedFold, value);
		}

		public bool IsUserLayout
		{
			get => _isUserLayout;
			set
			{
				if (value)
				{
					IsJoinUserLayout    = true;
					IsWaitingUserLayout = false;
				}
				_isUserLayout = value;
			}
		}

#endregion // ViewModelProperties
	}
}
