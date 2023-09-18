/*===============================================================
* Product:		Com2Verse
* File Name:	ChatApi.cs
* Developer:	jhkim
* Date:			2023-05-04 16:38
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Unity.Collections.LowLevel.Unsafe;

namespace Com2Verse.Chat
{
	public static class ChatApi
	{
#region Variables
		public enum eMessageType
		{
			MESSAGE_READ = 100, // 메시지 읽음
			MESSAGE_WITHDRAW = 101, // 메시지 회수
			MESSAGE_FORWARD = 102, // 메시지 전달
			GROUP_INVITE = 103, // 그룹 초대
			GROUP_EXIT = 104, // 그룹 나가기
			MESSAGE_SEND = 105, // 메시지 전송
			AREA_ENTER = 200, // 지역 입장
			AREA_EXIT = 201, // 지역 나가기
			AREA_MESSAGE_SEND = 202, // 지역 메시지 전송
			PRIVATE_MESSAGE_SEND = 203, // 1:1 메시지 전송
		}

		public enum eCommentsType
		{
			TEXT = 1000,
			MENTION = 1001,
			URL = 1002,
			FILE = 1003,
			EMOTICON = 1004,
			CUSTOM = 1005,
		}

		/// <summary>
		/// 채팅서버 V1 URL 포멧. 아래는 파라미터 인덱스별 설명
		/// 0 : 채팅 서버 주소
		/// 1 : 접속하는 기기 ID
		/// 2 : 접속하는 APP ID (컴투버스 = 1, PC, 모바일 메신저)
		/// 3 : 유저 고유값
		/// </summary>
		[NotNull] private static readonly string ChatServerV1URLFormat = "wss://{0}/chat?deviceId={1}&appId={2}&userId={3}";

		/// <summary>
		/// 채팅서버 V2 URL 포멧. 아래는 파라미터 인덱스별 설명
		/// 0 : 채팅 서버 주소
		/// 1 : 접속하는 기기 ID
		/// 2 : 접속하는 APP ID (컴투버스 = 1, PC, 모바일 메신저)
		/// 3 : 유저 고유값
		/// 4 : hivePlayerId
		/// </summary>
		[NotNull] private static readonly string ChatServerV2URLFormat = "wss://{0}/chat/v2?deviceId={1}&appId={2}&userId={3}&hivePlayerId={4}";
#endregion // Variables

#region Public Functions
		public static string MakeChatServerUrl(string serverAddr, string userId, long deviceId, int appId) =>
			string.Format(ChatServerV1URLFormat, serverAddr, Convert.ToString(deviceId), Convert.ToString(appId), Convert.ToString(userId));
		public static string MakeChatServerUrl(string serverAddr, string userId, long deviceId, int appId, ulong hivePlayerId) =>
			string.Format(ChatServerV2URLFormat, serverAddr, Convert.ToString(deviceId), Convert.ToString(appId), Convert.ToString(userId), Convert.ToString(hivePlayerId));
		public static eMessageType GetMessageType(int code) => UnsafeUtility.As<int, eMessageType>(ref code);
		public static eCommentsType GetCommentsType(int code) => UnsafeUtility.As<int, eCommentsType>(ref code);
#endregion // Public Functions

#region Request
		internal static string CreateSendMessageRequest(SendMessageRequestPayload payload)
		{
			var request = new RequestSendMessage
			{
				MessageType = GetMessageTypeCode(eMessageType.MESSAGE_SEND),
				Payload = payload,
			};
			return ToJson(request);
		}

		internal static string CreateReadMessageRequest(ReadMessageRequestPayload payload)
		{
			var request = new RequestReadMessage
			{
				MessageType = GetMessageTypeCode(eMessageType.MESSAGE_READ),
				Payload = payload,
			};
			return ToJson(request);
		}

		internal static string CreateWithdrawMessageRequest(WithdrawMessageRequestPayload payload)
		{
			var request = new RequestWithdrawMessage
			{
				MessageType = GetMessageTypeCode(eMessageType.MESSAGE_WITHDRAW),
				Payload = payload,
			};
			return ToJson(request);
		}

		internal static string CreateForwardMessageRequest(string senderUserId, string groupId, ForwardMessage[] forwardMessages)
		{
			var request = new RequestForwardMessage
			{
				MessageType = GetMessageTypeCode(eMessageType.MESSAGE_FORWARD),
				SendUserId = senderUserId,
				GroupId = groupId,
				ForwardMessages = forwardMessages,
			};
			return ToJson(request);
		}

		internal static string CreateGroupInviteRequest(GroupInviteRequestPayload payload)
		{
			var request = new RequestGroupInvite
			{
				MessageType = GetMessageTypeCode(eMessageType.GROUP_INVITE),
				Payload = payload,
			};
			return ToJson(request);
		}

		internal static string CreateGroupExitRequest(GroupExitRequestPayload payload)
		{
			var request = new RequestGroupExit
			{
				MessageType = GetMessageTypeCode(eMessageType.GROUP_EXIT),
				Payload = payload,
			};
			return ToJson(request);
		}

		internal static string CreateSendAreaMessageRequest(SendAreaMessageRequestPayload payload)
		{
			var request = new RequestSendAreaMessage
			{
				MessageType = GetMessageTypeCode(eMessageType.AREA_MESSAGE_SEND),
				Payload = payload,
			};
			return ToJson(request);
		}

		internal static string CreateSendPrivateMessageRequest(SendPrivateMessageRequestPayload payload)
		{
			var request = new RequestSendPrivateMessage
			{
				MessageType = GetMessageTypeCode(eMessageType.PRIVATE_MESSAGE_SEND),
				Payload = payload,
			};
			return ToJson(request);
		}
		private static string ToJson<T>(T obj) where T : class => JsonConvert.SerializeObject(obj);
#endregion // Request

#region Private Functions
		private static int GetMessageTypeCode(eMessageType type) => UnsafeUtility.As<eMessageType, int>(ref type);
		private static int GetCommentsTypeCode(eCommentsType type) => UnsafeUtility.As<eCommentsType, int>(ref type);
#endregion // Private Functions

#region 메시지 전송 (SendMessage)
#region Request
		/// <summary>
		/// 메시지 전송
		/// 유저가 속한 그룹, area 로 메시지를 전송합니다.
		/// 발송 대상이 팀 그룹인 경우, 해당 팀이 속해 있는 area 로 메시지가 relay 됩니다.
		/// </summary>
		[Serializable]
		internal class RequestSendMessage
		{
			[JsonProperty("messageType")]
			public int MessageType;

			[JsonProperty("payload")]
			public SendMessageRequestPayload Payload;
		}

		[Serializable]
		internal class SendMessageRequestPayload
		{
			[JsonProperty("sendUserId")]
			public string SendUserId;

			[JsonProperty("groupId")]
			public string GroupId;

			[JsonProperty("comments")]
			public Comment[] Comments;

			[JsonProperty("parentMessage")]
			public SendMessageParentMessage ParentMessage;
		}
#endregion // Request
#region Response
		[Serializable]
		internal class ResponseSendMessage : ResponseBase
		{
			[JsonProperty("payload")]
			public SendMessageResponsePayload Payload;
		}

		[Serializable]
		public class SendMessageResponsePayload
		{
			[JsonProperty("messageTime")]
			public long MessageTime;

			[JsonProperty("messageId")]
			public long MessageId;

			[JsonProperty("parentMessage")]
			public SendMessageParentMessage ParentMessage;

			[JsonProperty("groupId")]
			public string GroupId;

			[JsonProperty("sendUserId")]
			public string SendUserId;

			[JsonProperty("comments")]
			public Comment[] Comments;
		}
#endregion // Response
#region Common
		[Serializable]
		public class SendMessageParentMessage
		{
			[JsonProperty("messageId")]
			public long MessageId;

			[JsonProperty("sendUserId")]
			public string SendUserId;

			[JsonProperty("comments")]
			public Comment[] Comments;
		}
#endregion // Common
#endregion // 메시지 전송 (SendMessage)

#region 메시지 읽음 (ReadMessage)
#region Request
		[Serializable]
		internal class RequestReadMessage
		{
			[JsonProperty("messageType")]
			public int MessageType;

			[JsonProperty("payload")]
			public ReadMessageRequestPayload Payload;
		}

		[Serializable]
		public class ReadMessageRequestPayload
		{
			[JsonProperty("sendUserId")]
			public string SendUserId;

			[JsonProperty("groupId")]
			public string GroupId;

			[JsonProperty("lastReadMessageId")]
			public string LastReadMessageId;

			[JsonProperty("lastMessageId")]
			public string LastMessageId;
		}
#endregion // Request
#region Response
		[Serializable]
		internal class ResponseReadMessage : ResponseBase
		{
			[JsonProperty("payload")]
			public ReadMessageResponsePayload Payload;
		}

		[Serializable]
		internal class ReadMessageResponsePayload
		{
			[JsonProperty("sendUserId")]
			public string SendUserId;

			[JsonProperty("groupId")]
			public string GroupId;

			[JsonProperty("lastReadMessageId")]
			public long LastReadMessageId;

			[JsonProperty("lastMessageId")]
			public long LastMessageId;
		}
#endregion // Response
#endregion // 메시지 읽음 (ReadMessage)

#region 메시지 회수 (WithdrawMessage)
#region Request
		[Serializable]
		internal class RequestWithdrawMessage
		{
			[JsonProperty("messageType")]
			public int MessageType;

			[JsonProperty("payload")]
			public WithdrawMessageRequestPayload Payload;
		}

		[Serializable]
		public class WithdrawMessageRequestPayload
		{
			[JsonProperty("sendUserId")]
			public string SendUserId;

			[JsonProperty("groupId")]
			public string GroupId;

			[JsonProperty("withdrawMessageId")]
			public long WithdrawMessageId;
		}
#endregion // Request
#region Response
		[Serializable]
		internal class ResponseWithdrawMessage : ResponseBase
		{
			[JsonProperty("payload")]
			public WithdrawMessageResponsePayload Payload;
		}

		[Serializable]
		internal class WithdrawMessageResponsePayload
		{
			[JsonProperty("sendUserId")]
			public string SendUserId;

			[JsonProperty("groupId")]
			public string GroupId;

			[JsonProperty("messageTime")]
			public long MessageTime;

			[JsonProperty("messageId")]
			public long MessageId;

			[JsonProperty("withdrawMessageId")]
			public long WithdrawMessageId;
		}
#endregion // Response
#endregion // 메시지 회수 (WithdrawMessage)

#region 메시지 전달 (ForwardMessage)
#region Request
		[Serializable]
		internal class RequestForwardMessage
		{
			[JsonProperty("messageType")]
			public int MessageType;

			[JsonProperty("sendUserId")]
			public string SendUserId;

			[JsonProperty("groupId")]
			public string GroupId;

			[JsonProperty("forwardMessages")]
			public ForwardMessage[] ForwardMessages;
		}
#endregion // Request
#region Response
		[Serializable]
		internal class ResponseForwardMessage : ResponseBase
		{
			[JsonProperty("payload")]
			public ForwardMessageResponsePayload Payload;
		}

		[Serializable]
		internal class ForwardMessageResponsePayload
		{
			[JsonProperty("messageTime")]
			public long MessageTime;

			[JsonProperty("messageId")]
			public long MessageId;

			[JsonProperty("sendUserId")]
			public string SendUserId;

			[JsonProperty("groupId")]
			public string GroupId;

			[JsonProperty("forwardMessages")]
			public ForwardMessage[] ForwardMessages;
		}
#endregion // Response
#endregion // 메시지 전달 (ForwardMessage)

#region 그룹 초대 (GroupInvite)
#region Request
		[Serializable]
		internal class RequestGroupInvite
		{
			[JsonProperty("messageType")]
			public int MessageType;

			[JsonProperty("payload")]
			public GroupInviteRequestPayload Payload;
		}

		[Serializable]
		public class GroupInviteRequestPayload
		{
			[JsonProperty("sendUserId")]
			public string SendUserId;

			[JsonProperty("groupId")]
			public string GroupId;

			[JsonProperty("joinUserIds")]
			public string[] JoinUserIds;
		}
#endregion // Request
#region Response
		[Serializable]
		internal class ResponseGroupInvite : ResponseBase
		{
			[JsonProperty("payload")]
			public GroupInviteResponsePayload Payload;
		}

		[Serializable]
		internal class GroupInviteResponsePayload
		{
			[JsonProperty("messageTime")]
			public long MessageTime;

			[JsonProperty("messageId")]
			public long MessageId;

			[JsonProperty("groupId")]
			public string GroupId;

			[JsonProperty("sendUserId")]
			public string SendUserId;

			[JsonProperty("joinUserIds")]
			public string[] JoinUserIds;
		}
#endregion // Response
#endregion // 그룹 초대 (GroupInvite)

#region 그룹 나가기 (GroupExit)
#region Request
		[Serializable]
		internal class RequestGroupExit
		{
			[JsonProperty("messageType")]
			public int MessageType;

			[JsonProperty("payload")]
			public GroupExitRequestPayload Payload;
		}

		[Serializable]
		public class GroupExitRequestPayload
		{
			[JsonProperty("groupId")]
			public string GroupId;

			[JsonProperty("sendUserId")]
			public string SendUserId;

			[JsonProperty("leaveUserId")]
			public string LeaveUserId;
		}
#endregion // Request
#region Response
		[Serializable]
		internal class ResponseGroupExit : ResponseBase
		{
			[JsonProperty("payload")]
			public GroupExitResponsePayload Payload;
		}

		[Serializable]
		internal class GroupExitResponsePayload
		{
			[JsonProperty("messageTime")]
			public long MessageTime;

			[JsonProperty("messageId")]
			public long MessageId;

			[JsonProperty("groupId")]
			public string GroupId;

			[JsonProperty("sendUserId")]
			public string SendUserId;

			[JsonProperty("leaveUserId")]
			public string LeaveUserId;
		}
#endregion // Response
#endregion // 그룹 나가기 (GroupExit)

#region 지역 메시지 전송 (SendAreaMessage)
#region Request
		[Serializable]
		internal class RequestSendAreaMessage
		{
			[JsonProperty("messageType")]
			public int MessageType;

			[JsonProperty("payload")]
			public SendAreaMessageRequestPayload Payload;
		}

		[Serializable]
		public class SendAreaMessageRequestPayload
		{
			[JsonProperty("sendUserId")]
			public string SendUserId;

			[JsonProperty("sendUserName")]
			public string SendUserName;

			[JsonProperty("groupId")]
			public string GroupId;

			[JsonProperty("relay")]
			public bool Relay;

			[JsonProperty("comments")]
			public AreaComment[] Comments;
		}
#endregion // Request
#region Response
		[Serializable]
		internal class ResponseSendAreaMessage : ResponseBase
		{
			[JsonProperty("mobile")]
			public bool Mobile;

			[JsonProperty("payload")]
			public SendAreaMessageResponsePayload Payload;
		}

		[Serializable]
		public class SendAreaMessageResponsePayload
		{
			[JsonProperty("messageTime")]
			public long MessageTime;

			[JsonProperty("messageId")]
			public long MessageId;

			[JsonProperty("groupId")]
			public string GroupId;

			[JsonProperty("sendUserId")]
			public string SendUserId;

			[JsonProperty("sendUserName")]
			public string SendUserName;

			[JsonProperty("relay")]
			public bool Relay;

			[JsonProperty("comments")]
			public AreaComment[] Comments;
		}
#endregion // Response
#endregion // 지역 메시지 전송 (SendAreaMessage)

#region 1:1 메시지 전송 (SendPrivateMessage)
#region Request
		[Serializable]
		internal class RequestSendPrivateMessage
		{
			[JsonProperty("messageType")]
			public int MessageType;

			[JsonProperty("payload")]
			public SendPrivateMessageRequestPayload Payload;
		}

		[Serializable]
		public class SendPrivateMessageRequestPayload
		{
			[JsonProperty("sendUserId")]
			public string SendUserId;

			[JsonProperty("receiveUserId")]
			public string ReceiveUserId;

			[JsonProperty("comments")]
			public PrivateComment[] Comments;
		}
#endregion // Request
#region Response
		[Serializable]
		internal class ResponseSendPrivateMessage : ResponseBase
		{
			[JsonProperty("payload")]
			public SendPrivateMessageResponsePayload Payload;
		}

		[Serializable]
		public class SendPrivateMessageResponsePayload
		{
			[JsonProperty("messageTime")]
			public long MessageTime;

			[JsonProperty("messageId")]
			public long MessageId;

			[JsonProperty("sendUserId")]
			public string SendUserId;

			[JsonProperty("receiveUserId")]
			public string ReceiveUserId;

			[JsonProperty("comments")]
			public PrivateComment[] Comments;
		}
#endregion // Response
#endregion // 1:1 메시지 전송 (SendPrivateMessage)

#region Common
		[Serializable]
		internal class ResponseBase
		{
			[JsonProperty("messageType")]
			public int MessageType;

			[JsonProperty("error")] [CanBeNull]
			public Error Error;
		}

		[Serializable]
		internal class Error
		{
			[JsonProperty("code")]
			public int Code;

			[JsonProperty("msg")]
			public string Msg;
		}

		[Serializable]
		public class Comment
		{
			[JsonProperty("type")]
			public int Type;

			[JsonProperty("text")] [CanBeNull]
			public string Text;

			[JsonProperty("mentionUserIds")] [CanBeNull]
			public string[] MentionUserIds;

			[JsonProperty("url")] [CanBeNull]
			public string URL;

			[JsonProperty("fileType")] [CanBeNull]
			public string FileType;

			[JsonProperty("fileUrl")] [CanBeNull]
			public string FileUrl;

			[JsonProperty("emoticon")] [CanBeNull]
			public string Emoticon;

			[JsonProperty("customData")] [CanBeNull]
			public object CustomData;
			public static Comment NewText(string text) => new Comment {Type = GetCommentsTypeCode(eCommentsType.TEXT), Text = text};
			public static Comment NewMentionUserIds(string[] userIds) => new Comment {Type = GetCommentsTypeCode(eCommentsType.MENTION), MentionUserIds = userIds};
			public static Comment NewUrl(string url) => new Comment {Type = GetCommentsTypeCode(eCommentsType.URL), URL = url};
			public static Comment NewFile(string fileUrl, string fileType) => new Comment {Type = GetCommentsTypeCode(eCommentsType.FILE), FileUrl = fileUrl, FileType = fileType};
			public static Comment NewEmoticon(string emoticon) => new Comment {Type = GetCommentsTypeCode(eCommentsType.EMOTICON), Emoticon = emoticon};
			public static Comment NewCustomData(object data) => new Comment {Type = GetCommentsTypeCode(eCommentsType.CUSTOM), CustomData = data};
		}

		[Serializable]
		public class AreaComment
		{
			[JsonProperty("type")]
			public int Type;

			[JsonProperty("text")]
			public string Text;

			[JsonProperty("emoticon")]
			public string Emoticon;

			[JsonProperty("customData")]
			public object CustomData;

			public static AreaComment NewText(string text) => new AreaComment {Type = GetCommentsTypeCode(eCommentsType.TEXT), Text = text};
			public static AreaComment NewEmoticon(string emoticon) => new AreaComment {Type = GetCommentsTypeCode(eCommentsType.EMOTICON), Emoticon = emoticon};
			public static AreaComment NewCustomData(object data) => new AreaComment {Type = GetCommentsTypeCode(eCommentsType.CUSTOM), CustomData = data};
		}

		[Serializable]
		public class PrivateComment
		{
			[JsonProperty("type")]
			public int Type;

			[JsonProperty("text")]
			public string Text;

			[JsonProperty("emoticon")]
			public string Emoticon;

			[JsonProperty("customData")]
			public object CustomData;

			public static PrivateComment NewText(string text) => new PrivateComment {Type = GetCommentsTypeCode(eCommentsType.TEXT), Text = text};
			public static PrivateComment NewEmoticon(string emoticon) => new PrivateComment {Type = GetCommentsTypeCode(eCommentsType.EMOTICON), Emoticon = emoticon};
			public static PrivateComment NewCustomData(object data) => new PrivateComment {Type = GetCommentsTypeCode(eCommentsType.CUSTOM), CustomData = data};
		}

		[Serializable]
		public class ForwardMessage
		{
			[JsonProperty("messageId")]
			public long MessageId;

			[JsonProperty("sendUserId")]
			public string SendUserId;

			[JsonProperty("comments")]
			public Comment[] Comments;
		}
#endregion // Common
		// TODO : 그룹서버를 통한 메세지 동기화 처리
	}
}
