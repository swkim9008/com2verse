/*===============================================================
* Product:		Com2Verse
* File Name:	UserConverter.cs
* Developer:	urun4m0r1
* Date:			2022-08-08 13:50
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Com2Verse.Logger;
using Cysharp.Text;
using MediaSdkUser = Com2Verse.Solution.UnityRTCSdk.User;

namespace Com2Verse.Communication.MediaSdk
{
	internal static class UserConverter
	{
		/// <summary>
		/// <see cref="Com2Verse.Solution.UnityRTCSdk"/>의 <see cref="MediaSdkUser"/>를 <see cref="Com2Verse.Communication"/>의 <see cref="User"/>로 변환합니다.
		/// </summary>
		/// <param name="mediaSdkUser">변환 대상</param>
		/// <param name="user">변환 결과</param>
		/// <param name="skipValidation"> <see cref="Uid"/>의 유효성 검사를 건너뜁니다. </param>
		internal static bool TryParseUser(this MediaSdkUser mediaSdkUser, out User user, bool skipValidation = false)
		{
			var uidString = mediaSdkUser.Uid;
			if (!Uid.TryParse(uidString, out var uid, skipValidation))
			{
				LogInvalidUid(mediaSdkUser);
				user = default;
				return false;
			}

			var name = mediaSdkUser.NickName;
			if (string.IsNullOrEmpty(name!))
			{
				LogInvalidName(mediaSdkUser);
				user = new User(uid, nameof(User), skipValidation);
				return true;
			}

			user = new User(uid, name, skipValidation);
			return true;
		}

#region Debug
		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		private static void LogInvalidUid(MediaSdkUser mediaSdkUser, [CallerMemberName] string? caller = null)
		{
			var message = ZString.Format(
				"Failed to parse uid from string: \"{0}\" / {1}"
			  , mediaSdkUser.Uid, mediaSdkUser.GetInfoText());

			C2VDebug.LogWarningMethod(GetLogCategory(), message, caller);
		}

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		private static void LogInvalidName(MediaSdkUser mediaSdkUser, [CallerMemberName] string? caller = null)
		{
			var message = ZString.Format(
				"User name is null or empty. Default name \"{0}\" will be used. / {1}"
			  , User.Default.Name, mediaSdkUser.GetInfoText());

			C2VDebug.LogWarningMethod(GetLogCategory(), message, caller);
		}

		[DebuggerHidden, StackTraceIgnore]
		private static string GetLogCategory() => nameof(UserConverter);
#endregion // Debug
	}
}
