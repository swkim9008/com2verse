/*===============================================================
* Product:		Com2Verse
* File Name:	NotificationPermissionPopUpViewModel.cs
* Developer:	ydh
* Date:			2023-05-08 19:55
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Network;

namespace Com2Verse.UI
{
	[ViewModelGroup("Notification")]
	public sealed class NotificationPermissionPopUpViewModel : ViewModel
	{
		public CommandHandler Command_OkButton { get; }
		public CommandHandler Command_AccessRightButton { get; }
		public CommandHandler Command_ReceivedButton { get; }
		public CommandHandler Command_CloseButton { get; }

		private bool _accessRight;

		public bool AccessRight
		{
			get => _accessRight;
			set
			{
				_accessRight = value;
				SetProperty(ref _accessRight, value);
			}
		}
		
		private bool _received;
		public bool Received
		{
			get => _received;
			set
			{
				_received = value;
				SetProperty(ref _received, value);
			}
		}

		public Action CloseAction;

		public NotificationPermissionPopUpViewModel()
		{
			Command_OkButton = new CommandHandler(OnCommand_OkButton);
			Command_AccessRightButton = new CommandHandler(OnCommand_AccessRightButton);
			Command_ReceivedButton = new CommandHandler(OnCommand_ReceivedButton);
			Command_CloseButton = new CommandHandler(OnCommand_CloseButton);
		}

		private void OnCommand_CloseButton()
		{
			CloseAction?.Invoke();
		}

		private void OnCommand_OkButton()
		{
			Commander.Instance.RequestNotificationOption(1,0,_accessRight, 1,1,_received);
		}
		
		private void OnCommand_AccessRightButton()
		{
			AccessRight = !AccessRight;
		}
		
		private void OnCommand_ReceivedButton()
		{
			Received = !Received;

			if (Received == true)
				AccessRight = true;
		}
	}
}