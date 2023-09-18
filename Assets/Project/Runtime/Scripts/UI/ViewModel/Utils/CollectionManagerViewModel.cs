/*===============================================================
* Product:		Com2Verse
* File Name:	CollectionManagerViewModel.cs
* Developer:	urun4m0r1
* Date:			2022-06-17 13:12
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Com2Verse.Logger;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	public abstract class CollectionManagerViewModel<TKey, TViewModel> : ViewModelBase, IDisposable where TViewModel : ViewModelBase
	{
		public event Action<TKey, TViewModel>? ViewModelAdded;
		public event Action<TKey, TViewModel>? ViewModelRemoved;

		[UsedImplicitly] public Collection<TViewModel> Collection { get; } = new();

		[UsedImplicitly] public int  Count      => Collection.CollectionCount;
		[UsedImplicitly] public bool ItemExists => Count > 0;
		[UsedImplicitly] public bool IsEmpty    => Count == 0;

		public IReadOnlyDictionary<TKey, TViewModel> ViewModelMap => _viewModelMap;

		private readonly Dictionary<TKey, TViewModel> _viewModelMap = new();

		public bool TryGet(TKey key, [NotNullWhen(true)] out TViewModel? viewModel) =>
			_viewModelMap.TryGetValue(key, out viewModel);

		public bool Contains(TKey key) =>
			_viewModelMap.ContainsKey(key);

		protected void Add(TKey key, TViewModel viewModel, bool replaceIfExists = false)
		{
			if (TryGet(key, out var oldViewModel))
			{
				if (!replaceIfExists)
				{
					C2VDebug.LogWarningCategory(GetType().Name, $"Failed to add key \"{key?.ToString()}\": Key already exists with value \"{oldViewModel.ViewModelInfo}\".");
					return;
				}

				ViewModelRemoved?.Invoke(key, oldViewModel);
				(oldViewModel as IDisposable)?.Dispose();

				Collection.RemoveItem(oldViewModel);
				_viewModelMap[key] = viewModel;
			}
			else
			{
				_viewModelMap.Add(key, viewModel);
			}

			Collection.AddItem(viewModel);

			ViewModelAdded?.Invoke(key, viewModel);

			InvokeCollectionChanged();
		}

		protected void Remove(TKey key)
		{
			if (!TryGet(key, out var viewModel))
			{
				// C2VDebug.LogWarningCategory(GetType().Name, $"Failed to remove key \"{key?.ToString()}\": Key not found in collection.");
				return;
			}

			ViewModelRemoved?.Invoke(key, viewModel);
			(viewModel as IDisposable)?.Dispose();

			Collection.RemoveItem(viewModel);
			_viewModelMap.Remove(key);

			InvokeCollectionChanged();
		}

		public void Clear()
		{
			foreach (var item in _viewModelMap)
			{
				ViewModelRemoved?.Invoke(item.Key, item.Value);
				(item.Value as IDisposable)?.Dispose();
			}

			_viewModelMap.Clear();

			Collection.Reset();

			InvokeCollectionChanged();
		}

		private void InvokeCollectionChanged()
		{
			InvokePropertyValueChanged(nameof(Count), Count);
			InvokePropertyValueChanged(nameof(ItemExists), ItemExists);
			InvokePropertyValueChanged(nameof(IsEmpty), IsEmpty);
		}

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
				Clear();
			}

			// Uncomment this line in inherited class to implement standard disposing pattern.
			// base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
