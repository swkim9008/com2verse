/*===============================================================
* Product:		Com2Verse
* File Name:	MiceArea.cs
* Developer:	ikyoung
* Date:			2023-03-31 11:47
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Net;
using System.Threading;
using Com2Verse.Logger;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using Protocols.Mice;

namespace Com2Verse.Mice
{
    public enum eMiceAreaType
    {
        NONE,
        LOBBY,
        LOUNGE,
        HALL,
        MEET_UP,
        WORLD,
        FREE_LOUNGE,
        MAX
    }

	public abstract class MiceArea
	{
        public eMiceAreaType MiceAreaType { get; protected set; }
        public eMiceAreaType PrevAreaType { get; protected set; }

        protected string _informationMessage = string.Empty;

        public virtual void Prepare()
		{
            C2VDebug.LogMethod(GetType().Name);
        }

		public virtual void OnStart(eMiceAreaType prevArea)
		{
			PrevAreaType = prevArea;
            C2VDebug.LogMethod(GetType().Name, $"{prevArea}");
        }

		public virtual void OnStop()
        {
            _informationMessage = string.Empty;
            C2VDebug.LogMethod(GetType().Name);
        }

        public virtual void OnEnterScene()
        {
            C2VDebug.LogMethod(GetType().Name);
        }
        public virtual void OnLeaveScene()
        {
            C2VDebug.LogMethod(GetType().Name);
        }
        public virtual void OnEnterMiceHallResponse(Protocols.Mice.EnterHallResponse response)
        {
            C2VDebug.LogMethod(GetType().Name, $"{response} {response.EnterRequestResult}");
        }

        public virtual void OnEnterMiceLoungeResponse(Protocols.Mice.EnterLoungeResponse response)
        {
            C2VDebug.LogMethod(GetType().Name, $"{response} {response.EnterRequestResult}");
        }
        public virtual void OnEnterMiceFreeLoungeResponse(Protocols.Mice.EnterFreeLoungeResponse response)
        {
            C2VDebug.LogMethod(GetType().Name, $"{response} {response.EnterRequestResult}");
        }

        public virtual void OnEnterMiceLobbyResponse(Protocols.Mice.EnterLobbyResponse response)
        {
            C2VDebug.LogMethod(GetType().Name, $"{response}");
        }

        public virtual void OnUsePortalResponse(Protocols.GameLogic.UsePortalResponse response)
        {
            C2VDebug.LogMethod(GetType().Name, $"{response}");
        }
        public virtual void OnTeleportUserStartNotifyResponse(Protocols.WorldState.TeleportUserStartNotify response)
        {
            C2VDebug.LogMethod(GetType().Name, $"{response}");
        }
        public virtual void OnMiceRoomNotify(Protocols.Mice.MiceRoomNotify response)
        {
            C2VDebug.LogMethod(GetType().Name, $"{response}");
        }
        public virtual void OnForcedMiceTeleportNotify(Protocols.Mice.ForcedMiceTeleportNotify response)
        {
            C2VDebug.LogMethod(GetType().Name, $"{response}");
        }
        public virtual async UniTask RequestEnterMiceLobby(CancellationTokenSource cts)
        {
            C2VDebug.LogMethod(GetType().Name);
            await UniTask.CompletedTask;
        }
        
        public virtual async UniTask RequestEnterMiceLounge(MiceEventID eventID, CancellationTokenSource cts)
        {
            C2VDebug.LogMethod(GetType().Name);
            await UniTask.CompletedTask;
        }
        public virtual async UniTask RequestEnterMiceFreeLounge(MiceEventID eventID, CancellationTokenSource cts)
        {
            C2VDebug.LogMethod(GetType().Name);
            await UniTask.CompletedTask;
        }
        public virtual async UniTask RequestEnterMiceHall(MiceSessionID sessionID, CancellationTokenSource cts)
        {
            C2VDebug.LogMethod(GetType().Name);
            await UniTask.CompletedTask;
        }
        
        // TODO 현재 위치를 표시하기 위한 임시작업
        public virtual string CurrentAreaDisplayInfo()
        {
            return $"현재 위치\n{MiceAreaType.ToName()}";
        }

        public virtual string CurrentInformationMessage()
        {
            return _informationMessage;
        }

        public virtual void OnError(EnterRequestResult enterResult)
        {
            if (enterResult != EnterRequestResult.Success)
            {
                NetworkUIManager.Instance.ShowMiceErrorMessage(enterResult);
            }
        }

        public virtual long GetRoomID()
        {
            return 0;
        }
        public virtual MiceWebClient.MiceType GetMiceType()
        {
            return MiceWebClient.MiceType.Lobby;
        }
        public virtual void OnServiceChangeNotify(Protocols.CommonLogic.ServiceChangeNotify response)
        {
        }
        public virtual void OnServiceChangeResponse(Protocols.GameLogic.ServiceChangeResponse response)
        {
        }
        public virtual void OnLeaveBuildingResponse(Protocols.GameLogic.LeaveBuildingResponse response)
        {
        }
        
        public virtual async UniTask ShowKioskMenu(string url)
        {
            await UniTask.CompletedTask;
        }

   
        public virtual async UniTask OnKioskEnterHall(MiceKioskWebViewMessage kioskMessage, GUIView guiView)
        {
            await MiceService.Instance.RequestEnterMiceHall(kioskMessage.SessionID.ToMiceSessionID());
        }
        
        public virtual async UniTask OnKioskEnterLounge(MiceKioskWebViewMessage kioskMessage, GUIView guiView)
        {
            await MiceService.Instance.RequestEnterMiceLounge(kioskMessage.EventID.ToMiceEventID());
        }
        public virtual async UniTask OnKioskEnterFreeLounge(MiceKioskWebViewMessage kioskMessage, GUIView guiView)
        {
            await MiceService.Instance.RequestEnterMiceFreeLounge(kioskMessage.EventID.ToMiceEventID());
        }
        
        public virtual async UniTask OnKioskRequireBusinessCard(MiceKioskWebViewMessage kioskMessage, GUIView guiView)
        {
            MiceBusinessCardBookViewModel.ShowView();
            await UniTask.WaitUntil(() => !MiceInfoManager.Instance.NeedToCreateUser);

            var eventID  = kioskMessage.EventID;
            
            eMiceKioskWebViewMessageType ret = eMiceKioskWebViewMessageType.None;
            if (Enum.TryParse<eMiceKioskWebViewMessageType>(kioskMessage.MessageType, out ret))
            {
                if (ret == eMiceKioskWebViewMessageType.RequireBusinessCardFreeLounge)
                {
                    await MiceService.Instance.RequestEnterMiceFreeLounge(eventID.ToMiceEventID());
                }
                else
                {
                    var response = await MiceWebClient.Event.EnterLoungePost_EventId(eventID);
                    if (response.Result.HttpStatusCode == HttpStatusCode.OK)
                        await MiceService.Instance.RequestEnterMiceLounge(eventID.ToMiceEventID());    
                }
            }
        }
        public virtual async UniTask OnKioskRefreshToken(MiceKioskWebViewMessage kioskMessage, GUIView guiView)
        {
            await UniTask.CompletedTask;
        }
        public virtual async UniTask OnKioskClosePage(MiceKioskWebViewMessage kioskMessage, GUIView guiView)
        {
            guiView.Hide();
            await UniTask.CompletedTask;
        }
        public virtual void OnTeleportCompletion()
        {
        }

        public virtual bool CheckReservedObject(Protocols.ObjectState objState)
        {
            return false;
        }
    }
}
