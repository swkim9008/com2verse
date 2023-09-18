/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarCloset.cs
* Developer:	eugene9721
* Date:			2023-04-18 11:24
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.Avatar.UI;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.UI;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using Localization = Com2Verse.UI.Localization;
using Vector3 = UnityEngine.Vector3;

namespace Com2Verse.Avatar
{
	public enum eViewType
	{
		TYPE,
		FACE,
		BODY,
		FASHION,
	}

	// TODO: AvatarInfo로 대체
	public class AvatarEditData
	{
		public bool HasAvatarType  => AvatarType != eAvatarType.NONE;
		public bool HasFaceItem    => AvatarMediator.Instance.AvatarCloset.HasAvatar;
		public bool HasBodyType    => AvatarMediator.Instance.AvatarCloset.HasAvatar;
		public bool HasFashionItem => AvatarMediator.Instance.AvatarCloset.HasAvatar;

		public eAvatarType AvatarType { get; set; }

		public void Clear()
		{
			AvatarType = eAvatarType.NONE;
		}
	}

	public sealed class AvatarCloset
	{
		public enum eResetCustomizeOption
		{
			ALL,
			ONLY_FACE,
			ONLY_BODY,
			ONLY_FASHION,
		}

		private const string TypeTapTextKey    = "UI_AvatarCreate_Tab_Type";
		private const string FaceTapTextKey    = "UI_AvatarCreate_Tab_Face";
		private const string BodyTapTextKey    = "UI_AvatarCreate_Tab_BodyShape";
		private const string FashionTapTextKey = "UI_AvatarCreate_Tab_Cloth";

		private Action<AvatarController>? _onAvatarCreated;

		public event Action<AvatarController>? OnAvatarCreated
		{
			add
			{
				_onAvatarCreated -= value;
				_onAvatarCreated += value;
			}
			remove => _onAvatarCreated -= value;
		}

		private Action<AvatarInfo>? _onAvatarItemInfoChanged;

		public event Action<AvatarInfo> OnAvatarItemInfoChanged
		{
			add
			{
				_onAvatarItemInfoChanged -= value;
				_onAvatarItemInfoChanged += value;
			}
			remove => _onAvatarItemInfoChanged -= value;
		}

		public AvatarInfo? CurrentAvatarInfo { get; private set; }

		public bool HasAvatar => CurrentAvatarInfo != null;

		private AvatarInventoryBase? _avatarInventory;

		public AvatarController? CurrentAvatar { get; set; }

		public AvatarJigController? AvatarJig { get; set; }

		public bool IsFaceEditing { get; set; }

		public AvatarEditData AvatarEditData { get; } = new();

		public IAvatarClosetController? Controller { get; private set; }

#region Initialize
		public void Initialize(AvatarInventoryBase avatarInventory, AvatarJigController avatarJig, IAvatarClosetController controller)
		{
			AvatarJig = avatarJig;
			InitializeAvatarInventory(avatarInventory);
			Controller = controller;
		}

		public void Clear()
		{
			EnableLookAt(false);
			ClearAvatarInventory();
			AvatarEditData.Clear();

			_onAvatarCreated         = null;
			_onAvatarItemInfoChanged = null;

			AvatarJig  = null;
			Controller = null;

			CurrentAvatarInfo = null;
		}
#endregion Initialize

		public string GetTypeTapTextKey(eViewType type)
		{
			switch (type)
			{
				case eViewType.TYPE:
					return Localization.Instance.GetString(TypeTapTextKey);
				case eViewType.FACE:
					return Localization.Instance.GetString(FaceTapTextKey);
				case eViewType.BODY:
					return Localization.Instance.GetString(BodyTapTextKey);
				case eViewType.FASHION:
					return Localization.Instance.GetString(FashionTapTextKey);
			}

			return string.Empty;
		}

#region Avatar Inventory
		private bool InitializeAvatarInventory(AvatarInventoryBase avatarInventory)
		{
			if (!avatarInventory.Initialize())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "Failed to initialize avatar inventory.");
				return false;
			}

			_avatarInventory = avatarInventory;
			return true;
		}

		private void ClearAvatarInventory()
		{
			_avatarInventory?.Clear();
		}

		public List<int>? GetFaceItemList() => _avatarInventory?.GetFaceItemList() ?? null;

		public List<int>? GetBodyShapeItemList() => _avatarInventory?.GetBodyShapeItemList() ?? null;

		public List<int>? GetFashionItemList() => _avatarInventory?.GetFashionItemList() ?? null;
#endregion Avatar Inventory

#region Avatar Creation
		public async UniTask CreateAvatar(AvatarInfo avatarInfo, bool isSetAvatarInfo)
		{
			if (avatarInfo.AvatarType == eAvatarType.NONE)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "AvatarType is None.");
				return;
			}

			if (AvatarJig.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "AvatarModelRoot is None.");
				return;
			}

			DestroyAvatar();
			var avatarController = await AvatarCreator.CreateAvatarAsync(avatarInfo, eAnimatorType.AVATAR_CUSTOMIZE, Vector3.zero, (int)Define.eLayer.CHARACTER);
			if (avatarController.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "avatarController is not created.");
				return;
			}

			CurrentAvatar = avatarController!;
			CurrentAvatar.SetAvatarInfo(avatarInfo);

			CurrentAvatar.SetActive(true);
			_onAvatarCreated?.Invoke(CurrentAvatar);

			if (isSetAvatarInfo)
				SetAvatarInfo(avatarInfo);

			await UniTask.DelayFrame(1);

			var avatarAnimatorController = CurrentAvatar!.AvatarAnimatorController;
			if (!avatarAnimatorController.IsUnityNull())
				avatarAnimatorController!.SetAnimatorState(-1, -1);

			SetAvatar(CurrentAvatar);
		}

		private void DestroyAvatar()
		{
			if (!CurrentAvatar.IsUnityNull())
				UnityEngine.Object.Destroy(CurrentAvatar!.gameObject);

			CurrentAvatar = null;
		}

		private void SetAvatar(AvatarController currentAvatar)
		{
			var avatarJig = Controller?.AvatarJigController;
			Controller?.OnCreatedAvatar(currentAvatar);
			if (!avatarJig.IsUnityNull() || !currentAvatar.HeadBone.IsUnityNull())
			{
				avatarJig!.SetAvatar(currentAvatar);
				avatarJig.SetNeedZoomRefresh();
			}
			else
			{
				C2VDebug.LogErrorCategory(GetType().Name, "LookAtController Init Failed.");
			}
		}

		public void EnableLookAt(bool value)
		{
			var avatarJig = Controller?.AvatarJigController;
			if (!avatarJig.IsUnityNull())
				avatarJig!.OnLookAtState(value);
		}

		public void ClearAvatar()
		{
			if (!HasAvatar) return;

			DestroyAvatar();

			var viewModel = ViewModelManager.Instance.Get<AvatarSelectionManagerViewModel>();
			if (viewModel != null)
				viewModel.IsCreatedAvatar = false;
		}
#endregion Avatar Creation

#region Avatar Customize
		/// <summary>
		/// 현재 커스터마이즈중인 정보를 초기화하고, 동기화중이던 아바타 인포로 초기화
		/// </summary>
		public void ResetCustomizeAvatar(eResetCustomizeOption option = eResetCustomizeOption.ALL)
		{
			var userItemInfo = AvatarMediator.Instance.UserAvatarInfo?.Clone();
			if (userItemInfo == null)
				return;

			var avatarInfo = AvatarMediator.Instance.AvatarCloset.CurrentAvatarInfo;
			if (avatarInfo == null) return;

			switch (option)
			{
				case eResetCustomizeOption.ONLY_FACE:
					userItemInfo.ClearFashionItem();
					userItemInfo.UpdateBodyShapeItem(avatarInfo.BodyShape);
					break;
				case eResetCustomizeOption.ONLY_BODY:
					userItemInfo.ClearFaceItem();
					userItemInfo.ClearFashionItem();
					break;
				case eResetCustomizeOption.ONLY_FASHION:
					userItemInfo.ClearFaceItem();
					userItemInfo.UpdateBodyShapeItem(avatarInfo.BodyShape);
					break;
			}

			new AvatarItemSelectAction(avatarInfo, userItemInfo).Do(true);
		}
#endregion Avatar Customize

#region Item Info
		public AvatarInfo SetDefaultAvatarInfo(eAvatarType type)
		{
			var avatarInfo = GetDefaultAvatarInfo(type);
			SetAvatarInfo(avatarInfo);
			return avatarInfo;
		}

		public AvatarInfo GetDefaultAvatarInfo(eAvatarType type = eAvatarType.PC01_W) =>
			AvatarManager.Instance.GetDefaultAvatarInfo(type);

		public void SetAvatarInfo(AvatarInfo? avatarInfo, bool isUpdateEditData = true)
		{
			CurrentAvatarInfo = avatarInfo;
			if (isUpdateEditData)
				AvatarEditData.AvatarType = avatarInfo?.AvatarType ?? eAvatarType.NONE;
		}

		private eViewType _currentView = eViewType.TYPE;

		public void ChangeCurrentView(eViewType viewType)
		{
			_currentView = viewType;
		}

		/// <summary>
		/// 현재 생성된 아바타의 정보와 AvatarCloset의 정보를 일치시킵니다.
		/// </summary>
		public void SyncAvatarInfo()
		{
			if (CurrentAvatar.IsUnityNull() || CurrentAvatar!.Info == null)
			{
				CurrentAvatarInfo = null;
				return;
			}

			if (CurrentAvatarInfo == null)
				CurrentAvatarInfo = new AvatarInfo();

			var avatarInfo = CurrentAvatar!.Info.Clone();

			switch (_currentView)
			{
				case eViewType.FACE:
					CurrentAvatarInfo.InitializeFaceItem(avatarInfo.GetFaceOptionList());
					break;
				case eViewType.BODY:
					CurrentAvatarInfo.UpdateBodyShapeItem(avatarInfo.BodyShape);
					break;
				case eViewType.FASHION:
					CurrentAvatarInfo.InitializeFashionItem(avatarInfo.GetFashionItemList());
					break;
				default:
					CurrentAvatarInfo = avatarInfo;
					break;
			}
		}

		public void ApplyFaceItem(int itemId)
		{
			if (CurrentAvatar.IsUnityNull() || CurrentAvatarInfo == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "CurrentAvatar or CurrentAvatarInfo is null.");
				return;
			}

			var color       = 0;
			var currentItem = CurrentAvatarInfo.GetFaceOption(AvatarTable.IdToFaceOption(itemId));
			if (currentItem != null)
				color = currentItem.ColorId;

			var newInfo = CurrentAvatarInfo.Clone();
			var itemInfo = new FaceItemInfo(itemId, color);
			newInfo.UpdateFaceItem(itemInfo);

			new AvatarItemSelectAction(CurrentAvatarInfo, newInfo).Do(true);
		}

		public void ApplyFaceColorItem(int color, eFaceOption faceOption)
		{
			if (CurrentAvatarInfo == null || CurrentAvatar.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "Cannot found faceItem");
				return;
			}

			var newInfo     = CurrentAvatarInfo.Clone();
			var currentItem = newInfo.GetFaceOption(faceOption);

			if (currentItem == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "CurrentItem Is Null");
				return;
			}

			currentItem.ColorId = color;
			newInfo.UpdateFaceItem(currentItem);

			new AvatarItemSelectAction(CurrentAvatarInfo, newInfo).Do(true);
		}

		public void SetFashionItem(int itemId)
		{
			if (CurrentAvatar.IsUnityNull() || CurrentAvatarInfo == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "CurrentAvatar or CurrentAvatarInfo is null.");
				return;
			}

			var newInfo = CurrentAvatarInfo.Clone();
			var itemInfo = new FashionItemInfo(itemId);
			newInfo.UpdateFashionItem(itemInfo);
			new AvatarItemSelectAction(CurrentAvatarInfo, newInfo).Do(true);

			if (itemInfo.FashionSubMenu == eFashionSubMenu.HAT)
				RefreshHairItem();
		}

		public void RemoveFashionItem(eFashionSubMenu fashionSubMenu)
		{
			if (CurrentAvatar.IsUnityNull() || CurrentAvatarInfo == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "CurrentAvatar or CurrentAvatarInfo is null.");
				return;
			}
			var newInfo = CurrentAvatarInfo.Clone();
			newInfo.RemoveFashionItem(fashionSubMenu);
			new AvatarItemSelectAction(CurrentAvatarInfo, newInfo).Do(true);

			if (fashionSubMenu == eFashionSubMenu.HAT)
				RefreshHairItem();
		}

		public void SetBodyShapeItem(int itemId)
		{
			if (CurrentAvatar.IsUnityNull() || CurrentAvatarInfo == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "CurrentAvatar or CurrentAvatarInfo is null.");
				return;
			}

			var prevInfo = CurrentAvatar!.Info ?? CurrentAvatarInfo;
			var newInfo = prevInfo.Clone();
			newInfo.UpdateBodyShapeItem(itemId);
			new AvatarItemSelectAction(prevInfo, newInfo).Do(true);
		}

		public void SetFacePreset(int itemId)
		{
			if (CurrentAvatar.IsUnityNull() || CurrentAvatarInfo == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "CurrentAvatar or CurrentAvatarInfo is null.");
				return;
			}

			var avatarInfo = AvatarManager.Instance.GetFacePresetInfo(itemId, CurrentAvatarInfo);
			if (avatarInfo == null)
				return;

			var newInfo = CurrentAvatarInfo.Clone();
			newInfo.InitializeFaceItem(avatarInfo.GetFaceOptionList());
			new AvatarItemSelectAction(CurrentAvatarInfo, newInfo).Do(true);
		}

		public async UniTask UpdateAvatarModel(AvatarInfo newAvatarInfo, Action? onComplete = null)
		{
			if (CurrentAvatar.IsUnityNull()) return;

			CurrentAvatarInfo ??= new AvatarInfo();
			await AvatarManager.Instance.UpdateAvatarParts(CurrentAvatar!, newAvatarInfo, false);

			SyncAvatarInfo();
			onComplete?.Invoke();
			_onAvatarItemInfoChanged?.Invoke(CurrentAvatarInfo);
		}

		private void RefreshHairItem()
		{
			if (CurrentAvatar.IsUnityNull() || CurrentAvatarInfo == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "CurrentAvatar or CurrentAvatarInfo is null.");
				return;
			}

			var currentHair = CurrentAvatarInfo.GetFaceOption(eFaceOption.HAIR_STYLE);
			if (currentHair == null)
				return;

			CurrentAvatar!.SetFaceOption(currentHair).Forget();
		}
#endregion Item Info
	}
}
