/*===============================================================
* Product:		Com2Verse
* File Name:	SharedScreen.cs
* Developer:	urun4m0r1
* Date:			2022-08-19 13:27
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Communication;
using Com2Verse.Extension;
using UnityEngine;

namespace Com2Verse.UI
{
	public class SharedScreen : IDisposable
	{
		public event Action<IViewModelUser?, IViewModelUser?>? UserChanged;

		public event Action<bool>?     IsObservingChanged;
		public event Action<Texture?>? TextureChanged;

		private IViewModelUser? _currentUser;

		private bool _isObserving;

		public SharedScreen()
		{
			ChannelManager.Instance.ViewModelUserAdded   += OnChannelUserAdded;
			ChannelManager.Instance.ViewModelUserRemoved += OnChannelUserRemoved;

			foreach (var user in ChannelManager.Instance.GetViewModelUsers())
			{
				AddUser(user);
			}
		}

		public void Dispose()
		{
			var channelManager = ChannelManager.InstanceOrNull;
			if (channelManager != null)
			{
				channelManager.ViewModelUserAdded   -= OnChannelUserAdded;
				channelManager.ViewModelUserRemoved -= OnChannelUserRemoved;

				foreach (var user in channelManager.GetViewModelUsers())
				{
					RemoveUser(user);
				}
			}
		}

		private void OnChannelUserAdded(IChannel channel, IViewModelUser user)
		{
			AddUser(user);
		}

		private void OnChannelUserRemoved(IChannel channel, IViewModelUser user)
		{
			RemoveUser(user);
		}

		private void AddUser(IViewModelUser user)
		{
			var screen = user.Screen;
			if (screen == null)
				return;

			screen.TextureChanged += OnTextureChanged(user);
			SetCurrentUser(user, screen.Texture);
		}

		private void RemoveUser(IViewModelUser user)
		{
			var screen = user.Screen;
			if (screen == null)
				return;

			screen.TextureChanged -= OnTextureChanged(user);
			SetCurrentUser(user, null);
		}

		private Action<Texture?> OnTextureChanged(IViewModelUser user)
		{
			return texture => SetCurrentUser(user, texture);
		}

		/// <summary>
		/// 마지막으로 스크린 텍스쳐가 추가된 유저를 현재 유저로 설정합니다.
		/// </summary>
		private void SetCurrentUser(IViewModelUser user, Texture? texture)
		{
			var prevUser = _currentUser;

			if (!texture.IsUnityNull())
			{
				_currentUser = user;
			}
			else
			{
				if (_currentUser == user)
				{
					_currentUser = null;
				}
			}

			if (prevUser != _currentUser)
				OnUserChanged(prevUser, _currentUser);
		}

		private void OnUserChanged(IViewModelUser? prevUser, IViewModelUser? user)
		{
			var prevScreen = prevUser?.Screen;
			if (prevScreen != null)
			{
				prevScreen.Input.StateChanged -= OnInputStateChanged;
				prevScreen.TextureChanged     -= OnTextureChanged;
			}

			var screen = user?.Screen;
			if (screen != null)
			{
				screen.Input.StateChanged += OnInputStateChanged;
				screen.TextureChanged     += OnTextureChanged;
			}

			UserChanged?.Invoke(prevUser, user);

			OnInputStateChanged(screen?.Input.IsRunning ?? false);
			OnTextureChanged(screen?.Texture);
		}

		private void OnInputStateChanged(bool isObserving)
		{
			IsObserving = isObserving;
		}

		private void OnTextureChanged(Texture? texture)
		{
			TextureChanged?.Invoke(texture);
		}

#region ViewModelProperties
		public bool IsObserving
		{
			get => _isObserving;
			set
			{
				if (_isObserving == value)
					return;

				if (_currentUser?.Screen?.Input is RemoteScreenProvider remoteScreenProvider)
					remoteScreenProvider.IsRunning = value;

				_isObserving = value;
				IsObservingChanged?.Invoke(value);
			}
		}

		public bool IsLocalScreen  => _currentUser is ILocalUser;
		public bool IsRemoteScreen => _currentUser is IRemoteUser;
		public bool IsRunning      => _currentUser != null;

		public Texture? Texture   => _currentUser?.Screen?.Texture;
		public bool     IsVisible => IsRunning && IsObserving && Texture != null;
		public bool     IsLoading => IsRunning && IsObserving && Texture == null;
#endregion // ViewModelProperties
	}
}
