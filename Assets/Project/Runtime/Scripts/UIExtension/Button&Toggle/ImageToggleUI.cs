/*===============================================================
* Product:		Com2Verse
* File Name:	ImageToggleUI.cs
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
	[AddComponentMenu("[CVUI]/[CVUI] ImageToggleUI")]
	public class ImageToggleUI : MonoBehaviour
	{
		[SerializeField, ReadOnly] private Toggle _toggle = null!;
		[SerializeField, ReadOnly] private Image  _image  = null!;

		[SerializeField] private Sprite? _onSprite;
		[SerializeField] private Sprite? _offSprite;

		// delegate caches
		private UnityAction<bool> _toggleHandler = null!;

		private void OnEnable()
		{
			if (_toggle == null || _image == null) throw new NullReferenceException(name);

			CacheDelegates();

			_toggle.onValueChanged?.AddListener(_toggleHandler);
			ChangeSprite(_toggle.isOn);
		}

		private void CacheDelegates()
		{
			_toggleHandler = ChangeSprite;
		}

		private void OnDisable()
		{
			if (_toggle != null) _toggle.onValueChanged?.RemoveListener(_toggleHandler);
		}

		private void ChangeSprite(bool isOn)
		{
			var sprite        = isOn ? _onSprite : _offSprite;
			var isSpriteExist = sprite != null;

			_image.enabled = isSpriteExist;
			_image.sprite  = sprite;
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (Application.isPlaying) return;

			if (_toggle == null)
				if (!TryGetComponent(out _toggle))
					throw new NullReferenceException(nameof(_toggle));

			if (_toggle == null) return;

			var image = _toggle.image;
			if (image == null)
				throw new NullReferenceException(nameof(_image));

			_image = image;

			_toggle.graphic = null;
			ChangeSprite(_toggle.isOn);
		}
#endif // UNITY_EDITOR
	}
}
