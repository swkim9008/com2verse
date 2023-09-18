/*===============================================================
* Product:    Com2Verse
* File Name:  Collection.cs
* Developer:  tlghks1009
* Date:       2022-03-04 14:12
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;

namespace Com2Verse.UI
{
	public class Collection<T> : INotifyCollectionChanged where T : ViewModel
	{
		private Action<eNotifyCollectionChangedAction, IList, int> _onCollectionItemChanged;

		public void AddEvent(Action<eNotifyCollectionChangedAction, IList, int> eventHandler)
		{
			_onCollectionItemChanged -= eventHandler;
			_onCollectionItemChanged += eventHandler;
		}

		public void RemoveEvent(Action<eNotifyCollectionChangedAction, IList, int> eventHandler)
		{
			_onCollectionItemChanged -= eventHandler;
		}


		private readonly List<T> _value;

		public IReadOnlyList<T> Value => _value;

		public Collection()
		{
			_value = new List<T>();
		}

		public IEnumerable ItemsSource => _value;

		public int CollectionCount => _value.Count;

		public void SetItem(int index, T value)
		{
			_value[index] = value;

			_onCollectionItemChanged?.Invoke(eNotifyCollectionChangedAction.REPLACE, _value, index);
		}


		public void AddItem(T value)
		{
			_value.Add(value);

			_onCollectionItemChanged?.Invoke(eNotifyCollectionChangedAction.ADD, _value, _value.Count - 1);
		}


		public void AddRange(List<T> list)
		{
			if (list == null)
				return;

			_value.AddRange(list);

			_onCollectionItemChanged?.Invoke(eNotifyCollectionChangedAction.ADD_RANGE, list, 0);
		}

		public void RemoveItem(T value)
		{
			RemoveItem(_value.IndexOf(value));
		}


		public void RemoveItem(int index)
		{
			if (index < 0 || index >= _value.Count)
				return;

			_value.RemoveAt(index);

			_onCollectionItemChanged?.Invoke(eNotifyCollectionChangedAction.REMOVE, _value, index);
		}


		public void Reset()
		{
			_value.Clear();

			_onCollectionItemChanged?.Invoke(eNotifyCollectionChangedAction.RESET, _value, 0);
		}


		public void DestroyAll()
		{
			_value.Clear();

			_onCollectionItemChanged?.Invoke(eNotifyCollectionChangedAction.DESTROY_ALL, _value, 0);
		}

		public List<T> Clone()
		{
			return new List<T>(_value);
		}

		public T LastItem()
		{
			if (_value.Count == 0)
				return default(T);

			return _value[_value.Count - 1];
		}

		public T FirstItem()
		{
			if (_value.Count == 0)
				return default(T);

			return _value[0];
		}
	}
}
