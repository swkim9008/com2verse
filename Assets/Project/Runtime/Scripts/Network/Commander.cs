/*===============================================================
* Product:		Com2Verse
* File Name:	Commander.cs
* Developer:	haminjeong
* Date:			2022-05-10 09:44
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Com2Verse.Logger;

namespace Com2Verse.Network
{
	public sealed partial class Commander : Singleton<Commander>
	{
		private static readonly Google.Protobuf.WellKnownTypes.Empty SEmpty = new();

		private Commander() { }

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogPacketSend(string message = null, [CallerMemberName] string caller = null)
			=> C2VDebug.LogMethod(nameof(Commander), message, caller);
	}
}
