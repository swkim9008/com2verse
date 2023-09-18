/*===============================================================
* Product:		Com2Verse
* File Name:	CommunicationUserManagerViewModel.cs
* Developer:	urun4m0r1
* Date:			2022-06-17 13:12
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Communication;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("Communication")]
	public sealed class CommunicationUserManagerViewModel : CollectionManagerViewModel<Uid, CommunicationUserViewModel>
	{
		public CommunicationUserManagerViewModel()
		{
			ChannelManager.Instance.ViewModelUserAdded   += OnChannelUserAdded;
			ChannelManager.Instance.ViewModelUserRemoved += OnChannelUserRemoved;

			foreach (var user in ChannelManager.Instance.GetViewModelUsers())
			{
				AddUser(user);
			}
		}

		private void OnChannelUserAdded(IChannel channel, IViewModelUser user)
		{
			if (user.User.Uid.Equals(0))
				return;
			AddUser(user);
		}

		private void OnChannelUserRemoved(IChannel channel, IViewModelUser user)
		{
			RemoveUser(user);
		}

		private void AddUser(IViewModelUser user)
		{
			Add(user.User.Uid, new CommunicationUserViewModel(user));
		}

		private void RemoveUser(IViewModelUser user)
		{
			Remove(user.User.Uid);
		}

#region IDisposable
		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				var channelManager = ChannelManager.InstanceOrNull;
				if (channelManager != null)
				{
					channelManager.ViewModelUserAdded   -= OnChannelUserAdded;
					channelManager.ViewModelUserRemoved -= OnChannelUserRemoved;
				}
			}

			// Uncomment this line in inherited class to implement standard disposing pattern.
			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
