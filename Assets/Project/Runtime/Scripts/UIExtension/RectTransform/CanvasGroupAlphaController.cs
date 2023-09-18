/*===============================================================
 * Product:		Com2Verse
 * File Name:	CanvasGroupAlphaController.cs
 * Developer:	urun4m0r1
 * Date:		2023-05-15 15:36
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Extension;
using Com2Verse.Logger;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace Com2Verse.UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class CanvasGroupAlphaController : MonoBehaviour
	{
		[SerializeField] private CanvasGroup? _canvasGroup;

		private void Awake()
		{
			if (_canvasGroup.IsReferenceNull())
				_canvasGroup = GetComponent<CanvasGroup>();

			if (_canvasGroup.IsUnityNull())
			{
				C2VDebug.LogErrorMethod(nameof(CanvasGroupAlphaController), "CanvasGroup is null");
				return;
			}
		}

		private void Start()
		{
			OnValueChanged(_value, _value);
			_valueChanged?.Invoke(_value);
		}

		[SerializeField]
		private float _minAlpha = 0;

		[SerializeField]
		private float _maxAlpha = 1;

		[SerializeField]
		private float _value;

		[SerializeField]
		private UnityEvent<float>? _valueChanged;

		[UsedImplicitly]
		public float Value
		{
			get => _value;
			set
			{
				var prevValue = _value;
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (prevValue == value)
					return;

				_value = value;
				OnValueChanged(prevValue, value);
				_valueChanged?.Invoke(value);
			}
		}

		private void OnValueChanged(float prevValue, float value)
		{
			_canvasGroup!.alpha = Mathf.Lerp(_minAlpha, _maxAlpha, value);
		}
	}
}
