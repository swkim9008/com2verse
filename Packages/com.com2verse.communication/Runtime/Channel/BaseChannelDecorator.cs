#if ENABLE_CHEATING

/*===============================================================
 * Product:		Com2Verse
 * File Name:	BaseChannelDecorator.cs
 * Developer:	urun4m0r1
 * Date:		2023-03-17 16:23
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;

namespace Com2Verse.Communication
{
	public abstract class BaseChannelDecorator : IChannel, IDisposable
	{
		public IConnectionController Connector => DecoratedChannel.Connector;

		public event Action<IChannel, eConnectionState>? ConnectionChanged
		{
			add => DecoratedChannel.ConnectionChanged += value;
			remove => DecoratedChannel.ConnectionChanged -= value;
		}

		public event Action<IChannel, ICommunicationUser>? UserJoin
		{
			add => DecoratedChannel.UserJoin += value;
			remove => DecoratedChannel.UserJoin -= value;
		}

		public event Action<IChannel, ICommunicationUser>? UserLeft
		{
			add => DecoratedChannel.UserLeft += value;
			remove => DecoratedChannel.UserLeft -= value;
		}

		public event Action<IChannel, ICommunicationUser?, ICommunicationUser?>? HostChanged
		{
			add => DecoratedChannel.HostChanged += value;
			remove => DecoratedChannel.HostChanged -= value;
		}

		public event Action<IChannel, ILocalUser?, ILocalUser?>? SelfChanged
		{
			add => DecoratedChannel.SelfChanged += value;
			remove => DecoratedChannel.SelfChanged -= value;
		}

		public ChannelInfo Info => DecoratedChannel.Info;

		public IReadOnlyDictionary<Uid, ICommunicationUser> ConnectedUsers => DecoratedChannel.ConnectedUsers;

		public ICommunicationUser? Host => DecoratedChannel.Host;
		public ILocalUser?         Self => DecoratedChannel.Self;

		public string GetDebugInfo() => DecoratedChannel.GetDebugInfo();

		protected IChannel DecoratedChannel { get; }

		protected BaseChannelDecorator(IChannel channel)
		{
			DecoratedChannel = channel;
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
				(DecoratedChannel as IDisposable)?.Dispose();
			}

			// Uncomment this line in inherited class to implement standard disposing pattern.
			// base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}

#endif // ENABLE_CHEATING
