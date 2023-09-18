/*===============================================================
* Product:		Com2Verse
* File Name:	BaseDevice.cs
* Developer:	urun4m0r1
* Date:			2022-04-05 17:48
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using Com2Verse.Extension;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Communication
{
	public abstract class BaseDevice : IDevice
	{
#region Events
		public event Action<DeviceInfo, int, DeviceInfo, int>? DeviceChanged;

		public event Action<DeviceInfo, int>? DeviceFailed;

		public event Action<IEnumerable<DeviceInfo>>? DeviceListChanged;

		public void RaiseDeviceFailedEvent()
		{
			Current.IsAvailable = false;
			DeviceFailed?.Invoke(Current, Index);
		}
#endregion // Events

#region CurrentDevice
		private int        _currentDeviceIndex = -1;
		private DeviceInfo _currentDeviceInfo  = DeviceInfo.Empty;

		public int Index
		{
			get => _currentDeviceIndex;
			set
			{
				if (_currentDeviceIndex == value) return;

				if (value < 0)
				{
					ResetDevice();
					return;
				}

				if (!_devices.TryGetAt(value, out var item))
				{
					Debug.CommunicationDeviceLogErrorMethod($"{GetType().Name} index \"{nameof(Index)}\" is out of range. Operation ignored.");
					return;
				}

				ChangeDevice(item!, value);
			}
		}

		public DeviceInfo Current
		{
			get => _currentDeviceInfo;
			set
			{
				if (_currentDeviceInfo == value) return;

				if (value.IsEmptyDevice)
				{
					ResetDevice();
					return;
				}

				if (!this.TryFindDevice(value, out var device))
				{
					Debug.CommunicationDeviceLogErrorMethod($"\"{value.Name}\" is not found on {GetType().Name} list. Operation ignored.");
					return;
				}

				var index = _devices.IndexOf(device);
				ChangeDevice(device, index);
			}
		}

		private void ResetDevice()
		{
			Debug.CommunicationDeviceLogMethod($"{GetType().Name} device set to empty.");

			var prevDevice = _currentDeviceInfo;
			var prevIndex  = _currentDeviceIndex;

			_currentDeviceInfo  = DeviceInfo.Empty;
			_currentDeviceIndex = -1;

			DeviceChanged?.Invoke(prevDevice, prevIndex, _currentDeviceInfo, _currentDeviceIndex);
			DeviceFailed?.Invoke(_currentDeviceInfo, _currentDeviceIndex);
		}

		private void ChangeDevice(DeviceInfo device, int index)
		{
			Debug.CommunicationDeviceLogMethod($"Changing {GetType().Name} device to \"{device.Name}\".");
			if (!TryChangeCurrentDevice(device))
			{
				Debug.CommunicationDeviceLogErrorMethod($"Failed to change {GetType().Name} device to \"{device.Name}\". Operation ignored.");
				return;
			}

			var prevDevice = _currentDeviceInfo;
			var prevIndex  = _currentDeviceIndex;

			_currentDeviceInfo  = device;
			_currentDeviceIndex = index;

			DeviceChanged?.Invoke(prevDevice, prevIndex, _currentDeviceInfo, _currentDeviceIndex);

			if (!device.IsAvailable)
				DeviceFailed?.Invoke(_currentDeviceInfo, _currentDeviceIndex);
		}

		protected abstract bool TryChangeCurrentDevice(DeviceInfo device);
#endregion // CurrentDevice

#region DeviceList
		public IEnumerable<DeviceInfo> Devices => _devices;

		public int Count => _devices.Count;

		private readonly List<DeviceInfo> _devices = new();

		private int  _updateInterval = Define.Device.RefreshInterval;
		private bool _useAutoRefresh;

		public bool UseAutoRefresh
		{
			get => _useAutoRefresh;
			set
			{
				var prevValue = _useAutoRefresh;
				if (prevValue == value)
					return;

				_useAutoRefresh = value;
				UpdateAutoRefresh(prevValue, value);
			}
		}

		public int RefreshInterval
		{
			get => _updateInterval;
			set
			{
				var prevValue = _updateInterval;
				if (prevValue == value)
					return;

				_updateInterval = value;
				UpdateRefreshInterval(prevValue, value);
			}
		}

		protected abstract void UpdateAutoRefresh(bool    prevUseAutoRefresh, bool useAutoRefresh);
		protected abstract void UpdateRefreshInterval(int prevIntervalMs,     int  intervalMs);

		public virtual UniTask Refresh(CancellationTokenSource? tokenSource)
		{
			var prevDevice = Current;

			_currentDeviceInfo  = DeviceInfo.Empty;
			_currentDeviceIndex = -1;

			_devices.Clear();
			_devices.AddRange(GetDevices());

			DeviceListChanged?.Invoke(_devices);

			if (Count == 0)
			{
				Debug.CommunicationDeviceLogWarningMethod($"No {GetType().Name} device found.");

				if (!prevDevice.IsEmptyDevice)
					ResetDevice();

				return UniTask.CompletedTask;
			}

			Debug.CommunicationDeviceLogMethod($"{GetType().Name} list updated. Total {Count} device(s) found.\n{string.Join("\n", _devices)}");

			if (!prevDevice.IsEmptyDevice)
				RecoverPreviousDevice(prevDevice);

			return UniTask.CompletedTask;
		}

		protected virtual IEnumerable<DeviceInfo> GetDevices()
		{
			var deviceCount = GetDevicesCount();
			for (var i = 0; i < deviceCount; ++i)
				if (TryGetDeviceAt(i, out var device))
					yield return device;
		}

		protected abstract int GetDevicesCount();

		protected abstract bool TryGetDeviceAt(int index, out DeviceInfo device);

		private void RecoverPreviousDevice(DeviceInfo prevDevice)
		{
			if (this.TryChangeDevice(prevDevice)) return;

			if (!prevDevice.IsEmptyDevice)
			{
				Debug.CommunicationDeviceLogWarningMethod($"Selected device \"{prevDevice.Name}\" removed from {GetType().Name} list. Failed to recover previous device.");
			}

			ResetDevice();
		}
#endregion // DeviceList
	}
}
