/*===============================================================
* Product:		Com2Verse
* File Name:	MiceWebClient_Dispatch.cs
* Developer:	sprite
* Date:			2023-04-28 14:52
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Logger;
using System.Collections.Generic;
using System.Reflection;

namespace Com2Verse.Mice
{
    /// <summary>
    /// 호출자 정보
    /// </summary>
    public readonly struct CallerInfo
    {
        /// <summary>
        /// API 그룹 이름
        /// </summary>
        public string GroupName { get; init; }
        /// <summary>
        /// API 이름
        /// </summary>
        public string CallerName { get; init; }
        /// <summary>
        /// API 호출 인자 정보(TAG)
        /// </summary>
        public Type ParamType { get; init; }
        /// <summary>
        /// API 고유 이름
        /// </summary>
        public string UniqueName => $"{this.GroupName}.{this.CallerName}";
        public bool IsValid => !string.IsNullOrEmpty(this.GroupName) && !string.IsNullOrEmpty(this.CallerName);

        public CallerInfo(string groupName, string callerName, Type paramType = null)
        {
            this.GroupName = groupName;
            this.CallerName = callerName;
            this.ParamType = paramType;
        }
    }

    public static partial class MiceWebClient   // Dispatch
    {
        internal class HandlerInfo
        {
            public SubscribeAttribute Attr { get; private set; }
            public Type HandlerType { get; private set; }
            public string MethodName { get; private set; }
            public Action<Response> OnResponse { get; private set; }

            private ResponseType _responseMapper;

            public HandlerInfo(SubscribeAttribute attr, MethodInfo method, Type type, object firstArg)
            {
                UnityEngine.Assertions.Assert.IsNotNull(attr);
                UnityEngine.Assertions.Assert.IsNotNull(method);
                UnityEngine.Assertions.Assert.IsNotNull(type);

                this.Attr = attr;
                this.HandlerType = type;
                this.MethodName = method.Name;

                if (!attr.HasResponseType)
                {
                    _responseMapper = null;

                    var parameters = method.GetParameters();
                    if (parameters == null || parameters.Length == 0)
                    {
                        var dele = Delegate.CreateDelegate(typeof(Action), firstArg, method) as Action;
                        this.OnResponse = _ => dele();
                    }
                    else
                    {
                        if (parameters.Length > 1)
                        {
                            throw new ArgumentException($"Too many parameters!");
                        }

                        var paramType = parameters[0].ParameterType;
                        var responseType = typeof(Response);

                        if (paramType != responseType)
                        {
                            if
                            (
                                !paramType.IsSubclassOf(responseType) &&
                                !paramType.IsAssignableFrom(responseType)
                            )
                            {
                                throw new ArgumentException($"Type mismatch! [{paramType.Name}]");
                            }

                            this.OnResponse = response => method.Invoke(firstArg, new[] { response });
                        }
                        else
                        {
                            this.OnResponse = Delegate.CreateDelegate(typeof(Action<Response>), firstArg, method) as Action<Response>;
                        }
                    }
                }
                else
                {
                    _responseMapper = attr.CreateMapper(method, firstArg);

                    this.OnResponse = _responseMapper.Invoke;
                }
            }
        }

        private static Dictionary<string, List<HandlerInfo>> _dispatchListMap = new Dictionary<string, List<HandlerInfo>>();
        private delegate void HookDelegate(Response response);
        private static HookDelegate _onHook;

        private static bool IsExistsHook(MethodInfo method)
        {
            if (_onHook == null) return false;

            var invocationList = _onHook.GetInvocationList();

            for (int i = 0, cnt = invocationList?.Length ?? 0; i < cnt; i++)
            {
                if (invocationList[i].Method == method)
                {
                    return true;
                }
            }

            return false;
        }

        private static void RemoveHook(Type type)
        {
            if (_onHook == null) return;

            var invocationList = _onHook.GetInvocationList();
            for (int i = invocationList?.Length ?? -1; i >= 0; --i)
            {
                var item = invocationList[i];

                if (item.Method.DeclaringType == type)
                {
                    _onHook -= item as HookDelegate;

                    C2VDebug.Log($"[MiceWebClient]<Dispatch> Remove hook handler: {type.Name}.{item.Method.Name}");
                }
            }
        }

        /// <summary>
        /// 응답 정보를 핸들링 할 메소드들을 찾아 등록한다.
        /// <para>(<see cref="SubscribeAttribute"/>가 설정된 메소드를 찾아 등록한다)</para>
        /// </summary>
        /// <param name="obj"></param>
        public static void BindHandlers(object obj)
        {
            var type = obj.GetType();
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            for (int i = 0, cnt = methods?.Length ?? 0; i < cnt; i++)
            {
                var method = methods[i];
                var attr = method.GetCustomAttribute<SubscribeAttribute>();
                if (attr == null) continue;

                var firstArg = method.IsStatic ? null : obj;

                if (attr is HookAttribute)
                {
                    if (!MiceWebClient.IsExistsHook(method))
                    {
                        _onHook += Delegate.CreateDelegate(typeof(HookDelegate), firstArg, method) as HookDelegate;

                        C2VDebug.Log($"[MiceWebClient]<Dispatch> Add hook handler: {type.Name}.{method.Name}");
                    }
                    else
                    {
                        C2VDebug.Log($"[MiceWebClient]<Dispatch> Already exists: {type.Name}.{method.Name}");
                    }
                    
                    continue;
                }

                if (!_dispatchListMap.TryGetValue(attr.Request, out var handlerList))
                {
                    handlerList = new List<HandlerInfo>(1);
                    _dispatchListMap.Add(attr.Request, handlerList);
                }

                bool exists = false;

                for (int j = 0, cntj = handlerList.Count; j < cntj; j++)
                {
                    var info = handlerList[j];
                    if 
                    (
                        info.HandlerType == type && 
                        string.Equals(info.Attr.Request, attr.Request) && 
                        info.Attr.ParamType == attr.ParamType &&
                        string.Equals(info.MethodName, method.Name)
                    )
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    handlerList.Add(new HandlerInfo(attr, method, type, firstArg));

                    C2VDebug.Log($"[MiceWebClient]<Dispatch> ({attr.Request}) Add dispatch handler: {type.Name}.{method.Name}");
                }
                else
                {
                    C2VDebug.Log($"[MiceWebClient]<Dispatch> ({attr.Request}) Already exists: {type.Name}.{method.Name}");
                }
            }
        }

        /// <summary>
        /// 등록된 응답 정보 핸들러들을 정리한다.
        /// </summary>
        /// <param name="obj"></param>
        public static void ClearHandlers(object obj)
        {
            var type = obj.GetType();

            MiceWebClient.RemoveHook(type);

            foreach (var key in _dispatchListMap.Keys)
            {
                var handlerList = _dispatchListMap[key];
                for (int i = handlerList.Count - 1; i >= 0; --i)
                {
                    var item = handlerList[i];

                    if (item.HandlerType == type)
                    {
                        handlerList.Remove(item);

                        C2VDebug.Log($"[MiceWebClient]<Dispatch> ({item.Attr.Request}) Remove dispatch handler: {type.Name}.{item.MethodName}");
                    }
                }
            }
        }

        public static void Dispatch(string request, Response response, Type paramType = null)
        {
            if (!_dispatchListMap.TryGetValue(request, out var handlerList)) return;

            if (_onHook != null)
            {
                _onHook(response);
            }

            HandlerInfo info;
            for (int i = 0, cnt = handlerList.Count; i < cnt; i++)
            {
                info = handlerList[i];

                if (paramType != null && info.Attr.ParamType != null && info.Attr.ParamType != paramType) continue;

                info.OnResponse(response);
            }
        }

#region API 호출 인자 식별용
        public class ParamType { }
        public class ParamType<TArg0> { }
        public class ParamType<TArg0, TArg1> { }
        public class ParamType<TArg0, TArg1, TArg2> { }
        public class ParamType<TArg0, TArg1, TArg2, TArg3> { }
        public class ParamType<TArg0, TArg1, TArg2, TArg3, TArg4> { }
        public class ParamType<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5> { }
        public class ParamType<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> { }
        public class ParamType<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> { }
        public class ParamType<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> { }
        public class ParamType<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9> { }
#endregion // API 호출  API 호출 인자 식별용

        /// <summary>
        /// 응답이 없는 경우 사용 가능.
        /// </summary>
        public class ResponseType
        {
            internal delegate void HandlerDelegate();

            protected Delegate _dele;

            protected ResponseType(Delegate dele)
            {
                _dele = dele;
            }

            public ResponseType(MethodInfo method, object firstArg)
                : this(Delegate.CreateDelegate(typeof(HandlerDelegate), firstArg, method))
            {
            }

            public virtual void Invoke(Response response) => (_dele as HandlerDelegate)();
        }

        /// <summary>
        /// 응답 타입 매퍼
        /// <para>응답 타입이 <see cref="Response"/> 가 아니거나 <see cref="Response"/>를 상속받은 타입이 아닌경우, 반드시 이 클래스의 타입을 <see cref="SubscribeAttribute"/>.ResponseType 으로 전달해야 한다.</para>
        /// </summary>
        /// <typeparam name="TParam"></typeparam>
        public class ResponseType<TParam> : ResponseType
        {
            internal new delegate void HandlerDelegate(TParam param);

            public ResponseType(MethodInfo method, object firstArg)
                : base(Delegate.CreateDelegate(typeof(HandlerDelegate), firstArg, method))
            {

            }

            public override void Invoke(Response response)
            {
                var paramType = typeof(TParam);
                var responseType = typeof(Response);
                if (paramType.IsSubclassOf(responseType) || paramType.IsAssignableFrom(responseType))
                {
                    (_dele as HandlerDelegate)((TParam)(object)response);
                    return;
                }

                (_dele as HandlerDelegate)(response.GetData<TParam>());
            }
        }

        /// <summary>
        /// 응답 구독용 속성 (Method Only)
        /// <para>ResponseType: <see cref="Response"/>, <see cref="Response&lt;T&gt;"/> 타입이 아닌 타입에 대한 정보 설정용</para>
        /// <para>ParamType: API 호출 메소드 이름이 같은 경우, 구분을 위한 메소드 인자 타입 정보 설정용</para>
        /// </summary>
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        public class SubscribeAttribute : Attribute
        {
            /// <summary>
            /// 요청 이름.
            /// </summary>
            public string Request;
            /// <summary>
            /// 요청 인자 타입 정보 타입.
            /// <para>(API 호출 메소드 이름이 같은 경우, 구분을 위한 메소드 인자 타입 정보 설정용)</para>
            /// </summary>
            public Type ParamType;
            /// <summary>
            /// 응답 타입
            /// <para>(<see cref="Response"/>, <see cref="Response&lt;T&gt;"/> 타입이 아닌 타입에 대한 정보 설정용)</para>
            /// </summary>
            public Type ResponseType;

            public bool HasResponseType => this.ResponseType != null;

            public ResponseType CreateMapper(MethodInfo method, object firstArg)
            {
                if (this.ResponseType == null) return null;

                return Activator.CreateInstance(this.ResponseType, method, firstArg) as ResponseType;
            }

            public SubscribeAttribute(string request)
            {
                this.Request = request;
            }
        }

        /// <summary>
        /// 모든 응답을 구독 하기 위한 속성 (Method Only)
        /// </summary>
        public sealed class HookAttribute : SubscribeAttribute
        {
            public HookAttribute()
                : base("")
            {
            }
        }
    }
}
