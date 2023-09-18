/*===============================================================
* Product:		Com2Verse
* File Name:	Commander_Connection.cs
* Developer:	haminjeong
* Date:			2022-05-16 15:11
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Avatar;
using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.UI;
using JetBrains.Annotations;

namespace Com2Verse.Network
{
	public sealed partial class Commander
	{
#region Connection
		public void Com2VerseLogin()
		{
			Protocols.GameLogic.LoginCom2verseRequest loginRequest = new()
			{
				Did = LoginManager.Instance.DeviceId,
				C2VAccessToken = User.Instance.CurrentUserData.AccessToken
			};

			C2VDebug.Log($"Sending LoginCom2verseRequest Packet");
			NetworkManager.Instance.ResendMessage = Com2VerseLogin;
			NetworkManager.Instance.Send(loginRequest, Protocols.GameLogic.MessageTypes.LoginCom2VerseRequest,
			                             Protocols.Channels.GameLogic, (int)Protocols.GameLogic.MessageTypes.LoginCom2VerseResponse,
			                             timeoutAction: () =>
			                             {
				                             User.Instance.DefaultTimeoutProcess();
			                             });
		}

		public void ServiceLogin()
		{
			Protocols.GameLogic.LoginOfficeRequest loginRequest = new ()
			{
				Did            = LoginManager.Instance.DeviceId,
				ServiceId      = User.Instance.CurrentUserData.UserName,
				C2VAccessToken = User.Instance.CurrentUserData.AccessToken
			};

			C2VDebug.Log("Sending LoginOfficeRequest Packet");
			NetworkManager.Instance.ResendMessage = ServiceLogin;
			NetworkManager.Instance.Send(loginRequest, Protocols.OfficeMessenger.MessageTypes.LoginOfficeRequest,
			                             Protocols.Channels.OfficeMessenger, (int)Protocols.OfficeMessenger.MessageTypes.LoginOfficeResponse,
			                             timeoutAction: () =>
			                             {
				                             User.Instance.DefaultTimeoutProcess();
			                             });
		}
		public void UsePortalRequestFinish()
		{
			if (NetworkUIManager.Instance.TargetFieldID == 0) return;
			C2VDebug.Log("Sending UsePortalRequestFinish");
			Protocols.GameLogic.UsePortalRequestFinish usePortalRequestFinish = new()
			{
				FieldId = NetworkUIManager.Instance.TargetFieldID,
				MapId = NetworkUIManager.Instance.CurrentMapId,
			};
			NetworkManager.Instance.Send(usePortalRequestFinish, Protocols.GameLogic.MessageTypes.UsePortalRequestFinish);
			NetworkUIManager.Instance.TargetFieldID = 0;
		}

		public void TeleportToUserPositionFinishNotify()
		{
			C2VDebug.Log("Sending TeleportToUserPositionFinishNotify");
			Protocols.WorldState.TeleportToUserPositionFinishNotify teleportToUserPositionFinishNotify = new()
			{
				DestnationAccountId = NetworkUIManager.Instance.TeleportTargetID,
				TeleportAccountId = User.Instance.CurrentUserData.ID
			};
			NetworkManager.Instance.Send(teleportToUserPositionFinishNotify, Protocols.WorldState.MessageTypes.TeleportToUserPositionFinishNotify);
			NetworkUIManager.Instance.TeleportTargetID = 0;
		}

		public void ClientReady()
		{
			C2VDebug.Log("Sending Client Ready");
			NetworkManager.Instance.Send(SEmpty, Protocols.InternalCycle.MessageTypes.ClientReady);
		}

		public void TeleportUserSceneLoadingCompletionNotify()
		{
			C2VDebug.Log("Sending TeleportUserSceneLoadingCompletionNotify");
			Protocols.GameLogic.TeleportUserSceneLoadingCompletionNotify teleportUserSceneLoadingCompletionNotify = new()
			{
				AccountId = User.Instance.CurrentUserData.ID
			};
			NetworkManager.Instance.Send(teleportUserSceneLoadingCompletionNotify, Protocols.GameLogic.MessageTypes.TeleportUserSceneLoadingCompletionNotify);
		}

		public void Logout()
		{
			C2VDebug.Log("Sending LogOutRequest");
			Protocols.GameLogic.LogOutRequest logOutRequest = new()
			{
				UserId = User.Instance.CurrentUserData.ID
			};
			NetworkManager.Instance.Send(logOutRequest, Protocols.GameLogic.MessageTypes.LogoutRequest,
			                             Protocols.Channels.GameLogic, (int)Protocols.GameLogic.MessageTypes.LogoutResponse);
		}

		public void CheckNicknameRequest(string nickName)
		{
			Protocols.GameLogic.CheckNicknameRequest checkNicknameRequest = new()
			{
				Nickname = nickName,
			};

			NetworkManager.Instance.Send(checkNicknameRequest, Protocols.GameLogic.MessageTypes.CheckNicknameRequest,
			                             Protocols.Channels.GameLogic,
			                             (int)Protocols.GameLogic.MessageTypes.CheckNicknameResponse,
			                             timeoutAction: NicknameRule.OnTimeoutError);
		}

		public void RequestCreateAvatar(eAvatarType avatarType, AvatarInfo avatarInfo, string nickName)
		{
			// TODO: 프리셋 업데이트시 페이스 옵션으로 변환하고 즉시 삭제하는 로직 적용
			avatarInfo.RemoveFaceItem(eFaceOption.PRESET_LIST);

			Protocols.GameLogic.CreateAvatarRequest createAvatarRequest = new();
			C2VDebug.Log($"AvatarType : {(int)avatarType}");

			var fashionItemList = avatarInfo.GetFashionItemList();
			var faceItems       = avatarInfo.GetFaceOptionList();

			createAvatarRequest.AvatarType = (int)avatarType;
			createAvatarRequest.Nickname   = nickName;
			createAvatarRequest.BodyShape  = avatarInfo.BodyShape;
			foreach (var fashionItem in fashionItemList)
			{
				C2VDebug.Log($"fashionItemId : {fashionItem.ItemId}, FashionSubMenu : {fashionItem.FashionSubMenu}");
				createAvatarRequest.FashionItemList.Add(fashionItem.GetProtobufType());
			}

			foreach (var faceItem in faceItems)
			{
				C2VDebug.Log($"bodyID : {faceItem.ItemId}, FaceOption : {faceItem.FaceOption}");
				createAvatarRequest.FaceItemList.Add(faceItem.GetProtobufType());
			}

			C2VDebug.Log("Sending Request CreateAvatar");
			NetworkManager.Instance.Send(createAvatarRequest, Protocols.GameLogic.MessageTypes.CreateAvatarRequest,
			                             Protocols.Channels.GameLogic,
			                             (int)Protocols.GameLogic.MessageTypes.CreateAvatarResponse,
			                             timeoutAction: NicknameRule.OnTimeoutError);
		}

		public void RequestUpdateAvatar([NotNull] AvatarInfo avatarInfo, bool isMiceCustom = false)
		{
			// TODO: 프리셋 업데이트시 페이스 옵션으로 변환하고 즉시 삭제하는 로직 적용
			avatarInfo.RemoveFaceItem(eFaceOption.PRESET_LIST);

			var avatarId        = avatarInfo.AvatarId;
			var avatarType      = avatarInfo.AvatarType;
			var fashionItemList = avatarInfo.GetFashionItemList();
			var faceItems       = avatarInfo.GetFaceOptionList();
			var nickName        = string.Empty;

			if (User.InstanceExists)
				nickName = User.Instance.CurrentUserData.UserName;

			var updateAvatarRequest = new Protocols.CommonLogic.UpdateAvatarRequest();
			var newAvatar = new Protocols.Avatar
			{
				AvatarID   = avatarId,
				AvatarType = (int)avatarType,
				Nickname   = nickName,
				BodyShape  = avatarInfo.BodyShape,
			};

			foreach (eFashionSubMenu fashionSubMenu in Enum.GetValues(typeof(eFashionSubMenu)))
			{
				var isItemAdded = false;
				foreach (var fashionItem in fashionItemList)
				{
					if (fashionItem.FashionSubMenu != fashionSubMenu)
						continue;

					C2VDebug.Log($"AvatarId : {avatarId}, FashionSubMenu : {fashionItem.FashionSubMenu}, ItemId : {fashionItem.ItemId}");
					newAvatar.FashionItemList.Add(fashionItem.GetProtobufType());
					isItemAdded = true;
				}

				if (!isItemAdded)
				{
					var fashionItem = new Protocols.FashionItem()
					{
						FashionID  = 0,
						FashionKey = (int)fashionSubMenu,
					};
					newAvatar.FashionItemList.Add(fashionItem);
				}
			}

			foreach (var faceItem in faceItems)
			{
				C2VDebug.Log($"AvatarId : {avatarId}, FaceOption : {faceItem.FaceOption}, ItemId : {faceItem.ItemId}");
				newAvatar.FaceItemList.Add(faceItem.GetProtobufType());
			}

			updateAvatarRequest.UpdateAvatar = newAvatar;
			//TODO: true일 때 DB에 저장하지 않고 서버 메모리 상에서만 교체 - Staff 의상 제거 시 제거 필요
			updateAvatarRequest.IsMiceCustom = isMiceCustom;
			C2VDebug.Log("Sending Request UpdateAvatar");
			NetworkManager.Instance.Send(updateAvatarRequest, Protocols.CommonLogic.MessageTypes.UpdateAvatarRequest);
		}

		public void RequestEnterWorld()
		{
			var avatarCloset = AvatarMediator.Instance.AvatarCloset;

			if (avatarCloset.CurrentAvatarInfo == null)
			{
				C2VDebug.LogError("AvatarInfo is null");
				return;
			}

			Protocols.GameLogic.EnterWorldRequest enterWorldRequest = new()
			{
				AvatarID = (int)avatarCloset.CurrentAvatarInfo!.AvatarId
			};

			C2VDebug.Log("Sending Request EnterWorld");
			NetworkManager.Instance.Send(enterWorldRequest,
			                             Protocols.GameLogic.MessageTypes.EnterWorldRequest,
			                             Protocols.Channels.WorldState,
			                             (int)Protocols.WorldState.MessageTypes.TeleportUserStartNotify,
			                             timeoutAction: () => User.Instance.DefaultTimeoutProcess());
		}

		/// <summary>
		/// 월드의 광장으로 이동하는 패킷을 보내는 커맨더
		/// </summary>
		public void RequestEnterPlaza()
		{
			var avatarCloset = AvatarMediator.Instance.AvatarCloset;

			if (avatarCloset.CurrentAvatarInfo == null)
			{
				C2VDebug.LogError("AvatarInfo is null");
				return;
			}

			Protocols.GameLogic.EnterPlazaRequest enterPlazaRequest = new()
			{
				AvatarID = (int)avatarCloset.CurrentAvatarInfo!.AvatarId
			};

			C2VDebug.Log("Sending Request EnterPlaza");
			NetworkManager.Instance.Send(enterPlazaRequest, Protocols.GameLogic.MessageTypes.EnterPlazaRequest);
		}
#endregion

#region Limit Enterance
		public void RequestConnectableToWorld()
		{
			Protocols.CommonLogic.UserConnectableCheckRequest userConnectableCheck = new();
		
			C2VDebug.Log("Sending Request UserConnectableCheck");
			NetworkManager.Instance.Send(userConnectableCheck, Protocols.CommonLogic.MessageTypes.UserConnectableCheckRequest,
				timeoutAction:() =>
				{
					User.Instance.DefaultTimeoutProcess();
				});
		}
		
		public void RequestConnectQueue(Action timeout = null)
		{
			Protocols.CommonLogic.ConnectQueueRequest connectQueue = new();
		
			C2VDebug.Log("Sending Request ConnectQueue");
			NetworkManager.Instance.Send(connectQueue, Protocols.CommonLogic.MessageTypes.ConnectQueueRequest,
				timeoutAction:() =>
				{
					timeout?.Invoke();
					User.Instance.DefaultTimeoutProcess();
				});
		}
#endregion Limit Enterance
    }
}
