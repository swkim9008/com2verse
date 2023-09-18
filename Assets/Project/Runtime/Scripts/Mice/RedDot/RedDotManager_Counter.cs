/*===============================================================
* Product:		Com2Verse
* File Name:	RedDotManager_Counter.cs
* Developer:	klizzard
* Date:			2023-08-09 11:39
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Com2Verse.Data;

namespace Com2Verse.Mice
{
	using CounterDelegate = Func<int>;
	using CounterFuncMap = Dictionary<RedDotManager.RedDotData.Key, Func<int>>;

	public sealed partial class RedDotManager // Counter
	{
		[AttributeUsage(AttributeTargets.Method)]
		public class RedDotCounterAttribute : Attribute
		{
			public RedDotData.Key Key { get; }

			public RedDotCounterAttribute(RedDotData.Key key)
			{
				Key = key;
			}
		}

		private CounterFuncMap _counterFuncMap;

		private void InitCounterFunc()
		{
			if (_counterFuncMap != null) return;
			_counterFuncMap = new CounterFuncMap();

			// default value
			foreach (RedDotData.Key key in Enum.GetValues(typeof(RedDotData.Key)))
				_counterFuncMap.Add(key, Counter_None);

			// add function by attribute
			var methods =
				typeof(RedDotManager).GetMethods(BindingFlags.Public | BindingFlags.Static |
				                                 BindingFlags.InvokeMethod);
			foreach (var method in methods)
			{
				var attr = method.GetCustomAttribute<RedDotCounterAttribute>();
				if (attr != null)
				{
					_counterFuncMap[attr.Key] =
						Delegate.CreateDelegate(typeof(CounterDelegate), method) as CounterDelegate;
				}
			}
		}
	}

	public sealed partial class RedDotManager
	{
		// default value
		[RedDotCounter(RedDotData.Key.None)]
		public static int Counter_None()
		{
			return 0;
		}

		[RedDotCounter(RedDotData.Key.MiceApp)]
		public static int Counter_MiceApp()
		{
			if (CurrentScene.ServiceType != eServiceType.MICE)
				return 0;

			var count = 0;
			{
				//공지사항.
				count += MiceInfoManager.Instance.NoticeInfos.Values.Count(e => MiceInfoManager.Instance.NoticeClickedPrefs.IsNew(e.NoticeEntity.BoardSeq));
				//내 티켓.
				count += MiceInfoManager.Instance.MyUserInfo?.PackageInfos.Values.Count(e => MiceInfoManager.Instance.MyPackageClickedPrefs.IsNew(e.PackageId)) ?? 0;
			}
			return count;
		}
	}
}
