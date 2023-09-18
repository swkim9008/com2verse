/*===============================================================
* Product:		Com2Verse
* File Name:	CommunicationAvatarViewModel.cs
* Developer:	urun4m0r1
* Date:			2022-06-20 12:45
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.Communication;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Mice;
using Com2Verse.Network;
using Com2Verse.SmallTalk;
using Com2Verse.SmallTalk.SmallTalkObject;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.UI
{
	/// <summary>
	/// <see cref="CommunicationUserViewModel"/>과 <see cref="ActiveObjectViewModel"/>을 하위 ViewModel로 가지는 ViewModel.
	/// <br/>네트워크 타이밍 문제로 인해 둘중 하나가 먼저 생성되는 경우가 있으므로, 둘중 하나가 나중에 생성되는 경우에도 정상적으로 동작하도록 하기 위해 하위 ViewModel을 동적으로 할당한다.
	/// </summary>
	[UsedImplicitly, ViewModelGroup("Communication")]
	public sealed class CommunicationAvatarViewModel : ViewModelBase, INestedViewModel, IDisposable
	{
#region INestedViewModel
		public IList<ViewModel> NestedViewModels { get; } = new List<ViewModel>();

		public CommunicationUserViewModel UserViewModel             { get; }
		public ActiveObjectViewModel      AvatarViewModel           { get; }
		public OrganizationUserViewModel  OrganizationUserViewModel { get; }
#endregion // INestedViewModel

		public CommunicationAvatarViewModel(CommunicationUserViewModel? userViewModel, ActiveObjectViewModel? avatarViewModel)
		{
			UserViewModel   = userViewModel   ?? CommunicationUserViewModel.Empty;
			AvatarViewModel = avatarViewModel ?? ActiveObjectViewModel.Empty;

			OrganizationUserViewModel = UserViewModel == CommunicationUserViewModel.Empty ? new OrganizationUserViewModel(UserId) : UserViewModel.OrganizationUserViewModel;

			NestedViewModels.Add(UserViewModel);
			NestedViewModels.Add(AvatarViewModel);

			NestedViewModels.Add(OrganizationUserViewModel);

			RegisterPropertyUpdateEvents();

			if (IsMeetingRoom && UserViewModel.Value is not ILocalUser)
			{
				CommunicationSpeakerManager.Instance.TryAddUser(UserViewModel.Value);
			}
		}

		public void Dispose()
		{
			UnregisterPropertyUpdateEvents();
		}

		private void RegisterPropertyUpdateEvents()
		{
			SceneManager.Instance.CurrentSceneChanged += OnCurrentSceneChanged;

			SmallTalkDistance.Instance.StateChanged           += OnDistanceSmallTalkConnectionChanged;
			SmallTalkObjectManager.Instance.ConnectionChanged += OnTriggerSmallTalkConnectionChanged;

			if (UserViewModel.Value is IPublishableLocalUser publishableLocalUser)
			{
				publishableLocalUser.Modules.ConnectionTargetChanged += OnLocalConnectionTargetChanged;
			}

			if (UserViewModel.Value is ISubscribableRemoteUser remoteUser)
			{
				var trackManager = remoteUser.SubscribeTrackManager;
				if (trackManager != null)
				{
					trackManager.TrackAdded   += OnRemoteUserTrackAddedOrRemoved;
					trackManager.TrackRemoved += OnRemoteUserTrackAddedOrRemoved;
				}
			}

			var voice = UserViewModel.Value?.Voice;
			if (voice != null)
			{
				voice.Input.StateChanged += OnVoiceInputChanged;
			}

			var camera = UserViewModel.Value?.Camera;
			if (camera != null)
				camera.TextureChanged += OnCameraTextureChanged;

			var smallTalkManager = SmallTalkDistance.InstanceOrNull;
			if (smallTalkManager != null)
				smallTalkManager.TrackPublishRequestChanged += OnTrackPublishRequestChanged;

			var mapObject = AvatarViewModel.Value;
			if (mapObject != null)
				mapObject.NameChanged += OnNameChanged;

			OrganizationUserViewModel.NameChanged += OnNameChanged;

			AuditoriumController.Instance.OnEnterSpeecherObjectEvent += OnAuditoriumSpeecherChanged;
			AuditoriumController.Instance.OnExitSpeecherObjectEvent  += OnAuditoriumSpeecherChanged;

			CommunicationSpeakerManager.Instance.SpeakerListChanged += OnCommunicationSpeakerListChanged;
		}

		public void UnregisterPropertyUpdateEvents()
		{
			var sceneManager = SceneManager.InstanceOrNull;
			if (sceneManager != null)
				sceneManager.CurrentSceneChanged -= OnCurrentSceneChanged;

			var distanceSmallTalk = SmallTalkDistance.InstanceOrNull;
			if (distanceSmallTalk != null)
				distanceSmallTalk.StateChanged -= OnDistanceSmallTalkConnectionChanged;

			var triggerSmallTalk = SmallTalkObjectManager.InstanceOrNull;
			if (!triggerSmallTalk.IsUnityNull())
				triggerSmallTalk!.ConnectionChanged -= OnTriggerSmallTalkConnectionChanged;

			if (UserViewModel.Value is IPublishableLocalUser publishableLocalUser)
			{
				publishableLocalUser.Modules.ConnectionTargetChanged -= OnLocalConnectionTargetChanged;
			}

			if (UserViewModel.Value is ISubscribableRemoteUser remoteUser)
			{
				var trackManager = remoteUser.SubscribeTrackManager;
				if (trackManager != null)
				{
					trackManager.TrackAdded   -= OnRemoteUserTrackAddedOrRemoved;
					trackManager.TrackRemoved -= OnRemoteUserTrackAddedOrRemoved;
				}
			}

			var voice = UserViewModel.Value?.Voice;
			if (voice != null)
			{
				voice.Input.StateChanged -= OnVoiceInputChanged;
			}

			var camera = UserViewModel.Value?.Camera;
			if (camera != null)
				camera.TextureChanged -= OnCameraTextureChanged;

			var smallTalkManager = SmallTalkDistance.InstanceOrNull;
			if (smallTalkManager != null)
				smallTalkManager.TrackPublishRequestChanged -= OnTrackPublishRequestChanged;

			var mapObject = AvatarViewModel.Value;
			if (mapObject != null)
				mapObject.NameChanged -= OnNameChanged;

			OrganizationUserViewModel.NameChanged -= OnNameChanged;

			var auditoriumController = AuditoriumController.InstanceOrNull;
			if (auditoriumController != null)
			{
				auditoriumController.OnEnterSpeecherObjectEvent -= OnAuditoriumSpeecherChanged;
				auditoriumController.OnExitSpeecherObjectEvent  -= OnAuditoriumSpeecherChanged;
			}

			var communicationSpeakerManager = CommunicationSpeakerManager.InstanceOrNull;
			if (communicationSpeakerManager != null)
			{
				communicationSpeakerManager.SpeakerListChanged -= OnCommunicationSpeakerListChanged;
				communicationSpeakerManager.RemoveUser(UserViewModel.Value);
			}
		}

		private void OnCurrentSceneChanged(SceneBase prevScene, SceneBase currentScene)
		{
			RefreshAllProperties();
			SceneManager.Instance.CurrentSceneChanged -= OnCurrentSceneChanged;
		}

		private void OnDistanceSmallTalkConnectionChanged(bool isConnected)
		{
			InvokePropertyValueChanged(nameof(UseVoiceObserver),  UseVoiceObserver);
			InvokePropertyValueChanged(nameof(UseCameraObserver), UseCameraObserver);
			InvokePropertyValueChanged(nameof(UseVoiceView),      UseVoiceView);
			InvokePropertyValueChanged(nameof(UseVoiceIcon),      UseVoiceIcon);
			InvokePropertyValueChanged(nameof(UseCameraView),     UseCameraView);
			InvokePropertyValueChanged(nameof(UseCameraTexture),  UseCameraTexture);
		}

		private void OnTriggerSmallTalkConnectionChanged(bool isConnected)
		{
			InvokePropertyValueChanged(nameof(UseVoiceObserver),  UseVoiceObserver);
			InvokePropertyValueChanged(nameof(UseCameraObserver), UseCameraObserver);
			InvokePropertyValueChanged(nameof(UseVoiceView),      UseVoiceView);
			InvokePropertyValueChanged(nameof(UseVoiceIcon),      UseVoiceIcon);
			InvokePropertyValueChanged(nameof(UseCameraView),     UseCameraView);
			InvokePropertyValueChanged(nameof(UseCameraTexture),  UseCameraTexture);
		}

		private void OnLocalConnectionTargetChanged(eTrackType trackType, bool isConnectionTarget)
		{
			switch (trackType)
			{
				case eTrackType.VOICE:
					InvokePropertyValueChanged(nameof(UseVoiceView), UseVoiceView);
					InvokePropertyValueChanged(nameof(UseVoiceIcon), UseVoiceIcon);
					break;
				case eTrackType.CAMERA:
					InvokePropertyValueChanged(nameof(UseCameraView),    UseCameraView);
					InvokePropertyValueChanged(nameof(UseCameraTexture), UseCameraTexture);
					break;
			}
		}

		private void OnRemoteUserTrackAddedOrRemoved(eTrackType trackType, IRemoteMediaTrack track)
		{
			if (trackType == eTrackType.VOICE) InvokePropertyValueChanged(nameof(UseVoiceIcon), UseVoiceIcon);
		}

		private void OnVoiceInputChanged(bool value)
		{
			InvokePropertyValueChanged(nameof(UseVoiceIcon), UseVoiceIcon);
		}

		private void OnCameraTextureChanged(Texture? value)
		{
			InvokePropertyValueChanged(nameof(UseCameraView),    UseCameraView);
			InvokePropertyValueChanged(nameof(UseCameraTexture), UseCameraTexture);
		}

		private void OnTrackPublishRequestChanged(ChannelObjectInfo target, eTrackType trackType, bool isPublish)
		{
			if (UserViewModel.IsLocalUser)
			{
				switch (trackType)
				{
					case eTrackType.CAMERA:
						InvokePropertyValueChanged(nameof(UseCameraView),    UseCameraView);
						InvokePropertyValueChanged(nameof(UseCameraTexture), UseCameraTexture);
						break;
					case eTrackType.VOICE:
						InvokePropertyValueChanged(nameof(UseVoiceView), UseVoiceView);
						InvokePropertyValueChanged(nameof(UseVoiceIcon), UseVoiceIcon);
						break;
				}
			}
			else if (target.OwnerId == UserViewModel.UserId)
			{
				switch (trackType)
				{
					case eTrackType.CAMERA:
						InvokePropertyValueChanged(nameof(UseCameraObserver), UseCameraObserver);
						break;
					case eTrackType.VOICE:
						InvokePropertyValueChanged(nameof(UseVoiceObserver), UseVoiceObserver);
						InvokePropertyValueChanged(nameof(UseVoiceView),     UseVoiceView);
						InvokePropertyValueChanged(nameof(UseVoiceIcon),     UseVoiceIcon);
						break;
				}
			}
		}

		private void OnNameChanged(BaseMapObject baseMapObject, string prevName, string newName)
		{
			InvokePropertyValueChanged(nameof(UseAvatarView), UseAvatarView);
			InvokePropertyValueChanged(nameof(UserName),      UserName);
		}

		private void OnNameChanged(OrganizationUserViewModel organizationUserViewModel)
		{
			InvokePropertyValueChanged(nameof(UseAvatarView), UseAvatarView);
			InvokePropertyValueChanged(nameof(UserName),      UserName);
		}

		private void RefreshAllProperties()
		{
			InvokePropertyValueChanged(nameof(UseAvatarView),     UseAvatarView);
			InvokePropertyValueChanged(nameof(UserName),          UserName);
			InvokePropertyValueChanged(nameof(UseVoiceObserver),  UseVoiceObserver);
			InvokePropertyValueChanged(nameof(UseCameraObserver), UseCameraObserver);
			InvokePropertyValueChanged(nameof(UseVoiceView),      UseVoiceView);
			InvokePropertyValueChanged(nameof(UseVoiceIcon),      UseVoiceIcon);
			InvokePropertyValueChanged(nameof(UseCameraView),     UseCameraView);
			InvokePropertyValueChanged(nameof(UseCameraTexture),  UseCameraTexture);

			RefreshNameplateProperties();
		}

		private void OnAuditoriumSpeecherChanged(BaseMapObject speecher)
		{
			if (speecher == AvatarViewModel.Value)
				RefreshNameplateProperties();
		}

		private void RefreshNameplateProperties()
		{
			InvokePropertyValueChanged(nameof(UseDefaultNameplate),           UseDefaultNameplate);
			InvokePropertyValueChanged(nameof(UseAuditoriumSpeakerNameplate), UseAuditoriumSpeakerNameplate);
			InvokePropertyValueChanged(nameof(UseMiceSpeakerNameplate),       UseMiceSpeakerNameplate);
			InvokePropertyValueChanged(nameof(IsAvatarInMiceSession),       IsAvatarInMiceSession);
		}

		private void OnCommunicationSpeakerListChanged(IReadOnlyList<IViewModelUser> list)
		{
			if (!IsMeetingRoom)
				return;

			InvokePropertyValueChanged(nameof(UseCameraObserver), UseCameraObserver);
		}

#region ViewModelProperties
		/// <summary>
		/// 아바타 뷰 자체를 사용할지 여부.
		/// </summary>
		public bool UseAvatarView => !string.IsNullOrWhiteSpace(UserName!);

		/// <summary>
		/// 표시되는 사용자명.
		/// <br/>월드: 월드 이름
		/// <br/>오피스 [로비]: 월드 이름
		/// <br/>오피스 [로비 이외]: 조직도 이름
		/// <br/>마이스: 월드 이름
		/// </summary>
		public string? UserName
		{
			get
			{
				if (IsAuthorizedOffice)
				{
					var organizationName = OrganizationUserViewModel.UserName;
					if (!string.IsNullOrWhiteSpace(organizationName!))
						return organizationName;
				}
				else
				{
					var avatarTagName = AvatarViewModel.Name;
					if (!string.IsNullOrWhiteSpace(avatarTagName))
						return avatarTagName;
				}

				return null;
			}
		}

		/// <summary>
		/// 아바타 음성 옵저버 사용 여부. (로컬 유저는 항상 false)
		/// <br/>스몰토크[거리] (Remote): 음성 트랙을 공유를 요청한 경우
		/// <br/>스몰토크[트리거] (Remote): 항상 true
		/// <br/>회의실 (Remote): 항상 true
		/// </summary>
		public bool UseVoiceObserver
		{
			get
			{
				if (!IsCommunicationUserAssigned)
					return false;

				if (!CurrentScene.UseVoiceModule)
					return false;

				if (UserViewModel.IsLocalUser)
					return false;

				if (IsDistanceSmallTalk)
					return SmallTalkDistance.Instance.VoiceRequestingUsers.ContainsKey(UserViewModel.UserId);

				if (IsTriggerSmallTalk)
					return true;

				if (IsMeetingRoom)
					return true;

				return false;
			}
		}

		/// <summary>
		/// 아바타 화상 옵저버 사용 여부. (로컬 유저는 항상 false)
		/// <br/>스몰토크[거리] (Remote): 카메라 트랙을 공유를 요청한 경우
		/// <br/>스몰토크[트리거] (Remote): 항상 true
		/// <br/>회의실 (Remote): 화자 대기열에 있는 경우 && 최대 표시 인원을 넘지 않는 경우
		/// </summary>
		public bool UseCameraObserver
		{
			get
			{
				if (!IsCommunicationUserAssigned)
					return false;

				if (!CurrentScene.UseCameraModule)
					return false;

				if (UserViewModel.IsLocalUser)
					return false;

				if (IsDistanceSmallTalk)
					return SmallTalkDistance.Instance.CameraRequestingUsers.ContainsKey(UserViewModel.UserId);

				if (IsTriggerSmallTalk)
					return true;

				if (IsMeetingRoom)
				{
					var index    = CommunicationSpeakerManager.InstanceOrNull?.GetSpeakerIndexOf(UserViewModel.Value) ?? -1;
					var maxIndex = VoiceDetectionManager.InstanceOrNull?.Settings?.MaxActivatedSpeakersCount - 1;
					return index >= 0 && index <= maxIndex;
				}

				return false;
			}
		}

		/// <summary>
		/// 아바타 화상 UI를 사용할지 여부.
		/// <br/>로비가 아닌 오피스에서는 항상 true (이하 규칙보다 우선시됨)
		/// <br/>씬 모듈에 정의되지 않은 경우 false (이하 규칙보다 우선시됨)
		/// <br/>카메라 트랙 송출이 되어있지 않는 로컬 유저의 경우 false (이하 규칙보다 우선시됨)
		/// <br/>스몰토크[거리] (Local): 화상이 보이는 경우 && 카메라 트랙을 공유중인 사용자가 있는 경우
		/// <br/>스몰토크[거리] (Remote): 화상이 보이는 경우
		/// <br/>스몰토크[트리거] (Local): 화상이 보이는 경우
		/// <br/>스몰토크[트리거] (Remote): 화상이 보이는 경우
		/// <br/>회의실 (Local): 항상 true
		/// <br/>회의실 (Remote): 항상 true
		/// </summary>
		public bool UseCameraView
		{
			get
			{
				if (IsAuthorizedOffice)
					return true;

				if (!IsCommunicationUserAssigned)
					return false;

				if (!CurrentScene.UseCameraModule)
					return false;

				if (UserViewModel is { IsLocalUser: true, IsLocalCameraConnectionTarget: false })
					return false;

				if (IsDistanceSmallTalk)
				{
					if (!UserViewModel.CameraViewModel.IsVisible)
						return false;

					if (UserViewModel.IsLocalUser)
						return SmallTalkDistance.Instance.CameraRequestingUsers.Count > 0;

					if (UserViewModel.IsRemoteUser)
						return true;
				}

				if (IsTriggerSmallTalk)
					return UserViewModel.CameraViewModel.IsVisible;

				if (IsMeetingRoom)
					return true;

				return false;
			}
		}

		/// <summary>
		/// 아바타 화상 텍스쳐를 보여줄지 여부.
		/// <br/>씬 모듈에 정의되지 않은 경우 false (이하 규칙보다 우선시됨)
		/// <br/>화상이 보이지 않는 경우 false (이하 규칙보다 우선시됨)
		/// <br/>카메라 트랙 송출이 되어있지 않는 로컬 유저의 경우 false (이하 규칙보다 우선시됨)
		/// <br/>스몰토크[거리] (Local): 카메라 트랙을 공유중인 사용자가 있는 경우
		/// <br/>스몰토크[거리] (Remote): 항상 true
		/// <br/>스몰토크[트리거] (Local): 항상 true
		/// <br/>스몰토크[트리거] (Remote): 항상 true
		/// <br/>회의실 (Local): 항상 true
		/// <br/>회의실 (Remote): 항상 true
		/// </summary>
		public bool UseCameraTexture
		{
			get
			{
				if (!IsCommunicationUserAssigned)
					return false;

				if (!CurrentScene.UseCameraModule)
					return false;

				if (!UserViewModel.CameraViewModel.IsVisible)
					return false;

				if (UserViewModel is { IsLocalUser: true, IsLocalCameraConnectionTarget: false })
					return false;

				if (IsDistanceSmallTalk)
				{
					if (UserViewModel.IsLocalUser)
						return SmallTalkDistance.Instance.CameraRequestingUsers.Count > 0;

					if (UserViewModel.IsRemoteUser)
						return true;
				}

				if (IsTriggerSmallTalk)
					return true;

				if (IsMeetingRoom)
					return true;

				return false;
			}
		}

		/// <summary>
		/// 아바타 음성 UI를 사용할지 여부.
		/// <br/>씬 모듈에 정의되지 않은 경우 false (이하 규칙보다 우선시됨)
		/// <br/>음성 트랙 송출이 되어있지 않는 로컬 유저의 경우 false (이하 규칙보다 우선시됨)
		/// <br/>스몰토크[거리] (Local): 음성 트랙을 공유중인 사용자가 있는 경우
		/// <br/>스몰토크[거리] (Remote): 음성 트랙을 공유를 요청한 경우
		/// <br/>스몰토크[트리거] (Local): 항상 true
		/// <br/>스몰토크[트리거] (Remote): 항상 true
		/// <br/>회의실 (Local): 항상 true
		/// <br/>회의실 (Remote): 항상 true
		/// </summary>
		public bool UseVoiceView
		{
			get
			{
				if (!IsCommunicationUserAssigned)
					return false;

				if (!CurrentScene.UseVoiceModule)
					return false;

				if (UserViewModel is { IsLocalUser: true, IsLocalVoiceConnectionTarget: false })
					return false;

				if (IsDistanceSmallTalk)
				{
					if (UserViewModel.IsLocalUser)
						return SmallTalkDistance.Instance.VoiceRequestingUsers.Count > 0;

					if (UserViewModel.IsRemoteUser)
						return SmallTalkDistance.Instance.VoiceRequestingUsers.ContainsKey(UserViewModel.UserId);
				}

				if (IsTriggerSmallTalk)
					return true;

				if (IsMeetingRoom)
					return true;

				return false;
			}
		}

		/// <summary>
		/// 아바타 음성 아이콘을 사용할지 여부.
		/// <br/>씬 모듈에 정의되지 않은 경우 false (이하 규칙보다 우선시됨)
		/// <br/>스몰토크[거리] (Local): 음성 트랙을 공유중인 사용자가 있는 경우 && 마이크가 꺼져있거나 송출중이지 않은 경우
		/// <br/>스몰토크[거리] (Remote): 음성 트랙을 공유를 요청한 경우 && 원격 트랙이 없는 경우
		/// <br/>스몰토크[트리거] (Local): 마이크가 꺼져있거나 송출중이지 않은 경우
		/// <br/>스몰토크[트리거] (Remote): 원격 트랙이 없는 경우
		/// <br/>회의실 (Local): 마이크가 꺼져있거나 송출중이지 않은 경우
		/// <br/>회의실 (Remote): 원격 트랙이 없는 경우
		/// </summary>
		public bool UseVoiceIcon
		{
			get
			{
				if (!IsCommunicationUserAssigned)
					return false;

				if (!CurrentScene.UseVoiceModule)
					return false;

				if (IsDistanceSmallTalk)
				{
					if (UserViewModel.IsLocalUser)
						return SmallTalkDistance.Instance.VoiceRequestingUsers.Count > 0 && (!UserViewModel.VoiceViewModel.IsRunning || !UserViewModel.IsLocalVoiceConnectionTarget);

					if (UserViewModel.IsRemoteUser)
						return SmallTalkDistance.Instance.VoiceRequestingUsers.ContainsKey(UserViewModel.UserId) && !UserViewModel.IsRemoteVoiceTrackAvailable;
				}

				if (IsTriggerSmallTalk || IsMeetingRoom)
				{
					if (UserViewModel.IsLocalUser)
						return !UserViewModel.VoiceViewModel.IsRunning || !UserViewModel.IsLocalVoiceConnectionTarget;

					if (UserViewModel.IsRemoteUser)
						return !UserViewModel.IsRemoteVoiceTrackAvailable;
				}

				return false;
			}
		}

		public bool UseDefaultNameplate => !UseMiceSpeakerNameplate;

		public bool UseAuditoriumSpeakerNameplate => AuditoriumController.InstanceOrNull?.IsContainsSpeecher(AvatarViewModel.ObjectId) ?? false;

		public bool UseMiceSpeakerNameplate => CurrentScene.ServiceType is eServiceType.MICE && IsSceneLoaded;
		
		public bool IsAvatarInMiceSession => MiceService.Instance.IsInSession;

		public Uid UserId => AvatarViewModel.OwnerId;
#endregion // ViewModelProperties

		private bool IsCommunicationUserAssigned => UserViewModel.Value != null;

		private bool IsDistanceSmallTalk => SmallTalkDistance.InstanceOrNull?.IsEnabled ?? false;

		private bool IsTriggerSmallTalk
		{
			get
			{
				var triggerSmallTalk = SmallTalkObjectManager.InstanceOrNull;
				if (!triggerSmallTalk.IsUnityNull() && triggerSmallTalk!.IsConnected)
					return true;

				return false;
			}
		}

		private bool IsMeetingRoom => CurrentScene.CommunicationType is eSpaceOptionCommunication.MEETING && IsSceneLoaded;

		private bool IsAuthorizedOffice => CurrentScene.ServiceType is eServiceType.OFFICE && CurrentScene.SpaceCode is not (eSpaceCode.LOBBY or eSpaceCode.MODEL_HOUSE) && IsSceneLoaded;

		private bool IsSceneLoaded => SceneManager.InstanceOrNull?.CurrentScene.SceneState is eSceneState.LOADED;
	}
}
