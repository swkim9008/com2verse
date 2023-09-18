/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarPartsItemController.cs
* Developer:	eugene9721
* Date:			2023-05-11 10:30
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using UnityEngine;
using System.Collections.Generic;
using Com2Verse.AssetSystem;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.LruObjectPool;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using Protocols.CommonLogic;
using Protocols.GameLogic;

namespace Com2Verse.Avatar.UI
{
	public sealed class AvatarPartsItemController : CollectionBinder
	{
		private readonly List<CustomizeItemViewModel> _collection = new();

		private async UniTask<GameObject?> GetPartsIconPrefab(int itemId, AvatarTable.eCustomizeItemType type)
		{
			var prefabResId = AvatarTable.GetItemSpriteAddressableName(itemId, type);
			if (string.IsNullOrEmpty(prefabResId)) return null; // TODO: 빈 리소스 로드

			var prefab = await RuntimeObjectManager.Instance.LoadAssetAsyncAwait<GameObject>(prefabResId);
			if (prefab.IsReferenceNull())
				return null;

			if (prefab.TryGetComponent(out RectTransform rect))
				rect!.SetStretch();

			return prefab;
		}

		private async UniTask InstantiatePartsIconObject(CustomizeItemViewModel viewModel)
		{
			if (viewModel.ItemHolder.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "ItemHolder is null");
				return;
			}

			var partsIconPrefab = await GetPartsIconPrefab(viewModel.ItemId, viewModel.CustomizeItemType);
			ClearPartsIconObject(viewModel);
			if (partsIconPrefab.IsReferenceNull())
				return;

			var partsIconObject = RuntimeObjectManager.Instance.Instantiate<GameObject>(partsIconPrefab!, Vector3.zero, Quaternion.identity);
			partsIconObject!.transform.SetParent(viewModel.ItemHolder!, false);
		}

		private void ClearPartsIconObject(CustomizeItemViewModel viewModel)
		{
			if (viewModel.ItemHolder.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "ItemHolder is null");
				return;
			}

			if (!RuntimeObjectManager.InstanceExists) return;
			
			foreach (Transform child in viewModel.ItemHolder!)
				RuntimeObjectManager.Instance.Remove(child.gameObject);
		}

#region Item Additonal Info
		private void CheckIsWearing(CustomizeItemViewModel viewModel)
		{
			var userAvatarInfo = AvatarMediator.Instance.UserAvatarInfo;
			if (userAvatarInfo == null)
			{
				viewModel.IsWearing = false;
				return;
			}

			if (viewModel.CustomizeItemType == AvatarTable.eCustomizeItemType.FACE)
			{
				if (viewModel.FaceOption == eFaceOption.PRESET_LIST)
				{
					var faceViewModel = ViewModelManager.Instance.Get<AvatarSelectionFaceViewModel>();
					if (faceViewModel != null)
						viewModel.IsWearing = viewModel.ItemId == faceViewModel.LastSelectedPresetId;
				}
				else
				{
					viewModel.IsWearing = userAvatarInfo.IsSetFaceItemWithoutColor(viewModel.ItemId);
				}
			}
			else if (viewModel.CustomizeItemType == AvatarTable.eCustomizeItemType.FASHION)
				viewModel.IsWearing = userAvatarInfo.IsWearingFashionItem(viewModel.ItemId);
			else
				viewModel.IsWearing = false;
		}

		private void RefreshItemInfo(CustomizeItemViewModel viewModel)
		{
			CheckIsWearing(viewModel);
		}
#endregion Item Additonal Info

#region Network
		private void OnUpdateAvatarResponse(UpdateAvatarResponse updateAvatarResponse)
		{
			foreach (var viewModel in _collection)
				RefreshItemInfo(viewModel);
		}
#endregion Network

#region CollectionBinder
		public override void Bind()
		{
			base.Bind();
			PacketReceiver.Instance.OnUpdateAvatarResponseEvent += OnUpdateAvatarResponse;
		}

		public override void Unbind()
		{
			base.Unbind();
			if (PacketReceiver.InstanceExists)
				PacketReceiver.Instance.OnUpdateAvatarResponseEvent -= OnUpdateAvatarResponse;
			_collection.Clear();
		}

		protected override void OnItemBinded(CollectionItem target)
		{
			var viewModel = GetCollectionInfo(target);
			if (viewModel == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "viewModel is null");
				return;
			}

			_collection.Add(viewModel);
			RefreshItemInfo(viewModel);

			InstantiatePartsIconObject(viewModel).Forget();
		}

		protected override void OnItemUnbinded(CollectionItem target)
		{
			var viewModel = GetCollectionInfo(target);
			if (viewModel == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "viewModel is null");
				return;
			}

			_collection.Remove(viewModel);
			ClearPartsIconObject(viewModel);
		}

		private static CustomizeItemViewModel? GetCollectionInfo(CollectionItem target)
		{
			var viewModel = target.ViewModelContainer?.GetViewModel<CustomizeItemViewModel>();
			return viewModel;
		}
#endregion CollectionBinder
	}
}
