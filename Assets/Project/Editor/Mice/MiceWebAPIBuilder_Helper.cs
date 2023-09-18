/*===============================================================
* Product:		Com2Verse
* File Name:	MiceWebAPIBuilder_Helper.cs
* Developer:	sprite
* Date:			2023-04-11 15:03
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor.Presets;

namespace Com2VerseEditor.Mice
{
    internal static class Helper
    {
        /// <summary>
        /// 모든 중괄호('{'~'}') 범위의 문자열을 찾는다.
        /// </summary>
        public static Regex regexBrace = new Regex(@"{\s*(.*?)[^}]*}", RegexOptions.Compiled);
        /// <summary>
        /// 모든 중첩된 중괄호('{'~'}')들 중 최하위에 있는 중괄호('{'~'}') 범위의 문자열을 찾는다.
        /// </summary>
        public static Regex regexNestedBrace = new Regex(@"(\{([^{}]|{?R})*\})", RegexOptions.Compiled);
        /// <summary>
        /// 모든 'public class' 문자열을 찾는다.
        /// </summary>
        public static Regex regexPublicClass = new Regex(@"public class", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        /// <summary>
        /// 모든 대괄호('['~']') 범위의 문자열을 찾는다.
        /// <para>(이중 대괄호('[[' ~ ']]') 제외)</para>
        /// </summary>
        //public static Regex regexSquareBrackets = new Regex(@"\[[^\]]*\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex regexSquareBrackets = new Regex(@"(?<!\[)\[([^\s\]]+)\](?!\])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// 개행 문자('\r' 또는 '\n') 시작 위치를 찾는다.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int IndexOfNewLine(this string str)
        {
            var crPos = str.IndexOf('\r');
            var lfPos = str.IndexOf('\n');

            if (crPos >= 0 && lfPos >= 0) return crPos < lfPos ? crPos : lfPos;
            else if (crPos == -1 && lfPos >= 0) return lfPos;
            else if (crPos >= 0 && lfPos == -1) return crPos;

            return -1;
        }

        /// <summary>
        /// 개행 문자('\r' 또는 '\n') 시작 위치를 문자열 뒤에서부터 찾는다.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int LastIndexOfNewLine(this string str, int startIndex = -1)
        {
            int crPos;
            int lfPos;

            if (startIndex < 0)
            {
                crPos = str.LastIndexOf('\r');
                lfPos = str.LastIndexOf('\n');
            }
            else
            {
                crPos = str.LastIndexOf('\r', startIndex);
                lfPos = str.LastIndexOf('\n', startIndex);
            }

            if (crPos >= 0 && lfPos >= 0) return crPos < lfPos ? crPos : lfPos;
            else if (crPos == -1 && lfPos >= 0) return lfPos;
            else if (crPos >= 0 && lfPos == -1) return crPos;

            return -1;
        }

        const char BRACE_OPEN = '{';
        const char BRACE_CLOSE = '}';
        const char BRACE_OPEN_TMP = '~';
        const char BRACE_CLOSE_TMP = '`';

        /// <summary>
        /// 중괄호 '{', '}' 를 각각 임시 문자 '~', '`' 로 치환한다.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ReplaceBraceToTemporaryCharacter(this string str)
            => new StringBuilder(str)
                .Replace(BRACE_OPEN, BRACE_OPEN_TMP)
                .Replace(BRACE_CLOSE, BRACE_CLOSE_TMP)
                .ToString();

        /// <summary>
        /// 임시 문자 '~', '`' 를 각각 중괄호 '{', '}' 로 치환한다.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ReplaceTemporaryCharacterToBrace(this string str)
            => new StringBuilder(str)
                .Replace(BRACE_OPEN_TMP, BRACE_OPEN)
                .Replace(BRACE_CLOSE_TMP, BRACE_CLOSE)
                .ToString();

        /// <summary>
        /// 문자열의 개별 줄 시작 부분에 indent 를 추가한다.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="indent"></param>
        /// <returns></returns>
        public static string InjectIndent(this string str, string indent)
            => new StringBuilder(str)
                .Insert(0, indent)
                .Replace("\r\n", "~")
                .Replace("\r", "~")
                .Replace("\n", "~")
                .Replace("~", $"\r\n{indent}")
                .ToString();

        public static string CombinePath(params string[] paths)
            => System.IO.Path.Combine(paths).Replace('\\', '/');

        public static string ToFriendlyTypeName(this string source)
            => source.ToLower() switch
            {
                "int64" => "long",
                "int32" => "int",
                "uint64" => "ulong",
                "uint32" => "uint",
                "integer" => "int",
                "date-time" => "DateTime",
                "boolean" => "bool",
                _ => source
            };

        public static string ToTargetName(this string reference)
        {
            var pos = reference.LastIndexOf('/');
            if (pos < 0) return reference;

            return reference.Substring(pos + 1);
        }

        public static bool TryGetTargetName(this string reference, out string value)
        {
            value = null;

            var pos = reference.LastIndexOf('/');
            if (pos < 0) return false;

            value = reference.Substring(pos + 1);

            return true;
        }

        public static string ToCapitalFirstCharacter(this string str, bool restLettersLowercase = true)
            => string.Concat(char.ToUpper(str[0]), restLettersLowercase ? str.ToLower().Substring(1) : str.Substring(1));
    }

    public class Indent
    {
        private string _indent;

        public Indent(int initialIndentLevel = 0)
        {
            _indent = new string('\t', initialIndentLevel);
        }

        public void Push() => _indent += "\t";

        public void Pop() => _indent = _indent.Remove(0, 1);

        public static implicit operator string(Indent indent) => indent._indent;

        public override string ToString() => _indent;
    }

    public static partial class JsonExtensions
    {
        public static string SafeToString(this JObject jobject, string key)
        {
            if (!(jobject?.ContainsKey(key) ?? false)) return string.Empty;

            return jobject[key].ToString();
        }

        public static JArray SafeToArray(this JObject jobject, string key)
        {
            if (!(jobject?.ContainsKey(key) ?? false) || jobject[key] is not JArray jArr) return new JArray();

            return jArr;
        }

        public static string SafeToString(this string str)
        {
            return string.IsNullOrEmpty(str) ? "(empty)" : str;
        }

        public static IEnumerable<KeyValuePair<string, JToken>> ToEnumerable(this JObject jobject)
        {
            foreach (var pair in jobject)
            {
                yield return pair;
            }
        }

        /// <summary>
        /// Json의 특정 토큰 경로에 해당하는 내용을 <typeparamref name="T"/> 타입으로 파싱한다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json">Json 텍스트</param>
        /// <param name="path">특정 토큰 경로 ((예)'$.aaa.bbb')</param>
        /// <param name="parser">parser 본체</param>
        /// <returns></returns>
        public static IEnumerable<T> Parse<T>(this string json, string path = null, Func<JToken, string, T> parser = null)
        {
            var jToken = JToken.Parse(json);
            if (jToken == null) yield break;

            if (!string.IsNullOrEmpty(path)) jToken = jToken.SelectToken(path);

            if (parser == null)
            {
                yield return JsonConvert.DeserializeObject<T>(jToken.ToString());
                yield break;
            }

            if (jToken is JArray jArr)
            {
                for (int i = 0, cnt = jArr.Count; i < cnt; i++)
                {
                    yield return parser(jArr[i], null);
                }
            }
            else if (jToken is JObject jObj)
            {
                foreach (var pair in jObj)
                {
                    yield return parser(pair.Value, pair.Key);
                }
            }
            else
            {
                yield return parser(jToken, null);
            }
        }
    }
}
