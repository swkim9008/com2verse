/*===============================================================
* Product:		Com2Verse
* File Name:	FilterListBase.cs
* Developer:	jhkim
* Date:			2023-07-14 20:28
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.BannedWords
{
	public abstract class FilterListBase<T> : IFilterList where T : new()
	{
		[NotNull] internal readonly Dictionary<string, List<BannedWordsInfo.WordInfo>> Map = new Dictionary<string, List<BannedWordsInfo.WordInfo>>();
		public void Add(string lang, IEnumerable<BannedWordsInfo.WordInfo> infos)
		{
			if (string.IsNullOrWhiteSpace(lang)) return;
			if (infos == null || !infos.Any()) return;

			lang = lang.ToLower();
			if (Map.TryGetValue(lang, out var value))
				value?.AddRange(infos);
			else
				Map.Add(lang, new List<BannedWordsInfo.WordInfo>(infos));
		}

		public void Clear()
		{
			foreach (var (_, value) in Map)
				value?.Clear();
			Map.Clear();
		}

		public static T Create()
		{
			var instance = new T();
			return instance;
		}
	}

	public class DerivedFilterList : FilterListBase<DerivedFilterList>
	{
		internal bool IsLangExist(string lang) => !string.IsNullOrWhiteSpace(lang) && Map.ContainsKey(lang);
		internal IReadOnlyList<BannedWordsInfo.WordInfo> GetLists(string lang) => string.IsNullOrWhiteSpace(lang) ? Array.Empty<BannedWordsInfo.WordInfo>() : Map?[lang];
	}

	public static class FilterList
	{
		[NotNull] public static readonly DerivedFilterList BlackList = new();
		[NotNull] public static readonly DerivedFilterList WhiteList = new();

#if UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Clear()
		{
			BlackList?.Clear();
			WhiteList?.Clear();
		}
#endif // UNITY_EDITOR
	}
}
