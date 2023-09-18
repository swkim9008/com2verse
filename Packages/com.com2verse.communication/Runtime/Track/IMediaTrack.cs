/*===============================================================
 * Product:		Com2Verse
 * File Name:	IMediaTrack.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-10 14:56
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Utils;
using Com2Verse.Sound;
using UnityEngine;

namespace Com2Verse.Communication
{
	public interface IMediaTrack
	{
		event Action<IMediaTrack, eConnectionState>? ConnectionChanged;

		MediaTrackConnector Connector { get; }
		ICommunicationUser  Owner     { get; }
		eTrackType          Type      { get; }
	}

	public interface ILocalMediaTrack : IMediaTrack { }

	public interface IRemoteMediaTrack : IMediaTrack
	{
		ObservableHashSet<IRemoteTrackObserver> Observers { get; }
	}

	public interface IAudioSourceTrack : IMediaTrack
	{
		MetaverseAudioSource? AudioSource { get; }
	}

	public interface IVideoTextureTrack : IMediaTrack
	{
		Texture? VideoTexture { get; }
	}
}
