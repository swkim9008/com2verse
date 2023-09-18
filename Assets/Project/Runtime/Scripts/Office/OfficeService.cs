/*===============================================================
* Product:		Com2Verse
* File Name:	OfficeService.cs
* Developer:	jhkim
* Date:			2023-06-19 20:43
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.Network;
using Protocols.GameLogic;
using UnityEngine;

namespace Com2Verse.Office
{
	public sealed class OfficeService : Singleton<OfficeService>, IDisposable
	{
#region Variables
		private static readonly long Com2UsBuildingId = 2;
		private static readonly long ShareOfficeBuildingId = 3;
		private static readonly long OfficeServiceId = (long)eServiceID.SPAXE;
		private static readonly long OpenOfficeServiceId = (long)eServiceID.SPAXE;

		private bool _inOffice;
#endregion // Variables

#region Properties
		public long BuildingId { get; private set; } = -1;
		private long ServiceId { get; set; }
		public bool InOffice => InCom2UsOffice || InShareOffice;
		public bool InCom2UsOffice => BuildingId == Com2UsBuildingId;
		public bool InShareOffice => BuildingId == ShareOfficeBuildingId;
		public bool IsSameBuilding(long buildingId) => BuildingId == buildingId;
		public bool IsTeamRoom => CurrentScene.SpaceCode is eSpaceCode.TEAM;
		public bool IsModelHouse => CurrentScene.SpaceCode is eSpaceCode.MODEL_HOUSE;
#endregion // Properties

#region Initialize
		private OfficeService() { }

		public void Initialize()
		{
			_inOffice = false;
			RegisterListener();
		}
#endregion // Initialize

#region Event Listener
		private void RegisterListener()
		{
			Network.GameLogic.PacketReceiver.Instance.ServiceChangeResponse += OnServiceChangeResponse;
			Network.GameLogic.PacketReceiver.Instance.LeaveBuildingResponse += OnLeaveBuildingResponse;
		}

		private void UnregisterListener()
		{
			if (!Network.GameLogic.PacketReceiver.InstanceExists) return;
			Network.GameLogic.PacketReceiver.Instance.ServiceChangeResponse -= OnServiceChangeResponse;
			Network.GameLogic.PacketReceiver.Instance.LeaveBuildingResponse -= OnLeaveBuildingResponse;
		}
		private void OnServiceChangeResponse(ServiceChangeResponse response)
		{
			BuildingId = response.BuildingId;
			ServiceId = response.ServiceId;

			_inOffice = IsOffice(response);
			if (_inOffice)
			{
				LoginManager.Instance.RequestOrganizationChart();
			}
		}

		private void OnLeaveBuildingResponse(LeaveBuildingResponse response)
		{
			if (_inOffice)
			{
				// TODO : 조직도 데이터 사용중일 때 예외처리 추가
				Organization.DataManager.DisposeOrganization();
				if (User.Instance.CurrentUserData is not OfficeUserData userData) return;
				userData.EmployeeID = string.Empty;
			}
			_inOffice = false;
		}
#endregion // Event Listener

		private bool IsOffice(ServiceChangeResponse response) => IsCom2UsOffice(response.BuildingId, response.ServiceId) || IsOpenOffice(response.BuildingId, response.ServiceId);
		private bool IsCom2UsOffice(long buildingId, long serviceId) => buildingId == Com2UsBuildingId && serviceId == OfficeServiceId;
		private bool IsOpenOffice(long buildingId, long serviceId) => buildingId == ShareOfficeBuildingId && serviceId == OpenOfficeServiceId;
#region Dispose
		public void Dispose()
		{
			UnregisterListener();
		}
#endregion // Dispose

#region Editor
#if UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void Reset()
		{
			var instance = InstanceOrNull;
			if (instance != null)
				instance._inOffice = false;
			DestroySingletonInstance();
		}
#endif // UNITY_EDITOR
#endregion // Editor
	}
}
