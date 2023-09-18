/*===============================================================
 * Product:		Com2Verse
 * File Name:	C2VDebug_LogCategoryFormat.cs
 * Developer:	urun4m0r1
 * Date:		2023-01-19 14:08
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Diagnostics;
using ZLogger;

namespace Com2Verse.Logger
{
	public static partial class C2VDebug
	{
#region T1
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogCategory<T1>
			(string? category, string? format, T1? arg1)
		{
			if (format == null)
			{
				LogCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogInformation(format, arg1);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogWarningCategory<T1>
			(string? category, string? format, T1? arg1)
		{
			if (format == null)
			{
				LogWarningCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogWarning(format, arg1);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogErrorCategory<T1>
			(string? category, string? format, T1? arg1)
		{
			if (format == null)
			{
				LogErrorCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogError(format, arg1);
		}
#endregion // T1

#region T2
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogCategory<T1, T2>
			(string? category, string? format, T1? arg1, T2? arg2)
		{
			if (format == null)
			{
				LogCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogInformation(format, arg1, arg2);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogWarningCategory<T1, T2>
			(string? category, string? format, T1? arg1, T2? arg2)
		{
			if (format == null)
			{
				LogWarningCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogWarning(format, arg1, arg2);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogErrorCategory<T1, T2>
			(string? category, string? format, T1? arg1, T2? arg2)
		{
			if (format == null)
			{
				LogErrorCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogError(format, arg1, arg2);
		}
#endregion // T2

#region T3
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogCategory<T1, T2, T3>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3)
		{
			if (format == null)
			{
				LogCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogInformation(format, arg1, arg2, arg3);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogWarningCategory<T1, T2, T3>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3)
		{
			if (format == null)
			{
				LogWarningCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogWarning(format, arg1, arg2, arg3);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogErrorCategory<T1, T2, T3>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3)
		{
			if (format == null)
			{
				LogErrorCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogError(format, arg1, arg2, arg3);
		}
#endregion // T3

#region T4
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogCategory<T1, T2, T3, T4>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4)
		{
			if (format == null)
			{
				LogCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogInformation(format, arg1, arg2, arg3, arg4);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogWarningCategory<T1, T2, T3, T4>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4)
		{
			if (format == null)
			{
				LogWarningCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogWarning(format, arg1, arg2, arg3, arg4);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogErrorCategory<T1, T2, T3, T4>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4)
		{
			if (format == null)
			{
				LogErrorCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogError(format, arg1, arg2, arg3, arg4);
		}
#endregion // T4

#region T5
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogCategory<T1, T2, T3, T4, T5>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5)
		{
			if (format == null)
			{
				LogCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogInformation(format, arg1, arg2, arg3, arg4, arg5);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogWarningCategory<T1, T2, T3, T4, T5>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5)
		{
			if (format == null)
			{
				LogWarningCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogWarning(format, arg1, arg2, arg3, arg4, arg5);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogErrorCategory<T1, T2, T3, T4, T5>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5)
		{
			if (format == null)
			{
				LogErrorCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogError(format, arg1, arg2, arg3, arg4, arg5);
		}
#endregion // T5

#region T6
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogCategory<T1, T2, T3, T4, T5, T6>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6)
		{
			if (format == null)
			{
				LogCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogInformation(format, arg1, arg2, arg3, arg4, arg5, arg6);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogWarningCategory<T1, T2, T3, T4, T5, T6>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6)
		{
			if (format == null)
			{
				LogWarningCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogWarning(format, arg1, arg2, arg3, arg4, arg5, arg6);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogErrorCategory<T1, T2, T3, T4, T5, T6>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6)
		{
			if (format == null)
			{
				LogErrorCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogError(format, arg1, arg2, arg3, arg4, arg5, arg6);
		}
#endregion // T6

#region T7
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogCategory<T1, T2, T3, T4, T5, T6, T7>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7)
		{
			if (format == null)
			{
				LogCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogInformation(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogWarningCategory<T1, T2, T3, T4, T5, T6, T7>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7)
		{
			if (format == null)
			{
				LogWarningCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogWarning(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogErrorCategory<T1, T2, T3, T4, T5, T6, T7>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7)
		{
			if (format == null)
			{
				LogErrorCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogError(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
		}
#endregion // T7

#region T8
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogCategory<T1, T2, T3, T4, T5, T6, T7, T8>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8)
		{
			if (format == null)
			{
				LogCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogInformation(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogWarningCategory<T1, T2, T3, T4, T5, T6, T7, T8>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8)
		{
			if (format == null)
			{
				LogWarningCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogWarning(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogErrorCategory<T1, T2, T3, T4, T5, T6, T7, T8>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8)
		{
			if (format == null)
			{
				LogErrorCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogError(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
		}
#endregion // T8

#region T9
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogCategory<T1, T2, T3, T4, T5, T6, T7, T8, T9>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8, T9? arg9)
		{
			if (format == null)
			{
				LogCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogInformation(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogWarningCategory<T1, T2, T3, T4, T5, T6, T7, T8, T9>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8, T9? arg9)
		{
			if (format == null)
			{
				LogWarningCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogWarning(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogErrorCategory<T1, T2, T3, T4, T5, T6, T7, T8, T9>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8, T9? arg9)
		{
			if (format == null)
			{
				LogErrorCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogError(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
		}
#endregion // T9

#region T10
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogCategory<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8, T9? arg9, T10? arg10)
		{
			if (format == null)
			{
				LogCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogInformation(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogWarningCategory<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8, T9? arg9, T10? arg10)
		{
			if (format == null)
			{
				LogWarningCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogWarning(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogErrorCategory<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8, T9? arg9, T10? arg10)
		{
			if (format == null)
			{
				LogErrorCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogError(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
		}
#endregion // T10

#region T11
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogCategory<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8, T9? arg9, T10? arg10, T11? arg11)
		{
			if (format == null)
			{
				LogCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogInformation(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogWarningCategory<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8, T9? arg9, T10? arg10, T11? arg11)
		{
			if (format == null)
			{
				LogWarningCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogWarning(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogErrorCategory<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8, T9? arg9, T10? arg10, T11? arg11)
		{
			if (format == null)
			{
				LogErrorCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogError(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
		}
#endregion // T11

#region T12
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogCategory<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8, T9? arg9, T10? arg10, T11? arg11, T12? arg12)
		{
			if (format == null)
			{
				LogCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogInformation(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogWarningCategory<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8, T9? arg9, T10? arg10, T11? arg11, T12? arg12)
		{
			if (format == null)
			{
				LogWarningCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogWarning(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogErrorCategory<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8, T9? arg9, T10? arg10, T11? arg11, T12? arg12)
		{
			if (format == null)
			{
				LogErrorCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogError(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
		}
#endregion // T12

#region T13
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogCategory<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8, T9? arg9, T10? arg10, T11? arg11, T12? arg12, T13? arg13)
		{
			if (format == null)
			{
				LogCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogInformation(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogWarningCategory<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8, T9? arg9, T10? arg10, T11? arg11, T12? arg12, T13? arg13)
		{
			if (format == null)
			{
				LogWarningCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogWarning(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogErrorCategory<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8, T9? arg9, T10? arg10, T11? arg11, T12? arg12, T13? arg13)
		{
			if (format == null)
			{
				LogErrorCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogError(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
		}
#endregion // T13

#region T14
		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogCategory<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8, T9? arg9, T10? arg10, T11? arg11, T12? arg12, T13? arg13, T14? arg14)
		{
			if (format == null)
			{
				LogCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogInformation(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogWarningCategory<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8, T9? arg9, T10? arg10, T11? arg11, T12? arg12, T13? arg13, T14? arg14)
		{
			if (format == null)
			{
				LogWarningCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogWarning(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
		}

		[Conditional(LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogErrorCategory<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
			(string? category, string? format, T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8, T9? arg9, T10? arg10, T11? arg11, T12? arg12, T13? arg13, T14? arg14)
		{
			if (format == null)
			{
				LogErrorCategory(category, MessageWhenNull);
				return;
			}

			GetOrCreateLogger(category).ZLogError(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
		}
#endregion // T14
	}
}
