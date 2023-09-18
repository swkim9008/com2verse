/*===============================================================
* Product:		Com2Verse
* File Name:	PacketReceiver.cs
* Developer:	urun4m0r1
* Date:			2022-06-08 18:36
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Com2Verse.Logger;
using JetBrains.Annotations;
using Protocols.Communication;

namespace Com2Verse.Network.Communication
{
	public partial class PacketReceiver : Singleton<PacketReceiver>, IDisposable
	{
		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private PacketReceiver() { }

#region Event_Common
		public event Action<JoinChannelResponse>?  JoinChannelResponse;
		public event Action<LeaveChannelResponse>? LeaveChannelResponse;
#endregion // Event_Common

#region Response_Common
		public void RaiseJoinChannelResponse(JoinChannelResponse response)
		{
			LogPacketReceived(response.ToString());
			JoinChannelResponse?.Invoke(response);
		}

		public void RaiseLeaveChannelResponse(LeaveChannelResponse response)
		{
			LogPacketReceived(response.ToString());
			LeaveChannelResponse?.Invoke(response);
		}
#endregion // Response_Common

		public void Dispose()
		{
			DestroyMeetingRoom();
			DestroyConnectingApp();
		}

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		public static void LogPacketReceived(string? message = null, [CallerMemberName] string? caller = null)
			=> C2VDebug.LogMethod(nameof(PacketReceiver), message, caller);
	}
}
