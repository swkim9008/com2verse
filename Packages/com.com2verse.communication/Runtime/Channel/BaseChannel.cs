/*===============================================================
* Product:		Com2Verse
* File Name:	BaseChannel.cs
* Developer:	urun4m0r1
* Date:			2022-04-07 19:26
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Com2Verse.Logger;
using Cysharp.Text;

namespace Com2Verse.Communication
{
	public abstract class BaseChannel : IChannel, IUserEventInvoker, IDisposable
	{
		public abstract IConnectionController Connector { get; }

		public event Action<IChannel, eConnectionState>? ConnectionChanged;

		public event Action<IChannel, ICommunicationUser>? UserJoin;
		public event Action<IChannel, ICommunicationUser>? UserLeft;

		public event Action<IChannel, ICommunicationUser?, ICommunicationUser?>? HostChanged;
		public event Action<IChannel, ILocalUser?, ILocalUser?>?                 SelfChanged;

		public ChannelInfo Info { get; }

		public IReadOnlyDictionary<Uid, ICommunicationUser> ConnectedUsers => Users;

		public Dictionary<Uid, ICommunicationUser> Users { get; } = new(UidComparer.Default);

		public ICommunicationUser? Host
		{
			get => _host;
			private set
			{
				var prevValue = _host;
				if (prevValue == value)
					return;

				_host = value;
				HostChanged?.Invoke(this, prevValue, value);
			}
		}

		public ILocalUser? Self
		{
			get => _self;
			private set
			{
				var prevValue = _self;
				if (prevValue == value)
					return;

				_self = value;
				SelfChanged?.Invoke(this, prevValue, value);
			}
		}

		protected IUserEventInvoker UserEventInvoker => this;

		private ICommunicationUser? _host;
		private ILocalUser?         _self;

		protected BaseChannel(ChannelInfo channelInfo)
		{
			Info = channelInfo;

			ChannelManager.Instance.NotifyChannelCreated(this);
		}

		internal void RaiseConnectionChangedEvent(IConnectionController controller, eConnectionState state)
		{
			OnConnectionChanged(state);
		}

		void IUserEventInvoker.RaiseUserJoinedEvent(ICommunicationUser user) => OnUserJoin(user);
		void IUserEventInvoker.RaiseUserLeftEvent(ICommunicationUser   user) => OnUserLeft(user);

		private void OnConnectionChanged(eConnectionState state)
		{
			switch (state)
			{
				case eConnectionState.CONNECTED:
				{
					UserEventInvoker.RaiseUserJoinedEvent(CreateSelf());
					break;
				}
				case eConnectionState.DISCONNECTED:
				{
					Dispose();
					break;
				}
			}

			ConnectionChanged?.Invoke(this, state);
		}

		protected abstract ILocalUser CreateSelf();

		private void OnUserJoin(ICommunicationUser user)
		{
			var count = Users.Count;
			if (!Users.TryAdd(user.User.Uid, user))
			{
				LogWarning(Format("User already exists with same uid, operation ignored.", user));
				return;
			}

			var message = ZString.Format("{0} -> {1}", count, count + 1);
			Log(Format(message, user));

			NotifyUserJoin(user);
		}

		private void OnUserLeft(ICommunicationUser user)
		{
			if (user.IsP2PChannelHost())
			{
				Log(Format("Channel host left, channel will be disposed.", user));
				Dispose();
				return;
			}

			var count = Users.Count;
			if (!Users.Remove(user.User.Uid))
			{
				LogWarning(Format("User not found, operation ignored.", user));
				return;
			}

			var message = ZString.Format("{0} -> {1}", count, count - 1);
			Log(Format(message, user));

			NotifyUserLeft(user);
		}

		private void ClearUsers()
		{
			if (Users.Count == 0)
			{
				LogWarning("No users to clear.");
				return;
			}

			Log(ZString.Format(
				    "Clearing {0} users."
				  , Users.Count));

			foreach (var user in Users.Values)
				NotifyUserLeft(user);

			Users.Clear();
		}

		private void NotifyUserJoin(ICommunicationUser user)
		{
			if (user.IsP2PChannelHost())
				Host = user;

			if (user is ILocalUser localUser)
				Self = localUser;

			if (user is not EmptyUser)
				UserJoin?.Invoke(this, user);
		}

		private void NotifyUserLeft(ICommunicationUser user)
		{
			if (user.IsP2PChannelHost())
				Host = null;

			if (user is ILocalUser)
				Self = null;

			if (user is not EmptyUser)
				UserLeft?.Invoke(this, user);

			(user as IDisposable)?.Dispose();
		}

		protected bool TryFindRemoteUser(User user, [NotNullWhen(true)] out IRemoteUser? remote)
		{
			remote = FindRemoteUser(user);
			return remote != null;
		}

		protected IRemoteUser? FindRemoteUser(User user)
		{
			return Users.TryGetValue(user.Uid, out var remote) ? remote as IRemoteUser : null;
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
				ClearUsers();

				ConnectionChanged?.Invoke(this, eConnectionState.DISCONNECTED);
				ChannelManager.Instance.NotifyChannelDisposed(this);
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
			var channelInfo = Info.GetInfoText();

			return ZString.Format(
				"{0}: {1}"
			  , className, channelInfo);
		}

		[DebuggerHidden, StackTraceIgnore]
		protected virtual string FormatMessage(string message)
		{
			var connectorInfo = Connector.GetDebugInfo();
			var channelInfo   = Info.GetDebugInfo();

			return ZString.Format(
				"{0}\n----------\n{1}\n----------\n{2}\n----------\n{3}"
			  , message, GetDebugInfo(), connectorInfo, channelInfo);
		}

		[DebuggerHidden, StackTraceIgnore]
		protected virtual string Format(string message, ICommunicationUser target)
		{
			return ZString.Format(
				"{0} / {1}"
			  , message, target.GetDebugInfo());
		}

		[DebuggerHidden, StackTraceIgnore]
		public virtual string GetDebugInfo()
		{
			var selfInfo = Self?.GetInfoText() ?? "null";
			var hostInfo = Host?.GetInfoText() ?? "null";

			return ZString.Format(
				"[{0}]\n: UserCount = {1}\n: Self = {2}\n: Host = {3}"
			  , GetLogCategory(), Users.Count, selfInfo, hostInfo);
		}
#endregion // Debug
	}
}
