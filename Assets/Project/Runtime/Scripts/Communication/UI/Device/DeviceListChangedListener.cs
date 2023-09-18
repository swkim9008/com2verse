/*===============================================================
* Product:		Com2Verse
* File Name:	DeviceListChangedListener.cs
* Developer:	urun4m0r1
* Date:			2022-08-22 13:40
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using Com2Verse.Communication;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace Com2Verse.Project.Communication.UI
{
	/// <summary>
	/// 디바이스 목록을 ViewModel 또는 코드 레벨에서 제어해, 목록이 갱신되는 경우 UnityEvent 를 발생시키는 클래스.<br/>
	/// 드롭다운 표시 등의 UI 제어에 사용할 수 있습니다.
	/// </summary>
	[AddComponentMenu("[Communication]/[Communication] Device List Changed Listener")]
	public sealed class DeviceListChangedListener : MonoBehaviour
	{
#region InspectorFields
		[Header("Events")]
		[SerializeField] private UnityEvent<List<string>, int>? _deviceListChanged;
#endregion // InspectorFields

		private IDevice? _device;

		private readonly List<string> _deviceNames = new();

		private void OnDestroy()
		{
			Clear();
		}

#region ViewModelProperties
		[UsedImplicitly] // Setter used by view model.
		public IDevice? Device
		{
			get => _device;
			set
			{
				var prevValue = _device;
				if (prevValue == value) return;

				if (prevValue != null) prevValue.DeviceListChanged -= OnDeviceListChanged;
				if (value     != null) value.DeviceListChanged     += OnDeviceListChanged;

				_device = value;

				OnDeviceListChanged(_device?.Devices);
			}
		}

		/// <summary>
		/// 상태를 초기화하고 이벤트를 발생시킵니다.
		/// </summary>
		public void Clear()
		{
			Device = null;
		}
#endregion // ViewModelProperties

		private void OnDeviceListChanged(IEnumerable<DeviceInfo>? devices)
		{
			UpdateDeviceNames(devices);
			InvokeListChangedEvent(_deviceNames, Device?.Index ?? -1);
		}

		private void UpdateDeviceNames(IEnumerable<DeviceInfo>? devices)
		{
			_deviceNames.Clear();

			if (devices != null)
				foreach (var device in devices)
					_deviceNames.Add(device.Name);
		}

		private void InvokeListChangedEvent(List<string> deviceNames, int index)
		{
			_deviceListChanged?.Invoke(deviceNames, index);
		}
	}
}
