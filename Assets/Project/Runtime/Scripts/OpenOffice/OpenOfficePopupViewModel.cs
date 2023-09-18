/*===============================================================
* Product:		Com2Verse
* File Name:	OpenOfficePopupViewModel.cs
* Developer:	jhkim
* Date:			2023-06-16 19:38
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.Data;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine.Device;

namespace Com2Verse.OpenOffice
{
	[ViewModelGroup("Open Office")]
	public sealed class OpenOfficePopupViewModel : ViewModelBase
	{
#region Variables
		private static readonly int MenuOpenAnimationIdx = 0;
		private static readonly int MenuCloseAnimationIdx = 1;

		private bool _isShowChatBot = true;
		private bool _isShowMenu = false;
		private int _playMenuAnimationIdx;

		private Dictionary<eSpaxeUrlType, string> _urlMap = new();
#endregion // Variables

#region Properties
		[UsedImplicitly]
		public bool IsShowChatBot
		{
			get => _isShowChatBot;
			set => SetProperty(ref _isShowChatBot, value);
		}
		[UsedImplicitly]
		public bool IsShowMenu
		{
			get => _isShowMenu;
			set => SetProperty(ref _isShowMenu, value);
		}
		[UsedImplicitly]
		public int PlayMenuAnimationIdx
		{
			get => _playMenuAnimationIdx;
			set => SetProperty(ref _playMenuAnimationIdx, value);
		}
		[UsedImplicitly] public CommandHandler ClickChatBot { get; private set; }
		[UsedImplicitly] public CommandHandler Close { get; private set; }
		[UsedImplicitly] public CommandHandler ShowIntroduction { get; private set; }
		[UsedImplicitly] public CommandHandler ShowRegistration { get; private set; }
#endregion // Properties

#region Initialize
		public OpenOfficePopupViewModel()
		{
			ClickChatBot = new CommandHandler(OnClickChatBot, null);
			Close = new CommandHandler(OnClose, null);
			ShowIntroduction = new CommandHandler(OnShowIntroduction, null);
			ShowRegistration = new CommandHandler(OnShowRegistration, null);

			LoadUrlProcess();
		}

		private void LoadUrlProcess()
		{
			_urlMap.Clear();

			var urlTable = TableDataManager.Instance.Get<TableSpaXeUrl>();
			if (urlTable?.Datas == null) return;

			LoadUrl(eSpaxeUrlType.INTRODUCE);
			LoadUrl(eSpaxeUrlType.REGISTRATION);

			void LoadUrl(eSpaxeUrlType type)
			{
				if (urlTable.Datas.ContainsKey(type))
					_urlMap.Add(type, urlTable.Datas[type].Url);
			}
		}
#endregion // Initialize

#region Binding Events
		private void OnClickChatBot() => ShowMenu();
		private void OnClose() => ShowChatBot();
		private void OnShowIntroduction() => TryOpenUrl(eSpaxeUrlType.INTRODUCE);
		private void OnShowRegistration() => TryOpenUrl(eSpaxeUrlType.REGISTRATION);
#endregion // Binding Events

		private void ShowChatBot()
		{
			IsShowChatBot = true;
			PlayMenuAnimation(false);
		}

		private void ShowMenu()
		{
			IsShowChatBot = false;
			IsShowMenu = true;
			PlayMenuAnimation(true);
		}

		private void PlayMenuAnimation(bool show)
		{
			PlayMenuAnimationIdx = show ? MenuOpenAnimationIdx : MenuCloseAnimationIdx;
		}

		private bool TryOpenUrl(eSpaxeUrlType type)
		{
			if (!_urlMap.ContainsKey(type)) return false;

			var url = _urlMap[type];
			if (string.IsNullOrWhiteSpace(url)) return false;

			Application.OpenURL(url);
			return true;
		}
		private bool TryShowWebView(eSpaxeUrlType type)
		{
			if (!_urlMap.ContainsKey(type)) return false;

			var url = _urlMap[type];

			if (string.IsNullOrWhiteSpace(url)) return false;

			UIManager.Instance.ShowPopupWebView(new UIManager.WebViewData {Url = url});
			return true;
		}
	}
}
