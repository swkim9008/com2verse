/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationProfileIconViewModel.cs
* Developer:	jhkim
* Date:			2022-09-29 13:15
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Organization;
using Com2Verse.UserState;
using Cysharp.Threading.Tasks;
using UnityEngine;
using MemberIdType = System.Int64;

namespace Com2Verse.UI
{
	[ViewModelGroup("Organization")]
	public sealed class OrganizationProfileIconViewModel : ViewModelBase
	{
		// !!! 새로운 항목은 마지막에 추가, 순서변경 X !!!
		public enum eProfileIconState
		{
			NONE,
			CALL,
			CALL_FAIL,
			CALL_MISS,
			CALL_REFUSE,
			CHANNEL,
			INVITE,
		}

		// !!! 새로운 항목은 마지막에 추가, 순서변경 X !!!
		public enum eProfileBadgeState
		{
			GREEN,
			RED,
			ORANGE,
			GRAY,
		}
#region Variables
		private bool _showDebug;
		private Texture _profileImage;
		private eProfileIconState _currentState;
		private eProfileBadgeState _currentBadgeState;
		private bool _isShowBadge = false;
		private Action _onClick;
		private MemberModel _memberModel;
#endregion // Variables

#region Properties
		public bool ShowDebug
		{
			get => _showDebug;
			set
			{
				_showDebug = value;
				InvokePropertyValueChanged(nameof(ShowDebug), value);
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
		public eProfileIconState CurrentState
		{
			get => _currentState;
			set
			{
				_currentState = value;
				InvokePropertyValueChanged(nameof(CurrentState), value);
			}
		}

		public eProfileBadgeState CurrentBadgeState
		{
			get => _currentBadgeState;
			set
			{
				_currentBadgeState = value;
				InvokePropertyValueChanged(nameof(CurrentBadgeState), value);
			}
		}

		public bool IsShowBadge
		{
			get => _isShowBadge;
			set
			{
				_isShowBadge = value;
				if (value)
					AddUserStateUpdateEvent();
				else
					RemoveUserStateUpdateEvent();
				InvokePropertyValueChanged(nameof(IsShowBadge), value);
			}
		}
		public CommandHandler Click        { get; }
		public CommandHandler ToggleDebug  { get; }
		public CommandHandler None         { get; }
		public CommandHandler Call         { get; }
		public CommandHandler CallFail     { get; }
		public CommandHandler CallMiss     { get; }
		public CommandHandler CallRefuse   { get; }
		public CommandHandler Channel      { get; }
		public CommandHandler Invite       { get; }
		public CommandHandler BadgeGreen   { get; }
		public CommandHandler BadgeRed     { get; }
		public CommandHandler BadgeOragnge { get; }
		public CommandHandler BadgeGray    { get; }
#endregion // Properties

#region Initialize
		public OrganizationProfileIconViewModel() : this(null) { }
		public OrganizationProfileIconViewModel(Action onClick)
		{
			_onClick = onClick;
			Click = new CommandHandler(OnClick);

			ToggleDebug = new CommandHandler(OnToggleDebug);

			None = new CommandHandler(OnNone);
			Call = new CommandHandler(OnCall);
			CallFail = new CommandHandler(OnCallFail);
			CallMiss = new CommandHandler(OnCallMiss);
			CallRefuse = new CommandHandler(OnCallRefuse);
			Channel = new CommandHandler(OnChannel);
			Invite = new CommandHandler(OnInvite);

			BadgeGreen = new CommandHandler(OnBadgeGreen);
			BadgeRed = new CommandHandler(OnBadgeRed);
			BadgeOragnge = new CommandHandler(OnBadgeOrange);
			BadgeGray = new CommandHandler(OnBadgeGray);

			AddUserStateUpdateEvent();
		}

		public OrganizationProfileIconViewModel(MemberIdType memberId, Action onClick) : this(onClick) => SetInfo(memberId);
		public OrganizationProfileIconViewModel(MemberModel memberModel, Action onClick) : this(onClick) => SetInfo(memberModel);

		public override void OnRelease()
		{
			base.OnRelease();
			_memberModel = null;
			RemoveUserStateUpdateEvent();
		}
#endregion // Initialize

#region Binding Events
		private void OnClick() => _onClick?.Invoke();

		private void OnToggleDebug() => ShowDebug = !ShowDebug;
		private void OnNone() => CurrentState = eProfileIconState.NONE;
		private void OnCall() => CurrentState = eProfileIconState.CALL;
		private void OnCallFail() => CurrentState = eProfileIconState.CALL_FAIL;
		private void OnCallMiss() => CurrentState = eProfileIconState.CALL_MISS;
		private void OnCallRefuse() => CurrentState = eProfileIconState.CALL_REFUSE;
		private void OnChannel() => CurrentState = eProfileIconState.CHANNEL;
		private void OnInvite() => CurrentState = eProfileIconState.INVITE;
		private void OnBadgeGreen() => CurrentBadgeState = eProfileBadgeState.GREEN;
		private void OnBadgeRed() => CurrentBadgeState = eProfileBadgeState.RED;
		private void OnBadgeOrange() => CurrentBadgeState = eProfileBadgeState.ORANGE;
		private void OnBadgeGray() => CurrentBadgeState = eProfileBadgeState.GRAY;
#endregion // Binding Events

#region UI
		public void SetInfo(MemberIdType memberId)
		{
			var employee = DataManager.Instance.GetMember(memberId);
			SetInfo(employee);
		}

		public void SetInfo(MemberModel memberModel)
		{
			_memberModel = memberModel;
			RefreshInfo(memberModel);
		}
		private void RefreshInfo(MemberModel memberModel)
		{
			if (memberModel == null)
			{
				ProfileImage = null;
				CurrentState = eProfileIconState.NONE;
				RefreshBadgeState();
				return;
			}

			// TODO : 프로필 상태 처리
			CurrentState = eProfileIconState.NONE;
			RefreshBadgeState();

			if (!string.IsNullOrEmpty(memberModel.Member.PhotoPath))
			{
				Util.DownloadTexture(memberModel.Member.PhotoPath, (success, texture) =>
				{
					if (success)
						ProfileImage = texture;
				}).Forget();
			}
		}

		private void RefreshBadgeState()
		{
			if (_memberModel == null) return;

			var state = Info.Instance.GetUserState(_memberModel.Member.AccountId);
			RefreshBadgeState(state);
		}
		private void RefreshBadgeState(Info.eUserState state)
		{
			CurrentBadgeState = GetBadgeStateFromUserState(state);
		}
#endregion // UI

		public void SetOnClick(Action onClick) => _onClick = onClick;

#region User State
		private eProfileBadgeState GetBadgeStateFromUserState(Info.eUserState userState) => userState switch
		{
			Info.eUserState.OFF_LINE => eProfileBadgeState.GRAY,
			Info.eUserState.ON_LINE  => eProfileBadgeState.GREEN,
			Info.eUserState.BUSY     => eProfileBadgeState.RED,
			_                        => eProfileBadgeState.GRAY,
		};

		private void OnUserStateUpdate(long accountId, Info.eUserState state)
		{
			if (_memberModel == null)
			{
				if (Info.InstanceExists)
					Info.Instance.OnUserStateUpdated -= OnUserStateUpdate;
				return;
			}
			if (_memberModel.Member.AccountId != accountId) return;

			RefreshBadgeState(state);
		}

		private void AddUserStateUpdateEvent()
		{
			if (Info.InstanceExists)
				Info.Instance.OnUserStateUpdated += OnUserStateUpdate;
			RefreshBadgeState();
		}

		private void RemoveUserStateUpdateEvent()
		{
			if (Info.InstanceExists)
				Info.Instance.OnUserStateUpdated -= OnUserStateUpdate;
		}
#endregion // User State
	}
}
