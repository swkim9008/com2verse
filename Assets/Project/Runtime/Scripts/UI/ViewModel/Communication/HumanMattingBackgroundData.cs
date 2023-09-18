/*===============================================================
 * Product:		Com2Verse
 * File Name:	HumanMattingBackgroundData.cs
 * Developer:	urun4m0r1
 * Date:		2023-06-28 12:42
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.AssetSystem;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.UI
{
	public sealed class HumanMattingBackgroundData : IDisposable
	{
		public HumanMattingBackgroundData(string? sdBackgroundPath, string? hdBackgroundPath)
		{
			SdBackgroundPath = sdBackgroundPath;
			HdBackgroundPath = hdBackgroundPath;
		}

		public string? SdBackgroundPath { get; }
		public string? HdBackgroundPath { get; }

		private C2VAsyncOperationHandle<Texture2D>? _sdBackgroundHandle;
		private C2VAsyncOperationHandle<Texture2D>? _hdBackgroundHandle;

		public async UniTask<Texture2D?> LoadSdBackgroundTextureAsync()
		{
			if (string.IsNullOrEmpty(SdBackgroundPath!))
				return null;

			_sdBackgroundHandle = C2VAddressables.LoadAssetAsync<Texture2D>(SdBackgroundPath);
			if (_sdBackgroundHandle == null)
				return null;

			return await _sdBackgroundHandle.ToUniTask();
		}

		public async UniTask<Texture2D?> LoadHdBackgroundTextureAsync()
		{
			if (string.IsNullOrEmpty(HdBackgroundPath!))
				return null;

			_hdBackgroundHandle = C2VAddressables.LoadAssetAsync<Texture2D>(HdBackgroundPath);
			if (_hdBackgroundHandle == null)
				return null;

			return await _hdBackgroundHandle.ToUniTask();
		}

		public void UnloadBackgroundTexture()
		{
			if (_sdBackgroundHandle != null) C2VAddressables.Release(_sdBackgroundHandle);
			if (_hdBackgroundHandle != null) C2VAddressables.Release(_hdBackgroundHandle);
		}

		public void Dispose()
		{
			UnloadBackgroundTexture();
		}
	}
}
