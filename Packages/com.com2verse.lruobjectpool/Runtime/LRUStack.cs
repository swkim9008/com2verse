/*===============================================================
* Product:		Com2Verse
* File Name:	LRUStack.cs
* Developer:	NGSG
* Date:			2023-05-26 16:30
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace Com2Verse.LruObjectPool
{
	public sealed class LRUStack<T> : IEnumerable<T> where T : class, new() 
	{
		private List<T> _stack;
		public List<T> Stack
		{
			get
			{
				return _stack;
			}
		}

		public LRUStack()
		{
			_stack = new List<T>();
		}

		public LRUStack(int capacity)
		{
			_stack = new List<T>(capacity);
		}

		public void Warmup(int capacity)
		{
			for (int i = 0; i < capacity; i++)
			{
				Push(new T());
			}
		}
		
		public IEnumerator<T> GetEnumerator()
		{
			return new LRUStackEnum<T>(_stack);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			//바로 위에 있는 함수를 불러서 사용한다.
			return GetEnumerator();
		}
		public void Clear()
		{
			_stack.Clear();
		}

		public int Count
		{
			get { return _stack.Count; }
		}
		
		public void Push(T t)
		{
			_stack.Add(t);
		}

		/// <summary>
		/// 풀에서 부족하면 비어있는 오브젝트 추가하고 리턴한다
		/// </summary>
		/// <param name="isAddEmpty"></param>
		/// <returns></returns>
		public T Pop(bool isAddEmpty = false)
		{
			if (_stack.Count > 0)
			{
				int lastIndex = _stack.Count - 1;
				T temp = _stack[lastIndex];
				_stack.RemoveAt(lastIndex);
				return temp;
			}

			// // 모자라면 추가하고 리턴한다
			// else if (isAddEmpty)
			// {
			// 	// 용량 두배로 늘린다
			// 	Warmup(_stack.Capacity);
			// 	UnityEngine.Debug.Log($"[LRUPool] Pool 에서 가져오기에는 부족라서 추가하고 리턴함 _stack.Capacity = {_stack.Capacity}");
			// 	return Pop();
			// }
			return null;
		}

		public T GetFront()
		{
			if (_stack.Count > 0)
			{
				T temp = _stack[0];
				_stack.RemoveAt(0);
				return temp;
			}

			return null;
		}

		public void Remove(int index)
		{
			_stack.RemoveAt(index);
		}

		public void RemoveRange(int index, int count)
		{
			_stack.RemoveRange(index, count);
		}


		public class LRUStackEnum<T> : IEnumerator<T>
		{
			private List<T> _list;
			private int position = -1;

			public LRUStackEnum(List<T> t)
			{
				_list = t;
			}

			public void Dispose() { }

			public bool MoveNext()
			{
				position++;
				return position < _list.Count;
			}

			public void Reset()
			{
				position = -1;
			}

			public T Current
			{
				get
				{
					try
					{
						return _list[position];
					}
					catch (IndexOutOfRangeException)
					{
						throw new InvalidOperationException();
					}
				}
			}

			object IEnumerator.Current => Current;
		}
	}
}
