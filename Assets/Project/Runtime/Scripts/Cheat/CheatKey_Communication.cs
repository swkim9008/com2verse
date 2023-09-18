#if ENABLE_CHEATING

/*===============================================================
* Product:		Com2Verse
* File Name:	CheatKey.cs
* Developer:	urun4m0r1
* Date:			2022-12-26 15:16
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Com2Verse.Communication;
using Com2Verse.Communication.Cheat;
using Com2Verse.Communication.Unity;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Loading;
using Com2Verse.Logger;
using Com2Verse.UI;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Cheat
{
	[SuppressMessage("ReSharper", "UnusedMember.Local")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static partial class CheatKey
	{
		private const string Header = "Cheat/Communication/";

#region Layout
		[MetaverseCheat(Header + "[Layout] " + nameof(ToggleDebugLayout))]
		private static void ToggleDebugLayout()
		{
			var meetingRoom = ViewModelManager.Instance.Get<MeetingRoomLayoutViewModel>();
			if (meetingRoom != null)
				meetingRoom.IsDebugLayout ^= true;
		}

		[MetaverseCheat(Header + "[Layout] " + nameof(MockLocalScreenShared))]
		private static void MockLocalScreenShared()
		{
			var meetingRoom = ViewModelManager.Instance.Get<MeetingRoomLayoutViewModel>();
			CallPrivateMethod(meetingRoom, "OnSharedScreenChanged", true, true);
		}

		[MetaverseCheat(Header + "[Layout] " + nameof(MockRemoteScreenShared))]
		private static void MockRemoteScreenShared()
		{
			var meetingRoom = ViewModelManager.Instance.Get<MeetingRoomLayoutViewModel>();
			CallPrivateMethod(meetingRoom, "OnSharedScreenChanged", false, true);
		}

		[MetaverseCheat(Header + "[Layout] " + nameof(MockScreenUnshared))]
		private static void MockScreenUnshared()
		{
			var meetingRoom = ViewModelManager.Instance.Get<MeetingRoomLayoutViewModel>();
			CallPrivateMethod(meetingRoom, "OnSharedScreenChanged", true, false);
		}

		private static void CallPrivateMethod(object? obj, string methodName, params object[] parameters)
		{
			if (obj == null)
				return;

			var method = obj.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			method?.Invoke(obj, parameters);
		}
#endregion // Layout

#region Communication
		[MetaverseCheat(Header + "[Communication] " + nameof(ChangeCommunicationType))] [HelpText("0~8")]
		private static void ChangeCommunicationType(string communicationType = "0")
		{
			var communicationTypeInt = Convert.ToInt32(communicationType);

			CommunicationManager.Instance.CommunicationType = communicationTypeInt.CastEnum<eCommunicationType>();
		}

		[MetaverseCheat(Header + "[Communication] " + nameof(DebugMeetingRoom))] [HelpText("1~n", "김직원", "room1")]
		private static void DebugMeetingRoom(string uid, string name, string channelId)
		{
			var useCustomUid       = !string.IsNullOrEmpty(uid);
			var useCustomName      = !string.IsNullOrEmpty(name);
			var useCustomChannelId = !string.IsNullOrEmpty(channelId);

			var uidActual       = useCustomUid ? (Uid)Convert.ToInt64(uid) : GetUid();
			var nameActual      = useCustomName ? name : GetName();
			var channelIdActual = useCustomChannelId ? channelId : "room1";

			CommunicationManager.Instance.ChangeCommunicationType(eCommunicationType.MEETING_ROOM_ROUND);
			ChannelManager.Instance.AddDebugChannel(new User(uidActual, $"{nameActual} (D)"), channelIdActual, "");

			var sceneProperty = new SceneProperty
			{
				AddressableName = "Office_Meeting_Common_1",
				SpaceTemplate = new SpaceTemplate
				{
					SpaceType = eSpaceType.OFFICE,
					SpaceCode = eSpaceCode.MEETING,
				},
				ServiceType        = eServiceType.OFFICE,
				CommunicationType  = eSpaceOptionCommunication.MEETING,
				ChattingUIType     = eSpaceOptionChattingUI.NONE,
				ToolbarUIType      = eSpaceOptionToolbarUI.NONE,
				MapType            = eSpaceOptionMap.NONE,
				GhostAvatarType    = eSpaceOptionGhostAvatar.DEFAULT,
				ViewportType       = eSpaceOptionViewport.DEFAULT,
				MotionTrackingType = default,
				IsDebug            = IsDebugModuleRequired(),
				SessionTimeout     = -1,
			};

			LoadingManager.Instance.ChangeScene<SceneSpace>(sceneProperty);

			ChannelManager.Instance.ViewModelUserAdded   += OnChannelUserAdded;
			ChannelManager.Instance.ViewModelUserRemoved += OnChannelUserRemoved;
		}

		private static Uid GetUid()
		{
			var user = Network.User.InstanceOrNull;
			if (user == null)
				return GetRandomUid();

			if (user.CurrentUserData.ID <= 0)
				return GetRandomUid();

			return user.CurrentUserData.ID;

			static Uid GetRandomUid() => UnityEngine.Random.Range(1, int.MaxValue);
		}

		private static string GetName()
		{
			var user = Network.User.InstanceOrNull;
			if (user == null)
				return GetRandomName();

			if (user.CurrentUserData.ID <= 0)
				return GetRandomName();

			if (string.IsNullOrWhiteSpace(user.CurrentUserData.UserName!))
				return GetRandomName();

			return user.CurrentUserData.UserName;

			static string GetRandomName() => DummyUser.DummyNames[UnityEngine.Random.Range(0, DummyUser.DummyNames.Count)]!;
		}

		private static void OnChannelUserAdded(IChannel channel, IViewModelUser user)
		{
			if (!IsDebugAvatarRequired(user))
				return;

			UniTaskHelper.InvokeOnMainThread(() => DebugUtils.AddCommunicationCharacter(user.User.Uid, user is ILocalUser)).Forget();
		}

		private static void OnChannelUserRemoved(IChannel channel, IViewModelUser user)
		{
			if (!IsDebugAvatarRequired(user))
				return;

			UniTaskHelper.InvokeOnMainThread(() => DebugUtils.RemoveCommunicationCharacter(user.User.Uid)).Forget();
		}

		private static bool IsDebugAvatarRequired(IViewModelUser? user)
		{
			if (user is DummyUser)
				return true;

			return IsDebugModuleRequired();
		}

		private static bool IsDebugModuleRequired()
		{
			if (!Network.User.InstanceExists) return true;

			return string.IsNullOrEmpty(Network.User.InstanceOrNull?.CurrentUserData.AccessToken!);
		}

		[MetaverseCheat(Header + "[Communication] " + nameof(AddDummyUser))]
		public static void AddDummyUser() => DummyUserManager?.AddDummyUser();

		[MetaverseCheat(Header + "[Communication] " + nameof(RemoveDummyUser))]
		public static void RemoveDummyUser() => DummyUserManager?.RemoveDummyUser();

		[MetaverseCheat(Header + "[Communication] " + nameof(ClearDummyUsers))]
		public static void ClearDummyUsers() => DummyUserManager?.ClearDummyUsers();

		public static DummyUserManager? DummyUserManager => ChannelManager.Instance.DebugChannel?.DummyUserManager;

		[MetaverseCheat(Header + "[Communication] " + nameof(LogRemoteUsersObserverStatus))]
		public static void LogRemoteUsersObserverStatus()
		{
			var channels = ChannelManager.Instance.JoiningChannels;
			foreach (var channel in channels.Values)
			{
				if (channel == null)
					continue;

				foreach (var user in channel.ConnectedUsers.Values)
				{
					if (user is not ISubscribableRemoteUser remoteUser)
						continue;

					foreach (var type in EnumUtility.Foreach<eTrackType>())
					{
						foreach (var observer in GetObservers(remoteUser, type))
						{
							C2VDebug.LogMethod(nameof(CheatKey), $"[{type}] Observer: {observer} / {user} / {channel}");
						}
					}
				}
			}
		}

		private static IEnumerable<IRemoteTrackObserver> GetObservers(ISubscribableRemoteUser user, eTrackType trackType)
		{
			var tracks = user.SubscribeTrackManager?.Tracks;
			if (tracks == null)
				yield break;

			if (!tracks.TryGetValue(trackType, out var track))
				yield break;

			foreach (var observer in track!.Observers)
				yield return observer;
		}
#endregion // Communication

#region Device
		[MetaverseCheat(Header + "[Device] " + nameof(ChangeVoiceSettings))] [HelpText("1,000~3,600,000", "1~int.MaxValue")]
		private static void ChangeVoiceSettings(string length = "5000", string frequency = "16000")
		{
			if (string.IsNullOrEmpty(length) || string.IsNullOrEmpty(frequency)) return;

			var intLength    = int.Parse(length);
			var intFrequency = int.Parse(frequency);

			ModuleManager.Instance.VoiceSettings.ChangeSettings(intLength, intFrequency);
		}

		[MetaverseCheat(Header + "[Device] " + nameof(ChangeCameraSettings))] [HelpText("145~4096", "49~4096", "0~240")]
		private static void ChangeCameraSettings(string width = "480", string height = "360", string fps = "15")
		{
			if (string.IsNullOrEmpty(width) || string.IsNullOrEmpty(height) || string.IsNullOrEmpty(fps)) return;

			var intWidth  = int.Parse(width);
			var intHeight = int.Parse(height);
			var intFps    = int.Parse(fps);

			ModuleManager.Instance.CameraSettings.ChangeSettings(intWidth, intHeight, intFps);
		}

		[MetaverseCheat(Header + "[Device] " + nameof(ChangeScreenSettings))] [HelpText("145~4096", "49~4096", "0~240")]
		private static void ChangeScreenSettings(string width = "1920", string height = "1080", string fps = "10")
		{
			if (string.IsNullOrEmpty(width) || string.IsNullOrEmpty(height) || string.IsNullOrEmpty(fps)) return;

			var intWidth  = int.Parse(width);
			var intHeight = int.Parse(height);
			var intFps    = int.Parse(fps);

			ModuleManager.Instance.ScreenSettings.ChangeSettings(intWidth, intHeight, intFps);
		}
#endregion // Device

#region WebRTC
		[MetaverseCheat(Header + "[WebRTC] " + nameof(ChangeVoicePublishSettings))] [HelpText("0~5,000,000")]
		private static void ChangeVoicePublishSettings(string bitrate = "500000")
		{
			if (string.IsNullOrEmpty(bitrate)) return;

			var intBitrate = int.Parse(bitrate);

			ModuleManager.Instance.VoicePublishSettings.ChangeSettings(intBitrate);
		}

		[MetaverseCheat(Header + "[WebRTC] " + nameof(ChangeCameraPublishSettings))] [HelpText("0~240", "0~5,000,000", "1.0~4.0")]
		private static void ChangeCameraPublishSettings(string fps = "15", string bitrate = "500000", string scale = "1.0")
		{
			if (string.IsNullOrEmpty(fps) || string.IsNullOrEmpty(bitrate) || string.IsNullOrEmpty(scale)) return;

			var intFps     = int.Parse(fps);
			var intBitrate = int.Parse(bitrate);
			var floatScale = float.Parse(scale);

			ModuleManager.Instance.CameraPublishSettings.ChangeSettings(intFps, intBitrate, floatScale);
		}

		[MetaverseCheat(Header + "[WebRTC] " + nameof(ChangeScreenPublishSettings))] [HelpText("0~240", "0~5,000,000", "1.0~4.0")]
		private static void ChangeScreenPublishSettings(string fps = "10", string bitrate = "4000000", string scale = "1.0")
		{
			if (string.IsNullOrEmpty(fps) || string.IsNullOrEmpty(bitrate) || string.IsNullOrEmpty(scale)) return;

			var intFps     = int.Parse(fps);
			var intBitrate = int.Parse(bitrate);
			var floatScale = float.Parse(scale);

			ModuleManager.Instance.ScreenPublishSettings.ChangeSettings(intFps, intBitrate, floatScale);
		}
#endregion // WebRTC

#region AI
		[MetaverseCheat(Header + "[AI] " + nameof(ChangeMattingUpdateFps))] [HelpText("0~240, (-1: Reset)")]
		private static void ChangeMattingUpdateFps(string fps = "-1")
		{
			if (string.IsNullOrEmpty(fps)) return;

			var intFps = int.Parse(fps);

			ModuleManager.Instance.HumanMattingTexturePipeline.RefreshFps = intFps;
		}

		[MetaverseCheat(Header + "[AI] " + nameof(ToggleVoiceNoiseReductionAI))]
		private static void ToggleVoiceNoiseReductionAI()
		{
			ModuleManager.Instance.VoiceNoiseReductionPipeline.IsRunning ^= true;
		}

		[MetaverseCheat(Header + "[AI] " + nameof(ToggleCameraHumanMattingAI))]
		private static void ToggleCameraHumanMattingAI()
		{
			ModuleManager.Instance.HumanMattingTexturePipeline.IsRunning ^= true;
		}
#endregion // AI
	}
}
#endif
