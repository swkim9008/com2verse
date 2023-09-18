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

namespace Com2Verse.Communication
{
	[DebuggerStepThrough]
	internal static class Debug
	{
		public static class LogCategory
		{
			public static readonly string CommunicationDevice = "Communication Device";
			public static readonly string NatML               = "Nat ML";
		}


		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void CommunicationDeviceLogMethod(string? message = null, [CallerMemberName] string? caller = null)
			=> C2VDebug.LogMethod(LogCategory.CommunicationDevice, message, caller);

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void CommunicationDeviceLogWarningMethod(string? message = null, [CallerMemberName] string? caller = null)
			=> C2VDebug.LogWarningMethod(LogCategory.CommunicationDevice, message, caller);

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void CommunicationDeviceLogErrorMethod(string? message = null, [CallerMemberName] string? caller = null)
			=> C2VDebug.LogErrorMethod(LogCategory.CommunicationDevice, message, caller);


		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void NatMLLogMethod(string? message = null, [CallerMemberName] string? caller = null)
			=> C2VDebug.LogMethod(LogCategory.NatML, message, caller);

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void NatMLLogWarningMethod(string? message = null, [CallerMemberName] string? caller = null)
			=> C2VDebug.LogWarningMethod(LogCategory.NatML, message, caller);

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void NatMLLogErrorMethod(string? message = null, [CallerMemberName] string? caller = null)
			=> C2VDebug.LogErrorMethod(LogCategory.NatML, message, caller);
	}
}
