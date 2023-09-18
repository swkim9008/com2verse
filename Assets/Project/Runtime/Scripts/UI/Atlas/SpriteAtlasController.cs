/*===============================================================
* Product:		Com2Verse
* File Name:	SpriteAtlasController.cs
* Developer:	tlghks1009
* Date:			2022-05-11 16:20
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.AssetSystem;
using Com2Verse.Logger;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.U2D;

namespace Com2Verse.UI
{
	public sealed class SpriteAtlasController
	{
		private readonly Dictionary<string, Sprite> _spriteDict;
		public           Dictionary<string, Sprite> Sprites => _spriteDict;

		private SpriteAtlas _spriteAtlas;

		[NotNull] private readonly string _address;
		[NotNull] public string Address => _address;

		public SpriteAtlasController(SpriteAtlas spriteAtlas, [NotNull] string address, bool loadAllSprite = false)
		{
			_spriteAtlas = spriteAtlas;
			_address = address;
			_spriteDict = new Dictionary<string, Sprite>();

			if (loadAllSprite)
			{
				Sprite[] sprites = new Sprite[_spriteAtlas.spriteCount];

				spriteAtlas.GetSprites(sprites);

				foreach (var sprite in sprites)
				{
					var spriteOriginalName = sprite.name.Substring(0, sprite.name.Length - 7);

					_spriteDict.Add(spriteOriginalName, sprite);
				}
			}
		}


		public Sprite GetSprite(string spriteName)
		{
			if (_spriteDict.TryGetValue(spriteName, out var sprite))
			{
				return sprite;
			}

			var spriteClone = _spriteAtlas.GetSprite(spriteName);

			if (ReferenceEquals(spriteClone, null))
			{
				C2VDebug.LogWarning($"[SpriteAtlasController] Can't find sprite. spriteName : {spriteName}");
				return null;
			}

			_spriteDict.Add(spriteName, spriteClone);

			return spriteClone;
		}

		public void Destroy()
		{
			foreach (var sprite in _spriteDict.Values)
				Object.Destroy(sprite);

			_spriteDict.Clear();

			if (AssetSystemManager.InstanceExists)
			{
				AssetSystemManager.Instance.ReleaseAssetAddressableName(_address);
			}

			_spriteAtlas = null;
		}
	}
}
