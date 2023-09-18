// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	ServiceManager.cs
//  * Developer:	yangsehoon
//  * Date:		2023-04-27 오후 3:22
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.Network.GameLogic;
using Protocols.CommonLogic;
using Protocols.GameLogic;

namespace Com2Verse.Network
{
	public class ServiceManager : Singleton<ServiceManager>, IDisposable
	{
		public event Action<ServiceChangeNotify> ServiceChangeEarlyNotify;
		
		private ServiceManager() { }

		public void Initialize()
		{
			Com2Verse.Network.CommonLogic.PacketReceiver.Instance.ServiceChangeNotify += OnServiceNotify;
			PacketReceiver.Instance.ServiceChangeResponse                             += OnServiceChange;
			PacketReceiver.Instance.LeaveBuildingResponse                             += OnLeaveBuilding;
		}

		public void OnServiceNotify(ServiceChangeNotify response)
		{
			C2VDebug.Log($"OnServiceNotify to {response.ServiceId}");
			ServiceChangeEarlyNotify?.Invoke(response);
			Protocols.DestinationLogicalAddress.SetServerID(Protocols.ServerType.Logic, response.ServiceId);
			User.Instance.ChangeUserData(response.ServiceId);
		}

		public void OnServiceChange(ServiceChangeResponse response)
		{
			Protocols.DestinationLogicalAddress.SetServerID(Protocols.ServerType.Logic, response.ServiceId);
			User.Instance.ChangeUserData(response.ServiceId);
			Commander.Instance.RequestEnterBuilding(response.BuildingId);
		}

		public void OnLeaveBuilding(LeaveBuildingResponse response)
		{
			Protocols.DestinationLogicalAddress.SetServerID(Protocols.ServerType.Logic, (long)eServiceID.WORLD);
			User.Instance.ChangeUserData((long)eServiceID.WORLD);
			Commander.Instance.RequestBuildingOut();
		}

		public void Dispose()
		{
			if (Com2Verse.Network.CommonLogic.PacketReceiver.InstanceExists)
				Com2Verse.Network.CommonLogic.PacketReceiver.Instance.ServiceChangeNotify -= OnServiceNotify;
			if (PacketReceiver.InstanceExists)
			{
				PacketReceiver.Instance.ServiceChangeResponse -= OnServiceChange;
				PacketReceiver.Instance.LeaveBuildingResponse -= OnLeaveBuilding;
			}

			ServiceChangeEarlyNotify = null;
		}
	}
}
