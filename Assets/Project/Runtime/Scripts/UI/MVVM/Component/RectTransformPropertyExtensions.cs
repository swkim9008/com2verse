/*===============================================================
* Product:		Com2Verse
* File Name:	RectTransformExtensions.cs
* Developer:	tlghks1009
* Date:			2022-07-18 18:55
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
using Com2Verse.Logger;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Com2Verse.UI
{
	[AddComponentMenu("[DB]/[DB] RectTransformPropertyExtensions")]
	public sealed class RectTransformPropertyExtensions : UIBehaviour
	{
		private RectTransform _rectTransform;
		[HideInInspector] public UnityEvent<float> _onChangeWidth;
		[HideInInspector] public UnityEvent<float> _onChangeHeight;
		[HideInInspector] public UnityEvent<float> _onChangePosX;
		[HideInInspector] public UnityEvent<float> _onChangePosY;
		[HideInInspector] public UnityEvent<float> _onChangeScaleX;
		[HideInInspector] public UnityEvent<float> _onChangeScaleY;

		private float _width;
		private float _height;
		private float _posX;
		private float _posY;
		private float _scaleX;
		private float _scaleY;

		[UsedImplicitly]
		public RectTransform RectTransform
		{
			get
			{
				if (_rectTransform.IsReferenceNull())
					FindRectTransform();
				return _rectTransform;
			}
			// ReSharper disable once ValueParameterNotUsed
			set => C2VDebug.LogWarningCategory(GetType().Name, "RectTransform is read only");
		}

		[UsedImplicitly]
		public Vector3 Position
		{
			get
			{
				CheckRectTransform();
				return _rectTransform.position;
			}
			set
			{
				CheckRectTransform();
				_rectTransform.position = value;
			}
		}

		[UsedImplicitly]
		public Quaternion Rotation
		{
			get
			{
				CheckRectTransform();
				return _rectTransform.rotation;
			}
			set
			{
				CheckRectTransform();
				_rectTransform.rotation = value;
			}
		}

		[UsedImplicitly]
		public Vector3 Scale
		{
			get
			{
				CheckRectTransform();
				return _rectTransform.localScale;
			}
			set
			{
				CheckRectTransform();
				_rectTransform.localScale = value;
			}
		}

		[UsedImplicitly]
		public float RotationX
		{
			get => Rotation.x;
			set
			{
				var angle = Rotation.eulerAngles;
				angle.x = value;
				Rotation = Quaternion.Euler(angle);
			}
		}

		[UsedImplicitly]
		public float RotationY
		{
			get => Rotation.y;
			set
			{
				var angle = Rotation.eulerAngles;
				angle.y = value;
				Rotation = Quaternion.Euler(angle);
			}
		}

		[UsedImplicitly]
		public float RotationZ
		{
			get => Rotation.z;
			set
			{
				var angle = Rotation.eulerAngles;
				angle.z = value;
				Rotation = Quaternion.Euler(angle);
			}
		}

		[UsedImplicitly]
		public float Width
		{
			get => _width;
			set
			{
				_width = value;
				_onChangeWidth.Invoke(_width);
			}
		}

		[UsedImplicitly]
		public float SetWidth
		{
			get => _width;
			set
			{
				CheckRectTransform();
				_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value);
			}
		}
		[UsedImplicitly]
		public float Height
		{
			get => _height;
			set
			{
				_height = value;
				_onChangeHeight.Invoke(_height);
			}
		}

		[UsedImplicitly]
		public float PosX
		{
			get => _posX;
			set
			{
				_posX = value;
				CheckRectTransform();
				_rectTransform.anchoredPosition = new Vector2(_posX, _rectTransform.anchoredPosition.y);
				_onChangePosX?.Invoke(_posX);
			}
		}

		[UsedImplicitly]
		public float PosY
		{
			get => _posY;
			set
			{
				_posY = value;
				CheckRectTransform();
				_rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, _posY );
				_onChangePosY?.Invoke(_posY);
			}
		}

		[UsedImplicitly]
		public float ScaleX
		{
			get => _scaleX;
			set
			{
				_scaleX = value;
				CheckRectTransform();
				_rectTransform.localScale = new Vector3(_scaleX, _rectTransform.localScale.y, _rectTransform.localScale.z);
				_onChangeScaleX?.Invoke(_scaleX);
			}
		}

		[UsedImplicitly]
		public float ScaleY
		{
			get => _scaleY;
			set
			{
				_scaleY = value;
				CheckRectTransform();
				_rectTransform.localScale = new Vector3(_rectTransform.localScale.x, _scaleY, _rectTransform.localScale.z);
				_onChangeScaleY?.Invoke(_scaleY);
			}
		}

		[UsedImplicitly]
		public int SetSibling
		{
			get => transform.GetSiblingIndex();
			set => transform.SetSiblingIndex(value);
		}

		[UsedImplicitly]
		public float SetHeight
		{
			get => default;
			set
			{
				CheckRectTransform();
				_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
			}
		}

		private void CheckRectTransform()
		{
			if (_rectTransform.IsReferenceNull())
				FindRectTransform();
		}

		private void FindRectTransform()
		{
			_rectTransform = GetComponent<RectTransform>();
		}

		protected override void OnRectTransformDimensionsChange()
		{
			base.OnRectTransformDimensionsChange();
			CheckRectTransform();

			var size = _rectTransform.rect.size;
			Width  = size.x;
			Height = size.y;
		}
	}
}
