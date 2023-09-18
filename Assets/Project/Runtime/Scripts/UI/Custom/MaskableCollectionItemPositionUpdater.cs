/*===============================================================
 * Product:		Com2Verse
 * File Name:	MaskableCollectionItemPositionUpdater.cs
 * Developer:	urun4m0r1
 * Date:		2023-04-18 16:03
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.UI;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse.UIExtension
{
	/// <inheritdoc />
	/// <summary>
	/// Mask 로 인해 가려질 수 있는 CollectionItem 의 위치를 업데이트 해주는 스크립트.
	/// <br /><see cref="T:Com2Verse.UIExtension.MaskableRectVisibilityListener" />와 같이 사용해야 한다.
	/// </summary>
	public class MaskableCollectionItemPositionUpdater : MonoBehaviour
	{
		[SerializeField] private CollectionBinder? _collectionBinder;
		[SerializeField] private Mask?             _itemMask;

		private RectTransform? _maskRect;

		private readonly List<MaskableRectVisibilityListener> _listeners = new();

		private readonly string _logCategory = nameof(MaskableCollectionItemPositionUpdater);

		private bool ValidateReferences()
		{
			if (_collectionBinder.IsUnityNull())
			{
				C2VDebug.LogErrorMethod(_logCategory, "CollectionBinder is null");
				return false;
			}

			if (_itemMask.IsUnityNull())
			{
				C2VDebug.LogErrorMethod(_logCategory, "ItemMask is null");
				return false;
			}

			return true;
		}

		private void Awake()
		{
			if (!ValidateReferences())
				return;

			_maskRect = _itemMask!.rectTransform!;

			_collectionBinder!.ItemActivated   += OnItemActivated;
			_collectionBinder!.ItemDeactivated += OnItemDeactivated;

			foreach (var item in _collectionBinder!.ActivatedItems!)
				OnItemActivated(item);
		}

		private void OnDestroy()
		{
			if (!_collectionBinder.IsUnityNull())
			{
				_collectionBinder!.ItemActivated   -= OnItemActivated;
				_collectionBinder!.ItemDeactivated -= OnItemDeactivated;
			}

			_listeners.Clear();
		}

		private void OnItemActivated(CollectionItem item)
		{
			var go = item.CollectionItemObject;
			if (go.IsUnityNull())
				return;

			var listener = go!.GetComponent<MaskableRectVisibilityListener>();
			if (listener.IsUnityNull())
				return;

			_listeners.Add(listener);

			OnPositionUpdated();
		}

		private void OnItemDeactivated(CollectionItem item)
		{
			var go = item.CollectionItemObject;
			if (go.IsUnityNull())
				return;

			var listener = go!.GetComponent<MaskableRectVisibilityListener>();
			if (listener.IsUnityNull())
				return;

			_listeners.Remove(listener);
		}

		/// <summary>
		/// <see cref="ScrollRect"/> 등 UI 업데이트 시 OnValueChanged 이벤트를 받아서 호출 가능.
		/// </summary>
		[UsedImplicitly]
		public void OnPositionUpdated()
		{
			foreach (var item in _listeners)
				item.InvokePositionUpdated(_maskRect!);
		}
	}
}
