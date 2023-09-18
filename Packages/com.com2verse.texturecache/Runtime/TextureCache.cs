/*===============================================================
* Product:		Com2Verse
* File Name:	TextureCache.cs
* Developer:	jhkim
* Date:			2022-10-20 17:22
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Extension;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using Cache = Com2Verse.LocalCache.Cache;

namespace Com2Verse.Utils
{
	public class TextureCache : Singleton<TextureCache>, IDisposable
	{
		private static readonly int MaxBufferSize = 1024 * 1024 * 10; // 10MB
		private QueueDictionary<string, CacheInfo> _cachedMap;
		private long _memoryUsage = 0;
		private HashSet<string> _downloadRequest;

#region Initialization
		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private TextureCache()
		{
			_cachedMap = new QueueDictionary<string, CacheInfo>();
			_downloadRequest = new HashSet<string>();
		}
#endregion // Initialization

#region Public Methods
		public async UniTask<Texture2D> GetOrDownloadTextureAsync(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
			{
				return null;
			}

			var key = System.IO.Path.GetFileName(new Uri(url).LocalPath);
			key = Cache.RemoveImageExtension(key);

			if (TryGetTexture(key, out var texture))
				return texture;

			if (!LocalCache.Cache.IsExist(key))
			{
				if (IsRequestExist(key))
					await UniTask.WaitUntil(() => !IsRequestExist(key));
				else
					AddRequest(key);
			}

			texture = await LoadTextureAndCacheAsync(key, url);
			ProcessDownloadRequest(key);

			return texture;
		}

		public async UniTask GetOrDownloadTextureAsync(string url, Action<bool, Texture> onLoaded)
		{
			if (string.IsNullOrWhiteSpace(url))
			{
				onLoaded?.Invoke(false, null);
				return;
			}

			if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
				return;

			var key = System.IO.Path.GetFileName(uri.LocalPath);
			key = Cache.RemoveImageExtension(key);

			if (TryGetTexture(key, out var texture))
			{
				onLoaded?.Invoke(true, texture);
				return;
			}

			if (!LocalCache.Cache.IsExist(key))
			{
				if (IsRequestExist(key))
					await UniTask.WaitUntil(() => !IsRequestExist(key));
				else
					AddRequest(key);
			}

			texture = await LoadTextureAndCacheAsync(key, url);
			ProcessDownloadRequest(key);

			onLoaded?.Invoke(!texture.IsReferenceNull(), texture);
		}

		public async UniTask GetOrDownloadTextureAsync(string key, string url, Action<bool, Texture> onLoaded)
		{
			if (string.IsNullOrWhiteSpace(url))
			{
				onLoaded?.Invoke(false, null);
				return;
			}

			if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
				return;

			if (TryGetTexture(key, out var texture))
			{
				onLoaded?.Invoke(true, texture);
				return;
			}

			if (!LocalCache.Cache.IsExist(key))
			{
				if (IsRequestExist(key))
					await UniTask.WaitUntil(() => !IsRequestExist(key));
				else
					AddRequest(key);
			}

			texture = await LoadTextureAndCacheAsync(key, url);
			ProcessDownloadRequest(key);

			onLoaded?.Invoke(!texture.IsReferenceNull(), texture);
		}
#endregion // Public Methods

#region Load Texture
		private async UniTask<Texture2D> LoadTextureAndCacheAsync(string key, string url)
		{
			var texture = await LocalCache.Cache.LoadTexture2DAsync(key, url);
			TryCacheTexture(key, texture);
			return texture;
		}
#endregion // Load Texture

#region Cache
		private bool TryGetTexture(string key, out Texture2D texture)
		{
			texture = null;
			if (_cachedMap.Contains(key))
			{
				texture = _cachedMap[key].Texture;
				return texture;
			}

			return false;
		}
		private bool TryCacheTexture(string key, Texture2D texture)
		{
			var textureSize = GetTextureSize(texture);
			if (textureSize > MaxBufferSize)
				return false;
			AddToCache(key, texture);
			return true;
		}
		private void AddToCache(string key, Texture2D texture)
		{
			var cacheInfo = new CacheInfo
			{
				Key = key,
				Size = GetTextureSize(texture),
				Texture = texture
			};

			if (_cachedMap.Contains(key))
				RemoveCache(key);

			while (!IsAvailableAdd(cacheInfo.Size) && _cachedMap.Count > 0)
				RemoveCacheFront();

			AddCache(cacheInfo);
		}

		private void RemoveCache(string key)
		{
			var removed = _cachedMap.Dequeue(key);
			_memoryUsage -= removed.Size;
		}

		private void AddCache(CacheInfo cacheInfo)
		{
			_memoryUsage += cacheInfo.Size;
			_cachedMap.Enqueue(cacheInfo.Key, cacheInfo);
		}

		private void RemoveCacheFront()
		{
			var frontItem = _cachedMap.Dequeue();
			_memoryUsage -= frontItem.Size;
		}

		private bool IsAvailableAdd(int size) => _memoryUsage + size <= MaxBufferSize;

		private int GetTextureSize(Texture2D tex)
		{
			if (tex.IsReferenceNull()) return 0;

			var bpp = TextureUtil.GetBitsPerPixel(tex.format);
			return tex.width * tex.height * bpp / 8;
		}

		private struct CacheInfo
		{
			public string Key;
			public int Size;
			public Texture2D Texture;
		}
#endregion // Cache

#region Queue
		// https://stackoverflow.com/questions/3965516/how-would-i-implement-a-queuedictionary-a-combination-of-queue-and-dictionary-i
		private class QueueDictionary<TKey, TValue>
		{
			private readonly LinkedList<Tuple<TKey, TValue>> _queue = new LinkedList<Tuple<TKey, TValue>>();
			private readonly Dictionary<TKey, LinkedListNode<Tuple<TKey, TValue>>> _dictionary = new Dictionary<TKey, LinkedListNode<Tuple<TKey, TValue>>>();
			public TValue this[TKey key]
			{
				get
				{
					if (Contains(key))
						return _dictionary[key].Value.Item2;
					return default;
				}
				set
				{
					if (Contains(key))
						Dequeue(key);
					Enqueue(key, value);
				}
			}
			public int Count => _dictionary.Count;
			public TValue Dequeue()
			{
				var item = _queue.First.Value;
				_queue.RemoveFirst();
				_dictionary.Remove(item.Item1);
				return item.Item2;
			}
			public TValue Peek()
			{
				var item = _queue.First.Value;
				return item.Item2;
			}
			public TValue Dequeue(TKey key)
			{
				var node = _dictionary[key];
				_dictionary.Remove(key);
				_queue.Remove(node);
				return node.Value.Item2;
			}

			public void Enqueue(TKey key, TValue value)
			{
				var node = _queue.AddLast(new Tuple<TKey, TValue>(key, value));
				_dictionary.Add(key, node);
			}

			public bool Contains(TKey key) => _dictionary.ContainsKey(key);
			public void Clear()
			{
				_queue.Clear();
				_dictionary.Clear();
			}
		}
#endregion // Queue

#region Download Request
		private bool IsRequestExist(string key) => _downloadRequest.Contains(key);
		private void AddRequest(string key)
		{
			if (!_downloadRequest.Contains(key))
				_downloadRequest.Add(key);
		}

		private void ProcessDownloadRequest(string key)
		{
			if (_downloadRequest.Contains(key))
				_downloadRequest.Remove(key);
		}
#endregion // Download Request
		public void Dispose() => _cachedMap?.Clear();
	}
}
