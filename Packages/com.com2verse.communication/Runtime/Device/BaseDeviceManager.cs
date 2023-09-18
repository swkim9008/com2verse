/*===============================================================
* Product:		Com2Verse
* File Name:	BaseDeviceManager.cs
* Developer:	urun4m0r1
* Date:			2022-08-05 16:07
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Threading;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Com2Verse.Communication
{
	public abstract class BaseDeviceManager<T, TAudioRecorder, TAudioPlayer, TVideoRecorder> : Singleton<T>
		where T : class
		where TAudioRecorder : class, IAudioDevice
		where TAudioPlayer : class, IAudioDevice
		where TVideoRecorder : class, IDevice
	{
		public abstract TAudioRecorder AudioRecorder { get; }
		public abstract TAudioPlayer   AudioPlayer   { get; }
		public abstract TVideoRecorder VideoRecorder { get; }

		public bool IsInitializing { get; private set; }
		public bool IsInitialized  { get; private set; }

		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly]
		protected BaseDeviceManager() { }

		/// <summary>
		/// 해당 메서드를 BaseDeviceManager 생성자에서 호출하면 virtual 맴버가 초기화되기 전에 호출되므로 NullReferenceException이 발생한다.
		/// <br/>따라서 상속받은 클래스의 생성자에서 해당 메서드를 호출해야 한다.
		/// </summary>
		protected void RegisterDeviceSaveEvent()
		{
			AudioRecorder.DeviceChanged += OnAudioRecorderDeviceChanged;
			AudioPlayer.DeviceChanged   += OnAudioPlayerDeviceChanged;
			VideoRecorder.DeviceChanged += OnVideoRecorderDeviceChanged;
		}

		private void OnAudioRecorderDeviceChanged(DeviceInfo prevDevice, int prevIndex, DeviceInfo deviceInfo, int deviceIndex) => DevicePref.SaveCurrentDeviceInfo(AudioRecorder).Forget();
		private void OnAudioPlayerDeviceChanged(DeviceInfo   prevDevice, int prevIndex, DeviceInfo deviceInfo, int deviceIndex) => DevicePref.SaveCurrentDeviceInfo(AudioPlayer).Forget();
		private void OnVideoRecorderDeviceChanged(DeviceInfo prevDevice, int prevIndex, DeviceInfo deviceInfo, int deviceIndex) => DevicePref.SaveCurrentDeviceInfo(VideoRecorder).Forget();

		public async UniTask TryInitializeAsync()
		{
			if (IsInitialized || IsInitializing)
				return;

			IsInitializing = true;
			{
				await RefreshAllDeviceAsync(CoroutineManager.Instance.GlobalCancellationTokenSource);
				await LoadAllSavedDeviceInfoAsync();
			}
			IsInitializing = false;
			IsInitialized  = true;
		}

		private async UniTask RefreshAllDeviceAsync(CancellationTokenSource? tokenSource)
		{
			await AudioRecorder.Refresh(tokenSource);
			await AudioPlayer.Refresh(tokenSource);
			await VideoRecorder.Refresh(tokenSource);
		}

		private async UniTask LoadAllSavedDeviceInfoAsync()
		{
			await DevicePref.LoadSavedDeviceInfoAsync(AudioRecorder);
			await DevicePref.LoadSavedDeviceInfoAsync(AudioPlayer);
			await DevicePref.LoadSavedDeviceInfoAsync(VideoRecorder);
		}

		private async UniTask SaveAllCurrentDeviceInfo()
		{
			await DevicePref.SaveCurrentDeviceInfo(AudioRecorder);
			await DevicePref.SaveCurrentDeviceInfo(AudioPlayer);
			await DevicePref.SaveCurrentDeviceInfo(VideoRecorder);
		}
	}
}
