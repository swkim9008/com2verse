/*===============================================================
 * Product:		Com2Verse
 * File Name:	CommunicationUserHelper.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-09 12:37
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

namespace Com2Verse.Communication
{
	public static class CommunicationUserHelper
	{
		/// <summary>
		/// 유저의 P2P 채널 역할이 호스트인지 확인합니다.
		/// <br/>P2P 채널이 아닌 경우 false를 반환합니다.
		/// </summary>
		public static bool IsP2PChannelHost(this ICommunicationUser user)
		{
			return RemoteBehaviourConverter.IsP2PChannelHost(
				user.ChannelInfo.ChannelType, user.ChannelInfo.ChannelDirection, user.Role);
		}

		/// <summary>
		/// 유저의 P2P 채널 역할이 게스트인지 확인합니다.
		/// <br/>P2P 채널이 아닌 경우 false를 반환합니다.
		/// </summary>
		public static bool IsP2PChannelGuest(this ICommunicationUser user)
		{
			return RemoteBehaviourConverter.IsP2PChannelGuest(
				user.ChannelInfo.ChannelType, user.ChannelInfo.ChannelDirection, user.Role);
		}
	}
}
