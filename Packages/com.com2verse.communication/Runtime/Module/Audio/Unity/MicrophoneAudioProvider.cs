/*===============================================================
* Product:		Com2Verse
* File Name:	MicrophoneAudioProvider.cs
* Developer:	urun4m0r1
* Date:			2022-08-24 18:07
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Extension;
using Com2Verse.Sound;

namespace Com2Verse.Communication.Unity
{
	public sealed class MicrophoneAudioProvider : BaseVolume, IAudioSourceProvider
	{
#region IModule
		public event Action<bool>? StateChanged;

		private bool _isRunning;

		public bool IsRunning
		{
			get => _isRunning;
			set
			{
				var prevValue = _isRunning;
				if (prevValue == value)
					return;

				_isRunning = value;
				StateChanged?.Invoke(value);

				UpdateMicrophoneConnection();
			}
		}
#endregion // IModule

#region IVolume
		protected override void ApplyLevel(float value)
		{
			base.ApplyLevel(value);

			if (!AudioSource.IsUnityNull())
				AudioSource!.Volume = value;
		}

		protected override void ApplyAudible(bool value)
		{
			base.ApplyAudible(value);

			if (!AudioSource.IsUnityNull())
				AudioSource!.TargetMixerGroup = GetAudioMixerGroupIndex(value);
		}
#endregion // IVolume

#region IAudioSourceProvider
		public event Action<MetaverseAudioSource?>? AudioSourceChanged;

		private MetaverseAudioSource? _audioSource;

		public MetaverseAudioSource? AudioSource
		{
			get => _audioSource;
			private set
			{
				var prevValue = _audioSource;
				if (prevValue == value)
					return;

				_audioSource = value;
				AudioSourceChanged?.Invoke(value);

				ApplyLevel(Level);
				ApplyAudible(IsAudible);
			}
		}

		private static int GetAudioMixerGroupIndex(bool isAudible) => isAudible
			? AudioMixerGroupIndex.LocalVoice
			: AudioMixerGroupIndex.Mute;
#endregion // IAudioSourceProvider

		private readonly IDevice             _device;
		private readonly AudioSettings       _requestedSettings;
		private readonly MicrophoneConnector _microphoneConnector;

		public MicrophoneAudioProvider(IDevice device, AudioSettings requestedSettings) : base(1f)
		{
			_device              = device;
			_requestedSettings   = requestedSettings;
			_microphoneConnector = new MicrophoneConnector(device, requestedSettings);

			_device.DeviceChanged              += OnDeviceChanged;
			_device.DeviceFailed               += OnDeviceFailed;
			_requestedSettings.SettingsChanged += OnRequestedSettingsChanged;
			_microphoneConnector.StateChanged  += OnStateChanged;
		}

		public void RefreshAudioSource()
		{
			UpdateMicrophoneConnection();
		}

		private void OnDeviceChanged(DeviceInfo prevDevice, int prevIndex, DeviceInfo deviceInfo, int deviceIndex)
		{
			UpdateMicrophoneConnection();
		}

		private void OnDeviceFailed(DeviceInfo deviceInfo, int deviceIndex)
		{
			UpdateMicrophoneConnection();
		}

		private void OnRequestedSettingsChanged(IReadOnlyAudioSettings _)
		{
			UpdateMicrophoneConnection();
		}

		private void UpdateMicrophoneConnection()
		{
			if (IsRunning && _device.Current.IsAvailable)
			{
				_microphoneConnector.TryForceConnect();
			}
			else
			{
				AudioSource = null;

				_microphoneConnector.TryDisconnect();
			}
		}

		private void OnStateChanged(IConnectionController controller, eConnectionState state)
		{
			AudioSource = state is eConnectionState.CONNECTED
				? _microphoneConnector.AudioSource
				: null;
		}

#region IDisposable
		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				_device.DeviceChanged              -= OnDeviceChanged;
				_device.DeviceFailed               -= OnDeviceFailed;
				_requestedSettings.SettingsChanged -= OnRequestedSettingsChanged;
				_microphoneConnector.StateChanged  -= OnStateChanged;

				_microphoneConnector.Dispose();
			}

			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
