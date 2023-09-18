/*===============================================================
* Product:		Com2Verse
* File Name:	RawImageController.cs
* Developer:	urun4m0r1
* Date:			2022-08-19 17:05
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse.UI
{
	/// <summary>
	/// RawImage에 On / Off 기능 및 별도 텍스쳐 표시.<br/>
	/// 자동 Aspect Ratio 조정 등 ViewModel 을 통한 제어 기능을 추가할때 사용하는 컴포넌트.
	/// </summary>
	[RequireComponent(typeof(RawImage))]
	[RequireComponent(typeof(AspectRatioFitter))]
	[AddComponentMenu("[CVUI]/[CVUI] Raw Image Controller")]
	public class RawImageController : MonoBehaviour
	{
		[Serializable]
		private struct RawImageContent
		{
			[field: SerializeField] public Texture? Texture { get; private set; }
			[field: SerializeField] public Color    Color   { get; private set; }

			public RawImageContent(Texture? texture, Color color)
			{
				Texture = texture;
				Color   = color;
			}
		}

#region InspectorFields
		[Header("Default Settings")]
		[SerializeField] private RawImageContent _defaultContent;

		[Header("Image Settings")]
		[SerializeField] private bool _isVisible = true;
		[SerializeField] private Texture? _texture;
		[SerializeField] private Texture? _fallbackTexture;

		[Header("UV Settings")]
		[SerializeField] private bool _isHorizontalFlipped;
		[SerializeField] private bool _isVerticalFlipped;

		[Header("Debug Info")]
		[SerializeField, ReadOnly] private RawImage? _rawImage;
		[SerializeField, ReadOnly] private AspectRatioFitter? _aspectRatioFitter;
#endregion // InspectorFields

#region ViewModelProperties
		[UsedImplicitly] // Setter used by view model.
		public bool IsVisibleReversed
		{
			get => !_isVisible;
			set => IsVisible = !value;
		}

		[UsedImplicitly] // Setter used by view model.
		public bool IsVisible
		{
			get => _isVisible;
			set
			{
				_isVisible = value;
				OnTextureStateChanged();
			}
		}

		[UsedImplicitly] // Setter used by view model.
		public Texture? Texture
		{
			get => _texture;
			set
			{
				_texture = value;
				OnTextureStateChanged();
			}
		}

		[UsedImplicitly] // Setter used by view model.
		public Texture? FallbackTexture
		{
			get => _fallbackTexture;
			set
			{
				_fallbackTexture = value;
				OnTextureStateChanged();
			}
		}

		[UsedImplicitly] // Setter used by view model.
		public bool IsHorizontalFlipped
		{
			get => _isHorizontalFlipped;
			set
			{
				_isHorizontalFlipped = value;
				OnTextureStateChanged();
			}
		}

		[UsedImplicitly] // Setter used by view model.
		public bool IsVerticalFlipped
		{
			get => _isVerticalFlipped;
			set
			{
				_isVerticalFlipped = value;
				OnTextureStateChanged();
			}
		}

		/// <summary>
		/// 상태를 초기화하고 이벤트를 발생시킵니다.
		/// </summary>
		public void Clear()
		{
			_isVisible       = false;
			_texture         = null;
			_fallbackTexture = null;

			_isHorizontalFlipped = false;
			_isVerticalFlipped   = false;

			OnTextureStateChanged();
		}
#endregion // ViewModelProperties

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (!Application.isPlaying)
				TryFindComponentReference();

			OnTextureStateChanged();
		}
#endif // UNITY_EDITOR

		private void Awake()
		{
			TryFindComponentReference();
		}

		private void TryFindComponentReference()
		{
			if (_rawImage.IsUnityNull())
				if (!TryGetComponent(out _rawImage))
					throw new NullReferenceException(nameof(_rawImage));

			if (_aspectRatioFitter.IsUnityNull())
				if (!TryGetComponent(out _aspectRatioFitter))
					throw new NullReferenceException(nameof(_rawImage));
		}

		private void OnEnable()
		{
			OnTextureStateChanged();
		}

		private void OnTextureStateChanged()
		{
			var rawImageContent = GetRawImageContent();
			UpdateRawImage(rawImageContent);

			var aspectRatio = GetAspectRatio(rawImageContent.Texture);
			UpdateAspectRatio(aspectRatio);

			UpdateUVFlip();
		}

		private RawImageContent GetRawImageContent()
		{
			Texture? texture = null;
			Color    color   = Color.white;

			if (isActiveAndEnabled && IsVisible && !_texture.IsUnityNull())
			{
				texture = _texture;
			}
			else
			{
				if (!_fallbackTexture.IsUnityNull())
					texture = _fallbackTexture;
				else if (!_defaultContent.Texture.IsUnityNull())
					texture = _defaultContent.Texture;
				else
					color = _defaultContent.Color;
			}

			return new RawImageContent(texture, color);
		}

		private static float GetAspectRatio(Texture? texture)
		{
			var aspectRatio = 1f;
			if (!texture.IsUnityNull())
			{
				aspectRatio = texture!.width / (float)texture!.height;
			}

			return aspectRatio;
		}

		private void UpdateRawImage(RawImageContent rawImageContent)
		{
			if (_rawImage.IsUnityNull())
			{
				C2VDebug.LogWarningCategory(nameof(RawImageController), "RawImage is null.");
				return;
			}

			_rawImage!.texture = rawImageContent.Texture;
			_rawImage!.color   = rawImageContent.Color;
		}

		private void UpdateAspectRatio(float aspectRatio)
		{
			if (_aspectRatioFitter.IsUnityNull())
			{
				C2VDebug.LogWarningCategory(nameof(RawImageController), "AspectRatioFitter is null.");
				return;
			}

			_aspectRatioFitter!.aspectRatio = aspectRatio;
		}

		private void UpdateUVFlip()
		{
			transform.FlipHorizontal(_isHorizontalFlipped);
			transform.FlipVertical(_isVerticalFlipped);
		}
	}
}
