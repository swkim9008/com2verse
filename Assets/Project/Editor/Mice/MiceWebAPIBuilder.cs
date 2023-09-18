/*===============================================================
* Product:		Com2Verse
* File Name:	MiceWebAPIBuilder.cs
* Developer:	sprite
* Date:			2023-03-30 14:37
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEditor;
using Com2Verse.HttpHelper;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;

namespace Com2VerseEditor.Mice
{
    public sealed partial class MiceWebAPIBuilder
    {
        public static readonly string MICE_WEB_API_SWAGGER = "/swagger/v1/swagger.json";

        private static readonly string MICE_WEB_API_GENERATED_TARGET_PATH = "Project/Runtime/Scripts/Mice/WebClient/Generated";
        private static readonly string MICE_WEB_API_GENERATED_FILE = "MiceWebClient_Generated.cs";
        private static readonly string MICE_WEB_API_GENERATED_RESPONSE_FILE = "MiceWebClient_Response_Generated.cs";

        public static readonly string MICE_WEB_API_GENERATED_ENTITIES = "Entities";

        private static readonly string GENERATED_CS_TEMPLATE_FMT =
@"{0}
#nullable enable
namespace Com2Verse.Mice
{{
    public static partial class MiceWebClient
    {{
{1}
    }}
}}
";
        public static readonly string MICEWEBTEST_3TH_PARAM_NAME = "onResult";

        [MenuItem("Com2Verse/MiceWebAPIBuilder/Build", priority = 120000)]
        private static void OnMiceWebAPIBuilder_Build()
        {
            MiceWebAPIBuilder.Build().Forget();
        }

        public static async UniTask Build()
        {
            var swaggerURL = $"{MiceWebAPIConfiguration.GetMiceServerIP()}{MICE_WEB_API_SWAGGER}";

            string resultMsg = $"작업 완료!\r\n'{swaggerURL}'";

            try
            {
                do
                {
#region Request "swagger.json"
                    var responseString = await Client.GET.RequestStringAsync(swaggerURL);
                    var protocol = responseString.Value;
                    if (string.IsNullOrEmpty(protocol))
                    {
                        C2VDebug.Log($"[MICEWebTest] Request API Json failed! ('{swaggerURL}')");

                        resultMsg = $"서버 요청 실패!\r\n'{swaggerURL}'";
                        break;
                    }
#endregion // Request "swagger.json"

#region Parse Response Classess & Enums
                    var components = ComponentInfo.Parse(protocol).ToList();
                    var enumComponents = components
                        .Where(e => e is EnumComponentInfo)
                        .Select(e => e as EnumComponentInfo)
                        .ToList();
                    var componentLog = ComponentInfo.Report(components);
                    C2VDebug.Log(componentLog);

                    MiceWebAPIBuilder.ExportResponse(components);
#endregion // Parse Response Classess & Enums

#region Parse Request Methods
                    var apiList = APIItem.Parse(protocol, enumComponents).ToList();
                    var log = APIItem.Report(apiList);
                    C2VDebug.Log(log);

                    var apiMap = apiList
                        .GroupBy(e => e._tag)
                        .ToDictionary(e => e.Key, e => e.ToList());

                    APIItem.InitCustomize();
                    APIItem.ApplyCustomize(apiMap);

                    MiceWebAPIBuilder.ExportRequest(apiMap);
#endregion // Parse Request Methods

                } while (false);
            }
            catch (Exception e)
            {
                resultMsg = e.Message;

                throw new Exception("Exception Occured!", e);
            }
            finally
            {
                EditorUtility.DisplayDialog("MiceWebAPIBuilder", resultMsg, "확인");
            }
        }
    }
}
