/*===============================================================
* Product:		Com2Verse
* File Name:	InputFieldDropdownUI.cs
* Developer:	wlemon
* Date:			2023-04-13 12:01
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable
using System;
using UnityEngine;
using System.Collections.Generic;
using Com2Verse.Extension;
using TMPro;
using UnityEngine.UI;

namespace Com2Verse
{
	[RequireComponent(typeof(TMP_InputField))]
	[AddComponentMenu("[CVUI]/[CVUI] InputFieldDropdownUI")]
	public sealed class InputFieldDropdownUI : MonoBehaviour
	{
		[SerializeField, HideInInspector] private TMP_InputField? _inputField;
		[SerializeField]                  private TMP_Dropdown?   _dropdown;
		[SerializeField]                  private string?         _defaultOption;
		[SerializeField]                  private List<string>?   _options;

		public string? DefaultOption
		{
			get => _defaultOption;
			set
			{
				_defaultOption = value;
				RefreshDropdown();
			}
		}

		public List<string>? Options
		{
			get => _options;
			set
			{
				_options = value;
				RefreshDropdown();
			}
		}

		private void Awake()
		{
			RefreshDropdown();
			RefreshText();
		}
		
		private void OnEnable()
		{
			RefreshText();
			if (_dropdown != null) _dropdown.onValueChanged.AddListener(OnValueChanged);
		}

		private void OnDisable()
		{
			if (_dropdown != null) _dropdown.onValueChanged.RemoveListener(OnValueChanged);
		}

		private void OnValueChanged(int index)
		{
			RefreshText();
		}
		
		private void RefreshText()
		{
			if (_dropdown != null)
			{
				if (_dropdown.value != 0)
				{
					_inputField!.text = _dropdown!.options![_dropdown.value]!.text;
				}
			}
		}

		public void RefreshDropdown()
		{
			if (_dropdown == null) return;
		
			_dropdown.ClearOptions();
			_dropdown.options?.Add(new TMP_Dropdown.OptionData(_defaultOption));
			if (_options != null) _dropdown.AddOptions(_options);
			_dropdown.RefreshShownValue();
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
