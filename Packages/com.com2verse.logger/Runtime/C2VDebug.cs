/*===============================================================
 * Product:		Com2Verse
 * File Name:	C2VDebug.cs
 * Developer:	urun4m0r1
 * Date:		2023-01-19 14:08
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Com2Verse.Logger
{
	/// <summary>
	/// <a href="https://jira.com2us.com/wiki/pages/viewpage.action?pageId=352174379">Confluence</a>
	/// </summary>
	[DebuggerStepThrough]
	public static partial class C2VDebug
	{
		public const string LogDefinition     = "ENABLE_LOGGING";
		public const string VerboseDefinition = "ENABLE_LOGGING_VERBOSE";

		private const string BoxingWarningMessage = "해당 메서드는 박싱을 발생시킵니다. 성능에 주의하세요.";
		private const string ParamsWarningMessage = "해당 메서드는 박싱 및 params 배열을 발생시킵니다. 성능에 주의하세요.";

		private static readonly string MessageWhenNull = "null";

		private static ILogger Logger => LogManager.GlobalLogger;

#region String
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void Log(string? message)
		{
			Logger.ZLogInformation(message ?? MessageWhenNull);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogWarning(string? message)
		{
			Logger.ZLogWarning(message ?? MessageWhenNull);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogError(string? message)
		{
			Logger.ZLogError(message ?? MessageWhenNull);
		}
#endregion // String

#region Exception
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void Log(Exception? exception)
		{
			Log(exception?.Message);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogWarning(Exception? exception)
		{
			LogWarning(exception?.Message);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogError(Exception? exception)
		{
			LogError(exception?.Message);
		}
#endregion // Exception

#region Object
		[Obsolete(BoxingWarningMessage)]
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void Log(object? obj)
		{
			Log(obj?.ToString());
		}

		[Obsolete(BoxingWarningMessage)]
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogWarning(object? obj)
		{
			LogWarning(obj?.ToString());
		}

		[Obsolete(BoxingWarningMessage)]
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogError(object? obj)
		{
			LogError(obj?.ToString());
		}
#endregion // Object

#region Params
		[Obsolete(ParamsWarningMessage)]
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void Log(string? format, params object?[]? par)
		{
			if (format == null)
			{
				Log(MessageWhenNull);
				return;
			}

			if (par == null)
			{
				Log(format);
				return;
			}

			Log(string.Format(format, par));
		}

		[Obsolete(ParamsWarningMessage)]
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogWarning(string? format, params object?[]? par)
		{
			if (format == null)
			{
				LogWarning(MessageWhenNull);
				return;
			}

			if (par == null)
			{
				LogWarning(format);
				return;
			}

			LogWarning(string.Format(format, par));
		}

		[Obsolete(ParamsWarningMessage)]
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogError(string? format, params object?[]? par)
		{
			if (format == null)
			{
				LogError(MessageWhenNull);
				return;
			}

			if (par == null)
			{
				LogError(format);
				return;
			}

			LogError(string.Format(format, par));
		}
#endregion // Params
	}
}
