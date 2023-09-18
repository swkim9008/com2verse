/*===============================================================
* Product:		Com2Verse
* File Name:	MiceLobbyViewModel.cs
* Developer:	ikyoung
* Date:			2023-04-04 11:57
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using Com2Verse.Mice;
using Com2Verse.Network;
using Cysharp.Threading.Tasks;

namespace Com2Verse.UI
{
    [ViewModelGroup("Mice")]
    public sealed partial class MiceLobbyViewModel : ViewModelBase
    {
        private GUIView _menuView;
        
        public CommandHandler EnterMiceLoungeFromLobby { get; private set; }
        public CommandHandler EnterMiceEntryFromLobby { get; private set; }
        
        public MiceLobbyViewModel()
        {
            RegisterCommandHandlers();
        }

        private void RegisterCommandHandlers()
        {
            EnterMiceLoungeFromLobby = new CommandHandler(OnEnterMiceLoungeClick);
            EnterMiceEntryFromLobby = new CommandHandler(OnEnterMiceEntryClick);
        }
        public override void OnRelease()
        {
            _menuView = null;
            base.OnRelease();
        }

        private void OnEnterMiceEntryClick()
        {
            Commander.Instance.RequestEnterWorld();
        }
        private void OnEnterMiceLoungeClick()
        {
            //MiceService.Instance.ShowKioskEventMenu().Forget();
        }
    }
}
