// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	Commander_ServiceChange.cs
//  * Developer:	yangsehoon
//  * Date:		2023-04-27 오후 1:01
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using Com2Verse.PlayerControl;
using Com2Verse.UI;
using Protocols.GameLogic;

namespace Com2Verse.Network
{
	public partial class Commander
	{
		public void RequestServiceChange(long buildingId)
		{
			ServiceChangeRequest serviceChangeRequest = new()
			{
				BuildingId = buildingId
			};
			LogPacketSend(serviceChangeRequest.ToString());
			NetworkManager.Instance.Send(serviceChangeRequest,
			                             MessageTypes.ServiceChangeRequest,
			                             Protocols.Channels.GameLogic,
			                             (int)MessageTypes.ServiceChangeResponse,
			                             timeoutAction: () =>
			                             {
				                             PlayerController.Instance.SetStopAndCannotMove(false);
				                             User.Instance.RestoreStandBy();
				                             UIManager.Instance.SendToastMessage(Localization.Instance.GetErrorString((int)Protocols.ErrorCode.DbError), toastMessageType: UIManager.eToastMessageType.WARNING);
			                             });
		}

		public void RequestEnterBuilding(long buildingId)
		{
			EnterBuildingRequest enterBuildingRequest = new()
			{
				BuildingId = buildingId
			};
			LogPacketSend(enterBuildingRequest.ToString());
			NetworkManager.Instance.Send(enterBuildingRequest,
			                             MessageTypes.EnterBuildingRequest,
			                             Protocols.Channels.WorldState,
			                             (int)Protocols.WorldState.MessageTypes.TeleportUserStartNotify,
			                             timeoutAction: () =>
			                             {
				                             PlayerController.Instance.SetStopAndCannotMove(false);
				                             User.Instance.RestoreStandBy();
				                             // 서비스만 전환이 되고 텔레포트가 안된 경우 서비스전환을 롤백
				                             Protocols.DestinationLogicalAddress.SetServerID(Protocols.ServerType.Logic, User.Instance.PrevServiceType);
				                             User.Instance.ChangeUserData(User.Instance.PrevServiceType);
				                             UIManager.Instance.SendToastMessage(Localization.Instance.GetErrorString((int)Protocols.ErrorCode.DbError), toastMessageType: UIManager.eToastMessageType.WARNING);
			                             });
		}

		public void RequestBuildingOut()
		{
			ExitBuildingRequest exitBuildingRequest = new();
			LogPacketSend(exitBuildingRequest.ToString());
			NetworkManager.Instance.Send(exitBuildingRequest,
			                             MessageTypes.ExitBuildingRequest,
			                             Protocols.Channels.WorldState,
			                             (int)Protocols.WorldState.MessageTypes.TeleportUserStartNotify,
			                             timeoutAction: () =>
			                             {
				                             PlayerController.Instance.SetStopAndCannotMove(false);
				                             User.Instance.RestoreStandBy();
				                             // 서비스만 전환이 되고 텔레포트가 안된 경우 서비스전환을 롤백
				                             Protocols.DestinationLogicalAddress.SetServerID(Protocols.ServerType.Logic, User.Instance.PrevServiceType);
				                             User.Instance.ChangeUserData(User.Instance.PrevServiceType);
				                             UIManager.Instance.SendToastMessage(Localization.Instance.GetErrorString((int)Protocols.ErrorCode.DbError), toastMessageType: UIManager.eToastMessageType.WARNING);
			                             });
		}

		public void LeaveBuildingRequest()
		{
			LeaveBuildingRequest leaveBuildingRequest = new() {};
			LogPacketSend(leaveBuildingRequest.ToString());
			NetworkManager.Instance.Send(leaveBuildingRequest,
			                             MessageTypes.LeaveBuildingRequest,
			                             Protocols.Channels.GameLogic,
			                             (int)MessageTypes.LeaveBuildingResponse,
			                             timeoutAction: () =>
			                             {
				                             PlayerController.Instance.SetStopAndCannotMove(false);
				                             User.Instance.RestoreStandBy();
				                             UIManager.Instance.SendToastMessage(Localization.Instance.GetErrorString((int)Protocols.ErrorCode.DbError), toastMessageType: UIManager.eToastMessageType.WARNING);
			                             });
		}
	}
}
