/*===============================================================
 * Product:		Com2Verse
 * File Name:	ConnectionController.cs
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
using System.Threading;
using System.Threading.Tasks;
using Com2Verse.Logger;
using Com2Verse.Utils;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using static Com2Verse.Communication.eConnectionState;

namespace Com2Verse.Communication
{
	public abstract class ConnectionController : IConnectionController, IAsyncDisposable, IDisposable
	{
		public event Action<IConnectionController, eConnectionState>? StateChanged;

		private eConnectionState _state = DISCONNECTED;

		public eConnectionState State
		{
			get => _state;
			private set
			{
				if (_state == value)
					return;

				_state = value;
				StateChanged?.Invoke(this, _state);
			}
		}

		private CancellationTokenSource? _connectionToken;

		private readonly CancellationTokenSource _classToken = new();

		public async UniTask<bool> ConnectAsync()
		{
			if (State is CONNECTED)
			{
				LogWarning("Already connected. Ignore connect request.");
				return true;
			}

			if (State is CONNECTING)
			{
				LogWarning("Already connecting. Ignore connect request.");
				return await WaitForConnected();
			}

			if (State is DISCONNECTING)
			{
				Log("Waiting for previous session to disconnect.");
				if (!await WaitForDisconnected())
					return false;
			}

			State = CONNECTING;
			Log("Start connecting...");
			{
				_connectionToken ??= new CancellationTokenSource();
				if (!await ConnectAsync(_connectionToken))
				{
					State = DISCONNECTED;
					return false;
				}
			}
			State = CONNECTED;
			Log("Successfully connected.");

			return true;
		}

		/// <summary>
		/// <see cref="ConnectAsyncImpl"/>의 결과를 반환합니다.<br/>
		/// 실행 실패시, 종료 시점의 <see cref="CancellationTokenSource.IsCancellationRequested"/> 여부에 따라 로그를 출력합니다.
		/// </summary>
		private async UniTask<bool> ConnectAsync(CancellationTokenSource cancellationTokenSource)
		{
			if (await ConnectAsyncImpl(cancellationTokenSource))
				return true;

			if (cancellationTokenSource.IsCancellationRequested)
			{
				LogWarning("Connection cancelled.");
				return false;
			}

			LogError("Failed to connect.");
			return false;
		}

		public async UniTask<bool> DisconnectAsync()
		{
			if (State is DISCONNECTED)
			{
				LogWarning("Already disconnected. Ignore disconnect request.");
				return true;
			}

			if (State is DISCONNECTING)
			{
				LogWarning("Already disconnecting. Ignore disconnect request.");
				return await WaitForDisconnected();
			}

			if (State is CONNECTING)
			{
				Log("Previous session is connecting. Cancel connection.");
				DisposeConnectionToken();
			}

			State = DISCONNECTING;
			Log("Start disconnecting...");
			{
				if (!await DisconnectAsyncImpl(_classToken))
				{
					LogError("Failed to disconnect.");
					State = DISCONNECTED;
					return false;
				}
			}
			State = DISCONNECTED;
			Log("Successfully disconnected.");

			return true;
		}

		protected abstract UniTask<bool> ConnectAsyncImpl(CancellationTokenSource    cancellationTokenSource);
		protected abstract UniTask<bool> DisconnectAsyncImpl(CancellationTokenSource cancellationTokenSource);

		private async UniTask<bool> WaitForConnected()
		{
			if (!await UniTaskHelper.WaitUntil(() => State is CONNECTED, _connectionToken))
			{
				LogWarning("Failed to wait for connected.");
				return false;
			}

			return true;
		}

		private async UniTask<bool> WaitForDisconnected()
		{
			if (!await UniTaskHelper.WaitUntil(() => State is DISCONNECTED, _classToken))
			{
				LogWarning("Failed to wait for disconnected.");
				return false;
			}

			return true;
		}

		private void DisposeConnectionToken()
		{
			if (_connectionToken == null || _connectionToken.IsCancellationRequested)
				return;

			_connectionToken.Cancel();
			_connectionToken.Dispose();
			_connectionToken = null;
		}

#region IDisposable
		private bool _disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				DisposeAsyncCore().Forget();
			}

			// Uncomment this line in inherited class to implement standard disposing pattern.
			// base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable

#region IAsyncDisposable
		private bool _asyncDisposed;

		public async ValueTask DisposeAsync()
		{
			await DisposeAsyncCore();

			Dispose(false);
			GC.SuppressFinalize(this);
		}

		protected virtual async UniTask DisposeAsyncCore()
		{
			if (_asyncDisposed)
				return;

			if (State is CONNECTING or CONNECTED)
				await DisconnectAsync();

			await WaitForDisconnected();

			DisposeConnectionToken();
			_classToken.Cancel();
			_classToken.Dispose();

			// Uncomment this line in inherited class to implement standard disposing pattern.
			// await base.DisposeAsyncCore();

			_asyncDisposed = true;
		}
#endregion // IAsyncDisposable

#region Debug
		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		protected virtual void Log(string message, [CallerMemberName] string? caller = null)
		{
			C2VDebug.LogMethod(GetLogCategory(), FormatMessage(message), caller);
		}

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		protected virtual void LogWarning(string message, [CallerMemberName] string? caller = null)
		{
			C2VDebug.LogWarningMethod(GetLogCategory(), FormatMessage(message), caller);
		}

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		protected virtual void LogError(string message, [CallerMemberName] string? caller = null)
		{
			C2VDebug.LogErrorMethod(GetLogCategory(), FormatMessage(message), caller);
		}

		[DebuggerHidden, StackTraceIgnore]
		protected virtual string GetLogCategory()
		{
			return GetType().Name;
		}

		[DebuggerHidden, StackTraceIgnore]
		protected virtual string FormatMessage(string message)
		{
			return ZString.Format(
				"{0}\n----------\n{1}"
			  , message, GetDebugInfo());
		}

		[DebuggerHidden, StackTraceIgnore]
		public virtual string GetDebugInfo()
		{
			return ZString.Format(
				"[{0}]\n: State = {1}"
			  , GetLogCategory(), State);
		}
#endregion // Debug
	}
}
