using System;
using System.Collections;
using System.Net.Http;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Com2Verse.HttpHelper.Tests
{
    public class PapagoTest : TestBase
    {
        private static readonly string PapagoApiUrl = "https://openapi.naver.com/v1/papago/n2mt";

        private static readonly string NaverHeaderClientID = "X-Naver-Client-Id";
        private static readonly string NaverHeaderClientSecret = "X-Naver-Client-Secret";
        private static readonly string NaverClientID = "nd2uMOepW6SLmdGlsJPE";
        private static readonly string NaverClientSecret = "gVNnj7hCh6";
        private static readonly string ContentTypeForm = "application/x-www-form-urlencoded";

        private static readonly (string, string)[] PapagoTests = new (string, string)[]
        {
            ("안녕하세요", "Hello"),
            ("반갑습니다", "Nice to meet you."),
            ("어디가세요?", "Where are you going?"),
        };

        // [UnityTest]
        public IEnumerator TranslateTest_AllInOne([ValueSource("PapagoTests")] (string, string) testPair)
        {
            yield return UniTask.ToCoroutine(async () => await RunTestAsync(async () =>
            {
                var source = "ko";
                var target = "en";
                var postParams = MakePostParams(testPair.Item1, source, target);
                var request = HttpRequestBuilder.Generate(new HttpRequestMessageInfo
                {
                    RequestMethod = Client.eRequestType.POST,
                    Url = PapagoApiUrl,
                    Content = new StringContent(postParams),
                    Headers = new (string, string)[]
                    {
                        (NaverHeaderClientID, NaverClientID),
                        (NaverHeaderClientSecret, NaverClientSecret),
                    },
                    ContentType = ContentTypeForm,
                    // ContentLength = postParams.Length,
                });

                var responseString = await Client.Message.RequestStringAsync(request);
                var response = JsonUtility.FromJson<JsonPapagoResponse>(responseString.Value);
                var translated = response.message.result.translatedText;
                C2VDebug.Log(translated);
                Assert.AreEqual(testPair.Item2, translated);
            }));
        }

        // [UnityTest]
        public IEnumerator TranslateTest_Builder([ValueSource("PapagoTests")] (string, string) testPair)
        {
            yield return UniTask.ToCoroutine(async () => await RunTestAsync(async () =>
            {
                var source = "ko";
                var target = "en";
                var postParams = MakePostParams(testPair.Item1, source, target);

                var builder = HttpRequestBuilder.CreateNew(Client.eRequestType.POST, PapagoApiUrl);
                builder.SetContent(postParams);
                builder.AddHeader(NaverHeaderClientID, NaverClientID);
                builder.AddHeader(NaverHeaderClientSecret, NaverClientSecret);
                builder.SetContentType(ContentTypeForm);

                var responseString = await Client.Message.RequestStringAsync(builder.Request);
                var response = JsonUtility.FromJson<JsonPapagoResponse>(responseString.Value);
                var translated = response.message.result.translatedText;
                C2VDebug.Log(translated);
                Assert.AreEqual(testPair.Item2, translated);
            }));
        }
        private string MakePostParams(string message, string source, string target) => $"source={source}&target={target}&text={message}";
#region Response Json
        private class JsonObject
        {
            public string ToJson() => JsonUtility.ToJson(this);
        }
        [Serializable]
        private class JsonPapagoResponse : JsonObject
        {
            public JsonPapagoMessageResponse message;
        }

        [Serializable]
        private class JsonPapagoMessageResponse : JsonObject
        {
            public JsonPapagoResultResponse result;
        }

        [Serializable]
        private class JsonPapagoResultResponse : JsonObject
        {
            public string translatedText;
        }
#endregion // Response Json
    }
}
