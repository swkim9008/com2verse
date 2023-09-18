/*===============================================================
* Product:		Com2Verse
* File Name:	MiceLoungeArea.cs
* Developer:	ikyoung
* Date:			2023-03-31 12:03
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Network;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using System.Threading;
using Com2Verse.UI;

namespace Com2Verse.Mice
{
    public sealed class MiceLoungeArea : MiceArea
	{
        public MiceLoungeArea() { MiceAreaType = eMiceAreaType.LOUNGE; }
        
        public override async UniTask RequestEnterMiceHall(MiceSessionID sessionID, CancellationTokenSource cts)
        {
            await base.RequestEnterMiceHall(sessionID, cts);
            C2VDebug.LogMethod(GetType().Name);
            Commander.Instance.RequestEnterMiceHall(sessionID);
        }
        public override async UniTask RequestEnterMiceLobby(CancellationTokenSource cts)
        {
            await base.RequestEnterMiceLobby(cts);
            C2VDebug.LogMethod(GetType().Name);
            Commander.Instance.RequestEnterMiceLobby();
        }
        public override string CurrentAreaDisplayInfo()
        {
            // TODO 임시
            return $"현재 위치\n{MiceAreaType.ToName()}\n{MiceService.Instance.EventID}번 행사";
        }
        
        public override long GetRoomID()
        {
            return MiceService.Instance.EventID;
        }
        
        public override MiceWebClient.MiceType GetMiceType()
        {
            return MiceWebClient.MiceType.EventLounge;
        }
        
        public override async UniTask ShowKioskMenu(string url)
        {
            await MiceService.Instance.ShowKioskSessionMenu(url);
        }
    }
}
