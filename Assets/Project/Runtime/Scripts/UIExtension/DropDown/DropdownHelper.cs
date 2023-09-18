/*===============================================================
* Product:		Com2Verse
* File Name:	DropdownHelper.cs
* Developer:	urun4m0r1
* Date:			2022-10-16 17:30
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.Utils;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Com2Verse.UI
{
	[RequireComponent(typeof(TMP_Dropdown))]
	[AddComponentMenu("[CVUI]/[CVUI] DropdownHelper")]
	public class DropdownHelper : MonoBehaviour, ILocalizationUI
	{
		[Header("References")]
		[SerializeField, ReadOnly] private TMP_Dropdown _dropdown = null!;

		[Header("Options")]
		[Tooltip("Use dropdown first item as default value like \"Select...\"")]
		[SerializeField] private bool _useDefaultOption;
		[SerializeField] private string?                 _defaultOptionLocalizationKey;
		[SerializeField] private TMP_Dropdown.OptionData _defaultOption = new("Select...");

		[Header("Events")]
		[SerializeField] private UnityEvent<int>? _onOptionSelected;

#region OnDropdownValueChanged
		protected virtual void Awake()
		{
			(this as ILocalizationUI).InitializeLocalization();
			_dropdown.onValueChanged?.AddListener(OnDropdownChanged);
			_defaultOption.text = Localization.Instance.GetString(_defaultOptionLocalizationKey!);
		}

		protected virtual void OnDestroy()
		{
			(this as ILocalizationUI).ReleaseLocalization();
			_dropdown.onValueChanged?.RemoveListener(OnDropdownChanged);
			Options = null;
		}

		// delegate caches
		private UnityAction<int>? _onDropdownValueChanged;
		private UnityAction<int>  OnDropdownChanged => _onDropdownValueChanged ??= _ => OnDropdownValueChanged();

		private void OnDropdownValueChanged()
		{
			_onOptionSelected?.Invoke(CurrentIndex);
		}
#endregion // OnDropdownValueChanged

#region Options
		private List<string>? _options;

		[UsedImplicitly] // Setter used by view model.
		public List<string>? Options
		{
			get => _options;
			set => InitializeDropdown(value);
		}

		public void OnLanguageChanged()
		{
			_defaultOption.text = Localization.Instance.GetString(_defaultOptionLocalizationKey!);
			InitializeDropdown(_options, CurrentIndex);
		}

		public void InitializeDropdown(List<string>? dropdownList, int initialIndex = -1)
		{
			_options = dropdownList;

			_dropdown.ClearOptions();

			if (_useDefaultOption || dropdownList == null || dropdownList.Count == 0)
			{
				_dropdown.options?.Add(_defaultOption);
			}

			if (dropdownList != null)
				_dropdown.AddOptions(dropdownList);

			RefreshDropdownShownValues(initialIndex);
		}

		private void OnDropdownListChanged()
		{
			_dropdown.RefreshShownValue();

			var wasExpanded = _dropdown.IsExpanded;
			_dropdown.Hide();
			if (wasExpanded) _dropdown.Show();
		}
#endregion // Options

#region CurrentIndex
		[UsedImplicitly] // Setter used by view model.
		public int CurrentIndex
		{
			get => _dropdown.value - (_useDefaultOption ? 1 : 0);
			set => RefreshDropdownShownValues(value);
		}

		private void RefreshDropdownShownValues(int value)
		{
			if (_dropdown.options?.Count <= 1 || value < 0)
				_dropdown.value = 0;
			else
				_dropdown.value = value + (_useDefaultOption ? 1 : 0);

			OnDropdownListChanged();
		}
#endregion // CurrentIndex

#if UNITY_EDITOR
		protected virtual void OnValidate()
		{
			if (Application.isPlaying) return;

			if (_dropdown == null)
				if (!TryGetComponent(out _dropdown))
					throw new NullReferenceException(nameof(_dropdown));

			if (_dropdown != null)
			{
				_dropdown.ClearOptions();
				_dropdown.options?.Add(_defaultOption);
				_dropdown.RefreshShownValue();
			}
		}
#endif // UNITY_EDITOR
	}
}
