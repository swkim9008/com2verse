/*===============================================================
* Product:		Com2Verse
* File Name:	UIBlocker.cs
* Developer:	jhkim
* Date:			2022-09-13 20:31
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Extension;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Com2Verse
{
	/* UI Blocker 사용방벙
	 * 생성
	 *		var blocker = UIBlocker.CreateBlocker(TargetTransform, CloseEvent);
	 *		(Blocker 터치시 삭제처리되며 CloseEvent를 호출합니다.)
	 * 삭제 (수동)
	 *		blocker.DestroyBlocker();
	 */
	public class UIBlocker
	{
#region Public Method
		public static UIBlocker CreateBlocker(Transform target, Action onCloseEvent = null, int? overrideTargetSoringOrder = null, int? overrideBlockerSortingOrder = null)
		{
			var blocker = new UIBlocker(target, onCloseEvent);
			blocker._blockerObject = blocker.CreateBlocker(overrideTargetSoringOrder, overrideBlockerSortingOrder);
			return blocker;
		}

		/// <summary>
		/// Convenience method to explicitly destroy the previously generated blocker object
		/// </summary>
		/// <remarks>
		/// Override this method to implement a different way to dispose of a blocker GameObject that blocks clicks to other controls while the dropdown list is open.
		/// </remarks>
		/// <param name="blocker">The blocker object to destroy.</param>
		public void DestroyBlocker()
		{
			if (!_blockerObject.IsReferenceNull())
			{
				if (_target.TryGetComponent<Canvas>(out var dropdownCanvas))
					dropdownCanvas.overrideSorting = false;
				Object.Destroy(_blockerObject);
				_blockerObject = null;
			}
		}
#endregion // Public Method

		private Transform _target;
		private GameObject _blockerObject;
		private Action _onCloseEvent;
		private UIBlocker(Transform target, Action onCloseEvent)
		{
			_target = target;
			_onCloseEvent = onCloseEvent;
		}

#region TMP_Dropdown
		/// <summary>
		/// Create a blocker that blocks clicks to other controls while the dropdown list is open.
		/// </summary>
		/// <remarks>
		/// Override this method to implement a different way to obtain a blocker GameObject.
		/// </remarks>
		/// <param name="rootCanvas">The root canvas the dropdown is under.</param>
		/// <returns>The created blocker object</returns>
		protected GameObject CreateBlocker(int? overrideTargetSoringOrder, int? overrideBlockerSortingOrder)
		{
			var rootCanvas = GetRootCanvas();
			var nearbyAncestorCanvas = GetNearbyAncestorCanvas();
			// Create blocker GameObject.
			GameObject blocker = new GameObject("Blocker");

			// Setup blocker RectTransform to cover entire root canvas area.
			RectTransform blockerRect = blocker.AddComponent<RectTransform>();
			blockerRect.SetParent(rootCanvas.transform, false);
			blockerRect.anchorMin = Vector3.zero;
			blockerRect.anchorMax = Vector3.one;
			blockerRect.sizeDelta = Vector2.zero;

			// Make blocker be in separate canvas in same layer as dropdown and in layer just below it.
			Canvas blockerCanvas = blocker.AddComponent<Canvas>();
			blockerCanvas.overrideSorting = true;
			Canvas dropdownCanvas = GetOrAddComponent<Canvas>(_target.gameObject);
			dropdownCanvas.overrideSorting = true;
			dropdownCanvas.sortingOrder = overrideTargetSoringOrder ?? nearbyAncestorCanvas.sortingOrder + 2;
			blockerCanvas.sortingLayerID = dropdownCanvas.sortingLayerID;
			blockerCanvas.sortingOrder = overrideBlockerSortingOrder ?? dropdownCanvas.sortingOrder - 1;

			// Add raycaster since it's needed to block.
			GetOrAddComponent<GraphicRaycaster>(blocker);

			// Add image since it's needed to block, but make it clear.
			Image blockerImage = blocker.AddComponent<Image>();
			blockerImage.color = Color.clear;

			// Add button since it's needed to block, and to close the dropdown when blocking area is clicked.
			Button blockerButton = blocker.AddComponent<Button>();
			blockerButton.onClick.AddListener(() => _onCloseEvent?.Invoke());
			return blocker;
		}

		private static T GetOrAddComponent<T>(GameObject go) where T : Component
		{
			T comp = go.GetComponent<T>();
			if (!comp)
				comp = go.AddComponent<T>();
			return comp;
		}

		private Canvas GetNearbyAncestorCanvas()
		{
			var tr = _target.parent;
			Canvas canvas = null;
			while (!tr.IsReferenceNull())
			{
				if (tr.TryGetComponent<Canvas>(out canvas))
					break;

				tr = tr.parent;
			}

			return canvas;
		}
		private Canvas GetRootCanvas()
		{
			// Get root Canvas.
			var list = ListPool<Canvas>.Get();
			_target.gameObject.GetComponentsInParent(false, list);
			if (list.Count == 0)
				return null;

			Canvas rootCanvas = list[list.Count - 1];
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].isRootCanvas)
				{
					rootCanvas = list[i];
					break;
				}
			}
			ListPool<Canvas>.Release(list);
			return rootCanvas;
		}
#endregion // TMP_Dropdown
	}
}
