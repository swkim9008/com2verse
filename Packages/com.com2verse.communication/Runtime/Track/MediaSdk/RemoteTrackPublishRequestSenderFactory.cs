/*===============================================================
 * Product:		Com2Verse
 * File Name:	RemoteTrackPublishRequestSenderFactory.cs
 * Developer:	urun4m0r1
 * Date:		2023-03-28 12:43
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

namespace Com2Verse.Communication.MediaSdk
{
	internal sealed class RemoteTrackPublishRequestSenderFactory
	{
		private readonly RtcTrackController            _trackController;
		private readonly IPublishRequestableRemoteUser _target;

		public RemoteTrackPublishRequestSenderFactory(RtcTrackController trackController, IPublishRequestableRemoteUser target)
		{
			_trackController = trackController;
			_target          = target;
		}

		public RemoteTrackPublishRequestSender Create(eTrackType trackType)
		{
			var requestHandler = new DefaultRemoteTrackPublishRequestHandler();
			return new RemoteTrackPublishRequestSender(trackType, _trackController, _target, requestHandler);
		}
	}
}
