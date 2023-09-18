/*===============================================================
 * Product:		Com2Verse
 * File Name:	NoticeManager.cs
 * Developer:	yangsehoon
 * Date:		2022-12-07 오전 11:35
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Chat;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Network;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Protocols.GameLogic;

namespace Com2Verse.UI
{
	public sealed class NoticeManager : Singleton<NoticeManager>, IDisposable
	{
		private const string WorldSpaceCodeName = "MapData_Property_Name_2";
		private const string SpaceCodeNameRoot  = "SpaceCodeName";

		private struct TextKey
		{
			public const string UIChatSystemLoginMsg               = "UI_Chat_System_Login_Msg";
			public const string UIChatSystemEnterSpaceMsg          = "UI_Chat_System_EnterSpace_Msg";
			public const string UIChatSystemEnterSpaceOtherUserMsg = "UI_Chat_System_EnterSpace_OtherUser_Msg";
			public const string UIChatSystemEnterAreaMsg           = "UI_Chat_System_Enter_Area_Msg";
		}

		[Flags]
		public enum eNoticeType
		{
			NONE       = 0,
			EVERYTHING = ~NONE,
			MAIN       = 1 << 0,
			CHATTING   = 1 << 1,
		}

		private struct NoticeData
		{
			public string      Message;
			public eNoticeType Type;

			public NoticeData(string message, eNoticeType type)
			{
				Message = message;
				Type    = type;
			}
		}

		/// <summary>
		/// FIXME: 임시 처리
		/// </summary>
		public static readonly long LobbyMapId = 10000100011;

		// private TableNoticeBoard  _data;

		// private readonly List<NoticeBoard> _resultList = new();

		private const string NoticePrefabName       = "UI_Popup_Notice";
		private const string PopupOpenAnimationName = "Popup_Open";

		private GUIView _noticeGUI;

		private readonly HashSet<long> _checkNoticePopupOnceHashSet = new HashSet<long>();

		private bool _isSceneLoaded;
		private bool _needEnterMessage;

		private readonly Queue<NoticeData> _noticeQueue = new();

		[UsedImplicitly] private NoticeManager() { }

		public void Initialize()
		{
			AddEvents();
		}

		public void Dispose()
		{
			RemoveEvents();
		}

		private void AddEvents()
		{
			SceneManager.Instance.BeforeSceneChanged  += OnBeforeSceneChanged;
			SceneManager.Instance.CurrentSceneChanged += OnCurrentSceneChanged;

			Network.GameLogic.PacketReceiver.Instance.SystemNoticeResponse += OnSystemNoticeResponse;
			PacketReceiver.Instance.OnLoginResponseEvent += OnLoginResponseEvent;
		}

		private void RemoveEvents()
		{
			if (SceneManager.InstanceExists)
			{
				SceneManager.Instance.BeforeSceneChanged  -= OnBeforeSceneChanged;
				SceneManager.Instance.CurrentSceneChanged -= OnCurrentSceneChanged;
			}

			if (Network.GameLogic.PacketReceiver.InstanceExists)
				Network.GameLogic.PacketReceiver.Instance.SystemNoticeResponse -= OnSystemNoticeResponse;
			if (PacketReceiver.InstanceExists)
				PacketReceiver.Instance.OnLoginResponseEvent -= OnLoginResponseEvent;
		}

		private void ProcessNotice(string message, eNoticeType type)
		{
			if (type == eNoticeType.NONE)
			{
				C2VDebug.LogWarningCategory(GetType().Name, "NoticeType is None");
				return;
			}

			if (type.HasFlag(eNoticeType.CHATTING))
				ChatManager.Instance.SendSystemMessage(message);

			if (type.HasFlag(eNoticeType.MAIN))
				UIManager.Instance.ShowAnnouncement(message);
		}

#region Public Methods
		public void SendNotice(string message, eNoticeType type)
		{
			if (_isSceneLoaded)
				ProcessNotice(message, type);
			else
				_noticeQueue?.Enqueue(new NoticeData(message, type));
		}

		public void OnEnterChattingUserInGroup(long userId, string userName)
		{
			// 월드와 마이스 씬에서 다른 유저의 입장 시스템 메시지 출력하지 않음
			if (CurrentScene.ServiceType is eServiceType.WORLD or eServiceType.MICE)
				return;

			if (userId == User.Instance.CurrentUserData.ID)
				return;

			var spaceCode = CurrentScene.SpaceCode;
			if (spaceCode == null) return;

			if (string.IsNullOrEmpty(userName)) 
				return;

			var spaceName = Localization.Instance.GetString($"{SpaceCodeNameRoot}_{(int)spaceCode}");
			if (string.IsNullOrEmpty(spaceName)) 
				return;

			SendNotice(Localization.Instance.GetString(TextKey.UIChatSystemEnterSpaceOtherUserMsg, userName, spaceName), eNoticeType.CHATTING);
		}
#endregion Public Methods

#region Events
		private void OnCurrentSceneChanged([NotNull] SceneBase prevScene, [NotNull] SceneBase currentScene)
		{
			_isSceneLoaded = true;

			if (_needEnterMessage) SendEnterMessage();
			SendSpaceMessage(prevScene, currentScene);

			if (_noticeQueue == null) return;

			while (_noticeQueue.TryDequeue(out var data))
				ProcessNotice(data.Message, data.Type);
		}

		private void SendEnterMessage()
		{
			ProcessNotice(Localization.Instance.GetString(TextKey.UIChatSystemLoginMsg), eNoticeType.CHATTING);
			_needEnterMessage = false;
		}

		private void SendSpaceMessage([NotNull] SceneBase prevScene, [NotNull] SceneBase currentScene)
		{
			if (prevScene.IsWorldScene && currentScene.IsWorldScene) return;

			ChatManager.InstanceOrNull?.RefreshViewMessageCollection();
			string spaceName;
			if (currentScene.IsWorldScene)
			{
				spaceName = Localization.Instance.GetString(WorldSpaceCodeName);
			}
			else
			{
				var spaceCode = currentScene.SpaceCode;
				if (spaceCode == null) return;

				spaceName = Localization.Instance.GetString($"{SpaceCodeNameRoot}_{(int)spaceCode}");
			}

			if (string.IsNullOrEmpty(spaceName)) return;

			var message = Localization.Instance.GetString(TextKey.UIChatSystemEnterSpaceMsg, spaceName);
			ProcessNotice(message, eNoticeType.CHATTING);
		}

		private void OnBeforeSceneChanged([NotNull] SceneBase currentScene, [NotNull] SceneBase newScene)
		{
			_isSceneLoaded = false;
		}

		private void OnSystemNoticeResponse(Protocols.Notification.AnnouncementNotify response)
		{
			SendNotice(response.Message, eNoticeType.EVERYTHING);
		}

		private void OnLoginResponseEvent(LoginCom2verseResponse _)
		{
			_needEnterMessage = true;
		}
#endregion Events

#region Office Notice
		private void CloseLastGUI()
		{
			if (!_noticeGUI.IsUnityNull())
				_noticeGUI.Hide();
		}

		public void ShowNoticeList(long mapId, bool force = false)
		{
			if (!force)
			{
				if (_checkNoticePopupOnceHashSet.Contains(mapId))
					return;

				_checkNoticePopupOnceHashSet.Add(mapId);
			}

			InitAndLoad();

			// _resultList.Clear();
			//
			// foreach (var post in _data.Datas.Values)
			// {
			// 	if (post.MapId == mapId)
			// 	{
			// 		_resultList.Add(post);
			// 	}
			// }

			UIManager.Instance.CreatePopup(NoticePrefabName, async (guiView) =>
			{
				_noticeGUI = guiView;
				guiView.Show();

				var otherViewModel = guiView.ViewModelContainer.GetViewModel<NoticeItemViewModel>();
				otherViewModel.TabActive = false;

				var viewModel = guiView.ViewModelContainer.GetViewModel<NoticeListViewModel>();
				viewModel.TabActive = false;
				await UniTask.WaitUntil(() => !guiView.AnimationPlayer.IsPlaying(PopupOpenAnimationName));
				// viewModel.SetPosts(mapId, _resultList);
				viewModel.InitializeScroll();
				viewModel.TabActive = true;
			}).Forget();
		}

		public void ShowNotice(int key)
		{
			InitAndLoad();

			// if (_data.Datas.TryGetValue(key, out var post))
			// {
			// 	UIManager.Instance.CreatePopup(NoticePrefabName, async (guiView) =>
			// 	{
			// 		_noticeGUI = guiView;
			// 		guiView.Show();
			//
			// 		var otherViewModel = guiView.ViewModelContainer.GetViewModel<NoticeListViewModel>();
			// 		otherViewModel.TabActive = false;
			// 		otherViewModel.SetPosts(post.MapId, null);
			//
			// 		var viewModel = guiView.ViewModelContainer.GetViewModel<NoticeItemViewModel>();
			// 		viewModel.TabActive = false;
			// 		await UniTask.WaitUntil(() => !guiView.AnimationPlayer.IsPlaying(PopupOpenAnimationName));
			// 		viewModel.PostItem = post;
			// 		viewModel.InitializeScroll();
			// 		viewModel.TabActive = true;
			// 	}).Forget();
			// }
		}

		public void ShowNoticeAtIndex(long mapId, int index)
		{
			InitAndLoad();

			// int i = 0;
			// foreach (var post in _data.Datas.Values)
			// {
			// 	if (post.MapId == mapId)
			// 	{
			// 		if (i++ == index)
			// 		{
			// 			ShowNotice(post.PK);
			// 			return;
			// 		}
			// 	}
			// }
		}

		private void InitAndLoad()
		{
			// if (_data == null)
			// {
			// 	LoadData();
			// }

			CloseLastGUI();
		}
#endregion Office Notice
	}
}
