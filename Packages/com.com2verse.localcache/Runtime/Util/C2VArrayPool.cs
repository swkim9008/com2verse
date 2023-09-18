/*===============================================================
* Product:		Com2Verse
* File Name:	ArrayPool.cs
* Developer:	jhkim
* Date:			2023-03-27 10:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;

namespace Com2Verse.LocalCache
{
	public sealed class C2VArrayPool<T> : IDisposable
	{
		private T[] _pooled;

		public T[] Data => _pooled;

		public static C2VArrayPool<T> Rent(int size)
		{
			var pool = new C2VArrayPool<T>()
			{
				_pooled = System.Buffers.ArrayPool<T>.Shared.Rent(size),
			};
			return pool;
		}

		public static C2VArrayPool<T> Rent(long size) => Rent(Convert.ToInt32(size));
		private void Release()
		{
			if (_pooled == null) return;
			System.Buffers.ArrayPool<T>.Shared.Return(_pooled);
		}
		public void Dispose() => Release();
	}
}
