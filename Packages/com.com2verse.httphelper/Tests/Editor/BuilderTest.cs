using System;
using System.Collections;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace Com2Verse.HttpHelper.Tests
{
    public class BuilderTest : TestBase
    {
        private static readonly string GetUrl = "https://jsonplaceholder.typicode.com/posts";
        private static readonly string PostUrl = "https://jsonplaceholder.typicode.com/posts";

        [UnityTest]
        public IEnumerator GetWithBuilder()
        {
            yield return RequestWithBuilder(Client.eRequestType.GET, GetUrl);
        }

        [UnityTest]
        public IEnumerator PostWithBuilder()
        {
            var content = JsonUserRequest.Create(1, "foo", "bar").ToJson();
            yield return RequestWithBuilder(Client.eRequestType.POST, PostUrl, content);
        }

        private IEnumerator RequestWithBuilder(Client.eRequestType requestType, string url, string content = "")
        {
            // 빌더 생성
            var builder = HttpRequestBuilder.CreateNew(requestType, url);

            // 속성 적용
            if (!string.IsNullOrWhiteSpace(content))
                builder.SetContent(content);

            yield return UniTask.ToCoroutine(async () =>
            {
                var request = await Client.Request.CreateRequestWithCallbackAsync(builder.Request, new Callbacks
                {
                    OnDownloadStart = () => C2VDebug.Log("DOWNLOAD START"),
                    OnDownloadProgress = (read, totalRead, totalSize) => C2VDebug.Log($"DOWNLOAD PROGRESS Read {read}, TotalRead {totalRead}, TotalSize {totalSize}"),
                    OnComplete = (stream, totalSize) => C2VDebug.Log("DOWNLOAD  COMPLETE"),
                    OnFailed = (statusCode) => C2VDebug.Log($"DOWNLOAD FAILED = {statusCode}"),
                    OnFinally = () => C2VDebug.Log("DOWNLOAD FINALLY"),
                });
                await request.SendAsync();
            });
        }
#region Data
        [Serializable]
        private class JsonUserRequest
        {
            public int userId;
            public string title;
            public string body;
            public static JsonUserRequest Create(int userId, string title, string body) => new JsonUserRequest {userId = userId, title = title, body = body};
            public string ToJson() => JsonUtility.ToJson(this);
        }
#endregion // Data
    }
}
