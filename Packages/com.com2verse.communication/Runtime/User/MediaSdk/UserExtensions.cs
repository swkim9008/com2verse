/*===============================================================
 * Product:		Com2Verse
 * File Name:	UserExtensions.cs
 * Developer:	urun4m0r1
 * Date:		2023-03-07 15:43
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Diagnostics;
using Com2Verse.Logger;
using Cysharp.Text;
using MediaSdkUser = Com2Verse.Solution.UnityRTCSdk.User;

namespace Com2Verse.Communication.MediaSdk
{
	internal static class UserExtensions
	{
#region Debug
		/// <summary>
		/// 간략한 디버그 정보를 반환합니다.
		/// </summary>
		/// <returns>
		/// "NickName (Uid)" as Role 형식의 문자열을 반환합니다.
		/// <br/>ex) "urun4m0r1 (1234567890)" as Host
		/// </returns>
		[DebuggerHidden, StackTraceIgnore]
		internal static string GetInfoText(this MediaSdkUser mediaSdkUser)
		{
			return ZString.Format(
				"\"{0} ({1})\" as {2}"
			  , mediaSdkUser.NickName, mediaSdkUser.Uid, mediaSdkUser.Role);
		}
#endregion // Debug
	}
}
