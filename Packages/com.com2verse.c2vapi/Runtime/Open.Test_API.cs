//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Com2Verse.HttpHelper;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using System;


namespace Com2Verse.WebApi.Open.Test
{
    
    
    // Public Open API (Test)
    public class Api
    {
        
        private const string _apiUrl = "https://test-api.com2verse.com";
        
        private const string _apiUrlFormat = "{0}/{1}";
        
        public class Asset
        {
            
            /// <summary>
            /// GET
            /// 에셋 패치버전 조회 - 클라이언트용
            /// /api/asset/{MetaversId}/{AssetType}/{BuildTarget}/{AppVersion}
            /// </summary>
            /// <param name="MetaversId">long</param>
            /// <param name="AssetType">string</param>
            /// <param name="BuildTarget">string</param>
            /// <param name="AppVersion">string</param>
            public static async UniTask<ResponseBase<Components.AssetResult>> GetAsset(long MetaversId, string AssetType, string BuildTarget, string AppVersion, System.Threading.CancellationTokenSource cts = null)
            {
                string url = $"{_apiUrl}/api/asset/{MetaversId}/{AssetType}/{BuildTarget}/{AppVersion}";
                var response = await Client.GET.RequestAsync<Components.AssetResult>(url, string.Empty, cts);
                return response;
            }
            
            /// <summary>
            /// POST
            /// 에셋 리스트 - 관리페이지용
            /// /api/asset/list
            /// </summary>
            public static async UniTask<ResponseBase<Components.AssetEntity[]>> PostAssetList(Components.AssetRequest requestBody, System.Threading.CancellationTokenSource cts = null)
            {
                string url = $"{_apiUrl}/api/asset/list";
                using var builder = HttpRequestBuilder.CreateNew(Client.eRequestType.POST, url);
                builder.SetContentType(Client.Constant.ContentJson);
                builder.SetContent(JsonConvert.SerializeObject(requestBody));
                var response = await Client.Message.Request<Components.AssetEntity[]>(builder.Request, cts);
                return response;
            }
            
            /// <summary>
            /// GET
            /// 에셋 상세조회 - 관리페이지용
            /// /api/asset/{AssetId}
            /// </summary>
            /// <param name="AssetId">long</param>
            public static async UniTask<ResponseBase<Components.AssetEntity>> GetAsset(long AssetId, System.Threading.CancellationTokenSource cts = null)
            {
                string url = $"{_apiUrl}/api/asset/{AssetId}";
                var response = await Client.GET.RequestAsync<Components.AssetEntity>(url, string.Empty, cts);
                return response;
            }
            
            /// <summary>
            /// PUT
            /// 에셋 수정 - 관리페이지용
            /// /api/asset/{AssetId}
            /// </summary>
            /// <param name="AssetId">long</param>
            public static async UniTask<ResponseBase<Components.AssetEntity>> PutAsset(long AssetId, Components.AssetEntity requestBody, System.Threading.CancellationTokenSource cts = null)
            {
                string url = $"{_apiUrl}/api/asset/{AssetId}";
                using var builder = HttpRequestBuilder.CreateNew(Client.eRequestType.PUT, url);
                builder.SetContentType(Client.Constant.ContentJson);
                builder.SetContent(JsonConvert.SerializeObject(requestBody));
                var response = await Client.Message.Request<Components.AssetEntity>(builder.Request, cts);
                return response;
            }
            
            /// <summary>
            /// DELETE
            /// 에셋 삭제 - 관리페이지용
            /// /api/asset/{AssetId}
            /// </summary>
            /// <param name="AssetId">long</param>
            public static async UniTask<ResponseBase<Components.AssetEntity>> DeleteAsset(long AssetId, System.Threading.CancellationTokenSource cts = null)
            {
                string url = $"{_apiUrl}/api/asset/{AssetId}";
                var response = await Client.DELETE.RequestAsync<Components.AssetEntity>(url, string.Empty, cts);
                return response;
            }
            
            /// <summary>
            /// POST
            /// 에셋 등록 - 관리페이지용
            /// /api/asset
            /// </summary>
            public static async UniTask<ResponseBase<Components.AssetEntity>> PostAsset(Components.AssetEntity requestBody, System.Threading.CancellationTokenSource cts = null)
            {
                string url = $"{_apiUrl}/api/asset";
                using var builder = HttpRequestBuilder.CreateNew(Client.eRequestType.POST, url);
                builder.SetContentType(Client.Constant.ContentJson);
                builder.SetContent(JsonConvert.SerializeObject(requestBody));
                var response = await Client.Message.Request<Components.AssetEntity>(builder.Request, cts);
                return response;
            }
        }
        
        public class Avatar
        {
            
            /// <summary>
            /// GET
            /// 아바타 정보 조회
            /// /api/avatar/{account_id}/{avatar_id}
            /// </summary>
            /// <param name="account_id">long</param>
            /// <param name="avatar_id">long</param>
            public static async UniTask<ResponseBase<Components.AvatarEntity>> GetAvatar(long account_id, long avatar_id, System.Threading.CancellationTokenSource cts = null)
            {
                string url = $"{_apiUrl}/api/avatar/{account_id}/{avatar_id}";
                var response = await Client.GET.RequestAsync<Components.AvatarEntity>(url, string.Empty, cts);
                return response;
            }
            
            /// <summary>
            /// GET
            /// 아바타 리스트 조회
            /// /api/avatar/{account_id}
            /// </summary>
            /// <param name="account_id">long</param>
            public static async UniTask<ResponseBase<Components.AvatarEntity[]>> GetAvatar(long account_id, System.Threading.CancellationTokenSource cts = null)
            {
                string url = $"{_apiUrl}/api/avatar/{account_id}";
                var response = await Client.GET.RequestAsync<Components.AvatarEntity[]>(url, string.Empty, cts);
                return response;
            }
        }
        
        public class Common
        {
            
            /// <summary>
            /// GET
            /// 공통 메타버스 리스트 조회
            /// /api/common/metaverse/list
            /// </summary>
            public static async UniTask<ResponseBase<Components.MetaverseEntity[]>> GetCommonMetaverseList(System.Threading.CancellationTokenSource cts = null)
            {
                string url = $"{_apiUrl}/api/common/metaverse/list";
                var response = await Client.GET.RequestAsync<Components.MetaverseEntity[]>(url, string.Empty, cts);
                return response;
            }
        }
    }
    
    public class Components
    {
        
        [Serializable()]
        public class AssetEntity
        {
            
            [JsonProperty("METAVERSE_ID")]
            public long MetaverseId { get; set; } //;
            
            [JsonProperty("METAVERSE_PATH")]
            public string MetaversePath { get; set; } //;
            
            [JsonProperty("ASSET_TYPE")]
            public string AssetType { get; set; } //;
            
            [JsonProperty("BUILD_TARGET")]
            public string BuildTarget { get; set; } //;
            
            [JsonProperty("APP_VERSION")]
            public string AppVersion { get; set; } //;
            
            [JsonProperty("PATCH_VERSION")]
            public string PatchVersion { get; set; } //;
            
            [JsonProperty("USE_YN")]
            public string UseYn { get; set; } //;
        }
        
        [Serializable()]
        public class AssetRequest
        {
            
            [JsonProperty("METAVERSE_ID")]
            public long MetaverseId { get; set; } //;
            
            [JsonProperty("BUILD_TARGET")]
            public string BuildTarget { get; set; } //;
            
            [JsonProperty("USE_YN")]
            public string UseYn { get; set; } //;
            
            [JsonProperty("PAGE_SIZE")]
            public int PageSize { get; set; } //;
            
            [JsonProperty("PAGE_NUM")]
            public int PageNum { get; set; } //;
        }
        
        [Serializable()]
        public class AssetResult
        {
            
            [JsonProperty("RSLT_CD")]
            public string RsltCd { get; set; } //;
            
            [JsonProperty("RSLT_MSG")]
            public string RsltMsg { get; set; } //;
            
            [JsonProperty("MetaversePath")]
            public string MetaversePath { get; set; } //;
            
            [JsonProperty("PatchVersion")]
            public string PatchVersion { get; set; } //;
        }
        
        [Serializable()]
        public class AvatarBody
        {
            
            [JsonProperty("body_id")]
            public int BodyId { get; set; } //;
            
            [JsonProperty("body_key")]
            public int BodyKey { get; set; } //;
            
            [JsonProperty("body_value")]
            public int BodyValue { get; set; } //;
            
            [JsonProperty("body_r")]
            public int BodyR { get; set; } //;
            
            [JsonProperty("body_g")]
            public int BodyG { get; set; } //;
            
            [JsonProperty("body_b")]
            public int BodyB { get; set; } //;
        }
        
        [Serializable()]
        public class AvatarEntity
        {
            
            [JsonProperty("RSLT_CD")]
            public string RsltCd { get; set; } //;
            
            [JsonProperty("RSLT_MSG")]
            public string RsltMsg { get; set; } //;
            
            [JsonProperty("AvatarId")]
            public long AvatarId { get; set; } //;
            
            [JsonProperty("AvatarType")]
            public int AvatarType { get; set; } //;
            
            [JsonProperty("BodyItems")]
            public Components.AvatarBody[] BodyItems { get; set; } //;
            
            [JsonProperty("FashionItems")]
            public Components.AvatarFashion[] FashionItems { get; set; } //;
        }
        
        [Serializable()]
        public class AvatarFashion
        {
            
            [JsonProperty("fashion_id")]
            public int FashionId { get; set; } //;
            
            [JsonProperty("fashion_type")]
            public int FashionType { get; set; } //;
            
            [JsonProperty("fashion_r")]
            public int FashionR { get; set; } //;
            
            [JsonProperty("fashion_g")]
            public int FashionG { get; set; } //;
            
            [JsonProperty("fashion_b")]
            public int FashionB { get; set; } //;
        }
        
        [Serializable()]
        public class MetaverseEntity
        {
            
            [JsonProperty("METAVERSE_ID")]
            public long MetaverseId { get; set; } //;
            
            [JsonProperty("METAVERSE_NAME")]
            public string MetaverseName { get; set; } //;
            
            [JsonProperty("USE_YN")]
            public string UseYn { get; set; } //;
        }
    }
}
