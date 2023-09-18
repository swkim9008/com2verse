/*===============================================================
* Product:		Com2Verse
* File Name:	ScreenInfoViewModel.cs
* Developer:	urun4m0r1
* Date:			2022-09-23 13:22
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Extension;
using Com2Verse.ScreenShare;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("ScreenShare")]
	public class ScreenInfoViewModel : ViewModelBase, IDisposable
	{
		[UsedImplicitly] public CommandHandler<bool> SetSelected { get; }

		[UsedImplicitly] public CommandHandler ToggleSelected { get; }

		public IReadOnlyScreenInfo Value { get; }

		public event Action<ScreenInfoViewModel?, bool>? Selected;

		private bool _isSelected;
		private bool _isShareTarget;

		public ScreenInfoViewModel(IReadOnlyScreenInfo readOnlyScreenInfo)
		{
			Value = readOnlyScreenInfo;

			SetSelected = new CommandHandler<bool>(value => IsSelected = value);

			ToggleSelected = new CommandHandler(() => IsSelected ^= true);

			ScreenCaptureManager.Instance.Controller.RequestedScreenChanged += OnRequestedScreenChanged;

			Value.TitleChanged      += OnTitleChanged;
			Value.VisibilityChanged += OnVisibilityChanged;
			Value.IconChanged       += OnIconChanged;
			Value.ThumbnailChanged  += OnThumbnailChanged;
			Value.ScreenChanged     += OnScreenChanged;

			IsShareTarget = GetIsShareTarget(ScreenCaptureManager.Instance.Controller.RequestedScreen);
		}

		public void Dispose()
		{
			var screenCaptureManager = ScreenCaptureManager.InstanceOrNull;
			if (screenCaptureManager != null)
			{
				screenCaptureManager.Controller.RequestedScreenChanged -= OnRequestedScreenChanged;
			}

			Value.TitleChanged      -= OnTitleChanged;
			Value.VisibilityChanged -= OnVisibilityChanged;
			Value.IconChanged       -= OnIconChanged;
			Value.ThumbnailChanged  -= OnThumbnailChanged;
			Value.ScreenChanged     -= OnScreenChanged;
		}

		private void OnRequestedScreenChanged(IReadOnlyScreenInfo? requestedScreen)
		{
			IsShareTarget = GetIsShareTarget(requestedScreen);
		}

		private bool GetIsShareTarget(IReadOnlyScreenInfo? requestedScreen)
		{
			if (requestedScreen == null)
				return false;

			return requestedScreen == Value;
		}

		private void OnIsShareTargetChanged(bool isShareTarget)
		{
			_isShareTarget = isShareTarget;
			InvokePropertyValueChanged(nameof(IsShareTarget), isShareTarget);

			OnScreenLoadingChanged();
		}

		private void OnIsSelectedChanged(bool isSelected)
		{
			_isSelected = isSelected;
			Selected?.Invoke(this, isSelected);
			InvokePropertyValueChanged(nameof(IsSelected), isSelected);
		}

		private void OnTitleChanged(string? title)
		{
			InvokePropertyValueChanged(nameof(Title), title);
		}

		private void OnVisibilityChanged(bool isVisible)
		{
			InvokePropertyValueChanged(nameof(IsVisible), isVisible);
			OnThumbnailLoadingChanged();
		}

		private void OnIconChanged(Texture? icon)
		{
			InvokePropertyValueChanged(nameof(Icon), icon);
		}

		private void OnThumbnailChanged(Texture? thumbnail)
		{
			InvokePropertyValueChanged(nameof(Thumbnail), thumbnail);
			OnThumbnailLoadingChanged();
		}

		private void OnScreenChanged(Texture? screen)
		{
			InvokePropertyValueChanged(nameof(Screen), screen);
			OnScreenLoadingChanged();
		}

		private void OnScreenLoadingChanged()
		{
			InvokePropertyValueChanged(nameof(IsScreenLoading), IsScreenLoading);
		}

		private void OnThumbnailLoadingChanged()
		{
			InvokePropertyValueChanged(nameof(IsThumbnailLoading), IsThumbnailLoading);
		}

#region ViewModelProperties
		public bool IsSelected
		{
			get => _isSelected;
			set
			{
				if (_isSelected != value)
					OnIsSelectedChanged(value);
			}
		}

		public bool IsShareTarget
		{
			get => _isShareTarget;
			private set
			{
				if (_isShareTarget != value)
					OnIsShareTargetChanged(value);
			}
		}

		public string   Title     => Value.Title ?? "Unknown";
		public bool     IsVisible => Value.IsVisible;
		public Texture? Thumbnail => Value.Thumbnail;
		public Texture? Icon      => Value.Icon;
		public Texture? Screen    => Value.Screen;

		public bool IsScreenLoading    => IsShareTarget && Screen.IsUnityNull();
		public bool IsThumbnailLoading => IsVisible     && Thumbnail.IsUnityNull();
#endregion // ViewModelProperties
	}
}
