/*===============================================================
 * Product:		Com2Verse
 * File Name:	ObservableHashSet.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-09 17:21
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using Com2Verse.Extension;

namespace Com2Verse.Utils
{
	/// <summary>
	/// <see cref="HashSet{T}"/> 을 이용해 중복을 허용하지 않는 아이템을 관리하는 클래스입니다.
	/// <br/><see cref="CollectionChanged"/> 이벤트는 아이템의 추가/제거가 발생할 때 발생합니다.
	/// <br/><see cref="ItemExistenceChanged"/> 이벤트는 목록에 아이템이 존재하는지 여부가 변경될 때 발생합니다.
	/// </summary>
	public sealed class ObservableHashSet<T> : ICollection<T>, IReadOnlyCollection<T>
	{
		/// <summary>
		/// 아이템의 추가/제거가 발생할 때 발생하는 이벤트입니다.
		/// </summary>
		public event Action<int>? CollectionChanged;

		/// <summary>
		/// 목록에 아이템이 존재하는지 여부가 변경될 때 발생하는 이벤트입니다.
		/// </summary>
		public event Action<bool>? ItemExistenceChanged;

		private readonly HashSet<T> _items = new();

		public bool IsReadOnly => false;

		public int Count => _items.Count;

		public bool IsAnyItemExists => _items.Count > 0;

		public bool Contains(T item)
		{
			return _items.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_items.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// 목록에 아이템을 추가합니다.
		/// <br/>옵저버 이벤트를 발생시킵니다.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// 이미 목록에 존재하는 아이템을 추가하려고 할 때 발생합니다.
		/// </exception>
		public void Add(T item)
		{
			bool result;
			var  previousCount = Count;
			{
				result = _items.Add(item);
			}
			CheckCollectionEvent(previousCount);

			if (!result)
				throw new InvalidOperationException($"'{item}' is already exists.");
		}

		/// <summary>
		/// 예외를 발생시키지 않고 목록에 아이템을 추가합니다.
		/// <br/>옵저버 이벤트를 발생시킵니다.
		/// </summary>
		public bool TryAdd(T item)
		{
			bool result;
			var  previousCount = Count;
			{
				result = _items.TryAdd(item);
			}
			CheckCollectionEvent(previousCount);
			return result;
		}

		/// <summary>
		/// 목록에서 아이템을 제거합니다.
		/// <br/>옵저버 이벤트를 발생시킵니다.
		/// </summary>
		public bool Remove(T item)
		{
			bool result;
			var  previousCount = Count;
			{
				result = _items.Remove(item);
			}
			CheckCollectionEvent(previousCount);
			return result;
		}

		/// <summary>
		/// 목록에서 모든 아이템을 제거합니다.
		/// <br/>옵저버 이벤트를 발생시킵니다.
		/// </summary>
		public void Clear()
		{
			var previousCount = Count;
			{
				_items.Clear();
			}
			CheckCollectionEvent(previousCount);
		}

		private void CheckCollectionEvent(int previousCount)
		{
			if (Count != previousCount)
			{
				CollectionChanged?.Invoke(Count);

				var wasAnyItemExists = previousCount > 0;
				if (IsAnyItemExists != wasAnyItemExists)
					ItemExistenceChanged?.Invoke(IsAnyItemExists);
			}
		}

		private IEnumerator<T>? _enumerator;

		public IEnumerator<T> GetEnumerator() => _enumerator ?? _items.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();
	}
}
