/*===============================================================
* Product:		Com2Verse
* File Name:	MiceFreeLoungeArea.cs
* Developer:	ikyoung
* Date:			2023-07-24 15:25
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
	public sealed class MiceFreeLoungeArea : MiceArea
	{
		public MiceFreeLoungeArea() { MiceAreaType = eMiceAreaType.FREE_LOUNGE; }
		
		public override async UniTask RequestEnterMiceLobby(CancellationTokenSource cts)
		{
			await base.RequestEnterMiceLobby(cts);
			C2VDebug.LogMethod(GetType().Name);
			Commander.Instance.RequestEnterMiceLobby();
		}
		public override string CurrentAreaDisplayInfo()
		{
			// TODO 임시
			return $"현재 위치\n{MiceAreaType.ToName()}\n{MiceService.Instance.EventID}번 무료 행사";
		}
        
		public override long GetRoomID()
		{
			return MiceService.Instance.EventID;
		}
        
		public override MiceWebClient.MiceType GetMiceType()
		{
			return MiceWebClient.MiceType.EventFreeLounge;
		}
        
		public override async UniTask ShowKioskMenu(string url)
		{
			await MiceService.Instance.ShowKioskSessionMenu(url);
		}
	}
}
