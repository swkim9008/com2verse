/*===============================================================
* Product:		Com2Verse
* File Name:	ChatRootViewModel.cs
* Developer:	haminjeong
* Date:			2022-07-13 15:08
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Com2Verse.Chat;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Option;
using Com2Verse.PlayerControl;
using Com2Verse.Project.RecyclableScroll;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse.UI
{
	[ViewModelGroup("Chat")]
	public sealed class ChatRootViewModel : ViewModelBase
	{
		private const float ChatViewWidth = 380f;
		private const float InactiveSize  = 200f;
		private const float MinimizeSize  = 332f;
		private const float MaximizeSize  = 1040f;

#region Variables
		public enum eChatType
		{
			ALL = 0,
			AREA,
			NEARBY,
			WHISPER,
		}

		public enum eWindowState
		{
			INACTIVE,
			MINIMIZE,
			MAXIMIZE,
		}
		private static readonly string GesturePattern = @"^\/\S+(\s\@[^\s\@\/]+)?$";

		private CancellationTokenSource _cts;
		private bool _isReleased = false;

		private Collection<ChatMessageViewModel> _viewMessageCollection       = new(); // 실제 화면에 보이는 Collection
		private Collection<ChatMessageViewModel> _allMessageCollection        = new();
		private Collection<ChatMessageViewModel> _areaChatMessageCollection   = new();
		private Collection<ChatMessageViewModel> _nearbyChatMessageCollection = new();
		private Collection<ChatMessageViewModel> _whisperMessageCollection    = new();

		private bool _windowRaycastEnable   = true;
		private bool _backgroundActive;
		private int  _windowState           = (int)eWindowState.INACTIVE;
		private bool _isWindowStateFullMode;

		private int    _chatMessageLimit = 50;
		private string _chatMessage      = new(string.Empty);
		private string _whisperTarget;

		private bool   _isWhisperInputFieldFocus = false;
		private bool   _isMessageInputFieldFocus = false;
		private string _inputFieldPlaceHolder    = Localization.Instance.GetString(TextKey.DefaultPlaceHolder);
		private string _whisperTargetPlaceHolder = Localization.Instance.GetString(TextKey.WhisperTargetPlaceHolder);
		private bool   _topTabWhisperRedDot      = false;

		// Tab 관리
		private int  _currentTabType   = (int)eChatType.ALL;
		private bool _allTabEnable     = true;
		private bool _areaTabEnable    = false;
		private bool _nearbyTabEnable  = false;
		private bool _whisperTabEnable = false;

		// MessageType 관리
		private int  _currentInputChatType   = (int)eChatType.AREA;
		private bool _areaChatInputActive    = true;
		private bool _nearbyChatInputActive  = false;
		private bool _whisperChatInputActive = false;

		private bool   _openChangeInputType;
		private string _selectedInputType;

		private Transform     _whisperInputFieldTransform;
		private Transform     _messageInputFieldTransform;
		private RectTransform _whisperCaret;
		private RectTransform _messageCaret;
		private ScrollRect    _chatScrollRect;
		private RecyclableScrollRect    _chatRecyclableScrollRect;

		private bool   _canSendMessage = true;
		private string _myName         = string.Empty;

		private Queue<float> _messageInputTime = new();

		private bool _isVisibleTime = true;

		public CommandHandler      WindowStateChangeButtonClick { get; }
		public CommandHandler      InputWhisperName             { get; }
		public CommandHandler      InputChatMessage             { get; }
		public CommandHandler      ChatUIClicked                { get; }
		public CommandHandler      OpenChangeInput              { get; }
		public CommandHandler<int> ChangeInputType              { get; }
		public CommandHandler<int> ChangeChatTab                { get; }
		public CommandHandler      OpenOptionPopup              { get; }

		private event Action OnChatUIInput;
#endregion

#region Initailize
		public ChatRootViewModel()
		{
			DontDestroyOnLoad            = true;
			//ChatMessageLimit             = ChatManager.Instance.TableSetting.ChatInputTextLimit;
			ChatMessageLimit             = 50;
			WindowStateChangeButtonClick = new CommandHandler(OnWindowStateChangeButton);
			InputWhisperName             = new CommandHandler(OnInputWhisperName);
			InputChatMessage             = new CommandHandler(OnInputChatMessage);
			OpenChangeInput              = new CommandHandler(OnOpenChangeInput);
			ChangeInputType              = new CommandHandler<int>(OnChatInputType);
			ChangeChatTab                = new CommandHandler<int>(OnChatTabChange);
			OpenOptionPopup              = new CommandHandler(OnOpenOptionPopup);
			ChatUIClicked                = new CommandHandler(OnChatUIActive);
			OpenChangeInputType          = false;
			CurrentTabType               = (int)eChatType.ALL;
			CurrentInputChatType         = (int)eChatType.AREA;
			OnChatUIInput                = () => ChatManager.InstanceOrNull?.OnFocusChatting(_isMessageInputFieldFocus || _isWhisperInputFieldFocus);
		}
#endregion

#region Properties
		public bool WindowRaycastEnable
		{
			get => _windowRaycastEnable;
			set => SetProperty(ref _windowRaycastEnable, value);
		}

		public bool BackgroundActive
		{
			get => _backgroundActive;
			set => SetProperty(ref _backgroundActive, value);
		}

		public int WindowState
		{
			get => _windowState;
			set
			{
				SetProperty(ref _windowState, value);
				InvokePropertyValueChanged(nameof(WorldChatWindowSize), WorldChatWindowSize);
				OnSetWindowState((eWindowState)value);
			}
		}

		[UsedImplicitly]
		public Vector2 WorldChatWindowSize
		{
			get
			{
				var height         = InactiveSize;
				if (WindowState == (int)eWindowState.MAXIMIZE)
					height = MaximizeSize;
				else if (WindowState == (int)eWindowState.MINIMIZE)
					height = MinimizeSize;

				ScrollDownCheckOnNextFrame(false).Forget();

				return new Vector2(ChatViewWidth, height);
			}
		}

		public bool IsVisibleTime
		{
			get => _isVisibleTime;
			set => SetProperty(ref _isVisibleTime, value);
		}

		/// <summary>
		/// 채팅 UI 상단에서 '전체 탭'토글을 활성화 할것인지 여부
		/// </summary>
		[UsedImplicitly]
		public bool AllTabActive => ChatManager.AllTabActive;

		/// <summary>
		/// 채팅 UI 상단에서 '지역 탭'토글을 활성화 할것인지 여부
		/// </summary>
		[UsedImplicitly]
		public bool AreaTabActive => ChatManager.AreaTabActive;

		/// <summary>
		/// 채팅 UI 상단에서 '일반 탭'토글을 활성화 할것인지 여부
		/// </summary>
		[UsedImplicitly]
		public bool NearbyTabActive => ChatManager.NearbyTabActive;

		/// <summary>
		/// 채팅 UI 상단에서 '귓속말 탭'토글을 활성화 할것인지 여부
		/// </summary>
		[UsedImplicitly]
		public bool WhisperTabActive => ChatManager.WhisperTabActive;

		/// <summary>
		/// 전체 탭의 메시지 선택 버튼에서, '지역' 메시지를 활성화 할 것인지 여부
		/// </summary>
		[UsedImplicitly]
		public bool AreaMessageActive => ChatManager.AreaMessageActive;

		/// <summary>
		/// 전체 탭의 메시지 선택 버튼에서, '일반' 메시지를 활성화 할 것인지 여부
		/// </summary>
		[UsedImplicitly]
		public bool NearbyMessageActive => ChatManager.NearbyMessageActive;

		/// <summary>
		/// 전체 탭의 메시지 선택 버튼에서, '귓속말' 메시지를 활성화 할 것인지 여부
		/// </summary>
		[UsedImplicitly]
		public bool WhisperMessageActive => ChatManager.WhisperMessageActive;

		public int CurrentTabType
		{
			get => _currentTabType;
			set => SetProperty(ref _currentTabType, value);
		}

		public bool AllTabEnable
		{
			get => _allTabEnable;
			set
			{
				if (value)
				{
					AreaTabEnable    = false;
					NearbyTabEnable  = false;
					WhisperTabEnable = false;
					CurrentTabType   = (int)eChatType.ALL;
					RefreshAllMessages();
					ScrollDownCheck();
				}

				_allTabEnable = value;
				SetProperty(ref _allTabEnable, value);
			}
		}

		[UsedImplicitly]
		public bool RestoreOriginalTextOnEscape => false;

		public void RefreshAllMessages()
		{
			ViewMessageCollection.Reset();
			var messages       = AllMessageCollection.Clone();
			var chatOption     = OptionController.Instance.GetOption<ChatOption>();
			var chattingFilter = chatOption?.ChattingFilter ?? eChattingFilter.NONE;
			var currentTapType = (eChatType)CurrentTabType;
			foreach (var message in messages)
			{
				if (message.MessageType == ChatCoreBase.eMessageType.AREA)
					if (currentTapType != eChatType.AREA && !chattingFilter.HasFlag(eChattingFilter.AREA))
						continue;

				if (message.MessageType == ChatCoreBase.eMessageType.NEARBY)
					if (currentTapType != eChatType.NEARBY && !chattingFilter.HasFlag(eChattingFilter.NEARBY))
						continue;

				if (message.MessageType == ChatCoreBase.eMessageType.WHISPER)
					if (currentTapType != eChatType.WHISPER && !chattingFilter.HasFlag(eChattingFilter.WHISPER))
						continue;

				if (message.MessageType == ChatCoreBase.eMessageType.SYSTEM)
					if (!chattingFilter.HasFlag(eChattingFilter.SYSTEM))
						continue;

				message.RefreshMessage();
				ViewMessageCollection.AddItem(message);
			}
			SetForceLayoutRebuildAsync().Forget();
		}

		// 특정 뷰모델에 대한 컬러 지정

		// 뷰메시지 콜랙션에 대한 컬러 지정

		private void ChangeViewMessageCollection(Collection<ChatMessageViewModel> collection)
		{
			ViewMessageCollection.Reset();
			ViewMessageCollection.AddRange(collection.Clone());
		}

		public void ClearPrevAreaInfo()
		{
			foreach (var viewMessage in AreaChatMessageCollection.Value)
				if (viewMessage?.MessageType == ChatCoreBase.eMessageType.AREA)
					viewMessage.IsExpired = true;

			foreach (var viewMessage in ViewMessageCollection.Value)
				if (viewMessage?.MessageType == ChatCoreBase.eMessageType.AREA)
					viewMessage.IsExpired = true;
		}

		public void RefreshViewMessageCollection()
		{
			foreach (var viewMessage in ViewMessageCollection.Value)
				viewMessage?.RefreshMessage();
		}

		public bool AreaTabEnable
		{
			get => _areaTabEnable;
			set
			{
				if (value)
				{
					AllTabEnable          = false;
					NearbyTabEnable       = false;
					WhisperTabEnable      = false;
					CurrentTabType        = (int)eChatType.AREA;
					ChangeViewMessageCollection(AreaChatMessageCollection);
					RefreshViewMessageCollection();
					ScrollDownCheck();
					SetForceLayoutRebuildAsync().Forget();
				}

				_areaTabEnable = value;
				SetProperty(ref _areaTabEnable, value);
			}
		}

		public bool NearbyTabEnable
		{
			get => _nearbyTabEnable;
			set
			{
				if (value)
				{
					AllTabEnable          = false;
					AreaTabEnable         = false;
					WhisperTabEnable      = false;
					CurrentTabType        = (int)eChatType.NEARBY;
					ChangeViewMessageCollection(NearbyChatMessageCollection);
					ScrollDownCheck();
					SetForceLayoutRebuildAsync().Forget();
				}

				_nearbyTabEnable = value;
				SetProperty(ref _nearbyTabEnable, value);
			}
		}

		public bool WhisperTabEnable
		{
			get => _whisperTabEnable;
			set
			{
				if (value)
				{
					AllTabEnable          = false;
					AreaTabEnable         = false;
					NearbyTabEnable       = false;
					CurrentTabType        = (int)eChatType.WHISPER;
					ChangeViewMessageCollection(WhisperMessageCollection);
					ScrollDownCheck();
					SetForceLayoutRebuildAsync().Forget();
				}
				_whisperTabEnable = value;
				SetProperty(ref _whisperTabEnable, value);
			}
		}

		public int CurrentInputChatType
		{
			get => _currentInputChatType;
			set
			{
				_currentInputChatType = value;
				SetSelectChatType();
				SetProperty(ref _currentInputChatType, value);
			}
		}

		public bool AreaChatInputActive
		{
			get => _areaChatInputActive;
			set
			{
				if (value)
				{
					CurrentInputChatType   = (int)eChatType.AREA;
				}

				_areaChatInputActive = value;
				SetProperty(ref _areaChatInputActive, value);
			}
		}

		public bool NearbyChatInputActive
		{
			get => _nearbyChatInputActive;
			set
			{
				if (value)
				{
					CurrentInputChatType   = (int)eChatType.NEARBY;
				}

				_nearbyChatInputActive = value;
				SetProperty(ref _nearbyChatInputActive, value);
			}
		}

		public bool WhisperChatInputActive
		{
			get => _whisperChatInputActive;
			set
			{
				if (value)
				{
					CurrentInputChatType  = (int)eChatType.WHISPER;
				}

				_whisperChatInputActive = value;
				SetProperty(ref _whisperChatInputActive, value);
			}
		}

		public bool OpenChangeInputType
		{
			get => _openChangeInputType;
			set => SetProperty(ref _openChangeInputType, value);
		}

		public string SelectedInputType
		{
			get => _selectedInputType;
			set => SetProperty(ref _selectedInputType, value);
		}

		public bool IsWindowStateFullMode
		{
			get => _isWindowStateFullMode;
			set => SetProperty(ref _isWindowStateFullMode, value);
		}

		public string InputFieldPlaceHolder
		{
			get => _inputFieldPlaceHolder;
			set => SetProperty(ref _inputFieldPlaceHolder, value);
		}

		public string WhisperTargetInputFieldPlaceHolder
		{
			get => _whisperTargetPlaceHolder;
			set => SetProperty(ref _whisperTargetPlaceHolder, value);
		}

		public int ChatMessageLimit
		{
			get => _chatMessageLimit;
			set => SetProperty(ref _chatMessageLimit, value);
		}

		public string ChatMessage
		{
			get
			{
				var value = _chatMessage;
				if (value.EndsWith('\n') || value.EndsWith('\v')) // 수직탭(shift + enter)
					ChatMessage = value.Substring(0, value.Length - 1);
				return _chatMessage;
			}
			set => SetProperty(ref _chatMessage, value);
		}

		public string WhisperTarget
		{
			get
			{
				var value = _whisperTarget;
				if (value.EndsWith('\n') || value.EndsWith('\v')) // 수직탭(shift + enter)
					WhisperTarget = value.Substring(0, value.Length - 1);
				return _whisperTarget;
			}
			set => SetProperty(ref _whisperTarget, value);
		}

		public bool IsWhisperInputFieldFocus
		{
			get => _isWhisperInputFieldFocus;
			set
			{
				_isWhisperInputFieldFocus = value;
				if (value)
				{
					if (_whisperCaret.IsUnityNull())
						if (!WhisperInputFieldTransform.IsUnityNull())
							_whisperCaret = WhisperInputFieldTransform.Find("Text Area/Caret").GetComponent<RectTransform>();

					if (!_whisperCaret.IsUnityNull())
					{
						_whisperCaret.anchorMin = Vector2.zero;
						_whisperCaret.anchorMax = Vector2.one;
						_whisperCaret.offsetMin = _whisperCaret.offsetMax = Vector2.zero;
					}
					IsMessageInputFieldFocus = false;
				}

				OnChatUIInput?.Invoke();
				SetProperty(ref _isWhisperInputFieldFocus, value);
			}
		}

		public bool IsMessageInputFieldFocus
		{
			get => _isMessageInputFieldFocus;
			set
			{
				_isMessageInputFieldFocus = value;
				if (value)
				{
					if (_messageCaret.IsUnityNull())
						if (!MessageInputFieldTransform.IsUnityNull())
							_messageCaret = MessageInputFieldTransform.Find("Text Area/Caret").GetComponent<RectTransform>();

					if (!_messageCaret.IsUnityNull())
					{
						_messageCaret.anchorMin = Vector2.zero;
						_messageCaret.anchorMax = Vector2.one;
						_messageCaret.offsetMin = _messageCaret.offsetMax = Vector2.zero;
					}
					IsWhisperInputFieldFocus = false;
				}

				OnChatUIInput?.Invoke();
				SetProperty(ref _isMessageInputFieldFocus, value);
			}
		}

		public Transform WhisperInputFieldTransform
		{
			get => _whisperInputFieldTransform;
			set => SetProperty(ref _whisperInputFieldTransform, value);
		}

		public Transform MessageInputFieldTransform
		{
			get => _messageInputFieldTransform;
			set => SetProperty(ref _messageInputFieldTransform, value);
		}

		public bool TopTabWhisperRedDot
		{
			get => _topTabWhisperRedDot;
			set => SetProperty(ref _topTabWhisperRedDot, value);
		}

		public Collection<ChatMessageViewModel> ViewMessageCollection
		{
			get => _viewMessageCollection;
			set => SetProperty(ref _viewMessageCollection, value);
		}

		public Collection<ChatMessageViewModel> AllMessageCollection
		{
			get => _allMessageCollection;
			set => SetProperty(ref _allMessageCollection, value);
		}

		public Collection<ChatMessageViewModel> AreaChatMessageCollection
		{
			get => _areaChatMessageCollection;
			set => SetProperty(ref _areaChatMessageCollection, value);
		}

		public Collection<ChatMessageViewModel> NearbyChatMessageCollection
		{
			get => _nearbyChatMessageCollection;
			set => SetProperty(ref _nearbyChatMessageCollection, value);
		}

		public Collection<ChatMessageViewModel> WhisperMessageCollection
		{
			get => _whisperMessageCollection;
			set => SetProperty(ref _whisperMessageCollection, value);
		}

		public ScrollRect ChatScrollRect
		{
			get => _chatScrollRect;
			set => SetProperty(ref _chatScrollRect, value);
		}

		public RecyclableScrollRect ChatRecyclableScrollRect
		{
			get => _chatRecyclableScrollRect;
			set => SetProperty(ref _chatRecyclableScrollRect, value);
		}

		public float ChattingBackgroundTransparent
		{
			get
			{
				var chatOption = OptionController.InstanceOrNull?.GetOption<ChatOption>();
				if (chatOption == null)
					return 1f;

				return Mathf.Clamp((chatOption.ChatTransparent) * 0.01f, 0, 1f);
			}
		}

		public bool SetForceLayoutRebuild => true;
#endregion

		public void RefreshOptionValues()
		{
			InvokePropertyValueChanged(nameof(ChattingBackgroundTransparent), ChattingBackgroundTransparent);
		}

		private void OnSetWindowState(eWindowState state)
		{
			if (_isReleased)
			{
				_cts?.Cancel();
				return;
			}
			IsWindowStateFullMode = state == eWindowState.MAXIMIZE;
			BackgroundActive = state != eWindowState.INACTIVE;
		}

		private void OnWindowStateChangeButton()
		{
			WindowState = (eWindowState)WindowState == eWindowState.MAXIMIZE ? (int)eWindowState.MINIMIZE : (int)eWindowState.MAXIMIZE;
			OnChatUIInput?.Invoke();
		}

		public async UniTask ScrollDownCheckOnNextFrame(bool forceScrollDown = true)
		{
			if (ChatScrollRect.IsUnityNull())
				return;

			var wasScrolledDown = ChatScrollRect!.verticalNormalizedPosition <= 0.2f;
			await UniTask.NextFrame();

			if (forceScrollDown)
			{
				ScrollViewForceDown();
				return;
			}

			if (!wasScrolledDown)
				return;

			ScrollDownCheck();
		}

		public void ScrollDownCheck(bool forceScrollDown = true)
		{
			if (forceScrollDown)
			{
				ScrollViewForceDown();
				return;
			}

			if (ChatScrollRect.verticalNormalizedPosition > 0.2f) return;
			ScrollViewForceDown();
		}

		private void ScrollViewForceDown()
		{
			if (ChatScrollRect.IsUnityNull()) return;
			Canvas.ForceUpdateCanvases();
			ChatScrollRect!.verticalNormalizedPosition = 0f;
		}

		private void OnInputWhisperName()
		{
			if (string.IsNullOrWhiteSpace(WhisperTarget))
				return;

			CurrentInputFocus();
			OnChatUIInput?.Invoke();
		}

		private void OnInputChatMessage()
		{
			if (string.IsNullOrWhiteSpace(ChatMessage))
			{
				IsMessageInputFieldFocus = false;
				return;
			}
			
			if (CommandSlashCheck())
			{
				CurrentInputFocus();
				DelayedInputFieldClear().Forget();
				return;
			}

			var cooltime = PreventionRule();
			if (cooltime != 0)
			{
				NoticeManager.Instance.SendNotice(Localization.Instance.GetString("UI_Chat_System_InputCoolTime_Msg", cooltime), NoticeManager.eNoticeType.CHATTING);
				return;
			}

			GetFilteredString();
			RemoveRichTextTags();

			C2VDebug.Log(ChatMessage);

			SetMessageAfterProcess();

			CurrentInputFocus();
			DelayedInputFieldClear().Forget();

			OnChatUIInput?.Invoke();
		}

		private void SetMessageAfterProcess()
		{
			switch (CurrentInputChatType)
			{
				case (int)eChatType.AREA:
					if (_myName == string.Empty)
					{
						// TODO : 월드닉네임을 가져오도록 수정
						//var myself = await DataManager.Instance.GetMyselfAsync();
						//_myName = myself.EmployeeName;
					}
					ChatManager.Instance.SendAreaMessage(ChatMessage);
					break;
				case (int)eChatType.NEARBY:
					break;
				case (int)eChatType.WHISPER:
					if (!string.IsNullOrEmpty(_whisperTarget))
						ChatManager.Instance.SendPrivateMessage(ChatMessage, _whisperTarget);
					break;
			}
		}

		private async UniTaskVoid DelayedInputFieldClear()
		{
			await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
			ChatMessage = string.Empty;
		}

		private void CurrentInputFocus(bool isInputEvent = false)
		{
			switch ((eChatType)CurrentInputChatType)
			{
				case eChatType.AREA:
					if (!IsMessageInputFieldFocus)
						IsMessageInputFieldFocus = true;
					break;
				case eChatType.WHISPER:
					if (!IsWhisperInputFieldFocus && !IsMessageInputFieldFocus)
					{
						IsWhisperInputFieldFocus = true;
					}
					// Input update 타이밍에 포커스를 업데이트 하는 경우 타이밍에 따라 UI갱신이 바로 되지 않아
					// Unity Update 타이밍에만 포커스를 갱신하도록 제한한다.
					else if (IsWhisperInputFieldFocus && !isInputEvent)
					{
						IsMessageInputFieldFocus = true;
					}
					break;
			}

			OnChatUIActive();
		}

#region String Manipulation
#region RichTextTagPattern
		private static readonly List<string> RichTextTagPatternList = new()
		{
			@"<align=\""[^<>]+\"">", @"<\/align>",
			@"<lowercase>", @"<\/lowercase>",
			@"<uppercase>", @"<\/uppercase>",
			@"<allcaps>", @"<\/allcaps>",
			@"<smallcaps>", @"<\/smallcaps>",
			@"<alpha=#[^<>]{2}>", @"<\/alpha>",
			@"<b>", @"<\/b>",
			@"<i>", @"<\/i>",
			@"<cspace=\""?[^<>]+\""?>", @"<\/cspace>",
			@"<font=\""[^<>]+\"">", @"<\/font>",
			@"<indent=\""?[0-9]+%\""?>", @"<\/indent>",
			@"<line-height=\""?[0-9]+%\""?>", @"<\/line-height>",
			@"<line-indent=\""?[0-9]+%\""?>", @"<\/line-indent>",
			@"<link=\""[^<>]+\"">", @"<\/link>",
			@"<margin=[^<>]+>", @"<\/margin>",
			@"<mark=#[^<>]+>", @"<\/mark>",
			@"<mspace=\""?[^<>]+\""?>", @"<\/mspace>",
			@"<pos=\""?[0-9]+%\""?>", @"<\/pos>",
			@"<rotate=\""?[-0-9]+\""?>", @"<\/rotate>",
			@"<s>", @"<\/s>",
			@"<size=\""?[^<>]+%?\""?>", @"<\/size>",
			@"<space=\""?[^<>]+\""?>", @"<\/space>",
			@"<style=\""[^<>]+\"">", @"<\/style>",
			@"<sub>", @"<\/sub>",
			@"<sup>", @"<\/sup>",
			@"<u>", @"<\/u>",
			@"<voffset=\""?[^<>]+\""?>", @"<\/voffset>",
			@"<width=\""?[0-9]+%\""?>", @"<\/width>",
		};
#endregion RichTextTagPattern

		private void RemoveRichTextTags()
		{
			foreach (var pattern in RichTextTagPatternList)
			{
				MatchCollection matches = Regex.Matches(ChatMessage, pattern);
				for (int i=matches.Count-1; i>=0; --i)
				{
					Match match = matches[i];
					ChatMessage = ChatMessage.Remove(match.Index, match.Value.Length);
				}
			}
		}

		private bool FindGestureString()
		{
			if (!ChatMessage.StartsWith("/")) return false;
			MatchCollection matches = Regex.Matches(ChatMessage, GesturePattern);
			if (matches.Count == 1)
			{
				var totalString = matches[0].Value.Replace("/", "");
				var splits = totalString.Split(" ");
				string gesture = splits[0];
				string targetUserName = string.Empty;
				if (splits.Length == 2)
					if (splits[1][0] == '@')
						targetUserName = splits[1].Replace("@", "");
				// TODO: 이름으로 유저를 검색할지, 다른 방법으로 직접 ID에 접근할지 선택
				if (!string.IsNullOrEmpty(targetUserName))
				{
					C2VDebug.Log("Gesture Target : " + targetUserName);
				}

				if (string.IsNullOrEmpty(gesture)) return false;
				if (PlayerController.Instance.CharacterView.IsUnityNull() ||
				    PlayerController.Instance.CharacterView!.AvatarController.IsUnityNull()) return false;
				var emotionList = PlayerController.Instance.GestureHelper.TableEmotion.Datas.Values.ToList();
				var findResult = emotionList.Find((emotion) =>
				{
					if (emotion!.AvatarType != eAvatarType.NONE && PlayerController.Instance.CharacterView!.AvatarController!.Info?.AvatarType != emotion.AvatarType)
						return false;
					if (string.IsNullOrEmpty(emotion!.ChatCommand)) return false;
					var targetGesture = Localization.Instance.GetString(emotion.ChatCommand);
					return targetGesture.ToLower().Equals(gesture.ToLower());
				});
				if (findResult != null)
					PlayerController.Instance.GestureHelper.EmotionCommand(findResult);
			}
			return true;
		}

		private bool CommandSlashCheck()
		{
			if (!ChatMessage.StartsWith("/")) return false;
			FindGestureString();
			return true;
		}

		private void GetFilteredString()
		{
			BannedWords.BannedWords.SetUsageSentence();
			var message = BannedWords.BannedWords.ApplyFilter(ChatMessage, "*", true);
			ChatMessage = message;
		}

		private int PreventionRule()
		{
			var chatTable = ChatManager.Instance.TableSetting;
			if (chatTable == null || _messageInputTime == null)
				return 10;

			float elapsedTime = _messageInputTime.Count > 0 ? Time.time - _messageInputTime.Peek() : 0;
			if (!_canSendMessage)
			{
				if (elapsedTime > chatTable.InputCoolTime)
				{
					_canSendMessage  = true;
					_messageInputTime.Clear();
					_messageInputTime.Enqueue(Time.time);
				}
				return _canSendMessage ? 0 : Mathf.CeilToInt(chatTable.InputCoolTime - elapsedTime);
			}

			var count = _messageInputTime.Count;
			for (var i = 0; i < count; i++)
			{
				var time = _messageInputTime.Dequeue();
				if (Time.time - time < chatTable.InputCheckTime)
				{
					_messageInputTime.Enqueue(time);
				}
			}

			// 5초동안 입력한 메세지가 3개
			if (_messageInputTime.Count == chatTable.InputCheckCount - 1)
			{
				_canSendMessage = false;
			}
			else
			{
				_messageInputTime.Enqueue(Time.time);
			}
			return _canSendMessage ? 0 : Mathf.CeilToInt(chatTable.InputCoolTime - elapsedTime);
		}
#endregion
		private void OnChatUIActive()
		{
			switch (WindowState)
			{
				case (int)eWindowState.INACTIVE:
					WindowState = (int)eWindowState.MINIMIZE;
					break;
			}
		}

		public void ChatUIOnKeyPressed()
		{
			CurrentInputFocus(true);
		}

		public void ResetProperties()
		{
			ChatMessage          = string.Empty;
			WhisperTarget        = string.Empty;
			OpenChangeInputType  = false;
			CurrentTabType       = (int)eChatType.ALL;
			CurrentInputChatType = (int)eChatType.AREA;
		}

		private void OnOpenChangeInput()
		{
			if (CurrentTabType != (int)eChatType.ALL)
				return;

			OpenChangeInputType = !OpenChangeInputType;
			if (WindowState == (int)eWindowState.INACTIVE)
				ChatUIOnKeyPressed();
			else
				OnChatUIInput?.Invoke();
		}

		private void OnChatInputType(int index)
		{
			CurrentInputChatType = index;
			OpenChangeInputType  = false;
			OnChatUIInput?.Invoke();
		}

		private void OnChatTabChange(int index)
		{
			CurrentTabType       = index;
			OpenChangeInputType  = false;
			switch (index)
			{
				case (int)eChatType.ALL:
					break;
				case (int)eChatType.AREA:
					AreaChatInputActive    = true;
					NearbyChatInputActive  = false;
					WhisperChatInputActive = false;
					CurrentInputChatType   = index;
					break;
				case (int)eChatType.NEARBY:
					NearbyChatInputActive  = true;
					AreaChatInputActive    = false;
					WhisperChatInputActive = false;
					CurrentInputChatType   = index;
					break;
				case (int)eChatType.WHISPER:
					WhisperChatInputActive = true;
					AreaChatInputActive    = false;
					NearbyChatInputActive  = false;
					CurrentInputChatType   = index;
					break;
			}
			OnChatUIInput?.Invoke();

			SetForceLayoutRebuildAsync().Forget();
		}

		private async UniTask SetForceLayoutRebuildAsync()
		{
			InvokePropertyValueChanged(nameof(SetForceLayoutRebuild), SetForceLayoutRebuild);
			await UniTask.Yield(PlayerLoopTiming.LastUpdate);
			InvokePropertyValueChanged(nameof(SetForceLayoutRebuild), SetForceLayoutRebuild);
		}

		private void OnOpenOptionPopup()
		{
			UIManager.Instance.CreatePopup("UI_Popup_Option", (guiView) =>
			{
				guiView.Show();
				var viewModel = guiView.ViewModelContainer.GetViewModel<MetaverseOptionViewModel>();
				guiView.OnOpenedEvent += (guiView) => viewModel.ScrollRectEnable = true;
				guiView.OnClosedEvent += (guiView) => viewModel.ScrollRectEnable = false;
				
				viewModel.IsChatCommunityOn = true;
			}).Forget();
			OnChatUIInput?.Invoke();
		}

		private void SetSelectChatType()
		{
			var chatType = (eChatType)CurrentInputChatType;
			var localizationString = chatType switch
			{
				eChatType.AREA    => Localization.Instance.GetString(TextKey.AreaTab),
				eChatType.NEARBY  => Localization.Instance.GetString(TextKey.NearbyTab),
				eChatType.WHISPER => Localization.Instance.GetString(TextKey.WhisperTab),
				_                 => null,
			};
			if (localizationString == null)
				return;

			SelectedInputType = ChatColor.ApplyChatTitleColor(chatType, localizationString);
		}

		public override void OnLanguageChanged()
		{
			SetSelectChatType();
			SetForceLayoutRebuildAsync().Forget();
		}

		public override void OnRelease()
		{
			base.OnRelease();
			_viewMessageCollection.Reset();
			_allMessageCollection.Reset();
			_areaChatMessageCollection.Reset();
			_nearbyChatMessageCollection.Reset();
			_whisperMessageCollection.Reset();
		}
	}
}
