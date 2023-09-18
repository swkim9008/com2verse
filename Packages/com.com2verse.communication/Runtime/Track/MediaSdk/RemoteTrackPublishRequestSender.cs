/*===============================================================
 * Product:		Com2Verse
 * File Name:	RemoteTrackPublishRequestSender.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-14 22:38
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Com2Verse.Logger;
using Cysharp.Text;

namespace Com2Verse.Communication.MediaSdk
{
	internal sealed class RemoteTrackPublishRequestSender : IDisposable
	{
		public IRemoteTrackPublishRequestHandler RequestHandler { get; }

		private readonly eTrackType                    _trackType;
		private readonly RtcTrackController            _trackController;
		private readonly IPublishRequestableRemoteUser _target;

		public RemoteTrackPublishRequestSender(eTrackType trackType, RtcTrackController trackController, IPublishRequestableRemoteUser target, IRemoteTrackPublishRequestHandler requestHandler)
		{
			_trackType       = trackType;
			_trackController = trackController;
			_target          = target;

			RequestHandler           =  requestHandler;
			RequestHandler.Requested += OnRequested;
		}

		public void Dispose()
		{
			RequestHandler.Requested -= OnRequested;
		}

		private void OnRequested(bool isPublish)
		{
			var result = _trackController.RequestPublishTrack(_target, _trackType, isPublish);
			if (!result)
				LogError($"Failed to request. / IsPublish = {isPublish.ToString()}");
			else
				Log($"IsPublish = {isPublish.ToString()}");
		}

#region Debug
		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		private void Log(string message, [CallerMemberName] string? caller = null)
		{
			C2VDebug.LogMethod(GetLogCategory(), FormatMessage(message), caller);
		}

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		private void LogWarning(string message, [CallerMemberName] string? caller = null)
		{
			C2VDebug.LogWarningMethod(GetLogCategory(), FormatMessage(message), caller);
		}

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		private void LogError(string message, [CallerMemberName] string? caller = null)
		{
			C2VDebug.LogErrorMethod(GetLogCategory(), FormatMessage(message), caller);
		}

		[DebuggerHidden, StackTraceIgnore]
		private string GetLogCategory()
		{
			var className   = GetType().Name;
			var channelInfo = _target.ChannelInfo.GetInfoText();
			var targetInfo  = _target.GetInfoText();

			return ZString.Format(
				"{0} ({1}): {2} / {3}"
			  , className, _trackType, channelInfo, targetInfo);
		}

		[DebuggerHidden, StackTraceIgnore]
		private string FormatMessage(string message)
		{
			var targetInfo  = _target.GetDebugInfo();
			var channelInfo = _target.ChannelInfo.GetDebugInfo();

			return ZString.Format(
				"{0}\n----------\n{1}\n----------\n{2}\n----------\n{3}"
			  , message, GetDebugInfo(), targetInfo, channelInfo);
		}

		[DebuggerHidden, StackTraceIgnore]
		public string GetDebugInfo()
		{
			var requestHandler = RequestHandler.GetType().Name;

			return ZString.Format(
				"[{0}]\n: Type = {1}\n: RequestHandler = {2}"
			  , GetLogCategory(), _trackType, requestHandler);
		}
#endregion // Debug
	}
}
