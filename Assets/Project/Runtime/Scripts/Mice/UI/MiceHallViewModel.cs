/*===============================================================
* Product:		Com2Verse
* File Name:	MiceHallViewModel.cs
* Developer:	ikyoung
* Date:			2023-04-04 14:40
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Mice;
using Com2Verse.Network;
using Cysharp.Threading.Tasks;

namespace Com2Verse.UI
{
    [ViewModelGroup("Mice")]
    public sealed partial class MiceHallViewModel : ViewModelBase
    {
        public CommandHandler EnterMiceLobbyFromHall { get; private set; }
        public CommandHandler EnterMiceLoungeFromHall { get; private set; }
        public CommandHandler EnterMiceEntryFromHall { get; private set; }
        
        public MiceHallViewModel()
        {
            RegisterCommandHandlers();
        }

        private void RegisterCommandHandlers()
        {
            EnterMiceLobbyFromHall = new CommandHandler(OnEnterMiceLobbyClick);
            EnterMiceLoungeFromHall = new CommandHandler(OnEnterMiceLoungeClick);
            EnterMiceEntryFromHall = new CommandHandler(OnEnterMiceEntryClick);
        }
        private void OnEnterMiceEntryClick()
        {
            Commander.Instance.RequestEnterWorld();
        }
        private void OnEnterMiceLobbyClick()
        {
            MiceService.Instance.RequestEnterMiceLobby().Forget();
        }
        private void OnEnterMiceLoungeClick()
        {
            MiceService.Instance.RequestEnterLastMiceLounge().Forget();
        }
    }
}
