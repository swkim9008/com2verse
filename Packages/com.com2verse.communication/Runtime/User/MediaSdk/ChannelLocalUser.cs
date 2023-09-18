/*===============================================================
* Product:		Com2Verse
* File Name:	ChannelLocalUser.cs
* Developer:	urun4m0r1
* Date:			2022-09-02 16:48
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;

namespace Com2Verse.Communication.MediaSdk
{
	/// <inheritdoc cref="IChannelLocalUser"/>
	/// <summary>
	/// <see cref="PublishableLocalUser"/>의 <see cref="IChannelLocalUser"/> 구현체입니다.
	/// </summary>
	internal sealed class ChannelLocalUser : PublishableLocalUser, IChannelLocalUser
	{
		public ILocalTrackManager? ChannelTrackManager { get; private set; }

		public ChannelLocalUser(ChannelInfo channelInfo) : base(channelInfo) { }

		/// <summary>
		/// <see cref="ChannelTrackManager"/>를 할당합니다.
		/// <br/><see cref="CommunicationUser.Dispose"/>시 할당된 <paramref name="channelTrackManager"/>도 함께 해제됩니다.
		/// </summary>
		public void AssignChannelTrackManager(ILocalTrackManager channelTrackManager)
		{
			if (ChannelTrackManager != null)
				throw new InvalidOperationException("Track manager is already assigned.");

			ChannelTrackManager = channelTrackManager;
		}

#region IDisposable
		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				(ChannelTrackManager as IDisposable)?.Dispose();
			}

			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
