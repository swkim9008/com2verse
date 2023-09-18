using System;
using System.Buffers;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Com2Verse.LocalCache
{
    internal class StreamingDownloader : IDisposable
    {
        private static readonly int DefaultBufferSize = 0x10000;    // 기본 버퍼 사이즈 (65K)
        private int _bufferSize = DefaultBufferSize;

        private Callbacks.OnProgress _onProgress;
        private Callbacks.OnHttpStatus _onFailed;
        private Callbacks.OnDownloadComplete _onDownloadComplete;
        private Callbacks.OnReadBytesToStreamAsync _onReadByteToStreamAsync;

        private CancellationTokenSource _cts;
        private long _streamOffset;
        private string _saveFilePath;
        private Stream _stream;

        public CancellationToken CancellationToken => _cts.Token;
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

        [NotNull]
        public StreamingDownloader WithOnReadBytesToStreamAsync([CanBeNull] string filePath, Callbacks.OnReadBytesToStreamAsync onReadByteToStreamAsync)
        {
            _saveFilePath = filePath;
            _onReadByteToStreamAsync = onReadByteToStreamAsync;
            return this;
        }
#endregion // Builder

        public async UniTask DownloadAsync([CanBeNull] string url, long seekOffset = 0)
        {
            var client = UnityHttpClient.Get();
            _cts = new CancellationTokenSource();
            HttpResponseMessage response = null;
            try
            {
                response = await client.GetAsync(url, _cts.Token);
                if (!response.IsSuccessStatusCode)
                {
                    await Failed(response.StatusCode);
                    return;
                }
            }
            catch (Exception e)
            {
                C2VDebug.LogError($"Download Async Failed... {e}");
                await Failed(HttpStatusCode.RequestTimeout);
                return;
            }

            long totalReadLen = 0;
            if (response.IsSuccessStatusCode)
            {
                response = response.EnsureSuccessStatusCode();
                using var memoryOwner = MemoryPool<byte>.Shared.Rent(_bufferSize);
                var buffer = memoryOwner.Memory;
                var readLen = 0;
                Memory<byte> readBuffer;
                await using var stream = GetStream(client, response, url);

                if (TrySeek(stream, seekOffset))
                    totalReadLen = seekOffset;
                var totalSize = stream.Length;
                
                do
                {
                    if (_cts.IsCancellationRequested)
                        break;

                    try
                    {
                        readLen = await stream.ReadAsync(buffer, _cts.Token);
                        _streamOffset = stream.Position;

                        if (readLen <= 0) continue;

                        totalReadLen += readLen;
                        readBuffer = buffer.Slice(0, readLen);
                        await ReadBytesToStreamAsync(_stream, readBuffer);
                        await Progress(readLen, totalReadLen, totalSize);
                    }
                    catch (Exception e)
                    {
                        C2VDebug.LogWarning($"Read Failed...\n{e}");
                        break;
                    }
                }
                while (readLen > 0 || stream.CanRead);

                Dispose();
                if (!_cts.IsCancellationRequested)
                    await DownloadComplete(stream.GetHash(), totalReadLen);
                memoryOwner.Dispose();
            }
            else
            {
                Dispose();
                await Failed(response.StatusCode);
            }
        }

        private bool TrySeek(Stream stream, long offset)
        {
            if (offset <= 0) return false;
            if (stream is not SeekableHttpStream seekableStream) return false;
            if (stream.Length <= offset) return false;
            seekableStream.Seek(offset, SeekOrigin.Begin);
            return true;

        }
        public long Cancel()
        {
            _cts.Cancel();
            _cts.Dispose();
            // _cts = new CancellationTokenSource();
            return _streamOffset;
        }

        private static BaseStream GetStream(HttpClient client, HttpResponseMessage response, string url)
        {
            if (response.Headers.AcceptRanges.Count == 0 || response.StatusCode == HttpStatusCode.PartialContent)
                return new HashStream(client, response, new HttpRequestMessage(HttpMethod.Get, url));

            return new SeekableHttpStream(client, response, new HttpRequestMessage(HttpMethod.Get, url));
        }

        private static bool IsSeekableStream(BaseStream stream) => stream is SeekableHttpStream;
        
#region Delegate
        private async UniTask ReadBytesToStreamAsync(Stream stream, Memory<byte> buffer)
        {
            stream = GetFileStream(stream);
            if (stream == null)
            {
                C2VDebug.LogWarning("Invalid Stream...");
                Cancel();
            }
            await UniTask.SwitchToMainThread();
            await _onReadByteToStreamAsync?.Invoke(stream, buffer);
            await UniTask.SwitchToThreadPool();
        }

        private Stream GetFileStream(Stream stream)
        {
            if (stream != null) return stream;
            if (string.IsNullOrWhiteSpace(_saveFilePath))
                return null;
            
            if (_stream != null) return _stream;
            try
            {
                _stream = new FileStream(_saveFilePath, FileMode.OpenOrCreate);
                if (_streamOffset > 0 && _stream.Length > _streamOffset)
                    _stream.Seek(_streamOffset, SeekOrigin.Begin);
                return _stream;
            }
            catch (IOException e)
            {
                C2VDebug.LogWarning($"Check FileStream Failed\n{e}");
                return null;
            }
        }
        private async UniTask Progress(int readLen, long totalReadLen, long totalSize)
        {
            await UniTask.SwitchToMainThread();
            _onProgress?.Invoke(readLen, totalReadLen, totalSize);
            await UniTask.SwitchToThreadPool();
        }

        private async UniTask DownloadComplete(string hash, long totalReadLen)
        {
            await UniTask.SwitchToMainThread();
            _onDownloadComplete?.Invoke(hash, totalReadLen);
            await UniTask.SwitchToThreadPool();
        }
        private async UniTask Failed(HttpStatusCode statusCode)
        {
            await UniTask.SwitchToMainThread();
            _onFailed?.Invoke(statusCode);
            await UniTask.SwitchToThreadPool();
        }
#endregion // Delegate

        public void Dispose()
        {
            if (_stream == null) return;
            _stream.Dispose();
            _stream = null;
        }
    }
}
