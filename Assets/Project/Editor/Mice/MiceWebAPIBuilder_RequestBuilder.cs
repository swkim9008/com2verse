/*===============================================================
* Product:		Com2Verse
* File Name:	MiceWebAPIBuilder_RequestBuilder.cs
* Developer:	sprite
* Date:			2023-04-11 15:40
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using Cysharp.Threading.Tasks;
using System;
using Newtonsoft.Json;
using System.Text;
using System.Linq;

namespace Com2VerseEditor.Mice
{
    public sealed partial class MiceWebAPIBuilder   // Parse API(from 'swagger.json'), Response classes
    {
        private static readonly string GENERATED_API_CS_TEMPLATE_HEADER =
@"using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
";
        private static readonly string DEVELOPMENT_PREPROCESSOR_NAME_00 = "ENV_DEV";
        private static readonly string DEVELOPMENT_PREPROCESSOR_NAME_01 = "ENABLE_CHEATING";

        private static void ExportRequest(Dictionary<string, List<APIItem>> apiMap)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sbContents = new StringBuilder();
            StringBuilder sbNotDev = new StringBuilder();

            Indent indent = new Indent(2);

            foreach (var pair in apiMap)
            {
                sbNotDev.Clear();

                var allAPIForDevelopment = pair.Value.All(e => e._apiForDevelopment);
                if (allAPIForDevelopment)
                {
                    sb.AppendLine($"#if {DEVELOPMENT_PREPROCESSOR_NAME_00} || {DEVELOPMENT_PREPROCESSOR_NAME_01}");

                    sbNotDev.AppendLine($"{indent}public static class {pair.Key}");
                    sbNotDev.AppendLine($"{indent}{{");
                }

                sb.AppendLine($"{indent}public static class {pair.Key}");
                sb.AppendLine($"{indent}{{");
                {
                    indent.Push();

                    var appendCallerInfo = false;

#region 이름이 중복되는 API 메소드들의 중복 이름 목록을 만든다
                    var dupAPINameList = pair.Value
                        .SelectMany
                        (
                            e =>
                            {
                                if (e._next == null) return new[] { e };
                                IEnumerable<APIItem> _()
                                {
                                    var node = e;
                                    while (node != null)
                                    {
                                        yield return node;
                                        node = node._next;
                                    }
                                }
                                return _();
                            }
                        )
                        .GroupBy(e => e._exportAPIName)
                        .Where(e => e.Count() >= 2)
                        .Select(e => e.Key)
                        .ToList();
#endregion // 이름이 중복되는 API 메소드들의 중복 이름 목록을 만든다

                    foreach (var item in pair.Value)
                    {
                        if (item._next == null) // 단 하나의 API만 존재하는 경우,
                        {
                            // 중복되는 이름을 가지는 API는 CallerInfo 를 추가한다.
                            appendCallerInfo = dupAPINameList.Any(e => string.Equals(item._exportAPIName, e));

                            MiceWebAPIBuilder.BuildMethod(item, allAPIForDevelopment, indent, sb, sbContents, sbNotDev, appendCallerInfo, pair.Key);
                        }
                        else // 여러개가 존재하는 경우,
                        {
                            var node = item;
                            while (node != null)
                            {
                                // 중복되는 이름을 가지는 API는 CallerInfo 를 추가한다.
                                appendCallerInfo = dupAPINameList.Any(e => string.Equals(node._exportAPIName, e));

                                MiceWebAPIBuilder.BuildMethod(node, allAPIForDevelopment, indent, sb, sbContents, sbNotDev, appendCallerInfo, pair.Key);

                                node = node._next;
                            }
                        }
                    }

                    indent.Pop();
                }
                sb.AppendLine($"{indent}}}");

                if (allAPIForDevelopment)
                {
                    if (sbNotDev.Length > 0)
                    {
                        sb.AppendLine("#else");
                        sb.AppendLine(sbNotDev.ToString().TrimEnd());
                        sb.AppendLine($"{indent}}}");
                    }

                    sb.AppendLine("#endif");
                }

                sb.AppendLine();
            }

#region Append customize contents.
            if (sbContents.Length > 0)
            {
                sb.AppendLine($"{indent}public static partial class {MICE_WEB_API_GENERATED_ENTITIES}");
                sb.AppendLine($"{indent}{{");
                {
                    indent.Push();

                    var contents = sbContents.Insert(0, indent.ToString()).Replace("\n", $"\n{indent}").ToString().TrimEnd();
                    sb.AppendLine(contents);

                    indent.Pop();
                }
                sb.AppendLine($"{indent}}}");
            }
#endregion // Append customize contents.

            var path = Helper.CombinePath(Application.dataPath, MICE_WEB_API_GENERATED_TARGET_PATH, MICE_WEB_API_GENERATED_FILE);
            System.IO.File.WriteAllText(path, string.Format(GENERATED_CS_TEMPLATE_FMT, GENERATED_API_CS_TEMPLATE_HEADER, sb.ToString().TrimEnd()));
        }

        private static void BuildMethod(APIItem item, bool allAPIForDevelopment, Indent indent, StringBuilder sb, StringBuilder sbContents, StringBuilder sbNotDev, bool appendCallerInfo = false, string groupName = "")
        {
            if (!allAPIForDevelopment && item._apiForDevelopment) sb.AppendLine($"#if {DEVELOPMENT_PREPROCESSOR_NAME_00} || {DEVELOPMENT_PREPROCESSOR_NAME_01}");

#region Build customize contents.
            if (!string.IsNullOrEmpty(item._customizeContents))
            {
                sbContents
                    .AppendLine(item._customizeContents.TrimEnd())
                    .Replace("[[", "[")
                    .Replace("]]", "]");
            }
#endregion // Build customize contents.

            var arraySupport = item._responseIsArray ? "ArraySupport." : "MiceWebClient.";
            var arraySupportForRespons = item._responseIsArray ? "ArraySupport." : "";
            var hasResponseClassName = !item._responseIsEnum && !string.IsNullOrEmpty(item._responseClassName);
            //var responseClassGenericParamForUniTask = hasResponseClassName ? $"<{arraySupportForRespons}Response<{MICE_WEB_API_GENERATED_ENTITIES}.{item._responseClassName}>>" : "<Response>";
            var responseClassGenericParamForReturn = hasResponseClassName ? $"{arraySupportForRespons}Response<{MICE_WEB_API_GENERATED_ENTITIES}.{item._responseClassName}>" : "Response";
            var responseClassGenericParamForRESTApi = hasResponseClassName ? $"<{MICE_WEB_API_GENERATED_ENTITIES}.{item._responseClassName}>" : "";
            var requestPayload = item.GetRequestPayloadParameter(out var payloadRelay);

            var apiName = item._exportAPIName;

#region Caller Info
            var callerInfo = "";
            if (appendCallerInfo)
            {
                var paramTypeNames = ParamInfo.MakeParameterTypeNames(item._parameters).ToList();
                var genericList = paramTypeNames != null && paramTypeNames.Count > 0 ? $"<{paramTypeNames.Aggregate((a, b) => $"{a}, {b}")}>" : "";
                callerInfo = $"{(string.IsNullOrEmpty(payloadRelay) ? ", \"\", " : ", ")}new CallerInfo(\"{groupName}\", \"{apiName}\", typeof(ParamType{genericList}))";
            }
#endregion

#region Append summary text.
            sb.AppendLine($"{indent}/// <summary>");
            {
                var strs = item._summary.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (strs.Length <= 1)
                {
                    sb.AppendLine($"{indent}/// {item._summary}");
                }
                else
                {
                    for (int i = 0, cnt = strs.Length; i < cnt; i++)
                    {
                        sb.AppendLine($"{indent}/// {strs[i].Replace("<br>", "").Trim()}");
                    }
                }

                var recommend = item.ConvertDescriptionToCSRecommendedXMLTags($"{indent}");
                if (!string.IsNullOrEmpty(recommend))
                {
                    sb.AppendLine(recommend);
                }
            }
            sb.AppendLine($"{indent}/// </summary>");
#endregion // Append summary text.

            var nullableParams = ParamInfo.GetNullableParameters(item._parameters);
            if (nullableParams != null && nullableParams.Count > 0)
            {
                //sb.AppendLine($"{indent}public static UniTask{responseClassGenericParamForUniTask} {apiName}({ParamInfo.MakeInvokeParameterList(item._parameters)}{requestPayload})");
                sb.AppendLine($"{indent}public static {responseClassGenericParamForReturn} {apiName}({ParamInfo.MakeInvokeParameterList(item._parameters)}{requestPayload})");
                sb.AppendLine($"{indent}{{");
                {
                    indent.Push();

                    sb.AppendLine($"{indent}var query = MiceWebClient.ConvertNullableParametersToQuery");
                    sb.AppendLine($"{indent}(");
                    {
                        indent.Push();
                        var tmp = nullableParams
                            .Select
                            (
                                e =>
                                {
                                    var paramName = e.name;

                                    if (ParamInfo.Schema.IsReferenceType(e.schema) && ParamInfo.Schema.IsStringType(e.schema))
                                    {
                                        return $"{indent}(\"{paramName}\", () => {paramName}!, () => !string.IsNullOrEmpty({paramName}))";
                                    }
                                    else
                                    {
                                        return $"{indent}(\"{paramName}\", () => {paramName}!.Value.ToString(), () => {paramName}.HasValue)";
                                    }
                                }
                            )
                            .Aggregate((a, b) => $"{a},\r\n{b}");

                        sb.AppendLine(tmp);
                        indent.Pop();
                    }
                    sb.AppendLine($"{indent});");
                    sb.AppendLine($"{indent}return {arraySupport}{item._restType.ToUpper()}{responseClassGenericParamForRESTApi}($\"{{MiceWebClient.REST_API_URL}}{item.MakeRequestURL()}{{query}}\"{payloadRelay}{callerInfo});");

                    indent.Pop();
                }
                sb.AppendLine($"{indent}}}");
            }
            else
            {
                //sb.Append($"{indent}public static UniTask{responseClassGenericParamForUniTask} {apiName}({ParamInfo.MakeInvokeParameterList(item._parameters)}{requestPayload})");
                sb.Append($"{indent}public static {responseClassGenericParamForReturn} {apiName}({ParamInfo.MakeInvokeParameterList(item._parameters)}{requestPayload})");
                sb.AppendLine($" => {arraySupport}{item._restType.ToUpper()}{responseClassGenericParamForRESTApi}($\"{{MiceWebClient.REST_API_URL}}{item.MakeRequestURL()}\"{payloadRelay}{callerInfo});");
            }

            if (item._apiForDevelopment)
            {
                var obsoleteMethod =
                    $"{indent}[System.Obsolete(\"MUST BE USED ONLY IN '{DEVELOPMENT_PREPROCESSOR_NAME_00} || {DEVELOPMENT_PREPROCESSOR_NAME_01}' ENVIRONMENT.\", true)]\r\n" +
                    //$"{indent}public static UniTask{responseClassGenericParamForUniTask} {apiName}(params object[] unused)" +
                    $"{indent}public static {responseClassGenericParamForReturn} {apiName}(params object[] unused)" +
                    //$" => UniTask.FromResult{responseClassGenericParamForUniTask}(default!);";
                    $" => default;";

                if (allAPIForDevelopment)
                {
                    sbNotDev.AppendLine(obsoleteMethod);
                }
                else
                {
                    sb.AppendLine("#else");
                    sb.AppendLine(obsoleteMethod);
                    sb.AppendLine("#endif");
                }
            }
        }

        /// <summary>
        /// Json Object Class - "$.info" token
        /// </summary>
        [Serializable]
        internal class APIDescription
        {
            [JsonProperty] public string title { get; set; }
            [JsonProperty] public string description { get; set; }
            [JsonProperty] public string version { get; set; }

            public static IEnumerable<APIDescription> Parse(string json) => json.Parse<APIDescription>("$.info");
        }
    }
}

