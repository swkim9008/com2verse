/*===============================================================
* Product:		Com2Verse
* File Name:	ChatViewer.cs
* Developer:	ksw
* Date:			2022-12-27 10:08
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.Option;
using Com2Verse.UI;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Pool;
using Utils;

namespace Com2Verse.Chat
{
	public sealed class ChatViewer : ILocalizationUI
	{
		private const int ChatSortOrderMinValue = 1800;
		private const int ChatSortOrderMaxValue = 1999;

		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly]
		public ChatViewer()
		{
			(this as ILocalizationUI).InitializeLocalization();
		}

		private readonly PriorityQueue<ChatUserViewModel, int> _chatViewModelPriorityQueue = new(Comparer<int>.Create((x, y) => y.CompareTo(x)));

		private ChatSetting       TableSetting   { get; set; }
		private TableChatColorSet TableChatColor { get; set; }

		private ObjectPool<ChatMessageViewModel>    _areaViewModelPool      = new(() => new ChatMessageViewModel());
		private ObjectPool<ChatMessageViewModel>    _nearbyViewModelPool    = new(() => new ChatMessageViewModel());
		private ObjectPool<ChatMessageViewModel>    _whisperViewModelPool   = new(() => new ChatMessageViewModel());
		private ObjectPool<ChatMessageViewModel>    _allViewModelPool       = new(() => new ChatMessageViewModel());
		private Dictionary<long, ChatUserViewModel> _speechBubbleDictionary = new();
		private ChatRootViewModel                   _chatRootViewModel;

		private static readonly string BubbleOpenAnimationName = TextKey.SpeechBalloonOpen;
		private static readonly string BubbleCloseAnimationName = TextKey.SpeechBalloonClose;

		private bool _isSelectChanging = false;

		private CancellationTokenSource _cts;

		private bool _isScrollDown = false;

		private bool _isStoredAreaMessage    = true;
		private bool _isStoredWhisperMessage = true;

		public ChatSetting TableSettings => TableSetting;

		public TableChatColorSet TableChatColors => TableChatColor;

		public void Initialize()
		{
			ReleaseObjects();
			LoadChatColorTable();
		}

		public void LoadTable(Action<ChatSetting> onComplete)
		{
			LoadChatSettingTable(onComplete);
		}

		private void LoadChatSettingTable(Action<ChatSetting> onComplete)
		{
			TableChatSetting tableChatSetting = TableDataManager.Instance.Get<TableChatSetting>();
			if (tableChatSetting == null) return;

			foreach (var data in tableChatSetting.Datas.Values)
			{
				if (data == null) continue;
				TableSetting = data;
			}

			onComplete?.Invoke(TableSetting);
		}

		private void LoadChatColorTable()
		{
			TableChatColor = TableDataManager.Instance.Get<TableChatColorSet>();
		}

		private void UpdateSpeechBubble()
		{
			if (_speechBubbleDictionary is not { Count: > 0 }) return;
			var keys = _speechBubbleDictionary.Keys.ToList();
			keys.ForEach((key) =>
			{
				if (!_speechBubbleDictionary.TryGetValue(key, out var viewModel)) return;

				var mapObject = MapController.Instance.GetObjectByUserID(key) as MapObject;
				if (mapObject.IsReferenceNull())
				{
					_speechBubbleDictionary.Remove(key);
					viewModel?.DisableChat();
					return;
				}

				if (viewModel == null)
				{
					_speechBubbleDictionary.Remove(key);
					return;
				}

				if (TableSetting == null || viewModel.PlayBackTime >= GetSpeechBubbleDisplayTime(TableSetting))
				{
					_speechBubbleDictionary.Remove(key);
					viewModel.PlayCloseAnimation().Forget();
					return;
				}

				viewModel.PlayBackTime += Time.deltaTime;
			});
		}

		// TODO: Npc등 조건 추가
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private float GetSpeechBubbleDisplayTime([NotNull] ChatSetting tableSetting) => SceneManager.InstanceOrNull?.CurrentScene.SpaceCode == eSpaceCode.MICE_CONFERENCE_HALL
			? tableSetting.HallSpeechBubbleDisplayTime
			: tableSetting.SpeechBubbleDisplayTime;

		public void OnUpdate()
		{
			if (_isScrollDown)
				ScrollDown();
			if (_chatRootViewModel?.WindowState == (int)ChatRootViewModel.eWindowState.INACTIVE &&
			    (_chatRootViewModel.IsMessageInputFieldFocus || _chatRootViewModel.IsWhisperInputFieldFocus))
				_chatRootViewModel?.ChatUIOnKeyPressed();
			UpdateSpeechBubble();
		}

		public List<long> GetSpeechBubbleKeys() => _speechBubbleDictionary.Count <= 0 ? null : _speechBubbleDictionary.Keys.ToList();

		public void ResetProperties()
		{
			if (_chatRootViewModel == null) return;
			_chatRootViewModel.ResetProperties();
			_speechBubbleDictionary.Clear();
		}

		public void ConnectChatUI(ChatRootViewModel chatRootViewModel)
		{
			if (_chatRootViewModel == null)
				_chatRootViewModel = chatRootViewModel;

			_chatRootViewModel.AllTabEnable = true;
			InitializeSceneChange();
		}

		private void InitializeSceneChange()
		{
			if (_chatRootViewModel.AreaTabEnable)
				SyncAreaChatMessages();
		}

		public void FocusOutChatting()
		{
			if (_chatRootViewModel != null)
				_chatRootViewModel.WindowState = (int)ChatRootViewModel.eWindowState.INACTIVE;
		}

		public void ClearSentPlaceOfAreaMessage()
		{
			_chatRootViewModel?.ClearPrevAreaInfo();
		}

		public void RefreshViewMessageCollection()
		{
			_chatRootViewModel?.RefreshViewMessageCollection();
		}

#region Sync View
		public void SyncAreaChatMessages(LinkedList<ChatCoreBase.MessageInfo> areaMessages = null)
		{
			if (_chatRootViewModel == null) return;
			LinkedList<ChatCoreBase.MessageInfo> messageList = null;
			messageList = areaMessages;

			if (messageList == null || messageList.Count <= 0) return;

			_cts ??= new CancellationTokenSource();
			SetChatCollection(_cts, messageList, ChatCoreBase.eMessageType.AREA).Forget();
		}

		public void SyncWhisperMessages(LinkedList<ChatCoreBase.MessageInfo> whisperMessages = null)
		{
			if (_chatRootViewModel == null) return;
			LinkedList<ChatCoreBase.MessageInfo> messageList = null;
			messageList = whisperMessages;

			if (messageList == null || messageList.Count <= 0) return;

			_cts ??= new CancellationTokenSource();
			SetChatCollection(_cts, messageList, ChatCoreBase.eMessageType.WHISPER).Forget();
		}

		private async UniTask<bool> SetChatCollection(CancellationTokenSource cancellationTokenSource, LinkedList<ChatCoreBase.MessageInfo> messageList, ChatCoreBase.eMessageType type)
		{
			var context = SynchronizationContext.Current;
			if (!await UniTaskHelper.TrySwitchToMainThread(context, cancellationTokenSource))
			{
				C2VDebug.LogWarning("Failed to switch to main thread.");
				await ReturnContext(context);
				return false;
			}

			if (_chatRootViewModel == null)
			{
				C2VDebug.LogWarning("ChatRootViewModel is null.");
				await ReturnContext(context);
				return false;
			}

			var currentTap = (ChatRootViewModel.eChatType)_chatRootViewModel.CurrentTabType;
			SetTapChatCollection(currentTap, messageList, type);
			SetAllChatCollection(currentTap, messageList, type);

			await ReturnContext(context);
			return true;
		}

		private void SetTapChatCollection(ChatRootViewModel.eChatType currentTap, LinkedList<ChatCoreBase.MessageInfo> messageList, ChatCoreBase.eMessageType type)
		{
			switch (type)
			{
				case ChatCoreBase.eMessageType.AREA:
					if (!ChatManager.AreaTabActive) return;

					var areaViewModel = SetViewModel(ChatRootViewModel.eChatType.AREA, messageList.Last());
					AddMessageToCollection(_chatRootViewModel?.AreaChatMessageCollection, areaViewModel);
					if (currentTap is ChatRootViewModel.eChatType.AREA)
					{
						_isScrollDown = true;
						AddMessageToCollection(_chatRootViewModel?.ViewMessageCollection, areaViewModel);
					}

					break;
				case ChatCoreBase.eMessageType.NEARBY:
					if (!ChatManager.NearbyTabActive) return;

					break;
				case ChatCoreBase.eMessageType.WHISPER:
					if (!ChatManager.WhisperTabActive) return;

					var whisperViewModel = SetViewModel(ChatRootViewModel.eChatType.WHISPER, messageList.Last());
					AddMessageToCollection(_chatRootViewModel?.WhisperMessageCollection, whisperViewModel);
					if (currentTap is ChatRootViewModel.eChatType.WHISPER)
					{
						_isScrollDown = true;
						AddMessageToCollection(_chatRootViewModel?.ViewMessageCollection, whisperViewModel);
					}

					break;
			}
		}

		private void SetAllChatCollection(ChatRootViewModel.eChatType currentTap, LinkedList<ChatCoreBase.MessageInfo> messageList, ChatCoreBase.eMessageType type)
		{
			if (!ChatManager.AllTabActive) return;

			var allViewModel = SetViewModel(ChatRootViewModel.eChatType.ALL, messageList.Last());
			switch (type)
			{
				case ChatCoreBase.eMessageType.AREA:
					if (!_isStoredAreaMessage)
						break;

					AddMessageToCollection(_chatRootViewModel?.AllMessageCollection, allViewModel);
					break;

				case ChatCoreBase.eMessageType.NEARBY:
					AddMessageToCollection(_chatRootViewModel?.AllMessageCollection, allViewModel);

					break;

				case ChatCoreBase.eMessageType.WHISPER:
					if (!_isStoredWhisperMessage)
						break;

					AddMessageToCollection(_chatRootViewModel?.AllMessageCollection, allViewModel);
					break;
			}

			if (currentTap is ChatRootViewModel.eChatType.ALL)
			{
				_isScrollDown = true;
				AddMessageToCollection(_chatRootViewModel?.ViewMessageCollection, allViewModel);
			}
		}

		private static async UniTask ReturnContext(SynchronizationContext context)
		{
			await UniTaskHelper.TrySwitchToSynchronizationContext(context);
		}

		public void SyncSystemMessages(LinkedList<ChatCoreBase.MessageInfo> allMessages)
		{
			LinkedList<ChatCoreBase.MessageInfo> messageList = null;
			messageList = allMessages;

			if (messageList is not { Count: > 0 }) return;
			if (_chatRootViewModel == null) return;

			var allViewModel = SetViewModel(ChatRootViewModel.eChatType.ALL, messageList.Last());

			AddMessageToCollection(_chatRootViewModel?.AllMessageCollection, allViewModel);
			AddMessageToCollection(_chatRootViewModel?.ViewMessageCollection, allViewModel);

			if (_chatRootViewModel?.CurrentTabType is (int)ChatRootViewModel.eChatType.ALL)
				_isScrollDown = true;
		}

		private void AddMessageToCollection(Collection<ChatMessageViewModel> collection, ChatMessageViewModel messageViewModel)
		{
			if (collection == null || TableSetting == null)
				return;

			if (collection.CollectionCount >= TableSetting.ChatMaxCount)
				collection.RemoveItem(0);

			collection.AddItem(messageViewModel);
		}

		private void ScrollDown()
		{
			_isScrollDown = false;
			_chatRootViewModel.ScrollDownCheck();
		}

		private ChatMessageViewModel SetViewModel(ChatRootViewModel.eChatType type, ChatCoreBase.MessageInfo messageInfo)
		{
			ChatMessageViewModel viewModel;
			switch (type)
			{
				case ChatRootViewModel.eChatType.ALL:
					viewModel = _allViewModelPool.Get();
					break;
				case ChatRootViewModel.eChatType.AREA:
					viewModel = _areaViewModelPool.Get();
					break;
				case ChatRootViewModel.eChatType.NEARBY:
					viewModel = _nearbyViewModelPool.Get();
					break;
				case ChatRootViewModel.eChatType.WHISPER:
					viewModel = _whisperViewModelPool.Get();
					break;
				default:
					viewModel = _allViewModelPool.Get();
					break;
			}

			viewModel.Initialize();
			if (messageInfo.Type == ChatCoreBase.eMessageType.SYSTEM)
			{
				viewModel.MessageColorType = eChatColorType.SYSTEM;
			}
			else
			{
				// TODO: 오퍼레이커, 스피커 등 타입 설정
				viewModel.MessageColorType = messageInfo.UserID == User.Instance.CurrentUserData.ID ? eChatColorType.CHAT_MY_SELF : eChatColorType.CHAT_DEFAULT;
			}

			viewModel.MessageInfo = messageInfo;
			return viewModel;
		}
#endregion
		public void CreateSpeechBubble(MapObject mapObject, string message, ChatCoreBase.eMessageType messageType, int emotion = -1)
		{
			var activeObjectManagerViewModel = ViewModelManager.Instance.GetOrAdd<ActiveObjectManagerViewModel>();
			if (!activeObjectManagerViewModel.TryGet(mapObject.ObjectID, out var avatarViewModel))
			{
				C2VDebug.LogErrorCategory(GetType().Name, "Can't find activeObject ViewModel");
				return;
			}

			var chatViewModel = avatarViewModel.ChatViewModel;

			chatViewModel.DisableChat();
			_speechBubbleDictionary.TryAdd(mapObject.OwnerID, chatViewModel);

			chatViewModel.PlayBackTime = 0f;

			chatViewModel.IsOnChatEmoticon = true;
			chatViewModel.ChatMessage      = (emotion == -1 ? message : string.Empty) ?? string.Empty;
			chatViewModel.IsOnChatBalloon  = emotion == -1;
			chatViewModel.MessageType      = messageType;
			chatViewModel.SetEmotionID(emotion);

			SetSortOrderToSpeechBubble(chatViewModel);
		}

		private void SetSortOrderToSpeechBubble(ChatUserViewModel chatViewModel)
		{
			while (_chatViewModelPriorityQueue.TryPeek(out var viewModel, out var sortOrder))
			{
				if (viewModel == null || viewModel == chatViewModel)
				{
					_chatViewModelPriorityQueue.Dequeue();
					continue;
				}

				if (!viewModel.IsOnChatBalloon)
				{
					_chatViewModelPriorityQueue.Dequeue();
					continue;
				}

				chatViewModel.SortOrder = Mathf.Clamp(sortOrder + 1, ChatSortOrderMinValue, ChatSortOrderMaxValue);
				_chatViewModelPriorityQueue.Enqueue(chatViewModel, chatViewModel.SortOrder);
				return;
			}

			chatViewModel.SortOrder = ChatSortOrderMinValue;
			_chatViewModelPriorityQueue.Enqueue(chatViewModel, ChatSortOrderMinValue);
		}

		public void ReleaseObjects()
		{
			_allViewModelPool.Clear();
			_areaViewModelPool.Clear();
			_nearbyViewModelPool.Clear();
			_whisperViewModelPool.Clear();
			_chatRootViewModel?.OnRelease();
		}

		public void OpenChatUI()
		{
			if (_isSelectChanging) return;
			ChangeOnState().Forget();
		}

		private async UniTaskVoid ChangeOnState()
		{
			_isSelectChanging = true;
			_chatRootViewModel?.ChatUIOnKeyPressed();
			await UniTask.NextFrame();
			_isSelectChanging = false;
		}

		public void OnLanguageChanged()
		{
			// TODO
		}

		public void SetAreaChatInitialize(bool isSimpleMode)
		{
			if (_chatRootViewModel == null) return;
			if (isSimpleMode)
				_chatRootViewModel.WindowState = (int)ChatRootViewModel.eWindowState.MINIMIZE;
			else
				_chatRootViewModel.WindowState = (int)ChatRootViewModel.eWindowState.MAXIMIZE;
			_chatRootViewModel.AreaTabEnable = true;
			_chatRootViewModel.ChatMessage = string.Empty;
			SyncAreaChatMessages();
		}

#region Option
		public void OnChattingFilterChanged(eChattingFilter filter)
		{
			if (_chatRootViewModel == null) return;

			if (_chatRootViewModel.AllTabEnable)
				_chatRootViewModel.RefreshAllMessages();

			_isStoredAreaMessage    = filter.HasFlag(eChattingFilter.AREA);
			_isStoredWhisperMessage = filter.HasFlag(eChattingFilter.WHISPER);
		}

		public void OnChattingBubbleFilterChanged(eChattingBubbleFilter filter)
		{
			if (_chatRootViewModel == null || _chatViewModelPriorityQueue == null) return;

			while (_chatViewModelPriorityQueue.TryPeek(out var viewModel, out var _))
			{
				if (viewModel == null)
				{
					_chatViewModelPriorityQueue.Dequeue();
					continue;
				}

				if (!viewModel.IsOnChatBalloon)
				{
					_chatViewModelPriorityQueue.Dequeue();
					continue;
				}

				var messageType   = viewModel.MessageType;
				var isOptionMatch = false;
				isOptionMatch |= messageType == ChatCoreBase.eMessageType.AREA   && (filter & eChattingBubbleFilter.AREA)   != 0;
				isOptionMatch |= messageType == ChatCoreBase.eMessageType.NEARBY && (filter & eChattingBubbleFilter.NEARBY) != 0;

				if (!isOptionMatch)
				{
					viewModel.DisableChat();
					_chatViewModelPriorityQueue.Dequeue();
					continue;
				}
				return;
			}
		}

		public void OnFontSizeChanged(float _)
		{
			if (_chatRootViewModel?.ViewMessageCollection?.Value == null)
				return;

			foreach (var viewModel in _chatRootViewModel.ViewMessageCollection.Value)
				viewModel?.RefreshSettingProperties();
		}

		public void OnTimeStampChanged(bool _)
		{
			if (_chatRootViewModel?.ViewMessageCollection?.Value == null)
				return;

			foreach (var viewModel in _chatRootViewModel.ViewMessageCollection.Value)
				viewModel?.RefreshSettingProperties();
		}

		public void OnChatTransparentChanged(float _)
		{
			_chatRootViewModel?.RefreshOptionValues();
		}
#endregion Option
	}
}
