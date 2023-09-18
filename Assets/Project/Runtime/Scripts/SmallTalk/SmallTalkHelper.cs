/*===============================================================
 * Product:		Com2Verse
 * File Name:	SmallTalkHelper.cs
 * Developer:	urun4m0r1
 * Date:		2023-05-09 18:49
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Net;
using Com2Verse.Communication;
using Com2Verse.HttpHelper;
using Com2Verse.Logger;
using Com2Verse.UI;
using Com2Verse.WebApi.Service;
using Cysharp.Threading.Tasks;
using Protocols.Communication;
using User = Com2Verse.Network.User;

namespace Com2Verse.SmallTalk
{
	public static class SmallTalkHelper
	{
		public static async UniTask<SelfMediaChannelNotify?> GetSelfChannel()
		{
			var smallTalkData = await Api.Media.PostMediaCreateSmallTalk(null, RequestOption.NoTimeout);
			if (smallTalkData == null)
			{
				NetworkUIManager.Instance.ShowWebApiErrorMessage(0);
				return null;
			}


			if (smallTalkData.StatusCode != HttpStatusCode.OK || smallTalkData.Value?.Code != Components.OfficeHttpResultCode.Success)
			{
				NetworkUIManager.Instance.ShowWebApiErrorMessage(Components.OfficeHttpResultCode.Fail);
				return null;
			}

			SelfMediaChannelNotify selfNotify = new()
			{
				ChannelId = smallTalkData.Value.Data?.RoomId,
			};

			try
			{
				RTCChannelInfo selfRtcChannelInfo = new()
				{
					AccessToken   = User.Instance.CurrentUserData.AccessToken,
					ServerUrl     = smallTalkData.Value.Data?.MediaUrl,
					MediaName     = smallTalkData.Value.Data?.MediaId,
					Direction     = Direction.Outgoing,
					AuthorityCode = AuthorityCode.Host,
					Password      = string.Empty,

					IceServerConfigs =
					{
						new List<IceServerConfig>()
						{
							new IceServerConfig
							{
								IceServerUrl = smallTalkData.Value.Data?.TurnServer?.Url,
								AccountName = smallTalkData.Value.Data?.TurnServer?.Account,
								Credential = smallTalkData.Value.Data?.TurnServer?.Password
							}, 
							new IceServerConfig
							{
								IceServerUrl = smallTalkData.Value.Data?.StunServer?.Url
							}
						}
					},
				};

				selfNotify.RtcChannelInfo = selfRtcChannelInfo;
			}
			catch (Exception e)
			{
				NetworkUIManager.Instance.ShowWebApiErrorMessage(0);
				C2VDebug.LogError($"RTCChannelInfo data is null : {e.Message}");
			}
			
			return selfNotify;
		}

		public static IChannel JoinChannel(string channelId, RTCChannelInfo rtcChannelInfo)
		{
			var response = new JoinChannelResponse
			{
				ChannelId      = channelId,
				ChannelType    = ChannelType.SmallTalk,
				RtcChannelInfo = rtcChannelInfo,
			};

			var role = rtcChannelInfo.Direction switch
			{
				Direction.Incomming => eUserRole.GUEST,
				Direction.Outgoing  => eUserRole.HOST,
				_                   => eUserRole.UNDEFINED,
			};

			return ChannelManagerHelper.JoinChannel(response, role);
		}
	}
}
