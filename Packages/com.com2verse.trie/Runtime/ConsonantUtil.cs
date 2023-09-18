/*===============================================================
* Product:		Com2Verse
* File Name:	ConsonantUtil.cs
* Developer:	jhkim
* Date:			2022-07-14 17:50
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Linq;
using System.Text;

namespace Com2Verse.Trie
{
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

		/// <summary>
		/// 초성 변환 (한글이 있으면 초성으로 변환, 그 외의 문자는 그대로 반환)
		/// </summary>
		/// <param name="text">원문 문자열</param>
		/// <returns>초성 변환된 문자열</returns>
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

		/// <summary>
		/// 입력된 문자열이 유효한 경우 초성 변환 (숫자, 알파벳, 한글 및 특수문자 '*')
		/// </summary>
		/// <param name="text">원문 문자열</param>
		/// <param name="result">초성 변환 된 문자열</param>
		/// <returns>모든 문자가 유효한 문자면 true</returns>
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
