/*===============================================================
 * Product:		Com2Verse
 * File Name:	Debug.cs
 * Developer:	urun4m0r1
 * Date:		2023-01-27 13:56
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Com2Verse.Logger;

namespace Com2Verse.ScreenShare
{
	[DebuggerStepThrough]
	internal static class Debug
	{
		public static class LogCategory
		{
			public static readonly string ScreenCapture = "ScreenCapture";
		}


		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void ScreenCaptureLogMethod(string? message = null, [CallerMemberName] string? caller = null)
			=> C2VDebug.LogMethod(LogCategory.ScreenCapture, message, caller);

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void ScreenCaptureLogWarningMethod(string? message = null, [CallerMemberName] string? caller = null)
			=> C2VDebug.LogWarningMethod(LogCategory.ScreenCapture, message, caller);

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void ScreenCaptureLogErrorMethod(string? message = null, [CallerMemberName] string? caller = null)
			=> C2VDebug.LogErrorMethod(LogCategory.ScreenCapture, message, caller);
	}
}
