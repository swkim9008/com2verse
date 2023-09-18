/*===============================================================
* Product:		Com2Verse
* File Name:	ChannelManagerHelper.cs
* Developer:	urun4m0r1
* Date:			2022-08-05 16:07
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Communication.MediaSdk;
using Com2Verse.Organization;
using Com2Verse.WebApi.Service;
using Newtonsoft.Json;
using Protocols.OfficeMeeting;

namespace Com2Verse.Communication
{
	public static class ChannelManagerHelper
	{
		private static User CreateSelf(string? nickname = null)
		{
			var userId   = Network.User.Instance.CurrentUserData.ID;
			var userName = Network.User.Instance.CurrentUserData.UserName;

			return nickname != null ? new User(userId, nickname) : new User(userId, userName);
		}

		public static IChannel AddChannel(Protocols.Communication.JoinChannelResponse response, eUserRole role)
		{
			var channelId      = response.ChannelId!;
			var channelType    = response.ChannelType;
			var rtcChannelInfo = response.RtcChannelInfo!;
			var accessToken    = Network.User.Instance.CurrentUserData.AccessToken;

			var channel = MediaChannelFactory.CreateInstance(channelId, accessToken, channelType, CreateSelf(), rtcChannelInfo, role);
			ChannelManager.Instance.AddChannel(channel);
			return channel;
		}

		//public static IChannel AddChannel(Protocols.OfficeMeeting.JoinChannelResponse response)
		//{
		//	var channelId      = response.ChannelId!;
		//	var channelType    = response.ChannelType;
		//	var rtcChannelInfo = response.RtcChannelInfo!;
		//	var accessToken    = Network.User.Instance.CurrentUserData.AccessToken;
//
		//	var channel = MediaChannelFactory.CreateInstance(channelId, accessToken, channelType, CreateSelf(), rtcChannelInfo, eUserRole.DEFAULT);
		//	ChannelManager.Instance.AddChannel(channel);
		//	return channel;
		//}

		public static IChannel AddChannel(Components.RoomJoinResponse response, Action disconnectRequest, string? extraInfo = null, string? nickname = null)
		{
			var channelId = response.RoomId!;
			var channelType = response.ChannelType.ToProtocolType();
			var direction = response.Direction.ToProtocolType();
			var rtcChannelInfo = new RTCChannelInfo
			{
				Direction = direction,
				MediaName = response.MediaId,
				ServerUrl = response.MediaUrl,
			};
			var accessToken = Network.User.Instance.CurrentUserData.AccessToken;
			var channel     = MediaChannelFactory.CreateInstance(channelId, accessToken, channelType, CreateSelf(nickname), rtcChannelInfo, eUserRole.DEFAULT, disconnectRequest, extraInfo);
			ChannelManager.Instance.AddChannel(channel);
			return channel;
		}

		public static IChannel JoinChannel(Protocols.Communication.JoinChannelResponse response, eUserRole role)
		{
			var channel = AddChannel(response, role);
			ChannelManager.Instance.JoinChannel(channel.Info.ChannelId);
			return channel;
		}

		//public static IChannel JoinChannel(Protocols.OfficeMeeting.JoinChannelResponse response)
		//{
		//	var channel = AddChannel(response);
		//	ChannelManager.Instance.JoinChannel(channel.Info.ChannelId);
		//	return channel;
		//}
	}

	[Serializable]
	public class ExtraInfo
	{
		[JsonProperty("id")]
		public string? Uid;

		[JsonProperty("job")]
		public string? Job;

		[JsonProperty("name")]
		public string? Name;

		[JsonProperty("position")]
		public string? Position;

		[JsonProperty("team")]
		public string? Team;

		[JsonProperty("token")]
		public string? Token;
	}
}
