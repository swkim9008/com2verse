/*===============================================================
* Product:		Com2Verse
* File Name:	MiceNamedLogger.cs
* Developer:	sprite
* Date:			2023-07-11 17:10
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Runtime.CompilerServices;
using Com2Verse.Logger;
using System.Reflection;
using System.Diagnostics;
using System;

namespace Com2Verse.Mice.NamedLoggerTag
{
    public class LoggerTag<TTag>
        where TTag : LoggerTag<TTag>
    {
        private static FieldInfo _fieldInfo;

        public static bool GetIsEnable()
        {
            if (_fieldInfo == null)
            {
                _fieldInfo = typeof(TTag).GetField("IsEnable", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                if (_fieldInfo == null) return false;
            }

            return (bool)_fieldInfo.GetValue(null);
        }

        public enum eLogType
        {
            LOG,
            WARNING,
            ERROR
        }

        [Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
        public static void LogCategory(eLogType logType, string category, string message, [CallerMemberName] string callerMemberName = default)
        {
            if (!LoggerTag<TTag>.GetIsEnable()) return;

            category = !string.IsNullOrEmpty(category) ? $" <color=#4EC981FF>{category}</color>" : "";
            var logCat = $"<color=white><{typeof(TTag).Name}></color>{category}";
            var logMsg = $"<color=#3A87D6FF>({callerMemberName})</color> {message}";

            switch (logType)
            {
                default:
                case eLogType.LOG:      C2VDebug.LogCategory(logCat, logMsg);           break;
                case eLogType.WARNING:  C2VDebug.LogWarningCategory(logCat, logMsg);    break;
                case eLogType.ERROR:    C2VDebug.LogErrorCategory(logCat, logMsg);      break;
            }
        }

        [Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
        public static void LogCategory(string category, string message, [CallerMemberName] string callerMemberName = default)
            => LoggerTag<TTag>.LogCategory(eLogType.LOG, category, message, callerMemberName);

        [Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
        public static void Log(string message, [CallerMemberName] string callerMemberName = default)
            => LoggerTag<TTag>.LogCategory(eLogType.LOG, null, message, callerMemberName);

        [Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
        public static void LogWarningCategory(string category, string message, [CallerMemberName] string callerMemberName = default)
            => LoggerTag<TTag>.LogCategory(eLogType.WARNING, category, message, callerMemberName);

        [Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
        public static void LogWarning(string message, [CallerMemberName] string callerMemberName = default)
            => LoggerTag<TTag>.LogCategory(eLogType.WARNING, null, message, callerMemberName);

        [Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
        public static void LogErrorCategory(string category, string message, [CallerMemberName] string callerMemberName = default)
            => LoggerTag<TTag>.LogCategory(eLogType.ERROR, category, message, callerMemberName);

        [Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
        public static void LogError(string message, [CallerMemberName] string callerMemberName = default)
            => LoggerTag<TTag>.LogCategory(eLogType.LOG, null, message, callerMemberName);
    }
}

namespace Com2Verse.Mice
{
    /// <summary>
    /// Category에 사용자 정의 이름(<typeparamref name="TTag"/>)을 붙인 Logger
    /// </summary>
    /// <typeparam name="TTag"></typeparam>
    public interface INamedLogger<TTag> { }

    public static partial class INamedLoggerExtensions
    {
        private static string ObjectToString(object obj)
            => obj?.ToString() ?? "(null)";

        [Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
        public static void Log<TTag>(this INamedLogger<TTag> logger, string message = default, [CallerMemberName] string callerMemberName = default)
            where TTag : NamedLoggerTag.LoggerTag<TTag>
            => NamedLoggerTag.LoggerTag<TTag>.LogCategory(NamedLoggerTag.LoggerTag<TTag>.eLogType.LOG, logger.GetType().Name, message, callerMemberName);

        [Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
        public static void Log<TTag>(this INamedLogger<TTag> logger, object obj, [CallerMemberName] string callerMemberName = default)
             where TTag : NamedLoggerTag.LoggerTag<TTag>
            => NamedLoggerTag.LoggerTag<TTag>.LogCategory(NamedLoggerTag.LoggerTag<TTag>.eLogType.LOG, logger.GetType().Name, ObjectToString(obj), callerMemberName);

        [Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
        public static void LogWarning<TTag>(this INamedLogger<TTag> logger, string message = default, [CallerMemberName] string callerMemberName = default)
            where TTag : NamedLoggerTag.LoggerTag<TTag>
            => NamedLoggerTag.LoggerTag<TTag>.LogCategory(NamedLoggerTag.LoggerTag<TTag>.eLogType.WARNING, logger.GetType().Name, message, callerMemberName);

        [Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
        public static void LogWarning<TTag>(this INamedLogger<TTag> logger, object obj, [CallerMemberName] string callerMemberName = default)
             where TTag : NamedLoggerTag.LoggerTag<TTag>
            => NamedLoggerTag.LoggerTag<TTag>.LogCategory(NamedLoggerTag.LoggerTag<TTag>.eLogType.WARNING, logger.GetType().Name, ObjectToString(obj), callerMemberName);

        [Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
        public static void LogError<TTag>(this INamedLogger<TTag> logger, string message = default, [CallerMemberName] string callerMemberName = default)
            where TTag : NamedLoggerTag.LoggerTag<TTag>
            => NamedLoggerTag.LoggerTag<TTag>.LogCategory(NamedLoggerTag.LoggerTag<TTag>.eLogType.ERROR, logger.GetType().Name, message, callerMemberName);

        [Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
        public static void LogError<TTag>(this INamedLogger<TTag> logger, object obj, [CallerMemberName] string callerMemberName = default)
             where TTag : NamedLoggerTag.LoggerTag<TTag>
            => NamedLoggerTag.LoggerTag<TTag>.LogCategory(NamedLoggerTag.LoggerTag<TTag>.eLogType.ERROR, logger.GetType().Name, ObjectToString(obj), callerMemberName);
    }
}
