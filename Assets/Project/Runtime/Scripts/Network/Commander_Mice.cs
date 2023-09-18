/*===============================================================
* Product:		Com2Verse
* File Name:	Commander_Mice.cs
* Developer:	ikyoung
* Date:			2023-03-29 19:29
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Avatar;
using Com2Verse.Logger;
using Protocols.Mice;
using Protocols.ObjectCondition;

namespace Com2Verse.Network
{
    public sealed partial class Commander
    {
        public void RequestEnterMiceLobby()
        {
            Protocols.Mice.EnterLobbyRequest enterMiceLobbyRequest = new()
            {
                AccountId = User.Instance.CurrentUserData.ID
            };

            C2VDebug.Log("Sending Request EnterMiceLobby");
            NetworkManager.Instance.Send(enterMiceLobbyRequest, Protocols.Mice.MessageTypes.EnterLobbyRequest);
        }

        public void RequestEnterMiceLounge(long eventID)
        {
            Protocols.Mice.EnterLoungeRequest enterMiceLoungeRequest = new()
            {
                AccountId = User.Instance.CurrentUserData.ID,
                EventId   = eventID
            };

            C2VDebug.Log("Sending Request RequestEnterMiceLounge");
            NetworkManager.Instance.Send(enterMiceLoungeRequest, Protocols.Mice.MessageTypes.EnterLoungeRequest);
        }

        public void RequestEnterMiceFreeLounge(long eventID)
        {
            Protocols.Mice.EnterFreeLoungeRequest enterMiceFreeLoungeRequest = new()
            {
                AccountId = User.Instance.CurrentUserData.ID,
                EventId   = eventID
            };

            C2VDebug.Log("Sending Request RequestEnterMiceFreeLounge");
            NetworkManager.Instance.Send(enterMiceFreeLoungeRequest, Protocols.Mice.MessageTypes.EnterFreeLoungeRequest);
        }

        public void RequestEnterMiceHall(long sessionId)
        {
            Protocols.Mice.EnterHallRequest enterMiceHallRequest = new()
            {
                SessionId = sessionId,
            };

            C2VDebug.Log("Sending Request RequestEnterMiceHall");
            NetworkManager.Instance.Send(enterMiceHallRequest, Protocols.Mice.MessageTypes.EnterHallRequest);
        }
        
        public void RequestMiceRoomNotify(Protocols.Mice.RequestNotifyType requestNotifyType, MiceType miceType, long roomID)
        {
            Protocols.Mice.RequestMiceRoomNotify request = new()
            {
                RequestNotifyType = requestNotifyType,
                MiceType = miceType,
                RoomId = roomID
            };
        
            C2VDebug.Log("Sending Request RequestMiceRoomNotify");
            NetworkManager.Instance.Send(request, Protocols.Mice.MessageTypes.RequestMiceRoomNotify);
        }
        
        public void RequestServiceExit()
        {
            Protocols.Mice.ServiceExitNotify request = new()
            {
            };
        
            C2VDebug.Log("Sending Request ServiceExitNotify");
            NetworkManager.Instance.Send(request, Protocols.Mice.MessageTypes.ServiceExitNotify);
        }
    }
}