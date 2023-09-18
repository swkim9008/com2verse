/*===============================================================
* Product:		Com2Verse
* File Name:	LocalWebcamModule.cs
* Developer:	urun4m0r1
* Date:			2022-08-24 18:07
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using UnityEngine;

namespace Com2Verse.Communication.Unity
{
	public sealed class WebcamTextureProvider : IVideoTextureProvider, IDisposable
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

				UpdateWebcamConnection();
			}
		}
#endregion // IModule

#region IVideoTextureProvider
		public event Action<Texture?>? TextureChanged;

		private Texture? _texture;

		public Texture? Texture
		{
			get => _texture;
			private set
			{
				var prevValue = _texture;
				if (prevValue == value)
					return;

				_texture = value;
				TextureChanged?.Invoke(value);
			}
		}
#endregion // IVideoTextureProvider

		private readonly IDevice                _device;
		private readonly IReadOnlyVideoSettings _requestedSettings;
		private readonly WebcamConnector        _webcamConnector;

		public WebcamTextureProvider(IDevice device, IReadOnlyVideoSettings requestedSettings)
		{
			_device            = device;
			_requestedSettings = requestedSettings;
			_webcamConnector   = new WebcamConnector(device, requestedSettings);

			_device.DeviceChanged              += OnDeviceChanged;
			_device.DeviceFailed               += OnDeviceFailed;
			_requestedSettings.SettingsChanged += OnRequestedSettingsChanged;
			_webcamConnector.StateChanged      += OnStateChanged;
		}

		public void RefreshTexture()
		{
			UpdateWebcamConnection();
		}

		private void OnDeviceChanged(DeviceInfo prevDevice, int prevIndex, DeviceInfo deviceInfo, int deviceIndex)
		{
			UpdateWebcamConnection();
		}

		private void OnDeviceFailed(DeviceInfo deviceInfo, int deviceIndex)
		{
			UpdateWebcamConnection();
		}

		private void OnRequestedSettingsChanged(IReadOnlyVideoSettings _)
		{
			UpdateWebcamConnection();
		}

		private void UpdateWebcamConnection()
		{
			if (IsRunning && _device.Current.IsAvailable)
			{
				_webcamConnector.TryForceConnect();
			}
			else
			{
				Texture   = null;

				_webcamConnector.TryDisconnect();
			}
		}

		private void OnStateChanged(IConnectionController controller, eConnectionState state)
		{
			Texture = state is eConnectionState.CONNECTED
				? _webcamConnector.WebcamTexture
				: null;
		}

		public void Dispose()
		{
			_device.DeviceChanged              -= OnDeviceChanged;
			_device.DeviceFailed               -= OnDeviceFailed;
			_requestedSettings.SettingsChanged -= OnRequestedSettingsChanged;
			_webcamConnector.StateChanged      -= OnStateChanged;

			_webcamConnector.Dispose();
		}
	}
}
