#if ENABLE_CHEATING

/*===============================================================
 * Product:		Com2Verse
 * File Name:	RandomTextureProvider.cs
 * Developer:	urun4m0r1
 * Date:		2023-03-17 15:23
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using Com2Verse.Extension;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Com2Verse.Communication.Cheat
{
	public class RandomTextureProvider : BaseModule, IVideoTextureProvider
	{
#region IVideoTextureProvider
		public event Action<Texture?>? TextureChanged;

		private Texture? _texture;

		public Texture? Texture
		{
			get => _texture;
			private set
			{
				var prevValue = _texture;
				if (prevValue == value)
					return;

				_texture = value;
				TextureChanged?.Invoke(value);
			}
		}
#endregion // IVideoTextureProvider

		private static readonly float RunningRate           = 0.8f;
		private static readonly int   StateUpdateInterval   = 1000;
		private static readonly int   TextureUpdateInterval = 1000;

		private static readonly List<Texture2D> RandomTextures = new()
		{
			Texture2D.blackTexture,
			Texture2D.whiteTexture,
			Texture2D.grayTexture,
			Texture2D.redTexture,
			Texture2D.normalTexture,
			Texture2D.linearGrayTexture,
		};

		private bool _randomState;

		private readonly CancellationTokenSource _stateUpdateToken = new();

		public RandomTextureProvider()
		{
			StartApplyRandomState().Forget();
			StartApplyRandomTexture().Forget();
		}

		private async UniTaskVoid StartApplyRandomState()
		{
			while (await UniTaskHelper.Delay(StateUpdateInterval, _stateUpdateToken))
				_randomState = Random.Range(0f, 1f) <= RunningRate;
		}

		private async UniTaskVoid StartApplyRandomTexture()
		{
			while (await UniTaskHelper.Delay(TextureUpdateInterval, _stateUpdateToken))
			{
				if (IsRunning && _randomState)
				{
					Texture = GetRandomTexture();
				}
				else
				{
					Texture = null;
				}
			}
		}

		private static Texture2D GetRandomTexture()
		{
			var randomTextureIndex = Random.Range(0, RandomTextures.Count);
			RandomTextures.TryGetAt(randomTextureIndex, out var texture);
			return texture!;
		}

#region IDisposable
		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				_stateUpdateToken.Cancel();
				_stateUpdateToken.Dispose();
			}

			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}

#endif // ENABLE_CHEATING
