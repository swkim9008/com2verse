/*===============================================================
* Product:		Com2Verse
* File Name:	AudioPlayerDevice.cs
* Developer:	urun4m0r1
* Date:			2022-08-04 16:40
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

//#define AUDIO_OUTPUT_WITH_FMOD

#nullable enable

using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Communication.Unity
{

#if AUDIO_OUTPUT_WITH_FMOD
	public sealed class AudioPlayerDevice : BaseAudioDevice, IDisposable
	{
		private MetaverseAudioSourceOutputDevice _audioSourceOutputDevice = null;

		private LogLevel _logLevel = LogLevel.ERROR;

		public override bool UseAutoRefresh => true;

		public override SystemVolume SystemVolume { get; } = new NullSystemVolume();

		public void Dispose()
		{
			UseAutoRefresh = false;
			Release();
		}

		private void Release()
		{
			if (_audioSourceOutputDevice.IsUnityNull())
				GameObject.Destroy(_audioSourceOutputDevice.gameObject);
		}


		protected override bool TryChangeCurrentDevice(DeviceInfo device)
		{
			if (_audioSourceOutputDevice.IsUnityNull())
			{
				_audioSourceOutputDevice = new GameObject("MetaverseAudioSourceOutputDevice").AddComponent<MetaverseAudioSourceOutputDevice>();
				GameObject.DontDestroyOnLoad(_audioSourceOutputDevice.gameObject);
			}

			return _audioSourceOutputDevice.StartFMODSound(device.Name, true);
		}


		protected override IEnumerable<DeviceInfo> GetDevices()
		{
			var outputDevices = FMODSystemsManager.AvailableOutputs(this._logLevel, null, null);
			foreach (var outputDevice in outputDevices)
			{
				var deviceInfo = new DeviceInfo(outputDevice.guid.ToString(), outputDevice.name, outputDevice.id == 0);
				if (deviceInfo.IsSystemDefault)
					SystemDefault = new DeviceInfo(deviceInfo.Id, $"System Default ({deviceInfo.Name})", true);
				yield return deviceInfo;
			}
		}

		protected override int GetDevicesCount() => 0;

		protected override bool TryGetDeviceAt(int index, out DeviceInfo device)
		{
			device = DeviceInfo.Empty;
			return false;
		}

		protected override bool TryGetDefaultDevice(out DeviceInfo device)
		{
			var outputDevices = FMODSystemsManager.AvailableOutputs(this._logLevel, null, null);
			if (outputDevices.Count > 0)
			{
				device = new DeviceInfo(outputDevices[0].guid.ToString(), outputDevices[0].name);
				return true;
			}

			device = DeviceInfo.Empty;
			return false;
		}
	}

#else
	/// <summary>
	/// Unity 기본 API는 운영체제 오디오 장치 변경을 지원하지 않습니다.
	/// </summary>
	public sealed class AudioPlayerDevice : BaseAudioDevice, IDisposable
	{
		/// <inheritdoc cref="NullSystemVolume"/>
		public override SystemVolume SystemVolume { get; } = new NullSystemVolume();

		public override UniTask Refresh(CancellationTokenSource? tokenSource)
		{
			// Do nothing
			return UniTask.CompletedTask;
		}

		protected override bool TryChangeCurrentDevice(DeviceInfo device)
		{
			return false;
		}

		protected override int GetDevicesCount()
		{
			return 0;
		}

		protected override bool TryGetDeviceAt(int index, out DeviceInfo device)
		{
			device = DeviceInfo.Empty;
			return false;
		}

		protected override bool TryGetDefaultDevice(out DeviceInfo device)
		{
			device = DeviceInfo.Empty;
			return false;
		}

		public void Dispose()
		{
			SystemVolume.Dispose();
		}
	}

#endif //AUDIO_OUTPUT_WITH_FMOD
}
