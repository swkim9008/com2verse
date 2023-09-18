/*===============================================================
* Product:		Com2Verse
* File Name:	CommunicationDeviceViewModel.cs
* Developer:	urun4m0r1
* Date:			2022-08-19 13:27
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.Communication;
using Com2Verse.Communication.Unity;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("Communication")]
	public sealed class AudioRecorderViewModel : CommunicationDeviceViewModel<IAudioDevice>
	{
		public AudioRecorderViewModel() : base(DeviceManager.Instance.AudioRecorder) { }
	}

	[UsedImplicitly, ViewModelGroup("Communication")]
	public sealed class AudioPlayerViewModel : CommunicationDeviceViewModel<IAudioDevice>
	{
		public AudioPlayerViewModel() : base(DeviceManager.Instance.AudioPlayer) { }
	}

	[UsedImplicitly, ViewModelGroup("Communication")]
	public sealed class VideoRecorderViewModel : CommunicationDeviceViewModel<IDevice>
	{
		public VideoRecorderViewModel() : base(DeviceManager.Instance.VideoRecorder) { }
	}

	public abstract class CommunicationDeviceViewModel<T> : ViewModelBase, IDisposable where T : class, IDevice
	{
		public T Device { get; }

		protected CommunicationDeviceViewModel(T device)
		{
			Device = device;

			Device.DeviceListChanged += OnDeviceListChanged;
			Device.DeviceChanged     += OnDeviceChanged;
			Device.DeviceFailed      += OnDeviceFailed;

			OnDeviceListChanged(Devices);
			NotifyDeviceChanged();
		}

		public void Dispose()
		{
			Device.DeviceListChanged -= OnDeviceListChanged;
			Device.DeviceChanged     -= OnDeviceChanged;
			Device.DeviceFailed      -= OnDeviceFailed;
		}

		private void OnDeviceChanged(DeviceInfo prevDevice, int prevIndex, DeviceInfo device, int index) => NotifyDeviceChanged();
		private void OnDeviceFailed(DeviceInfo  device, int index) => NotifyDeviceChanged();

		private void OnDeviceListChanged(IEnumerable<DeviceInfo>? devices)
		{
			InvokePropertyValueChanged(nameof(IsDeviceExist), IsDeviceExist);
			InvokePropertyValueChanged(nameof(Devices),       Devices);
		}

		private void NotifyDeviceChanged()
		{
			InvokePropertyValueChanged(nameof(IsAvailable),        IsAvailable);
			InvokePropertyValueChanged(nameof(CurrentDevice),      CurrentDevice);
			InvokePropertyValueChanged(nameof(CurrentDeviceIndex), CurrentDeviceIndex);
		}

#region ViewModelProperties
		public DeviceInfo CurrentDevice
		{
			get => Device.Current;
			set => Device.Current = value;
		}

		public int CurrentDeviceIndex
		{
			get => Device.Index;
			set => Device.Index = value;
		}

		public bool IsDeviceExist => Device.Count > 0;

		public IEnumerable<DeviceInfo> Devices => Device.Devices;

		public bool IsAvailable => CurrentDevice.IsAvailable;
#endregion // ViewModelProperties
	}
}
