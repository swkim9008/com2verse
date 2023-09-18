using System;
using System.Collections.Generic;

namespace Com2Verse.LocalCache
{
	public class DownloadRequest
	{
		internal Action OnReady;
		public bool IsPause;
		public bool IsRequested { get; internal set; }

		internal static readonly DownloadRequest Requested = new() { IsRequested = true };
	}
	internal class DownloadManager
	{
		private static readonly int MaxDownload = 10;
		private Queue<DownloadRequest> _downloadRequestQueue;
		private int _activeDownloadCount;
		private DownloadHandlerManager _handlerManager;
#region Singleton
		private static Lazy<DownloadManager> _instance = new(new DownloadManager());
		public static DownloadManager Instance = _instance.Value;
#endregion // Singleton

#region Initialize
		private DownloadManager()
		{
			_downloadRequestQueue = new Queue<DownloadRequest>();
			_handlerManager = new DownloadHandlerManager();
		}
#endregion // Initialize

#region Public Methods
		public DownloadRequest NewRequest(Action onDownloadReady)
		{
			if (_activeDownloadCount < MaxDownload)
			{
				_activeDownloadCount++;
				onDownloadReady?.Invoke();
				return DownloadRequest.Requested;
			}
			else
				return EnqueueNewRequest(onDownloadReady);
		}

		public DownloadHandler NewDownload(string url, string filePath, Callbacks.DownloadCallbacks onDownloadCallbacks, long offset = 0)
		{
			if (_handlerManager[url, filePath] != null)
			{
				if (onDownloadCallbacks.OnProgress != null)
					_handlerManager[url, filePath].AddOnProgress(onDownloadCallbacks.OnProgress);
				return _handlerManager[url, filePath];
			}

			var newHandler = new DownloadHandler(url, filePath, onDownloadCallbacks, offset);
			newHandler.AddOnDownloadComplete(OnDownloadComplete);
			newHandler.AddOnDownloadComplete((_, _) =>
			{
				if (_handlerManager[url, filePath] != null)
					_handlerManager.Remove(url, filePath);
			});
			_handlerManager.Add(url, filePath, newHandler);
			return newHandler;
		}
#endregion // Public Methods

#region DownloadHandlerManager
		private class DownloadHandlerManager
		{
			private Dictionary<(string, string), DownloadHandler> _mapItems;

			public DownloadHandlerManager()
			{
				_mapItems = new();
			}
			public DownloadHandler this[string url, string filePath] => _mapItems.ContainsKey((url, filePath)) ? _mapItems[(url, filePath)] : null;

			public void Remove(string url, string filePath)
			{
				if (_mapItems.ContainsKey((url, filePath)))
					_mapItems.Remove((url, filePath));
			}

			public void Add(string url, string filePath, DownloadHandler handler)
			{
				if (!_mapItems.ContainsKey((url, filePath)))
					_mapItems.Add((url, filePath), handler);
			}
		}
#endregion // DownloadHandlerManager

#region Request
		private DownloadRequest EnqueueNewRequest(Action onReady)
		{
			DownloadRequest newRequest = new DownloadRequest
			{
				OnReady = () =>
				{
					_activeDownloadCount++;
					onReady?.Invoke();
				}
			};
			_downloadRequestQueue.Enqueue(newRequest);
			return newRequest;
		}

		private void OnDownloadComplete(string hash, long totalSize)
		{
			_activeDownloadCount--;
			CheckRequestQueue();
		}
		private void CheckRequestQueue()
		{
			if (_activeDownloadCount >= MaxDownload) return;
			while (_downloadRequestQueue.Count > 0)
			{
				var request = _downloadRequestQueue.Peek();
				if (request.IsPause) continue;
				request.IsRequested = true;

				_downloadRequestQueue.Dequeue();
				if (request.OnReady == null) continue;
				request.OnReady();
				break;
			}
		}
#endregion // Request
	}
}
