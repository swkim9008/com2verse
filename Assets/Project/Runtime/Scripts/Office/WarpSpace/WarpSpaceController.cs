/*===============================================================
* Product:		Com2Verse
* File Name:	WarpSpaceController.cs
* Developer:	jhkim
* Date:			2023-06-30 14:38
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Com2Verse.Chat;
using Com2Verse.HttpHelper;
using Com2Verse.InputSystem;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.Organization;
using Com2Verse.PlayerControl;
using Com2Verse.UI;
using Com2Verse.WebApi.Service;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Com2Verse.Office.WarpSpace
{
	public sealed class WarpSpaceController
	{
		private static readonly string LogCategory = "WarpSpace";
		private struct StringKey
		{
			[NotNull] public static readonly string SpaceName = "UI_Space_Move_Menu_Title1";
			[NotNull] public static readonly string RestName = "UI_Space_Move_Menu_Title2";
			[NotNull] public static readonly string LobbyName = "UI_Space_Move_Menu_Title3";
			[NotNull] public static readonly string RestAreaButton = "UI_Space_Move_Menu_Btn1";
			[NotNull] public static readonly string LobbyButton = "UI_Space_Move_Menu_Btn2";
		}

		private static readonly int SpaceItemsPerGroup = 6;
		private WarpSpaceModel _model;
		public WarpSpaceGroupModel[] Groups => _model.Groups ?? Array.Empty<WarpSpaceGroupModel>();
		public string GroupName => _model.GroupName;
		public WarpSpaceController()
		{
			_model = WarpSpaceModel.Empty;
		}
		public async UniTask<bool> LoadAsync(bool useDummy = false)
		{
			if (!DataManager.Instance.IsReady && !useDummy) return false;

			var warpSpaceGroups = new List<WarpSpaceGroupModel>();
			var warpSpaceItems = new List<WarpSpaceItemModel>();

			var workLabel = Localization.Instance.GetString(StringKey.SpaceName);
			var restLabel = Localization.Instance.GetString(StringKey.RestName);
			var lobbyLabel = Localization.Instance.GetString(StringKey.LobbyName);
			var restAreaButtonFormat = Localization.Instance.GetString(StringKey.RestAreaButton);
			var lobbyButton = Localization.Instance.GetString(StringKey.LobbyButton);

			var pid = 0;
			var request = new Components.GetActivatedTeamSpaceInfoRequest
			{
				GroupId = DataManager.Instance.GroupID,
			};

			ResponseBase<Components.GetActivatedTeamSpaceInfoResponseResponseFormat> response = null;

			if (useDummy)
				response = LoadDummyData("warpspace.json");

			response ??= await Com2Verse.WebApi.Service.Api.Organization.PostGetActivatedTeamSpaceInfo(request);

			if (response?.StatusCode == HttpStatusCode.OK)
			{
				if (response.Value.Code == Components.OfficeHttpResultCode.Success)
				{
					foreach (var activatedSpaceTeamInfo in response.Value.Data.Teams)
					{
						AddWorkSpace(new Components.Team
						{
							TeamName = activatedSpaceTeamInfo.TeamName,
							SpaceId = activatedSpaceTeamInfo.SpaceId,
							TeamId = activatedSpaceTeamInfo.TeamId,
							SubTeams = Array.Empty<Components.Team>(),
						});
					}
					AddNewGroup(workLabel);

					var restSpaces = response.Value.Data.RestSpaces;
					if (restSpaces != null)
					{
						for (var i = 0; i < restSpaces.Length; i++)
							AddRestSpace(string.Format(restAreaButtonFormat, Convert.ToString(i + 1)), restSpaces[i]?.SpaceId);

						AddNewGroup(restLabel);
					}
				}
				else
				{
					NetworkUIManager.Instance.ShowWebApiErrorMessage(response.Value.Code);
					return false;
				}
			}
			else
			{
				NetworkUIManager.Instance.ShowWebApiErrorMessage(Components.OfficeHttpResultCode.Fail);
				return false;
			}

			AddLobbyGroup();

			_model = new WarpSpaceModel
			{
				GroupName = response.Value.Data.GroupName,
				Groups = warpSpaceGroups.ToArray(),
			};

			return true;

#region Group
			void AddNewGroup(string label)
			{
				if (warpSpaceItems.Count == 0) return;

				warpSpaceGroups.Add(new WarpSpaceGroupModel
				{
					Label = label,
					Items = warpSpaceItems.ToArray(),
				});
				warpSpaceItems.Clear();
			}

			void AddLobbyGroup()
			{
				warpSpaceItems.Clear();

				AddLobbySpace();
				AddNewGroup(lobbyLabel);
			}
#endregion // Group

#region Space
			void AddWorkSpace(Components.Team team)
			{
				if (team == null) return;

				AddSpaceItem(new WarpSpaceItemModel
				{
					Pid = pid++,
					Type = WarpSpaceItemModel.eType.WORK,
					State = WarpSpaceItemModel.eState.DESELECTED,
					Label = team.TeamName,
					GroupID = DataManager.Instance.GroupID,
					TeamID = team.TeamId,
					SpaceID = team.SpaceId,
				});

				if (warpSpaceItems.Count == SpaceItemsPerGroup)
					AddNewGroup(workLabel);

				if (team.SubTeams != null)
				{
					foreach (var subTeam in team.SubTeams)
						AddWorkSpace(subTeam);
				}
			}

			void AddRestSpace(string label, string spaceId)
			{
				if (string.IsNullOrWhiteSpace(spaceId)) return;

				AddSpaceItem(new WarpSpaceItemModel
				{
					Pid = pid++,
					Type = WarpSpaceItemModel.eType.REST,
					State = WarpSpaceItemModel.eState.DESELECTED,
					Label = label,
					GroupID = DataManager.Instance.GroupID,
					SpaceID = spaceId,
				});
			}

			void AddLobbySpace()
			{
				AddSpaceItem(new WarpSpaceItemModel
				{
					Pid = pid++,
					Type = WarpSpaceItemModel.eType.LOBBY,
					State = WarpSpaceItemModel.eState.DESELECTED,
					Label = lobbyButton,
				});
			}
			void AddSpaceItem(WarpSpaceItemModel model)
			{
				if (!IsValidModel()) return;

				warpSpaceItems.Add(model);

				bool IsValidModel()
				{
					switch (model.Type)
					{
						case WarpSpaceItemModel.eType.WORK:
						case WarpSpaceItemModel.eType.REST:
							return !string.IsNullOrWhiteSpace(model.SpaceID);
						case WarpSpaceItemModel.eType.LOBBY:
							return true;
						default:
							return false;
					}
				}
			}
#endregion // Space

#region Cheat
			ResponseBase<Components.GetActivatedTeamSpaceInfoResponseResponseFormat> LoadDummyData(string dummyFile)
			{
				if (!File.Exists(dummyFile)) return null;

				var jsonText = File.ReadAllText(dummyFile);
				var responseObj = JsonConvert.DeserializeObject<Components.GetActivatedTeamSpaceInfoResponseResponseFormat>(jsonText);

				return new ResponseBase<Components.GetActivatedTeamSpaceInfoResponseResponseFormat>(responseObj, null)
				{
					StatusCode = HttpStatusCode.OK,
				};
			}
#endregion // Cheat
		}

#region Warp
		public async UniTask WarpToAsync(int selectedPid)
		{
			var item = GetItemControllerByPid(selectedPid);
			if (!item.HasValue) return;

			// 이동중 OSR/Input 막기
			PlayerController.Instance.SetStopAndCannotMove(true);
			User.Instance.DiscardPacketBeforeStandBy();

			var model = item.Value;
			switch (model.Type)
			{
				case WarpSpaceItemModel.eType.WORK:
					await WarpToWorkAsync(model.GroupID, model.TeamID);
					break;
				case WarpSpaceItemModel.eType.REST:
					await WarpToRestAsync(model.GroupID, model.SpaceID);
					break;
				case WarpSpaceItemModel.eType.LOBBY:
					await WarpToLobbyAsync();
					break;
				default:
					break;
			}
		}

		private void RestoreMoveInput()
		{
			PlayerController.Instance.SetStopAndCannotMove(false);
			User.Instance.RestoreStandBy();
		}

		private async UniTask WarpToWorkAsync(long groupId, long teamId)
		{
			var request = new Components.MoveGroupTeamSpaceRequest
			{
				GroupId = groupId,
				TeamId = teamId,
			};
			var response = await Api.Organization.PostWarpGroupTeamSpace(request);
			if (response.StatusCode == HttpStatusCode.OK)
			{
				if (response.Value.Code == Components.OfficeHttpResultCode.Success)
					ChatManager.Instance.SetAreaMove(response.Value.Data.ChatGroupId);
				else
				{
					C2VDebug.LogWarningCategory(LogCategory, $"WarpToWorkAsync failed. {response.Value.Code} = {response.Value.Msg}");
					NetworkUIManager.Instance.ShowWebApiErrorMessage(response.Value.Code);
					RestoreMoveInput();
				}
			}
			else
			{
				C2VDebug.LogWarningCategory(LogCategory, $"WarpToWorkAsync HTTP failed. {response.StatusCode}");
				RestoreMoveInput();
			}
		}

		private async UniTask WarpToRestAsync(long groupId, string spaceId)
		{
			var request = new Components.WarpGroupRestSpaceRequest
			{
				GroupId = groupId,
				RestSpaceId = spaceId,
			};
			var response = await Api.Organization.PostWarpGroupRestSpace(request);
			if (response.StatusCode == HttpStatusCode.OK)
			{
				if (response.Value.Code == Components.OfficeHttpResultCode.Success)
					ChatManager.Instance.SetAreaMove(response.Value.Data.ChatGroupId);
				else
				{
					C2VDebug.LogWarningCategory(LogCategory, $"WarpToRestAsync failed. {response.Value.Code} = {response.Value.Msg}");
					NetworkUIManager.Instance.ShowWebApiErrorMessage(response.Value.Code);
					RestoreMoveInput();
				}
			}
			else
			{
				C2VDebug.LogWarningCategory(LogCategory, $"WarpToRestAsync HTTP failed. {response.StatusCode}");
				RestoreMoveInput();
			}
		}

		private async UniTask WarpToLobbyAsync()
		{
			var request = new Components.WarpBuildingRepresentSpaceRequest
			{
				BuildingId = OfficeService.Instance.BuildingId,
			};
			var response = await Api.Organization.PostWarpBuildingRepresentSpace(request);
			if (response.StatusCode == HttpStatusCode.OK)
			{
				if (response.Value.Code == Components.OfficeHttpResultCode.Success)
					ChatManager.Instance.SetAreaMove(response.Value.Data.ChatGroupId);
				else
				{
					C2VDebug.LogWarningCategory(LogCategory, $"WarpToLobbyAsync failed. {response.Value.Code} = {response.Value.Msg}");
					NetworkUIManager.Instance.ShowWebApiErrorMessage(response.Value.Code);
					RestoreMoveInput();
				}
			}
			else
			{
				C2VDebug.LogWarningCategory(LogCategory, $"WarpToLobbyAsync failed. {response.StatusCode}");
				RestoreMoveInput();
			}
		}
#endregion // Warp

		private WarpSpaceItemModel? GetItemControllerByPid(int pid)
		{
			foreach (var group in Groups)
			{
				var findIdx = Array.FindIndex(group.Items, item => item.Pid == pid);
				if (findIdx != -1)
					return group.Items[findIdx];
			}

			return null;
		}
	}
#region Model
	public struct WarpSpaceModel
	{
		public string GroupName;
		public WarpSpaceGroupModel[] Groups;

		public static WarpSpaceModel Empty = new WarpSpaceModel
		{
			GroupName = string.Empty,
			Groups = Array.Empty<WarpSpaceGroupModel>(),
		};
	}
#endregion // Model
}
