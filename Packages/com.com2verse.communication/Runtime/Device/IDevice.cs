/*===============================================================
* Product:		Com2Verse
* File Name:	IDevice.cs
* Developer:	urun4m0r1
* Date:			2022-04-04 10:31
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Com2Verse.Communication
{
	/// <summary>
	/// Interface for media device such as webcam, audio, etc.
	/// </summary>
	public interface IDevice : IRefreshable
	{
		/// <summary>
		/// Get all available devices list.
		/// </summary>
		IEnumerable<DeviceInfo> Devices { get; }

		/// <summary>
		/// Get or set current controlling device.
		/// </summary>
		DeviceInfo Current { get; set; }

		/// <summary>
		/// Get current device's index from <see cref="Devices"/> or set current device by index.
		/// </summary>
		int Index { get; set; }

		/// <summary>
		/// Get <see cref="Devices"/> count.
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Invoked when current device changed.
		/// </summary>
		event Action<DeviceInfo, int, DeviceInfo, int>? DeviceChanged;

		/// <summary>
		/// Invoked when device become unavailable.
		/// </summary>
		event Action<DeviceInfo, int>? DeviceFailed;

		void RaiseDeviceFailedEvent();

		/// <summary>
		/// Invoked when device list changed.
		/// </summary>
		event Action<IEnumerable<DeviceInfo>>? DeviceListChanged;
	}

	public static class DeviceExtensions
	{
		public static bool TryChangeDevice(this IDevice device, int index)
		{
			if (index < 0 || index >= device.Count)
				return false;

			device.Index = index;
			return true;
		}

		public static bool TryChangeDevice(this IDevice device, DeviceInfo target)
		{
			if (target.IsEmptyDevice) return false;

			if (!device.TryFindDevice(target, out var result)) return false;

			device.Current = result;
			return true;
		}

		public static bool TryFindDevice(this IDevice device, DeviceInfo target, out DeviceInfo result)
		{
			result = device.Devices.FirstOrDefault(x => x.IsSameDevice(target)) ?? DeviceInfo.Empty;
			return !result.IsEmptyDevice;
		}

		public static void OnDeviceUnavailable(this IDevice device)
		{
			device.Current.IsAvailable = false;

			if (!device.TryAssignAnyAvailableDevice())
				device.RaiseDeviceFailedEvent();
		}

		public static bool TryAssignAnyAvailableDevice(this IDevice device)
		{
			var availableDevice = device.Devices.FirstOrDefault(static x => x.IsAvailable);
			if (availableDevice != null)
			{
				Debug.CommunicationDeviceLogWarningMethod($"Current {device.GetType().Name} device is unavailable. Switching to \"{availableDevice.Name}\"");
				device.Current = availableDevice;
				return true;
			}

			Debug.CommunicationDeviceLogErrorMethod($"All {device.GetType().Name} devices are unavailable. Please check your device connection.");
			return false;
		}
	}
}
