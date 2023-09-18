/*===============================================================
 * Product:		Com2Verse
 * File Name:	UserRoleConverter.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-10 11:42
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Solution.UnityRTCSdk;

namespace Com2Verse.Communication.MediaSdk
{
	internal static class UserRoleConverter
	{
		/// <summary>
		/// <see cref="Com2Verse.Solution.UnityRTCSdk"/>의 <see cref="USER_ROLE"/>을 <see cref="Com2Verse.Communication"/>의 <see cref="eUserRole"/>로 변환합니다.
		/// </summary>
		internal static eUserRole GetUserRole(this USER_ROLE userRole)
		{
			return userRole switch
			{
				USER_ROLE.Host     => eUserRole.HOST,
				USER_ROLE.Guest    => eUserRole.GUEST,
				USER_ROLE.Audience => eUserRole.AUDIENCE,
				_                  => eUserRole.UNDEFINED,
			};
		}

		/// <summary>
		/// <see cref="Com2Verse.Communication"/>의 <see cref="eUserRole"/>을 <see cref="Com2Verse.Solution.UnityRTCSdk"/>의 <see cref="USER_ROLE"/>로 변환합니다.
		/// </summary>
		/// <exception cref="ArgumentException">
		/// <paramref name="userRole"/>이 <see cref="eUserRole.UNDEFINED"/>일 경우 발생합니다.
		/// </exception>
		internal static USER_ROLE GetUserRole(this eUserRole userRole)
		{
			return userRole switch
			{
				eUserRole.HOST     => USER_ROLE.Host,
				eUserRole.GUEST    => USER_ROLE.Guest,
				eUserRole.AUDIENCE => USER_ROLE.Audience,
				eUserRole.DEFAULT  => USER_ROLE.Host,
				_                  => throw new ArgumentException("Undefined user role"),
			};
		}
	}
}
