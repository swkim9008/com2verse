/*===============================================================
* Product:		Com2Verse
* File Name:	DisplayNameHelper.cs
* Developer:	urun4m0r1
* Date:			2022-09-28 19:19
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Communication;
using Com2Verse.Extension;
using Com2Verse.Organization;
using Cysharp.Threading.Tasks;

namespace Com2Verse.UI
{
	public enum eDisplayName
	{
		UID,
		COMMUNICATION_NAME,
		LOGIN_ID,
		EMPLOYEE_NAME,
	}

	/// <summary>
	/// 자신 또는 다른 유저의 DisplayName을 비동기로 가져오는 Helper
	/// </summary>
	public static class DisplayNameHelper
	{
		public static async UniTask<string?> GetLocalUserName()
		{
			if (!Network.User.InstanceExists) return string.Empty;
			var user = new User(Network.User.Instance.CurrentUserData.ID, Network.User.Instance.CurrentUserData.UserName!);
			return await GetDisplayName(user);
		}

		public static async UniTask<string?> GetDisplayName(User user)
		{
			var enumSize = Enum.GetNames(typeof(eDisplayName)).Length;
			for (var i = enumSize - 1; i >= 0; i--)
			{
				var name = await GetDisplayName(user, (eDisplayName)i);
				if (IsValidName(name))
				{
					return name!;
				}
			}

			return null;
		}

		public static async UniTask<string?> GetDisplayName(User user, eDisplayName display) => display switch
		{
			eDisplayName.UID                => user.Uid.ToString(),
			eDisplayName.COMMUNICATION_NAME => user.Name,
			eDisplayName.LOGIN_ID           => GetLoginId(user.Uid),
			eDisplayName.EMPLOYEE_NAME      => await GetMemberNameAsync(user.Uid),
			_                               => throw new ArgumentOutOfRangeException(nameof(display), display, null!),
		};

		public static string? GetLoginId(long uid)
		{
			if (IsLocalUser(uid))
			{
				var accountId = Network.User.Instance.CurrentUserData.UserName;
				if (IsValidName(accountId))
				{
					return accountId;
				}
			}

			return null; // TODO: 원격 유저의 로그인 아이디르 가져오는 로직이 필요.
		}

		public static async UniTask<string?> GetMemberNameAsync(long uid)
		{
			var memberModel = await DataManager.Instance.GetMemberAsync(uid);

			var employeeName = memberModel?.Member.MemberName;
			if (IsValidName(employeeName))
			{
				return employeeName;
			}

			if (IsLocalUser(uid))
				return Network.User.Instance.CurrentUserData.UserName;

			return null;
		}

		private static bool IsLocalUser(long uid) => uid == Network.User.Instance.CurrentUserData.ID;

		private static bool IsValidName(string? value) => !string.IsNullOrWhiteSpace(value!);
	}
}
