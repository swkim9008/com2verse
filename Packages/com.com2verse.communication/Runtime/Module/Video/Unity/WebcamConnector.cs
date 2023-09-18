/*===============================================================
* Product:		Com2Verse
* File Name:	WebcamConnector.cs
* Developer:	urun4m0r1
* Date:			2022-08-24 18:07
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Diagnostics;
using System.Threading;
using Com2Verse.Logger;
using Com2Verse.Utils;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.Communication.Unity
{
	public sealed class WebcamConnector : ConnectionController
	{
		public WebCamTexture WebcamTexture { get; }

		private DeviceInfo _recordingDeviceInfo;

		private readonly IDevice                _device;
		private readonly IReadOnlyVideoSettings _requestedSettings;

		private readonly Application.LogCallback _onLogMessageReceivedCache;

		public WebcamConnector(IDevice device, IReadOnlyVideoSettings requestedSettings)
		{
			_onLogMessageReceivedCache = OnLogMessageReceived;

			_device            = device;
			_requestedSettings = requestedSettings;

			WebcamTexture = new WebCamTexture();

			_recordingDeviceInfo = device.Current;
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
			if (!await PlayWebcamTextureAsync(targetDevice, cancellationTokenSource))
			{
				await ReturnContext(context);
				LogWarning("Failed to play WebcamTexture.");
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

			if (!StopWebcamTexture(_recordingDeviceInfo))
				Log("WebcamTexture is already stopped.");

			_recordingDeviceInfo = DeviceInfo.Empty;

			await ReturnContext(context);
			return true;
		}

		private async UniTask<bool> PlayWebcamTextureAsync(DeviceInfo device, CancellationTokenSource cancellationTokenSource)
		{
			if (!device.IsAvailable)
			{
				LogWarning("Current device is not available.");
				return false;
			}

			WebcamTexture.deviceName      = device.Id;
			WebcamTexture.requestedWidth  = MathUtil.Clamp(_requestedSettings.Width,  VideoProperty.SdkMinLimit.Width,  VideoProperty.SdkMaxLimit.Width);
			WebcamTexture.requestedHeight = MathUtil.Clamp(_requestedSettings.Height, VideoProperty.SdkMinLimit.Height, VideoProperty.SdkMaxLimit.Height);
			WebcamTexture.requestedFPS    = _requestedSettings.Fps;

			PlayWebcamTexture(true);
			_recordingDeviceInfo = device;

			while (await UniTaskHelper.DelayFrame(1, cancellationTokenSource, PlayerLoopTiming.PostLateUpdate))
			{
				if (!WebcamTexture.didUpdateThisFrame)
					continue;

				Log("WebcamTexture started recording.");
				return true;
			}

			LogWarning("Failed to start recording.");
			return false;
		}

		private bool StopWebcamTexture(DeviceInfo device)
		{
			if (device.IsEmptyDevice)
				return false;

			if (WebcamTexture.isPlaying)
				WebcamTexture.Stop();

			WebcamTexture.deviceName = null;

			return true;
		}

		private static async UniTask ReturnContext(SynchronizationContext? context)
		{
			await UniTaskHelper.TrySwitchToSynchronizationContext(context);
		}

#region ErrorHandling
		private void PlayWebcamTexture(bool willCatchDeviceFailedEvent)
		{
			if (willCatchDeviceFailedEvent)
			{
				Application.logMessageReceived -= _onLogMessageReceivedCache;
				Application.logMessageReceived += _onLogMessageReceivedCache;
			}

			WebcamTexture.Play();
		}

		private static readonly string VideoDeviceNotFound = "Could not find specified video device";

		private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
		{
			if (type == LogType.Error && logString.Contains(VideoDeviceNotFound))
			{
				Application.logMessageReceived -= _onLogMessageReceivedCache;
				_device.OnDeviceUnavailable();
			}
		}
#endregion // ErrorHandling

#region Debug
		[DebuggerHidden, StackTraceIgnore]
		public override string GetDebugInfo()
		{
			var baseInfo            = base.GetDebugInfo();
			var selectedDeviceName  = _device.Current.ToString();
			var recordingDeviceName = _recordingDeviceInfo.ToString();
			var settingsName        = _requestedSettings.ToString();

			return ZString.Format(
				"{0}\n: Selected = \"{1}\"\n: Recording = \"{2}\"\n: Settings = \"{3}\""
			  , baseInfo, selectedDeviceName, recordingDeviceName, settingsName);
		}
#endregion // Debug
	}
}
