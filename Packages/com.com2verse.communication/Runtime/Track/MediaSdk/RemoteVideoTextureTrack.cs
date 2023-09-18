/*===============================================================
 * Product:		Com2Verse
 * File Name:	RemoteAudioSourceTrack.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-14 22:38
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Utils;
using Com2Verse.Solution.UnityRTCSdk;
using UnityEngine;

namespace Com2Verse.Communication.MediaSdk
{
	internal sealed class RemoteVideoTextureTrack : RemoteMediaTrack, IVideoTextureTrack
	{
		public Texture? VideoTexture => _payload.Texture;

		private readonly RemoteVideoTrack _payload;

		public RemoteVideoTextureTrack(
			eTrackType                              trackType
		  , RtcTrackController                      trackController
		  , ISubscribableRemoteUser                 owner
		  , RemoteVideoTrack                        payload
		  , ObservableHashSet<IRemoteTrackObserver> observers
		) : base(trackType, trackController, owner, payload, observers)
		{
			_payload = payload;
		}
	};
}
