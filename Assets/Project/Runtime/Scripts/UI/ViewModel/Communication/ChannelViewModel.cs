/*===============================================================
* Product:		Com2Verse
* File Name:	ChannelViewModel.cs
* Developer:	urun4m0r1
* Date:			2022-06-20 12:45
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Communication;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("Communication")]
	public class ChannelViewModel : ViewModelBase, IDisposable
	{
		private readonly IChannel    _channel;
		private readonly ChannelInfo _channelInfo;

		public ChannelViewModel(IChannel channel)
		{
			_channel     = channel;
			_channelInfo = channel.Info;

			_channel.ConnectionChanged += OnChannelConnectionChanged;
			_channel.UserJoin          += OnUserJoinOrLeft;
			_channel.UserLeft          += OnUserJoinOrLeft;
		}

		public void Dispose()
		{
			_channel.ConnectionChanged -= OnChannelConnectionChanged;
			_channel.UserJoin          -= OnUserJoinOrLeft;
			_channel.UserLeft          -= OnUserJoinOrLeft;
		}

		private void OnChannelConnectionChanged(IChannel channel, eConnectionState state)
		{
			InvokePropertyValueChanged(nameof(ConnectionState), ConnectionState);
		}

		private void OnUserJoinOrLeft(IChannel channel, ICommunicationUser user)
		{
			InvokePropertyValueChanged(nameof(UserCount), UserCount);
		}

#region ViewModelProperties
		[UsedImplicitly] public string           ChannelId       => _channelInfo.ChannelId;
		[UsedImplicitly] public eConnectionState ConnectionState => _channel.Connector.State;
		[UsedImplicitly] public int              UserCount       => _channel.ConnectedUsers.Count;
#endregion // ViewModelProperties
	}
}
