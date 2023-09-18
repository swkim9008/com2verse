/*===============================================================
* Product:		Com2Verse
* File Name:	MiceAreaLobby.cs
* Developer:	ikyoung
* Date:			2023-03-31 11:46
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/
using Com2Verse.Network;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using System.Threading;
using Com2Verse.UI;
using Com2Verse.Data;

namespace Com2Verse.Mice
{
	public sealed class MiceLobbyArea : MiceArea
	{
        public MiceLobbyArea() { MiceAreaType = eMiceAreaType.LOBBY; }

        public override async UniTask RequestEnterMiceLounge(MiceEventID eventID, CancellationTokenSource cts)
        {
	        await base.RequestEnterMiceLounge(eventID, cts);
	        C2VDebug.LogMethod(GetType().Name);
	        Commander.Instance.RequestEnterMiceLounge(eventID);
        }
        public override async UniTask RequestEnterMiceFreeLounge(MiceEventID eventID, CancellationTokenSource cts)
        {
	        await base.RequestEnterMiceLounge(eventID, cts);
	        C2VDebug.LogMethod(GetType().Name);
	        Commander.Instance.RequestEnterMiceFreeLounge(eventID);
        }
        public override async UniTask RequestEnterMiceHall(MiceSessionID sessionID, CancellationTokenSource cts)
        {
	        await base.RequestEnterMiceHall(sessionID, cts);
	        C2VDebug.LogMethod(GetType().Name);
	        Commander.Instance.RequestEnterMiceHall(sessionID);
        }
        public override MiceWebClient.MiceType GetMiceType()
        {
	        return MiceWebClient.MiceType.Lobby;
        }
        
        public override async UniTask ShowKioskMenu(string url)
        {
	        await MiceService.Instance.ShowKioskEventMenu(url);
        }
        
        public override async UniTask OnKioskClosePage(MiceKioskWebViewMessage kioskMessage, GUIView guiView)
        {
	        UIManager.Instance.ShowPopupYesNo(Data.Localization.eKey.MICE_UI_Popup_Title_Lobby_Exit.ToLocalizationString(), Data.Localization.eKey.MICE_UI_Popup_Msg_Lobby_Exit.ToLocalizationString(),
		        (guiView) =>
		        {
			        MiceService.Instance.LastAreaChangeReason = eMiceAreaChangeReason.ExitButton;
			        Commander.Instance.LeaveBuildingRequest();
		        });
	        await UniTask.CompletedTask;
        }
    }
}
