/*===============================================================
 * Product:		Com2Verse
 * File Name:	LocalMediaTrackConnector.cs
 * Developer:	urun4m0r1
 * Date:		2023-03-10 16:36
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Diagnostics;
using System.Threading;
using Com2Verse.Logger;
using Com2Verse.Solution.UnityRTCSdk;
using Com2Verse.Utils;
using Cysharp.Text;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Communication.MediaSdk
{
	internal sealed class LocalMediaTrackConnector : MediaTrackConnector
	{
		private LocalTrack? _payload;

		private readonly RtcTrackController      _trackController;
		private readonly IPublishableLocalUser   _publisher;
		private readonly IPublishableRemoteUser? _target;

		public LocalMediaTrackConnector(eTrackType trackType, RtcTrackController trackController, IPublishableLocalUser publisher, IPublishableRemoteUser? target = null)
			: base(publisher, trackType)
		{
			_trackController = trackController;
			_publisher       = publisher;
			_target          = target;

			var audioPublishSettings = _publisher.GetAudioPublishSettings(Type);
			if (audioPublishSettings != null)
				audioPublishSettings.SettingsChanged += OnAudioPublishSettingsChanged;

			var videoPublishSettings = _publisher.GetVideoPublishSettings(Type);
			if (videoPublishSettings != null)
				videoPublishSettings.SettingsChanged += OnVideoPublishSettingsChanged;

			if (Type is eTrackType.CAMERA)
			{
				_trackController.VideoQualityChanged += OnCameraQualityChanged;

				if (_trackController.ChannelAdapter.GetVideoQuality(out var videoQuality))
					OnCameraQualityChanged(videoQuality);
			}
		}

		private void OnCameraQualityChanged(VideoQuality videoQuality)
		{
			_publisher.GetVideoPublishSettings(Type)?.ChangeSettings((int)videoQuality.Fps, (int)videoQuality.Bitrate, (float)videoQuality.Scale);
		}

		private void OnAudioPublishSettingsChanged(IReadOnlyAudioPublishSettings settings)
		{
			_trackController.ChangeAudioQuality(_payload as LocalAudioTrack, settings.Convert());
		}

		private void OnVideoPublishSettingsChanged(IReadOnlyVideoPublishSettings settings)
		{
			_trackController.ChangeVideoQuality(_payload as LocalVideoTrack, settings.Convert());
		}

		protected override async UniTask<bool> ConnectAsyncImpl(CancellationTokenSource cancellationTokenSource)
		{
			if (!await UniTaskHelper.Delay(Define.PublishDelay, cancellationTokenSource))
			{
				LogWarning("Failed to wait for publish delay.");
				return false;
			}

			var context = SynchronizationContext.Current;
			if (!await UniTaskHelper.TrySwitchToMainThread(context, cancellationTokenSource))
			{
				LogWarning("Failed to switch to main thread.");
				await ReturnContext(context);
				return false;
			}

			if (!_trackController.TryPublishTrack(Type, _publisher, _target, out _payload))
			{
				LogWarning("Failed to publish track.");
				await ReturnContext(context);
				return false;
			}

			await ReturnContext(context);
			return _payload != null;
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

			if (!_trackController.UnpublishTrack(_payload))
				Log("Track is already unpublished.");

			_payload = null;

			await ReturnContext(context);
			return true;
		}

		private static async UniTask ReturnContext(SynchronizationContext? context)
		{
			await UniTaskHelper.TrySwitchToSynchronizationContext(context);
		}

#region Debug
		[DebuggerHidden, StackTraceIgnore]
		protected override string GetLogCategory()
		{
			var baseCategory = base.GetLogCategory();
			var channelInfo  = _trackController.ChannelInfo.GetInfoText();
			var targetInfo   = _target?.GetInfoText() ?? "null";

			var format = _target != null
				? "{0}: {1} / {2}"
				: "{0}: {1}";

			return ZString.Format(
				format
			  , baseCategory, channelInfo, targetInfo);
		}

		[DebuggerHidden, StackTraceIgnore]
		protected override string FormatMessage(string message)
		{
			var baseMessage    = base.FormatMessage(message);
			var channelDetails = _trackController.ChannelInfo.GetDebugInfo();

			return ZString.Format(
				"{0}\n----------\n{1}"
			  , baseMessage, channelDetails);
		}

		[DebuggerHidden, StackTraceIgnore]
		public override string GetDebugInfo()
		{
			var baseInfo    = base.GetDebugInfo();
			var targetInfo  = _target?.GetInfoText()  ?? "null";
			var payloadInfo = _payload?.GetInfoText() ?? "null";

			return ZString.Format(
				"{0}\n: Target = {1}\n: Payload = \"{2}\""
			  , baseInfo, targetInfo, payloadInfo);
		}
#endregion // Debug

#region IDisposable
		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				if (Type is eTrackType.CAMERA) _trackController.VideoQualityChanged -= OnCameraQualityChanged;

				var audioPublishSettings = _publisher.GetAudioPublishSettings(Type);
				if (audioPublishSettings != null)
					audioPublishSettings.SettingsChanged -= OnAudioPublishSettingsChanged;

				var videoPublishSettings = _publisher.GetVideoPublishSettings(Type);
				if (videoPublishSettings != null)
					videoPublishSettings.SettingsChanged -= OnVideoPublishSettingsChanged;
			}

			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
