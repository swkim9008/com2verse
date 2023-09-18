/*===============================================================
* Product:		Com2Verse
* File Name:	ChannelConnector.cs
* Developer:	urun4m0r1
* Date:			2023-03-13 16:26
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
using MediaSdkUser = Com2Verse.Solution.UnityRTCSdk.User;

namespace Com2Verse.Communication.MediaSdk
{
	internal sealed class ChannelConnector : ConnectionController
	{
		private enum eConnectionSignal
		{
			IDLE = 0,
			INITIALIZE_ERROR,
			DISCONNECT_REQUEST,
			JOIN_FAILED,
			LEAVE_FAILED,
			JOIN_SUCCESS,
			LEAVE_SUCCESS,
		}

		private eConnectionSignal _connectionSignal;

		public ChannelInfo               ChannelInfo     { get; }
		public RtcChannelAdapter         ChannelAdapter  { get; }
		public RtcChannelEventDispatcher EventDispatcher { get; }

		public ChannelConnector(ChannelInfo channelInfo, RtcChannelAdapter channelAdapter, RtcChannelEventDispatcher eventDispatcher)
		{
			ChannelInfo     = channelInfo;
			ChannelAdapter  = channelAdapter;
			EventDispatcher = eventDispatcher;

			RegisterChannelCallbacks();
		}

		private void RegisterChannelCallbacks()
		{
			UnRegisterChannelCallbacks();

			EventDispatcher.InitializeError += OnInitializeError;
			EventDispatcher.DisconnectRequest    += OnDisconnectRequest;
			EventDispatcher.JoinFailed      += OnJoinFailed;
			EventDispatcher.LeaveFailed     += OnLeaveFailed;
			EventDispatcher.JoinSuccess     += OnJoinSuccess;
			EventDispatcher.LeaveSuccess    += OnLeaveSuccess;
		}

		private void UnRegisterChannelCallbacks()
		{
			EventDispatcher.InitializeError -= OnInitializeError;
			EventDispatcher.DisconnectRequest    -= OnDisconnectRequest;
			EventDispatcher.JoinFailed      -= OnJoinFailed;
			EventDispatcher.LeaveFailed     -= OnLeaveFailed;
			EventDispatcher.JoinSuccess     -= OnJoinSuccess;
			EventDispatcher.LeaveSuccess    -= OnLeaveSuccess;
		}

		private void OnInitializeError(string channelId, string reason)
		{
			_connectionSignal = eConnectionSignal.INITIALIZE_ERROR;
			LogError(reason);
		}

		private void OnDisconnectRequest(string channelId, string reason)
		{
			_connectionSignal = eConnectionSignal.DISCONNECT_REQUEST;
			LogWarning(reason);
		}

		private void OnJoinFailed(string channelId, string reason)
		{
			_connectionSignal = eConnectionSignal.JOIN_FAILED;
			LogError(reason);
		}

		private void OnLeaveFailed(string channelId, string reason)
		{
			_connectionSignal = eConnectionSignal.LEAVE_FAILED;
			LogError(reason);
		}

		private void OnJoinSuccess(string channelId)
		{
			_connectionSignal = eConnectionSignal.JOIN_SUCCESS;
		}

		private void OnLeaveSuccess(string channelId)
		{
			_connectionSignal = eConnectionSignal.LEAVE_SUCCESS;
		}

		private async UniTask<bool> WaitForSignalChanged(CancellationTokenSource cancellationTokenSource)
		{
			return await UniTaskHelper.WaitUntil(() => _connectionSignal is not eConnectionSignal.IDLE, cancellationTokenSource);
		}

		protected override async UniTask<bool> ConnectAsyncImpl(CancellationTokenSource cancellationTokenSource)
		{
#if DISABLE_WEBRTC
			LogWarning("WebRTC is disabled. Skip connecting to channel.");
			return true;
#endif // DISABLE_WEBRTC

			_connectionSignal = eConnectionSignal.IDLE;

			ChannelAdapter.Initialize();
			ChannelAdapter.Join();

			if (!await WaitForSignalChanged(cancellationTokenSource))
			{
				LogWarning("Failed to wait for signal changed.");
				return false;
			}

			if (_connectionSignal is not eConnectionSignal.JOIN_SUCCESS)
			{
				LogError("Connection failed.");
				return false;
			}

			return true;
		}


		protected override async UniTask<bool> DisconnectAsyncImpl(CancellationTokenSource cancellationTokenSource)
		{
#if DISABLE_WEBRTC
			LogWarning("WebRTC is disabled. Skip disconnecting from channel.");
			return true;
#endif // DISABLE_WEBRTC

			_connectionSignal = eConnectionSignal.IDLE;

			ChannelAdapter.Leave();

			if (!await WaitForSignalChanged(cancellationTokenSource))
			{
				LogWarning("Failed to wait for signal changed.");
				return false;
			}

			if (_connectionSignal is not (eConnectionSignal.LEAVE_SUCCESS))
			{
				LogError("Disconnection failed.");
				return false;
			}

			ChannelAdapter.DeInitialize();
			return true;
		}

		private void DisposeChannel()
		{
#if DISABLE_WEBRTC
			return;
#endif // DISABLE_WEBRTC

			ChannelAdapter.Leave();
			ChannelAdapter.DeInitialize();

			UnRegisterChannelCallbacks();
		}

#region IAsyncDisposable
		private bool _asyncDisposed;

		protected override async UniTask DisposeAsyncCore()
		{
			if (_asyncDisposed)
				return;

			// Uncomment this line in inherited class to implement standard disposing pattern.
			await base.DisposeAsyncCore();

			DisposeChannel();

			_asyncDisposed = true;
		}
#endregion // IAsyncDisposable

#region Debug
		[DebuggerHidden, StackTraceIgnore]
		protected override string GetLogCategory()
		{
			var className   = GetType().Name;
			var channelInfo = ChannelInfo.GetInfoText();

			return ZString.Format(
				"{0}: {1}"
			  , className, channelInfo);
		}

		[DebuggerHidden, StackTraceIgnore]
		protected override string FormatMessage(string message)
		{
			var baseMessage = base.FormatMessage(message);
			var channelInfo = ChannelInfo.GetDebugInfo();

			return ZString.Format(
				"{0}\n----------\n{1}"
			  , baseMessage, channelInfo);
		}

		[DebuggerHidden, StackTraceIgnore]
		public override string GetDebugInfo()
		{
			var baseInfo = base.GetDebugInfo();

			return ZString.Format(
				"{0}\n: ConnectionSignal = {1}"
			  , baseInfo, _connectionSignal);
		}
#endregion // Debug
	}
}
