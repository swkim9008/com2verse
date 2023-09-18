/*===============================================================
* Product:		Com2Verse
* File Name:	SpriteAtlasManager.cs
* Developer:	tlghks1009
* Date:			2022-05-11 16:20
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.AssetSystem;
using Com2Verse.Logger;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.U2D;

namespace Com2Verse.UI
{
	public sealed class SpriteAtlasManager : Singleton<SpriteAtlasManager>, IDisposable
	{
		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private SpriteAtlasManager() { }

		private readonly Dictionary<string, SpriteAtlasController> _spriteAtlasControllerDict = new();

		private readonly Dictionary<string, C2VAsyncOperationHandle<SpriteAtlas>> _addressableHandles = new();

		public void LoadSpriteAtlasAsync(string spriteAtlasAddress, Action<SpriteAtlasAssetOperationHandle> onCompleted, bool preloadAllSprite = false)
		{
			string assetAddressableName = $"{spriteAtlasAddress}.spriteatlasv2";

			if (_addressableHandles.ContainsKey(assetAddressableName))
			{
				onCompleted?.Invoke(null);
				return;
			}


			var assetOperationHandle = new SpriteAtlasAssetOperationHandle(assetAddressableName, preloadAllSprite);

			assetOperationHandle.OnLoadCompletedEvent += OnSpriteAtlasLoadCompleted;
			assetOperationHandle.OnLoadCompletedEvent += onCompleted;

			var handle = C2VAddressables.LoadAssetAsync<SpriteAtlas>(assetAddressableName);

			handle.OnCompleted += assetOperationHandle.OnLoadCompleted;

			_addressableHandles.Add(assetAddressableName, handle);

			//AssetSystemManager.Instance.LoadAssetAsync<SpriteAtlas>(assetAddressableName, assetOperationHandle.OnLoadCompleted);
		}


		public Sprite GetSprite(string atlasName, string spriteName)
		{
			if (!_spriteAtlasControllerDict.TryGetValue(atlasName, out var spriteAtlasController))
			{
				return null;
			}

			return spriteAtlasController.GetSprite(spriteName);
		}


		public void Destroy(string atlasName)
		{
			if (!_spriteAtlasControllerDict.TryGetValue(atlasName, out var spriteAtlasController))
			{
				return;
			}

			spriteAtlasController.Destroy();

			_addressableHandles.Remove(spriteAtlasController.Address);
			_spriteAtlasControllerDict.Remove(atlasName);
		}


		public void DestroyAll()
		{
			foreach (var spriteAtlasController in _spriteAtlasControllerDict.Values)
			{
				spriteAtlasController.Destroy();
			}

			_addressableHandles.Clear();
			_spriteAtlasControllerDict.Clear();
		}


		private void OnSpriteAtlasLoadCompleted(SpriteAtlasAssetOperationHandle handle)
		{
			var spriteAtlasController = new SpriteAtlasController(handle.SpriteAtlas, handle.Address, handle.LoadAllSprite);

			if (!_spriteAtlasControllerDict.TryAdd(handle.SpriteAtlas.name, spriteAtlasController))
			{
				C2VDebug.LogError("[SpriteAtlasManager] key overlay");
			}
		}


		public void Dispose()
		{
			DestroyAll();
		}
	}
}
