/*===============================================================
* Product:		Com2Verse
* File Name:	MiceWebAPIBuilder_APIItem.cs
* Developer:	sprite
* Date:			2023-04-11 15:12
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

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
    public partial class APIItem
    {
        private static readonly string DEVELOPMENT_API_PREFIX = "/dev";

        public string _api;
        public string _restType;    // post, get, put, delete
        public string[] _tags;
        public string _summary;
        public string _description;
        public List<ParamInfo> _parameters;
        public ResponseInfo _responses;
        public string _responseClassName;
        public bool _responseIsArray;
        public bool _responseIsEnum;
        public string _apiName;
        public APIItem _next;
        public string _customizeContents;
        public bool _apiForDevelopment;
        public string _tag => _tags[0];
        public RequestInfo _request;
        public string _requestClassName;
        public bool _requestIsPrimitiveType;

        public List<ResponseClassInfo> _responseClassInfoList;

        private static readonly string PAYLOAD_VAR_NAME = "payload";
        public string GetRequestPayloadParameter(out string payloadRelay)
        {
            payloadRelay = "";

            var hasPayload = !string.IsNullOrEmpty(this._requestClassName);
            var hasParameters = this._parameters != null && this._parameters.Count > 0;

            if (hasPayload)
            {
                if (this._requestIsPrimitiveType)
                {
                    if (string.Equals(this._requestClassName, "string", StringComparison.OrdinalIgnoreCase))
                    {
                        payloadRelay = $@", !string.IsNullOrEmpty({PAYLOAD_VAR_NAME}) ? {PAYLOAD_VAR_NAME} : """"";
                    }
                    else
                    {
                        if (string.Equals(this._requestClassName, "object", StringComparison.OrdinalIgnoreCase))
                        {
                            payloadRelay = $@", {PAYLOAD_VAR_NAME}?.ToString() ?? """"";
                        }
                        else
                        {
                            payloadRelay = $@", {PAYLOAD_VAR_NAME}.HasValue ? {PAYLOAD_VAR_NAME}.ToString() : """"";
                        }
                    }
                }
                else
                {
                    payloadRelay = $@", JsonConvert.SerializeObject({PAYLOAD_VAR_NAME})";
                }
            }

            return hasPayload
                ? $"{(hasParameters ? ", " : "")}{(this._requestIsPrimitiveType ? "" : $"{MiceWebAPIBuilder.MICE_WEB_API_GENERATED_ENTITIES}.")}" +
                  $"{this._requestClassName}? {PAYLOAD_VAR_NAME} = null"
                : "";
        }

        public string _exportAPIName => string.Format(_apiName, _restType.ToCapitalFirstCharacter());// $"{_apiName}{_restType.ToCapitalFirstCharacter()}";

        public string GetResponseClassName(out bool isArray, out bool isEnum)
        {
            isArray = false;
            isEnum = false;

            if
            (
                _responses == null ||
                !_responses.resultMap.TryGetValue((int)System.Net.HttpStatusCode.OK, out var result)
            )
            {
                return null;
            }

            do
            {
                isEnum = result.isEnum;

                var type = result.contentType;
                if (!string.IsNullOrEmpty(type))
                {
                    isArray = string.Equals(type, "array", StringComparison.OrdinalIgnoreCase);
                }

                var refr = result.contentRef;
                if (string.IsNullOrEmpty(refr) || !refr.TryGetTargetName(out var value))
                {
                    break;
                }
                
                return value;
            } while (false);

            return null;
        }

        public string GetRequestClassName(out bool isPrimitiveType)
        {
            isPrimitiveType = false;

            if
            (
                _request == null ||
                (_request.content?.Count ?? 0) == 0
            )
            {
                return null;
            }

            var result = _request.content.Values.First();

            if (!string.IsNullOrEmpty(result.contentRef) && result.contentRef.TryGetTargetName(out var targetName)) return targetName;
            else if (!string.IsNullOrEmpty(result.contentType))
            {
                isPrimitiveType = true;
                return result.contentType.ToFriendlyTypeName();
            }

            return null;
        }

        public static IEnumerable<APIItem> Parse(string json, List<MiceWebAPIBuilder.EnumComponentInfo> components)
        {
            return json.Parse("$.paths", InternalParse);

            APIItem InternalParse(JToken jToken, string api)
            {
                UnityEngine.Assertions.Assert.IsTrue(jToken is JObject);

                var jobj = jToken as JObject;
                var info = new APIItem();
                var root = info;
                var prev = default(APIItem);

                info.SetAPI(api);

                foreach (var pair in jobj)
                {
                    // API가 여러개 일 경우 링크드리스트를 구성한다. (one-way linked list)
                    if (prev != null && prev._restType != pair.Key)
                    {
                        info = new APIItem();
                        info.SetAPI(api);
                        prev._next = info;
                    }

                    info._restType = pair.Key;
                    var contents = pair.Value as JObject;
                    info._tags = contents["tags"].Select(e => e.ToString()).ToArray();
                    info._summary = contents.SafeToString("summary");
                    info._description = contents.SafeToString("description");
                    info._parameters = ParamInfo.Parse(contents.SafeToArray("parameters")).ToList();
                    info._responses = ResponseInfo.Parse(contents["responses"] as JObject, components);
                    info._request = RequestInfo.Parse(contents["requestBody"] as JObject);
                    info._responseClassName = info.GetResponseClassName(out info._responseIsArray, out info._responseIsEnum);
                    info._requestClassName = info.GetRequestClassName(out info._requestIsPrimitiveType);
                    info._responseClassInfoList = ResponseClassInfo.From(info);
                    prev = info;
                }

                return root;
            }
        }

        public static string Report(IEnumerable<APIItem> items)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in items)
            {
                sb.AppendLine($"[MICEWebTest] {item._api}");

                item.InternalReport(item._api, sb);
            }

            return sb.ToString();
        }

        private void InternalReport(string api, StringBuilder sb)
        {
            sb.AppendLine($"> [{_restType}] {api}");
            sb.AppendLine($"> tag               = {_tag}");
            sb.AppendLine($"> summary           = {_summary.SafeToString()}");
            sb.AppendLine($"> description       = {_description.SafeToString()}");
            ParamInfo.Report(_parameters, sb);
            _request.Report(sb);
            sb.AppendLine($"> requestClassName = '{(_requestClassName ?? "(empty)")}'");
            _responses.Report(sb);
            sb.AppendLine($"> responseClassName = '{(_responseClassName ?? "(empty)")}'");
            ResponseClassInfo.Report(_responseClassInfoList, sb);
        }

        private void SetAPI(string api)
        {
            var matches = Helper.regexBrace.Matches(api);

            var tmp = api;
            for (int i = matches.Count - 1; i >= 0; i--)
            {
                var match = matches[i];
                api = $"{api.Substring(0, match.Index)}{{{i}}}{api.Substring(match.Index + match.Length)}";
                tmp = $"{tmp.Substring(0, match.Index - 1)}{tmp.Substring(match.Index + match.Length)}";
            }

            string footer = string.Empty;
            if (matches.Count > 0)
            {
                footer = "_" + matches.Select(e => e.Value.Trim('{', '}').ToCapitalFirstCharacter(false)).Aggregate((a, b) => $"{a}_{b}");
            }

            _api = api;

            _apiName = tmp.Substring(tmp.LastIndexOf('/') + 1);
            _apiName = $"{char.ToUpper(_apiName[0])}{_apiName.Substring(1)}{{0}}{footer}";

            _apiForDevelopment = api.Contains(DEVELOPMENT_API_PREFIX);
        }

        public string MakeRequestURL()
        {
            StringBuilder sb = new StringBuilder();

            var api = _api;
            var matches = Helper.regexBrace.Matches(api);
            for (int i = matches.Count - 1; i >= 0; i--)
            {
                var match = matches[i];

                api = $"{api.Substring(0, match.Index)}{{{_parameters[i].name}}}{api.Substring(match.Index + match.Length)}";
            }

            sb.Append(api);

            return sb.ToString();
        }

        public string ConvertDescriptionToCSRecommendedXMLTags(string indent = null)
        {
            if (string.IsNullOrEmpty(_description)) return string.Empty;

            var strs = _description.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return strs
                .Select(e => $"{indent}/// <para>{e.Replace("<br>", "").Replace("<br/>", "").Trim()}</para>")
                .Aggregate((a, b) => $"{a}\r\n{b}");
        }

#region Response Class Info
        public class ResponseClassInfo
        {
            public string _name;
            public bool _isArray;
            public bool _isEnum;
            public int _resultCode;

            public static List<ResponseClassInfo> From(APIItem info)
            {
                return Internal().ToList();

                IEnumerable < ResponseClassInfo > Internal()
                {
                    int resultCode;
                    string className;
                    bool isArray = false;
                    bool isEnum;

                    foreach (var pair in info._responses.resultMap)
                    {
                        do
                        {
                            resultCode = pair.Key;
                            isEnum = pair.Value.isEnum;

                            var type = pair.Value.contentType;
                            if (!string.IsNullOrEmpty(type))
                            {
                                isArray = string.Equals(type, "array", StringComparison.OrdinalIgnoreCase);
                            }

                            var refr = pair.Value.contentRef;
                            if (string.IsNullOrEmpty(refr) || !refr.TryGetTargetName(out className))
                            {
                                break;
                            }

                            yield return new ResponseClassInfo()
                            {
                                _name = className,
                                _isArray = isArray,
                                _isEnum = isEnum,
                                _resultCode = resultCode
                            };
                        } while (false);
                    }
                }
            }

            public static void Report(List<ResponseClassInfo> responseClassInfoList, StringBuilder sb)
            {
                var cnt = responseClassInfoList.Count;
                sb.AppendLine($"> response class info ({cnt})");

                for (int i = 0; i < cnt; i++)
                {
                    var info = responseClassInfoList[i];

                    sb.AppendLine($"    [{info._resultCode}] = {info._name} [isArray:{info._isArray}, isEnum:{info._isEnum}]");
                }
            }
        }
#endregion // Response Class Info
    }

    /// <summary>
    /// Json Object Class
    /// </summary>
    [Serializable]
    public class ParamInfo
    {
        [Serializable]
        public class Schema
        {
            [JsonProperty] public string @ref { get; set; }
            [JsonProperty] public string type { get; set; }
            [JsonProperty] public string format { get; set; }
            [JsonIgnore] public bool isCustomType => !string.IsNullOrEmpty(this.@ref);

            public string GetTypeName()
            {
                if (!string.IsNullOrEmpty(this.format)) return this.format;
                return this.type;
            }

            public string GetFriendlyTypeName()
            {
                if (this.isCustomType)
                {
                    return this.@ref.ToTargetName();
                }

                return this.GetTypeName().ToFriendlyTypeName();
            }

            public void Report(StringBuilder sb)
            {
                if (this.isCustomType)
                {
                    sb.AppendLine($"$ref: '{this.@ref}'");
                }
                else
                {
                    sb.Append($"(type = {this.type}, format = ");
                    if (!string.IsNullOrEmpty(this.format))
                    {
                        sb.AppendLine($"{this.format})");
                    }
                    else
                    {
                        sb.AppendLine("(empty))");
                    }
                }
            }

            public static bool IsReferenceType(Schema schema)
                => schema.isCustomType ||
                    schema.GetTypeName().ToLower() switch
                    {
                        "string" => true,
                        _ => false
                    };

            public static bool IsStringType(Schema schema)
                => !schema.isCustomType &&
                    string.Compare(schema.GetTypeName().ToLower(), "string", true) == 0;
        }

        [JsonProperty] public string name { get; set; }
        [JsonProperty] public string @in { get; set; }
        [JsonProperty] public bool required { get; set; }
        [JsonProperty] public Schema schema { get; set; }

        public void Report(StringBuilder sb)
        {
            sb.AppendLine($"    [name]          = {this.name}");
            sb.AppendLine($"    [in]            = {this.@in}");
            sb.AppendLine($"    [required]      = {this.required}");
            sb.Append($"    [schema]        = ");
            this.schema.Report(sb);
        }

        public static IEnumerable<ParamInfo> Parse(JArray jArr)
        {
            for (int i = 0, cnt = jArr.Count; i < cnt; i++)
            {
                var result = JsonConvert.DeserializeObject<ParamInfo>
                (
                    jArr[i].ToString().Replace("$", "")  // '$ref' -> 'ref'
                );

                if (result != null)
                {
                    yield return result;
                }
            }
        }

        public static void Report(IEnumerable<ParamInfo> items, StringBuilder sb)
        {
            if (items == null)
            {
                sb.AppendLine($"> No parameters.");
            }
            else
            {
                int i = 0;
                foreach (var item in items)
                {
                    if (item == null)
                    {
                        sb.AppendLine($"> [Parameter] [{i:000}] (null)");
                    }
                    else
                    {
                        sb.AppendLine($"> [Parameter] [{i:000}]");
                        item.Report(sb);
                    }

                    i++;
                }
            }
        }
        
        private static string GetParamType(ParamInfo info, bool allParamTypesChangeToString = false)
        {
            if (allParamTypesChangeToString)
            {
                return "string";
            }

            return $"{info.schema.GetFriendlyTypeName()}{(info.required ? "" : "?")}";
        }

        public static string MakeInvokeParameterList(List<ParamInfo> items, bool allParamTypesChangeToString = false, Func<string, bool, bool, string> defaultParam = null)
        {
            if (items == null || items.Count == 0)
            {
                return string.Empty;
            }
            else
            {
                static string DefaultParam(string _0, bool _1, bool _2) => " = null";

                if (defaultParam == null) defaultParam = DefaultParam;


                return items
                    .Select
                    (
                        e =>
                        {
                            var paramType = GetParamType(e, allParamTypesChangeToString);
                            var isReferenceType = ParamInfo.Schema.IsReferenceType(e.schema);
                            var isStringType = ParamInfo.Schema.IsStringType(e.schema);
                            return $"{paramType} {e.name}{(e.required ? "" : defaultParam(paramType, isReferenceType, isStringType))}";
                        }
                    )
                    .Aggregate((a, b) => $"{a}, {b}");
            }
        }

        public static IEnumerable<string> MakeParameterTypeNames(List<ParamInfo> items)
        {
            for (int i = 0, cnt = items?.Count ?? 0; i < cnt; i++)
            {
                yield return GetParamType(items[i]);
            }
        }

        public static List<ParamInfo> GetNullableParameters(List<ParamInfo> items)
        {
            if (items == null || items.Count == 0) return new List<ParamInfo>();

            return items.Where(e => !e.required).ToList();
        }
    }

    public class ResponseInfo
    {
        [Serializable]
        public class Result
        {
            [Serializable]
            public class Schema
            {
                [Serializable]
                public class Items
                {
                    [JsonProperty] public string @ref { get; set; }

                    public void Report(StringBuilder sb)
                    {
                        sb.AppendLine($"$ref: '{this.@ref}')");
                    }
                }

                [JsonProperty] public string @ref { get; set; }
                [JsonProperty] public string type { get; set; }
                [JsonProperty] public Items items { get; set; }

                public void Report(StringBuilder sb)
                {
                    if (!string.IsNullOrEmpty(this.@ref))
                    {
                        sb.AppendLine($"$ref: '{this.@ref}'");
                    }
                    else
                    {
                        sb.Append($"(type = {(string.IsNullOrEmpty(this.type) ? "(empty)" : this.type)}, items = ");
                        if (this.items == null)
                        {
                            sb.AppendLine("(empty))");
                        }
                        else
                        {
                            this.items.Report(sb);
                        }
                    }
                }

                public string GetReference()
                {
                    if (!string.IsNullOrEmpty(this.@ref)) return this.@ref;
                    if (this.items != null) return this.items.@ref;

                    return null;
                }
            }

            [Serializable]
            public class Content
            {
                [Serializable]
                public class Json
                {
                    [JsonProperty] public Schema schema { get; set; }

                    public void Report(StringBuilder sb)
                    {
                        if (this.schema == null)
                        {
                            sb.AppendLine("(null)");
                        }
                        else
                        {
                            this.schema.Report(sb);
                        }
                    }
                }

                [JsonProperty("application/json")] public Json contentJson { get; set; }

                public void Report(StringBuilder sb)
                {
                    this.contentJson.Report(sb);
                }
            }


            [JsonProperty] public string description { get; set; }
            [JsonProperty] public Content content { get; set; }

            [JsonIgnore] public string contentType => this.content?.contentJson?.schema?.type;
            [JsonIgnore] public string contentRef => this.content?.contentJson?.schema?.GetReference();
            [JsonIgnore] public bool isEnum { get; private set; }

            public void Report(StringBuilder sb)
            {
                sb.AppendLine($"    [description]   = {this.description} ");
                sb.Append($"    [content]       = ");
                if (this.content == null)
                {
                    sb.AppendLine("(empty)");
                }
                else
                {
                    this.content.Report(sb);
                }
                sb.AppendLine($"    [reference]     = Enum:{this.isEnum}");
            }

            public void CheckIsEnum(List<MiceWebAPIBuilder.EnumComponentInfo> components)
            {
                this.isEnum = false;

                if (string.IsNullOrEmpty(this.contentRef) || !this.contentRef.TryGetTargetName(out var targetName)) return;

                this.isEnum = components.Any(e => string.Equals(e._name, targetName));
            }
        }

        public Dictionary<int, Result> resultMap { get; private set; }

        public static ResponseInfo Parse(JObject jObj, List<MiceWebAPIBuilder.EnumComponentInfo> components)
        {
            ResponseInfo info = new ResponseInfo();
            info.resultMap = new Dictionary<int, Result>(1);

            foreach (var pair in jObj)
            {
                if (!int.TryParse(pair.Key, out var resultCode)) continue;

                var result = JsonConvert.DeserializeObject<Result>
                (
                    pair.Value.ToString().Replace("$", "")  // '$ref' -> 'ref'
                );

                result.CheckIsEnum(components);

                info.resultMap.Add(resultCode, result);
            }

            return info;
        }

        public void Report(StringBuilder sb)
        {
            foreach (var pair in this.resultMap)
            {
                sb.AppendLine($"> [Responses] ({pair.Key})");
                pair.Value.Report(sb);
            }
        }
    }
    
    public class RequestInfo
    {
        public class ContentInfo
        {
            [Serializable]
            public class Json
            {
                [Serializable]
                public class Schema
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

                    [JsonProperty] public string @ref { get; set; }
                    [JsonProperty] public string type { get; set; }
                    [JsonProperty] public string format { get; set; }
                    [JsonProperty] public Items items { get; set; }


                    public void Report(StringBuilder sb)
                    {
                        if (!string.IsNullOrEmpty(this.type))
                        {
                            sb.AppendLine($"type = {this.type}");
                        }

                        if (!string.IsNullOrEmpty(this.@ref))
                        {
                            sb.AppendLine($"$ref: '{this.@ref}'");
                        }
                        else
                        {
                            sb.Append($"(type = {(string.IsNullOrEmpty(this.type) ? "(empty)" : this.type)}, items = ");
                            if (this.items == null)
                            {
                                sb.AppendLine("(empty))");
                            }
                            else
                            {
                                this.items.Report(sb);
                            }
                        }

                        if (string.IsNullOrEmpty(this.type) && string.IsNullOrEmpty(this.@ref))
                        {
                            sb.AppendLine($"(contents empty)");
                        }
                    }

                    public string GetReference()
                    {
                        if (!string.IsNullOrEmpty(this.@ref))
                        {
                            return this.@ref;
                        }
                        if (this.items != null)
                        {
                            return this.items.@ref;
                        }

                        return null;
                    }
                }

                [JsonProperty] public Schema schema { get; set; }

                public void Report(StringBuilder sb)
                {
                    if (this.schema == null)
                    {
                        sb.AppendLine("(null)");
                    }
                    else
                    {
                        this.schema.Report(sb);
                    }
                }
            }

            [JsonProperty] public Json contentJson { get; set; }

            [JsonIgnore]
            public string contentType
            {
                get
                {
                    var format = contentJson?.schema?.format;
                    if (!string.IsNullOrEmpty(format)) return format;

#region Items
                    format = contentJson?.schema?.items?.format;
                    if (!string.IsNullOrEmpty(format)) return format;

                    var type = contentJson?.schema?.items?.type;
                    if (!string.IsNullOrEmpty(type)) return type;
#endregion // Items

                    return contentJson?.schema?.type;
                }
            }

            [JsonIgnore] public string contentRef => contentJson?.schema?.GetReference();

            public void Report(StringBuilder sb)
            {
                if (this.contentJson == null)
                {
                    sb.AppendLine("(null)");
                }
                else
                {
                    this.contentJson.Report(sb);
                }
            }
        }

        public Dictionary<string, ContentInfo> content;


        public void Report(StringBuilder sb)
        {
            sb.AppendLine($"> [Request] {(this.content == null || this.content.Count == 0 ? "(empty)" : "")}");

            foreach (var pair in this.content)
            {
                sb.Append($"    [content]({pair.Key}) = ");
                pair.Value.Report(sb);
            }
        }

        public static RequestInfo Parse(JObject jobj)
        {
            RequestInfo info = new RequestInfo();
            info.content = new Dictionary<string, ContentInfo>(3);

            if (jobj == null || !jobj.ContainsKey("content")) return info;

            ContentInfo contentInfo;

            var subObj = jobj["content"] as JObject;

            foreach (var pair in subObj)
            {

                contentInfo = new ContentInfo();
                contentInfo.contentJson = JsonConvert.DeserializeObject<ContentInfo.Json>
                (
                    pair.Value.ToString().Replace("$", "")  // '$ref' -> 'ref'
                );

                info.content.Add(pair.Key, contentInfo);
            }

            return info;
        }
    }
}
