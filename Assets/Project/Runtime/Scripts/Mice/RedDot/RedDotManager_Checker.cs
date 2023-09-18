/*===============================================================
* Product:		Com2Verse
* File Name:	RedDotManager_Checker.cs
* Developer:	seaman2000
* Date:			2023-08-01 09:45
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System;
using System.Reflection;
using Com2Verse.Data;

namespace Com2Verse.Mice
{
	using CheckerDelegate = Func<object[], bool>;
	using CheckerFuncMap = Dictionary<RedDotManager.CheckerKey, Func<object[], bool>>;

	public sealed partial class RedDotManager // Checker
	{
		[AttributeUsage(AttributeTargets.Method)]
		public class RedDotCheckerAttribute : Attribute
		{
			public CheckerKey Key { get; }

			public RedDotCheckerAttribute(CheckerKey key)
			{
				Key = key;
			}
		}

		private CheckerFuncMap _checkerFuncMap;

		private void InitCheckerFunc()
		{
			if (_checkerFuncMap != null) return;
			_checkerFuncMap = new CheckerFuncMap();

			// default value
			foreach (CheckerKey key in Enum.GetValues(typeof(CheckerKey)))
				_checkerFuncMap.Add(key, Checker_None);

			// add function by attribute
			var methods =
				typeof(RedDotManager).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod);
			foreach (var method in methods)
			{
				var attr = method.GetCustomAttribute<RedDotCheckerAttribute>();
				if (attr != null)
				{
					_checkerFuncMap[attr.Key] =
						Delegate.CreateDelegate(typeof(CheckerDelegate), method) as CheckerDelegate;
				}
			}
		}
	}

	public sealed partial class RedDotManager
	{
		public enum CheckerKey
		{
			None,
			HasNewNotice,
			HasNewMyPackage
		}

		// default value
		[RedDotChecker(CheckerKey.None)]
		public static bool Checker_None(object[] triggerArgs)
		{
			return false;
		}


		[RedDotChecker(CheckerKey.HasNewNotice)]
		public static bool Checker_HasNewNotice(object[] triggerArgs)
		{
			if (CurrentScene.ServiceType != eServiceType.MICE)
				return false;

			var values = MiceInfoManager.Instance.NoticeInfos.Values;
			foreach (var info in values)
			{
				if (MiceInfoManager.Instance.NoticeClickedPrefs.IsNew(info.NoticeEntity.BoardSeq))
					return true;
			}

			return false;
		}

		[RedDotChecker(CheckerKey.HasNewMyPackage)]
		public static bool Checker_HasNewMyPackage(object[] triggerArgs)
		{
			if (CurrentScene.ServiceType != eServiceType.MICE)
				return false;

			var myUserInfo = MiceInfoManager.Instance.MyUserInfo;
			if (myUserInfo == null) return false;

			var values = myUserInfo.PackageInfos.Values;
			foreach (var info in values)
			{
				if (MiceInfoManager.Instance.MyPackageClickedPrefs.IsNew(info.PackageId))
					return true;
			}

			return false;
		}
	}
}
