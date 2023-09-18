/*===============================================================
* Product:		Com2Verse
* File Name:	BannedWordsInfo.cs
* Developer:	jhkim
* Date:			2023-03-08 19:04
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Text;
using Com2Verse.Logger;
using JetBrains.Annotations;
using UnityEngine;
using static Com2Verse.BannedWords.FilterList;

namespace Com2Verse.BannedWords
{
	/*	word	lang	country	usage */
	[Serializable]
	public sealed class BannedWordsInfo : IDisposable
	{
#region Variables
		private const string All = BannedWords.All;
		private const string TokenSplitter = "\t";

		private static readonly int ColumnCount = 4;
		private static readonly int IndexWord = 0;
		private static readonly int IndexLang = 1;
		private static readonly int IndexCountry = 2;
		private static readonly int IndexUsage = 3;

		private static readonly string[] HeaderNames = {"word", "lang", "country", "usage"};

		[SerializeField]
		private string _langCode;
		[SerializeField]
		private string _countryCode;
		[SerializeField]
		private string _usage;

		private Dictionary<string, List<WordInfo>> _wordInfoMap = new(); // Key : LangCode
		private StringBuilder _sb;
#endregion // Variables

#region Properties
		public IReadOnlyDictionary<string, List<WordInfo>> WordInfoMap => _wordInfoMap;
		public string LanguageCode => _langCode;
		public string CountryCode => _countryCode;
		public string Usage => _usage;
#endregion // Properties

#region Initialization
		internal static BannedWordsInfo Create(string data, string langCode = All, string countryCode = All, string usage = All)
		{
			var result = new BannedWordsInfo();
			result.SetLanguageCode(langCode);
			result.SetCountryCode(countryCode);
			result.SetUsage(usage);
			result.ParseData(data);
			return result;
		}
#endregion // Initialization

#region Public Methods
		public void SetLanguageCode(string lang) => _langCode = lang.ToLower();
		public void ResetLanguageCode() => SetLanguageCode(All);
		public void SetCountryCode(string country) => _countryCode = country.ToLower();
		public void ResetCountryCode() => SetCountryCode(All);
		public void SetUsage(string usage) => _usage = usage.ToLower();
		public void ResetUsage() => SetUsage(All);

		public string ApplyFilter(string text, string replace, bool matchNumberOfFiltered)
		{
			_sb ??= new();

			_sb.Clear();
			_sb.Append(text);

			if (_langCode != All)
				FindAndReplace(_langCode);
			FindAndReplace(All);

			return _sb.ToString();

			void FindAndReplace(string key)
			{
				FindBannedWords(text, key, (findIdx, wordLength) =>
				{
					_sb.Remove(findIdx, wordLength);

					if (matchNumberOfFiltered)
						_sb.Insert(findIdx, replace, wordLength);
					else
						_sb.Insert(findIdx, replace);
					return true;
				});
			}
		}

		public bool HasBannedWords(string text)
		{
			_sb ??= new();
			_sb.Clear();
			_sb.Append(text);

			var find = false;
			if (_langCode != All)
			{
				FindBannedWords(text, _langCode, Find);
				if (find) return true;
			}

			_sb.Clear();
			_sb.Append(text);
			FindBannedWords(text, All, Find);
			return find;

			bool Find(int findIdx, int wordLength)
			{
				find = true;
				return false;
			}
			;
		}

		private void FindBannedWords(string text, string key, Func<int, int, bool> onFindWord)
		{
			key = key.ToLower();
			if (!_wordInfoMap.ContainsKey(key)) return;

			foreach (var wordInfo in _wordInfoMap[key])
			{
				var keepSearch = FindInternal(wordInfo.Word, wordInfo.Country, wordInfo.Usage);
				if (!keepSearch) break;
			}

			FindBlackList();

			// Return - keepSearch
			bool FindInternal(string word, string country = All, string usage = All)
			{
				if (text.Contains(word, StringComparison.OrdinalIgnoreCase))
				{
					country = country.ToLower();
					usage = usage.ToLower();
					if (!(country == All || country == _countryCode)) return true;
					if (!(usage == All || usage == _usage)) return true;

					var idx = 0;
					int findIdx;
					while ((findIdx = IndexOf(_sb, word, idx, true)) != -1)
					{
						if (IsWhiteListed(_sb, idx, findIdx, true))
						{
							idx++;
							continue;
						}

						var keepSearch = onFindWord?.Invoke(findIdx, word.Length);
						if (keepSearch.HasValue && !keepSearch.Value) return false;

						idx = (idx == findIdx) ? idx + 1 : findIdx;
					}
				}

				return true;
			}

			void FindBlackList()
			{
				FindBlackListInternal(All);
				if (BlackList.IsLangExist(_langCode) && _langCode != All)
					FindBlackListInternal(_langCode);

				void FindBlackListInternal(string langCode)
				{
					langCode = langCode.ToLower();
					if (!BlackList.IsLangExist(langCode)) return;

					var blackList = BlackList.GetLists(langCode);
					foreach (var wordInfo in blackList)
					{
						var keepSearch = FindInternal(wordInfo.Word, wordInfo.Country, wordInfo.Usage);
						if (!keepSearch) break;
					}
				}
			}
		}

		private bool IsBlackListed(StringBuilder sb, int startIdx, int findIdx, bool ignoreCase)
		{
			var isFound = IsFound(_langCode);
			if (!isFound && _langCode != All)
				isFound = IsFound(All);
			return isFound;

			bool IsFound(string language)
			{
				if (!BlackList.IsLangExist(language)) return false;

				var blackList = BlackList.GetLists(language);

				foreach (var wordInfo in blackList)
				{
					var idx = IndexOf(sb, wordInfo.Word, startIdx, ignoreCase);
					if (findIdx == idx)
						return true;
				}

				return false;
			}
		}
		private bool IsWhiteListed(StringBuilder sb, int startIdx, int findIdx, bool ignoreCase)
		{
			var isFound = IsFound(_langCode);
			if (!isFound && _langCode != All)
				isFound = IsFound(All);
			return isFound;

			bool IsFound(string language)
			{
				if (!WhiteList.IsLangExist(language)) return false;

				var whiteList = WhiteList.GetLists(language);

				foreach (var wordInfo in whiteList)
				{
					var idx = IndexOf(sb, wordInfo.Word, startIdx, ignoreCase);
					if (findIdx == idx)
						return true;
				}

				return false;
			}
		}
		// https://stackoverflow.com/questions/1359948/why-doesnt-stringbuilder-have-indexof-method
		private static int IndexOf(StringBuilder sb, string value, int startIndex, bool ignoreCase)
		{
			int len = value.Length;
			int max = (sb.Length - len) + 1;
			var v1 = (ignoreCase)
				? value.ToLower()
				: value;
			var func1 = (ignoreCase)
				? new Func<char, char, bool>((x, y) => char.ToLower(x) == y)
				: new Func<char, char, bool>((x, y) => x == y);
			for (int i1 = startIndex; i1 < max; ++i1)
				if (func1(sb[i1], v1[0]))
				{
					int i2 = 1;
					while ((i2 < len) && func1(sb[i1 + i2], v1[i2]))
						++i2;
					if (i2 == len)
						return i1;
				}

			return -1;
		}
#endregion // Public Methods

#region Private Methods
		private void ParseData(string data)
		{
			if (string.IsNullOrWhiteSpace(data))
			{
				C2VDebug.LogWarning("금칙어 데이터가 없습니다.");
				return;
			}

			var lines = data.Split("\n");
			if (lines.Length < 1)
			{
				C2VDebug.LogWarning("금칙어 데이터가 없습니다.");
				return;
			}

			if (!ValidateHeader(lines[0]))
			{
				C2VDebug.LogWarning("금칙어 데이터가 올바르지 않습니다.");
				return;
			}

			for (var i = 1; i < lines.Length; i++)
			{
				var w = WordInfo.Create(lines[i]);
				if (w is not { }) continue;

				var wordInfo = w.Value;
				var lang = wordInfo.Lang;
				var key = string.IsNullOrWhiteSpace(lang) ? All : lang;
				key = key.ToLower();

				if (!_wordInfoMap.ContainsKey(key))
					_wordInfoMap.Add(key, new List<WordInfo>());
				_wordInfoMap[key].Add(wordInfo);
			}

			// 글자 수 내림차순 정렬 (짧은 단어로 먼저 필터링 되는 것을 방지)
			foreach (var key in _wordInfoMap.Keys)
				_wordInfoMap[key].Sort((l, r) => r.Word.Length - l.Word.Length);
		}

		private bool ValidateHeader(string headerLine)
		{
			var tokens = headerLine.Split(TokenSplitter);
			if (tokens.Length != HeaderNames.Length) return false;

			for (var i = 0; i < tokens.Length; i++)
				if (!tokens[i].Equals(HeaderNames[i])) return false;

			return true;
		}
#endregion // Private Methods

#region Dispose
		public void Dispose()
		{
			if (_wordInfoMap != null)
			{
				foreach (var (key, value) in _wordInfoMap)
					value.Clear();
				_wordInfoMap.Clear();
				_wordInfoMap = null;
			}
		}
#endregion // Dispose

#region Word Info
		[Serializable]
		public struct WordInfo
		{
#region Variables
			[SerializeField]
			private string _word;
			[SerializeField] [CanBeNull]
			private string _lang;
			[SerializeField] [CanBeNull]
			private string _country;
			[SerializeField] [CanBeNull]
			private string _usage;
#endregion // Variables

#region Properties
			public string Word => _word;
			public string Lang => _lang ?? All;
			public string Country => _country ?? All;
			public string Usage => _usage ?? All;
#endregion // Properties

			private WordInfo(string word)
			{
				_word = word;
				_lang = null;
				_country = null;
				_usage = null;
			}

			internal static WordInfo? Create(string line)
			{
				var tokens = line.Split(TokenSplitter);
				if (tokens.Length != ColumnCount) return null;

				var wordInfo = new WordInfo(tokens[IndexWord]);
				if (tokens[IndexLang] != All)
					wordInfo._lang = tokens[IndexLang].ToLower();
				if (tokens[IndexCountry] != All)
					wordInfo._country = tokens[IndexCountry].ToLower();
				if (tokens[IndexUsage] != All)
					wordInfo._usage = tokens[IndexUsage].ToLower();
				return wordInfo;
			}

			public static WordInfo Create(string word, string lang, string country, string usage)
			{
				var wordInfo = new WordInfo(word)
				{
					_lang = lang,
					_country = country,
					_usage = usage,
				};
				return wordInfo;
			}
		}
#endregion // Word Info
	}
}