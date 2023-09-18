using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Com2Verse.LocalCache
{
    public static class Callbacks
    {
        public delegate void OnProgress(int currentRead, long totalRead, long totalSize);
        public delegate void OnHttpStatus(HttpStatusCode statusCode);
        public delegate void OnDownloadComplete(string hash, long totalSize);
        public delegate void OnLoadComplete(bool success, long totalSize, Memory<byte> bytes);
        public delegate void OnDownloadHandler(DownloadHandler handler);
        internal delegate Task OnReadBytesToStreamAsync(Stream stream, Memory<byte> readBytes);
        public delegate void OnDownloadRequest(DownloadRequest request);
        public struct LoadCacheCallbacks
        {
            public OnProgress OnProgress;
            public OnLoadComplete OnLoadComplete;
        }
        public struct DownloadCallbacks
        {
            public OnProgress OnProgress;
            public OnHttpStatus OnFailed;
            public OnDownloadComplete OnDownloadComplete;
            public OnDownloadHandler OnDownloadHandler;
            internal OnReadBytesToStreamAsync OnReadBytesToStreamAsync;

            public static DownloadCallbacks Empty { get; } = new()
            {
                OnProgress = (_, _, _) => { },
                OnFailed = _ => { },
                OnDownloadComplete = (_, _) => { },
                OnDownloadHandler = _ => { },
                OnReadBytesToStreamAsync = (_, _) => null,
            };
        }

        public struct DownloadRequestCallbacks
        {
            public OnDownloadRequest OnDownloadRequest;
        }
    }
    
    static class CallbacksExtensions
    {
        public static bool IsUndefined(this Callbacks.DownloadCallbacks callbacks) => callbacks.OnFailed == null && callbacks.OnDownloadComplete == null;
    }
}