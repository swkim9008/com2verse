using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Com2Verse.HttpHelper.Tests
{
    public class SimpleTest : TestBase
    {
#region Test Data
        // Dummy Api - https://dummy.restapiexample.com/
        private static readonly TestInfo TestInfo_Get_All = TestInfo.Create(RequestInfo.CreateGet("https://dummy.restapiexample.com/api/v1/employees"), typeof(JsonEmployeesResponse));
        private static readonly TestInfo TestInfo_Get_Single = TestInfo.Create(RequestInfo.CreateGet("https://dummy.restapiexample.com/api/v1/employee/1"), typeof(JsonEmployeeResponse));
        private static readonly TestInfo TestInfo_Post = TestInfo.Create(RequestInfo.CreatePost("https://dummy.restapiexample.com/api/v1/create", JsonSalaryRequest.Create("test", 123, 23).ToJson()), typeof(JsonSalaryResponse));
        private static readonly TestInfo TestInfo_Put = TestInfo.Create(RequestInfo.CreatePut("https://dummy.restapiexample.com/api/v1/update/21", JsonSalaryRequest.Create("test", 123, 23).ToJson()), typeof(JsonSalaryResponse));
        private static readonly TestInfo TestInfo_Delete = TestInfo.Create(RequestInfo.CreateDelete("https://dummy.restapiexample.com/api/v1/delete/2"), typeof(JsonMessageResponse));

        // JsonPlaceHolder - https://jsonplaceholder.typicode.com/
        private static readonly TestInfo TestInfo_JPH_Get_All = TestInfo.Create(RequestInfo.CreateGet("https://jsonplaceholder.typicode.com/posts"));
        private static readonly TestInfo TestInfo_JPH_Get_One = TestInfo.Create(RequestInfo.CreateGet("https://jsonplaceholder.typicode.com/posts/1"), typeof(JsonUserResponse));
        private static readonly TestInfo TestInfo_JPH_Get_One_Comment = TestInfo.Create(RequestInfo.CreateGet("https://jsonplaceholder.typicode.com/posts/1/comments"));
        private static readonly TestInfo TestInfo_JPH_Get_With_Parameter = TestInfo.Create(RequestInfo.CreateGet("https://jsonplaceholder.typicode.com/comments?postId=1"));
        private static readonly TestInfo TestInfo_JPH_Post = TestInfo.Create(RequestInfo.CreatePost("https://jsonplaceholder.typicode.com/posts", JsonUserRequest.Create(1, "foo", "bar").ToJson()), typeof(JsonUserResponse));
        private static readonly TestInfo TestInfo_JPH_Put = TestInfo.Create(RequestInfo.CreatePut("https://jsonplaceholder.typicode.com/posts/1", JsonUserModifyRequest.Create(1, 1, "foo", "bar").ToJson()), typeof(JsonUserResponse));
        private static readonly TestInfo TestInfo_JPH_Delete = TestInfo.Create(RequestInfo.CreateDelete("https://jsonplaceholder.typicode.com/posts/1"));

        // https://www.gutenberg.org/ebooks/1513.txt.utf-8
        // http://ipv4.download.thinkbroadband.com/50MB.zip
        private static readonly TestInfo TestInfo_Get_With_Progress = TestInfo.Create(RequestInfo.CreateGetWithProgress("https://www.gutenberg.org/ebooks/69956.txt.utf-8"));

        private static TestInfo[] _getTests = new[]
        {
            TestInfo_JPH_Get_All,
            TestInfo_JPH_Get_All,
            TestInfo_JPH_Get_One,
            TestInfo_JPH_Get_One_Comment,
            TestInfo_JPH_Get_With_Parameter,
        };

        private static TestInfo[] _postTests = new[]
        {
            TestInfo_JPH_Post,
        };

        private static TestInfo[] _putTests = new[]
        {
            TestInfo_JPH_Put,
        };

        private static TestInfo[] _deleteTests = new[]
        {
            TestInfo_JPH_Delete,
        };

        private static TestInfo[] _getWithProgressTests = new[]
        {
            TestInfo_Get_With_Progress,
        };
        private IEnumerable<TestInfo> _allTests => _getTests.Concat(_postTests).Concat(_putTests).Concat(_deleteTests);
#endregion // Test Data

#region UNIT TESTS
        [UnityTest]
        public IEnumerator TestAll()
        {
            yield return RunTests(_allTests);
        }
        [UnityTest]
        public IEnumerator Get()
        {
            yield return RunTests(_getTests);
        }

        // [UnityTest]
        public IEnumerator GetInfinity()
        {
            var accept = EditorUtility.DisplayDialog("주의", "요청을 반복해서 보내고 테스트는 끝나지 않습니다.\n계속 하시겠습니까?", "확인", "취소");
            if (accept)
            {
                while(true)
                    yield return RunTests(_getTests);
            }
        }

        // [UnityTest]
        public IEnumerator GetWithProgress()
        {
            yield return RunTests(_getWithProgressTests);
        }
        [UnityTest]
        public IEnumerator Post()
        {
            yield return RunTests(_postTests);
        }
        [UnityTest]
        public IEnumerator Put()
        {
            yield return RunTests(_putTests);
        }
        [UnityTest]
        public IEnumerator Delete()
        {
            yield return RunTests(_deleteTests);
        }

        private IEnumerator RunTests(IEnumerable<TestInfo> tests)
        {
            foreach (var test in tests)
            {
                switch (test.Request.Type)
                {
                    case eTestType.GET:
                        yield return GetRequest(test);
                        break;
                    case eTestType.GET_PROGRESS:
                        yield return GetWithProgressRequest(test);
                        break;
                    case eTestType.POST:
                        yield return PostRequest(test);
                        break;
                    case eTestType.PUT:
                        yield return PutRequest(test);
                        break;
                    case eTestType.DELETE:
                        yield return DeleteRequest(test);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        private IEnumerator GetRequest(TestInfo testInfo) => UniTask.ToCoroutine(async () => await GetTest(testInfo));
        private IEnumerator GetWithProgressRequest(TestInfo testInfo) => UniTask.ToCoroutine(async () => await GetTestWithProgress(testInfo));
        private IEnumerator PostRequest(TestInfo testInfo) => UniTask.ToCoroutine(async () => await PostTest(testInfo));
        private IEnumerator PutRequest(TestInfo testInfo) => UniTask.ToCoroutine(async () => await PutTest(testInfo));
        private IEnumerator DeleteRequest(TestInfo testInfo) => UniTask.ToCoroutine(async () => await DeleteTest(testInfo));
#endregion // UNIT TESTS

#region Test
        private async UniTask GetTest(TestInfo testInfo)
        {
            await RunTestAsync(async () =>
            {
                var responseJson = await Client.GET.RequestStringAsync(testInfo.Request.Url);
                Assert.NotNull(responseJson);
                // C2VDebug.Log(responseJson);
            });
        }

        private async UniTask GetTestWithProgress(TestInfo testInfo)
        {
            await RunTestAsync(async () =>
            {
                var handler = await Client.Request.CreateRequestWithCallbackAsync(Client.eRequestType.GET, _callbacks, testInfo.Request.Url);
                await handler.SendAsync();
            });
        }
        private async UniTask PostTest(TestInfo testInfo)
        {
            await RunTestAsync(async () =>
            {
                var responseJson = await Client.POST.RequestStringAsync(testInfo.Request.Url, testInfo.Request.Content);
                Assert.NotNull(responseJson);
                C2VDebug.Log(responseJson);
            });
        }

        private async UniTask PostTestWithProgress(TestInfo testInfo)
        {
            await RunTestAsync(async () =>
            {
                var handler = await Client.Request.CreateRequestWithCallbackAsync(Client.eRequestType.POST, _callbacks, testInfo.Request.Url, testInfo.Request.Content);
                await handler.SendAsync();
            });
        }
        private async UniTask PutTest(TestInfo testInfo)
        {
            await RunTestAsync(async () =>
            {
                var responseJson = await Client.PUT.RequestStringAsync(testInfo.Request.Url, testInfo.Request.Content);
                Assert.NotNull(responseJson);
                C2VDebug.Log(responseJson);
            });
        }

        private async UniTask DeleteTest(TestInfo testInfo)
        {
            await RunTestAsync(async () =>
            {
                var responseJson = await Client.DELETE.RequestStringAsync(testInfo.Request.Url);
                Assert.NotNull(responseJson);
                C2VDebug.Log(responseJson);
            });
        }

        private static Callbacks _callbacks = new Callbacks
        {
            OnDownloadProgress = (read, totalRead, totalSize) => C2VDebug.Log($"[{totalRead}/{totalSize}] read {read} bytes"),
            OnComplete = (async (stream, totalReadLen) =>
            {
                using (var sr = new StreamReader(stream))
                {
                    var result = await sr.ReadToEndAsync();
                    C2VDebug.Log($"Complete [{totalReadLen}]\n{result}");
                }
            }),
            OnFailed = (statusCode => { C2VDebug.LogWarning($"Failed = {statusCode}"); }),
        };
#endregion // Test

#region Data
        private enum eTestType
        {
            GET,
            GET_PROGRESS,
            POST,
            PUT,
            DELETE,
        }
        private struct RequestInfo
        {
            public eTestType Type;
            public string Url;
            public string Content;

            public static RequestInfo CreateGet(string url) => NewTest(eTestType.GET, url);
            public static RequestInfo CreateGetWithProgress(string url) => NewTest(eTestType.GET_PROGRESS, url);
            public static RequestInfo CreatePost(string url, string param) => NewTest(eTestType.POST, url, param);
            public static RequestInfo CreatePut(string url, string param) => NewTest(eTestType.PUT, url, param);
            public static RequestInfo CreateDelete(string url) => NewTest(eTestType.DELETE, url);
            private static RequestInfo NewTest(eTestType type, string url, string param = "") => new RequestInfo {Type = type, Url = url, Content = param};
        }

        private struct TestInfo
        {
            public RequestInfo Request;
            public Type ResponseType;
            public string Expect;
            public static TestInfo Create(RequestInfo request, Type responseType = null, string expect = "") => new TestInfo {Request = request, ResponseType = responseType, Expect = expect};
        }

#region Response Json
        private class JsonObject
        {
            public string ToJson() => JsonUtility.ToJson(this);
        }

        // GET (All)
        [Serializable]
        private class JsonEmployeesResponse : JsonObject
        {
            public string status;
            public JsonEmployeeData[] data;

            public static JsonEmployeesResponse Create(string status, JsonEmployeeData[] data) => new JsonEmployeesResponse {status = status, data = data};
        }

        // GET (Single)
        [Serializable]
        private class JsonEmployeeResponse : JsonObject
        {
            public string status;
            public JsonEmployeeData data;

            public static JsonEmployeeResponse Create(string status, JsonEmployeeData data) => new JsonEmployeeResponse {status = status, data = data};
        }

        // POST, PUT
        [Serializable]
        private class JsonSalaryResponse : JsonObject
        {
            public string status;
            public JsonSalaryData data;

            public static JsonSalaryResponse Create(string status, JsonSalaryData data) => new JsonSalaryResponse {status = status, data = data};
        }

        // DELETE
        [Serializable]
        private class JsonMessageResponse : JsonObject
        {
            public string status;
            public string message;

            public static JsonMessageResponse Create(string status, string message) => new JsonMessageResponse {status = status, message = message};
        }

        [Serializable]
        private class JsonEmployeeData : JsonObject
        {
            public int id;
            public string employee_name;
            public int employee_salary;
            public int employee_age;
            public string profile_image;
        }

        [Serializable]
        private class JsonSalaryData : JsonObject
        {
            public string name;
            public int salary;
            public int age;
            public int id;
        }

        [Serializable]
        private class JsonSalaryRequest : JsonObject
        {
            public string name;
            public int salary;
            public int age;

            public static JsonSalaryRequest Create(string name, int salary, int age) => new JsonSalaryRequest {name = name, salary = salary, age = age};
        }

        [Serializable]
        private class JsonUserRequest : JsonObject
        {
            public int userId;
            public string title;
            public string body;
            public static JsonUserRequest Create(int userId, string title, string body) => new JsonUserRequest {userId = userId, title = title, body = body};
        }

        [Serializable]
        private class JsonUserModifyRequest : JsonObject
        {
            public int userId;
            public int id;
            public string title;
            public string body;
            public static JsonUserModifyRequest Create(int userid, int id, string title, string body) => new JsonUserModifyRequest {userId = userid, id = id, title = title, body = body};
        }

        [Serializable]
        private class JsonUserResponse : JsonObject
        {
            public int userId;
            public int id;
            public string title;
            public string body;
        }
#endregion // Response Json
#endregion // Data
    }
}
