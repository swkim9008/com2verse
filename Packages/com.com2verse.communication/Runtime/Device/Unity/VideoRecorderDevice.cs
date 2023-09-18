/*===============================================================
* Product:		Com2Verse
* File Name:	VideoRecorderDevice.cs
* Developer:	urun4m0r1
* Date:			2022-08-04 16:40
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using Com2Verse.Extension;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.Communication.Unity
{
	public sealed class VideoRecorderDevice : BaseDevice, IDisposable
	{
		private WebCamDevice[]? _unityDevices;

		public IReadOnlyList<WebCamDevice>? UnityDevices => _unityDevices;

		public WebCamDevice CurrentUnityDevice { get; private set; }

		private static WebCamDevice[] GetNewDevices()
			=> WebCamTexture.devices
			?? throw new DeviceException($"Failed to get {WebCamTexture.devices} list.");

		protected override void UpdateAutoRefresh(bool prevUseAutoRefresh, bool useAutoRefresh)
		{
			if (useAutoRefresh) StartCheckDevices();
			else StopCheckDevices();
		}

		protected override void UpdateRefreshInterval(int prevIntervalMs, int intervalMs) { }

		private CancellationTokenSource? _tokenSource;

		private void StartCheckDevices()
		{
			StopCheckDevices();
			_tokenSource ??= new CancellationTokenSource();
			CheckDevices().Forget();
		}

		private void StopCheckDevices()
		{
			_tokenSource?.Cancel();
			_tokenSource?.Dispose();
			_tokenSource = null;
		}

		private async UniTask CheckDevices()
		{
			while (await UniTaskHelper.Delay(RefreshInterval, _tokenSource))
			{
				await Refresh(_tokenSource);
			}
		}

		public override async UniTask Refresh(CancellationTokenSource? tokenSource)
		{
			var devices = GetNewDevices();
			if (devices.Length != _unityDevices?.Length)
			{
				_unityDevices = devices;
				await base.Refresh(tokenSource);
			}
		}

		protected override int GetDevicesCount() => _unityDevices?.Length ?? 0;

		protected override bool TryChangeCurrentDevice(DeviceInfo device)
		{
			if (!TryGetUnityDevice(device, out var unityDevice))
			{
				return false;
			}

			CurrentUnityDevice = unityDevice;
			return true;
		}

		protected override bool TryGetDeviceAt(int index, out DeviceInfo device)
		{
			if (_unityDevices == null || !_unityDevices.TryGetAt(index, out var unityDevice))
			{
				device = DeviceInfo.Empty;
				return false;
			}

			device = Convert(unityDevice);
			return true;
		}

		private bool TryGetUnityDevice(DeviceInfo device, out WebCamDevice result)
		{
			if (_unityDevices != null)
			{
				foreach (var unityDevice in _unityDevices)
				{
					if (unityDevice.name == device.Name)
					{
						result = unityDevice;
						return true;
					}
				}
			}

			result = default;
			return false;
		}

		private static DeviceInfo Convert(WebCamDevice unityDevice)
		{
			var id   = unityDevice.name ?? string.Empty;
			var name = unityDevice.name ?? string.Empty;

			return new DeviceInfo(id, name);
		}

		public void Dispose()
		{
			StopCheckDevices();
		}
	}
}
