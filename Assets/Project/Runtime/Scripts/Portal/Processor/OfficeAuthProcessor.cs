// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	OfficeAuthProcessor.cs
//  * Developer:	yangsehoon
//  * Date:		2023-04-27 오후 12:08
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System.Net;
using Com2Verse.Chat;
using Com2Verse.Data;
using Com2Verse.EventTrigger;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.Organization;
using Com2Verse.PlayerControl;
using Com2Verse.UI;
using Com2Verse.WebApi.Service;
using Cysharp.Threading.Tasks;
using Protocols;
using Protocols.GameLogic;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.OFFICE_AUTH)]
	public class OfficeAuthProcessor : BaseLogicTypeProcessor
	{
		public override void OnTriggerEnter(TriggerInEventParameter triggerInParameter)
		{
			base.OnTriggerEnter(triggerInParameter);
			C2VDebug.LogCategory("OfficeAuthProcessor", $"{nameof(OfficeAuthProcessor)} Trigger Enter");
			LoginManager.Instance.RequestServiceLogin((result) =>
			{
				if (result) CheckBeforeTeleportAsync().Forget();
			});
		}

		private async UniTaskVoid CheckBeforeTeleportAsync()
		{
			var myself = await DataManager.Instance.GetMyselfAsync();
			if (myself == null)
			{
				C2VDebug.LogErrorCategory("OfficeAuthProcessor", $"Can't Find My MemberModel");
				NetworkUIManager.Instance.ShowCommonErrorMessage();
			}
			else if (myself.Member == null)
			{
				C2VDebug.LogErrorCategory("OfficeAuthProcessor", $"My Member is NULL");
				NetworkUIManager.Instance.ShowCommonErrorMessage();
			}
			// else if (myself.Member.TeamId <= 0)
			// {
			// 	C2VDebug.LogErrorCategory("OfficeAuthProcessor", $"My TeamId is Invalid");
			// 	NetworkUIManager.Instance.ShowCommonErrorMessage();
			// }
			else
			{
				C2VDebug.LogCategory("OfficeAuthProcessor", $"My TeamId : {myself.Member.TeamId}");
				var teamId = myself.Member.TeamId;
				var myTeam = DataManager.Instance.GetTeam(teamId);
				if (myTeam == null)
				{
					C2VDebug.LogErrorCategory("OfficeAuthProcessor", $"TeamModel is NULL");
					NetworkUIManager.Instance.ShowCommonErrorMessage();
				}
				else if (myTeam.Info == null)
				{
					C2VDebug.LogErrorCategory("OfficeAuthProcessor", $"TeamModel Info is NULL");
					NetworkUIManager.Instance.ShowCommonErrorMessage();
				}
				else if (string.IsNullOrEmpty(myTeam.Info.SpaceId))
				{
					C2VDebug.LogErrorCategory("OfficeAuthProcessor", $"TeamModel SpaceId is NULL");
					NetworkUIManager.Instance.ShowCommonErrorMessage();
				}
				else
				{
					C2VDebug.LogCategory("OfficeAuthProcessor", $"Teleport to {myTeam.Info.SpaceId}");
					TeleportMyTeamSpaceAsync().Forget();
				}
			}
		}

		private async UniTaskVoid TeleportMyTeamSpaceAsync()
		{
			if (!DataManager.Instance.IsReady) return;

			// 이동중 OSR/Input 막기
			PlayerController.Instance.SetStopAndCannotMove(true);
			User.Instance.DiscardPacketBeforeStandBy();

			var request = new Components.WarpGroupMyTeamSpaceRequest()
			{
				CurrentServiceId = (long)eServiceID.SPAXE
			};

			var response = await Api.Organization.PostWarpGroupMyTeamSpace(request);
			if (response == null)
			{
				C2VDebug.LogErrorCategory("OfficeAuthProcessor", $"WarpGroupMyTeamSpaceResponse is NULL");
				ErrorInvoke(0);
			}
			else if (response.StatusCode == HttpStatusCode.OK)
			{
				if (response.Value == null)
				{
					C2VDebug.LogErrorCategory("OfficeAuthProcessor", $"WarpGroupMyTeamSpaceResponse Value is NULL");
					ErrorInvoke(0);
				}
				else
				{
					switch (response.Value.Code)
					{
						case Components.OfficeHttpResultCode.Success:
							C2VDebug.LogCategory("OfficeAuthProcessor", $"WarpGroupMyTeamSpace Success");
							ChatManager.Instance.SetAreaMove(response.Value.Data?.ChatGroupId);
							break;
						default:
							C2VDebug.LogErrorCategory("OfficeAuthProcessor", $"WarpGroupMyTeamSpace Error : {response.Value.Code.ToString()}");
							ErrorInvoke(response.Value.Code);
							break;
					}
				}
			}
			else
			{
				C2VDebug.LogErrorCategory("OfficeAuthProcessor", $"WarpGroupMyTeamSpace Fail : {response.StatusCode.ToString()}");
				ErrorInvoke(Components.OfficeHttpResultCode.Fail);
			}

			void ErrorInvoke(Components.OfficeHttpResultCode code)
			{
				NetworkUIManager.Instance.ShowWebApiErrorMessage(code);
				PlayerController.Instance.SetStopAndCannotMove(false);
				User.Instance.RestoreStandBy();
			}
		}
	}
}
