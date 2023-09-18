/*===============================================================
* Product:		Com2Verse
* File Name:	StreamingDownloader.cs
* Developer:	jhkim
* Date:			2023-02-06 10:14
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Buffers;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Com2Verse.HttpHelper
{
    internal class StreamingDownloader : IDisposable
    {
#region Variables
        private static readonly int DefaultBufferSize = 0x10000; // 기본 버퍼 사이즈 (65K)
        private int _bufferSize = DefaultBufferSize;

        private Callbacks.OnProgress _onProgress;
        private Callbacks.OnHttpStatus _onFailed;
        private Callbacks.OnDownloadComplete _onDownloadComplete;
        private Action _onFinally;
        // private Callbacks.OnReadBytesToStreamAsync _onReadByteToStreamAsync;

        private CancellationTokenSource _cts;
        private long _streamOffset;
        private Stream _stream;
#endregion // Variables

#region Properties
        public CancellationToken CancellationToken => _cts.Token;
#endregion // Properties

#region Builder
        [NotNull]
        public StreamingDownloader SetBufferSize(int bufferSize)
        {
            _bufferSize = bufferSize;
            return this;
        }

        // [NotNull]
        // public StreamingDownloader WithOnReadBytesAsync([CanBeNull] OnReadBytesAsync onReadBytesAsync)
        // {
        //     _onReadBytesAsync = onReadBytesAsync;
        //     return this;
        // }
        [NotNull]
        public StreamingDownloader WithOnProgress([CanBeNull] Callbacks.OnProgress onProgress)
        {
            _onProgress = onProgress;
            return this;
        }
        [NotNull]
        public StreamingDownloader WithOnFailed([CanBeNull] Callbacks.OnHttpStatus onFailed)
        {
            _onFailed = onFailed;
            return this;
        }

        [NotNull]
        public StreamingDownloader WithOnDownloadComplete([CanBeNull] Callbacks.OnDownloadComplete onDownloadComplete)
        {
            _onDownloadComplete = onDownloadComplete;
            return this;
        }

        // [NotNull]
        // public StreamingDownloader WithOnReadBytesToStreamAsync(Callbacks.OnReadBytesToStreamAsync onReadByteToStreamAsync)
        // {
        //     _onReadByteToStreamAsync = onReadByteToStreamAsync;
        //     return this;
        // }
        [NotNull]
        public StreamingDownloader WithCancellationTokenSource([CanBeNull] CancellationTokenSource cts)
        {
            _cts = cts;
            return this;
        }

        [NotNull]
        public StreamingDownloader WithOnFinally(Action onFinally)
        {
            _onFinally = onFinally;
            return this;
        }
#endregion // Builder

#region Public Methods
        public async UniTask RequestAsync(HttpResponseMessage response, string url, string content = "", CancellationTokenSource cts = null)
        {
            if (response == null)
            {
                C2VDebug.LogError("Invalid Request");
                return;
            }

            var client = UnityHttpClient.Get();
            _cts ??= cts ?? new CancellationTokenSource();

            long totalReadLen = 0;
            if (response.IsSuccessStatusCode)
            {
                response = response.EnsureSuccessStatusCode();
                using var memoryOwner = MemoryPool<byte>.Shared.Rent(_bufferSize);
                var buffer = memoryOwner.Memory;
                var readLen = 0;
                Memory<byte> readBuffer;
                await using var stream = GetStream(client, response, url, content);

                var totalSize = stream.Length;

                await DisposeStreamAsync();
                _stream = new MemoryStream();

                do
                {
                    if (_cts == null || _cts.IsCancellationRequested)
                        break;

                    try
                    {
                        var (isCancelled, len) = await stream.ReadAsync(buffer, _cts.Token).SuppressCancellationThrow();
                        readLen = len;
                        _streamOffset = stream.Position;

                        if (isCancelled) break;

                        if (readLen <= 0) continue;

                        totalReadLen += readLen;
                        readBuffer = buffer.Slice(0, readLen);

                        isCancelled = await ReadBytesToStreamAsync(_stream, readBuffer).SuppressCancellationThrow();
                        if (isCancelled) break;

                        await Progress(readLen, totalReadLen, totalSize);
                    }
                    catch (Exception e)
                    {
                        if (!_cts.IsCancellationRequested)
                            C2VDebug.LogWarning($"Read Failed...\n{e}");
                        break;
                    }
                }
                while (readLen > 0 || stream.CanRead);

                if (!_cts.IsCancellationRequested)
                {
                    await DownloadComplete(_stream, totalReadLen);
                    await DisposeAsync();
                }
                else
                {
                    Dispose(true);
                }
                memoryOwner.Dispose();
            }
            else
            {
                Dispose(true);
                C2VDebug.LogWarning($"Request Failed...{response.StatusCode}\n{response.RequestMessage.Content.Headers.ContentType}");
                await Failed(response.StatusCode);
            }

            _onFinally?.Invoke();
        }

        public long Cancel()
        {
            DisposeCancellationTokenSource();
            return _streamOffset;
        }
#endregion // Public Methods

#region Stream
        private bool TrySeek(Stream stream, long offset)
        {
            if (offset <= 0) return false;
            if (stream is not SeekableHttpStream seekableStream) return false;
            if (stream.Length <= offset) return false;
            seekableStream.Seek(offset, SeekOrigin.Begin);
            return true;
        }

        private static BaseStream GetStream(HttpClient client, HttpResponseMessage response, string url, string content = "")
        {
            var request = new HttpRequestMessage(response.RequestMessage.Method, url);
            if (response.RequestMessage.Method == HttpMethod.Post || response.RequestMessage.Method == HttpMethod.Put)
            {
                if (!string.IsNullOrWhiteSpace(content))
                    request.Content = new StringContent(content);
            }

            if (response.Headers.AcceptRanges.Count == 0 || response.StatusCode == HttpStatusCode.PartialContent)
                return new HashStream(client, response, request);

            return new SeekableHttpStream(client, response, request);
        }

        private static bool IsSeekableStream(BaseStream stream) => stream is SeekableHttpStream;
#endregion // Stream

#region Delegate
        private async UniTask ReadBytesToStreamAsync(Stream stream, Memory<byte> buffer)
        {
            if (stream == null)
            {
                C2VDebug.LogError("Invalid Stream...");
                Cancel();
                return;
            }

            await UniTask.SwitchToMainThread();
            if (stream.CanWrite)
            {
                var task = stream.WriteAsync(buffer, _cts.Token);
                if (!task.IsCanceled)
                    await task;
            }
            await UniTask.SwitchToThreadPool();
        }
        private async UniTask Progress(int readLen, long totalReadLen, long totalSize)
        {
            await UniTask.SwitchToMainThread();
            _onProgress?.Invoke(readLen, totalReadLen, totalSize);
            await UniTask.SwitchToThreadPool();
        }
        private async UniTask DownloadComplete(Stream readStream, long totalReadLen)
        {
            await UniTask.SwitchToMainThread();
            if (readStream.CanSeek)
                readStream.Seek(0, SeekOrigin.Begin);
            _onDownloadComplete?.Invoke(readStream, totalReadLen);
            await UniTask.SwitchToThreadPool();
        }
        private async UniTask Failed(HttpStatusCode statusCode)
        {
            if (_onFailed == null) return;

            await UniTask.SwitchToMainThread();
            _onFailed.Invoke(statusCode);
            await UniTask.SwitchToThreadPool();
        }
#endregion // Delegate

#region Dispose
        public void Dispose() => Dispose(false);
        private void Dispose(bool disposeCts)
        {
            DisposeStream();
            if (disposeCts)
                DisposeCancellationTokenSource();
        }

        private async UniTask DisposeAsync(bool disposeCts = false)
        {
            await DisposeStreamAsync();
            if (disposeCts)
                DisposeCancellationTokenSource();
        }

        private void DisposeStream()
        {
            if (_stream == null) return;
            _stream.Dispose();
            _stream = null;
        }

        private async UniTask DisposeStreamAsync()
        {
            if (_stream == null) return;
            await _stream.DisposeAsync();
            _stream = null;
        }

        public void DisposeCancellationTokenSource()
        {
            if (_cts is {IsCancellationRequested: false})
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }
#endregion // Dispose
    }
}
