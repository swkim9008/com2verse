/*===============================================================
* Product:		Com2Verse
* File Name:	VideoViewModel.cs
* Developer:	urun4m0r1
* Date:			2022-08-19 13:27
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Communication;
using Com2Verse.Communication.Unity;
using Com2Verse.Extension;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("Communication")]
	public sealed class CameraViewModel : VideoViewModel<Video>
	{
		public static CameraViewModel Empty { get; } = new();

		public CameraViewModel(Video video) : base(video) { }

		public CameraViewModel() : this(ModuleManager.Instance.Camera) { }
	}

	[UsedImplicitly, ViewModelGroup("Communication")]
	public sealed class ScreenViewModel : VideoViewModel<Video>
	{
		public static ScreenViewModel Empty { get; } = new();

		public ScreenViewModel(Video video) : base(video) { }

		public ScreenViewModel() : this(ModuleManager.Instance.Screen) { }
	}

	public abstract class VideoViewModel<T> : ViewModelBase, IDisposable where T : Video
	{
		[UsedImplicitly] public CommandHandler<bool> SetInputState    { get; }
		[UsedImplicitly] public CommandHandler       ToggleInputState { get; }

		public T Value { get; }

		protected VideoViewModel(T video)
		{
			Value = video;

			SetInputState    = new CommandHandler<bool>(value => IsRunning =  value);
			ToggleInputState = new CommandHandler(() => IsRunning          ^= true);

			RegisterEvents();
		}

		public void Dispose()
		{
			Value.TextureChanged     -= OnTextureChanged;
			Value.Input.StateChanged -= OnInputStateChanged;
		}

		private void RegisterEvents()
		{
			Value.TextureChanged     += OnTextureChanged;
			Value.Input.StateChanged += OnInputStateChanged;
		}

		private void OnTextureChanged(Texture? _)
		{
			InvokePropertyValueChanged(nameof(Texture),   Texture);
			InvokePropertyValueChanged(nameof(IsVisible), IsVisible);
		}

		private void OnInputStateChanged(bool _)
		{
			InvokePropertyValueChanged(nameof(IsRunning), IsRunning);
		}

#region ViewModelProperties
		[UsedImplicitly] public bool IsHorizontalFlipped => Value == ModuleManager.Instance.Camera || Value == ModuleManager.Instance.Screen;
		[UsedImplicitly] public bool IsVerticalFlipped   => false;

		[UsedImplicitly] public bool IsRunning
		{
			get => Value.Input.IsRunning;
			set => Value.Input.IsRunning = value;
		}

		[UsedImplicitly] public bool     IsVisible => !Value.Texture.IsUnityNull();
		[UsedImplicitly] public Texture? Texture   => Value.Texture;
#endregion // ViewModelProperties
	}
}
