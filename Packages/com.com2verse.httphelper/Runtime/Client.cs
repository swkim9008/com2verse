/*===============================================================
* Product:		Com2Verse
* File Name:	Client.cs
* Developer:	jhkim
* Date:			2023-02-02 16:01
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Com2Verse.HttpHelper
{
    public static class Client
    {
#region Enum
        private enum eAuthorizationType
        {
            BASIC,
            AUTH_TOKEN,
        }

        public enum eRequestType
        {
            GET,
            POST,
            PUT,
            DELETE,
        }
#endregion // Enum

#region Timeout
        interface ITimeout
        {
            public bool HasPending { get; set; }
            public bool HasTimeout { get; set; }
        }

        internal class TimeoutTimerData
        {
            public long                    Timer;
            public CancellationTokenSource Cts;
        }
        
        private static          int _pendingTime = 2000;
        private static          int _timeoutTime = 30000;
        private static readonly int PumpingTime  = 10;

        private static readonly ConcurrentDictionary<string, long>             PendingTimerDic = new();
        private static readonly ConcurrentDictionary<string, TimeoutTimerData> TimeoutTimerDic = new();
        private static readonly List<string>                                   RemoveKeys      = new();

        private static readonly List<Action> OnPendingEventStack = new();
        public static event Action OnPendingEvent
        {
            add => OnPendingEventStack!.Insert(0, value);
            remove => OnPendingEventStack!.Remove(value);
        }
        private static readonly List<Action> OnTimeoutEventStack = new();
        public static event Action OnTimeoutEvent
        {
            add => OnTimeoutEventStack!.Insert(0, value);
            remove => OnTimeoutEventStack!.Remove(value);
        }

        private static readonly List<Action> OnTimerClearEventStack = new();
        public static event Action OnTimerClearEvent
        {
            add => OnTimerClearEventStack!.Insert(0, value);
            remove => OnTimerClearEventStack!.Remove(value);
        }

        public static void SetWebRequestTimeout(float pending, float timeout)
        {
            _pendingTime = (int)(pending * 1000);
            _timeoutTime = (int)(timeout * 1000);
        }

        private static string PushPacketIntoTimer(RequestDataMessage message, CancellationTokenSource cts)
        {
            string guid = Guid.NewGuid().ToString();
            if (!message.HasPending || !PendingTimerDic!.TryAdd(guid, 0)) return string.Empty;
            if (!message.HasTimeout || !TimeoutTimerDic!.TryAdd(guid, new TimeoutTimerData { Timer = 0, Cts = cts, })) return string.Empty;
            UpdateTimeoutTimer().Forget();
            return guid;
        }

        private static string PushPacketIntoTimer(RequestDataSimple message, CancellationTokenSource cts)
        {
            string guid = Guid.NewGuid().ToString();
            if (!message.HasPending || !PendingTimerDic!.TryAdd(guid, 0)) return string.Empty;
            if (!message.HasTimeout || !TimeoutTimerDic!.TryAdd(guid, new TimeoutTimerData { Timer = 0, Cts = cts, })) return string.Empty;
            UpdateTimeoutTimer().Forget();
            return guid;
        }

        private static void PopPacketFromTimer(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return;
            PendingTimerDic.Remove(guid, out _);
            TimeoutTimerDic.Remove(guid, out _);
            if (OnTimerClearEventStack!.Count > 0)
                OnTimerClearEventStack[0]?.Invoke();
        }
        
        private static async UniTaskVoid UpdateTimeoutTimer()
        {
            if (TimeoutTimerDic!.Count > 1) return;
            while (TimeoutTimerDic.Count > 0)
            {
                await UniTask.Delay(PumpingTime);
                foreach (var key in PendingTimerDic!.Keys)
                {
                    PendingTimerDic[key!] += PumpingTime;
                    if (PendingTimerDic[key] > _pendingTime)
                    {
                        if (OnPendingEventStack!.Count > 0)
                            OnPendingEventStack[0]?.Invoke();
                        RemoveKeys!.Add(key);
                    }
                }
                RemoveKeys!.ForEach((key) => PendingTimerDic.Remove(key!, out _));
                RemoveKeys.Clear();
                foreach (var key in TimeoutTimerDic.Keys)
                {
                    TimeoutTimerDic[key!]!.Timer += PumpingTime;
                    if (TimeoutTimerDic[key]!.Timer > _timeoutTime)
                    {
                        if (OnTimeoutEventStack!.Count > 0)
                            OnTimeoutEventStack[0]?.Invoke();
                        RemoveKeys.Add(key);
                        TimeoutTimerDic[key].Cts!.Cancel();
                    }
                }
                RemoveKeys.ForEach((key) => TimeoutTimerDic.Remove(key!, out _));
            }
        }
#endregion // Timeout

#region Variables
        internal static readonly int MaxConcurrency = 4;

        private static Dictionary<eAuthorizationType, string> _authTypeKey = new Dictionary<eAuthorizationType, string>()
        {
            {eAuthorizationType.BASIC, "Basic"},
            {eAuthorizationType.AUTH_TOKEN, Util.KeyAuthtoken},
        };

        private const string DefaultBasicAuthFormat = "{0}:{1}";
        private static int _concurrentRequest = 0;

        private static readonly ConcurrentQueue<RequestInfo> RequestQueue = new ConcurrentQueue<RequestInfo>();

        // Non-Callback Request
        public static readonly ClientGet GET = new();
        public static readonly ClientPost POST = new();
        public static readonly ClientPut PUT = new();
        public static readonly ClientDelete DELETE = new();
        public static readonly ClientMessage Message = new();

        public static readonly ClientRequest Request = new();
        public static readonly ClientSettings Settings = new();
        public static readonly ClientAuth Auth = new();
        public static readonly ClientDebug Debug = new();

        public static readonly ClientConstant Constant = new();

        private static readonly int AccessTokenRequestMax = 3;
        private static int _accessTokenRequestCount = 0;

        private static Func<UniTask<bool>> _onAccessTokenExpired = async () => false;
        private static Action _onRefreshTokenExpired = () => { };
        private static Action _onAccessTokenRetryExceed = () => { };

        private static readonly string LogHttpHelper = nameof(HttpHelper);
#endregion // Variable

#region Wrapper
#region Wrapper - Base
        public abstract class BaseNonCallbackRequest : ITimeout
        {
            protected eRequestType RequestType;

            public virtual bool HasPending { get; set; } = true;
            public virtual bool HasTimeout { get; set; } = true;

            /// <summary>
            /// Http 웹 요청 (Non-Callback)
            /// </summary>
            /// <param name="url">요청 URL</param>
            /// <param name="content">내용이 필요한 경우 추가 (POST, PUT 등)</param>
            /// <param name="cts">CTS</param>
            /// <param name="requestOption">요청 옵션</param>
            /// <returns>string</returns>
            public async UniTask<ResponseBase<string>> RequestStringAsync(string url, string content = "", CancellationTokenSource cts = null, RequestOption requestOption = null)
            {
                requestOption ??= RequestOption.Default;
                var request = new RequestDataSimple(RequestType, url) {Content = content, Cts = cts, HasPending = requestOption.HasPending, HasTimeout = requestOption.HasTimeout};
                return await RequestStringAsync(request);
            }

            private async UniTask<ResponseBase<string>> RequestStringAsync(RequestDataSimple request) => await SendRequestStringAsync(request);

            /// <summary>
            /// Http 웹 요청 (Non-Callback)
            /// </summary>
            /// <param name="url">요청 URL</param>
            /// <param name="content">내용이 필요한 경우 추가 (POST, PUT 등)</param>
            /// <param name="cts">CTS</param>
            /// <param name="requestOption">요청 옵션</param>
            /// <returns>Stream</returns>
            public async UniTask<ResponseStream> RequestStreamAsync(string url, string content = "", CancellationTokenSource cts = null, RequestOption requestOption = null)
            {
                requestOption ??= RequestOption.Default;
                var request = new RequestDataSimple(RequestType, url) {Content = content, Cts = cts, HasPending = requestOption.HasPending, HasTimeout = requestOption.HasTimeout};
                return await RequestStreamAsync(request);
            }

            private async UniTask<ResponseStream> RequestStreamAsync(RequestDataSimple request) => await SendRequestStreamAsync(request);

            /// <summary>
            /// Http 웹 요청 (Non-Callback)
            /// </summary>
            /// <param name="url">요청 URL</param>
            /// <param name="content">내용이 필요한 경우 추가 (POST, PUT 등)</param>
            /// <param name="cts">CTS</param>
            /// <param name="requestOption">요청 옵션</param>
            /// <typeparam name="T">Response 오브젝트 타입 (Json 파싱이 가능한)</typeparam>
            /// <returns></returns>
            public async UniTask<ResponseBase<T>> RequestAsync<T>(string url, string content = "", CancellationTokenSource cts = null, RequestOption requestOption = null)
            {
                requestOption ??= RequestOption.Default;
                var request = new RequestDataSimple(RequestType, url) { Content = content, Cts = cts, HasPending = requestOption.HasPending, HasTimeout = requestOption.HasTimeout};
                return await RequestAsync<T>(request);
            }

            private async UniTask<ResponseBase<T>> RequestAsync<T>(RequestDataSimple request) => await SendRequestObjectAsync<T>(request);
        }
#endregion // Wrapper - Base

#region Wrapper - Non-Callback Request
        public class ClientGet : BaseNonCallbackRequest
        {
            internal ClientGet() => RequestType = eRequestType.GET;
        }

        public class ClientPost : BaseNonCallbackRequest
        {
            internal ClientPost() => RequestType = eRequestType.POST;
        }

        public class ClientPut : BaseNonCallbackRequest
        {
            internal ClientPut() => RequestType = eRequestType.PUT;
        }

        public class ClientDelete : BaseNonCallbackRequest
        {
            internal ClientDelete() => RequestType = eRequestType.DELETE;
        }

        public class ClientMessage : ITimeout
        {
            internal ClientMessage() { }

            public virtual bool HasPending { get; set; } = true;
            public virtual bool HasTimeout { get; set; } = true;

            /// <summary>
            /// Http 웹 요청 (Non-Callback)
            /// </summary>
            /// <param name="message">요청 메시지</param>
            /// <param name="cts">CTS</param>
            /// <param name="requestOption">요청 옵션</param>
            /// <param name="option">응답 옵션 (기본값: HttpCompletionOption.ResponseHeadersRead)</param>
            /// <returns>string</returns>
            public async UniTask<ResponseBase<string>> RequestStringAsync(HttpRequestMessage message, CancellationTokenSource cts = null, RequestOption requestOption = null, HttpCompletionOption option = HttpCompletionOption.ResponseHeadersRead)
            {
                requestOption ??= RequestOption.Default;
                var request = new RequestDataMessage(message) { Cts = cts, Option = option, HasPending = requestOption.HasPending, HasTimeout = requestOption.HasTimeout};
                return await RequestStringAsync(request);
            }
            public async UniTask<ResponseBase<string>> RequestStringAsync(RequestDataMessage request) => await SendRequestStringAsync(request);

            /// <summary>
            /// Http 웹 요청 (Non-Callback)
            /// </summary>
            /// <param name="message">요청 메시지</param>
            /// <param name="cts">CTS</param>
            /// <param name="requestOption">요청 옵션</param>
            /// <param name="option">응답 옵션 (기본값: HttpCompletionOption.ResponseHeadersRead)</param>
            /// <returns>Stream</returns>
            public async UniTask<ResponseStream> RequestStreamAsync(HttpRequestMessage message, CancellationTokenSource cts = null, RequestOption requestOption = null, HttpCompletionOption option = HttpCompletionOption.ResponseHeadersRead)
            {
                requestOption ??= RequestOption.Default;
                var request = new RequestDataMessage(message) { Cts = cts, Option = option, HasPending = requestOption.HasPending, HasTimeout = requestOption.HasTimeout};
                return await RequestStreamAsync(request);
            }

            private async UniTask<ResponseStream> RequestStreamAsync(RequestDataMessage infoMessage) => await SendRequestStreamAsync(infoMessage);

            /// <summary>
            /// Http 웹 요청 (Non-Callback)
            /// </summary>
            /// <param name="message">요청 메시지</param>
            /// <param name="cts">CTS</param>
            /// <param name="requestOption">요청 옵션</param>
            /// <param name="option">응답 옵션 (기본값: HttpCompletionOption.ResponseHeaderRead)</param>
            /// <typeparam name="T">Response 오브젝트 타입 (Json 파싱이 가능한)</typeparam>
            /// <returns></returns>
            public async UniTask<ResponseBase<T>> Request<T>(HttpRequestMessage message, CancellationTokenSource cts = null, RequestOption requestOption = null, HttpCompletionOption option = HttpCompletionOption.ResponseHeadersRead)
            {
                requestOption ??= RequestOption.Default;
                var request = new RequestDataMessage(message) { Cts = cts, Option = option, HasPending = requestOption.HasPending, HasTimeout = requestOption.HasTimeout};
                return await Request<T>(request);
            }

            private async UniTask<ResponseBase<T>> Request<T>(RequestDataMessage request) => await SendRequestObjectAsync<T>(request);
        }
#endregion // Wrapper - Non-Callback Request

#region Wrapper - With-Callback Request
        public class ClientRequest : ITimeout
        {
            internal ClientRequest() { }

            public virtual bool HasPending { get; set; } = true;
            public virtual bool HasTimeout { get; set; } = true;

            /// <summary>
            /// Http 웹 요청 (With-Callback)
            /// IRequestHandler를 통해 다운로드 상태에 대한 제어 가능
            /// </summary>
            /// <param name="requestType">요청 타입</param>
            /// <param name="callbacks">콜백 이벤트 등록</param>
            /// <param name="url">요청 URL</param>
            /// <param name="content">내용이 필요한 경우 추가 (POST, PUT 등)</param>
            /// <param name="cts">CTS</param>
            /// <param name="requestOption">요청 옵션</param>
            /// <returns></returns>
            [Obsolete("현재 사용되지 않는 기능입니다.")]
            public async UniTask<IRequestHandler> CreateRequestWithCallbackAsync(eRequestType requestType, Callbacks callbacks, string url, string content = "", CancellationTokenSource cts = null, RequestOption requestOption = null)
            {
                requestOption ??= RequestOption.Default;
                var requestInfo = new RequestDataSimple(requestType, url) { Content = content, Cts = cts, HasPending = requestOption.HasPending, HasTimeout = requestOption.HasTimeout};
                var response    = await SendRequestAsync(requestInfo);
                if (response == null || cts is {IsCancellationRequested: true}) return null;

                var request = RequestHandler.New(callbacks, response, cts);
                request.URL = url;
                request.Content = content;
                return request;
            }

            /// <summary>
            /// Http 웹 요청 (With-Callback)
            /// IRequestHandler를 통해 다운로드 상태에 대한 제어 가능
            /// </summary>
            /// <param name="message">요청 메시지</param>
            /// <param name="callbacks">콜백 이벤트 등록</param>
            /// <param name="cts">CTS</param>
            /// <param name="requestOption">요청 옵션</param>
            /// <returns></returns>
            [Obsolete("현재 사용되지 않는 기능입니다.")]
            public async UniTask<IRequestHandler> CreateRequestWithCallbackAsync(HttpRequestMessage message, Callbacks callbacks, CancellationTokenSource cts = null, RequestOption requestOption = null)
            {
                requestOption ??= RequestOption.Default;
                var requestInfo = new RequestDataMessage(message) { Cts = cts, HasPending = requestOption.HasPending, HasTimeout = requestOption.HasTimeout};
                var response    = await SendRequestAsync(requestInfo);
                if (response == null || cts is {IsCancellationRequested: true}) return null;

                var request = RequestHandler.New(callbacks, response, cts);
                request.URL = message.RequestUri.OriginalString;
                return request;
            }
        }
#endregion // Wrapper - With-Callback Request

#region Wrapper - Settings
        public class ClientSettings
        {
            internal ClientSettings() { }

            /// <summary>
            /// HttpClient의 BaseAddress를 지정
            /// </summary>
            /// <param name="url">BaseAddress가 포함된 url</param>
            public void SetBaseAddress(string url) => Client.SetBaseAddress(url);

            /// <summary>
            /// HttpClient의 BaseAddress 확인
            /// </summary>
            public string BaseAddress => GetClient().BaseAddress?.Host;
        }
#endregion // Wrapper - Settings

#region Wrapper - Auth
        public class ClientAuth : ITimeout
        {
            internal ClientAuth() { }

            public virtual bool HasPending { get; set; } = true;
            public virtual bool HasTimeout { get; set; } = true;

            /// <summary>
            /// 일반 인증 (ID, Password)
            /// </summary>
            /// <param name="authInfos">Util.MakeBasicAuthInfo(id, password)로 생성</param>
            public void SetBasicAuthentication(params (string, string)[] authInfos) => SetAuthentication(eAuthorizationType.BASIC, authInfos);

            /// <summary>
            /// 토큰 기반 인증 (Bearer 인증)
            /// </summary>
            /// <param name="authInfos">Util.MakeTokenAuthInfo(token)로 생성</param>
            public void SetTokenAuthentication(params (string, string)[] authInfos) => SetAuthentication(eAuthorizationType.AUTH_TOKEN, authInfos);
        }
#endregion // Wrapper - Auth

#region Wrapper - Debug
        public class ClientDebug
        {
            /// <summary>
            /// 동시 연결중인 요청 수 확인 (Debug)
            /// </summary>
            public int ConcurrentRequest => _concurrentRequest;

            /// <summary>
            /// 요청 패킷에 대한 Uri 로그 표시
            /// (일반 요청은 RequestType 포함)
            /// </summary>
            public bool ShowRequestLog => true;

            /// <summary>
            /// 응답 패킷에 대한 로그 표시
            /// </summary>
            public bool ShowResponseLog => true;
        }
#endregion // Wrapper - Debug

#region Wrapper - Constant
        public class ClientConstant
        {
            public string ContentJson => "application/json";
            public string ContentMultipartForm => "multipart/form-data";
            public string ContentOctetStream => "application/octet-stream";
        }
#endregion // Wrapper - Constant
#endregion // Wrapper

#region HTTP Request
#region Non-Callback
        // Basic Request
        private static async UniTask<ResponseBase<string>> SendRequestStringAsync(RequestDataSimple request)
        {
            var response = await SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return new ResponseBase<string>(null, response);

            var result = await response.Content.ReadAsStringAsync();
            DebugResponseLog(request.Url, result);
            return new ResponseBase<string>(result, response);
        }
        private static async UniTask<ResponseStream> SendRequestStreamAsync(RequestDataSimple request)
        {
            var response = await SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return new ResponseStream(response, null);

            var stream = await response.Content.ReadAsStreamAsync();
            return new ResponseStream(response, stream);
        }
        private static async UniTask<ResponseBase<T>> SendRequestObjectAsync<T>(RequestDataSimple request)
        {
            var response = await SendAsync(request);
            if (response is not {IsSuccessStatusCode: true}) return new ResponseBase<T>(response);

            try
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<T>(responseJson);
                DebugResponseLog(request.Url, responseJson);
                return new ResponseBase<T>(result, response);
            }
            catch (Exception e)
            {
                C2VDebug.LogWarningCategory(LogHttpHelper, e);
                return new ResponseBase<T>(response);
            }
        }
        private static async UniTask<HttpResponseMessage> SendAsync(RequestDataSimple request)
        {
            var response = await SendRequestAsync(request);
            response = await ValidateResponseMessageAsync(response, request.Cts);
            return response;
        }

        // Message
        private static async UniTask<ResponseBase<string>> SendRequestStringAsync(RequestDataMessage request)
        {
            var response = await SendAsync(request);
            if (response == null)
                return new ResponseBase<string>(null, response);

            var value = await response.Content.ReadAsStringAsync();
            DebugResponseLog(request.Message.RequestUri?.ToString(), value);
            return new ResponseBase<string>(value, response);
        }

        private static async UniTask<ResponseStream> SendRequestStreamAsync(RequestDataMessage request)
        {
            var response = await SendAsync(request);
            if (response == null)
                return new ResponseStream(response, null);

            var stream = await response.Content.ReadAsStreamAsync();
            return new ResponseStream(response, stream);
        }

        private static async UniTask<ResponseBase<T>> SendRequestObjectAsync<T>(RequestDataMessage request)
        {
            var response = await SendAsync(request);
            if (response == null) return new ResponseBase<T>(response);

            try
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                DebugResponseLog(request.Message.RequestUri?.ToString(), responseJson);
                var obj = JsonConvert.DeserializeObject<T>(responseJson);
                return new ResponseBase<T>(obj, response);
            }
            catch (Exception e)
            {
                C2VDebug.LogWarningCategory(LogHttpHelper, e);
                return null;
            }
        }

        private static async UniTask<HttpResponseMessage> SendAsync(RequestDataMessage request)
        {
            var response = await SendRequestAsync(request);
            response = await ValidateResponseMessageAsync(response, request.Cts);
            return response;
        }

        private static async UniTask<HttpResponseMessage> ValidateResponseMessageAsync(HttpResponseMessage response, CancellationTokenSource cts = null)
        {
            if (response is {IsSuccessStatusCode: false})
            {
                if ((int) response.StatusCode == ErrorAuthenticationTimeout)
                    response = await OnTokenExpiredAsync(response);
                else
                    response = await ValidateResponseErrorMessageAsync(response);
            }

            return response;

            async UniTask<HttpResponseMessage> ValidateResponseErrorMessageAsync(HttpResponseMessage responseMessage)
            {
                var errorJson = await responseMessage?.Content?.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(errorJson))
                    return responseMessage;

                try
                {
                    var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorJson);
                    switch (errorResponse?.Code)
                    {
                        case ErrorAccessTokenExpired:
                        {
                            responseMessage = await OnTokenExpiredAsync(responseMessage);
                        }
                            break;
                        case ErrorRefreshTokenExpired:
                        {
                            _onRefreshTokenExpired?.Invoke();
                        }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    C2VDebug.LogWarningCategory(LogHttpHelper, $"유효하지 않음 응답 ({responseMessage.StatusCode})\n{errorJson}\n{e}");
                }

                return responseMessage;
            }

            async UniTask<HttpResponseMessage> OnTokenExpiredAsync(HttpResponseMessage responseMessage)
            {
                if (_onAccessTokenExpired != null)
                {
                    _accessTokenRequestCount++;
                    if (_accessTokenRequestCount >= AccessTokenRequestMax)
                    {
                        _onAccessTokenRetryExceed?.Invoke();
                        return responseMessage;
                    }

                    var success = await _onAccessTokenExpired.Invoke();
                    if (success)
                    {
                        switch (LastRequest.LastRequestType)
                        {
                            case LastRequest.eLastRequestType.SIMPLE_REQUEST:
                            {
                                var request = new RequestDataSimple(LastRequest.RequestType, LastRequest.Url)
                                {
                                    Content    = LastRequest.Content,
                                    Cts        = LastRequest.Cts,
                                    HasPending = LastRequest.HasPending,
                                    HasTimeout = LastRequest.HasTimeout
                                };
                                responseMessage = await SendAsync(request);
                            }
                                break;
                            case LastRequest.eLastRequestType.MESSAGE_REQUEST:
                            {
                                var request = new RequestDataMessage(LastRequest.Message)
                                {
                                    Option     = LastRequest.Option,
                                    Cts        = LastRequest.Cts,
                                    HasPending = LastRequest.HasPending,
                                    HasTimeout = LastRequest.HasTimeout
                                };
                                responseMessage = await SendAsync(request);
                            }
                                break;
                            default:
                                C2VDebug.LogErrorCategory(LogHttpHelper, "토큰 만료 - 재요청할 Web API가 없습니다.");
                                break;
                        }
                        _accessTokenRequestCount = 0;
                    }
                    else
                    {
                        C2VDebug.LogWarningCategory(LogHttpHelper, "엑세스 토큰 갱신 실패.");
                    }
                }

                return responseMessage;
            }
        }
        private static void FillHttpResponse<T>(T result, HttpResponseMessage response)
        {
            var responseBase = result as ResponseBase<T>;
            if (responseBase != null)
            {
                responseBase.Response = response;
                responseBase.StatusCode = response.StatusCode;
            }
        }
#endregion // Non-Callback

#region With-Callback
        private static async UniTask<HttpResponseMessage> SendRequestAsync(RequestDataSimple request) => await SendRequestAsync(request, false);
        private static async UniTask<HttpResponseMessage> SendRequestAsync(RequestDataSimple request, bool ignoreConnectionLimit)
        {
            if (ignoreConnectionLimit)
                return await SendRequestInternalAsync(request);

            return await EnqueueAndWaitRequestAsync(async () => await SendRequestInternalAsync(request), request.Cts);

            async UniTask<HttpResponseMessage> SendRequestInternalAsync(RequestDataSimple request)
            {
                DebugRequestLog(request);

                var requestType = request.RequestType;
                var url = request.Url;
                var content = request.Content;
                var cts = request.Cts;

                if (request.CanResend)
                    LastRequest.Set(requestType, url, content, request.HasPending, request.HasTimeout);
                
                if (request.HasTimeout)
                    cts ??= new CancellationTokenSource();

                Increment();
                SetMaxConcurrency(url, MaxConcurrency);
                
                var key = PushPacketIntoTimer(request, cts);
                HttpResponseMessage response = null;
                try
                {
                    switch (requestType)
                    {
                        case eRequestType.GET:
                        {
                            if (cts == null)
                                response = await GetClient().GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                            else
                                response = await GetClient().GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cts.Token);
                        }
                            break;
                        case eRequestType.PUT:
                        {
                            if (cts == null)
                                response = await GetClient().PutAsync(url, new StringContent(content));
                            else
                                response = await GetClient().PutAsync(url, new StringContent(content), cts.Token);
                        }
                            break;
                        case eRequestType.POST:
                        {
                            if (cts == null)
                                response = await GetClient().PostAsync(url, new StringContent(content));
                            else
                                response = await GetClient().PostAsync(url, new StringContent(content), cts.Token);
                        }
                            break;
                        case eRequestType.DELETE:
                        {
                            if (cts == null)
                                response = await GetClient().DeleteAsync(url);
                            else
                                response = await GetClient().DeleteAsync(url, cts.Token);
                        }
                            break;
                        default:
                            C2VDebug.LogWarningCategory(LogHttpHelper, $"지원되지 않는 요청 타입 {requestType}");
                            break;
                    }
                }
                catch (TaskCanceledException e)
                {
                    C2VDebug.LogCategory(LogHttpHelper, $"요청이 취소되었습니다.\n{url}\n{e}");
                    response = new() {StatusCode = HttpStatusCode.RequestTimeout};
                }
                catch (HttpRequestException e)
                {
                    var innerException = e.InnerException;
                    switch (innerException)
                    {
                        case UnauthorizedAccessException innerEx:
                            response = new() {StatusCode = HttpStatusCode.Unauthorized};
                            PrintErrorWithException(innerEx);
                            break;
                        case TimeoutException innerEx:
                            response = new() {StatusCode = HttpStatusCode.RequestTimeout};
                            PrintErrorWithException(innerEx);
                            break;
                        default:
                            PrintErrorWithException(e);
                            break;
                    }
                    C2VDebug.LogErrorCategory(LogHttpHelper, $"연결할 수 없습니다 ({url})\n{e}");
                }
                finally
                {
                    if (!string.IsNullOrEmpty(key))
                        PopPacketFromTimer(key);
                    Decrement();
                }
                return response;

                void PrintErrorWithException(Exception e)
                {
                    C2VDebug.LogErrorCategory(LogHttpHelper, $"연결할 수 없습니다 ({url})\n{e}");
                }
            }
        }

        private static async UniTask<HttpResponseMessage> SendRequestAsync(RequestDataMessage request) => await SendRequestAsync(request, false);
        private static async UniTask<HttpResponseMessage> SendRequestAsync(RequestDataMessage request, bool ignoreConnectionLimit)
        {
            if (ignoreConnectionLimit)
                return await SendRequestInternalAsync(request);

            return await EnqueueAndWaitRequestAsync(async () => await SendRequestInternalAsync(request), request.Cts);

            async UniTask<HttpResponseMessage> SendRequestInternalAsync(RequestDataMessage request)
            {
                DebugRequestLog(request);

                var message = request.Message;
                var option = request.Option;
                var cts = request.Cts;

                if (request.CanResend)
                    LastRequest.Set(message, option, request.HasPending, request.HasTimeout);

                if (request.HasTimeout)
                    cts ??= new CancellationTokenSource();

                Increment();
                SetMaxConcurrency(message.RequestUri, MaxConcurrency);
                var key = PushPacketIntoTimer(request, cts);
                HttpResponseMessage response = null;
                try
                {
                    if (cts == null)
                        response = await GetClient().SendAsync(message, option);
                    else
                        response = await GetClient().SendAsync(message, option, cts.Token);
                    
                    return response;
                }
                catch (TaskCanceledException e)
                {
                    C2VDebug.LogCategory(LogHttpHelper, $"요청이 취소되었습니다.\n{message.RequestUri}\n{e}");
                    response = new()
                    {
                        StatusCode = HttpStatusCode.RequestTimeout,
                    };
                }
                catch (HttpRequestException e)
                {
                    var innerException = e.InnerException;
                    switch (innerException)
                    {
                        case UnauthorizedAccessException innerEx:
                            response = new() {StatusCode = HttpStatusCode.Unauthorized};
                            PrintErrorWithException(innerEx);
                            break;
                        case TimeoutException innerEx:
                            response = new() {StatusCode = HttpStatusCode.RequestTimeout};
                            PrintErrorWithException(innerEx);
                            break;
                        default:
                            PrintErrorWithException(e);
                            break;
                    }
                }
                finally
                {
                    if (!string.IsNullOrEmpty(key))
                        PopPacketFromTimer(key);
                    Decrement();
                }

                return response;

                void PrintErrorWithException(Exception e)
                {
                    C2VDebug.LogErrorCategory(LogHttpHelper, $"연결할 수 없습니다 ({message.RequestUri.OriginalString})\n{e}");
                }
            }
        }
#endregion // With-Callback
#endregion // HTTP Request

#region Settings
        private static void SetBaseAddress(string url)
        {
            try
            {
                var client = GetClient();
                var uri = new Uri(url);
                client.BaseAddress = new Uri(uri.Host);
            }
            catch (Exception e)
            {
                C2VDebug.LogErrorCategory(LogHttpHelper, $"BaseAddress 지정 실패\n{e}");
            }
        }
#endregion // Settings

#region Auth
        private static void SetAuthentication(eAuthorizationType authType, params (string, string)[] authInfos)
        {
            switch (authType)
            {
                case eAuthorizationType.BASIC:
                case eAuthorizationType.AUTH_TOKEN:
                    GetAuthorizedClient(authType, authInfos);
                    break;
                default:
                    C2VDebug.LogErrorCategory(LogHttpHelper, "유효하지 않은 인증 타입");
                    break;
            }
        }

        private static HttpClient GetAuthorizedClient(eAuthorizationType authType, params (string, string)[] authInfos)
        {
            var client = GetClient();
            switch (authType)
            {
                case eAuthorizationType.BASIC:
                    client = SetBasicAuth(client, authInfos);
                    break;
                case eAuthorizationType.AUTH_TOKEN:
                    client = SetTokenAuth(client, authInfos);
                    break;
                default:
                    break;
            }

            return client;
        }
        private static HttpClient SetBasicAuth(HttpClient client, (string, string)[] authInfos, string authFormat = DefaultBasicAuthFormat)
        {
            if (authInfos is not {Length: 2})
            {
                C2VDebug.LogErrorCategory(LogHttpHelper, "기본 인증 실패. 잘못된 인증 정보");
                return null;
            }

            string id = null, password = null;
            foreach (var (key, value) in authInfos)
            {
                switch (key)
                {
                    case Util.KeyID:
                        id = value;
                        break;
                    case Util.KeyPassword:
                        password = value;
                        break;
                }
            }

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(password))
            {
                C2VDebug.LogErrorCategory(LogHttpHelper, "기본 인증 실패. ID 또는 패스워드가 없습니다.");
                return null;
            }

            var token = GenerateBase64(id, password, authFormat);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_authTypeKey[eAuthorizationType.BASIC], token);

            return client;

            string GenerateBase64(string id, string password, string authFormat)
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(authFormat)) return string.Empty;

                var bytes = Encoding.ASCII.GetBytes(string.Format(authFormat, id, password));
                var base64String = Convert.ToBase64String(bytes);
                return base64String;
            }
        }

        private static HttpClient SetTokenAuth(HttpClient client, (string, string)[] authInfos)
        {
            if (authInfos is not {Length: 1})
            {
                C2VDebug.LogErrorCategory(LogHttpHelper, "토큰 인증 실패. 잘못 된 인증 정보");
                return null;
            }

            var (key, token) = authInfos[0];
            if (key != Util.KeyAuthtoken)
            {
                C2VDebug.LogErrorCategory(LogHttpHelper, "토큰 인증 실패. Auth 키가 올바르지 않습니다.");
                return null;
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Util.KeyAuthtoken, token);
            return client;
        }
#endregion // Auth

#region Client
        private static HttpClient GetClient() => UnityHttpClient.Get();

        // https://makolyte.com/csharp-how-to-make-concurrent-requests-with-httpclient/
        internal static void SetMaxConcurrency(string url, int maxConcurrentRequests) => SetMaxConcurrency(new Uri(url), maxConcurrentRequests);
        private static void SetMaxConcurrency(Uri uri, int maxConcurrentRequests) => ServicePointManager.FindServicePoint(uri).ConnectionLimit = maxConcurrentRequests * 2; // 최대 연결만큼 요청이 쌓여있을 때 네트워크 요청이 처리되지 않는 현상이 있어 여유분 추가
#endregion // Client

#region Concurrency
        private static async UniTask<HttpResponseMessage> EnqueueAndWaitRequestAsync(Func<Task<HttpResponseMessage>> request, CancellationTokenSource cts = null)
        {
            try
            {
                var id = EnqueueRequest(request);
                RequestInfo requestInfo = await CheckRequestInQueueAsync(id);

                if (requestInfo.IsEmpty() || requestInfo.RequestFunc == null || cts is {IsCancellationRequested: true})
                    return null;

                return await requestInfo.RequestFunc?.Invoke();
            }
            catch (TaskCanceledException e)
            {
                C2VDebug.LogCategory(LogHttpHelper, $"요청이 취소되었습니다.\n{e}");
                return null;
            }
            catch (Exception e)
            {
                C2VDebug.LogErrorCategory(LogHttpHelper, $"Http 요청 실패...\n{e}");
                return null;
            }
        }

        private static async UniTask<RequestInfo> CheckRequestInQueueAsync(int id)
        {
            RequestInfo requestInfo = RequestInfo.Empty;
            await UniTask.WaitUntil(() =>
            {
                if (_concurrentRequest >= MaxConcurrency)
                    return false;

                if (RequestQueue.TryPeek(out requestInfo))
                {
                    if (requestInfo.Id == id)
                    {
                        RequestQueue.TryDequeue(out requestInfo);
                        return true;
                    }
                }
                return false;
            });
            return requestInfo;
        }
        private static int EnqueueRequest(Func<Task<HttpResponseMessage>> request)
        {
            var requestInfo = new RequestInfo(request);
            RequestQueue.Enqueue(requestInfo);
            return requestInfo.Id;
        }

        internal static void Increment() => Interlocked.Increment(ref _concurrentRequest);
        internal static void Decrement() => Interlocked.Decrement(ref _concurrentRequest);
#endregion // Concurrency

#region Dispose
        private static void CleanUp()
        {
            _concurrentRequest = 0;
            UnityHttpClient.ResetAll();

            RequestQueue.Clear();
            RequestInfo.Reset();

            LastRequest.Dispose();
        }
#endregion // Dispose

#region Data
        private struct RequestInfo
        {
            public int Id;
            public Func<Task<HttpResponseMessage>> RequestFunc;

            private const int MaxRequestId = int.MaxValue;
            private const int EmptyRequestId = -1;
            private static int _requestId = 0;

            internal RequestInfo(Func<Task<HttpResponseMessage>> requestFunc)
            {
                var prev = Interlocked.CompareExchange(ref _requestId, 0, MaxRequestId);
                if (prev != _requestId)
                {
                    Id = _requestId;
                }
                else
                {
                    Interlocked.Increment(ref _requestId);
                    Id = _requestId;
                }
                RequestFunc = requestFunc;
            }
            internal bool IsEmpty() => Id == EmptyRequestId;

            internal static void Reset()
            {
                _requestId = 0;
            }

            internal static RequestInfo Empty = new RequestInfo
            {
                Id = EmptyRequestId,
            };
        }

        private static class LastRequest
        {
            internal enum eLastRequestType
            {
                NOT_SET,
                SIMPLE_REQUEST,
                MESSAGE_REQUEST,
            }
#region Variables
            public static bool                    HasPending      { get; private set; }
            public static bool                    HasTimeout      { get; private set; }
            public static eLastRequestType        LastRequestType { get; private set; } = eLastRequestType.NOT_SET;
            public static HttpRequestMessage      Message         { get; private set; }
            public static HttpCompletionOption    Option          { get; private set; } = HttpCompletionOption.ResponseHeadersRead;
            public static eRequestType            RequestType     { get; private set; } = eRequestType.GET;
            public static string                  Url             { get; private set; }
            public static string                  Content         { get; private set; }
            public static CancellationTokenSource Cts             { get; private set; }
#endregion // Variables

            public static void Set(eRequestType requestType, string url, string content, bool pending = true, bool timeout = true, CancellationTokenSource cts = null)
            {
                Clear();
                LastRequestType = eLastRequestType.SIMPLE_REQUEST;
                RequestType     = requestType;
                Url             = url;
                Content         = content;
                Cts             = cts;
                HasPending      = pending;
                HasTimeout      = timeout;
            }

            public static void Set(HttpRequestMessage message, HttpCompletionOption option = HttpCompletionOption.ResponseHeadersRead, bool pending = true, bool timeout = true, CancellationTokenSource cts = null)
            {
                Clear();

                if (message == null)
                    return;

                LastRequestType = eLastRequestType.MESSAGE_REQUEST;
                Message = new HttpRequestMessage
                {
                    Method = message.Method,
                    RequestUri = message.RequestUri,
                    Content = message.Content,
                    Version = message.Version,
                };
                if (message.Headers != null)
                {
                    foreach (var (key, value) in message.Headers)
                        Message.Headers.Add(key, value);
                }

                if (message.Properties != null)
                {
                    foreach (var (key, value) in message.Properties)
                        Message.Properties.Add(key, value);
                }

                Option     = option;
                Cts        = cts;
                HasPending = pending;
                HasTimeout = timeout;
            }

            private static void Clear()
            {
                LastRequestType = eLastRequestType.NOT_SET;
                Message = null;
                Option = HttpCompletionOption.ResponseHeadersRead;
                RequestType = eRequestType.GET;
                Url = string.Empty;
                Content = string.Empty;
                Cts = null;
            }
            public static void Dispose()
            {
                Clear();
            }
        }

        public struct RequestDataSimple
        {
            public eRequestType RequestType;
            public string Url;
            public string Content;
            public CancellationTokenSource Cts;
            
            public bool CanResend;
            public bool HasPending;
            public bool HasTimeout;

            public RequestDataSimple(eRequestType requestType, string url)
            {
                RequestType = requestType;
                Url         = url;
                Content     = string.Empty;
                Cts         = null;
                CanResend   = true;
                HasPending  = true;
                HasTimeout  = true;
            }
        }
        public struct RequestDataMessage
        {
            public HttpRequestMessage      Message;
            public HttpCompletionOption    Option;
            public CancellationTokenSource Cts;
            
            public bool CanResend;
            public bool HasPending;
            public bool HasTimeout;

            public RequestDataMessage(HttpRequestMessage message)
            {
                Message    = message;
                Option     = HttpCompletionOption.ResponseHeadersRead;
                Cts        = null;
                CanResend  = true;
                HasPending = true;
                HasTimeout = true;
            }
        }
#endregion // Data

#region Com2Verse Web API
        private const int ErrorAuthenticationTimeout = 419;
        private const int ErrorAccessTokenExpired = -200;
        private const int ErrorRefreshTokenExpired = -204;
        [Serializable]
        private class ErrorResponse
        {
            [JsonProperty("code")]
            public int Code;

            [JsonProperty("msg")]
            public string Message;
        }

        /* 아래 콜백은 각각 1번만 등록 */
        public static void SetOnAccessTokenExpired(Func<UniTask<bool>> onTokenExpired) => _onAccessTokenExpired = onTokenExpired;
        public static void SetOnRefreshTokenExpired(Action onRefreshTokenExpired) => _onRefreshTokenExpired = onRefreshTokenExpired;
        public static void SetOnAccessTokenRetryExceed(Action onAccessTokenRetryExceed) => _onAccessTokenRetryExceed = onAccessTokenRetryExceed;
#endregion // Com2Verse Web API

#region Util
        public static eRequestType GetRequestType(string requestTypeStr) => requestTypeStr.ToLower() switch
        {
            "get"    => eRequestType.GET,
            "put"    => eRequestType.PUT,
            "post"   => eRequestType.POST,
            "delete" => eRequestType.DELETE,
            _        => eRequestType.GET,
        };
#endregion // Util

#region Debug
        private static void DebugRequestLog(RequestDataSimple request)
        {
            if (Debug.ShowRequestLog)
            {
                var requestColor = RequestTypeColorMap[request.RequestType];
                C2VDebug.LogCategory(LogHttpHelper, $"<b>[<color={requestColor}>{request.RequestType}</color>]</b> {request.Url}");
            }
        }

        private static void DebugRequestLog(RequestDataMessage request)
        {
            if (Debug.ShowRequestLog)
                C2VDebug.LogCategory(LogHttpHelper, $"{request.Message.RequestUri}\n{request.Message.Content}");
        }

        private static void DebugResponseLog(string requestUrl, string jsonResponse)
        {
            if (Debug.ShowResponseLog)
                C2VDebug.LogCategory(LogHttpHelper, $"{requestUrl}\n{jsonResponse}");
        }

        private static readonly Dictionary<eRequestType, string> RequestTypeColorMap = new Dictionary<eRequestType, string>
        {
            {eRequestType.GET, "#5FBF86"},
            {eRequestType.POST, "#F2D978"},
            {eRequestType.PUT, "#6FA6E9"},
            {eRequestType.DELETE, "#F0958A"},
        };
#endregion // Debug

#region Editor
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            CleanUp();
        }
#endif     // UNITY_EDITOR
#endregion // Editor
    }
}
