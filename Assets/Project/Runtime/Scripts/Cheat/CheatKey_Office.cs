/*===============================================================
* Product:		Com2Verse
* File Name:	CheatKey_Office.cs
* Developer:	jhkim
* Date:			2023-05-13 16:26
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Com2Verse.Data;
using Com2Verse.HttpHelper;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.Organization;
using Com2Verse.UI;
using Com2Verse.WebApi.Service;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Protocols;
using UnityEngine;
using Client = Com2Verse.HttpHelper.Client;
using Localization = Com2Verse.UI.Localization;
using Util = Com2Verse.HttpHelper.Util;

namespace Com2Verse.Cheat
{
	public static partial class CheatKey
	{
		[MetaverseCheat("Cheat/Office/Space/1. ServiceChangeRequest")]
		public static void ServiceChangeRequest(string buildingIdStr = "2")
		{
			if (!long.TryParse(buildingIdStr, out var buildingId))
			{
				C2VDebug.LogWarning($"Invalid Building Id = {buildingIdStr}");
				return;
			}

			Network.GameLogic.PacketReceiver.Instance.ServiceChangeResponse += OnServiceChangeResponse;
			Commander.Instance.RequestServiceChange(buildingId);
		}

		[MetaverseCheat("Cheat/Office/Space/2. EnterBuildingRequest")]
		private static void EnterBuildingRequest(string buildingIdStr = "2")
		{
			if (!long.TryParse(buildingIdStr, out var buildingId))
			{
				C2VDebug.LogWarning($"Invalid Building Id = {buildingIdStr}");
				return;
			}

			Protocols.DestinationLogicalAddress.SetServerID(ServerType.Logic, (long)eServiceID.SPAXE);
			User.Instance.ChangeUserData((long)eServiceID.SPAXE);
			PacketReceiver.Instance.OnTeleportUserStartNotifyEvent += OnTeleportUserStartNotify;
			Commander.Instance.RequestEnterBuilding(buildingId);
		}

		[MetaverseCheat("Cheat/Office/Space/3. TeleportSpace")]
		public static void TeleportSpace(string spaceId = "f-0100")
		{
			Commander.Instance.TeleportUserSpaceRequest(spaceId);
		}
		private static void OnServiceChangeResponse(Protocols.GameLogic.ServiceChangeResponse response)
		{
			C2VDebug.Log($"SERVICE CHANGE RESPONSE = {response}");
			Network.GameLogic.PacketReceiver.Instance.ServiceChangeResponse -= OnServiceChangeResponse;
		}
		private static void OnTeleportUserStartNotify(Protocols.WorldState.TeleportUserStartNotify notify)
		{
			C2VDebug.Log($"TELEPORT USER START NOTIFY = {notify}");
			PacketReceiver.Instance.OnTeleportUserStartNotifyEvent -= OnTeleportUserStartNotify;
		}

		[MetaverseCheat("Cheat/Office/Get Unidentified Member List")][HelpText("Organization Group ID")]
		private static async UniTaskVoid PrintUnidentifiedMemberAsync(int groupId = -1)
		{
			var unidentifiedMembers = new List<(string, string)>();
			if (groupId != -1)
			{
				if (DataManager.Instance.IsReady)
					DataManager.DisposeOrganization();
				await DataManager.SendOrganizationChartRequestAsync(groupId);
			}
			if (DataManager.Instance.IsReady)
			{
				var groupModel = DataManager.Instance.GetGroupModel();

				foreach (var team in groupModel.Group.Teams)
					CollectUnIdentifiedMembers(team);
			}
			else
			{
				UIManager.Instance.SendToastMessage("조직도가 준비되지 않았습니다.", toastMessageType: UIManager.eToastMessageType.WARNING);
				return;
			}

			var sb = new StringBuilder();
			foreach (var (key, name) in unidentifiedMembers)
			{
				var msg = $"KEY = {key}, NAME = {name}";
				sb.AppendLine(msg);
				C2VDebug.Log(msg);
			}

			GUIUtility.systemCopyBuffer = sb.ToString();
			UIManager.Instance.SendToastMessage($"클립보드로 복사되었습니다. ({unidentifiedMembers.Count})");

			void CollectUnIdentifiedMembers(Components.Team team)
			{
				unidentifiedMembers.AddRange(from member in team.Members where member.AccountId == 0 select (member.IdentifyKey, member.MemberName));

				foreach (var teamSubTeam in team.SubTeams)
					CollectUnIdentifiedMembers(teamSubTeam);
			}
		}
#region Organization
		private static CancellationTokenSource _tcs;

		[MetaverseCheat("Cheat/Organization/HierarkeyTest Toggle")] [HelpText("조직도 항목 선택 테스트용 치트")]
		private static async void ToggleHierarchyTest()
		{
			var viewModel = ViewModelManager.Instance.Get<OrganizationHierarchyViewModel>();
			if (viewModel == null)
			{
				C2VDebug.LogWarning("조직도 팝업을 연 상태에서 실행 해 주세요");
				return;
			}

			if (_tcs != null)
				_tcs.Cancel();
			else
				await HierarchyTestAsync();
		}

		static async UniTask HierarchyTestAsync()
		{
			int  idx       = 0;
			bool reverse   = false;
			var  viewModel = ViewModelManager.Instance.Get<OrganizationHierarchyViewModel>();
			_tcs = new CancellationTokenSource();
			while (!_tcs.IsCancellationRequested)
			{
				viewModel.Pick(idx);
				await UniTask.Delay(250);
				if (reverse)
				{
					if (idx - 1 < 0)
						reverse = !reverse;
					else
						idx--;
				}
				else
				{
					if (idx + 1 >= viewModel.HierarchyItemCount)
						reverse = !reverse;
					else
						idx++;
				}
			}

			_tcs = null;
		}

		[MetaverseCheat(("Cheat/Organization/Network/Send Request"))]
		private static void OrganizationNetworkRequest()
		{
			if (DataManager.Instance.IsReady)
				Organization.DataManager.SendOrganizationChartRequest(DataManager.Instance.GroupID);
		}

		[MetaverseCheat("Cheat/Organization/Network/Dispose Organization")]
		private static void OrganizationDispose()
		{
			Organization.DataManager.DisposeOrganization();
		}

		[MetaverseCheat("Cheat/Organization/Show DataView")]
		private static void OrganizationShowDataView()
		{
			OrganizationDataViewModel.ShowView();
		}

		[MetaverseCheat("Cheat/Organization/Show Hierarchy Tree")]
		private static void OrganizationShowHierarchyTree()
		{
			OrganizationHierarchyViewModel.ShowView(OrganizationHierarchyViewModel.ePopupType.NONE, OrganizationHierarchyViewModel.HierarchyViewInfo.Empty);
		}
#endregion // Organization

#region Office ErrorString
		[MetaverseCheat("Cheat/Office/ErrorString/Show")]
		private static void ShowOfficeError(string codeStr)
		{
			if (!int.TryParse(codeStr, out var code)) return;

			var msg = Localization.Instance.GetOfficeErrorString(code);
			UIManager.Instance.SendToastMessage(msg);
		}
#endregion // Office ErrorString

#region Organization Refresh Cheat
		[MetaverseCheat("Cheat/Organization/CheckCoolTime")]
		private static async void CheckCoolTimeCheat()
		{
			var success = await DataManager.TryOrganizationRefreshAsync(eOrganizationRefreshType.CONNECTING_APP, 1);
			C2VDebug.Log($"Check CoolTime Request SUCCESS = {success}");
		}
#endregion // Organization Refresh Cheat
	}
}
