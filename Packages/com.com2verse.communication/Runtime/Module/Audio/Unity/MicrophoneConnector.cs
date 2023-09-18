/*===============================================================
* Product:		Com2Verse
* File Name:	MicrophoneConnector.cs
* Developer:	urun4m0r1
* Date:			2022-08-24 18:07
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Diagnostics;
using System.Threading;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Sound;
using Com2Verse.Utils;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Com2Verse.Communication.Unity
{
	public sealed class MicrophoneConnector : ConnectionController
	{
		public int PlaybackDelay { get; set; }

		public MetaverseAudioSource AudioSource { get; }

		private DeviceInfo _recordingDeviceInfo;

		private readonly IDevice                _device;
		private readonly IReadOnlyAudioSettings _requestedSettings;

		public MicrophoneConnector(IDevice device, IReadOnlyAudioSettings requestedSettings)
		{
			_device            = device;
			_requestedSettings = requestedSettings;

			var className = GetType().Name;
			var name = ZString.Format(
				"[{0}]"
			  , className);

			var go = new GameObject(name) { hideFlags = HideFlags.DontSave };
			Object.DontDestroyOnLoad(go);

			var audioSource = go.AddComponent<AudioSource>()!;
			AudioSource = MetaverseAudioSource.CreateWithSource(go, audioSource)!;

			audioSource.name = name;
			AudioSource.name = name;

			_recordingDeviceInfo = DeviceInfo.Empty;
		}

		protected override async UniTask<bool> ConnectAsyncImpl(CancellationTokenSource cancellationTokenSource)
		{
			var context = SynchronizationContext.Current;
			if (!await UniTaskHelper.TrySwitchToMainThread(context, cancellationTokenSource))
			{
				LogWarning("Failed to switch to main thread.");
				await ReturnContext(context);
				return false;
			}

			var targetDevice = _device.Current;
			if (!await SyncAndPlayAudioClip(targetDevice, cancellationTokenSource))
			{
				await ReturnContext(context);
				LogWarning("Failed to sync and play audio clip.");
				return false;
			}

			await ReturnContext(context);
			return true;
		}

		protected override async UniTask<bool> DisconnectAsyncImpl(CancellationTokenSource cancellationTokenSource)
		{
			var context = SynchronizationContext.Current;
			if (!await UniTaskHelper.TrySwitchToMainThread(context, cancellationTokenSource))
			{
				LogWarning("Failed to switch to main thread.");
				await ReturnContext(context);
				return false;
			}

			if (!DisposeAudioClip(_recordingDeviceInfo))
				Log("Microphone is already disposed.");

			_recordingDeviceInfo = DeviceInfo.Empty;

			await ReturnContext(context);
			return true;
		}

		private async UniTask<bool> SyncAndPlayAudioClip(DeviceInfo device, CancellationTokenSource cancellationTokenSource)
		{
			if (!device.IsAvailable)
			{
				LogWarning("Current device is not available.");
				return false;
			}

			var length    = MathUtil.Clamp(_requestedSettings.Length,    AudioProperty.SdkMinLimit.Length,    AudioProperty.SdkMaxLimit.Length);
			var frequency = MathUtil.Clamp(_requestedSettings.Frequency, AudioProperty.SdkMinLimit.Frequency, AudioProperty.SdkMaxLimit.Frequency);
			var lengthSec = MathUtil.ToSecondsInt(length);

			var audioClip = MicrophoneProxy.Instance.Start(this, device.Id, true, lengthSec, frequency)!;
			_recordingDeviceInfo = device;

			if (audioClip.IsUnityNull())
			{
				LogError("Failed to start recording.");
				return false;
			}

			AudioSource.Loop = true;
			AudioSource.SetClip(audioClip);

			var positionRange = GetDelayedPositionRange(audioClip);
			while (await UniTaskHelper.DelayFrame(1, cancellationTokenSource))
			{
				var position = GetPosition(device);
				if (position == 0)
					continue;

				if (position < positionRange.x || position > positionRange.y)
					continue;

				AudioSource.Play();

				if (!await UniTaskHelper.WaitUntil(() => AudioSource.IsPlaying, cancellationTokenSource))
				{
					LogWarning("Failed to wait until audio source is playing.");
					return false;
				}

				Log($"Microphone started recording at {position.ToString()}");
				return true;
			}

			LogWarning("Failed to wait until microphone is ready.");
			return false;
		}

		public static int GetPosition(DeviceInfo device) => Microphone.GetPosition(device.Id);

		private Vector2Int GetDelayedPositionRange(AudioClip audioClip)
		{
			var tolerance   = Define.Audio.DelayTolerance;
			var minDelaySec = MathUtil.ToSeconds(PlaybackDelay);
			var maxDelaySec = MathUtil.ToSeconds(PlaybackDelay + tolerance);

			var samplesPerSec   = audioClip.frequency;
			var minDelaySamples = Mathf.RoundToInt(minDelaySec * samplesPerSec);
			var maxDelaySamples = Mathf.RoundToInt(maxDelaySec * samplesPerSec);

			var maxSamples  = audioClip.samples - 1;
			var minPosition = Math.Clamp(minDelaySamples, 0, maxSamples);
			var maxPosition = Math.Clamp(maxDelaySamples, 0, maxSamples);

			return new Vector2Int(minPosition, maxPosition);
		}

		private bool DisposeAudioClip(DeviceInfo device)
		{
			if (device.IsEmptyDevice)
				return false;

			if (AudioSource.IsPlaying)
				AudioSource.Stop();

			AudioSource.SetClip(null!);
			MicrophoneProxy.InstanceOrNull?.End(this, device.Id);

			return true;
		}

		private static async UniTask ReturnContext(SynchronizationContext? context)
		{
			await UniTaskHelper.TrySwitchToSynchronizationContext(context);
		}

#region IDisposable
		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				AudioSource.DestroyGameObject();
			}

			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable

#region Debug
		[DebuggerHidden, StackTraceIgnore]
		public override string GetDebugInfo()
		{
			var baseInfo            = base.GetDebugInfo();
			var selectedDeviceName  = _device.Current.ToString();
			var recordingDeviceName = _recordingDeviceInfo.ToString();
			var settingsName        = _requestedSettings.ToString();
			var audioSourcePath = AudioSource.IsUnityNull()
				? "null"
				: AudioSource.transform.GetFullPathInHierachy();

			return ZString.Format(
				"{0}\n: Selected = \"{1}\"\n: Recording = \"{2}\"\n: Settings = \"{3}\"\n: PlaybackDelay = {4}\n: AudioSourcePath = \"{5}\""
			  , baseInfo, selectedDeviceName, recordingDeviceName, settingsName, PlaybackDelay, audioSourcePath);
		}
#endregion // Debug
	}
}
