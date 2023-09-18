/*===============================================================
* Product:		Com2Verse
* File Name:	ScreenInfo.cs
* Developer:	urun4m0r1
* Date:			2022-09-23 17:35
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Extension;
using UnityEngine;

namespace Com2Verse.ScreenShare
{
	internal class ScreenInfo : IReadOnlyScreenInfo
	{
		public event Action<string?>? TitleChanged;
		public event Action<bool>?    VisibilityChanged;

		public event Action<Texture2D?>? IconChanged;
		public event Action<Texture2D?>? ThumbnailChanged;
		public event Action<Texture2D?>? ScreenChanged;

		public ScreenId    Id         { get; }
		public eScreenType ScreenType { get; }

		public string? Title
		{
			get => _title;
			set
			{
				_title = value;
				TitleChanged?.Invoke(_title);
			}
		}

		public bool IsVisible
		{
			get => _isVisible;
			set
			{
				_isVisible = value;
				VisibilityChanged?.Invoke(_isVisible);
			}
		}

		public Texture2D? Icon
		{
			get => _icon;
			set => SetTexture(out _icon, value, nameof(Icon), IconChanged);
		}

		public Texture2D? Thumbnail
		{
			get => _thumbnail;
			set => SetTexture(out _thumbnail, value, nameof(Thumbnail), ThumbnailChanged);
		}

		public Texture2D? Screen
		{
			get => _screen;
			set => SetTexture(out _screen, value, nameof(Screen), ScreenChanged);
		}

		private string? _title;
		private bool    _isVisible;

		private Texture2D? _icon;
		private Texture2D? _thumbnail;
		private Texture2D? _screen;

		internal ScreenInfo(ScreenId id, eScreenType screenType)
		{
			Id         = id;
			ScreenType = screenType;
		}

		public override string ToString() => GetInfoText();

		public string GetInfoText() => $"[{ScreenType}] ({Id.ToString()}) ({IsVisible.ToString()}) \"{Title}\"";

		private string GetTextureName(string textureType) => $"{textureType}_{GetInfoText()}";

		private void SetTexture(out Texture2D? target, Texture2D? value, string textureType, Action<Texture2D?>? callback)
		{
			if (value.IsUnityNull() || value!.width <= 0 || value.height <= 0)
			{
				target = null;
			}
			else
			{
				target      = value;
				target.name = GetTextureName(textureType);
			}

			callback?.Invoke(target);
		}
	}
}
