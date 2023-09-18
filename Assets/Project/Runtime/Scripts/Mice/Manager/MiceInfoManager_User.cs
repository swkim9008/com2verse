/*===============================================================
* Product:		Com2Verse
* File Name:	MiceInfoManager_User.cs
* Developer:	ikyoung
* Date:			2023-04-13 15:59
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Com2Verse.HttpHelper;
using Com2Verse.Network;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Com2Verse.Mice
{
	public sealed partial class MiceInfoManager
	{
		public MiceUserInfo       MyUserInfo       { get; private set; }
		public bool               NeedToCreateUser { get; private set; }
		public List<MiceUserInfo> UserInfoList     { get; private set; } = new();

		private async UniTask SyncMyUser(MiceWebClient.Entities.AccountEntity accountEntity)
		{
			if (accountEntity != null && !string.IsNullOrEmpty(accountEntity.GivenName))
				NeedToCreateUser = false;
			else
				NeedToCreateUser = true;

			if (MyUserInfo != null)
				MyUserInfo.Set(accountEntity);
			else
				MyUserInfo = new MiceUserInfo(accountEntity);

			if (!string.IsNullOrEmpty(MyUserInfo.PhotoUrl))
				await TextureCache.Instance.GetOrDownloadTextureAsync(MyUserInfo.PhotoUrl);
		}

		public async UniTask SyncMyUser()
		{
			var response = await MiceWebClient.User.AccountGet();
			var data     = response.Data;
			await SyncMyUser(data);

			var responseCheck = await MiceWebClient.User.CheckPost();
			User.Instance.CurrentUserData.UserName = responseCheck.Data.Nickname;
		}

		public async UniTask<bool> SaveMyUser(MiceUserInfo userInfo)
		{
			var response = await MiceWebClient.User.AccountPost(userInfo.ToAccountInfo());
			if (response.Result.HttpStatusCode != HttpStatusCode.OK) return false;

			await SyncMyUser(response.Data);
			return true;
		}

		public async UniTask<bool> UploadMyCardImage(Texture2D photo)
		{
			if (!WebApi.Util.Instance.TrySetAuthToken()) return false;

			var bytes   = photo.EncodeToPNG();
			var apiUrl  = $"{Configurator.Instance.Config.MiceServerAddress}/api/User/Account/RegisterCardPhotoNew";
			var builder = HttpRequestBuilder.CreateNew(Client.eRequestType.POST, apiUrl);

			var content = new ByteArrayContent(bytes);
			content.Headers.Add("Content-Type", Client.Constant.ContentMultipartForm);
			builder.SetMultipartFormContent(new MultipartFormInfo
			{
				Content  = content,
				Name     = "\"file\"",
				FileName = "photo.png",
			});

			var response = await Client.Message.RequestStringAsync(builder.Request);
			if (response.StatusCode != HttpStatusCode.OK) return false;
			return true;
		}

		public async UniTask SyncCardList()
		{
			var response = await MiceWebClient.User.CardGet();
			if (response.Result.HttpStatusCode == HttpStatusCode.OK)
			{
				UserInfoList.Clear();
				foreach (var accountEntity in response.Data)
				{
					UserInfoList.Add(new MiceUserInfo(accountEntity));
				}
			}
		}

		public async UniTask RemoveCardList(IEnumerable<long> accountIdList)
		{
			foreach (var accountId in accountIdList)
			{
				var response = await MiceWebClient.User.CardDelete_TargetAccountId(accountId);
				if (response.Result.HttpStatusCode == HttpStatusCode.OK)
				{
					var index = UserInfoList.FindIndex((userInfo) => userInfo.AccountId == accountId);
					if (index == -1) continue;
					UserInfoList.RemoveAt(index);
				}
			}
		}

		public async UniTask<bool> ExchangeBusinessCard(MiceUserInfo otherUserInfo)
		{
			var response = await MiceWebClient.User.ExchangePost(otherUserInfo.AccountId);
			if (response.Result == MiceWebClient.Entities.CheckResult.OK)
			{
				otherUserInfo.SetExchangeCode(MiceWebClient.eMiceAccountCardExchangeCode.FOLLOW);
				return true;
			}
			return false;
		}

		public bool IsMyUser(MiceUserInfo userInfo)
		{
			return MyUserInfo.AccountId == userInfo.AccountId;
		}
	}
}
