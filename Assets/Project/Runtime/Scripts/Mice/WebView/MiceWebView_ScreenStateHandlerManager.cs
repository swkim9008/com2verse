/*===============================================================
* Product:		Com2Verse
* File Name:	MiceWebView_ScreenStateHandlerManager.cs
* Developer:	sprite
* Date:			2023-07-28 11:00
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Com2Verse.Mice
{
    public partial class MiceWebView    // Screen State (eScreenState) Handler Manager
    {
        public struct ScreenStateParameters
        {
            public eScreenState State;
            public string Parameters;
            public object ExtraParameter;
            public CancellationToken CancellationToken;

            public void Deconstruct(out eScreenState state, out string parameters, out object extraParameter, out CancellationToken cancellationToken)
            {
                state = this.State;
                parameters = this.Parameters;
                extraParameter = this.ExtraParameter;
                cancellationToken = this.CancellationToken;
            }
        }

        public delegate UniTask ScreenStateHandlerDelegate();
        public delegate void ScreenStateHandlerVoidDelegate();

        private Dictionary<eScreenState, ScreenStateHandlerDelegate> _screenStateHandlerMap;
        private ScreenStateParameters _currentSSParams = new();

        partial void PartialInitScreenStateHandlers()
        {
            _screenStateHandlerMap = ScreenStateAttribute.Collect(this);
        }

        private UniTask InvokeScreenStateHandler(eScreenState state, string parameters, object extraParameter = null, CancellationToken cancellationToken = default)
        {
            _currentSSParams.State = state;
            _currentSSParams.Parameters = parameters;
            _currentSSParams.ExtraParameter = extraParameter;
            _currentSSParams.CancellationToken = cancellationToken;

#if UNITY_EDITOR
            if (_isSplitScreenController && _currentSSParams.ExtraParameter == null)
            {
                _currentSSParams.ExtraParameter = GetTestSplitScreenJson(_testIndex);
            }
#endif

            return _screenStateHandlerMap.TryGetValue(state, out var handler) ? handler() : UniTask.CompletedTask;
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
        public class ScreenStateAttribute : Attribute
        {
            public eScreenState ScreenState { get; private set; }

            public ScreenStateAttribute(eScreenState screenState)
            {
                this.ScreenState = screenState;
            }

            public static Dictionary<eScreenState, ScreenStateHandlerDelegate> Collect(Type type, object target = null)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.InvokeMethod);
                if (methods == null || methods.Length == 0) return null;

                return methods
                    .Where(e => e.IsDefined(typeof(ScreenStateAttribute)))
                    .SelectMany
                    (
                        e =>
                        {
                            var attrs = e.GetCustomAttributes<ScreenStateAttribute>();

                            IEnumerable<(eScreenState ScreenState, ScreenStateHandlerDelegate Callback)> EnumCallbacks()
                            {
                                ScreenStateHandlerDelegate handler = null;

                                try
                                {
                                    if (e.ReturnType == typeof(UniTask))
                                    {
                                        handler = Delegate.CreateDelegate(typeof(ScreenStateHandlerDelegate), target, e) as ScreenStateHandlerDelegate;
                                    }
                                    else
                                    {
                                        var realHandler = Delegate.CreateDelegate(typeof(ScreenStateHandlerVoidDelegate), target, e) as ScreenStateHandlerVoidDelegate;
                                        handler = () => { realHandler(); return UniTask.CompletedTask; };
                                    }
                                }
                                catch (Exception e)
                                {
                                    NamedLoggerTag.Sprite.Log($"<color=red><Exception></color> {e.Message}\r\n{e.StackTrace}");
                                    yield break;
                                }

                                foreach (var attr in attrs)
                                {
                                    yield return (attr.ScreenState, handler);
                                }
                            }

                            return EnumCallbacks();
                        }
                    )
                    .ToDictionary(e => e.ScreenState, e => e.Callback);
            }

            public static Dictionary<eScreenState, ScreenStateHandlerDelegate> Collect<T>(T target)
                => ScreenStateAttribute.Collect(typeof(T), target);
        }
    }
}
