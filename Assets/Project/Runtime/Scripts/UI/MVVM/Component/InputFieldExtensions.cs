/*===============================================================
* Product:		Com2Verse
* File Name:	InputFieldExtensions.cs
* Developer:	jhkim
* Date:			2022-09-07 10:02
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Text;
using Com2Verse.Extension;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Com2Verse.UI
{
	[AddComponentMenu("[DB]/[DB] InputFieldExtensions")]
	[RequireComponent(typeof(TMP_InputField))]
	public sealed class InputFieldExtensions : MonoBehaviour
	{
		public TMP_InputField InputField { get => _inputField; set => _inputField = value; }
		
		private TMP_InputField _inputField;
		private string _prevString;
		private static readonly StringBuilder Sb = new();

		private static readonly List<TMP_InputField> TotalInputFields = new();

		/// <summary>
		/// 전체 InputField 포커스 조사
		/// </summary>
		public static bool IsExistFocused => TotalInputFields!.Exists((input) => input!.isFocused);

		[HideInInspector] public UnityEvent<string> _onValueChangedEvent;
		[HideInInspector] public UnityEvent<bool> _onFocusChangedEvent;

		public string Text { get; set; } = string.Empty;

		private bool _isFocused = false;
		public bool IsFocused
		{
			get => _isFocused;
			set
			{
				_isFocused = value;
				if (_inputField.IsReferenceNull()) return;
				if (_isFocused)
					_inputField.OnSelect(null);
				else
					_inputField.OnDeselect(null);
			}
		}

		public bool ClearText
		{
			get => false;
			set => _inputField.text = string.Empty;
		}

		public bool ShowPlaceHolder
		{
			get => _inputField.placeholder.enabled;
			set
			{
				_inputField.placeholder.enabled = value;
				_inputField.placeholder.gameObject.SetActive(value);
			}
		}

		private void Awake()
		{
			_inputField = GetComponent<TMP_InputField>();
			_prevString = string.Empty;
			_isFocused = false;
			TotalInputFields.TryAdd(_inputField);
		}

		private void OnDestroy()
		{
			if (TotalInputFields.Contains(_inputField))
				TotalInputFields.Remove(_inputField);
			_inputField = null;
		}

		private void Update()
		{
			UpdateIMECheck();
			UpdateFocusCheck();
		}

		private void UpdateIMECheck()
		{
			if (Input.imeCompositionMode == IMECompositionMode.Off) return;
			if (!_inputField.isActiveAndEnabled || !_inputField.isFocused) return;

			var currentString = GetCurrentString();
			if (_prevString.Equals(currentString)) return;

			_prevString = currentString;
			Text = currentString;
			_onValueChangedEvent?.Invoke(Text);

			string GetCurrentString()
			{
				var composition = Input.compositionString;
				var idx = _inputField.caretPosition - 1;
				var text = _inputField.text;

				if (idx < 0)
					return text;
				if (idx >= text.Length)
					return $"{text}{composition}";

				var span = text.AsSpan();
				var left = span.Slice(0, idx);
				var right = span.Slice(idx, text.Length - idx);
				Sb.Clear();
				Sb.Append(left);
				Sb.Append(composition);
				Sb.Append(right);
				return Sb.ToString();
			}
		}

		private void UpdateFocusCheck()
		{
			bool prevState = _isFocused;
			_isFocused = _inputField.isFocused;
			if (_isFocused != prevState)
				_onFocusChangedEvent?.Invoke(IsFocused);
		}
	}
}
