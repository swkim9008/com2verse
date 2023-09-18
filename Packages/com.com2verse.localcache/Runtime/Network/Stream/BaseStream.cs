using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Com2Verse.LocalCache
{
	internal abstract class BaseStream : Stream
	{
		public new virtual UniTask<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => throw new NotImplementedException();
		public new virtual UniTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => throw new NotImplementedException();
#region Hash
		public abstract bool CanHash { get; }
		public abstract bool UseHash { get; set; }
		public abstract string GetHash();
		protected abstract string GetHash(byte[] passphraseBytes);
		protected abstract void ClearHash();
#endregion // Hash
	}
}
