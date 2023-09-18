/*===============================================================
* Product:		Com2Verse
* File Name:	CommunicationAvatarManagerViewModel.cs
* Developer:	urun4m0r1
* Date:			2022-06-17 13:12
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using Com2Verse.Communication;
using Com2Verse.Data;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("Communication")]
	public sealed class CommunicationAvatarManagerViewModel : CollectionManagerViewModel<Uid, CommunicationAvatarViewModel>
	{
		/// <summary>
		/// CommunicationUserViewModel과 ActiveObjectViewModel을 연결하는 CollectionManagerViewModel
		/// <br/>해당 콜렉션의 아이템은 ActiveObjectViewModel이 존재하는 경우에만 생성됩니다.
		/// <br/>따라서 ActiveObjectViewModel이 존재하지 않는 경우 (아바타가 존재하지 않지만 화상 통화는 필요한 상황 등), 해당 ViewModel은 생성되지 않습니다.
		/// </summary>
		public CommunicationAvatarManagerViewModel()
		{
			var avatarManager = ViewModelManager.Instance.GetOrAdd<ActiveObjectManagerViewModel>();
			avatarManager.ViewModelAdded   += OnAvatarViewModelAdded;
			avatarManager.ViewModelRemoved += OnAvatarViewModelRemoved;

			foreach (var item in avatarManager.ViewModelMap)
				AddAvatar(item.Key, item.Value!);
			WaitSceneLoaded().Forget();
		}

		private async UniTask WaitSceneLoaded()
		{
			await UniTask.WaitUntil(() => IsSceneLoaded);
			// if (IsAuthorizedOffice)
			{
				_userManagerViewModel                  =  new CommunicationUserManagerViewModel();
				_userManagerViewModel.ViewModelAdded   += OnUserViewModelAdded;
				_userManagerViewModel.ViewModelRemoved += OnUserViewModelRemoved;

				foreach (var item in _userManagerViewModel.ViewModelMap)
					AddUser(item.Key, item.Value!);
			}
		}

		private void OnUserViewModelAdded(Uid      uid,      CommunicationUserViewModel viewModel) => AddUser(uid, viewModel);
		private void OnUserViewModelRemoved(Uid    uid,      CommunicationUserViewModel viewModel) => RemoveUser(uid, viewModel);
		private void OnAvatarViewModelAdded(long   objectId, ActiveObjectViewModel      viewModel) => AddAvatar(objectId, viewModel);
		private void OnAvatarViewModelRemoved(long objectId, ActiveObjectViewModel      viewModel) => RemoveAvatar(objectId, viewModel);

		private void AddUser(Uid uid, CommunicationUserViewModel userViewModel)
		{
			if (!TryGet(uid, out var target))
				return;

			ReplaceViewModel(uid, userViewModel, target.AvatarViewModel);
		}

		private void RemoveUser(Uid uid, CommunicationUserViewModel userViewModel)
		{
			if (!TryGet(uid, out var target))
				return;

			ReplaceViewModel(uid, null, target.AvatarViewModel);
		}

		private void AddAvatar(long objectId, ActiveObjectViewModel avatarViewModel)
		{
			var uid = avatarViewModel.OwnerId;
			if (TryGet(uid, out var target))
				return;

			CommunicationUserViewModel? userViewModel = null;
			if (IsAuthorizedOffice)
				ViewModelManager.InstanceOrNull?.Get<CommunicationUserManagerViewModel>()?.TryGet(uid, out userViewModel);

			Add(uid, new CommunicationAvatarViewModel(userViewModel, avatarViewModel));
		}

		private void RemoveAvatar(long objectId, ActiveObjectViewModel avatarViewModel)
		{
			var uid = avatarViewModel.OwnerId;
			if (!TryGet(uid, out var target))
				return;

			Remove(uid);
		}

		private void ReplaceViewModel(Uid uid, CommunicationUserViewModel? userViewModel, ActiveObjectViewModel? avatarViewModel)
		{
			Remove(uid);

			if (userViewModel == CommunicationUserViewModel.Empty)
				userViewModel = null;

			if (avatarViewModel == ActiveObjectViewModel.Empty)
				avatarViewModel = null;

			if (userViewModel == null && avatarViewModel == null)
				return;

			Add(uid, new CommunicationAvatarViewModel(userViewModel, avatarViewModel));
		}

#region IDisposable
		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				//var userManager = ViewModelManager.InstanceOrNull?.Get<CommunicationUserManagerViewModel>();
				//if (userManager != null)
				//{
				//	userManager.ViewModelAdded   -= OnUserViewModelAdded;
				//	userManager.ViewModelRemoved -= OnUserViewModelRemoved;
				//	userManager.Dispose();
				//}

				if (_userManagerViewModel != null)
				{
					_userManagerViewModel.ViewModelAdded   -= OnUserViewModelAdded;
					_userManagerViewModel.ViewModelRemoved -= OnUserViewModelRemoved;
					_userManagerViewModel.Dispose();
				}

				var avatarManager = ViewModelManager.InstanceOrNull?.Get<ActiveObjectManagerViewModel>();
				if (avatarManager != null)
				{
					avatarManager.ViewModelAdded   -= OnAvatarViewModelAdded;
					avatarManager.ViewModelRemoved -= OnAvatarViewModelRemoved;
					avatarManager.Dispose();
				}
			}

			// Uncomment this line in inherited class to implement standard disposing pattern.
			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable

		private bool IsAuthorizedOffice => CurrentScene.ServiceType is eServiceType.OFFICE && CurrentScene.SpaceCode is not (eSpaceCode.LOBBY or eSpaceCode.MODEL_HOUSE) && IsSceneLoaded;
		private bool IsSceneLoaded      => SceneManager.InstanceOrNull?.CurrentScene.SceneState is eSceneState.LOADED;
		private CommunicationUserManagerViewModel? _userManagerViewModel;
	}
}
