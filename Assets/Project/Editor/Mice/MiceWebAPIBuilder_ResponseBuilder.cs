/*===============================================================
* Product:		Com2Verse
* File Name:	MiceWebAPIBuilder_ResponseBuilder.cs
* Developer:	sprite
* Date:			2023-04-11 15:38
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System;
using Newtonsoft.Json;
using System.Text;
using System.Linq;

namespace Com2VerseEditor.Mice
{
    public sealed partial class MiceWebAPIBuilder   // Parse Response Classes. (from 'swagger.json')
    {
        private static readonly string GENERATED_COMPONENTS_CS_TEMPLATE_HEADER =
@"using System;
using System.Collections.Generic;
using Newtonsoft.Json;
";

        private static void ExportResponse(List<ComponentInfo> components)
        {
            StringBuilder sb = new StringBuilder();

            Indent indent = new Indent(2);

            var enumComponents = components
                .Where(e => e is EnumComponentInfo)
                .Select(e => e as EnumComponentInfo)
                .ToList();

            #region Export Enums
            sb.AppendLine
            (
                enumComponents
                    .Select(e => e.ExportScript(indent))
                    .Aggregate((a, b) => $"{a}\r\n{b}")
            );
            #endregion // Export Enums

            #region Export Classes
            sb.AppendLine($"{indent}public static partial class {MICE_WEB_API_GENERATED_ENTITIES}");
            sb.AppendLine($"{indent}{{");
            {
                indent.Push();

                sb.AppendLine
                (
                    components
                        .Where(e => e is ClassComponentInfo)
                        .Select(e => e.ExportScript(indent, enumComponents))
                        .Aggregate((a, b) => $"{a}\r\n{b}")
                );

                indent.Pop();
            }
            sb.AppendLine($"{indent}}}");
            #endregion // Export Classes

            var path = Helper.CombinePath(Application.dataPath, MICE_WEB_API_GENERATED_TARGET_PATH, MICE_WEB_API_GENERATED_RESPONSE_FILE);
            System.IO.File.WriteAllText(path, string.Format(GENERATED_CS_TEMPLATE_FMT, GENERATED_COMPONENTS_CS_TEMPLATE_HEADER, sb.ToString()));
        }

        public abstract class ComponentInfo
        {
            [Serializable]
            public class Property
            {
                [Serializable]
                public class Items
                {
                    [JsonProperty] public string @ref { get; set; }
                    [JsonProperty] public string type { get; set; }
                    [JsonProperty] public string format { get; set; }

                    public void Report(StringBuilder sb)
                    {
                        sb.AppendLine($"$ref: '{this.@ref}')");
                    }
                }

                [JsonProperty] public string type { get; set; }
                [JsonProperty] public Items items { get; set; }
                [JsonProperty] public bool nullable { get; set; }
                [JsonProperty] public string format { get; set; }
                [JsonProperty] public string @ref { get; set; }

                [JsonIgnore]
                public bool isEnum { get; set; }

                public string friendlyTypeName
                {
                    get
                    {
                        string GenerateFriendlyTypeName()
                        {
                            string source = this.type;

                            if (!string.IsNullOrEmpty(this.format))
                            {
                                source = this.format;
                            }
                            else if (this.items != null)
                            {
                                if (!string.IsNullOrEmpty(this.items.format))
                                {
                                    source = this.items.format.ToFriendlyTypeName();
                                }
                                else
                                {
                                    source = this.items.@ref.ToTargetName();
                                }

                                if (this.type == "array")
                                {
                                    return $"IEnumerable<{source}>";
                                }

                                return source;
                            }
                            else if
                            (
                                !string.IsNullOrEmpty(this.@ref) &&
                                this.@ref.TryGetTargetName(out source) &&
                                !string.IsNullOrEmpty(source)
                            )
                            {
                                return this.isEnum ? source : $"{source}?";
                            }

                            return source.ToFriendlyTypeName();
                        }

                        return $"{GenerateFriendlyTypeName()}{(this.nullable ? "?" : "")}";
                    }
                }
            }

            public string _name { get; private set; }
            public string _type { get; private set; }

            public ComponentInfo(string name, string type)
            {
                _name = name;
                _type = type;
            }

            public virtual void Report(StringBuilder sb)
            {
                sb.AppendLine($"> [ComponentInfo<{this.GetType().Name}>] {_name} ({_type})");
            }

            /// <summary>
            /// 스크립트 코드를 생성한다.
            /// </summary>
            /// <param name="sb"></param>
            /// <param name="indent"></param>
            public abstract void ExportScript(StringBuilder sb, string indent = null, IEnumerable<EnumComponentInfo> enumComponents = null);

            /// <summary>
            /// 스크립트 코드를 생성한다.
            /// </summary>
            /// <param name="indent"></param>
            /// <returns></returns>
            public string ExportScript(string indent = null, IEnumerable<EnumComponentInfo> enumComponents = null)
            {
                StringBuilder sb = new StringBuilder();

                this.ExportScript(sb, indent, enumComponents);

                return sb.ToString();
            }

            public static IEnumerable<ComponentInfo> Parse(string json)
            {
                return json.Parse("$.components.schemas", InternalParse);

                static ComponentInfo InternalParse(JToken jToken, string name)
                {
                    UnityEngine.Assertions.Assert.IsTrue(jToken is JObject);

                    var jobj = jToken as JObject;
                    if (jobj.ContainsKey("properties"))
                    {
                        return ClassComponentInfo.ParseFromJObject(name, jobj);
                    }
                    else if (jobj.ContainsKey("enum"))
                    {
                        return EnumComponentInfo.ParseFromJObject(name, jobj);
                    }

                    return null;
                }
            }

            public static string Report(IEnumerable<ComponentInfo> items)
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("[MICEWebTest] Components");

                foreach (var item in items)
                {
                    item.Report(sb);
                }

                return sb.ToString();
            }
        }

        public class ClassComponentInfo : ComponentInfo
        {
            public Dictionary<string, Property> _properties { get; private set; }
            public bool _additionalProperties { get; private set; }

            public ClassComponentInfo(string name, string type)
                : base(name, type)
            {

            }

            public override void Report(StringBuilder sb)
            {
                base.Report(sb);

                sb.AppendLine($"> [properties] (_additionalProperties: {_additionalProperties})");
                foreach (var pair in _properties)
                {
                    sb.AppendLine($"    [{pair.Key}]    = (type: {pair.Value.type}, nullable: {pair.Value.nullable})");
                }
            }

            public override void ExportScript(StringBuilder sb, string indent = null, IEnumerable<EnumComponentInfo> enumComponents = null)
            {
                sb.AppendLine($"{indent}public partial class {this._name}");
                sb.AppendLine($"{indent}{{");
                {
                    foreach (var pair in _properties)
                    {
                        pair.Value.isEnum = enumComponents.Any(e => !string.IsNullOrEmpty(pair.Value.@ref) && pair.Value.@ref.Contains(e._name));

                        sb.AppendLine($"{indent}\t[JsonProperty(\"{pair.Key}\")] public {pair.Value.friendlyTypeName} {pair.Key.ToCapitalFirstCharacter(restLettersLowercase: false)} {{ get; set; }}");
                    }
                }
                sb.AppendLine($"{indent}}}");
            }

            public static ClassComponentInfo ParseFromJObject(string name, JObject jobj)
            {
                var type = jobj["type"].ToString();

                ClassComponentInfo info = new ClassComponentInfo(name, type);

                var jProperties = jobj["properties"] as JObject;

                info._properties = jProperties
                    .ToEnumerable()
                    .ToDictionary
                    (
                        e => e.Key,
                        e => JsonConvert.DeserializeObject<Property>(e.Value.ToString().Replace("$", ""))      // '$ref' -> 'ref'
                    );

                var value = jobj["additionalProperties"].ToString();
                info._additionalProperties = value == "true";

                return info;
            }
        }

        public class EnumComponentInfo : ComponentInfo
        {
            public struct EnumInfo
            {
                public int _value;
                public string _alias;

                public string _valueName => !string.IsNullOrEmpty(_alias) ? _alias : $"RESERVED_{_value:000}";
            }

            public List<EnumInfo> _enum { get; private set; }

            public EnumComponentInfo(string name, string type)
                : base(name, type)
            {

            }

            public override void Report(StringBuilder sb)
            {
                base.Report(sb);

                sb.AppendLine($"> [enum]");

                if (_enum == null || _enum.Count == 0)
                {
                    sb.AppendLine($"    (empty)");
                }
                else
                {
                    sb.AppendLine
                    (
                        Enumerable
                            .Range(0, _enum.Count)
                            .Select(i => $"    [{i}] {_enum[i]._alias}    = {_enum[i]._value}")
                            .Aggregate((a, b) => $"{a} {b}")
                    );
                }
            }

            public override void ExportScript(StringBuilder sb, string indent = null, IEnumerable<EnumComponentInfo> enumComponents = null)
            {
                sb.AppendLine($"{indent}public enum {_name}");
                sb.AppendLine($"{indent}{{");
                {
                    sb.AppendLine
                    (
                        _enum
                            .Select(e => $"{indent}\t{e._valueName} = {e._value}")
                            .Aggregate((a, b) => $"{a},\r\n{b}")
                    );
                }
                sb.AppendLine($"{indent}}}");
            }

            public static EnumComponentInfo ParseFromJObject(string name, JObject jobj)
            {
                var type = jobj["type"].ToString();

                EnumComponentInfo info = new EnumComponentInfo(name, type);

                var enumValues = jobj["enum"].ToObject<int[]>();
                var jsonEnumNames = jobj["description"].ToString();

                /**
                 * 
                 * JSON의 키값이 음수(정수형) 일 경우, JObject.Parse가 Exception을 발생시키기 때문에 제외시키고, 파서를 직접 작성(ParseEnumNames)
                 * (이 위치에서의 JSON은 포멧이 정해져 있음)
                 * 
                 * (포멧 예시) "{200:"Success",-500:"Fail",-400:"InvalidValue"}"
                 *
                 * (이전 코드) var enumNames = JObject.Parse(jsonEnumNames).ToEnumerable().ToDictionary(e => int.Parse(e.Key), e => e.Value.ToString());
                 * 
                 */
                var enumNames = ParseEnumNames(jsonEnumNames);

                string GetEnumNames(int value)
                {
                    if (enumNames.TryGetValue(value, out var name)) return name;

                    return $"RESERVED_{value:000}";
                }

                info._enum = Enumerable
                    .Range(0, enumValues?.Length ?? 0)
                    .Select(index => new EnumInfo() { _value = enumValues[index], _alias = GetEnumNames(enumValues[index]) })
                    .ToList();

                return info;
            }

            private static Dictionary<int, string> ParseEnumNames(string jsonEnumNames)
            {
                IEnumerable<(int Key, string Value)> Parse()
                {
                    var strs = jsonEnumNames.Trim('{', '}').Split(',');
                    for (int i = 0, cnt = strs?.Length ?? 0; i < cnt; i++)
                    {
                        var str = strs[i];

                        var kvs = str.Split(':');
                        if (kvs == null || kvs.Length < 2 || !int.TryParse(kvs[0], out var key)) continue;

                        yield return (key, kvs[1].Trim('"'));
                    }
                }

                // 키값이 중복인 경우, 마지막 값을 취한다.
                return Parse()
                      .GroupBy(e => e.Key)  // 중복 키 그룹 생성.
                      .ToDictionary(e => e.Key, e => e.Last().Value);
            }
        }
    }
}



