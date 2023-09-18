/*===============================================================
* Product:		Com2Verse
* File Name:	ImagePropertyExtensions.cs
* Developer:	tlghks1009
* Date:			2022-07-15 17:24
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.AssetSystem;
using Com2Verse.Extension;
using Com2Verse.Logger;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse.UI
{
	[RequireComponent(typeof(Image))]
	public sealed class ImagePropertyExtensions : MonoBehaviour
	{
		private Image?  _image;
		private string? _spriteName;

		[UsedImplicitly]
		public Sprite? Sprite
		{
			get => _image.IsUnityNull() || _image!.sprite.IsUnityNull() ? null : _image!.sprite;
			set
			{
				if (_image.IsReferenceNull())
					FindImageComponent();

				_image!.sprite = value;
				_image.SetNativeSize();
			}
		}

		[UsedImplicitly]
		public Sprite? SpriteWithoutNativeSize
		{
			get => _image.IsUnityNull() || _image!.sprite.IsUnityNull() ? null : _image!.sprite;
			set
			{
				if (_image.IsReferenceNull())
					FindImageComponent();

				_image!.sprite = value;
			}
		}

		[UsedImplicitly]
		public string? SpriteName
		{
			get => _spriteName;
			set
			{
				if (_image.IsReferenceNull())
					FindImageComponent();

				if (string.IsNullOrEmpty(value)) return;
				
				_spriteName = value;
				if (!C2VAddressables.AddressableResourceExists<Sprite>(_spriteName))
				{
					C2VDebug.LogErrorCategory("ImagePropertyExtensions", $"Image {_spriteName} is doesn't exist asset!");
					return;
				}
				C2VAddressables.LoadAssetAsync<Sprite>(_spriteName).OnCompleted += (handle) =>
				{
					var loadedAsset = handle.Result;
					if (loadedAsset.IsUnityNull()) return;
					_image!.sprite = loadedAsset;
					_image.SetNativeSize();
					handle.Release();
				};
			}
		}

		private void FindImageComponent()
		{
			_image = GetComponent<Image>();
		}
	}
}
