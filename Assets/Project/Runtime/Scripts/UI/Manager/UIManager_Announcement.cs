/*===============================================================
* Product:		Com2Verse
* File Name:	UIManager_Announcement.cs
* Developer:	ksw
* Date:			2023-05-23 18:48
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Network;
using Cysharp.Threading.Tasks;

namespace Com2Verse.UI
{
	public partial class UIManager
	{
		private static readonly int    PumpingLoopTime          = 10;
		private static readonly string AvatarSelectionSceneName = "SceneAvatarSelection";
		
		private          AnnouncementViewModel _announcementViewModel;
		private readonly Queue<string>         _announcementQueue = new();
		private          bool                  _isPolling         = false;

		public void ShowAnnouncement(string message)
		{
			if (CurrentScene.SceneName.Equals(AvatarSelectionSceneName)) return;
			EnqueueSystemNotice(message);
		}

		private void ShowAnnouncementPopup(string message, bool loop = false, float speed = 100f)
		{
			var announcementPopup = GetSystemView(eSystemViewType.ANNOUNCEMENT);
			if (announcementPopup.IsUnityNull())
			{
				C2VDebug.LogError("Announcement View is null");
				return;
			}

			announcementPopup.Show();


			_announcementViewModel = announcementPopup.ViewModelContainer.GetViewModel<AnnouncementViewModel>();

			_announcementViewModel.Message       = message;
			_announcementViewModel.TextMoveSpeed = speed;
			_announcementViewModel.IsLoop        = loop;
			_announcementViewModel.IsPlay        = true;
		}
		
		public void HideAnnouncement()
		{
			var announcementPopup = GetSystemView(eSystemViewType.ANNOUNCEMENT);

			if (announcementPopup.IsUnityNull()) return;

			if (_announcementViewModel != null)
				_announcementViewModel.IsPlay = false;

			announcementPopup.Hide();
		}

		public void ClearAnnouncement()
		{
			HideAnnouncement();
			_announcementQueue!.Clear();
			_isPolling                             =  false;
			NetworkManager.Instance.OnDisconnected -= ClearAnnouncement;
		}

		private void EnqueueSystemNotice(string message)
		{
			if (GeneralData.General != null)
			{
				for (int i = 0; i < GeneralData.General.NoticeRepeatCount; ++i)
					_announcementQueue!.Enqueue(message);
			}
			else
				_announcementQueue!.Enqueue(message);
			PollingSystemNoticeQueue().Forget();
		}

		private async UniTaskVoid PollingSystemNoticeQueue()
		{
			if (_isPolling) return;
			_isPolling                       =  true;
			NetworkManager.Instance.OnDisconnected += ClearAnnouncement;
			while (_announcementQueue!.Count > 0)
			{
				if (_announcementViewModel is { IsPlay: true })
				{
					await UniTask.Delay(PumpingLoopTime);
					continue;
				}
				var message = _announcementQueue.Dequeue();
				ShowAnnouncementPopup(message);
			}
			_isPolling                             =  false;
			NetworkManager.Instance.OnDisconnected -= ClearAnnouncement;
		}
	}
}
