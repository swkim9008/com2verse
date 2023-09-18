/*===============================================================
* Product:		Com2Verse
* File Name:	Video.cs
* Developer:	urun4m0r1
* Date:			2022-08-24 11:35
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Com2Verse.Extension;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.Communication
{
	public class Video : IVideoTextureProvider, IDisposable
	{
#region Decorator
		public bool IsRunning
		{
			get => Input.IsRunning;
			set => Input.IsRunning = value;
		}

		public event Action<bool>? StateChanged
		{
			add => Input.StateChanged += value;
			remove => Input.StateChanged -= value;
		}
#endregion // Decorator

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
				UniTaskHelper.InvokeOnMainThread(() => TextureChanged?.Invoke(value)).Forget();
			}
		}

		public IVideoTextureProvider Input { get; }

		private readonly IEnumerable<IVideoTexturePipeline>? _texturePipelines;

		public Video(IVideoTextureProvider input, IEnumerable<IVideoTexturePipeline>? texturePipelines = null)
		{
			Input = input;

			_texturePipelines = texturePipelines;

			if (_texturePipelines != null)
			{
				var target = Input;
				foreach (var pipeline in _texturePipelines)
				{
					pipeline.Target = target;
					target          = pipeline;
				}

				target.TextureChanged += OnInputTextureChanged;
			}

			Input.TextureChanged += OnInputTextureChanged;
		}

		public void Dispose()
		{
			if (_texturePipelines != null)
			{
				var target = Input;
				foreach (var pipeline in _texturePipelines)
				{
					pipeline.Target = null;
					target          = pipeline;
				}

				target.TextureChanged -= OnInputTextureChanged;
			}

			Input.TextureChanged -= OnInputTextureChanged;

			Texture = null;
		}

		private void OnInputTextureChanged(Texture? texture)
		{
			if (_texturePipelines != null)
			{
				foreach (var pipeline in _texturePipelines.Reverse())
				{
					if (!pipeline.IsRunning || pipeline.Texture.IsUnityNull())
						continue;

					Texture = pipeline.Texture;
					return;
				}

				Texture = Input.Texture;
				return;
			}

			Texture = texture;
		}
	}
}
