/*===============================================================
* Product:		Com2Verse
* File Name:	CommunicationUserViewModel.cs
* Developer:	urun4m0r1
* Date:			2022-06-20 12:45
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.Chat;
using Com2Verse.Communication;
using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("Communication")]
	public sealed class CommunicationUserViewModel : ViewModelBase, INestedViewModel, IDisposable
	{
		public static CommunicationUserViewModel Empty { get; } = new(null);

#region INestedViewModel
		public IList<ViewModel> NestedViewModels { get; } = new List<ViewModel>();

		public VoiceViewModel  VoiceViewModel  { get; }
		public CameraViewModel CameraViewModel { get; }
		public ScreenViewModel ScreenViewModel { get; }

		public OrganizationUserViewModel OrganizationUserViewModel { get; }
#endregion // INestedViewModel

		public IViewModelUser? Value { get; }

		[UsedImplicitly] public CommandHandler<bool> SetVoiceState  { get; }
		[UsedImplicitly] public CommandHandler<bool> SetCameraState { get; }
		[UsedImplicitly] public CommandHandler<bool> SetScreenState { get; }

		[UsedImplicitly] public CommandHandler ToggleVoiceState  { get; }
		[UsedImplicitly] public CommandHandler ToggleCameraState { get; }
		[UsedImplicitly] public CommandHandler ToggleScreenState { get; }

		[UsedImplicitly] public CommandHandler RequestVoiceToggle  { get; }
		[UsedImplicitly] public CommandHandler RequestCameraToggle { get; }

		// ReSharper disable InconsistentNaming
		private static readonly string TextKey_OrganizerControl_CameraOff_Toast  = "UI_MeetingRoom_MicCameraCT_OrganizerControl_CameraOff_Toast";
		private static readonly string TextKey_OrganizerControl_MicOff_Toast     = "UI_MeetingRoom_MicCameraCT_OrganizerControl_MicOff_Toast";
		private static readonly string TextKey_CameraOff_Title_Popup_Text        = "UI_MeetingRoom_MicCameraCT_CameraOff_Title_Popup_Text";
		private static readonly string TextKey_CameraOff_Popup_Text              = "UI_MeetingRoom_MicCameraCT_CameraOff_Popup_Text";
		private static readonly string TextKey_MicOff_Title_Popup_Text           = "UI_MeetingRoom_MicCameraCT_MicOff_Title_Popup_Text";
		private static readonly string TextKey_MicOff_Popup_Text                 = "UI_MeetingRoom_MicCameraCT_MicOff_Popup_Text";
		private static readonly string TextKey_OrganizerControl_CameraOn_Toast   = "UI_MeetingRoom_MicCameraCT_OrganizerControl_CameraOn_Toast";
		private static readonly string TextKey_OrganizerControl_MicOn_Toast      = "UI_MeetingRoom_MicCameraCT_OrganizerControl_MicOn_Toast";
		private static readonly string TextKey_MicCameraControl_Title_Popup_Text = "UI_MeetingRoom_MicCameraCT_MicCameraControl_Title_Popup_Text";
		private static readonly string TextKey_MicCameraControl_Popup_Text       = "UI_MeetingRoom_MicCameraCT_MicCameraControl_Popup_Text";
		private static readonly string TextKey_ControlAccept_Toast               = "UI_MeetingRoom_MicCameraCT_ControlAccept_Toast";
		private static readonly string TextKey_ControlDecline_Toast              = "UI_MeetingRoom_MicCameraCT_ControlDecline_Toast";
		// ReSharper restore InconsistentNaming

		public CommunicationUserViewModel(IViewModelUser? user)
		{
			Value = user;

			OrganizationUserViewModel = new OrganizationUserViewModel(UserId);

			if (user is ILocalUser)
			{
				VoiceViewModel  = ViewModelManager.Instance.GetOrAdd<VoiceViewModel>();
				CameraViewModel = ViewModelManager.Instance.GetOrAdd<CameraViewModel>();
				ScreenViewModel = ViewModelManager.Instance.GetOrAdd<ScreenViewModel>();
			}
			else
			{
				VoiceViewModel  = user?.Voice  == null ? VoiceViewModel.Empty : new VoiceViewModel(user.Voice);
				CameraViewModel = user?.Camera == null ? CameraViewModel.Empty : new CameraViewModel(user.Camera);
				ScreenViewModel = user?.Screen == null ? ScreenViewModel.Empty : new ScreenViewModel(user.Screen);
			}

			NestedViewModels.Add(VoiceViewModel);
			NestedViewModels.Add(CameraViewModel);
			NestedViewModels.Add(ScreenViewModel);
			NestedViewModels.Add(OrganizationUserViewModel);

			OrganizationUserViewModel.NameChanged += OnNameChanged;

			if (IsAuthorizedOffice)
			{
				OrganizationUserViewModel.FetchOrganizationInfo().Forget();
			}

			SetVoiceState  = new CommandHandler<bool>(OnSetVoiceState);
			SetCameraState = new CommandHandler<bool>(OnSetCameraState);
			SetScreenState = new CommandHandler<bool>(OnSetScreenState);

			ToggleVoiceState  = new CommandHandler(OnToggleVoiceState);
			ToggleCameraState = new CommandHandler(OnToggleCameraState);
			ToggleScreenState = new CommandHandler(OnToggleScreenState);

			if (Value is IPublishableLocalUser localUser)
			{
				localUser.Modules.ConnectionTargetChanged += OnLocalConnectionTargetChanged;
			}

			if (Value is ISubscribableRemoteUser remoteUser)
			{
				var trackManager = remoteUser.SubscribeTrackManager;
				if (trackManager != null)
				{
					trackManager.TrackAdded   += OnRemoteTrackAdded;
					trackManager.TrackRemoved += OnRemoteTrackRemoved;
				}
			}

			RequestVoiceToggle  = new CommandHandler(OnRequestVoiceToggle);
			RequestCameraToggle = new CommandHandler(OnRequestCameraToggle);

			ChatManager.Instance.OnTrackPublishRequest  += OnTrackPublishRequest;
			ChatManager.Instance.OnTrackPublishResponse += OnTrackPublishResponse;
			ChatManager.Instance.OnTrackUnpublishNotify += OnTrackUnpublishNotify;
			SceneManager.Instance.CurrentSceneChanged   += OnCurrentSceneChanged;
		}

		private void OnCurrentSceneChanged(SceneBase prevScene, SceneBase currentScene)
		{
			if (IsAuthorizedOffice)
			{
				if (UserId != 0)
					OrganizationUserViewModel.FetchOrganizationInfo().Forget();
			}

			if (IsInMeeting && Value is not ILocalUser)
			{
				CommunicationSpeakerManager.Instance.TryAddUser(Value);
			}
			else
			{
				CommunicationSpeakerManager.Instance.RemoveUser(Value);
			}

			RefreshAllProperties();
		}

		private void RefreshAllProperties()
		{
			InvokePropertyValueChanged(nameof(UserName),                      UserName);
			InvokePropertyValueChanged(nameof(IsLocalVoiceConnectionTarget),  IsLocalVoiceConnectionTarget);
			InvokePropertyValueChanged(nameof(IsLocalCameraConnectionTarget), IsLocalCameraConnectionTarget);
			InvokePropertyValueChanged(nameof(IsLocalScreenConnectionTarget), IsLocalScreenConnectionTarget);
			InvokePropertyValueChanged(nameof(IsRemoteVoiceTrackAvailable),   IsRemoteVoiceTrackAvailable);
			InvokePropertyValueChanged(nameof(IsRemoteCameraTrackAvailable),  IsRemoteCameraTrackAvailable);
			InvokePropertyValueChanged(nameof(IsRemoteScreenTrackAvailable),  IsRemoteScreenTrackAvailable);
		}


		private void OnRequestVoiceToggle()
		{
			var voice = Value?.Voice;
			if (voice == null)
				return;

			if (Value is ILocalUser)
			{
				ViewModelManager.Instance.GetOrAdd<TrackPublisherViewModel>().OnToggleVoiceState();
				return;
			}

			if (voice.Input is not RemoteAudioProvider remoteVoiceProvider)
				return;

			string message;

			if (remoteVoiceProvider.Track == null)
			{
				ChatManager.Instance.SendCustomData(ChatManager.CustomDataType.TRACK_PUBLISH_REQUEST, UserId, eTrackType.VOICE);
				message = Localization.Instance.GetString(TextKey_OrganizerControl_MicOn_Toast, GetUserName());
			}
			else
			{
				ChatManager.Instance.SendCustomData(ChatManager.CustomDataType.TRACK_UNPUBLISH_NOTIFY, UserId, eTrackType.VOICE);
				message = Localization.Instance.GetString(TextKey_OrganizerControl_MicOff_Toast, GetUserName());
			}

			UIManager.Instance.SendToastMessage(message);
		}

		private void OnRequestCameraToggle()
		{
			var camera = Value?.Camera;
			if (camera == null)
				return;

			if (Value is ILocalUser)
			{
				ViewModelManager.Instance.GetOrAdd<TrackPublisherViewModel>().OnToggleCameraState();
				return;
			}

			if (camera.Input is not RemoteVideoProvider remoteCameraProvider)
				return;

			ChatManager.CustomDataType dataType;
			string                     message;

			if (remoteCameraProvider.Track == null)
			{
				dataType = ChatManager.CustomDataType.TRACK_PUBLISH_REQUEST;
				message  = Localization.Instance.GetString(TextKey_OrganizerControl_CameraOn_Toast, GetUserName());
			}
			else
			{
				dataType = ChatManager.CustomDataType.TRACK_UNPUBLISH_NOTIFY;
				message  = Localization.Instance.GetString(TextKey_OrganizerControl_CameraOff_Toast, GetUserName());
			}

			ChatManager.Instance.SendCustomData(dataType, UserId, eTrackType.CAMERA);
			UIManager.Instance.SendToastMessage(message);
		}

		private void OnTrackPublishRequest(long sender, eTrackType trackType)
		{
			if (Value is not ILocalUser)
				return;

			string title   = string.Empty;
			string content = string.Empty;

			var senderName = GetUserName(sender);

			switch (trackType)
			{
				case eTrackType.VOICE:
					title   = Localization.Instance.GetString(TextKey_MicCameraControl_Title_Popup_Text);
					content = Localization.Instance.GetString(TextKey_MicCameraControl_Popup_Text, senderName);
					break;
				case eTrackType.CAMERA:
					title   = Localization.Instance.GetString(TextKey_MicCameraControl_Title_Popup_Text);
					content = Localization.Instance.GetString(TextKey_MicCameraControl_Popup_Text, senderName);
					break;
			}

			UIManager.Instance.ShowPopupYesNo(title, content, OnYesSelected, OnNoSelected);

			void OnNoSelected(GUIView  _) => ApplySelection(false);
			void OnYesSelected(GUIView _) => ApplySelection(true);

			void ApplySelection(bool result)
			{
				var data = new ChatManager.TrackPublishResponseData
				{
					TrackType = trackType,
					Result    = result,
				};

				if (result) ViewModelManager.Instance.GetOrAdd<TrackPublisherViewModel>().SetConnectionTarget(trackType, true);
				ChatManager.Instance.SendCustomData(ChatManager.CustomDataType.TRACK_PUBLISH_RESPONSE, sender, data);
			}
		}

		private void OnTrackPublishResponse(long sender, eTrackType trackType, bool result)
		{
			if (Value is not ILocalUser)
				return;

			var senderName = GetUserName(sender);

			var message = (trackType, result) switch
			{
				(eTrackType.VOICE, true)   => Localization.Instance.GetString(TextKey_ControlAccept_Toast,  senderName),
				(eTrackType.VOICE, false)  => Localization.Instance.GetString(TextKey_ControlDecline_Toast, senderName),
				(eTrackType.CAMERA, true)  => Localization.Instance.GetString(TextKey_ControlAccept_Toast,  senderName),
				(eTrackType.CAMERA, false) => Localization.Instance.GetString(TextKey_ControlDecline_Toast, senderName),
				_                          => string.Empty,
			};

			UIManager.Instance.SendToastMessage(message);
		}

		private void OnTrackUnpublishNotify(long sender, eTrackType trackType)
		{
			if (Value is not ILocalUser)
				return;

			string title   = string.Empty;
			string content = string.Empty;

			var senderName = GetUserName(sender);

			switch (trackType)
			{
				case eTrackType.VOICE:
					title   = Localization.Instance.GetString(TextKey_MicOff_Title_Popup_Text);
					content = Localization.Instance.GetString(TextKey_MicOff_Popup_Text, senderName);
					break;
				case eTrackType.CAMERA:
					title   = Localization.Instance.GetString(TextKey_CameraOff_Title_Popup_Text);
					content = Localization.Instance.GetString(TextKey_CameraOff_Popup_Text, senderName);
					break;
			}

			ViewModelManager.InstanceOrNull?.Get<TrackPublisherViewModel>()?.SetConnectionTarget(trackType, false);
			UIManager.Instance.ShowPopupConfirm(title, content);
		}

		private void OnLocalConnectionTargetChanged(eTrackType trackType, bool isConnectionTarget)
		{
			var propertyName = trackType switch
			{
				eTrackType.VOICE  => nameof(IsLocalVoiceConnectionTarget),
				eTrackType.CAMERA => nameof(IsLocalCameraConnectionTarget),
				eTrackType.SCREEN => nameof(IsLocalScreenConnectionTarget),
				_                 => throw new ArgumentOutOfRangeException(nameof(trackType), trackType, null!),
			};

			InvokePropertyValueChanged(propertyName, isConnectionTarget);
		}

		private void OnRemoteTrackAdded(eTrackType trackType, IRemoteMediaTrack track)
		{
			InvokePropertyValueChanged(GetRemoteTrackAvailablePropertyName(trackType), true);
		}

		private void OnRemoteTrackRemoved(eTrackType trackType, IRemoteMediaTrack track)
		{
			InvokePropertyValueChanged(GetRemoteTrackAvailablePropertyName(trackType), false);
		}

		private void OnNameChanged(OrganizationUserViewModel organizationUserViewModel)
		{
			InvokePropertyValueChanged(nameof(UserName),      UserName);
		}

		private string GetRemoteTrackAvailablePropertyName(eTrackType trackType) => trackType switch
		{
			eTrackType.VOICE  => nameof(IsRemoteVoiceTrackAvailable),
			eTrackType.CAMERA => nameof(IsRemoteCameraTrackAvailable),
			eTrackType.SCREEN => nameof(IsRemoteScreenTrackAvailable),
			_                 => throw new ArgumentOutOfRangeException(nameof(trackType), trackType, null!),
		};

		private string GetUserName() => GetUserName(UserId);

		private string GetUserName(Uid uid)
		{
			if (IsInMeeting)
				return MeetingReservationProvider.GetMemberName(uid) ?? string.Empty;
			ViewModelManager.Instance.GetOrAdd<CommunicationUserManagerViewModel>().TryGet(uid, out var userViewModel);
			return userViewModel?.UserName ?? string.Empty;
		}

		public void Dispose()
		{
			if (Value is not ILocalUser)
			{
				VoiceViewModel.Dispose();
				CameraViewModel.Dispose();
				ScreenViewModel.Dispose();
			}

			var sceneManager = SceneManager.InstanceOrNull;
			if (sceneManager != null)
				sceneManager.CurrentSceneChanged -= OnCurrentSceneChanged;

			if (Value is IPublishableLocalUser localUser)
			{
				localUser.Modules.ConnectionTargetChanged -= OnLocalConnectionTargetChanged;
			}

			if (Value is ISubscribableRemoteUser remoteUser)
			{
				var trackManager = remoteUser.SubscribeTrackManager;
				if (trackManager != null)
				{
					trackManager.TrackAdded   -= OnRemoteTrackAdded;
					trackManager.TrackRemoved -= OnRemoteTrackRemoved;
				}
			}

			var chatManager = ChatManager.InstanceOrNull;
			if (chatManager != null)
			{
				chatManager.OnTrackPublishRequest  -= OnTrackPublishRequest;
				chatManager.OnTrackPublishResponse -= OnTrackPublishResponse;
				chatManager.OnTrackUnpublishNotify -= OnTrackUnpublishNotify;
			}
		}

		private void OnSetVoiceState(bool value)
		{
			var voice = Value?.Voice;
			if (voice == null)
				return;

			if (Value is ILocalUser)
				ViewModelManager.Instance.GetOrAdd<TrackPublisherViewModel>().OnSetVoiceState(value);
			else if (Value is IRemoteUser)
				voice.Input.IsAudible = value;
		}

		private void OnToggleVoiceState()
		{
			var voice = Value?.Voice;
			if (voice == null)
				return;

			if (Value is ILocalUser)
				ViewModelManager.Instance.GetOrAdd<TrackPublisherViewModel>().OnToggleVoiceState();
			else if (Value is IRemoteUser)
				voice.Input.IsAudible ^= true;
		}

		private void OnSetCameraState(bool value)
		{
			var camera = Value?.Camera;
			if (camera == null)
				return;

			if (Value is ILocalUser)
				ViewModelManager.Instance.GetOrAdd<TrackPublisherViewModel>().OnSetCameraState(value);
			else if (Value is IRemoteUser)
				camera.Input.IsRunning = value;
		}

		private void OnToggleCameraState()
		{
			var camera = Value?.Camera;
			if (camera == null)
				return;

			if (Value is ILocalUser)
				ViewModelManager.Instance.GetOrAdd<TrackPublisherViewModel>().OnToggleCameraState();
			else if (Value is IRemoteUser)
				camera.Input.IsRunning ^= true;
		}

		private void OnSetScreenState(bool value)
		{
			var screen = Value?.Screen;
			if (screen == null)
				return;

			if (Value is ILocalUser)
				ViewModelManager.Instance.GetOrAdd<TrackPublisherViewModel>().OnSetScreenState(value);
			else if (Value is IRemoteUser)
				screen.Input.IsRunning = value;
		}

		private void OnToggleScreenState()
		{
			var screen = Value?.Screen;
			if (screen == null)
				return;

			if (Value is ILocalUser)
				ViewModelManager.Instance.GetOrAdd<TrackPublisherViewModel>().OnToggleScreenState();
			else if (Value is IRemoteUser)
				screen.Input.IsRunning ^= true;
		}

#region ViewModelProperties
		public bool IsLocalUser  => Value is ILocalUser;
		public bool IsRemoteUser => Value is IRemoteUser;
#if ENABLE_CHEATING
		public bool IsDummyUser => Value is Communication.Cheat.DummyUser;
#endif // ENABLE_CHEATING

		public string? UserName => Value?.User.Name;
		public long    UserId   => Value?.User.Uid ?? 0;

		public bool IsLocalVoiceConnectionTarget  => (Value as IPublishableLocalUser)?.Modules.IsConnectionTarget(eTrackType.VOICE)  ?? false;
		public bool IsLocalCameraConnectionTarget => (Value as IPublishableLocalUser)?.Modules.IsConnectionTarget(eTrackType.CAMERA) ?? false;
		public bool IsLocalScreenConnectionTarget => (Value as IPublishableLocalUser)?.Modules.IsConnectionTarget(eTrackType.SCREEN) ?? false;

		public bool IsRemoteVoiceTrackAvailable  => (Value as ISubscribableRemoteUser)?.SubscribeTrackManager?.Tracks.ContainsKey(eTrackType.VOICE)  ?? false;
		public bool IsRemoteCameraTrackAvailable => (Value as ISubscribableRemoteUser)?.SubscribeTrackManager?.Tracks.ContainsKey(eTrackType.CAMERA) ?? false;
		public bool IsRemoteScreenTrackAvailable => (Value as ISubscribableRemoteUser)?.SubscribeTrackManager?.Tracks.ContainsKey(eTrackType.SCREEN) ?? false;

		private bool IsAuthorizedOffice => CurrentScene.ServiceType is eServiceType.OFFICE && CurrentScene.SpaceCode is not (eSpaceCode.LOBBY or eSpaceCode.MODEL_HOUSE) && IsSceneLoaded;
		private bool IsSceneLoaded      => SceneManager.InstanceOrNull?.CurrentScene.SceneState is eSceneState.LOADED;
		private bool IsInMeeting        => CurrentScene.ServiceType is eServiceType.OFFICE && CurrentScene.SpaceCode is eSpaceCode.MEETING;
#endregion // ViewModelProperties
	}
}
