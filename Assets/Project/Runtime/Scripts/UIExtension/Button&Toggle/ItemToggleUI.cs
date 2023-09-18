/*===============================================================
* Product:		Com2Verse
* File Name:	ItemToggleUI.cs
* Developer:	urun4m0r1
* Date:			2022-04-14 20:42
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Com2Verse.UI
{
	[ExecuteAlways]
	[RequireComponent(typeof(Toggle))]
	[AddComponentMenu("[CVUI]/[CVUI] ItemToggleUI")]
	public class ItemToggleUI : MonoBehaviour
	{
		[SerializeField, ReadOnly] private Toggle _toggle = null!;

		[SerializeField] private GameObject? _onItem;
		[SerializeField] private GameObject? _offItem;

		// delegate caches
		private UnityAction<bool> _toggleHandler = null!;

		private void OnEnable()
		{
			if (_toggle == null) throw new NullReferenceException(name);

			CacheDelegates();

			_toggle.onValueChanged?.AddListener(_toggleHandler);
			ChangeItem(_toggle.isOn);
		}

		private void CacheDelegates()
		{
			_toggleHandler = ChangeItem;
		}

		private void OnDisable()
		{
			if (_toggle != null) _toggle.onValueChanged?.RemoveListener(_toggleHandler);
		}

		private void ChangeItem(bool isOn)
		{
			if (_onItem  != null) _onItem.SetActive(isOn);
			if (_offItem != null) _offItem.SetActive(!isOn);
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (Application.isPlaying) return;

			if (_toggle == null)
				if (!TryGetComponent(out _toggle))
					throw new NullReferenceException(nameof(_toggle));

			if (_toggle == null) return;

			ChangeItem(_toggle.isOn);
		}
#endif // UNITY_EDITOR
	}
}
