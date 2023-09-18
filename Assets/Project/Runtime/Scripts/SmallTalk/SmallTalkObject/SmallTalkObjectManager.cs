// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	SmallTalkObjectManager.cs
//  * Developer:	yangsehoon
//  * Date:		2023-06-23 오후 12:53
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Communication;
using Com2Verse.Data;
using Com2Verse.EventTrigger;
using Com2Verse.Interaction;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.PhysicsAssetSerialization;
using Com2Verse.UI;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Protocols.Communication;
using Protocols.GameLogic;
using Localization = Com2Verse.UI.Localization;
using PacketReceiver = Com2Verse.Network.GameLogic.PacketReceiver;

namespace Com2Verse.SmallTalk.SmallTalkObject
{
	public class SmallTalkObjectManager : DestroyableMonoSingleton<SmallTalkObjectManager>
	{
		public event Action<bool> ConnectionChanged;
		private IChannel _channel;
		public bool IsConnected;
		private GUIView _stateDisplay;
		private SmallTalkObjectManager() { }
		private string _channelId;
		private C2VEventTrigger _currentSmallTalkTrigger;

		public Dictionary<BaseMapObject, InteractionStringParameterSource> ParameterSourcesMap { get; set; } = new ();
		public InteractionLink LastRequestedInteractionLink { get; set; }

		private void OnEnable()
		{
			PacketReceiver.Instance.EnterObjectInteractionSmalltalkNotify += OnEnterObjectSmallTalkNotify;
		}

		private void OnDisable()
		{
			PacketReceiver.Instance.EnterObjectInteractionSmalltalkNotify -= OnEnterObjectSmallTalkNotify;
		}
		
		private bool IsInTrigger => TriggerEventManager.Instance.IsInTrigger(_currentSmallTalkTrigger, LastRequestedInteractionLink.TriggerIndex - 1);

		private void OnEnterObjectSmallTalkNotify(EnterObjectInteractionSmallTalkNotify notify)
		{
			_currentSmallTalkTrigger = MapController.Instance.GetStaticObjectByID(notify.ObjectId)?.GetComponentInChildren<C2VEventTrigger>();
			if (!IsInTrigger) return;

			try
			{
				TriggerMediaChannelNotify objectChannel = new()
				{
					ChannelId = notify.RoomId,
					RtcChannelInfo = new()
					{
						ServerUrl = notify.MediaUrl,
						MediaName = notify.MediaId,
						Direction = Direction.Bilateral,
						AccessToken = Network.User.Instance.CurrentUserData.AccessToken,
						IceServerConfigs =
						{
							new List<IceServerConfig>()
							{
								new IceServerConfig
								{
									IceServerUrl = notify.TurnServer?.Url,
									AccountName = notify.TurnServer?.Account,
									Credential = notify.TurnServer?.Password
								}, 
								new IceServerConfig
								{
									IceServerUrl = notify.StunServer?.Url
								}
							}
						}
					}
				};

				Connect(objectChannel);
			}
			catch (Exception e)
			{
				C2VDebug.LogError($"TriggerMediaChannelNotify data is null : {e.Message}");
			}
		}

		private void Connect(TriggerMediaChannelNotify objectChannel)
		{
			_channelId         = objectChannel.ChannelId;
			
			var rtcChannelInfo = objectChannel.RtcChannelInfo;

			if (_channelId == null || rtcChannelInfo == null)
			{
				C2VDebug.LogErrorMethod(nameof(SmallTalkDistance), "ChannelId or RtcChannelInfo is null");
				return;
			}
			
			var response = new JoinChannelResponse
			{
				ChannelId      = _channelId,
				ChannelType    = ChannelType.SmallTalk,
				RtcChannelInfo = rtcChannelInfo,
			};

			_channel = ChannelManagerHelper.JoinChannel(response, eUserRole.DEFAULT);
			_channel.ConnectionChanged += OnChannelConnectionChanged;
		}

		private void OnChannelConnectionChanged(IChannel iChannel, eConnectionState connectionState)
		{
			switch (connectionState)
			{
				case eConnectionState.DISCONNECTED:
					Disconnect();
					_stateDisplay?.Hide();
					IsConnected = false;
					ConnectionChanged?.Invoke(false);
					break;
				case eConnectionState.DISCONNECTING:
					break;
				case eConnectionState.CONNECTED:
					if (!IsInTrigger)
					{
						RemoveChannel();
						return;
					}
					
					UIManager.Instance.SendToastMessage(
						ZString.Format(Localization.Instance.GetString("UI_Interaction_Zone_Available_Toast"),
							Localization.Instance.GetString(InteractionManager.Instance.GetInteractionNameKey((eLogicType)_currentSmallTalkTrigger.Callback[0].Function))),
						3f, UIManager.eToastMessageType.NORMAL);
					
					UIManager.Instance.CreatePopup("UI_Display_State", guiView =>
					{
						guiView.Show();
						_stateDisplay = guiView;
						guiView.ViewModelContainer.GetViewModel<StateDisplay>().DisplayText = Localization.Instance.GetString("UI_Interaction_SmallTalkVoice_StatePopup");
					}).Forget();
					
					IsConnected = true;
					ConnectionChanged?.Invoke(true);
					break;
				case eConnectionState.CONNECTING:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(connectionState), connectionState, null);
			}
		}

		public void Disconnect()
		{
			var smallTalkObject = _currentSmallTalkTrigger.GetComponentInParent<BaseMapObject>();
			Commander.InstanceOrNull?.RequestExitObjectInteraction(smallTalkObject.ObjectID, InteractionManager.GetInteractionLinkId(smallTalkObject.ObjectTypeId, LastRequestedInteractionLink.TriggerIndex - 1, LastRequestedInteractionLink.CallbackIndex - 1));
			
			if (_channel == null)
				return;
			
			if (!IsConnected)
				UIManager.InstanceOrNull?.SendToastMessage(Localization.Instance.GetString("UI_SmallTalkVoice_Connect_Msg_FailedConnect"), 3f, UIManager.eToastMessageType.WARNING);
			
			_channel.ConnectionChanged -= OnChannelConnectionChanged;
			_channel = null;
			
			_channelId = null;
			_currentSmallTalkTrigger = null;
			ConnectionChanged?.Invoke(false);
		}
		
		public void RemoveChannel()
		{
			if (string.IsNullOrEmpty(_channelId))
				return;
		
			ChannelManager.Instance.RemoveChannel(_channelId);
		}
	}
}
