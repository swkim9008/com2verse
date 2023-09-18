/*===============================================================
* Product:		Com2Verse
* File Name:	MiceWebClient.cs
* Developer:	sprite
* Date:			2023-04-05 19:57
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#define USE_CLIENT_INTERNALS
//#define USE_INTERNAL_HTTP_STATUS_CODE
//#define TEST_SERVER_ERROR
//#define USE_LEGACY_METHODS // ManualErrorHandling 사용 못함.

using System;
using System.Linq;
using Com2Verse.HttpHelper;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using Com2Verse.Network;
using System.Reflection;
using System.Runtime.CompilerServices;

#if USE_CLIENT_INTERNALS
using System.Threading;
using System.Net.Http;
using Com2Verse.UI;
#endif

#if USE_INTERNAL_HTTP_STATUS_CODE
using eHttpStatusCode = Com2Verse.Mice.MiceWebClient.HttpStatusCode;
#else
using eHttpStatusCode = System.Net.HttpStatusCode;
#endif

#nullable enable
namespace Com2Verse.Mice
{
    public static partial class MiceWebClient
    {
        private static string REST_API_URL => Configurator.Instance.Config.MiceServerAddress;

        public class Response
        {
            public string Request { get; private set; }
            public Entities.CheckResult Result { get; protected set; }
            public ServerError? ServerError { get; protected set; }
            public string? Json { get; protected set; }

            public UniTask<Response?> Task { get; private set; } = UniTask.FromResult(default(Response));
            public UniTask<Response?>.Awaiter GetAwaiter() => this.Task.GetAwaiter();

            public bool IsManualErrorHandling { get; protected set; } = false;

            public static implicit operator bool(Response result) => result.Result;

            public Response()
            {
                this.Request = string.Empty;
                this.Result = Entities.CheckResult.INTERNAL_ERROR;
                this.ServerError = null;
            }

            public Response(string json, string requestName = "", string url = "")
            {
                var uniqueName = this.Init(requestName, url);

                this.Request = uniqueName;
                this.Json = json;
                this.Result = string.IsNullOrEmpty(json)
                    ? Entities.CheckResult.INTERNAL_ERROR
                    : Entities.CheckResult.OK;

                MiceWebClient.Dispatch(uniqueName, this);
            }

            public Response(string json, CallerInfo callerInfo)
            {
                if (!callerInfo.IsValid) throw new ArgumentException($"CallerInfo is not valid!");

                this.Request = callerInfo.UniqueName;
                this.Json = json;
                this.Result = string.IsNullOrEmpty(json)
                    ? Entities.CheckResult.INTERNAL_ERROR
                    : Entities.CheckResult.OK;

                MiceWebClient.Dispatch(callerInfo.UniqueName, this, callerInfo.ParamType);
            }

            public Response((eHttpStatusCode httpStatusCode, string json) result, string requestName = "", string url = "")
                : this(result.json, requestName, url)
            {
                Result.SetHttpStatusCode(result.httpStatusCode);
            }

            public Response((eHttpStatusCode httpStatusCode, string json) result, CallerInfo callerInfo)
                : this(result.json, callerInfo)
            {
                Result.SetHttpStatusCode(result.httpStatusCode);
            }

            public Response(Func<Response, UniTask<(eHttpStatusCode httpStatusCode, string json)>> requester, string requestName = "", string url = "")
                : this()
            {
                var uniqueName = this.Init(requestName, url);
                this.Task = UniTask.Defer(() => Response.InternalRequest(this, requester, uniqueName));
            }

            public Response(Func<Response, UniTask<(eHttpStatusCode httpStatusCode, string json)>> requester, CallerInfo callerInfo)
                : this()
            {
                var uniqueName = this.Init(callerInfo);
                this.Task = UniTask.Defer(() => Response.InternalRequest(this, requester, uniqueName, callerInfo.ParamType));
            }

            protected string Init(string requestName = "", string url = "")
            {
                var pos = url.IndexOf("/api");
                var groupName = url.Substring(pos + 1).Split('/')[1];
                var uniqueName = $"{groupName}.{requestName}";

                this.Request = uniqueName;
                this.Result = Entities.CheckResult.INTERNAL_ERROR;

                return uniqueName;
            }

            protected string Init(CallerInfo callerInfo)
            {
                this.Request = callerInfo.UniqueName;
                this.Result = Entities.CheckResult.INTERNAL_ERROR;

                return callerInfo.UniqueName;
            }


            protected static async UniTask<TResponse?> InternalRequest<TResponse>
                (
                    TResponse response, 
                    Func<Response, UniTask<(eHttpStatusCode httpStatusCode, string json)>> requester, 
                    string uniqueName,
                    Type? paramType = null,
                    Action? beforeDispatch = null
                )
                where TResponse : Response
            {
                (eHttpStatusCode httpStatusCode, string json) = await requester(response);

                response.Request = uniqueName;
                response.Json = json;
                response.Result = string.IsNullOrEmpty(response.Json)
                    ? Entities.CheckResult.INTERNAL_ERROR
                    : Entities.CheckResult.OK;

                response.Result.SetHttpStatusCode(httpStatusCode);

                response.ServerError = MiceWebClient.LastServerError;
                MiceWebClient.LastServerError = null;

                beforeDispatch?.Invoke();

                MiceWebClient.Dispatch(uniqueName, response, paramType);

                return response;
            }

            public UniTask<Response?> ManualErrorHandling()
            {
                this.IsManualErrorHandling = true;
                return this.Task;
            }

            public virtual void ShowErrorMessage()
            {
                var uiMsg = Localization.Instance.GetErrorString((int)Protocols.ErrorCode.DbError);
                MiceWebClient.ShowErrorMessage(uiMsg);
            }

            /// <summary>
            /// 응답 Json이 존재하는 경우, Json을 특정 타입으로 역직렬화한다.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public T? GetData<T>()
            {
                if (string.IsNullOrEmpty(Json)) return default;

                // 성공 결과.
                var obj = JsonConvert.DeserializeObject<T>(Json);
                if (obj != null) return obj;

                // 실패 결과.
                var checkResult = JsonConvert.DeserializeObject<Entities.CheckResult>(Json);
                if (checkResult != null)
                {
                    Result = checkResult.SetHttpStatusCode(Result.HttpStatusCode);
                }

                return default;
            }

            public async void Forget() => await this.Task;
        }

        public class Response<T> : Response
        {
            public T? Data { get; private set; }

#if !USE_LEGACY_METHODS
            public new UniTask<Response<T>?> Task { get; private set; } = UniTask.FromResult(default(Response<T>));
            public new UniTask<Response<T>?>.Awaiter GetAwaiter() => this.Task.GetAwaiter();

            public Response() : base()
            {
            }
#endif
            public Response(string json, string requestName = "", string url = "")
                : base(json, requestName, url)
            {
                this.Data = this.GetData<T>();
            }
            public Response(string json, CallerInfo callerInfo)
                 : base(json, callerInfo)
            {
                this.Data = this.GetData<T>();
            }

#if USE_LEGACY_METHODS
            public Response((eHttpStatusCode httpStatusCode, string json) result, string requestName = "", string url = "")
                : base(result, requestName, url)
            {
                this.Data = this.GetData<T>();
            }

            public Response((eHttpStatusCode httpStatusCode, string json) result, CallerInfo callerInfo)
                : base(result, callerInfo)
            {
                this.Data = this.GetData<T>();
            }
#else
            public Response(Func<Response, UniTask<(eHttpStatusCode httpStatusCode, string json)>> requester, string requestName = "", string url = "")
                : base()
            {
                var uniqueName = this.Init(requestName, url);
                this.Task = UniTask.Defer(() => Response.InternalRequest(this, requester, uniqueName, beforeDispatch: () => this.Data = this.GetData<T>()));
            }

            public Response(Func<Response, UniTask<(eHttpStatusCode httpStatusCode, string json)>> requester, CallerInfo callerInfo)
                : base()
            {
                var uniqueName = this.Init(callerInfo);
                this.Task = UniTask.Defer(() => Response.InternalRequest(this, requester, uniqueName, callerInfo.ParamType, beforeDispatch: () => this.Data = this.GetData<T>()));
            }

            public new UniTask<Response<T>?> ManualErrorHandling()
            {
                this.IsManualErrorHandling = true;
                return this.Task;
            }

            public new async void Forget() => await this.Task;
#endif
        }

        internal static string ConvertNullableParametersToQuery(params (string name, Func<string> getValue, Func<bool> hasValue)[] parameters)
        {
            var paramInfoList = parameters
                .Where(e => e.hasValue())
                .Select(e => $"{e.name}={e.getValue()}")
                .ToList();

            string query = "";
            if (paramInfoList != null && paramInfoList.Count > 0)
            {
                query = paramInfoList.Aggregate((a, b) => $"{a}&{b}");
            }

            if (!string.IsNullOrEmpty(query))
            {
                query = query.Insert(0, "?");
            }

            return query;
        }

        public static partial class ArraySupport
        {
            public class Response<T> : Response
            {
                public T[]? Data { get; private set; }

#if !USE_LEGACY_METHODS
                public new UniTask<ArraySupport.Response<T>?> Task { get; private set; } = UniTask.FromResult(default(ArraySupport.Response<T>));
                public new UniTask<ArraySupport.Response<T>?>.Awaiter GetAwaiter() => this.Task.GetAwaiter();

                public Response()
                    : base()
                {

                }
#endif

                public Response(string json, string requestName = "", string url = "")
                    : base(json, requestName, url)
                {
                    Data = this.GetData<T[]?>();
                }

                public Response(string json, CallerInfo callerInfo)
                    : base(json, callerInfo)
                {
                    Data = this.GetData<T[]?>();
                }

#if USE_LEGACY_METHODS
                public Response((eHttpStatusCode httpStatusCode, string json) result, string requestName = "", string url = "")
                    : base(result, requestName, url)
                {
                    Data = this.GetData<T[]?>();
                }

                public Response((eHttpStatusCode httpStatusCode, string json) result, CallerInfo callerInfo)
                    : base(result, callerInfo)
                {
                    Data = this.GetData<T[]?>();
                }
#else
                public Response(Func<Response, UniTask<(eHttpStatusCode httpStatusCode, string json)>> requester, string requestName = "", string url = "")
                    : base()
                {
                    var uniqueName = this.Init(requestName, url);
                    this.Task = UniTask.Defer(() => InternalRequest(this, requester, uniqueName, beforeDispatch: () => this.Data = this.GetData<T[]?>()));
                }

                public Response(Func<Response, UniTask<(eHttpStatusCode httpStatusCode, string json)>> requester, CallerInfo callerInfo)
                    : base()
                {
                    var uniqueName = this.Init(callerInfo);
                    this.Task = UniTask.Defer(() => InternalRequest(this, requester, uniqueName, callerInfo.ParamType, beforeDispatch: () => this.Data = this.GetData<T[]?>()));
                }

                public new UniTask<Response<T>?> ManualErrorHandling()
                {
                    this.IsManualErrorHandling = true;
                    return this.Task;
                }

                public new async void Forget() => await this.Task;
#endif

                public IEnumerator<T>? GetEnumerator() => ((IEnumerable<T>?)Data)!?.GetEnumerator();
            }
        }

        public static partial class Entities
        { 
            /// <summary>
            /// 공용 결과 클래스.
            /// </summary>
            public partial class CheckResult    // Base
            {
                public const eHttpStatusCode    HTTPSTATUSCODE_INTERNAL_ERROR       = (eHttpStatusCode)(-1);
                public const eMiceHttpErrorCode MICEHTTPERRORCODE_INTERNAL_ERROR    = (eMiceHttpErrorCode)(-1);

                public static readonly CheckResult OK = new(eHttpStatusCode.OK, eMiceHttpErrorCode.OK, true);

                /// <summary>
                /// MiceWebClient 내부 오류
                /// </summary>
                public static readonly CheckResult INTERNAL_ERROR = new(HTTPSTATUSCODE_INTERNAL_ERROR, MICEHTTPERRORCODE_INTERNAL_ERROR, false, "empty json");

                public eHttpStatusCode HttpStatusCode { get; private set; }

                public CheckResult() { }

                public CheckResult(eHttpStatusCode httpStatusCode, eMiceHttpErrorCode code, bool result, string reason = "")
                {
                    this.HttpStatusCode = httpStatusCode;
                    this.MiceStatusCode = code;
                    this.Reason = reason;
                    this.Result = result;
                }

                public CheckResult SetHttpStatusCode(eHttpStatusCode code)
                {
                    this.HttpStatusCode = code;
                    return this;
                }

                public static implicit operator bool(CheckResult value) => value.Result;

                public override string ToString()
                    =>  $"[CheckResult] " +
                        $"RESULT:<color=#3A87D6FF>{this.Result}</color> " +
                        $"HTTP:<color=#A0D7A3FF>{this.HttpStatusCode}</color> " +
                        $"MICE:<color=#A0D7A3FF>{this.MiceStatusCode}</color>\r\n" +
                        $"[REASON]\r\n'{this.Reason}'";
            }
        }
    }
}

#if !USE_CLIENT_INTERNALS
namespace Com2Verse.Mice
{
    public static partial class MiceWebClient   // Using Client Get, PUT, POST, DELETE
    {
        private static async UniTask<string> RequestJson<TRestMethod>(TRestMethod restMethod, string url, string content = "", string requestName = "")
            where TRestMethod : Client.BaseNonCallbackRequest
        {
            var response = await restMethod.RequestStringAsync(url, content);
            if (string.IsNullOrWhiteSpace(response)) return string.Empty;

            C2VDebug.Log
            (
                $"[MiceWebClient]({requestName}) request = '{url}' \r\n" +
                $"(json) response = '{response}'"
            );

            return response;
        }

        private static async UniTask<Response<T>> Request<T, TRestMethod>(TRestMethod restMethod, string url, string content = "", string requestName = "")
            where TRestMethod : Client.BaseNonCallbackRequest
            => new Response<T>(await MiceWebClient.RequestJson<TRestMethod>(restMethod, url, content, requestName), requestName, url);

        private static async UniTask<Response> Request<TRestMethod>(TRestMethod restMethod, string url, string content = "", string requestName = "")
            where TRestMethod : Client.BaseNonCallbackRequest
            => new Response(await MiceWebClient.RequestJson<TRestMethod>(restMethod, url, content, requestName), requestName, url);

        private static UniTask<Response<T>> GET<T>(string url, string content = "", [CallerMemberName] string caller = "") where T : class => MiceWebClient.Request<T, Client.ClientGET>(Client.GET, url, content, caller);
        private static UniTask<Response<T>> POST<T>(string url, string content = "", [CallerMemberName] string caller = "") where T : class => MiceWebClient.Request<T, Client.ClientPOST>(Client.POST, url, content, caller);
        private static UniTask<Response<T>> PUT<T>(string url, string content = "", [CallerMemberName] string caller = "") where T : class => MiceWebClient.Request<T, Client.ClientPUT>(Client.PUT, url, content, caller);
        private static UniTask<Response<T>> DELETE<T>(string url, string content = "", [CallerMemberName] string caller = "") where T : class => MiceWebClient.Request<T, Client.ClientDELETE>(Client.DELETE, url, content, caller);

        private static UniTask<Response> GET(string url, string content = "", [CallerMemberName] string caller = "") => MiceWebClient.Request<Client.ClientGET>(Client.GET, url, content, caller);
        private static UniTask<Response> POST(string url, string content = "", [CallerMemberName] string caller = "") => MiceWebClient.Request<Client.ClientPOST>(Client.POST, url, content, caller);
        private static UniTask<Response> PUT(string url, string content = "", [CallerMemberName] string caller = "") => MiceWebClient.Request<Client.ClientPUT>(Client.PUT, url, content, caller);
        private static UniTask<Response> DELETE(string url, string content = "", [CallerMemberName] string caller = "") => MiceWebClient.Request<Client.ClientDELETE>(Client.DELETE, url, content, caller);

        public static partial class ArraySupport
        {
            private static async UniTask<Response<T>> Request<T, TRestMethod>(TRestMethod restMethod, string url, string content = "", string requestName = "")
                where TRestMethod : Client.BaseNonCallbackRequest
                => new ArraySupport.Response<T>(await MiceWebClient.RequestJson<TRestMethod>(restMethod, url, content, requestName), requestName, url);

            public static UniTask<Response<T>> GET<T>(string url, string content = "", [CallerMemberName] string caller = "") where T : class => MiceWebClient.ArraySupport.Request<T, Client.ClientGET>(Client.GET, url, content, caller);
            public static UniTask<Response<T>> POST<T>(string url, string content = "", [CallerMemberName] string caller = "") where T : class => MiceWebClient.ArraySupport.Request<T, Client.ClientPOST>(Client.POST, url, content, caller);
            public static UniTask<Response<T>> PUT<T>(string url, string content = "", [CallerMemberName] string caller = "") where T : class => MiceWebClient.ArraySupport.Request<T, Client.ClientPUT>(Client.PUT, url, content, caller);
            private static UniTask<Response<T>> DELETE<T>(string url, string content = "", [CallerMemberName] string caller = "") where T : class => MiceWebClient.ArraySupport.Request<T, Client.ClientDELETE>(Client.DELETE, url, content, caller);
        }
    }
}
#endif

#if USE_CLIENT_INTERNALS
namespace Com2Verse.Mice
{
    public static partial class MiceWebClient   // Using Client internals...
    {
#nullable restore
        private static readonly BindingFlags BINDING_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod;

        private delegate UniTask<HttpResponseMessage> SendRequestAsyncDelegate(Client.RequestDataSimple request);
        private delegate UniTask<HttpResponseMessage> SendRequestAsync2Delegate(Client.RequestDataMessage request);
        private delegate UniTask<HttpResponseMessage> ValidateResponseMessageAsyncDelegate(HttpResponseMessage response, CancellationTokenSource cts = null);

        private static SendRequestAsyncDelegate sendRequestAsync = null;
        private static SendRequestAsync2Delegate sendRequestAsync2 = null;
        private static ValidateResponseMessageAsyncDelegate validateResponseMessageAsync = null;

        private static bool TryGetDelegate<TDelegate>(Type targetType, string methodName, ref TDelegate dele, Type[] invokeParamTypes = null)
            where TDelegate : Delegate
        {
            if (dele != null) return true;

            MethodInfo method;
            if (invokeParamTypes == null)
            {
                method = targetType.GetMethod(methodName, BINDING_FLAGS);
            }
            else
            {
                method = targetType.GetMethod(methodName, BINDING_FLAGS, null, invokeParamTypes, null);
            }

            UnityEngine.Assertions.Assert.IsNotNull(method, $"'{methodName}' method not found!");

            dele = Delegate.CreateDelegate(typeof(TDelegate), null, method) as TDelegate;

            return dele != null;
        }

        private static bool LazyInitMethods()
            => TryGetDelegate
               (
                   typeof(Client), "SendRequestAsync", ref sendRequestAsync,
                   new[] { typeof(Client.RequestDataSimple) }
               )

            && TryGetDelegate
               (
                   typeof(Client), "SendRequestAsync", ref sendRequestAsync2,
                   new[] { typeof(Client.RequestDataMessage) }
               )

            && TryGetDelegate(typeof(Client), "ValidateResponseMessageAsync", ref validateResponseMessageAsync)
            ;

        public static ServerError LastServerError = null;

        private static async UniTask<(eHttpStatusCode httpStatusCode, string json)> RequestJson(Client.eRequestType requestType, string url, string content = "", string requestName = "", Response responseObj = null)// Func<bool> manualErrorHandling = null)
        {
            eHttpStatusCode statusCode = Entities.CheckResult.HTTPSTATUSCODE_INTERNAL_ERROR;
            string json = null;

            try
            {
                do
                {
                    if (!LazyInitMethods()) break;

                    HttpResponseMessage response;

                    if (!string.IsNullOrEmpty(Network.User.Instance.CurrentUserData.AccessToken))
                    {
                        Client.Auth.SetTokenAuthentication(Util.MakeTokenAuthInfo(Network.User.Instance.CurrentUserData.AccessToken));
                    }

                    if (requestType == Client.eRequestType.POST)
                    {
                        var builder = HttpRequestBuilder.CreateNew(Client.eRequestType.POST, url);
                        builder.SetContent(content);
                        builder.SetContentType(Client.Constant.ContentJson);

                        response = await sendRequestAsync2
                        (
                            new()
                            {
                                Message = builder.Request,
                                Option = HttpCompletionOption.ResponseHeadersRead,
                                CanResend = true,
                                Cts = null
                            }
                        );
                    }
                    else
                    {
                        response = await sendRequestAsync
                        (
                            new()
                            {
                                RequestType = requestType,
                                Url = url,
                                Content = content,
                                CanResend = true,
                                Cts = null,
                            }
                        );
                    }

                    await validateResponseMessageAsync(response);

                    try
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    catch (Exception e)
                    {
                        C2VDebug.Log(e.Message);
                    }

                    var statusSuccess = response.IsSuccessStatusCode;
                    statusCode = response.StatusCode;
                    json = await response.Content.ReadAsStringAsync();

#if UNITY_EDITOR && TEST_SERVER_ERROR
            statusSuccess = false;
            statusCode = eHttpStatusCode.InternalServerError;
            json =
@"{
	""type"": ""https://tools.ietf.org/html/rfc7231#section-6.6.1"",
	""title"": ""Table 'mice.account_card' doesn't exist"",
	""status"": 500,
	""detail"": ""   at MySqlConnector.Core.ServerSession.ReceiveReplyAsyncAwaited(ValueTask`1 task) in /_/src/MySqlConnector/Core/ServerSession.cs:line 964\n   at MySqlConnector.Core.ResultSet.ReadResultSetHeaderAsync(IOBehavior ioBehavior) in /_/src/MySqlConnector/Core/ResultSet.cs:line 175\n   at MySqlConnector.MySqlDataReader.ActivateResultSet(CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 133\n   at MySqlConnector.MySqlDataReader.CreateAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, IDictionary`2 cachedProcedures, IMySqlCommand command, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 493\n   at MySqlConnector.Core.CommandExecutor.ExecuteReaderAsync(IReadOnlyList`1 commands, ICommandPayloadCreator payloadCreator, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/CommandExecutor.cs:line 77\n   at MySqlConnector.MySqlCommand.ExecuteReaderAsync(CommandBehavior behavior, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlCommand.cs:line 345\n   at MySqlConnector.MySqlCommand.ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlCommand.cs:line 337\n   at Dapper.SqlMapper.QueryAsync[T](IDbConnection cnn, Type effectiveType, CommandDefinition command) in /_/Dapper/SqlMapper.Async.cs:line 459\n   at C2VServerMiceCommon.Repositories.UserRepository.GetAccountCardListAsync(Int64 accountId) in /src/C2VServerMiceCommon/Repositories/UserRepository.cs:line 595\n   at C2VServerMiceWeb.Controllers.Api.UserController.GetAccountCardListAsync() in /src/C2VServerMiceWeb/Controllers/Api/UserController.cs:line 148\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask`1 actionResultValueTask)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)\n   at Microsoft.AspNetCore.Routing.EndpointMiddleware.<Invoke>g__AwaitRequestTask|6_0(Endpoint endpoint, Task requestTask, ILogger logger)\n   at C2VServerMiceWeb.Middleware.UserClaimsMiddleware.InvokeAsync(HttpContext httpContext) in /src/C2VServerMiceWeb/Middleware/UserClaimsMiddleware.cs:line 49\n   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)\n   at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)\n   at Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddlewareImpl.<Invoke>g__Awaited|8_0(ExceptionHandlerMiddlewareImpl middleware, HttpContext context, Task task)"",
	""traceId"": ""00-e4e514d273feeb91aa76131a7bdfb0ae-d092dad798d0f846-00""
}";
#endif

#if !USE_LEGACY_METHODS
                    var isManualErrorHandling = responseObj?.IsManualErrorHandling ?? false;
#endif

                    C2VDebug.Log
                    (
                        $"[MiceWebClient] request = '{url}' {(isManualErrorHandling ? "(ManualEH)" : "")}\r\n" +
                        $"<color=#A0D7A3FF>(http status)</color> {statusCode}\r\n" +
                        $"<color=#A0D7A3FF>(json response)</color>\r\n{json}"
                    );

                    if (!statusSuccess && ServerError.TryParse(json, out var inst))
                    {
                        MiceWebClient.LastServerError = inst;

                        var resultJson = json; 
                        json = string.Empty;

                        var msg = $" [{requestName}]({MiceWebClient.URLToAPIInfo(url)})";
                        C2VDebug.Log(msg);

#if !USE_LEGACY_METHODS
                        if (!isManualErrorHandling)
#endif
                        {
                            // 실패 결과 있을때.
                            var checkResult = JsonConvert.DeserializeObject<Entities.CheckResult>(resultJson);
                            if(checkResult != null)
                            {
                                MiceWebClient.ShowErrorMessage(statusCode, checkResult);
                            }
                            else // 실패 결과 없을때.
                            {
                                var uiMsg = Localization.Instance.GetErrorString((int)Protocols.ErrorCode.DbError);
                                MiceWebClient.ShowErrorMessage(uiMsg);
                            }
                        }
                    }

                } while (false);
            }
            catch (Exception e)
            {
                throw new Exception("Exception Occurred.", e);
            }

            return (statusCode, json);
        }

        private static string URLToAPIInfo(string url)
        {
            int pos = url.IndexOf("/api");
            if (pos < 0) return string.Empty;

            return url.Substring(pos);
        }

        private static void ShowErrorMessage(eHttpStatusCode statusCode, Entities.CheckResult result)
        {
            var loadingView = UIManager.Instance.GetSystemView(eSystemViewType.UI_LOADING_PAGE);
            loadingView?.Hide();

            if (statusCode == eHttpStatusCode.Conflict)
            {
                NetworkUIManager.Instance.ShowMiceWebApiErrorMessage(result.MiceStatusCode);               
            }
            else
            {
                var uiMsg = Localization.Instance.GetErrorString((int)Protocols.ErrorCode.DbError);
                UIManager.Instance.ShowPopupCommon(uiMsg, yes: Data.Localization.eKey.UI_Common_Btn_OK.ToLocalizationString());
            }
        }
        
        private static void ShowErrorMessage(string context)
        {
            var loadingView = UIManager.Instance.GetSystemView(eSystemViewType.UI_LOADING_PAGE);
            loadingView?.Hide();

            UIManager.Instance.ShowPopupCommon(context, yes: Data.Localization.eKey.UI_Common_Btn_OK.ToLocalizationString());
        }

#nullable enable

#if !USE_LEGACY_METHODS
        private static Response<T> Request<T>(Client.eRequestType requestType, string url, string content, string requestName = "")
            => new(r => MiceWebClient.RequestJson(requestType, url, content, requestName, r), requestName, url);
        private static Response Request(Client.eRequestType requestType, string url, string content, string requestName = "")
            => new(r => MiceWebClient.RequestJson(requestType, url, content, requestName, r), requestName, url);
        private static Response<T> Request<T>(Client.eRequestType eRequestType, string url, string content, string requestName = "", CallerInfo callerInfo = default)
            => new(r => MiceWebClient.RequestJson(eRequestType, url, content, requestName, r), callerInfo);
        private static Response Request(Client.eRequestType eRequestType, string url, string content, string requestName = "", CallerInfo callerInfo = default)
            => new(r => MiceWebClient.RequestJson(eRequestType, url, content, requestName, r), callerInfo);

        private static Response<T> GET<T>(string url, string content = "", [CallerMemberName] string caller = "") where T : class => MiceWebClient.Request<T>(Client.eRequestType.GET, url, content, caller);
        private static Response<T> POST<T>(string url, string content = "", [CallerMemberName] string caller = "") where T : class => MiceWebClient.Request<T>(Client.eRequestType.POST, url, content, caller);
        private static Response<T> PUT<T>(string url, string content = "", [CallerMemberName] string caller = "") where T : class => MiceWebClient.Request<T>(Client.eRequestType.PUT, url, content, caller);
        private static Response<T> DELETE<T>(string url, string content = "", [CallerMemberName] string caller = "") where T : class => MiceWebClient.Request<T>(Client.eRequestType.DELETE, url, content, caller);

        private static Response GET(string url, string content = "", [CallerMemberName] string caller = "") => MiceWebClient.Request(Client.eRequestType.GET, url, content, caller);
        private static Response POST(string url, string content = "", [CallerMemberName] string caller = "") => MiceWebClient.Request(Client.eRequestType.POST, url, content, caller);
        private static Response PUT(string url, string content = "", [CallerMemberName] string caller = "") => MiceWebClient.Request(Client.eRequestType.PUT, url, content, caller);
        private static Response DELETE(string url, string content = "", [CallerMemberName] string caller = "") => MiceWebClient.Request(Client.eRequestType.DELETE, url, content, caller);

        private static Response<T> GET<T>(string url, string content, CallerInfo callerInfo, [CallerMemberName] string caller = "") where T : class => MiceWebClient.Request<T>(Client.eRequestType.GET, url, content, caller, callerInfo);
        private static Response<T> POST<T>(string url, string content, CallerInfo callerInfo, [CallerMemberName] string caller = "") where T : class => MiceWebClient.Request<T>(Client.eRequestType.POST, url, content, caller, callerInfo);
        private static Response<T> PUT<T>(string url, string content, CallerInfo callerInfo, [CallerMemberName] string caller = "") where T : class => MiceWebClient.Request<T>(Client.eRequestType.PUT, url, content, caller, callerInfo);
        private static Response<T> DELETE<T>(string url, string content, CallerInfo callerInfo, [CallerMemberName] string caller = "") where T : class => MiceWebClient.Request<T>(Client.eRequestType.DELETE, url, content, caller, callerInfo);

        private static Response GET(string url, string content, CallerInfo callerInfo, [CallerMemberName] string caller = "") => MiceWebClient.Request(Client.eRequestType.GET, url, content, caller, callerInfo);
        private static Response POST(string url, string content, CallerInfo callerInfo, [CallerMemberName] string caller = "") => MiceWebClient.Request(Client.eRequestType.POST, url, content, caller, callerInfo);
        private static Response PUT(string url, string content, CallerInfo callerInfo, [CallerMemberName] string caller = "") => MiceWebClient.Request(Client.eRequestType.PUT, url, content, caller, callerInfo);
        private static Response DELETE(string url, string content, CallerInfo callerInfo, [CallerMemberName] string caller = "") => MiceWebClient.Request(Client.eRequestType.DELETE, url, content, caller, callerInfo);
#else
        private static async UniTask<Response<T>> Request<T>(Client.eRequestType eRequestType, string url, string content, string requestName = "")
            => new Response<T>(await MiceWebClient.RequestJson(eRequestType, url, content, requestName), requestName, url);

        private static async UniTask<Response> Request(Client.eRequestType eRequestType, string url, string content, string requestName = "")
            => new Response(await MiceWebClient.RequestJson(eRequestType, url, content, requestName), requestName, url);

        private static async UniTask<Response<T>> Request<T>(Client.eRequestType eRequestType, string url, string content, string requestName = "", CallerInfo callerInfo = default)
            => new Response<T>(await MiceWebClient.RequestJson(eRequestType, url, content, requestName), callerInfo);

        private static async UniTask<Response> Request(Client.eRequestType eRequestType, string url, string content, string requestName = "", CallerInfo callerInfo = default)
            => new Response(await MiceWebClient.RequestJson(eRequestType, url, content, requestName), callerInfo);

        private static UniTask<Response<T>> GET<T>(string url, string content = "", [CallerMemberName] string caller = "") where T : class => MiceWebClient.Request<T>(Client.eRequestType.GET, url, content, caller);
        private static UniTask<Response<T>> POST<T>(string url, string content = "", [CallerMemberName] string caller = "") where T : class => MiceWebClient.Request<T>(Client.eRequestType.POST, url, content, caller);
        private static UniTask<Response<T>> PUT<T>(string url, string content = "", [CallerMemberName] string caller = "") where T : class => MiceWebClient.Request<T>(Client.eRequestType.PUT, url, content, caller);
        private static UniTask<Response<T>> DELETE<T>(string url, string content = "", [CallerMemberName] string caller = "") where T : class => MiceWebClient.Request<T>(Client.eRequestType.DELETE, url, content, caller);

        private static UniTask<Response> GET(string url, string content = "", [CallerMemberName] string caller = "") => MiceWebClient.Request(Client.eRequestType.GET, url, content, caller);
        private static UniTask<Response> POST(string url, string content = "", [CallerMemberName] string caller = "") => MiceWebClient.Request(Client.eRequestType.POST, url, content, caller);
        private static UniTask<Response> PUT(string url, string content = "", [CallerMemberName] string caller = "") => MiceWebClient.Request(Client.eRequestType.PUT, url, content, caller);
        private static UniTask<Response> DELETE(string url, string content = "", [CallerMemberName] string caller = "") => MiceWebClient.Request(Client.eRequestType.DELETE, url, content, caller);

        private static UniTask<Response<T>> GET<T>(string url, string content, CallerInfo callerInfo, [CallerMemberName] string caller = "") where T : class => MiceWebClient.Request<T>(Client.eRequestType.GET, url, content, caller, callerInfo);
        private static UniTask<Response<T>> POST<T>(string url, string content, CallerInfo callerInfo, [CallerMemberName] string caller = "") where T : class => MiceWebClient.Request<T>(Client.eRequestType.POST, url, content, caller, callerInfo);
        private static UniTask<Response<T>> PUT<T>(string url, string content, CallerInfo callerInfo, [CallerMemberName] string caller = "") where T : class => MiceWebClient.Request<T>(Client.eRequestType.PUT, url, content, caller, callerInfo);
        private static UniTask<Response<T>> DELETE<T>(string url, string content, CallerInfo callerInfo, [CallerMemberName] string caller = "") where T : class => MiceWebClient.Request<T>(Client.eRequestType.DELETE, url, content, caller, callerInfo);

        private static UniTask<Response> GET(string url, string content, CallerInfo callerInfo, [CallerMemberName] string caller = "") => MiceWebClient.Request(Client.eRequestType.GET, url, content, caller, callerInfo);
        private static UniTask<Response> POST(string url, string content, CallerInfo callerInfo, [CallerMemberName] string caller = "") => MiceWebClient.Request(Client.eRequestType.POST, url, content, caller, callerInfo);
        private static UniTask<Response> PUT(string url, string content, CallerInfo callerInfo, [CallerMemberName] string caller = "") => MiceWebClient.Request(Client.eRequestType.PUT, url, content, caller, callerInfo);
        private static UniTask<Response> DELETE(string url, string content, CallerInfo callerInfo, [CallerMemberName] string caller = "") => MiceWebClient.Request(Client.eRequestType.DELETE, url, content, caller, callerInfo);
#endif

        public static partial class ArraySupport
        {
#if !USE_LEGACY_METHODS
            public static ArraySupport.Response<T> Request<T>(Client.eRequestType requestType, string url, string content, string requestName = "")
                => new(r => MiceWebClient.RequestJson(requestType, url, content, requestName, r), requestName, url);
            private static ArraySupport.Response<T> Request<T>(Client.eRequestType requestType, string url, string content, string requestName = "", CallerInfo callerInfo = default)
                => new(r => MiceWebClient.RequestJson(requestType, url, content, requestName, r), callerInfo);

            public static Response<T> GET<T>(string url, string content = "", [CallerMemberName] string caller = "") where T : class => MiceWebClient.ArraySupport.Request<T>(Client.eRequestType.GET, url, content, caller);
            public static Response<T> POST<T>(string url, string content = "", [CallerMemberName] string caller = "") where T : class => MiceWebClient.ArraySupport.Request<T>(Client.eRequestType.POST, url, content, caller);
            public static Response<T> PUT<T>(string url, string content = "", [CallerMemberName] string caller = "") where T : class => MiceWebClient.ArraySupport.Request<T>(Client.eRequestType.PUT, url, content, caller);
            private static Response<T> DELETE<T>(string url, string content = "", [CallerMemberName] string caller = "") where T : class => MiceWebClient.ArraySupport.Request<T>(Client.eRequestType.DELETE, url, content, caller);

            public static Response<T> GET<T>(string url, string content, CallerInfo callerInfo, [CallerMemberName] string caller = "") where T : class => MiceWebClient.ArraySupport.Request<T>(Client.eRequestType.GET, url, content, caller, callerInfo);
            public static Response<T> POST<T>(string url, string content, CallerInfo callerInfo, [CallerMemberName] string caller = "") where T : class => MiceWebClient.ArraySupport.Request<T>(Client.eRequestType.POST, url, content, caller, callerInfo);
            public static Response<T> PUT<T>(string url, string content, CallerInfo callerInfo, [CallerMemberName] string caller = "") where T : class => MiceWebClient.ArraySupport.Request<T>(Client.eRequestType.PUT, url, content, caller, callerInfo);
            private static Response<T> DELETE<T>(string url, string content, CallerInfo callerInfo, [CallerMemberName] string caller = "") where T : class => MiceWebClient.ArraySupport.Request<T>(Client.eRequestType.DELETE, url, content, caller, callerInfo);
#else
            private static async UniTask<Response<T>> Request<T>(Client.eRequestType requestType, string url, string content, string requestName = "")
                => new ArraySupport.Response<T>(await MiceWebClient.RequestJson(requestType, url, content, requestName), requestName, url);

            private static async UniTask<Response<T>> Request<T>(Client.eRequestType requestType, string url, string content, string requestName = "", CallerInfo callerInfo = default)
                => new ArraySupport.Response<T>(await MiceWebClient.RequestJson(requestType, url, content, requestName), callerInfo);

            public static UniTask<Response<T>> GET<T>(string url, string content = "", [CallerMemberName] string caller = "") where T : class => MiceWebClient.ArraySupport.Request<T>(Client.eRequestType.GET, url, content, caller);
            public static UniTask<Response<T>> POST<T>(string url, string content = "", [CallerMemberName] string caller = "") where T : class => MiceWebClient.ArraySupport.Request<T>(Client.eRequestType.POST, url, content, caller);
            public static UniTask<Response<T>> PUT<T>(string url, string content = "", [CallerMemberName] string caller = "") where T : class => MiceWebClient.ArraySupport.Request<T>(Client.eRequestType.PUT, url, content, caller);
            private static UniTask<Response<T>> DELETE<T>(string url, string content = "", [CallerMemberName] string caller = "") where T : class => MiceWebClient.ArraySupport.Request<T>(Client.eRequestType.DELETE, url, content, caller);

            public static UniTask<Response<T>> GET<T>(string url, string content, CallerInfo callerInfo, [CallerMemberName] string caller = "") where T : class => MiceWebClient.ArraySupport.Request<T>(Client.eRequestType.GET, url, content, caller, callerInfo);
            public static UniTask<Response<T>> POST<T>(string url, string content, CallerInfo callerInfo, [CallerMemberName] string caller = "") where T : class => MiceWebClient.ArraySupport.Request<T>(Client.eRequestType.POST, url, content, caller, callerInfo);
            public static UniTask<Response<T>> PUT<T>(string url, string content, CallerInfo callerInfo, [CallerMemberName] string caller = "") where T : class => MiceWebClient.ArraySupport.Request<T>(Client.eRequestType.PUT, url, content, caller, callerInfo);
            private static UniTask<Response<T>> DELETE<T>(string url, string content, CallerInfo callerInfo, [CallerMemberName] string caller = "") where T : class => MiceWebClient.ArraySupport.Request<T>(Client.eRequestType.DELETE, url, content, caller, callerInfo);
#endif
        }
    }
}
#endif

#nullable disable
namespace Com2Verse.Mice
{
    public static partial class MiceWebClient
    {
        [Serializable]
        public class ServerError
        {
            [JsonProperty("type")] public string Type { get; set; }
            [JsonProperty("title")] public string Title { get; set; }
            [JsonProperty("status")] public int Status { get; set; }
            [JsonProperty("detail")] public string Detail { get; set; }
            [JsonProperty("traceId")] public string TraceId { get; set; }

            public HttpStatusCode StatusCode => (HttpStatusCode)this.Status;

            public override string ToString() => this.ToString(false);

            public string ToString(bool withoutDetails, string additionalInfo = "")
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                sb.AppendLine($"{this.StatusCode}({this.Status}){additionalInfo}");
                sb.AppendLine($"{this.Title}");
                if (!withoutDetails)
                {
                    sb.AppendLine($"[Type]    {this.Type}");
                    sb.AppendLine($"[TraceId] {this.TraceId}");
                    var details = string.IsNullOrEmpty(this.Detail) ? "(empty)" : this.Detail.Split("\n").Aggregate((a, b) => $"{a}\r\n{b}");
                    sb.AppendLine($"[Detail]\r\n{details}");
                }

                return sb.ToString();
            }

            public static bool TryParse(string json, out ServerError result)
            {
                result = null;

                try
                {
                    result = JsonConvert.DeserializeObject<ServerError>(json);
                }
                catch
                {
                    // Ignore exceptions.
                }

                return result != null;
            }
        }
    }
}

