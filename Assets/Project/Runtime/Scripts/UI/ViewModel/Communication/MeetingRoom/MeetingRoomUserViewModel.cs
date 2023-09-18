/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingRoomUserViewModel.cs
* Developer:	tlghks1009
* Date:			2022-11-08 15:06
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Com2Verse.AvatarAnimation;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Com2Verse.Project.Animation;
using Com2Verse.WebApi.Service;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using User = Com2Verse.Network.User;
using MeetingUserType = Com2Verse.WebApi.Service.Components.MeetingMemberEntity;
using AuthorityCode = Com2Verse.WebApi.Service.Components.AuthorityCode;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("Communication")]
	public partial class MeetingRoomUserViewModel : ViewModelBase, INestedViewModel, IDisposable
	{
#region INestedViewModel
		public IList<ViewModel> NestedViewModels { get; } = new List<ViewModel>();

		public CommunicationUserViewModel UserViewModel { get; }
#endregion // INestedViewModel

		public event Action<CommunicationUserViewModel>?         OnClickProfile;
		public event Action<long, AuthorityCode, AuthorityCode>? OnClickAuthorityChange;

		// ReSharper disable InconsistentNaming
		[UsedImplicitly] public CommandHandler      Command_PermissionChangeDropDownButtonClick { get; }
		[UsedImplicitly] public CommandHandler<int> Command_PermissionChangeButtonClick         { get; }
		[UsedImplicitly] public CommandHandler      CommandForcedOutButtonClick                 { get; }
		[UsedImplicitly] public CommandHandler      CommandProfileButtonClick                   { get; }

		[UsedImplicitly] public CommandHandler CommandCloseDropDown { get; }
		// ReSharper restore InconsistentNaming


		private MeetingUserType? _meetingUserInfo;

		private bool    _isVisibleDropDown;
		private bool    _isOrganizerDropDown;
		private bool    _isEmotionActive;
		private bool    _isHandsUp;
		private bool    _isOtherEmotion;
		private Sprite? _emotionSprite;
		private bool    _isViewEmotion;
		private bool    _isAuthorityLayoutOpen;

		private CancellationTokenSource? _cts = new CancellationTokenSource();

		private readonly int _emoticonActiveTime = 5000;

		public MeetingRoomUserViewModel(CommunicationUserViewModel userViewModel)
		{
			MeetingReservationProvider.OnMeetingInfoChanged += OnMeetingInfoChanged;

			UserViewModel = userViewModel;
			NestedViewModels.Add(UserViewModel);

			Command_PermissionChangeDropDownButtonClick = new CommandHandler(OnCommand_PermissionChangeDropDownButtonClicked);
			Command_PermissionChangeButtonClick         = new CommandHandler<int>(OnCommand_PermissionChangeButtonClicked);
			CommandForcedOutButtonClick                 = new CommandHandler(OnCommand_ForcedOutButtonClicked);
			CommandProfileButtonClick                   = new CommandHandler(OnCommand_ProfileButtonClicked);
			CommandCloseDropDown                        = new CommandHandler(CloseDropDown);

			var activeObject = MapController.Instance.GetObjectByUserID(userViewModel.UserId) as ActiveObject;
			if (!activeObject.IsUnityNull())
			{
				if (!activeObject!.BaseBodyTransform.IsUnityNull())
				{
					var animatorController = activeObject.BaseBodyTransform.GetComponent<AvatarAnimatorController>();
					if (!animatorController.IsUnityNull())
					{
						animatorController!.OnSetEmotion -= ActiveEmotionCallback;
						animatorController.OnSetEmotion += ActiveEmotionCallback;
					}
				}
			}

			RefreshUserInfo().Forget();

			IsOtherEmotion  = false;
			IsHandsUp       = false;
			IsEmotionActive = false;
		}

		public void Dispose()
		{
			MeetingReservationProvider.OnMeetingInfoChanged -= OnMeetingInfoChanged;

			var mapController = MapController.InstanceOrNull;
			if (!mapController.IsReferenceNull())
			{
				var mapObject = mapController!.GetObjectByUserID(UserViewModel.UserId);
				if (!mapObject.IsUnityNull())
				{
					var avatarAnimatorController = mapObject!.GetComponent<AvatarAnimatorController>();
					if (!avatarAnimatorController.IsReferenceNull())
						avatarAnimatorController!.OnSetEmotion -= ActiveEmotionCallback;
				}
			}

			if (_cts != null)
			{
				_cts.Cancel();
				_cts.Dispose();
				_cts = null;
			}
		}

		private void OnMeetingInfoChanged()
		{
			InvokePropertyValueChanged(nameof(ShowOrganizerControl), ShowOrganizerControl);
		}

#region Properties
		public bool IsVisibleDropDown
		{
			get => _isVisibleDropDown;
			set => SetProperty(ref _isVisibleDropDown, value);
		}

		public bool IsOrganizerDropDown
		{
			get => _isOrganizerDropDown;
			set => SetProperty(ref _isOrganizerDropDown, value);
		}

		public bool IsEmotionActive
		{
			get => _isEmotionActive;
			set => SetProperty(ref _isEmotionActive, value);
		}

		public bool IsHandsUp
		{
			get => _isHandsUp;
			set => SetProperty(ref _isHandsUp, value);
		}

		public bool IsOtherEmotion
		{
			get => _isOtherEmotion;
			set => SetProperty(ref _isOtherEmotion, value);
		}

		public Sprite? EmotionSprite
		{
			get => _emotionSprite;
			set => SetProperty(ref _emotionSprite, value);
		}

		public bool IsAuthorityLayoutOpen
		{
			get => _isAuthorityLayoutOpen;
			set => SetProperty(ref _isAuthorityLayoutOpen, value);
		}

		public bool IsPermissionParticipant => _meetingUserInfo?.AuthorityCode is AuthorityCode.Participant;
		public bool IsPermissionPresenter   => _meetingUserInfo?.AuthorityCode is AuthorityCode.Presenter;
		public bool IsVisibleModerator      => false;
		public bool IsPermissionOrganizer   => _meetingUserInfo?.AuthorityCode is AuthorityCode.Organizer;

		[UsedImplicitly] public bool ShowOrganizerControl => MeetingReservationProvider.IsOrganizer(User.Instance.CurrentUserData.ID);
#endregion

#region UserList
		private async UniTask RefreshUserInfo()
		{
			if (UserViewModel.Value is not null)
			{
				await UniTask.WaitUntil(() => GetMeetingUserInfo() != null);
				_meetingUserInfo = GetMeetingUserInfo();
			}

			RefreshPermissionView();
		}
		private MeetingUserType? GetMeetingUserInfo()
		{
			var currentMeetingInfo = MeetingReservationProvider.EnteredMeetingInfo;

			return currentMeetingInfo?.MeetingMembers?.FirstOrDefault(meetingUserInfo => meetingUserInfo?.AccountId == UserViewModel.UserId);
		}

		public void RefreshPermissionView()
		{
			_meetingUserInfo = GetMeetingUserInfo();
			InvokePropertyValueChanged(nameof(IsPermissionParticipant), IsPermissionParticipant);
			InvokePropertyValueChanged(nameof(IsPermissionPresenter), IsPermissionPresenter);
			InvokePropertyValueChanged(nameof(IsVisibleModerator), IsVisibleModerator);
			InvokePropertyValueChanged(nameof(IsPermissionOrganizer), IsPermissionOrganizer);
		}

		private void OnCommand_PermissionChangeDropDownButtonClicked()
		{
			if (MeetingReservationProvider.IsGuest())
			{
				UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_Guest_MeetingRoom_GuestNotUse_Toast"));
				return;
			}

			if (UserViewModel.IsLocalUser)
			{
				IsOrganizerDropDown = false;
			}
			else
			{
				IsOrganizerDropDown = MeetingReservationProvider.IsOrganizer(User.Instance.CurrentUserData.ID);
			}

			OpenDropDown();
		}


		private void OnCommand_PermissionChangeButtonClicked(int permissionId)
		{
			var newAuthorityCode = (AuthorityCode)permissionId;
			var oldAuthorityCode = GetMeetingUserInfo()!.AuthorityCode;

			CloseDropDown();
			if (oldAuthorityCode == newAuthorityCode)
				return;

			OnClickAuthorityChange?.Invoke(UserViewModel.UserId, oldAuthorityCode, newAuthorityCode);
			IsAuthorityLayoutOpen = false;
		}

		private void OnCommand_ForcedOutButtonClicked()
		{
			// UIManager.Instance.ShowWaitingResponsePopup();
			RequestMeetingForcedOut();
			IsAuthorityLayoutOpen = false;
		}

		private void OnCommand_ProfileButtonClicked()
		{
			CloseDropDown();
			OnClickProfile?.Invoke(UserViewModel);
		}

		private void CloseDropDown()
		{
			IsVisibleDropDown     = false;
			IsAuthorityLayoutOpen = false;
		}

		private void OpenDropDown()
		{
			IsVisibleDropDown     = true;
			IsAuthorityLayoutOpen = false;
		}
#endregion

#region CustomDataNotify
		private void ActiveEmotionCallback(Emotion emotion, int emotionState)
		{
			if (emotion.EmotionType != eEmotionType.EMOTICON)
				return;

			if (_cts != null)
			{
				_cts.Cancel();
				_cts.Dispose();
				_cts = null;
			}

			EmotionSprite   = SpriteAtlasManager.Instance.GetSprite(GestureHelper.UIAtlasName, emotion.IconName);
			IsEmotionActive = true;
			IsOtherEmotion  = true;

			_cts = new CancellationTokenSource();
			EmotionActive().Forget();
		}

		private async UniTask EmotionActive()
		{
			await UniTask.Delay(_emoticonActiveTime, cancellationToken: _cts!.Token);
			IsOtherEmotion = false;
			if (!IsHandsUp)
				IsEmotionActive = false;
		}
#endregion
	}
}
