using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Com2Verse.LocalCache
{
    internal class HashStream : BaseStream
    {
        private Stream _underlyingStream;
        private HashAlgorithm _hash;
        private bool _canHash;
        private string _hashString = string.Empty;
        private bool _hashAvailable => UseHash && _canHash;
        private readonly string _hashAlgorithm = "MD5";
        public HttpClient Client { get; }
        public HttpResponseMessage Response { get; }
        public HttpRequestMessage Request { get; }

        internal HashStream(HttpClient client, HttpResponseMessage response, HttpRequestMessage request)
        {
            Client = client;
            Response = response;
            Request = request;

            _canHash = true;
            _hash = HashAlgorithm.Create(_hashAlgorithm);
        }
        public override void Flush()
        {
        }

        private async UniTask EnsureStreamOpen()
        {
            _underlyingStream ??= await Response.Content.ReadAsStreamAsync();
        }
        public override int Read(byte[] buffer, int offset, int count) => throw new System.NotImplementedException();

        public override async UniTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await EnsureStreamOpen();
            int read = await _underlyingStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            Position += read;
            if (_hashAvailable)
            {
                var bufferArr = buffer.ToArray();
                var offset = Convert.ToInt32(Position - read);
                _hash.TransformBlock(bufferArr, offset, read, bufferArr, offset);
            }

            return read;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new System.NotImplementedException();

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead { get; }
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length { get; }
        public override long Position { get; set; }

#region Hash
        public override bool CanHash => _canHash;
        public override bool UseHash { get; set; } = true;
        public override string GetHash() => GetHash(Array.Empty<byte>());

        protected override string GetHash(byte[] passphraseBytes)
        {
            if (!_hashAvailable)
                return string.Empty;
            if (string.IsNullOrWhiteSpace(_hashString))
            {
                _hash.TransformFinalBlock(passphraseBytes, 0, passphraseBytes.Length);
                _hashString = HashUtil.ByteArrayToString(_hash.Hash);
            }

            return _hashString;
        }

        protected override void ClearHash()
        {
            _hash?.Clear();
            _hash = HashAlgorithm.Create(_hashAlgorithm);
            _hashString = string.Empty;
        }
#endregion // Hash
    }
}
