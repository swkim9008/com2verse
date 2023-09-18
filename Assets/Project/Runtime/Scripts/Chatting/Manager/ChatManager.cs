/*===============================================================
* Product:		Com2Verse
* File Name:	ChatManager.cs
* Developer:	haminjeong
* Date:			2022-07-13 11:15
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.BannedWords;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.Office;
using Com2Verse.Option;
using Com2Verse.UI;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Localization = Com2Verse.UI.Localization;

namespace Com2Verse.Chat
{
	public struct TextKey
	{
		public static readonly string SystemTitle              = "UI_Chat_System_Title";
		public static readonly string AreaTitle                = "UI_Chat_Area_Title";
		public static readonly string NearbyTitle              = "UI_Chat_Nearby_Title";
		public static readonly string WhisperTitle             = "UI_Chat_Whisper_Title";
		public static readonly string AllTab                   = "UI_Chat_Tab_All";
		public static readonly string AreaTab                  = "UI_Chat_Tab_Area";
		public static readonly string NearbyTab                = "UI_Chat_Tab_NearBy";
		public static readonly string WhisperTab               = "UI_Chat_Tab_Whisper";
		public static readonly string StartVoiceTalk           = "UI_TC_VoiceTalk_Popup_StartVoiceTalk";
		public static readonly string SpeechBalloonOpen        = "SpeechBalloon_Open";
		public static readonly string SpeechBalloonClose       = "SpeechBalloon_Close";
		public static readonly string MeetingRoom              = "UI_MeetingRoomSpecChange_MeetingRoom";
		public static readonly string AreaTarget               = "UI_Chat_Area_BT_Target";
		public static readonly string DefaultPlaceHolder       = "UI_Chat_Input_DefaultMsg";
		public static readonly string WhisperTargetPlaceHolder = "UI_Chat_Input_DefaultName";
	}

	public sealed partial class ChatManager : Singleton<ChatManager>, IDisposable
	{
		private ChatCoreCom2Verse? _chat;

		private ChatViewer?        _chatViewer;
		public  ChatSetting?       TableSetting => _chatViewer?.TableSettings;
		public  TableChatColorSet? TableColor   => _chatViewer?.TableChatColors;

		private ChatRootViewModel? _chatRootViewModel;

		private bool  _isFocusChatting;
		private float _focusOutChattingTime;

		private bool _isStoredSystemMessage = true;
		private bool _isAutoSimplifyUi      = true;

		// TODO : 테이블 데이터 가져와서 Tab / Chat 입력 Type Setting
		public static bool AllTabActive     = true;
		public static bool AreaTabActive    = false;
		public static bool NearbyTabActive  = false;
		public static bool WhisperTabActive = true;

		public static bool AreaMessageActive    = true;
		public static bool NearbyMessageActive  = false;
		public static bool WhisperMessageActive = true;

		public string CurrentArea => _chat?.CurrentArea ?? string.Empty;
		public string DeviceId
		{
			get => _chat == null ? "0" : Convert.ToString(_chat?.DeviceId);
			set
			{
				if (_chat == null) return;

				if (long.TryParse(value, out var did))
					_chat.DeviceId = did;
			}
		}

		private ChatManager() { }

		public void Initialize()
		{
			_chat = new ChatCoreCom2Verse();
			_chatViewer = new ChatViewer();
			_chat.Initialize();
			_chatViewer.Initialize();
			_chatViewer.LoadTable(LoadTableComplete);
			_chat.OnReceivedAreaMessage    += OnReceivedAreaMessage;
			_chat.OnReceivedWhisperMessage += OnReceivedWhisperMessage;
			_chat.OnReceivedCustomData     += OnReceivedCustomData;
			_chat.SetSendRelayMessageFunc(IsSendRelayMessage);

			NetworkManager.Instance.OnDisconnected += OnDisconnected;
			AddOptionChangeEvents();
		}

		public void SetChatUrl(string chatUrl) => _chat?.SetChatUrl(chatUrl);

		private async UniTask SetBannedWord()
		{
			if (BannedWords.BannedWords.IsReady)
				return;

			var available = await BannedWords.BannedWords.CheckAndUpdateAsync(AppDefine.Default);
			if (available)
			{
				await BannedWords.BannedWords.LoadAsync(AppDefine.Default);
				BannedWords.BannedWords.SetLanguageCode("All");
				BannedWords.BannedWords.SetCountryCode("All");
				BannedWords.BannedWords.SetUsageSentence();
			}
		}

		public void SetAreaMove(string groupId)
		{
			_chat?.AreaMove(groupId);
		}

		private void LoadTableComplete(ChatSetting tableSetting)
		{
			_chat?.SetChatTableData(tableSetting.ChatMaxCount);
		}

		public void OnUpdate()
		{
			_chat?.UpdateQueue();
			_chatViewer?.OnUpdate();

			CheckAutoInactiveUi();
		}

		private void CheckAutoInactiveUi()
		{
			if (!_isAutoSimplifyUi) return;

			if (!_isFocusChatting && _chatRootViewModel?.WindowState != (int)ChatRootViewModel.eWindowState.INACTIVE)
			{
				_focusOutChattingTime += Time.deltaTime;

				if (_focusOutChattingTime > TableSetting?.StateChangeTime)
				{
					_chatViewer?.FocusOutChatting();
					_focusOutChattingTime = 0f;
				}
			}
		}

		public void ResetProperties()
		{
			_chat?.ResetProperties();
			_chatViewer?.ResetProperties();
		}

		private void ReleaseObjects()
		{
			_chat?.ReleaseObjects();
			_chatViewer?.ReleaseObjects();
		}

		private void OnDisconnected()
		{
			ReleaseObjects();
		}

		private void RemoveNetworkEvents()
		{
			var networkManager = NetworkManager.InstanceOrNull;
			if (networkManager != null)
				networkManager.OnDisconnected -= OnDisconnected;

			if (_chat == null) return;
			_chat.OnReceivedAreaMessage    -= OnReceivedAreaMessage;
			_chat.OnReceivedWhisperMessage -= OnReceivedWhisperMessage;
			_chat.OnReceivedCustomData     -= OnReceivedCustomData;
		}

		public void Dispose()
		{
			RemoveNetworkEvents();
			RemoveOptionChangeEvents();
			_chat?.ReleaseObjects();
			_chatViewer?.ReleaseObjects();
			_chat?.DisconnectChatServer();
		}

		public void ConnectChatUI()
		{
			_chatRootViewModel = ViewModelManager.Instance.GetOrAdd<ChatRootViewModel>();
			_chatViewer?.ConnectChatUI(_chatRootViewModel);
			SetBannedWord().Forget();
			RefreshOption();
		}

		public void RefreshViewMessageCollection()
		{
			_chatViewer?.ClearSentPlaceOfAreaMessage();
			_chatViewer?.RefreshViewMessageCollection();
		}

		public void SetAreaChatInitialize(bool isSimpleMode)
		{
			//_chatViewer.SetAreaChatInitialize(isSimpleMode);
		}

		public void CreateSpeechBubble(MapObject mapObject, string message, ChatCoreBase.eMessageType messageType, int emotion = -1)
		{
			if (mapObject.IsUnityNull()) return;

			var chatOption = OptionController.Instance.GetOption<ChatOption>();

			var isOptionMatch = false;
			if (chatOption != null)
			{
				isOptionMatch |= messageType == ChatCoreBase.eMessageType.AREA   && (chatOption.ShowBubbleOption & eChattingBubbleFilter.AREA)   != 0;
				isOptionMatch |= messageType == ChatCoreBase.eMessageType.NEARBY && (chatOption.ShowBubbleOption & eChattingBubbleFilter.NEARBY) != 0;
			}

			if (emotion == -1 && !isOptionMatch)
				return;

			_chatViewer?.CreateSpeechBubble(mapObject, message, messageType, emotion);
		}

		public void SendSystemMessage(string message)
		{
			if (!_isStoredSystemMessage)
				return;
			if (!IsConnected)
				return;

			var time          = MetaverseWatch.NowDateTime.ToString("HH:mm");
			var systemMessage = ZString.Format("{0} ({1})", message, time);
			if (_chat == null || _chatViewer == null)
				return;

			_chat.SendSystemMessage(systemMessage, time, Localization.Instance.GetString(TextKey.SystemTitle));
			_chatViewer.SyncSystemMessages(_chat.GetAllMessages()!);
		}

		public void OpenChatUI()
		{
			_chatViewer?.OpenChatUI();
		}

		public void SetHandsUpEmoticon(long userId, bool handsUp)
		{
			var mapController = MapController.InstanceOrNull;
			if (!mapController.IsReferenceNull())
			{
				var mapObject = mapController!.GetObjectByUserID(userId);
				if (mapObject.IsUnityNull())
				{
					C2VDebug.LogErrorCategory(GetType().Name, "Can't find mapObject");
					return;
				}

				var activeObjectManagerViewModel = ViewModelManager.Instance.GetOrAdd<ActiveObjectManagerViewModel>();
				if (!activeObjectManagerViewModel.TryGet(mapObject!.ObjectID, out var avatarViewModel))
				{
					C2VDebug.LogErrorCategory(GetType().Name, "Can't find activeObject ViewModel");
					return;
				}

				var chatViewModel = avatarViewModel.ChatViewModel;
				chatViewModel.IsOnHandsUp = handsUp;
			}
		}

#region Option
		private void RefreshOption()
		{
			var chatOption = OptionController.Instance.GetOption<ChatOption>();
			if (chatOption == null)
			{
				C2VDebug.LogWarningCategory(GetType().Name, "ChatOption is null");
				return;
			}

			OnChattingFilterChanged(chatOption.ChattingFilter);
			OnChattingBubbleFilterChanged(chatOption.ShowBubbleOption);
			OnLinkWarningChanged(chatOption.IsOnLinkWarning);
			OnFontSizeChanged(chatOption.FontSize);
			OnTimeStampChanged(chatOption.IsOnTimeStamp);
			OnChatTransparentChanged(chatOption.ChatTransparent);
		}

		private void AddOptionChangeEvents()
		{
			var chatOption = OptionController.Instance.GetOption<ChatOption>();
			if (chatOption == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "ChatOption is null");
				return;
			}

			chatOption.OnChattingFilterChanged       += OnChattingFilterChanged;
			chatOption.OnChattingBubbleFilterChanged += OnChattingBubbleFilterChanged;
			chatOption.OnAutoSimplifyChanged         += OnAutoSimplifyChanged;
			chatOption.OnLinkWarningChanged          += OnLinkWarningChanged;
			chatOption.OnFontSizeChanged             += OnFontSizeChanged;
			chatOption.OnTimeStampChanged            += OnTimeStampChanged;
			chatOption.OnChatTransparentChanged      += OnChatTransparentChanged;
		}

		private void RemoveOptionChangeEvents()
		{
			if (!OptionController.InstanceExists) return;
			var chatOption = OptionController.Instance.GetOption<ChatOption>();
			if (chatOption == null)
				return;

			chatOption.OnChattingFilterChanged       -= OnChattingFilterChanged;
			chatOption.OnChattingBubbleFilterChanged -= OnChattingBubbleFilterChanged;
			chatOption.OnAutoSimplifyChanged         -= OnAutoSimplifyChanged;
			chatOption.OnLinkWarningChanged          -= OnLinkWarningChanged;
			chatOption.OnFontSizeChanged             -= OnFontSizeChanged;
			chatOption.OnTimeStampChanged            -= OnTimeStampChanged;
			chatOption.OnChatTransparentChanged      -= OnChatTransparentChanged;
		}

		private void OnChattingFilterChanged(eChattingFilter filter)
		{
			_chatViewer?.OnChattingFilterChanged(filter);
			_isStoredSystemMessage = filter.HasFlag(eChattingFilter.SYSTEM);
		}

		private void OnChattingBubbleFilterChanged(eChattingBubbleFilter filter)
		{
			_chatViewer?.OnChattingBubbleFilterChanged(filter);
		}

		private void OnAutoSimplifyChanged(bool isOn)
		{
			_isAutoSimplifyUi = isOn;
			_focusOutChattingTime = 0f;
		}

		private void OnLinkWarningChanged(bool isOn)
		{
		}

		private void OnFontSizeChanged(float fontSize)
		{
			_chatViewer?.OnFontSizeChanged(fontSize);
		}

		private void OnTimeStampChanged(bool isOn)
		{
			_chatViewer?.OnTimeStampChanged(isOn);
		}

		private void OnChatTransparentChanged(float value)
		{
			_chatViewer?.OnChatTransparentChanged(value);
		}
#endregion Option

		public void OnFocusChatting(bool value)
		{
			_isFocusChatting      = value;
			_focusOutChattingTime = 0f;
		}

		public void SetUserStandby(bool value)
		{
			if (_chat != null)
				_chat.UserStandBy = value;
		}

#region Check Relay Message
		private bool IsSendRelayMessage()
		{
			var inTeamRoom = OfficeService.Instance.InOffice && OfficeService.Instance.IsTeamRoom;
			/* 채팅 옵션으로 Relay를 활성화 해야하는 경우 여기에 코드 추가 */
			return inTeamRoom;
		}
#endregion // Check Relay Message
	}
}
