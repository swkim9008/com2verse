using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Com2Verse.LocalCache
{
    public class DownloadHandler
    {
        private enum eDownloadStatus
        {
            NONE,
            DOWNLOADING_RESERVED,
            DOWNLOADING,
            PAUSED,
            SUCCESS,
            FAILED,
        }

#region Variables
        private string _url;
        private long _offset;
        private string _saveFilePath;
        private StreamingDownloader _downloader;
        private Callbacks.DownloadCallbacks _downloadCallbacks;
        private Callbacks.DownloadCallbacks _userDownloadCallbacks;
        private eDownloadStatus _downloadStatus = eDownloadStatus.NONE;
#endregion // Variables

#region Properties
        internal CancellationToken CancellationToken => _downloader.CancellationToken;
#endregion // Properties

#region Initialization
        // [Obsolete]
        // internal DownloadHandler(string url, Callbacks.DownloadCallbacks downloadCallbacks, Stream stream, long offset = 0)
        // {
        //     _url = url;
        //     _offset = offset;
        //     _downloader = new StreamingDownloader();
        //     _downloadCallbacks = downloadCallbacks;
        //     _userDownloadCallbacks = new Callbacks.DownloadCallbacks();
        //
        //     SetDefaultCallbacks(ref _downloadCallbacks);
        //     SetDownloadCallbacks(downloadCallbacks);
        //
        //     void SetDefaultCallbacks(ref Callbacks.DownloadCallbacks callbacks)
        //     {
        //         if (callbacks.OnProgress != null) callbacks.OnProgress += OnProgress;
        //         if (callbacks.OnReadBytesToStreamAsync != null) callbacks.OnReadBytesToStreamAsync += OnReadBytesToStreamAsync;
        //         if (callbacks.OnFailed != null) callbacks.OnFailed += OnFailed;
        //         if (callbacks.OnDownloadComplete != null) callbacks.OnDownloadComplete += OnDownloadComplete;
        //     }
        //
        //     void SetDownloadCallbacks(Callbacks.DownloadCallbacks callbacks)
        //     {
        //         _downloader.WithOnProgress(callbacks.OnProgress);
        //         _downloader.WithOnFailed(callbacks.OnFailed);
        //         _downloader.WithOnDownloadComplete(callbacks.OnDownloadComplete);
        //     }
        // }

        internal DownloadHandler(string url, string filePath, Callbacks.DownloadCallbacks downloadCallbacks, long offset = 0)
        {
            _url = url;
            _offset = offset;
            _saveFilePath = filePath;
            _downloader = new StreamingDownloader();
            _downloadCallbacks = downloadCallbacks;
            _userDownloadCallbacks = new Callbacks.DownloadCallbacks();
            
            SetDefaultCallbacks(ref _downloadCallbacks);
            SetDownloadCallbacks(downloadCallbacks);

            void SetDefaultCallbacks(ref Callbacks.DownloadCallbacks callbacks)
            {
                if (callbacks.OnProgress != null) callbacks.OnProgress += OnProgress;
                if (callbacks.OnReadBytesToStreamAsync != null) callbacks.OnReadBytesToStreamAsync += OnReadBytesToStreamAsync;
                if (callbacks.OnFailed != null) callbacks.OnFailed += OnFailed;
                if (callbacks.OnDownloadComplete != null) callbacks.OnDownloadComplete += OnDownloadComplete;
            }

            void SetDownloadCallbacks(Callbacks.DownloadCallbacks callbacks)
            {
                _downloader.WithOnProgress(callbacks.OnProgress);
                _downloader.WithOnReadBytesToStreamAsync(_saveFilePath, callbacks.OnReadBytesToStreamAsync);
                _downloader.WithOnFailed(callbacks.OnFailed);
                _downloader.WithOnDownloadComplete(callbacks.OnDownloadComplete);
            }
        }
#endregion // Initialization

#region Public Methods
        public bool IsDownloading() => _downloadStatus == eDownloadStatus.DOWNLOADING;
        public bool IsSuccess() => _downloadStatus == eDownloadStatus.SUCCESS;
        public bool IsFailed() => _downloadStatus == eDownloadStatus.FAILED;
        public bool IsPaused() => _downloadStatus == eDownloadStatus.PAUSED;
        public bool IsDone() => IsSuccess() || IsFailed();

        public void StartDownload()
        {
            if (_downloadStatus == eDownloadStatus.NONE)
            {
                _downloadStatus = eDownloadStatus.DOWNLOADING_RESERVED;
                _downloader.DownloadAsync(_url, _offset).Forget();
            }
        }

        public void PauseDownload()
        {
            if (IsPaused())
                return;

            _offset = _downloader.Cancel();
            _downloadStatus = eDownloadStatus.PAUSED;
        }

        public void ResumeDownload()
        {
            if (IsPaused())
                StartDownload();
        }
        public void StopDownload()
        {
            PauseDownload();
            _offset = 0;
        }
#endregion // Public Methods

#region Download Callbacks
        void OnProgress(int read, long totalRead, long size)
        {
            _downloadStatus = eDownloadStatus.DOWNLOADING;
        }
        Task OnReadBytesToStreamAsync(Stream stream, Memory<byte> bytes) => Task.CompletedTask;

        void OnFailed(HttpStatusCode statusCode)
        {
            _downloadStatus = eDownloadStatus.FAILED;
        }
        void OnDownloadComplete(string hash, long totalSize)
        {
            _downloadStatus = eDownloadStatus.SUCCESS;
        }
        internal void AddOnProgress(Callbacks.OnProgress onProgress)
        {
            if (_downloadCallbacks.OnProgress == null) return;
            _downloadCallbacks.OnProgress -= _userDownloadCallbacks.OnProgress;
            _userDownloadCallbacks.OnProgress += onProgress;
            _downloadCallbacks.OnProgress += _userDownloadCallbacks.OnProgress;
            // _userDownloadCallbacks.OnProgress = onProgress;
            _downloader.WithOnProgress(_downloadCallbacks.OnProgress);
        }

        internal void SetOnReadBytesToStreamAsync(Callbacks.OnReadBytesToStreamAsync onReadBytesToStreamAsync)
        {
            if (_downloadCallbacks.OnReadBytesToStreamAsync == null) return;
            _downloadCallbacks.OnReadBytesToStreamAsync -= _userDownloadCallbacks.OnReadBytesToStreamAsync;
            _downloadCallbacks.OnReadBytesToStreamAsync += onReadBytesToStreamAsync;
            _userDownloadCallbacks.OnReadBytesToStreamAsync = onReadBytesToStreamAsync;
            _downloader.WithOnReadBytesToStreamAsync(_saveFilePath, _downloadCallbacks.OnReadBytesToStreamAsync);
        }

        internal void SetOnFailed(Callbacks.OnHttpStatus onFailed)
        {
            if (_downloadCallbacks.OnFailed == null) return;
            _downloadCallbacks.OnFailed -= _userDownloadCallbacks.OnFailed;
            _downloadCallbacks.OnFailed += onFailed;
            _userDownloadCallbacks.OnFailed = onFailed;
            _downloader.WithOnFailed(_downloadCallbacks.OnFailed);
        }

        internal void AddOnDownloadComplete(Callbacks.OnDownloadComplete onDownloadComplete)
        {
            if (_downloadCallbacks.OnDownloadComplete == null) return;
            _downloadCallbacks.OnDownloadComplete -= _userDownloadCallbacks.OnDownloadComplete;
            _userDownloadCallbacks.OnDownloadComplete += onDownloadComplete;
            _downloadCallbacks.OnDownloadComplete += _userDownloadCallbacks.OnDownloadComplete;
            _downloader.WithOnDownloadComplete(_downloadCallbacks.OnDownloadComplete);
        }
#endregion // Download Callbacks
    }
}
