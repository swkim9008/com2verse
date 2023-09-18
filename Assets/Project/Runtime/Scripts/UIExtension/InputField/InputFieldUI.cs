/*===============================================================
* Product:		Com2Verse
* File Name:	InputFieldUI.cs
* Developer:	wlemon
* Date:			2023-04-12 14:35
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable
using System;
using Com2Verse.Utils;
using TMPro;
using UnityEngine;

namespace Com2Verse.UI
{
	[ExecuteAlways]
	[RequireComponent(typeof(TMP_InputField))]
	[AddComponentMenu("[CVUI]/[CVUI] InputFieldUI")]
	public sealed class InputFieldUI : MonoBehaviour
	{
		
		[SerializeField, ReadOnly] private TMP_InputField? _inputField;
		[SerializeField]           private GameObject?     _onSelectItem;
		[SerializeField]           private GameObject?     _onWarningItem;
		
		
		private bool              _isWarning           = false;
		
		public bool IsWarning
		{
			get => _isWarning;
			set => _isWarning = value;
		}

		public bool IsWarningReverse
		{
			get => !_isWarning;
			set => _isWarning = !value;
			
		}

		public bool IsWarningCheckImmediatly
		{
			get => false;
			set
			{
				if (value && _onWarningItem != null) _onWarningItem.gameObject.SetActive(IsWarning);
			}
		}

		private void OnEnable()
		{
			if (_inputField == null) throw new NullReferenceException(name);

			_inputField.onSelect.AddListener(OnSelect);
			_inputField.onDeselect.AddListener(OnDeselect);

			if (_onSelectItem  != null) _onSelectItem.gameObject.SetActive(false);
			if (_onWarningItem != null) _onWarningItem.gameObject.SetActive(false);
		}
		
		private void OnDisable()
		{
			if (_inputField == null) throw new NullReferenceException(name);

			_inputField.onSelect.RemoveListener(OnSelect);
			_inputField.onDeselect.RemoveListener(OnDeselect);
		}

		private void OnSelect(string value)
		{
			if (_onSelectItem  != null) _onSelectItem.gameObject.SetActive(true);
			if (_onWarningItem != null) _onWarningItem.gameObject.SetActive(false);
		}

		private void OnDeselect(string value)
		{
			if (_onSelectItem  != null) _onSelectItem.gameObject.SetActive(false);
			if (_onWarningItem != null) _onWarningItem.gameObject.SetActive(IsWarning);
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (Application.isPlaying) return;

			if (_inputField == null)
				if (!TryGetComponent(out _inputField))
					throw new NullReferenceException(nameof(_inputField));

			if (_inputField == null) return;

		}
#endif // UNITY_EDITOR
	}
}
