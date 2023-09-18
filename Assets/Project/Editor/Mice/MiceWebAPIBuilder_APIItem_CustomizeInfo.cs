/*===============================================================
* Product:		Com2Verse
* File Name:	MiceWebAPIBuilder_APIItem_CustomizeInfo.cs
* Developer:	sprite
* Date:			2023-04-11 14:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.Logger;
using System;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Com2VerseEditor.Mice
{
    public partial class APIItem
    {
        public static class Customize
        {
            public static readonly string TEMPLATE =
@"[Token.GenerateGet]
_responseClassName=TokenResult

[Token.GenerateGet/TokenResult]
public class TokenResult
{
    [[JsonProperty]]
    public long accountId { get; set; }

    [[JsonProperty]]
    public string? c2vAccessToken { get; set; }

    [[JsonProperty]]
    public string? c2vRefreshToken { get; set; }
}
";
            public static void HardCoded_CheatParameters(StringBuilder sb, Indent indent, string category, string apiName)
            {
                // Token.GenerateGet 일 경우, 결과값의 AccessToken을 Client에 설정하는 코드 추가.
                if (category == "Token" && apiName == "GenerateGet")
                {
                    sb.Append($",\r\n{indent}{MiceWebAPIBuilder.MICEWEBTEST_3TH_PARAM_NAME}: r =>");
                    sb.Append($"\r\n{indent}{{");
                    sb.Append($"\r\n{indent}\tNetwork.User.Instance.AccessToken = r.Data.c2vAccessToken;");
                    sb.Append($"\r\n{indent}\tHttpHelper.Client.Auth.SetTokenAuthentication(HttpHelper.Util.MakeTokenAuthInfo(r.Data.c2vAccessToken));");
                    sb.Append($"\r\n{indent}}}");
                }
            }
        }

        private static Dictionary<string, Dictionary<string, CustomizeInfo>> customizeMap;

        public static void InitCustomize()
        {
            StringBuilder sbLog = new StringBuilder();
            sbLog.AppendLine($"[MICEWebTest]<Customize> Begin.");

            IEnumerable<CustomizeInfo> Parse()
            {
                var src = Customize.TEMPLATE;

                Match match;
                var matches = Helper.regexSquareBrackets.Matches(src);
                sbLog.AppendLine($"[MICEWebTest]<Customize> '[~]' ({matches?.Count ?? 0}) matches found.");
                for (int i = 0, cnt = matches.Count; i < cnt; i++)
                {
                    match = matches[i];

                    var lastPos = src.Length;
                    if (i + 1 < cnt) lastPos = matches[i + 1].Index;
                    var section = src.Substring(match.Index, lastPos - match.Index);

                    var sectionInfo = CustomizeInfo.ParseSection(section, sbLog);

                    sbLog.AppendLine($"[MICEWebTest]<Customize> section [{sectionInfo._category}.{sectionInfo._apiName}{(!string.IsNullOrEmpty(sectionInfo._itemValue) ? $"/{sectionInfo._itemValue}" : "")}]");
                    if (!sectionInfo) continue;

                    var info = CustomizeInfo.Create(sectionInfo, sbLog);
                    if (info == null) continue;

                    yield return info;
                }
            }

            customizeMap = Parse()
                .GroupBy(e => e._category)
                .ToDictionary
                (
                    e => e.Key,
                    e => e.GroupBy(el => el._apiName)
                          .ToDictionary
                          (
                              el => el.Key,
                              el =>
                              {
                                  var root = el.First(ele => !ele._hasContents);
                                  var contentsList = el.Where(ele => ele._hasContents);
                                  root.AppendContents(contentsList, sbLog);
                                  return root;
                              }
                          )
                );

            sbLog.AppendLine($"[MICEWebTest]<Customize> Done.");

            C2VDebug.Log(sbLog.ToString());
        }

        public static void ApplyCustomize(Dictionary<string, List<APIItem>> apiMap)
        {
            foreach (var pair in apiMap)
            {
                var category = pair.Key;

                if (!customizeMap.TryGetValue(category, out var customizeApiMap)) continue;

                foreach (var item in pair.Value)
                {
                    var apiName = item._exportAPIName;

                    if (!customizeApiMap.TryGetValue(apiName, out var info)) continue;

                    info.Customize(item);
                }
            }
        }

        public class CustomizeInfo
        {
            public string _category { get; private set; }
            public string _apiName { get; private set; }
            public string _itemValue { get; private set; }
            public string _contents { get; private set; }

            public bool _hasContents => !string.IsNullOrEmpty(_contents);

            public class ItemInfo
            {
                public string _value;
                public string _contents;
            }

            private Dictionary<string, ItemInfo> _itemMap;

            public void Customize(APIItem item)
            {
                var type = typeof(APIItem);

                foreach (var pair in _itemMap)
                {
                    var members = type.GetMember(pair.Key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (members == null || members.Length == 0) continue;

                    foreach (var member in members)
                    {
                        switch (member)
                        {
                            case FieldInfo field when member is FieldInfo: field.SetValue(item, pair.Value._value); break;
                            case PropertyInfo prop when member is PropertyInfo: prop.SetValue(item, pair.Value._value); break;
                        }
                    }
                }

                if (this._hasContents)
                {
                    item._customizeContents = this._contents;
                }
            }

            public struct SectionInfo
            {
                public bool _result;
                public string _category;
                public string _apiName;
                public string _itemValue;
                public string[] _lines;

                public bool _hasContent => !string.IsNullOrEmpty(_itemValue);

                public static implicit operator bool(SectionInfo info) => info._result;
            }

            public static SectionInfo ParseSection(string sectionText, StringBuilder sbLog)
            {
                SectionInfo result = new SectionInfo()
                {
                    _result = false,
                    _category = null,
                    _apiName = null,
                    _itemValue = null,
                    _lines = null
                };

                var lines = sectionText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines == null || lines.Length == 0)
                {
                    sbLog.AppendLine($"[MICEWebTest]<Customize><CustomizeInfo> empty section text.");
                    return result;
                }

                var caption = lines[0].Replace("[", "").Replace("]", "");
                var subNames = caption.Split('.');
                if (subNames == null || subNames.Length < 2)
                {
                    sbLog.AppendLine($"[MICEWebTest]<Customize><CustomizeInfo> Invalid section title.");
                    return result;
                }

                result._result = true;
                result._category = subNames[0].Trim();
                result._apiName = subNames[1].Trim();
                result._itemValue = null;
                result._lines = lines;

                var strs = result._apiName.Split('/');
                if (strs.Length >= 2)
                {
                    result._apiName = strs[0].Trim();
                    result._itemValue = strs[1].Trim();
                }

                return result;
            }

            public static CustomizeInfo Create(SectionInfo sectionInfo, StringBuilder sbLog)
            {
                if (!sectionInfo)
                {
                    sbLog.AppendLine($"[MICEWebTest]<Customize><CustomizeInfo> Invalid section info.");
                    return null;
                }

                CustomizeInfo info = new CustomizeInfo();
                info._category = sectionInfo._category;
                info._apiName = sectionInfo._apiName;
                info._itemValue = null;
                info._contents = null;
                info._itemMap = null;

                sbLog.AppendLine($"[MICEWebTest]<Customize><CustomizeInfo> category: {info._category}, api: {info._apiName}");

                if (sectionInfo._hasContent)
                {
                    info._itemValue = sectionInfo._itemValue;
                    info._contents = sectionInfo._lines.Skip(1).Aggregate((a, b) => $"{a}\r\n{b}");

                    sbLog.AppendLine($"[MICEWebTest]<Customize><CustomizeInfo> contents\r\n{info._contents}");
                }
                else
                {
                    IEnumerable<(string propertyName, string value)> ParseItems()
                    {
                        for (int i = 1, cnt = sectionInfo._lines.Length; i < cnt; i++)
                        {
                            var line = sectionInfo._lines[i];

                            var subTxt = line.Split('=');
                            if (subTxt == null || subTxt.Length <= 1)
                            {
                                sbLog.AppendLine($"[MICEWebTest]<Customize><CustomizeInfo> Invalid Item. [{line.Trim()}]");
                                continue;
                            }

                            sbLog.AppendLine($"[MICEWebTest]<Customize><CustomizeInfo> ({i:00}) '{subTxt[0]}' = '{subTxt[1]}'");

                            yield return (subTxt[0], subTxt[1]);
                        }
                    }

                    info._itemMap = ParseItems()
                        .ToDictionary
                        (
                            e => e.propertyName,
                            e => new ItemInfo()
                            {
                                _value = e.value,
                                _contents = null
                            }
                        );
                }

                return info;
            }

            public void AppendContents(IEnumerable<CustomizeInfo> contentsList, StringBuilder sbLog)
            {
                StringBuilder sb = new StringBuilder(_contents);

                foreach (var item in contentsList)
                {
                    if (!item._hasContents) continue;

                    sbLog.AppendLine($"[MICEWebTest]<Customize><CustomizeInfo> Merged from [{item._category}.{item._apiName}/{item._itemValue}] to [{this._category}.{this._apiName}].");

                    sb.AppendLine(item._contents);
                }

                _contents = sb.ToString();
            }
        }
    }
}
