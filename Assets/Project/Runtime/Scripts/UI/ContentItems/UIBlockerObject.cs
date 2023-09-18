/*===============================================================
 * Product:		Com2Verse
 * File Name:	UIBlockerObject.cs
 * Developer:	urun4m0r1
 * Date:		2023-05-24 15:11
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Com2Verse.UI
{
	public class UIBlockerObject : MonoBehaviour
	{
		[field: SerializeField] public UnityEvent? BlockerCreatedEvent   { get; private set; }
		[field: SerializeField] public UnityEvent? BlockerDestroyedEvent { get; private set; }

		[field: SerializeField] public bool WillCreateBlockerOnEnable   { get; set; } = true;
		[field: SerializeField] public bool WillDestroyBlockerOnDisable { get; set; } = true;

		[SerializeField] private bool _overrideSortingOrder;

		[SerializeField, DrawIf(nameof(_overrideSortingOrder), true)] private int _targetSortingOrder;
		[SerializeField, DrawIf(nameof(_overrideSortingOrder), true)] private int _blockerSortingOrder;

		public bool IsBlockerCreated => Blocker != null;

		public UIBlocker? Blocker { get; private set; }

		public event Action? BlockerCreated;
		public event Action? BlockerDestroyed;

		public void CreateBlocker()
		{
			if (IsBlockerCreated)
				return;

			Blocker = _overrideSortingOrder
				? UIBlocker.CreateBlocker(transform, OnBlockerClosed, _targetSortingOrder, _blockerSortingOrder)
				: UIBlocker.CreateBlocker(transform, OnBlockerClosed);

			BlockerCreated?.Invoke();
			BlockerCreatedEvent?.Invoke();
		}

		public void DestroyBlocker()
		{
			if (!IsBlockerCreated)
				return;

			Blocker?.DestroyBlocker();
			Blocker = null;
			BlockerDestroyed?.Invoke();
			BlockerDestroyedEvent?.Invoke();
		}

		private void OnEnable()
		{
			if (WillCreateBlockerOnEnable)
				CreateBlocker();
		}

		private void OnDisable()
		{
			if (WillDestroyBlockerOnDisable)
				DestroyBlocker();
		}

		private void OnBlockerClosed()
		{
			DestroyBlocker();
		}
	}
}
