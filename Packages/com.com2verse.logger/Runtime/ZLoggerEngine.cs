/*===============================================================
 * Product:		Com2Verse
 * File Name:	ZLoggerEngine.cs
 * Developer:	urun4m0r1
 * Date:		2023-01-13 12:51
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Text;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Com2Verse.Logger
{
	public static class LogManager
	{
#region IgnoredLoggers
		private static readonly List<string> IgnoredLoggers = new();

		public static void AddIgnoredCategory(string category)
		{
			if (IgnoredLoggers.Contains(category))
				return;

			IgnoredLoggers.Add(category);
		}

		public static void RemoveIgnoredCategory(string category)
		{
			if (!IgnoredLoggers.Contains(category))
				return;

			IgnoredLoggers.Remove(category);
		}

		[UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetLoggers()
		{
			IgnoredLoggers.Clear();
		}
#endregion IgnoredLoggers

		public static ILogger GlobalLogger { get; }

		public static ILogger<T> GetLogger<T>() where T : class => LoggerFactory.CreateLogger<T>();
		public static ILogger    GetLogger(string categoryName) => LoggerFactory.CreateLogger(categoryName);

		private static readonly ILoggerFactory  LoggerFactory;
		private static readonly LogManagerScope Scope;

		private sealed class LogManagerScope
		{
			~LogManagerScope()
			{
				LoggerFactory.Dispose();

				Log("LogManager disposed.");
			}
		}

		static LogManager()
		{
			// Standard LoggerFactory does not work on IL2CPP,
			// But you can use ZLogger's UnityLoggerFactory instead,
			// it works on IL2CPP, all platforms(includes mobile).
			LoggerFactory = UnityLoggerFactory.Create(static builder =>
			{
				builder.AddFilter(FilterCategory());
				builder.SetMinimumLevel(LogLevel.Trace);
				builder.AddZLoggerUnityDebug(ConfigureLog());
			})!;

			GlobalLogger = LoggerFactory.CreateLogger(string.Empty);

			Scope = new LogManagerScope();

			Log("LogManager initialized.");
		}

		private static Func<string, LogLevel, bool> FilterCategory() => static (category, level) =>
		{
			if (IgnoredLoggers.Contains(category))
				return false;

			return true;
		};

		private static Action<ZLoggerOptions> ConfigureLog() => static x =>
		{
			x.PrefixFormatter = static (writer, info) => FormatPrefix(info, writer);
			x.SuffixFormatter = static (writer, info) => FormatSuffix(info, writer);
		};

		private static void FormatPrefix(LogInfo info, IBufferWriter<byte> writer)
		{
			if (info.CategoryName.Length <= 0) return;

#if UNITY_EDITOR
			FormatLogInfo(info, writer, "<b>[{0}]</b> ");
#else
			FormatLogInfo(info, writer, "[{0}] ");
#endif // UNITY_EDITOR
		}

		private static void FormatSuffix(LogInfo info, IBufferWriter<byte> writer)
		{
			FormatLogInfo(info, writer, "\n----------\n[{1}] ({2}) {3}\n\n");
		}

		private static void FormatLogInfo(LogInfo info, IBufferWriter<byte> writer, string format)
		{
			var category = info.CategoryName;
			var level    = info.LogLevel.ToString();
			var eventId  = info.EventId.ToString();
			var dateTime = info.Timestamp.ToLocalTime().DateTime;
			ZString.Utf8Format(writer, format, category, level, eventId, dateTime);
		}

		[DebuggerStepThrough, DebuggerHidden, StackTraceIgnore]
		private static void Log(string message)
		{
#if UNITY_EDITOR
			UnityEngine.Debug.Log($"<color=cyan><b>[C2VDebug]</b></color> {message}");
#else
			UnityEngine.Debug.Log($"[C2VDebug] {message}");
#endif // UNITY_EDITOR
		}
	}
}
