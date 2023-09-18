/*===============================================================
* Product:		Com2Verse
* File Name:	BaseAudioDevice.cs
* Developer:	urun4m0r1
* Date:			2022-04-05 17:48
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Communication
{
	public abstract class BaseAudioDevice : BaseDevice, IAudioDevice
	{
		protected override void UpdateAutoRefresh(bool prevUseAutoRefresh, bool useAutoRefresh)
		{
			SystemVolume.UseAutoRefresh = useAutoRefresh;
		}

		protected override void UpdateRefreshInterval(int prevIntervalMs, int intervalMs)
		{
			SystemVolume.RefreshInterval = intervalMs;
		}

		public abstract SystemVolume SystemVolume { get; }

		public override async UniTask Refresh(CancellationTokenSource? tokenSource)
		{
			SystemVolume.IsDeviceExist = false;
			await base.Refresh(tokenSource);
			SystemVolume.IsDeviceExist = SystemDefault.IsAvailable;
		}

		protected override IEnumerable<DeviceInfo> GetDevices()
		{
			RefreshDefaultDevice();

			if (!SystemDefault.IsEmptyDevice)
				yield return SystemDefault;

			foreach (var device in base.GetDevices())
				yield return device;
		}

#region DefaultDevice
		public DeviceInfo SystemDefault { get; protected set; } = DeviceInfo.Empty;

		protected abstract bool TryGetDefaultDevice(out DeviceInfo device);

		private void RefreshDefaultDevice()
		{
			if (!TryGetDefaultDevice(out var device))
			{
				Debug.CommunicationDeviceLogWarningMethod($"System Default {GetType().Name} not found");
				SystemDefault = DeviceInfo.Empty;
				return;
			}

			if (device.IsSameDevice(SystemDefault))
				return;

			SystemDefault = new DeviceInfo(device.Id, $"System Default ({device.Name})", true);
		}
#endregion // DefaultDevice
	}
}
