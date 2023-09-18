/*===============================================================
* Product:		Com2Verse
* File Name:	MediaSdkChannelAdapter.cs
* Developer:	urun4m0r1
* Date:			2022-09-08 18:04
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Text;
using MediaSdkUser = Com2Verse.Solution.UnityRTCSdk.User;

namespace Com2Verse.Communication.MediaSdk
{
	public static partial class MediaSdkChannelAdapter
	{
		public static MediaSdkUser GetMediaSdkUser(string channelId, User loginUser, eUserRole role, string? extraInfo = null)
		{
			var user = new MediaSdkUser(channelId, loginUser.Uid.ToString()!, loginUser.Name!)
			{
				Role = role.GetUserRole(),
				ExtraInfo = extraInfo != null ? Encoding.UTF8.GetBytes(extraInfo) : null,
			};

			return user;
		}
	}
}
