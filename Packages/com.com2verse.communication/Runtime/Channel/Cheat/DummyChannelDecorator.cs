#if ENABLE_CHEATING

/*===============================================================
 * Product:		Com2Verse
 * File Name:	DummyChannelDecorator.cs
 * Developer:	urun4m0r1
 * Date:		2023-03-17 16:23
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

namespace Com2Verse.Communication.Cheat
{
	public class DummyChannelDecorator : BaseChannelDecorator
	{
		public DummyUserManager DummyUserManager { get; }

		private IUserEventInvoker? UserEventInvoker => DecoratedChannel as IUserEventInvoker;

		public DummyChannelDecorator(IChannel channel) : base(channel)
		{
			DummyUserManager = new DummyUserManager(Info);

			DummyUserManager.UserAdded   += OnDummyUserAdded;
			DummyUserManager.UserRemoved += OnDummyUserRemoved;
		}

		private void OnDummyUserAdded(ICommunicationUser user)
		{
			UserEventInvoker?.RaiseUserJoinedEvent(user);
		}

		private void OnDummyUserRemoved(ICommunicationUser user)
		{
			UserEventInvoker?.RaiseUserLeftEvent(user);
		}

#region IDisposable
		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				DummyUserManager.Dispose();
			}

			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}

#endif // ENABLE_CHEATING
