/*===============================================================
* Product:		Com2Verse
* File Name:	SharedScreenViewModel.cs
* Developer:	urun4m0r1
* Date:			2022-08-19 13:27
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Communication;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("ScreenShare")]
	public class SharedScreenViewModel : ViewModelBase, IDisposable
	{
		[UsedImplicitly] public CommandHandler<bool> SetIsObserving    { get; }
		[UsedImplicitly] public CommandHandler       ToggleIsObserving { get; }

		private readonly SharedScreen _sharedScreen;

		public SharedScreenViewModel()
		{
			SetIsObserving    = new CommandHandler<bool>(value => IsObserving =  value);
			ToggleIsObserving = new CommandHandler(() => IsObserving          ^= true);

			_sharedScreen = new SharedScreen();

			_sharedScreen.UserChanged        += OnUserChanged;
			_sharedScreen.IsObservingChanged += OnIsObservingChanged;
			_sharedScreen.TextureChanged     += OnTextureChanged;
		}

		public void Dispose()
		{
			_sharedScreen.UserChanged        -= OnUserChanged;
			_sharedScreen.IsObservingChanged -= OnIsObservingChanged;
			_sharedScreen.TextureChanged     -= OnTextureChanged;

			_sharedScreen.Dispose();
		}

		private void OnUserChanged(IViewModelUser? prevUser, IViewModelUser? user)
		{
			UserName = user == null
				? string.Empty
				: GetUserName(user.User.Uid);

			InvokePropertyValueChanged(nameof(IsLocalScreen),  IsLocalScreen);
			InvokePropertyValueChanged(nameof(IsRemoteScreen), IsRemoteScreen);
			InvokePropertyValueChanged(nameof(IsRunning),      IsRunning);
		}

		private void OnIsObservingChanged(bool isObserving)
		{
			InvokePropertyValueChanged(nameof(IsVisible), IsVisible);
			InvokePropertyValueChanged(nameof(IsLoading), IsLoading);
		}

		private void OnTextureChanged(Texture? texture)
		{
			InvokePropertyValueChanged(nameof(Texture), texture);

			InvokePropertyValueChanged(nameof(IsVisible), IsVisible);
			InvokePropertyValueChanged(nameof(IsLoading), IsLoading);
		}

		private string GetUserName(Uid uid)
		{
			ViewModelManager.Instance.GetOrAdd<CommunicationUserManagerViewModel>().TryGet(uid, out var userViewModel);
			return userViewModel?.UserName ?? string.Empty;
		}

#region ViewModelProperties
		public bool IsObserving
		{
			get => _sharedScreen.IsObserving;
			set
			{
				_sharedScreen.IsObserving = value;
				InvokePropertyValueChanged(nameof(IsObserving), value);
			}
		}

		private string? _userName;

		public string? UserName
		{
			get => _userName;
			private set => SetProperty(ref _userName, value);
		}

		public bool IsLocalScreen  => _sharedScreen.IsLocalScreen;
		public bool IsRemoteScreen => _sharedScreen.IsRemoteScreen;
		public bool IsRunning      => _sharedScreen.IsRunning;

		public Texture? Texture   => _sharedScreen.Texture;
		public bool     IsVisible => _sharedScreen.IsVisible;
		public bool     IsLoading => _sharedScreen.IsLoading;
#endregion // ViewModelProperties
	}
}
