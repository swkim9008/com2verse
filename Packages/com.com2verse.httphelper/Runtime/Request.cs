/*===============================================================
* Product:		Com2Verse
* File Name:	Request.cs
* Developer:	jhkim
* Date:			2023-02-06 10:14
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using Com2Verse.HttpHelper;
using Cysharp.Threading.Tasks;

namespace Com2Verse.HttpHelper
{
	internal sealed class RequestHandler : IDisposable, IRequestHandler
	{
#region Enum
		private enum eState
		{
			NONE,
			IN_QUEUE,
			DOWNLOAD_START,
			DOWNLOADING,
			SUCCESS,
			FAILED,
			CANCELLED,
		}
#endregion // Enum

#region Variables
		private eState _state = eState.NONE;
		private Callbacks _callbacks;
		private StreamingDownloader _downloader;
		private HttpResponseMessage _message;
		private CancellationTokenSource _cts;
		private string _url;
		private string _content;
#endregion // Variables

#region Properties
		internal string URL
		{
			set => _url = value;
		}

		internal string Content
		{
			set => _content = value;
		}

		internal static RequestHandler Empty = new RequestHandler
		{
			_callbacks = Callbacks.Empty,
		};
#endregion // Properties

#region Initialization
		private RequestHandler() { }

		private void Init()
		{
			if (_callbacks.IsUndefined()) return;

			_downloader.WithOnProgress((read, totalRead, totalSize) =>
			{
				SetState(eState.DOWNLOADING);
				_callbacks.OnDownloadProgress?.Invoke(read, totalRead, totalSize);
			});
			_downloader.WithOnDownloadComplete((readStream, totalReadLen) =>
			{
				SetState(eState.SUCCESS);
				_callbacks.OnComplete?.Invoke(readStream, totalReadLen);
			});
			_downloader.WithOnFailed((httpStatusCode) =>
			{
				SetState(eState.FAILED);
				_callbacks.OnFailed?.Invoke(httpStatusCode);
			});
			_downloader.WithOnFinally(() =>
			{
				_callbacks.OnFinally?.Invoke();
			});
		}
#endregion // Initialization

#region Public Methods
		public async UniTask SendAsync()
		{
			if (!IsSendAvailable) return;

			// Queue에서 대기
			SetState(eState.IN_QUEUE);

			if (_cts.IsCancellationRequested) return;

			Client.Increment();
			Client.SetMaxConcurrency(_url, Client.MaxConcurrency);
			SetState(eState.DOWNLOAD_START);
			_callbacks.OnDownloadStart?.Invoke();
			await _downloader.RequestAsync(_message, _url, _content, _cts);
			Client.Decrement();
		}
		public void Cancel()
		{
			SetState(eState.CANCELLED);
			DisposeCancellation();
		}
#endregion // Public Methods

#region Internal / Private Methods
		internal static RequestHandler New(Callbacks callbacks, HttpResponseMessage response = null, CancellationTokenSource cts = null)
		{
			var newRequest = new RequestHandler
			{
				_callbacks = callbacks,
				_downloader = new StreamingDownloader(),
			};
			newRequest.Init();
			newRequest.WithCancellationToken(cts);

			if (response != null)
				newRequest.SetResponseMessage(response);
			return newRequest;
		}

		private void SetResponseMessage(HttpResponseMessage message) => _message = message;
		private RequestHandler WithCancellationToken(CancellationTokenSource cts, bool use = true)
		{
			if (use)
			{
				_cts ??= cts ?? new CancellationTokenSource();
				_downloader.WithCancellationTokenSource(_cts);
			}
			else
				DisposeCancellation();

			return this;
		}
#endregion // Internal / Private Methods

#region Cancellation
		private void DisposeCancellation()
		{
			if (_cts is {IsCancellationRequested: false})
			{
				_cts.Cancel();
				_cts.Dispose();
				_cts = null;
				_downloader.DisposeCancellationTokenSource();
			}
		}
#endregion // Cancellation

#region State
		private void SetState(eState state)
		{
			if (state == _state) return;

			_state = state;
		}

		private bool IsSendAvailable => _state switch
		{
			eState.NONE => true,
			_           => false,
		};

		private bool IsCancelAvailable => _state switch
		{
			eState.IN_QUEUE | eState.DOWNLOAD_START | eState.DOWNLOADING => true,
			_ => false,
		};
#endregion // State

#region Dispose
		public void Dispose()
		{
			DisposeCancellation();
		}
#endregion // Dispose
	}
#region Interfaces
	public interface IRequestHandler
	{
		UniTask SendAsync();
		void Cancel();
		void Dispose();
	}
#endregion // Interfaces

#region Callbacks
	public struct Callbacks
	{
		public delegate void OnProgress(int currentRead, long totalRead, long totalSize);
		public delegate void OnDownloadComplete(Stream readStream, long totalReadLen);
		public delegate void OnHttpStatus(HttpStatusCode statusCode);

		public OnProgress OnDownloadProgress;
		public OnDownloadComplete OnComplete;
		public OnHttpStatus OnFailed;
		public Action OnDownloadStart;
		public Action OnFinally;
		public static Callbacks Empty { get; } = new()
		{
			OnDownloadProgress = (_, _, _) => { },
			OnComplete = (_, _) => { },
			OnFailed = (_) => { },
			OnDownloadStart = () => { },
			OnFinally = () => { },
		};
	}
#endregion // Callbacks
}

#region Extensions
static class CallbackExtensions
{
	public static bool IsUndefined(this Callbacks callbacks) => callbacks.OnDownloadProgress == null
	                                                         && callbacks.OnComplete == null
	                                                         && callbacks.OnFailed == null
	                                                         && callbacks.OnDownloadStart == null
	                                                         && callbacks.OnFinally == null;
}
#endregion // Extensions