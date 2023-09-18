/*===============================================================
 * Product:		Com2Verse
 * File Name:	LobbyNoticeViewModel.cs
 * Developer:	yangsehoon
 * Date:		2022-12-09 오후 5:06
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.UI
{
	public class LobbyNoticeViewModel : ViewModelBase
	{
		private float _screenWidth;

		public RectTransform TargetTransform { get; set; }
		public CommandHandler OnClickLobbyScreen { get; }

		public Vector2 ScreenWidth {
			get => Vector2.zero;
			set
			{
				_screenWidth = value.x;
				InvokePropertyValueChanged(nameof(ScreenWidth), ScreenWidth);
			}
		}

		private int CurrentPostIndex => Mathf.RoundToInt(-TargetTransform.anchoredPosition.x / _screenWidth);

		public LobbyNoticeViewModel()
		{
			OnClickLobbyScreen = new CommandHandler(ShowNotice);
		}

		private void ShowNotice()
		{
			NoticeManager.Instance.ShowNoticeAtIndex(NoticeManager.LobbyMapId, CurrentPostIndex);
		}
	}
}
