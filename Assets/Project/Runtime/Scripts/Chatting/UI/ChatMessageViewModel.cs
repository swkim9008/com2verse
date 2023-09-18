/*===============================================================
* Product:		Com2Verse
* File Name:	ChatMessageViewModel.cs
* Developer:	haminjeong
* Date:			2022-07-13 15:13
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Runtime.CompilerServices;
using Com2Verse.Chat;
using Com2Verse.Data;
using Com2Verse.Network;
using Com2Verse.Option;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[ViewModelGroup("Chat")]
	public sealed class ChatMessageViewModel : ViewModelBase
	{
		private const string To   = "TO.";
		private const string From = "FROM.";

		private readonly float _normalWidth     = 350f;
		private readonly float _connectingWidth = 320f;

		private float _messageWidth = 350f;

		private ChatCoreBase.MessageInfo _messageInfo;

		private eChatColorType _messageColorType;

		private bool IsMine => MessageColorType == eChatColorType.CHAT_MY_SELF;

		public bool IsExpired { get; set; }

		[UsedImplicitly]
		public ChatCoreBase.MessageInfo MessageInfo
		{
			get => _messageInfo;
			set
			{
				SetProperty(ref _messageInfo, value);
				RefreshMessage();
			}
		}

		public eChatColorType MessageColorType
		{
			get => _messageColorType;
			set => SetProperty(ref _messageColorType, value);
		}

		public ChatCoreBase.eMessageType MessageType => _messageInfo.Type;

		public bool IsSystemMessage => MessageType == ChatCoreBase.eMessageType.SYSTEM;

		public bool IsWhisperMessage => MessageType == ChatCoreBase.eMessageType.WHISPER;

		public string Message
		{
			get
			{
				if (IsExpired)
				{
					var disableChatTypeString = ChatColor.ApplyDisableColor(_messageInfo.Message ?? string.Empty);
					return disableChatTypeString;
				}

				var chatTypeString = ChatColor.ApplyMessageColor(MessageColorType, _messageInfo.Message ?? string.Empty);
				return chatTypeString;
			}
		}

		public string WhisperDirection => CheckDisabledLocalString(IsMine ? To : From);

		public string ReceivedTime => CheckDisabledLocalString(_messageInfo.TimeString ?? string.Empty);

		public string Sender
		{
			get
			{
				var senderName = _messageInfo.SenderName ?? string.Empty;

				var currentServiceType = (eServiceID)(User.InstanceOrNull?.CurrentServiceType ?? (long)eServiceID.WORLD);
				var isMice             = currentServiceType == eServiceID.MICE;
				if (_messageInfo.IsMobile && isMice)
					senderName = $"{senderName} (M)";

				if (IsExpired)
				{
					var disableChatTypeString = ChatColor.ApplyDisableColor(senderName);
					return disableChatTypeString;
				}

				var chatTypeString = ChatColor.ApplyMessageColor(MessageColorType, senderName, true);
				return chatTypeString;
			}
		}

		public string ChatTypeString
		{
			get
			{
				var localizationString = MessageType switch
				{
					ChatCoreBase.eMessageType.AREA    => Localization.Instance.GetString(TextKey.AreaTitle),
					ChatCoreBase.eMessageType.NEARBY  => Localization.Instance.GetString(TextKey.NearbyTitle),
					ChatCoreBase.eMessageType.WHISPER => Localization.Instance.GetString(TextKey.WhisperTitle),
					ChatCoreBase.eMessageType.SYSTEM  => Localization.Instance.GetString(TextKey.SystemTitle),
					_                                 => string.Empty,
				};

				if (IsExpired)
				{
					var disableChatTypeString = ChatColor.ApplyDisableColor(localizationString);
					return disableChatTypeString;
				}

				var chatTypeString = ChatColor.ApplyChatTitleColor(MessageType, localizationString);
				return chatTypeString;
			}
		}

		public bool IsOnTimeStamp => OptionController.InstanceOrNull?.GetOption<ChatOption>()?.IsOnTimeStamp ?? false;

		public float FontSize => OptionController.InstanceOrNull?.GetOption<ChatOption>()?.FontSize ?? 0;

		public float MessageWidth
		{
			get => _messageWidth;
			set => SetProperty(ref _messageWidth, value);
		}

		private string CheckDisabledLocalString(string value)
		{
			if (!IsExpired)
				return value ?? string.Empty;

			return ChatColor.ApplyDisableColor(value ?? string.Empty);
		}

		public void RefreshMessage()
		{
			InvokePropertyValueChanged(nameof(MessageType), MessageType);

			InvokePropertyValueChanged(nameof(IsSystemMessage),  IsSystemMessage);
			InvokePropertyValueChanged(nameof(IsWhisperMessage), IsWhisperMessage);

			InvokePropertyValueChanged(nameof(Message),          Message);
			InvokePropertyValueChanged(nameof(WhisperDirection), WhisperDirection);
			InvokePropertyValueChanged(nameof(ReceivedTime),     ReceivedTime);
			InvokePropertyValueChanged(nameof(Sender),           Sender);

			InvokePropertyValueChanged(nameof(ChatTypeString), ChatTypeString);
		}

		public void RefreshSettingProperties()
		{
			InvokePropertyValueChanged(nameof(IsOnTimeStamp), IsOnTimeStamp);
			InvokePropertyValueChanged(nameof(FontSize),      FontSize);
			InvokePropertyValueChanged(nameof(MessageWidth),  MessageWidth);
		}

		public void Initialize()
		{
			MessageWidth     = CurrentScene.SpaceCode == eSpaceCode.MEETING ? _connectingWidth : _normalWidth;
			MessageColorType = eChatColorType.CHAT_DEFAULT;
		}

		public override void OnLanguageChanged()
		{
			RefreshMessage();
		}
	}
}
