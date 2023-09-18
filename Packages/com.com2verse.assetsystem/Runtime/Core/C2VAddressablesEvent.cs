/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesEvent.cs
* Developer:	tlghks1009
* Date:			2023-02-20 14:52
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Com2Verse.AssetSystem
{
	public sealed class C2VAddressablesEvent<T, T1>
	{
		public event Action<T, T1> OnCompleted;

		public AsyncOperationStatus Status { get; set; } = AsyncOperationStatus.Succeeded;

		public void AddListener(Action<T, T1> handler)
		{
			OnCompleted -= handler;
			OnCompleted += handler;
		}

		public void RemoveListener(Action<T, T1> handler) => OnCompleted -= handler;

		public void RemoveListener() => OnCompleted = null;

		public void Invoke(T t, T1 t1) => OnCompleted?.Invoke(t, t1);

		public void Dispose() => RemoveListener();
	}

	public sealed class C2VAddressablesEvent<T>
	{
		public event Action<T> OnCompleted;

		public AsyncOperationStatus Status { get; set; } = AsyncOperationStatus.Succeeded;

		public void AddListener(Action<T> handler)
		{
			OnCompleted -= handler;
			OnCompleted += handler;
		}

		public void RemoveListener(Action<T> handler) => OnCompleted -= handler;

		public void RemoveListener() => OnCompleted = null;

		public void Invoke(T result) => OnCompleted?.Invoke(result);

		public void Dispose() => RemoveListener();
	}


	public sealed class C2VAddressablesEvent
	{
		public event Action OnCompleted;

		public AsyncOperationStatus Status { get; set; } = AsyncOperationStatus.Succeeded;

		public void AddListener(Action handler)
		{
			OnCompleted -= handler;
			OnCompleted += handler;
		}

		public void RemoveListener(Action handler) => OnCompleted -= handler;

		public void RemoveListener() => OnCompleted = null;

		public void Invoke() => OnCompleted?.Invoke();

		public void Dispose() => RemoveListener();
	}
}
