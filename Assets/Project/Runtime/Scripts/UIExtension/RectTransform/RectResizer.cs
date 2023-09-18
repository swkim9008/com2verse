/*===============================================================
 * Product:		Com2Verse
 * File Name:	RectResizer.cs
 * Developer:	urun4m0r1
 * Date:		2023-05-15 15:36
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace Com2Verse.UI
{
	[RequireComponent(typeof(RectTransform))]
	public class RectResizer : MonoBehaviour
	{
		private void Start()
		{
			OnValueChanged(_value, _value);
			_valueChanged?.Invoke(_value);
		}

		private RectTransform RectTransform => (transform as RectTransform)!;

		[SerializeField]
		private Vector2Int _minSize = new(0, 0);

		[SerializeField]
		private Vector2Int _maxSize = new(0, 0);

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
			var sizeX = Mathf.Lerp(_minSize.x, _maxSize.x, value);
			var sizeY = Mathf.Lerp(_minSize.y, _maxSize.y, value);

			RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sizeX);
			RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   sizeY);
		}
	}
}
