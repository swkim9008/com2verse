/*===============================================================
* Product:		Com2Verse
* File Name:	ChannelManagerViewModel.cs
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
	public sealed class ChannelManagerViewModel : CollectionManagerViewModel<string, ChannelViewModel>
	{
		public ChannelManagerViewModel()
		{
			ChannelManager.Instance.ChannelAdded   += OnChannelAdded;
			ChannelManager.Instance.ChannelRemoved += OnChannelRemoved;

			foreach (var channel in ChannelManager.Instance.GetAllChannels())
			{
				OnChannelAdded(channel);
			}
		}

		public void OnChannelAdded(IChannel channel)
		{
			Add(GetKey(channel), new ChannelViewModel(channel));
		}

		public void OnChannelRemoved(IChannel channel)
		{
			Remove(GetKey(channel));
		}

		private static string GetKey(IChannel channel) => channel.Info.ChannelId;

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
					channelManager.ChannelAdded   -= OnChannelAdded;
					channelManager.ChannelRemoved -= OnChannelRemoved;
				}
			}

			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
