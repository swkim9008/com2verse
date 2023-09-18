/*===============================================================
* Product:		Com2Verse
* File Name:	AssetOperationHandle.cs
* Developer:	tlghks1009
* Date:			2022-05-12 14:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Diagnostics.CodeAnalysis;
using Com2Verse.AssetSystem;
using UnityEngine.U2D;

namespace Com2Verse.UI
{
	public class SpriteAtlasAssetOperationHandle
	{
		private Action<SpriteAtlasAssetOperationHandle> _onLoadCompletedEvent;
		public event Action<SpriteAtlasAssetOperationHandle> OnLoadCompletedEvent
		{
			add
			{
				_onLoadCompletedEvent -= value;
				_onLoadCompletedEvent += value;
			}
			remove => _onLoadCompletedEvent -= value;
		}

		private SpriteAtlas _spriteAtlas;
		public SpriteAtlas SpriteAtlas => _spriteAtlas;

		[NotNull] private readonly string _address;
		[NotNull] public string Address => _address;


		private bool _loadAllSprite;
		public bool LoadAllSprite => _loadAllSprite;

		public SpriteAtlasAssetOperationHandle([NotNull] string address, bool loadAllSprite)
		{
			_address = address;
			_loadAllSprite = loadAllSprite;
		}

		public void OnLoadCompleted(C2VAsyncOperationHandle<SpriteAtlas> handle)
		{
			_spriteAtlas = handle.Result;

			_onLoadCompletedEvent?.Invoke(this);

			_onLoadCompletedEvent = null;
		}
	}
}
