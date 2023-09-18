/*===============================================================
* Product:		Com2Verse
* File Name:	MiceLoungeViewModel.cs
* Developer:	ikyoung
* Date:			2023-04-04 13:30
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using Com2Verse.Mice;
using Com2Verse.Network;
using Com2Verse.Logger;
using System;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;

namespace Com2Verse.UI
{
    [ViewModelGroup("Mice")]
    public sealed partial class MiceLoungeViewModel : ViewModelBase
    {
        private GUIView _menuView;
        
        public CommandHandler EnterMiceLobbyFromLounge { get; private set; }
        public CommandHandler EnterMiceHallFromLounge { get; private set; }
        public CommandHandler EnterMiceEntryFromLounge { get; private set; }
        
        public MiceLoungeViewModel()
        {
            RegisterCommandHandlers();
        }

        private void RegisterCommandHandlers()
        {
            EnterMiceLobbyFromLounge = new CommandHandler(OnEnterMiceLobbyClick);
            EnterMiceHallFromLounge = new CommandHandler(OnEnterMiceHallClick);
            EnterMiceEntryFromLounge = new CommandHandler(OnEnterMiceEntryClick);
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
        private void OnEnterMiceLobbyClick()
        {
            //MiceService.Instance.RequestEnterMiceLobby().Forget();
        }
        private void OnEnterMiceHallClick()
        {
            //MiceService.Instance.ShowKioskSessionMenu().Forget();
        }
	}
}
