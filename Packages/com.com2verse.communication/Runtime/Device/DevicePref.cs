/*===============================================================
* Product:		Com2Verse
* File Name:	DevicePref.cs
* Developer:	urun4m0r1
* Date:			2022-06-16 11:26
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.Communication
{
	public static class DevicePref
	{
		private static readonly string Header = $"{nameof(Communication)}.{nameof(DevicePref)}";

		private static string GetPrefKey(IDevice device) => $"{Header}.{device.GetType().Name}";

		public static async UniTask SaveCurrentDeviceInfo(IDevice device)
		{
			await UniTaskHelper.InvokeOnMainThread(() => Save(device));
		}

		public static async UniTask LoadSavedDeviceInfoAsync(IDevice device)
		{
			await UniTaskHelper.InvokeOnMainThread(() => device.TryChangeDevice(Load(device)));
		}

		private static void Save(IDevice device)
		{
			var key    = GetPrefKey(device);
			var target = device.Current;

			PlayerPrefs.SetString(key, Convert(target));
			Debug.CommunicationDeviceLogMethod($"Preference saved: {{ {key}: \"{target.Name}\" }}");
		}

		private static DeviceInfo Load(IDevice device)
		{
			var key    = GetPrefKey(device);
			var value  = PlayerPrefs.GetString(key);
			var result = Convert(value);

			if (result != null)
				Debug.CommunicationDeviceLogMethod($"Preference loaded: {{ {key}: \"{result.Name}\" }}");
			else
				Debug.CommunicationDeviceLogWarningMethod($"Failed to load preference: {{ {key} }}");

			return result ?? DeviceInfo.Empty;
		}

		private static string Convert(DeviceInfo device) =>
			JsonUtility.ToJson(device) ?? string.Empty;

		private static DeviceInfo? Convert(string? data)
		{
			if (string.IsNullOrWhiteSpace(data!)) return null;

			try
			{
				return JsonUtility.FromJson<DeviceInfo>(data);
			}
			catch (Exception e)
			{
				Debug.CommunicationDeviceLogErrorMethod($"Failed to convert {data} to DeviceInfo: {e.Message}");
				return null;
			}
		}
	}
}
