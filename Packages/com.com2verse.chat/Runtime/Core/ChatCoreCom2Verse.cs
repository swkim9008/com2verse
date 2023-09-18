/*===============================================================
* Product:		Com2Verse
* File Name:	ChatCoreCom2Verse.cs
* Developer:	ksw
* Date:			2022-12-29 13:58
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Com2Verse.Logger;
using Com2Verse.Network;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;

namespace Com2Verse.Chat
{
	public sealed partial class ChatCoreCom2Verse : ChatCoreBase, IDisposable
	{
		private struct ReceivedMessageStruct
		{
			public ChatApi.eMessageType Type;
			public string               Message;

			public ReceivedMessageStruct(ChatApi.eMessageType type, string message)
			{
				Type    = type;
				Message = message;
			}
		}

		private static readonly int AppID = 20827;

		private static readonly string URLPattern = @"([\w+]+\:\/\/)?(([0-9]{1,3}([\.]{1}[0-9]{1,3}){3})|([\w-]{3,}([\.]{1}[\w-]{2,}){1,3}))(\:[0-9]+)?([\/]{1}[\w-\/\?\=\&\#\.]+)*\/?";

		private string _chatUrl = string.Empty;

		private long                    _currentWhisperTarget;
		private LinkedList<MessageInfo> _areaMessages    = new();
		private LinkedList<MessageInfo> _nearbyMessages  = new();
		private LinkedList<MessageInfo> _whisperMessages = new();
		private LinkedList<MessageInfo> _allMessages = new();

		private readonly ConcurrentQueue<EventArgs> _chatEventArgsQueue = new();

		private readonly ConcurrentQueue<ReceivedMessageStruct> _messageQueue = new();

		private int _chatSaveCount = 300;
		public event Action OnConnectComplete;
		public event Action<CloseEventArgs> OnDisconnectComplete;
		public event Action<object> OnReceivedCustomData;
		public event Action<string> OnAreaEnter;
		[NotNull] private Func<bool> _isSendRelayMessageFunc = () => false;

		/// <summary>
		/// 유저가 standby 상태가 아닐때 지역메시지를 받지 않습니다
		/// </summary>
		public bool UserStandBy            { get; set; } = false;

		public long DeviceId { get; set; } = 0;

		public override void SetChatTableData(int chatSaveCount)
		{
			_chatSaveCount = chatSaveCount;
		}

		public override void Initialize()
		{
			OnEnterUserInGroup = null;
			OnLeaveUserInGroup = null;
		}

		public void SetChatUrl(string chatUrl) => _chatUrl = chatUrl;

		public override void UpdateQueue()
		{
			while (_chatEventArgsQueue.TryDequeue(out var eventArgs))
				ProcessReceivedServerEvent(eventArgs);

			while (_messageQueue.TryDequeue(out var messageStruct))
				ProcessMessage(messageStruct);
		}

		public override void ResetProperties()
		{
			
		}

		public override void ReleaseObjects()
		{
			_areaMessages.Clear();
			_nearbyMessages.Clear();
			_whisperMessages.Clear();
			_allMessages.Clear();
		}

		public void SendSocketCommunicationMessage(object data)
		{
			RequestSendCustomDataToArea(data);
		}

		public void SendAreaMessage(string message, string userName)
		{
			RequestSendAreaMessage(message, userName);
		}

		public void SendPrivateMessage(string message, string targetId)
		{
			RequestSendPrivateMessage(message, targetId);
		}

#region ChatClient Origin
#region Variables
		private static readonly string WorldAreaName = "WORLD";
		private                 string _serverUrl;
		private                 string _token;
		private                 string _sender;
		private                 string _currentArea;

		private WebSocketClient _webSocketClient;

		private readonly Dictionary<string, List<ChatApi.SendAreaMessageResponsePayload>> _chatMessages = new();
#endregion // Variables

#region Properties
		public string CurrentArea  => _currentArea;

		public IReadOnlyDictionary<string, List<ChatApi.SendAreaMessageResponsePayload>> ChatMessages => _chatMessages;
		public IReadOnlyList<ChatApi.SendAreaMessageResponsePayload> CurrentAreaMessages
		{
			get
			{
				if (_currentArea == null || !_chatMessages.ContainsKey(_currentArea))
					return Array.Empty<ChatApi.SendAreaMessageResponsePayload>();

				return _chatMessages[_currentArea];
			}
		}
#endregion // Properties

#region Public Functions
		/// <summary>
		/// V1 채팅서버 url 연결
		/// </summary>
		public void SetServerUrl(string serverAddr, string userId, long deviceId, int appId, string token = null)
		{
			_sender    = userId;
			_serverUrl = ChatApi.MakeChatServerUrl(serverAddr, userId, deviceId, appId);
			_token     = token;
		}

		/// <summary>
		/// V2 채팅서버 url 연결
		/// </summary>
		private void SetServerUrl(string serverAddr, string userId, ulong hiveUserId, long deviceId, int appId, string token = null)
		{
			_sender    = userId;
			_serverUrl = ChatApi.MakeChatServerUrl(serverAddr, userId, deviceId, appId, hiveUserId);
			_token     = token;
		}

		private void Connect()
		{
			if (string.IsNullOrWhiteSpace(_serverUrl))
			{
				C2VDebug.LogWarning("invalid server url");
				return;
			}
			if (_webSocketClient is {IsConnected: true})
			{
				C2VDebug.LogWarning($"client already connected. {_serverUrl}");
				return;
			}

			_webSocketClient                  =  new WebSocketClient();
			_webSocketClient.OnConnected      += OnServerConnected;
			_webSocketClient.OnDisconnected   += OnServerDisconnected;
			_webSocketClient.OnReceivedString += OnReceivedString;
			_webSocketClient.OnEventReceived  += ChatServerEventReceived;
			_webSocketClient.OnStateChanged   += ChatServerStateChanged;
			_webSocketClient.Connect(_serverUrl, _token);
		}

		private void OnServerConnected()
		{
			OnConnectComplete?.Invoke();
		}

		private void OnServerDisconnected(CloseEventArgs e)
		{
			OnDisconnectComplete?.Invoke(e);
		}

		public void RequestSendMessage(string message)
		{
			var sendMessageRequestPayload = new ChatApi.SendMessageRequestPayload
			{
				GroupId    = _currentArea,
				SendUserId = _sender,
				Comments = new[]
				{
					ChatApi.Comment.NewText(message),
				},
			};

			RequestSendMessage(sendMessageRequestPayload);
		}

		public void RequestSendPrivateMessage(string message)
		{
			var sendMessageRequestPayload = new ChatApi.SendPrivateMessageRequestPayload()
			{
				ReceiveUserId = _currentArea,
				SendUserId    = _sender,
				Comments = new[]
				{
					ChatApi.PrivateComment.NewText(message),
				},
			};

			RequestSendPrivateMessage(sendMessageRequestPayload);
		}

		public IReadOnlyList<ChatApi.SendAreaMessageResponsePayload> GetChatListByCurrentArea() => GetChatListByArea(_currentArea);
		
		public IReadOnlyList<ChatApi.SendAreaMessageResponsePayload> GetChatListByArea(string areaName)
		{
			if (!_chatMessages.ContainsKey(areaName)) return Array.Empty<ChatApi.SendAreaMessageResponsePayload>();

			return _chatMessages[areaName];
		}
		
		public void ClearChatByArea(string areaName)
		{
			if (!_chatMessages.ContainsKey(areaName)) return;

			_chatMessages[areaName]?.Clear();
			_chatMessages.Remove(areaName);
		}
		
		public void ClearAllChat()
		{
			if (_chatMessages == null || _chatMessages.Keys.Count == 0) return;

			var keys = _chatMessages.Keys.ToArray();
			foreach (var key in keys)
			{
				C2VDebug.Log($"CLEAR CHAT [{key}] = {_chatMessages[key].Count}");
				_chatMessages[key].Clear();
				_chatMessages.Remove(key);
			}
			_chatMessages.Clear();
		}

		public void AreaMove(string areaName)
		{
			if (_currentArea == areaName)
			{
				//C2VDebug.LogWarning($"chat move area failed. same area = {areaName}");
				return;
			}

			if (!string.IsNullOrWhiteSpace(_currentArea))
				AreaExit(_currentArea);
			AreaEnter(areaName);
		}

		private void AreaEnter(string groupId)
		{
			C2VDebug.Log("Current Area GroupId Set : " + groupId);
			_currentArea = groupId;
			OnAreaEnter?.Invoke(groupId);
		}

		public void AreaExit(string groupId)
		{
		}

		public void RequestSendCustomDataToArea(object data)
		{
			var sendSocketCommunicationPayload = new ChatApi.SendAreaMessageRequestPayload
			{
				SendUserId = _sender,
				GroupId    = _currentArea,
				Relay      = _isSendRelayMessageFunc.Invoke(),
				Comments = new[]
				{
					ChatApi.AreaComment.NewCustomData(data),
				},
			};
			RequestSendAreaMessage(sendSocketCommunicationPayload);
		}

		public void RequestSendAreaMessage(string message, string userName)
		{
			var sendAreaMessagePayload = new ChatApi.SendAreaMessageRequestPayload
			{
				GroupId      = _currentArea,
				SendUserId   = _sender,
				SendUserName = userName,
				Relay        = _isSendRelayMessageFunc.Invoke(),
				Comments = new[]
				{
					ChatApi.AreaComment.NewText(message),
				},
			};

			RequestSendAreaMessage(sendAreaMessagePayload);
		}

		public void RequestSendPrivateMessage(string message, string receiverUserId)
		{
			if (long.TryParse(_sender, out var senderId))
			{
				Protocols.CommonLogic.WhisperChattingRequest moveCommand = new()
				{
					TargetAvatarName = receiverUserId,
					Contents         = message,
					SourceAccountId  = senderId,
				};
				NetworkManager.Instance.Send(moveCommand, Protocols.CommonLogic.MessageTypes.WhisperChattingRequest);
			}
			else
			{
				C2VDebug.LogError("invalid sender id");
			}
		}
#endregion // Public Function

#region Wrapper
		public bool IsOpen => _webSocketClient?.IsConnected ?? false;
		public WebSocketClient.eWebSocketState State => _webSocketClient?.State ?? WebSocketClient.eWebSocketState.CLOSED;
		private void RequestSendMessage([NotNull] ChatApi.SendMessageRequestPayload payload)
		{
			if (!Validate()) return;

			var request = ChatApi.CreateSendMessageRequest(payload);
			Send(request);
		}

		public void RequestReadMessage([NotNull] ChatApi.ReadMessageRequestPayload payload)
		{
			if (!Validate()) return;

			var request = ChatApi.CreateReadMessageRequest(payload);
			Send(request);
		}

		public void RequestWithdrawMessage([NotNull] ChatApi.WithdrawMessageRequestPayload payload)
		{
			if (!Validate()) return;

			var request = ChatApi.CreateWithdrawMessageRequest(payload);
			Send(request);
		}

		public void RequestForwardMessage([NotNull] string senderUserId, [NotNull] string groupId, [NotNull] ChatApi.ForwardMessage[] forwardMessages)
		{
			if (!Validate()) return;

			var request = ChatApi.CreateForwardMessageRequest(senderUserId, groupId, forwardMessages);
			Send(request);
		}

		public void RequestGroupInvite([NotNull] ChatApi.GroupInviteRequestPayload payload)
		{
			if (!Validate()) return;

			var request = ChatApi.CreateGroupInviteRequest(payload);
			Send(request);
		}

		public void RequestGroupExit([NotNull] ChatApi.GroupExitRequestPayload payload)
		{
			if (!Validate()) return;

			var request = ChatApi.CreateGroupExitRequest(payload);
			Send(request);
		}

		private void RequestSendAreaMessage([NotNull] ChatApi.SendAreaMessageRequestPayload payload)
		{
			if (!Validate()) return;

			var request = ChatApi.CreateSendAreaMessageRequest(payload);
			Send(request);
		}

		private void RequestSendPrivateMessage([NotNull] ChatApi.SendPrivateMessageRequestPayload payload)
		{
			if (!Validate()) return;

			var request = ChatApi.CreateSendPrivateMessageRequest(payload);
			Send(request);
		}
		private void Send(string request)
		{
			_webSocketClient?.Send(request);
		}
#endregion // Wrapper

#region Private Function
		private bool Validate()
		{
			if (!IsOpen)
			{
				C2VDebug.LogWarning($"chat invalid connection state = {State}");
				return false;
			}

			return true;
		}

#region Handle Response Message
		protected override void OnHandleMessageType(ChatApi.eMessageType type, string messageString)
		{
			_messageQueue.Enqueue(new ReceivedMessageStruct(type, messageString));
		}

		private void ProcessMessage(ReceivedMessageStruct data)
		{
			var type = data.Type;
			var messageString = data.Message;

			switch (type)
			{
				case ChatApi.eMessageType.MESSAGE_READ:
					HandleReadMessage(messageString);
					break;
				case ChatApi.eMessageType.MESSAGE_WITHDRAW:
					HandleWithdrawMessage(messageString);
					break;
				case ChatApi.eMessageType.MESSAGE_FORWARD:
					HandleForwardMessage(messageString);
					break;
				case ChatApi.eMessageType.GROUP_INVITE:
					HandleGroupInviteMessage(messageString);
					break;
				case ChatApi.eMessageType.GROUP_EXIT:
					HandleGroupExitMessage(messageString);
					break;
				case ChatApi.eMessageType.MESSAGE_SEND:
					HandleReceivedMessage(messageString);
					break;
				case ChatApi.eMessageType.AREA_ENTER:
				case ChatApi.eMessageType.AREA_EXIT:
					HandleAreaEnterExit(type, messageString);
					break;
				case ChatApi.eMessageType.AREA_MESSAGE_SEND:
					HandleReceivedMessage(messageString);
					break;
				default:
					C2VDebug.LogWarning($"invalid chat message type = {type}");
					break;
			}
		}

		protected override void OnHandleError(int code, string message)
		{
			C2VDebug.LogWarning($"CHAT REQUEST ERROR = {message} ({code})");
		}

		private void HandleReceivedMessage(string responseJson)
		{
			if (string.IsNullOrEmpty(responseJson))
			{
				C2VDebug.LogErrorCategory(GetType().Name, "invalid response json");
				return;
			}

			if (string.IsNullOrEmpty(_currentArea))
			{
				C2VDebug.LogWarningCategory(GetType().Name, "invalid area, please check enter area");
				return;
			}

			// 지역채팅의 경우 유저가 StandBy상태가 아니면 무시
			if (!UserStandBy)
				return;

			var response = JsonConvert.DeserializeObject<ChatApi.ResponseSendAreaMessage>(responseJson);

			// 현재 지역이 아니면 무시
			if (response?.Payload?.GroupId != _currentArea)
				return;

			if (!_chatMessages.ContainsKey(_currentArea!))
				_chatMessages.Add(_currentArea, new List<ChatApi.SendAreaMessageResponsePayload> { response.Payload });
			else
				_chatMessages[_currentArea]!.Add(response.Payload);

			foreach (var sendMessageComment in response.Payload.Comments)
			{
				switch (ChatApi.GetCommentsType(sendMessageComment.Type))
				{
					case ChatApi.eCommentsType.TEXT:
					case ChatApi.eCommentsType.MENTION:
					case ChatApi.eCommentsType.URL:
					case ChatApi.eCommentsType.FILE:
					case ChatApi.eCommentsType.EMOTICON:
						HandleAreaChatMessage(response.Mobile);
						break;
					case ChatApi.eCommentsType.CUSTOM:
						OnReceivedCustomData?.Invoke(sendMessageComment.CustomData);
						break;
					default:
						C2VDebug.LogWarning($"invalid comment type = {sendMessageComment.Type}");
						break;
				}
			}
		}

		private void HandleAreaEnterExit(ChatApi.eMessageType type, string messageString)
		{
			JObject jObject = null;
			try
			{
				jObject = JObject.Parse(messageString);
			}
			catch (Exception exception)
			{
				C2VDebug.LogError($"{exception.Message}\n{exception.StackTrace}");
				throw;
			}

			switch (type)
			{
				case ChatApi.eMessageType.AREA_ENTER:
					var groupId = jObject["payload"]?["groupId"];
					var enterUserIdToken   = jObject["payload"]?["sendUserId"];
					var enterUserNameToken = jObject["payload"]?["sendUserName"];
					if (groupId == null || enterUserIdToken == null)
						return;

					C2VDebug.Log("Received Area Enter   GroupId : " + groupId + "  UserId : " + enterUserIdToken + "   UserName : " + enterUserNameToken);
					try
					{
						OnEnterUserInGroup?.Invoke((string)groupId, (long)enterUserIdToken, (string)enterUserNameToken);
					}
					catch (ArgumentException e)
					{
						C2VDebug.LogErrorCategory(GetType().Name, e.Message);
					}
					break;
				case ChatApi.eMessageType.AREA_EXIT:
					var leaveUserIdToken   = jObject["payload"]?["sendUserId"];
					var leaveUserNameToken = jObject["payload"]?["sendUserName"];
					if (leaveUserIdToken == null || leaveUserNameToken == null)
						return;

					try
					{
						OnLeaveUserInGroup?.Invoke((long)leaveUserIdToken, (string)leaveUserNameToken);
					}
					catch (ArgumentException e)
					{
						C2VDebug.LogErrorCategory(GetType().Name, e.Message);
					}
					break;
			}
		}

		private void HandleReadMessage(string responseJson)
		{
			// TODO : 필요시 구현
			// var readMessage = JsonConvert.DeserializeObject<ChatApi.ReadMessageResponsePayload>(responseJson);
		}

		private void HandleWithdrawMessage(string responseJson)
		{
			// TODO : 필요시 구현
			// var withdrawMessage = JsonConvert.DeserializeObject<ChatApi.WithdrawMessageResponsePayload>(responseJson);
		}
		private void HandleForwardMessage(string responseJson)
		{
			// TODO : 필요시 구현
			// var forwardMessage = JsonConvert.DeserializeObject<ChatApi.ForwardMessageResponsePayload>(responseJson);
		}
		private void HandleGroupInviteMessage(string responseJson)
		{
			// TODO : 필요시 구현
			// var groupInviteMessage = JsonConvert.DeserializeObject<ChatApi.GroupInviteResponsePayload>(responseJson);
		}
		private void HandleGroupExitMessage(string responseJson)
		{
			// TODO : 필요시 구현
			// var groupExitMessage = JsonConvert.DeserializeObject<ChatApi.GroupExitResponsePayload>(responseJson);
		}
#endregion // Handle Response Message
#endregion // Private Function

		public void Dispose()
		{
			_webSocketClient.OnConnected      -= OnServerConnected;
			_webSocketClient.OnDisconnected   -= OnServerDisconnected;
			_webSocketClient.OnReceivedString -= OnReceivedString;
			_webSocketClient.OnEventReceived  -= ChatServerEventReceived;
			_webSocketClient.OnStateChanged   -= ChatServerStateChanged;
			
			_webSocketClient?.Dispose();
			_webSocketClient = null;
		}
#endregion ChatClient Origin

#region WebSocket Connection
		public void ConnectChatServerV1(long uid, string token = null)
		{
			C2VDebug.Log(GetType().Name, $"ConnectChatServer - userId: {uid}");
			SetServerUrl(_chatUrl, uid.ToString(), DeviceId, AppID, token);
			Connect();
		}

		public void ConnectChatServerV2(long uid, ulong hiveUserId, string token = null)
		{
			C2VDebug.Log(GetType().Name, $"ConnectChatServer - userId: {uid}, hiveUserId: {hiveUserId}");
			SetServerUrl(_chatUrl, uid.ToString(), hiveUserId, DeviceId, AppID, token);
			Connect();
		}

		public void DisconnectChatServer()
		{
			if (_webSocketClient != null)
			{
				_webSocketClient.OnConnected      -= OnServerConnected;
				_webSocketClient.OnDisconnected   -= OnServerDisconnected;
				_webSocketClient.OnReceivedString -= OnReceivedString;
				_webSocketClient.OnEventReceived  -= ChatServerEventReceived;
				_webSocketClient.OnStateChanged   -= ChatServerStateChanged;
				_webSocketClient.Stop();
				OnServerDisconnected(null);
			}
		}

		public void EnterAreaChat(string groupId)
		{
			AreaEnter(groupId);
		}

		public void ExitAreaChat(string groupId)
		{
			AreaExit(groupId);
		}
#endregion

#region String Manipulation
		private string ConvertUrlFromMessage(eMessageType type, string message)
		{
			MatchCollection matches = Regex.Matches(message!, URLPattern!);
			for (int i = matches.Count - 1; i >= 0; --i)
			{
				Match match = matches[i];
				var   url   = match.Value;
				if (!url.StartsWith("https://") && !url.StartsWith("http://"))
					url = url.Insert(0, "https://");
				if (!IsVaildURL(url)) continue;
				message = message.Insert(match.Index + match.Value.Length,
				                         $"</u></link>")
				                 .Insert(match.Index, $"<#41B5FF><link=\"{match.Value}\"><u>");
			}
			return message;
		}

		public override void SendSystemMessage(string message, string time, string senderName)
		{
			MessageInfo info = new MessageInfo()
			{
				Message = message,
				Type = eMessageType.SYSTEM,
				SenderName = senderName,
				TimeString = time,
				UserID = 0,
			};
			//info.Message = CreateChatMessage(info.Type, info.SenderName, info.Message, info.TimeString);
			AddAllMessageList(info);
		}

		private string ConvertToDateFormatOnlyTime(long seconds)
		{
			var init     = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var dateTime = init.AddSeconds(seconds);
			return ConvertUtcToKst(dateTime).ToString("HH:mm");
		}

		private DateTime ConvertUtcToKst(DateTime utc)
		{
			//var kstTime     = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");
			var kstDateTime = TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.Local);
			return kstDateTime;
		}

		private bool IsVaildURL(string url)
		{
			try
			{
				//Creating the HttpWebRequest
				HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
				//Setting the Request method HEAD, you can also use GET too.
				request.Method = "HEAD";
				//Getting the Web Response.
				HttpWebResponse response = request.GetResponse() as HttpWebResponse;
				//Returns TRUE if the Status code == 200
				response.Close();
				return (response.StatusCode == HttpStatusCode.OK);
			}
			catch
			{
				//Any exception will returns false.
				return false;
			}
		}
#endregion

#region Check Relay Message
		public void SetSendRelayMessageFunc(Func<bool> isSendRelayMessageFunc)
		{
			if (isSendRelayMessageFunc == null) return;

			_isSendRelayMessageFunc = isSendRelayMessageFunc;
		}
#endregion // Check Relay Message
	}
}
