/*===============================================================
 * Product:		Com2Verse
 * File Name:	C2VDebug_LogMethod.cs
 * Developer:	urun4m0r1
 * Date:		2023-01-19 14:08
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Com2Verse.Logger
{
	public static partial class C2VDebug
	{
		private static readonly string LogMethodFormat = "{0}: {1}";

#region LogMethod
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogMethod(string? category, string? message = null, [CallerMemberName] string? caller = null)
		{
			if (message == null)
			{
				LogCategory(category, caller);
				return;
			}

			LogCategory(category, LogMethodFormat, caller, message);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogWarningMethod(string? category, string? message = null, [CallerMemberName] string? caller = null)
		{
			if (message == null)
			{
				LogWarningCategory(category, caller);
				return;
			}

			LogWarningCategory(category, LogMethodFormat, caller, message);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogErrorMethod(string? category, string? message = null, [CallerMemberName] string? caller = null)
		{
			if (message == null)
			{
				LogErrorCategory(category, caller);
				return;
			}

			LogErrorCategory(category, LogMethodFormat, caller, message);
		}
#endregion // LogMethod

#region Exception
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogMethod(string? category, Exception? exception, [CallerMemberName] string? caller = null)
		{
			LogMethod(category, exception?.Message, caller);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogWarningMethod(string? category, Exception? exception, [CallerMemberName] string? caller = null)
		{
			LogWarningMethod(category, exception?.Message, caller);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogErrorMethod(string? category, Exception? exception, [CallerMemberName] string? caller = null)
		{
			LogErrorMethod(category, exception?.Message, caller);
		}
#endregion // Exception
	}
}
