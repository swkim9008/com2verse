/*===============================================================
 * Product:		Com2Verse
 * File Name:	SubscribeOnlyRemoteUser.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-20 18:28
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using MediaSdkUser = Com2Verse.Solution.UnityRTCSdk.User;

namespace Com2Verse.Communication.MediaSdk
{
	/// <inheritdoc cref="ISubscribableRemoteUser"/>
	/// <summary>
	/// <see cref="CommunicationUser"/>의 <see cref="ISubscribableRemoteUser"/> 구현체입니다.
	/// </summary>
	internal class SubscribeOnlyRemoteUser : CommunicationUser, ISubscribableRemoteUser
	{
		public MediaSdkUser MediaSdkUser { get; }

		public IRemoteTrackManager? SubscribeTrackManager { get; private set; }

		public MediaModules Modules { get; }

		private Audio? _voice;
		private Video? _camera;
		private Video? _screen;

		private RemoteAudioProvider?  _voiceProvider;
		private RemoteVideoProvider?  _cameraProvider;
		private RemoteScreenProvider? _screenProvider;

		public SubscribeOnlyRemoteUser(ChannelInfo channelInfo, User user, eUserRole role, MediaSdkUser mediaSdkUser)
			: base(channelInfo, user, role)
		{
			MediaSdkUser = mediaSdkUser;

			Modules = new(GetAudio, GetVideo);
		}

		private Audio? GetAudio(eTrackType trackType) => trackType switch
		{
			eTrackType.VOICE => _voice,
			_                => null,
		};

		private Video? GetVideo(eTrackType trackType) => trackType switch
		{
			eTrackType.CAMERA => _camera,
			eTrackType.SCREEN => _screen,
			_                 => null,
		};

		/// <summary>
		/// <see cref="IRemoteTrackManager"/>를 할당하고 미디어 트랙 모듈을 초기화합니다.
		/// <br/><see cref="CommunicationUser.Dispose"/>시 할당된 <paramref name="trackManager"/>도 함께 해제됩니다.
		/// </summary>
		public void AssignTrackManager(IRemoteTrackManager trackManager)
		{
			if (SubscribeTrackManager != null)
				throw new InvalidOperationException("Track manager is already assigned.");

			SubscribeTrackManager = trackManager;

			_voiceProvider = new RemoteAudioProvider(SubscribeTrackManager, eTrackType.VOICE)
			{
				IsRunning = true,
				IsAudible = true,
				Level     = 1f,
			};

			_cameraProvider = new RemoteVideoProvider(SubscribeTrackManager, eTrackType.CAMERA)
			{
				IsRunning = true,
			};

			_screenProvider = new RemoteScreenProvider(SubscribeTrackManager, eTrackType.SCREEN)
			{
				IsRunning = true,
			};

			_voice  = new Audio(_voiceProvider);
			_camera = new Video(_cameraProvider);
			_screen = new Video(_screenProvider);
		}

		public bool ContainsObserver(eTrackType trackType, IRemoteTrackObserver observer)
		{
			return SubscribeTrackManager?.ContainsObserver(trackType, observer) ?? false;
		}

		public bool TryAddObserver(eTrackType trackType, IRemoteTrackObserver observer)
		{
			return SubscribeTrackManager?.TryAddObserver(trackType, observer) ?? false;
		}

		public bool RemoveObserver(eTrackType trackType, IRemoteTrackObserver observer)
		{
			return SubscribeTrackManager?.RemoveObserver(trackType, observer) ?? false;
		}

#region IDisposable
		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				_voiceProvider?.Dispose();
				_cameraProvider?.Dispose();
				_screenProvider?.Dispose();

				_voice?.Dispose();
				_camera?.Dispose();
				_screen?.Dispose();

				Modules.Dispose();

				(SubscribeTrackManager as IDisposable)?.Dispose();
			}

			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
