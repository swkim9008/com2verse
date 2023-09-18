/*===============================================================
* Product:		Com2Verse
* File Name:	AuditoriumController.cs
* Developer:	haminjeong
* Date:			2023-06-13 12:15
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Communication.Unity;
using Com2Verse.Data;
using Com2Verse.EventTrigger;
using Com2Verse.Extension;
using Com2Verse.Interaction;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.PhysicsAssetSerialization;
using Com2Verse.UI;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Protocols.GameLogic;
using Localization = Com2Verse.UI.Localization;

namespace Com2Verse.Communication
{
	[Serializable]
	public class GroupChannelInfo
	{
		public IChannel         Channel;
		public string           RoomId;
		public string           ServerUrl;
		public string           ServerName;
		public bool             IsSpeech;
		public eConnectionState State;
	}

	public sealed class AuditoriumController : Singleton<AuditoriumController>, IDisposable, IRemoteTrackObserver
	{
		private static readonly string DisplayStatePopupName = "UI_Display_State";

		private GroupChannelInfo _currentChannel;
		private GroupChannelInfo _prevChannel;

		private bool _isHighQuality = false;
		public bool IsHighQuality
		{
			get => _isHighQuality;
			set
			{
				if (_isHighQuality != value)
				{
					var noiseReductionViewModel = ViewModelManager.Instance.Get<NoiseReductionViewModel>();
					if (noiseReductionViewModel != null)
						noiseReductionViewModel.UseVoiceNoiseReduction = !value;
				}
				_isHighQuality = value;
			}
		}

		public GroupChannelInfo CurrentGroupChannel
		{
			get => _currentChannel;
			set
			{
				if (value is { IsSpeech: true })
					_prevChannel = _currentChannel;
				_currentChannel = value;
				if (_currentChannel is { IsSpeech: true })
				{
					if (_stateGUIView.IsUnityNull())
					{
						UIManager.Instance.CreatePopup(DisplayStatePopupName, guiView =>
						{
							guiView.Show();
							_stateGUIView = guiView;
							DelayedMicOnOff().Forget();
						}).Forget();
					}
					else
					{
						_stateGUIView!.Show();
						DelayedMicOnOff().Forget();
					}
					SpeakerConnectionChanged?.Invoke(true);
				}
				else
				{
					if (_currentChannel == null)
					{
						ResetAudioSettings();
						ClearVariables();
					}
					else
					{
						if (!_stateGUIView.IsUnityNull())
							_stateGUIView!.Hide();
					}
					SpeakerConnectionChanged?.Invoke(false);
				}
			}
		}

		public eConnectionState                                            CurrentState        => CurrentGroupChannel?.State ?? eConnectionState.DISCONNECTED;
		public Dictionary<BaseMapObject, InteractionStringParameterSource> ParameterSourcesMap { get; set; } = new();
		public C2VEventTrigger                                             CurrentMicTrigger   { get; set; }
		public ServerZone                                                  CurrentAudienceZone { get; set; }

		private bool IsInTrigger => CurrentMicTrigger != null && TriggerEventManager.Instance.IsInTrigger(CurrentMicTrigger, 0);
		private bool IsInZone    => CurrentAudienceZone != null && TriggerEventManager.Instance.IsInZone(CurrentAudienceZone);

		public event Action<bool> SpeakerConnectionChanged;

		private GUIView _stateGUIView;

		[UsedImplicitly]
		private AuditoriumController()
		{
			Com2Verse.Network.GameLogic.PacketReceiver.Instance.EnterAudioMuxMicAreaNotify   += OnEnterAudioMuxMicAreaNotify;
			Com2Verse.Network.GameLogic.PacketReceiver.Instance.EnterAudioMuxCrowdAreaNotify += OnEnterAudioMuxCrowdAreaNotify;
			Commander.Instance.OnEscapeEvent                                                 += OnEscape;
			NetworkManager.Instance.OnDisconnected                                           += OnLogout;
			ModuleManager.Instance.VoicePublishSettings.SettingsChanged                      += OnAudioPublishSettingChanged;
		}

		public void Dispose()
		{
			if (Com2Verse.Network.GameLogic.PacketReceiver.InstanceExists)
			{
				Com2Verse.Network.GameLogic.PacketReceiver.Instance.EnterAudioMuxMicAreaNotify   -= OnEnterAudioMuxMicAreaNotify;
				Com2Verse.Network.GameLogic.PacketReceiver.Instance.EnterAudioMuxCrowdAreaNotify -= OnEnterAudioMuxCrowdAreaNotify;
			}
			if (Commander.InstanceExists)
				Commander.Instance.OnEscapeEvent -= OnEscape;
			if (NetworkManager.InstanceExists)
				NetworkManager.Instance.OnDisconnected -= OnLogout;
			if (ModuleManager.InstanceExists)
				ModuleManager.Instance.VoicePublishSettings.SettingsChanged -= OnAudioPublishSettingChanged;
			LeaveChannel(false);
			ResetAudioSettings();
			ClearVariables();
			_stateGUIView = null;
			_speecherList!.Clear();
		}

		private void OnLogout()
		{
			OnEscape();
		}

		private void OnEscape()
		{
			LeaveChannel(false);
			ClearVariables();
			ResetAudioSettings();
			_stateGUIView = null;
			_speecherList!.Clear();
		}

		private void ClearVariables()
		{
			IsHighQuality      = false;
			CurrentAudienceZone = null;
			CurrentMicTrigger   = null;
			_currentChannel     = _prevChannel = null;
			if (!_stateGUIView.IsUnityNull())
				_stateGUIView!.Hide();
		}

#region AudioSettings
		private void OnAudioPublishSettingChanged(IReadOnlyAudioPublishSettings settings)
		{
			// 툴바의 기능을 통해서만 동작되어야 한다.
			if (_currentChannel is not { State: eConnectionState.CONNECTED } || !_currentChannel.IsSpeech) return;
			DelayedMicOnOff().Forget();
		}

		private async UniTask DelayedMicOnOff()
		{
			if (_currentChannel!.Channel != null && _currentChannel.Channel.Self is IPublishableLocalUser publishableLocalUser)
			{
				publishableLocalUser.Modules.TryAddConnectionBlocker(eTrackType.VOICE, this);
				await UniTask.DelayFrame(30);
				publishableLocalUser.Modules.RemoveConnectionBlocker(eTrackType.VOICE, this);
			}
			if (!IsInTrigger)
			{
				C2VDebug.LogWarningCategory("Auditorium", "Out of Trigger");
				if (!_stateGUIView.IsUnityNull())
					_stateGUIView!.Hide();
				return;
			}
			if (!_stateGUIView.IsUnityNull())
				_stateGUIView!.ViewModelContainer!.GetViewModel<StateDisplay>()!.DisplayText = Localization.Instance.GetString("UI_Interaction_Speech_StatePopup_1",
				                                                                                                               Localization.Instance.GetString(
					                                                                                                               IsHighQuality
						                                                                                                               ? "SoundQualityType_HighQuality"
						                                                                                                               : "SoundQualityType_LowQuality"));
		}

		private void ResetAudioSettings()
		{
			if (!ModuleManager.InstanceExists) return;
			ModuleManager.Instance.VoiceSettings.ChangeSettings(frequency: 16000);
			ModuleManager.Instance.VoicePublishSettings.ChangeSettings(20000);
		}
#endregion AudioSettings
		
		private void SetGroupChannelInfo(bool isSpeech)
		{
			CurrentGroupChannel = new GroupChannelInfo
			{
				IsSpeech = isSpeech,
				State = eConnectionState.DISCONNECTED,
			};
		}

#region Speecher Update
		private readonly List<long> _speecherList = new();

		public void UpdateSpeecherList(long id, bool isUse)
		{
			C2VDebug.LogCategory("Auditorium", $"id {id} is speech use {isUse}");
			if (isUse)
			{
				_speecherList!.TryAdd(id);
				OnEnterSpeecherObjectEvent?.Invoke(MapController.Instance.GetActiveObjectByID(id));
			}
			else
			{
				_speecherList!.TryRemove(id);
				OnExitSpeecherObjectEvent?.Invoke(MapController.Instance.GetActiveObjectByID(id));
			}
		}

		/// <summary>
		/// 새로운 화자가 등장할때 이벤트가 호출됩니다.
		/// </summary>
		public event Action<BaseMapObject> OnEnterSpeecherObjectEvent;
		/// <summary>
		/// 화자가 퇴장할때 이벤트가 호출됩니다.
		/// </summary>
		public event Action<BaseMapObject> OnExitSpeecherObjectEvent;

		public bool IsContainsSpeecher(long objectID) => _speecherList!.Contains(objectID);
#endregion Speecher Update

#region Notifies
		private void OnEnterAudioMuxMicAreaNotify(EnterAudioMuxMicAreaNotify notify)
		{
			if (!IsInTrigger)
			{
				C2VDebug.LogWarningCategory("Auditorium", "Out of Trigger");
				return;
			}
			
			// 기존 청중 채널 정리
			LeaveChannel(false, _ =>
			{
				// 연결 패킷을 받은시점에 트리거 밖에 있으면 연결하지 않는다.
				Protocols.Communication.JoinChannelResponse channelInfo;
				if (!IsInTrigger)
				{
					// 기존 채널로 재접속
					channelInfo = CreateChannelInfo(_currentChannel.RoomId, _currentChannel.ServerUrl, _currentChannel.ServerName);
					JoinChannel(channelInfo);
					C2VDebug.LogWarningCategory("Auditorium", "Out of Trigger");
					return;
				}

				// TODO: 값 테이블화
				if (IsHighQuality)
				{
					ModuleManager.Instance.VoiceSettings.ChangeSettings(frequency: 44100);
					ModuleManager.Instance.VoicePublishSettings.ChangeSettings(384000);
				}
				else
					ResetAudioSettings();

				SetGroupChannelInfo(true);
				UIManager.Instance.SendToastMessage(
					ZString.Format(Localization.Instance.GetString("UI_Interaction_Zone_Available_Toast"),
					               Localization.Instance.GetString(InteractionManager.Instance.GetInteractionNameKey(eLogicType.SPEECH))));
				channelInfo = CreateChannelInfo(notify.RoomId, notify.ServerUrl, notify.ServerName);
				JoinChannel(channelInfo);
			});
		}

		private void OnEnterAudioMuxCrowdAreaNotify(EnterAudioMuxCrowdAreaNotify notify)
		{
			// 연결 패킷을 받은시점에 존 밖에 있으면 연결하지 않는다.
			if (!IsInZone)
			{
				LeaveChannel(false);
				return;
			}

			SetGroupChannelInfo(false);
			UIManager.Instance.SendToastMessage(
				ZString.Format(Localization.Instance.GetString("UI_Interaction_Zone_Available_Toast"),
				               Localization.Instance.GetString(InteractionManager.Instance.GetInteractionNameKey(eLogicType.AUDIENCE))));
			var channelInfo = CreateChannelInfo(notify.RoomId, notify.ServerUrl, notify.ServerName);
			JoinChannel(channelInfo);
		}
#endregion Notifies

		private Protocols.Communication.JoinChannelResponse CreateChannelInfo(string roomId, string url, string name)
		{
			_currentChannel!.RoomId    = roomId;
			_currentChannel.ServerUrl  = url;
			_currentChannel.ServerName = name;

			var response = new Protocols.Communication.JoinChannelResponse
			{
				ChannelId      = roomId,
				ChannelType    = Protocols.Communication.ChannelType.Meeting,
				RtcChannelInfo = new Protocols.Communication.RTCChannelInfo
				{
					ServerUrl   = url,
					MediaName   = name,
					Direction   = _currentChannel.IsSpeech ? Protocols.Communication.Direction.Bilateral : Protocols.Communication.Direction.Incomming,
					AccessToken = Network.User.Instance.CurrentUserData.AccessToken,
				},
			};
			return response;
		}

		private void JoinChannel(Protocols.Communication.JoinChannelResponse response)
		{
			if (response        == null) return;
			if (_currentChannel == null) return;

			_currentChannel.Channel                   =  ChannelManagerHelper.JoinChannel(response, _currentChannel.IsSpeech ? eUserRole.DEFAULT : eUserRole.AUDIENCE);
			_currentChannel.Channel.ConnectionChanged += OnChannelConnectionChanged;
			_currentChannel.State                     =  eConnectionState.CONNECTING;
		}

		public void LeaveChannel(bool bContinuePrev = true, Action<bool> onAfterLeaveChannel = null)
		{
			if (_currentChannel?.Channel == null) return;

			if (_currentChannel.Channel != null)
			{
				if (bContinuePrev == _currentChannel.IsSpeech && _currentChannel.State >= eConnectionState.CONNECTED)
				{
					_currentChannel.State = eConnectionState.DISCONNECTING;
					ChannelManager.Instance.RemoveChannel(_currentChannel.RoomId);
				}
			}
			
			WaitForDisconnecting(bContinuePrev, onAfterLeaveChannel).Forget();
		}

		private void OnAfterLeaveChannelDefault(bool bContinuePrev)
		{
			if (bContinuePrev)
			{
				CurrentMicTrigger = null;
				ResetAudioSettings();
				CurrentGroupChannel = _prevChannel;
				_prevChannel        = null;
				if (_currentChannel?.Channel == null) return;
				UIManager.Instance.SendToastMessage(Localization.Instance.GetString("UI_Interaction_Speech_Disconnect_Toast"));
				var channelInfo = CreateChannelInfo(_currentChannel.RoomId, _currentChannel.ServerUrl, _currentChannel.ServerName);
				JoinChannel(channelInfo);
			}
			else
			{
				UIManager.InstanceOrNull?.SendToastMessage(Localization.InstanceOrNull?.GetString("UI_Interaction_Audience_Disconnect_Toast"));
				CurrentGroupChannel = null;
			}
		}

		private static readonly int WaitDisconnectTimeout = 3000;
		private async UniTaskVoid WaitForDisconnecting(bool bContinuePrev, Action<bool> afterCallback = null)
		{
			var expiredTime = MetaverseWatch.Time + WaitDisconnectTimeout;
			while (_currentChannel is { State: eConnectionState.DISCONNECTING })
			{
				if (MetaverseWatch.Time > expiredTime) break;
				await UniTask.Yield();
			}
			afterCallback ??= OnAfterLeaveChannelDefault;
			afterCallback.Invoke(bContinuePrev);
		}

#region Channel Events
		private void OnChannelConnectionChanged(IChannel channel, eConnectionState state)
		{
			switch (state)
			{
				case eConnectionState.CONNECTED:
				{
					if (channel.Equals(_currentChannel?.Channel))
					{
						if (_currentChannel!.IsSpeech && (CurrentMicTrigger == null || !IsInTrigger))
						{
							C2VDebug.LogWarningCategory("Auditorium", "Out of Trigger");
							LeaveChannel();
							return;
						}
						if (!_currentChannel!.IsSpeech && (CurrentAudienceZone == null || !IsInZone))
						{
							LeaveChannel(false);
							return;
						}
					}
					ConnectedChannel(channel);
					break;
				}
				case eConnectionState.DISCONNECTED:
				{
					CurrentChannelDisconnected(channel);
					break;
				}
			}
		}

		private void ConnectedChannel(IChannel channel)
		{
			C2VDebug.LogWarningCategory("Auditorium", $"Current Channel({channel.Info.ChannelId}) Connected");

			_currentChannel.State = eConnectionState.CONNECTED;
			if (_currentChannel.IsSpeech)
			{
				// publisher
				(channel.Self as IPublishableLocalUser)?.Modules.RemoveConnectionBlocker(eTrackType.VOICE, this);
			}
			else
			{
				// subscriber
				(channel.Self as IPublishableLocalUser)?.Modules.TryAddConnectionBlocker(eTrackType.VOICE, this);
			}
			channel.UserJoin += OnChannelUserJoined;
			channel.UserLeft += OnChannelUserLeft;
			foreach (var user in channel.ConnectedUsers.Values)
				OnChannelUserJoined(channel, user);
		}

		private void OnChannelUserJoined(IChannel arg1, ICommunicationUser arg2)
		{
			(arg2 as ISubscribableRemoteUser)?.TryAddObserver(eTrackType.VOICE, this);
			C2VDebug.LogWarningCategory("Auditorium", $"Current Channel({arg1.Info.ChannelId}) User({arg2.User.Name}) Joined");
		}

		private void OnChannelUserLeft(IChannel arg1, ICommunicationUser arg2)
		{
			(arg2 as ISubscribableRemoteUser)?.RemoveObserver(eTrackType.VOICE, this);
			C2VDebug.LogWarningCategory("Auditorium", $"Current Channel({arg1.Info.ChannelId}) User({arg2.User.Name}) Left");
		}

		private void CurrentChannelDisconnected(IChannel channel)
		{
			if (_currentChannel?.State == eConnectionState.CONNECTING)
				C2VDebug.LogErrorCategory("Auditorium", $"Cannot Join the Channel");

			if (_currentChannel != null)
				_currentChannel.State = eConnectionState.DISCONNECTED;
			
			if (channel == null) return;
			channel.UserJoin          -= OnChannelUserJoined;
			channel.UserLeft          -= OnChannelUserLeft;
			channel.ConnectionChanged -= OnChannelConnectionChanged;

			C2VDebug.LogWarningCategory("Auditorium", $"Current Channel({channel.Info.ChannelId}) Disconnected");
		}
#endregion Channel Events

		public string GetDebugInfo() => GetType().Name;
	}
}
