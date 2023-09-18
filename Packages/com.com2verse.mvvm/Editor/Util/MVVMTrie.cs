

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com2VerseEditor.UI
{
   	public class Node<T> : IDisposable where T : class
	{
#region Variables
		protected Dictionary<char, List<Node<T>>> _childs = new();
		private List<T> _items = new();
		private int _idx;
#endregion // Variables

#region Properties
		public IReadOnlyDictionary<char, List<Node<T>>> Childs => _childs;
		public IReadOnlyList<T> Items => _items;
		public int Index => _idx;
#endregion // Properties

#region Initialization
		public Node(int index) => _idx = index;
#endregion // Initialization

#region Indexer
		public List<Node<T>> this[char c]
		{
			get => _childs.ContainsKey(c) ? _childs[c] : null;
			set
			{
				if (_childs.ContainsKey(c))
					_childs[c] = value;
				else
					_childs.Add(c, value);
			}
		}
#endregion // Indexer

#region Public Methods
		public void AddItem(T item) => _items.Add(item);
#endregion // Public Methods

#region Dispose
		public virtual void Dispose()
		{
			if (_childs != null)
			{
				foreach (var nodes in _childs.Values)
				{
					foreach (var node in nodes)
						node.Dispose();
					nodes.Clear();
				}
			}
			_childs?.Clear();
			_childs = null;

			_items?.Clear();
			_items = null;
		}

		public virtual void Clear()
		{
			_childs?.Clear();
			_items?.Clear();
		}
#endregion // Dispose
	}
	public sealed class MVVMTrie<T> : Node<T> where T : class
	{
#region Variables
		private List<T> _searchResult;
		private MVVMTrie<T> _root;
		private int _currentIdx = 0;
		private TrieSettings _settings;
#endregion // Variables

#region Debug
		public int TotalCount => _currentIdx;
#endregion // Debug

#region Settings
		public struct TrieSettings
		{
			// 부분 검색
			public bool UsePartialSearch;
			// 초성 검색
			public bool UseConsonantSearch;
			public static TrieSettings Default = new()
			{
				UsePartialSearch = true,
				UseConsonantSearch = true,
			};
		}
#endregion // Settings

#region Create & Initialize
		public static MVVMTrie<T> CreateNew(TrieSettings settings, params Pair[] pairs)
		{
			var newTrie = new MVVMTrie<T>(pairs);
			newTrie._root = newTrie;
			newTrie._settings = settings;
			return newTrie;
		}

		private MVVMTrie(params Pair[] pairs) : base(0)
		{
			_searchResult = new List<T>();
			_currentIdx = 0;
			foreach (var pair in pairs)
				Insert(pair);
		}

		public void Insert(Pair pair) => Insert(pair.Key, pair.Value);
		private void Insert(string key, T value)
		{
			key = ToValidKey(key);
			InsertInternal(key);

			if (_settings.UseConsonantSearch)
			{
				// 모든 문자열이 초성인경우에만 초성검색 허용
				if (ConsonantUtil.TryConvertToConsonant(key, out var result))
					InsertInternal(result);
			}

			void InsertInternal(string insertKey)
			{
				Node<T> node = _root;
				foreach (var c in insertKey)
				{
					var child = new Node<T>(_currentIdx++);
					if (node[c] == null)
						node[c] = new List<Node<T>> {child};
					else
						node[c].Add(child);

					if (_settings.UsePartialSearch)
					{
						if (_root[c] == null)
							_root[c] = new List<Node<T>> {child};
						else
							_root[c].Add(child);
					}
					node = node[c][^1];
				}
				node.AddItem(value);
			}
		}
#endregion // Create & Initialize

#region Search
		public List<T> FindAll(string key)
		{
			key = ToValidKey(key);
			var results = Find(key);
			if (results.Count == 0)
			{
				return null;
			}

			_searchResult.Clear();
			var result = FindAllLeaf(results);
			return result;
		}

		private List<Node<T>> GetAllNodes(MVVMTrie<T> root)
		{
			if (root == null) return null;

			var result = new List<Node<T>>();
			foreach (var (key, value) in root.Childs)
				result.AddRange(value.ToArray());
			return result;
		}

		private List<Node<T>> Find(string key)
		{
			var root = this;
			var result = new List<Node<T>>();
			var checkVisit = new HashSet<int>();
			if (key.Length == 0)
				return GetAllNodes(this);

			FindInternal(root, key, 0);
			return result;

			void FindInternal(Node<T> t, string s, int idx)
			{
				if (idx >= s.Length) return;

				var lt = t[s[idx]];
				if (lt == null) return;

				foreach (var subT in lt)
				{
					if (idx == s.Length - 1)
					{
						if (checkVisit.Contains(subT.Index)) continue;
						checkVisit.Add(subT.Index);
						result.Add(subT);
					}
					else
						FindInternal(subT, s, idx + 1);
				}
			}
		}
		private List<T> FindAllLeaf(List<Node<T>> nodes)
		{
			var result = _searchResult;
			foreach (var node in nodes)
			{
				if (node.Items != null)
				{
					foreach (var nodeItem in node.Items)
					{
						if (result.Contains(nodeItem))
							continue;
						result.Add(nodeItem);
					}
				}

				foreach (var kvp in node.Childs)
					FindAllLeaf(kvp.Value);
			}
			return result;
		}
#endregion // Search

#region Dispose / Clear
		public override void Dispose()
		{
			_root = null;
			_searchResult?.Clear();
			_searchResult = null;
			base.Dispose();
		}

		public override void Clear()
		{
			_searchResult?.Clear();
			base.Clear();
		}
#endregion // Dispose / Clear

#region Util
		private string ToValidKey(string key)
		{
			key = key.Replace(" ", string.Empty);
			key = key.ToLower();
			return key;
		}
#endregion // Util

#region Data
		public struct Pair
		{
			public string Key;
			public T Value;
		}
#endregion // Data
	}
	
	
	public static class ConsonantUtil
	{
		private static (int, int)[] _numberRange = new[]
		{
			// 0 ~ 9
			(0x30, 0x39),
		};
		private static (int, int)[] _engRange = new[]
		{
			// a z
			(0x61, 0x7A),
			// A Z
			(0x41, 0x5A),
		};
		private static (int, char)[] _korMap = new[]
		{
			// 가 까 나 다 따
			(0xAC00, 'ㄱ'), (0xAE4C, 'ㄲ'), (0xB098, 'ㄴ'), (0xB2E4, 'ㄷ'), (0xB530, 'ㄸ'),
			// 라 마 바 빠 사
			(0xB77C, 'ㄹ'), (0xB9C8, 'ㅁ'), (0xBC14, 'ㅂ'), (0xBE60, 'ㅃ'), (0xC0AC, 'ㅅ'),
			// 싸 아 자 짜 차
			(0xC2F8, 'ㅆ'), (0xC544, 'ㅇ'), (0xC790, 'ㅈ'), (0xC9DC, 'ㅉ'), (0xCC28, 'ㅊ'),
			// 카 타 파 하 힣
			(0xCE74, 'ㅋ'), (0xD0C0, 'ㅌ'), (0xD30C, 'ㅍ'), (0xD558, 'ㅎ'), (0xD7A4, 'ㅎ'),
		};

		private static int[] _specialChars = new[]
		{
			0x2A, // *
		};

		private static readonly StringBuilder _sb = new();
		private static readonly char _invalidConsonant = '_';

		public static string ConvertToConsonant(string text)
		{
			var unicodes = ConvertToUnicode(text.ToLower());
			return ConvertToConsonant(unicodes);
		}
		private static int[] ConvertToUnicode(string text)
		{
			var result = new int[text.Length];
			for (int i = 0; i < text.Length; ++i)
				result[i] = char.ConvertToUtf32(text, i);
			return result;
		}

		private static string ConvertToConsonant(int[] unicodes)
		{
			_sb.Clear();
			foreach (var unicode in unicodes)
				_sb.Append(GetConsonant(unicode));
			return _sb.ToString();
		}

		public static bool TryConvertToConsonant(string text, out string result)
		{
			var unicodes = ConvertToUnicode(text.ToLower());
			return TryConvertToConsonant(unicodes, out result);
		}
		private static bool TryConvertToConsonant(int[] unicodes, out string result)
		{
			result = string.Empty;

			_sb.Clear();
			foreach (var unicode in unicodes)
			{
				var consonant = GetConsonant(unicode);
				if (consonant == _invalidConsonant)
					return false;

				_sb.Append(consonant);
			}

			result = _sb.ToString();
			return true;
		}
		private static char GetConsonant(int unicode)
		{
			if (IsNumber(unicode))
				return char.ConvertFromUtf32(unicode)[0];
			if (IsEnglish(unicode))
				return char.ConvertFromUtf32(unicode)[0];
			if (IsKorean(unicode))
				return GetConsonantKor(unicode);
			if (IsSpecialChar(unicode))
				return char.ConvertFromUtf32(unicode)[0];
			return _invalidConsonant;
		}

		private static bool IsNumber(int unicode) => unicode >= _numberRange[0].Item1 && unicode <= _numberRange[0].Item2;
		private static bool IsEnglish(int unicode)
		{
			foreach(var range in _engRange)
				if (unicode >= range.Item1 && unicode <= range.Item2)
					return true;
			return false;
		}
		private static bool IsKorean(int unicode) => unicode >= _korMap[0].Item1 &&
		                                             unicode <= _korMap[^1].Item1;

		private static bool IsSpecialChar(int unicode) => _specialChars.Contains(unicode);
		private static char GetConsonantKor(int unicode)
		{
			for (int i = 1; i < _korMap.Length; ++i)
				if (_korMap[i - 1].Item1 <= unicode && unicode < _korMap[i].Item1)
					return _korMap[i - 1].Item2;

			return '-';
		}
	}
}
