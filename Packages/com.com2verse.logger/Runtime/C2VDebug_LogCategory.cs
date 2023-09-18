/*===============================================================
 * Product:		Com2Verse
 * File Name:	C2VDebug_LogCategory.cs
 * Developer:	urun4m0r1
 * Date:		2023-01-19 14:08
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using ZLogger;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Com2Verse.Logger
{
	public static partial class C2VDebug
	{
		public static readonly string CategoryWhenNull = "null";

		private static readonly Dictionary<string, ILogger> Loggers = new();

		private static ILogger GetOrCreateLogger(string? category)
		{
			category ??= CategoryWhenNull;
			if (!Loggers.TryGetValue(category, out var logger))
			{
				logger = LogManager.GetLogger(category);
				Loggers.Add(category, logger);
			}

			return logger ?? Logger;
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetLoggers()
		{
			Loggers.Clear();
		}

#region String
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogCategory(string? category, string? message)
		{
			GetOrCreateLogger(category).ZLogInformation(message ?? MessageWhenNull);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogWarningCategory(string? category, string? message)
		{
			GetOrCreateLogger(category).ZLogWarning(message ?? MessageWhenNull);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogErrorCategory(string? category, string? message)
		{
			GetOrCreateLogger(category).ZLogError(message ?? MessageWhenNull);
		}
#endregion // String

#region Exception
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogCategory(string? category, Exception? exception)
		{
			LogCategory(category, exception?.Message);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogWarningCategory(string? category, Exception? exception)
		{
			LogWarningCategory(category, exception?.Message);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogErrorCategory(string? category, Exception? exception)
		{
			LogErrorCategory(category, exception?.Message);
		}
#endregion // Exception
	}
}
