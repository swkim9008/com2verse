/*===============================================================
* Product:		Com2Verse
* File Name:	GuestAccessViewModel.cs
* Developer:	jhkim
* Date:			2023-06-15 20:06
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Com2Verse.GuestAccess
{
	[ViewModelGroup("GuestAccess")]
	public sealed class GuestAccessViewModel : ViewModelBase, IDisposable
	{
#region Variables
		private static readonly string ResName = "UI_Popup_Guest_Access";
		private static GUIView _view;

		private bool   _canUse;
		private string _code;
		private string _nickName;

		private readonly GuestAccessController _controller;
#endregion // Variables

#region Properties
		[UsedImplicitly]
		public bool CanUse
		{
			get => _canUse;
			set => SetProperty(ref _canUse, value);
		}

		[UsedImplicitly]
		public string Code
		{
			get => _code;
			set => SetProperty(ref _code, value);
		}

		[UsedImplicitly]
		public string NickName
		{
			get => _nickName;
			set
			{
				if (_controller == null) return;

				if (_controller.GetBytes(value) > GuestAccessController.MaxNickNameBytes)
					value = _nickName;

				SetProperty(ref _nickName, value);
			}
		}

		[UsedImplicitly] public CommandHandler Enter { get; private set; }
		[UsedImplicitly] public CommandHandler Close { get; private set; }
#endregion // Properties

#region Initialize
		public GuestAccessViewModel()
		{
			_controller = new();

			Enter = new CommandHandler(OnEnter, null);
			Close = new CommandHandler(OnClose, null);
		}
#endregion // Initialize

#region View
		private static void Hide()
		{
			_view?.Hide();
			_view = null;
		}
#endregion // View

#region Binding Events
		private void OnEnter()
		{
			_controller?.RequestEnterAsync(Code, NickName, Hide);
		}

		private void OnClose() => Hide();

		public void SetView(GUIView view)
		{
			_view    = view;
			CanUse   = CurrentScene.SpaceCode is not eSpaceCode.MEETING;
			Code     = string.Empty;
			NickName = string.Empty;
		}
#endregion // Binding Events

#region Dispose
		public void Dispose()
		{
			Hide();
		}
#endregion // Dispose
	}
}
