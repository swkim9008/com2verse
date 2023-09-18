/*===============================================================
* Product:		Com2Verse
* File Name:	InitializableViewModel.cs
* Developer:	urun4m0r1
* Date:			2022-08-29 13:05
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;

namespace Com2Verse.UI
{
	public abstract class InitializableViewModel<T> : ViewModelBase, IInitializable<T>, IDisposable where T : class
	{
		public bool IsInitialized        => Value != null;
		public void Initialize(T? value) => Value = value;
		public void Terminate()          => Value = null;

		private T? _value;

		public T? Value
		{
			get => _value;
			private set
			{
				var prevValue = _value;
				if (prevValue == value) return;

				UnsubscribePrevEvent();
				SubscribeCurrentEvent();

				_value = value;

				InvokePropertyValueChanged(nameof(Value), value);
				RefreshViewModel();

				void UnsubscribePrevEvent()
				{
					if (prevValue == null) return;

					OnPrevValueUnassigned(prevValue);
				}

				void SubscribeCurrentEvent()
				{
					if (value == null) return;

					OnCurrentValueAssigned(value);
				}
			}
		}

		protected abstract void OnPrevValueUnassigned(T  value);
		protected abstract void OnCurrentValueAssigned(T value);
		public abstract void RefreshViewModel();

#region IDisposable
		private bool _disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				Terminate();
			}

			// Uncomment this line in inherited class to implement standard disposing pattern.
			// base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
