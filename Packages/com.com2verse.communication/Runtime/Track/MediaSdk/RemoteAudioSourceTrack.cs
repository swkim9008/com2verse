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
using Com2Verse.Extension;
using Com2Verse.Solution.UnityRTCSdk;
using Com2Verse.Sound;
using Cysharp.Text;
using UnityEngine;

namespace Com2Verse.Communication.MediaSdk
{
	internal sealed class RemoteAudioSourceTrack : RemoteMediaTrack, IAudioSourceTrack
	{
		public MetaverseAudioSource? AudioSource { get; }

		public RemoteAudioSourceTrack(
			eTrackType                              trackType
		  , RtcTrackController                      trackController
		  , ISubscribableRemoteUser                 owner
		  , RemoteAudioTrack                        payload
		  , ObservableHashSet<IRemoteTrackObserver> observers
		) : base(trackType, trackController, owner, payload, observers)
		{
			var className   = GetType().Name;
			var channelInfo = trackController.ChannelInfo.GetInfoText();
			var ownerInfo   = Owner.GetInfoText();

			var name = ZString.Format(
				"[{0}: {1} / {2}] ({3})"
			  , className, channelInfo, ownerInfo, trackType);

			var go = new GameObject(name) { hideFlags = HideFlags.DontSave };
			Object.DontDestroyOnLoad(go);

			var audioSource = go.AddComponent<AudioSource>()!;
			payload.Source = audioSource;

			AudioSource = MetaverseAudioSource.CreateWithSource(go, audioSource)!;

			audioSource.name = name;
			AudioSource.name = name;
		}

#region IDisposable
		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				AudioSource.DestroyGameObject();
			}

			// Uncomment this line in inherited class to implement standard disposing pattern.
			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
