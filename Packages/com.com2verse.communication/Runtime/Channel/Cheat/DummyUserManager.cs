#if ENABLE_CHEATING

/*===============================================================
 * Product:		Com2Verse
 * File Name:	DummyUserManager.cs
 * Developer:	urun4m0r1
 * Date:		2023-03-17 13:06
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Com2Verse.Logger;
using Cysharp.Text;

namespace Com2Verse.Communication.Cheat
{
	/// <summary>
	/// 디버그용 더미 유저들을 관리하는 클래스입니다.<br/>
	/// <see cref="Dispose"/>메서드 호출 시, 모든 유저에 대해 <see cref="UserRemoved"/>이벤트를 발생시킵니다.
	/// </summary>
	public class DummyUserManager : IDisposable
	{
		public event Action<DummyUser>? UserAdded;
		public event Action<DummyUser>? UserRemoved;

		public ChannelInfo ChannelInfo { get; }

		public IReadOnlyCollection<DummyUser> Users => _users;

		private readonly Queue<DummyUser> _users = new();

		public DummyUserManager(ChannelInfo channelInfo)
		{
			ChannelInfo = channelInfo;
		}

		public void AddDummyUser()
		{
			var dummy = DummyUser.CreateInstance(ChannelInfo);

			var count = _users.Count;
			_users.Enqueue(dummy);

			Log(ZString.Format(
				    "{0} -> {1} {2}\n{3}"
				  , count, count + 1, dummy.GetInfoText(), dummy.GetDebugInfo()));

			NotifyDummyUserAdded(dummy);
		}

		public void RemoveDummyUser()
		{
			var count = _users.Count;
			if (!_users.TryDequeue(out var dummy))
			{
				LogWarning("No dummy user to remove.");
				return;
			}

			Log(ZString.Format(
				    "{0} -> {1} {2}\n{3}"
				  , count, count - 1, dummy!.GetInfoText(), dummy.GetDebugInfo()));

			NotifyDummyUserRemoved(dummy);
		}

		public void ClearDummyUsers()
		{
			if (_users.Count == 0)
			{
				LogWarning("No dummy users to clear.");
				return;
			}

			Log(ZString.Format(
				    "Clearing {0} dummy users."
				  , _users.Count));

			while (_users.TryDequeue(out var dummy))
				NotifyDummyUserRemoved(dummy!);
		}

		private void NotifyDummyUserAdded(DummyUser dummy)
		{
			UserAdded?.Invoke(dummy);
		}

		private void NotifyDummyUserRemoved(DummyUser dummy)
		{
			UserRemoved?.Invoke(dummy);
			dummy.Dispose();
		}

#region IDisposable
		private bool _disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				ClearDummyUsers();
			}

			// Uncomment this line in inherited class to implement standard disposing pattern.
			// base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable

#region Debug
		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		protected virtual void Log(string message, [CallerMemberName] string? caller = null)
		{
			C2VDebug.LogMethod(GetLogCategory(), FormatMessage(message), caller);
		}

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		protected virtual void LogWarning(string message, [CallerMemberName] string? caller = null)
		{
			C2VDebug.LogWarningMethod(GetLogCategory(), FormatMessage(message), caller);
		}

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		protected virtual void LogError(string message, [CallerMemberName] string? caller = null)
		{
			C2VDebug.LogErrorMethod(GetLogCategory(), FormatMessage(message), caller);
		}

		[DebuggerHidden, StackTraceIgnore]
		protected virtual string GetLogCategory()
		{
			var className   = GetType().Name;
			var channelInfo = ChannelInfo.GetInfoText();

			return ZString.Format(
				"{0}: {1}"
			  , className, channelInfo);
		}

		[DebuggerHidden, StackTraceIgnore]
		protected virtual string FormatMessage(string message)
		{
			var channelInfo = ChannelInfo.GetDebugInfo();

			return ZString.Format(
				"{0}\n----------\n{1}\n----------\n{2}"
			  , message, GetDebugInfo(), channelInfo);
		}

		[DebuggerHidden, StackTraceIgnore]
		public virtual string GetDebugInfo()
		{
			return ZString.Format(
				"[{0}]\n: UserCount = {1}"
			  , GetLogCategory(), Users.Count);
		}
#endregion // Debug
	}
}

#endif // ENABLE_CHEATING
