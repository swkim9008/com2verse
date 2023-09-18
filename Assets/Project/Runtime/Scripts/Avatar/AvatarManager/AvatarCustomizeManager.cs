/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarCustomizeManager.cs
* Developer:	eugene9721
* Date:			2023-05-17 09:55
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.AssetSystem;
using UnityEngine;
using Com2Verse.Avatar.UI;
using Com2Verse.CameraSystem;
using Com2Verse.Extension;
using Com2Verse.InputSystem;
using Com2Verse.Logger;
using Com2Verse.LruObjectPool;
using Com2Verse.Network;
using Com2Verse.Project.InputSystem;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Avatar
{
	public sealed class AvatarCustomizeManager : DestroyableMonoSingleton<AvatarCustomizeManager>, IAvatarClosetController
	{
		private readonly Vector3 _avatarJigPosition = new(-1000, -1000, -1000);

#region TextKey
		private const string BackPopupTitleKey   = "UI_AvatarCustomize_Popup_Title_FinishCustomize";
		private const string BackPopupMessageKey = "UI_AvatarCustomize_Popup_Msg_FinishCustomize";

		private const string ChangeTypePopupTitleKey   = "UI_AvatarCustomize_Popup_Title_ChangeTab";
		private const string ChangeTypePopupMessageKey = "UI_AvatarCustomize_Popup_Msg_ChangeTab";

		private const string AvatarJigPrefabName = "UI_AvatarCustomize_Jig.prefab";
		private const string UIPopupPrefabName   = "UI_AvatarCustomiz_Popup";

		public string TitleTextKey  => "UI_AvatarCustomize_Title";

		public string DisableToastTextKey => "UI_Avatar_Toast_Msg_CanNotChange";

		public string ButtonTextKey(eViewType viewType) => viewType switch
		{
			eViewType.TYPE    => "UI_AvatarCustomize_Type_Btn_Save",
			eViewType.FACE    => "UI_AvatarCustomize_Face_Btn_Save",
			eViewType.BODY    => "UI_AvatarCustomize_BodyShape_Btn_Save",
			eViewType.FASHION => "UI_AvatarCustomize_Cloth_Btn_Save",
			_                 => string.Empty,
		};
#endregion TextKey

#region IAvatarClosetControllerProperties
		public eViewType StartView => eViewType.FASHION;

		public bool NeedAvatarCameraFrame => true;

		public bool IsOnUndoButton => true;

		public bool IsUseAdditionalInfoAtItem => true;

		public AvatarJigController? AvatarJigController => _avatarJigController;

		public float RotateMarkerHideTime => _rotateMarkerHideTime;
#endregion IAvatarClosetControllerProperties

		[Header("Interaction")]
		[SerializeField] private float _zoomDuration = 1.1f;

		[SerializeField] private float _rotateMarkerHideTime = 3f;

		private AvatarJigController? _avatarJigController;
		private ActionMapUIControl?  _actionMapUIControl;

		private bool _isInitialized;
		private bool _isEnabled;

		private GUIView? _customizePopup;

		private bool _prevDitherValue;

		private AvatarSelectionManagerViewModel? _viewModel;

#region UnityEvent
		protected override void AwakeInvoked()
		{
			Initialize().Forget();
		}

		private void Update()
		{
			_viewModel ??= ViewModelManager.Instance.Get<AvatarSelectionManagerViewModel>();
			_viewModel?.OnUpdate();
		}

		protected override void OnApplicationQuitInvoked() => Destroy();
		protected override void OnDestroyInvoked()         => Destroy();

		private void Destroy()
		{
			DisableCustomizePopup();
		}

		private async UniTask Initialize()
		{
			await UniTask.WaitUntil(IsTableLoaded);
			await LoadAvatarJig();
			_isInitialized = true;
		}

		private bool IsTableLoaded() => AvatarTable.IsTableLoaded;

		private bool IsInitialized() => _isInitialized;
#endregion UnityEvent

#region Enable
		public void ShowCustomizePopup()
		{
			if (_isEnabled) return;

			_isEnabled = true;
			BindInputAction();
			ShowCustomizePopupAsync().Forget();

			IAvatarClosetController.AvatarClosetSetting.SetSetting();
		}

		private async UniTask ShowCustomizePopupAsync()
		{
			await UniTask.WaitUntil(IsInitialized);
			if (_avatarJigController.IsUnityNull())
				return;

			var avatarManger = AvatarMediator.Instance;
			avatarManger.AvatarCloset.Initialize(new AvatarInventory(), _avatarJigController!, this);
			await UIManager.Instance.CreatePopup(UIPopupPrefabName, SetCustomizePopup);
		}

		private void SetCustomizePopup(GUIView guiView)
		{
			guiView.Show();
			_customizePopup = guiView;
			_viewModel      ??= ViewModelManager.Instance.Get<AvatarSelectionManagerViewModel>();
			if (_viewModel != null)
			{
				var avatarInfo     = GetAvatarInfo();
				var avatarMediator = AvatarMediator.Instance;
				avatarMediator.AvatarCloset.SetAvatarInfo(avatarInfo);

				_viewModel.FaceStepButtonState    = (int)AvatarSelectionManagerViewModel.eStepButtonState.COMPLETION;
				_viewModel.BodyStepButtonState    = (int)AvatarSelectionManagerViewModel.eStepButtonState.COMPLETION;
				_viewModel.FashionStepButtonState = (int)AvatarSelectionManagerViewModel.eStepButtonState.COMPLETION;
				_viewModel.OnEnable();

				if (!_avatarJigController.IsUnityNull())
					_avatarJigController!.Enable();

				UIManager.Instance.SetGuiViewActive(false);
				UIManager.Instance.SetPopupLayerGuiViewActive(false, true, guiView);
			}
		}

		private void DisableCustomizePopup()
		{
			if (!_isEnabled) return;

			_isEnabled = false;
			UnbindInputAction();

			var avatarMediator = AvatarMediator.Instance;
			avatarMediator.AvatarCloset.Clear();

			if (!_avatarJigController.IsUnityNull())
				_avatarJigController!.Disable();

			var metaverseCamera = CameraManager.Instance.MetaverseCamera;
			if (!metaverseCamera.IsUnityNull() && !metaverseCamera!.Brain.IsUnityNull())
				metaverseCamera.Brain!.ManualUpdate();

			IAvatarClosetController.AvatarClosetSetting.ResetSetting();

			UIManager.Instance.SetGuiViewActive(true);
			UIManager.Instance.SetPopupLayerGuiViewActive(true, false, _customizePopup);
		}
#endregion Enable

#region Interaction
		private void BindInputAction()
		{
			_actionMapUIControl = InputSystemManager.Instance.GetActionMap<ActionMapUIControl>();
			if (_actionMapUIControl != null)
			{
				_actionMapUIControl.LeftDragAction  += OnDragActionChanged;
				_actionMapUIControl.RightDragAction += OnDragActionChanged;
			}
			else
			{
				C2VDebug.LogErrorCategory(GetType().Name, "ActionMapUIControl is null!");
			}
		}

		private void UnbindInputAction()
		{
			if (_actionMapUIControl != null)
			{
				_actionMapUIControl.LeftDragAction  -= OnDragActionChanged;
				_actionMapUIControl.RightDragAction -= OnDragActionChanged;
				_actionMapUIControl                 =  null;
			}
		}

		private void OnDragActionChanged(Vector2 value)
		{
			if (!_isInitialized) return;

			_avatarJigController!.ReserveRotate(value.x);
		}
#endregion Interaction

#region Load Assets
		private async UniTask LoadAvatarJig()
		{
			// GameObject? avatarJigPrefab = null;
			// var avatarJigPrefabHandle = C2VAddressables.LoadAssetAsync<GameObject>(AvatarJigPrefabName);
			// if (avatarJigPrefabHandle != null)
			// 	avatarJigPrefab = await avatarJigPrefabHandle.ToUniTask();

			GameObject? avatarJigPrefab = null;
			avatarJigPrefab = await RuntimeObjectManager.Instance.LoadAssetAsyncAwait<GameObject>(AvatarJigPrefabName);
			if (avatarJigPrefab.IsReferenceNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"Failed to load avatarJigPrefab!");
				return;
			}

			var jigObject = RuntimeObjectManager.Instance.Instantiate<GameObject>(avatarJigPrefab, transform);
			_avatarJigController = jigObject!.GetComponent<AvatarJigController>();
			_avatarJigController!.transform.position = _avatarJigPosition;
		}
#endregion Load Assets

		public void SetFaceVirtualCamera()
		{
			if (_avatarJigController.IsUnityNull()) return;

			_avatarJigController!.SetZoomLevel(AvatarJigController.ZoomLevelMax, _zoomDuration);
		}

		public void SetFullBodyVirtualCamera()
		{
			if (_avatarJigController.IsUnityNull()) return;

			_avatarJigController!.SetZoomLevel(0, _zoomDuration);
		}

		public void OnClear()
		{
		}

		public void UpdateCustomize(eViewType type)
		{
			var avatarInfo = AvatarMediator.Instance.AvatarCloset.CurrentAvatarInfo;
			if (avatarInfo == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "AvatarInfo is null!");
				return;
			}

			Commander.Instance.RequestUpdateAvatar(avatarInfo);
		}

		public AvatarInfo? GetAvatarInfo() => AvatarMediator.Instance.UserAvatarInfo?.Clone();

		public void OnCreatedViewModel()
		{
			if (_avatarJigController.IsUnityNull()) return;

			_avatarJigController!.SetZoomLevel(AvatarJigController.ZoomLevelMax, 0f);
		}

		public void OnClickBackButton(eViewType type)
		{
			var  userAvatar = AvatarMediator.Instance.UserAvatarInfo;
			var  closet     = AvatarMediator.Instance.AvatarCloset;

			if (closet.CurrentAvatarInfo != userAvatar)
			{
				var typeText = AvatarMediator.Instance.AvatarCloset.GetTypeTapTextKey(type);
				UIManager.Instance.ShowPopupYesNo(Localization.Instance.GetString(BackPopupTitleKey),
				                                  Localization.Instance.GetString(BackPopupMessageKey, typeText),
				                                  _ => OnClickBackButtonConfirm());
			}
			else
			{
				OnClickBackButtonConfirm();
			}
		}

		private void OnClickBackButtonConfirm()
		{
			if (!_customizePopup.IsUnityNull())
				_customizePopup!.Hide();

			if (!_avatarJigController.IsUnityNull())
				_avatarJigController!.ResetRotationImmediate();

			AvatarMediator.Instance.AvatarCloset.ResetCustomizeAvatar();

			DisableCustomizePopup();

			_viewModel ??= ViewModelManager.Instance.Get<AvatarSelectionManagerViewModel>();
			_viewModel?.OnDisable();
		}

		public void ChangeTypeTap(eViewType prevType, eViewType nextType, Action? typeChangeAction)
		{
			var avatarMediator = AvatarMediator.Instance;
			var userAvatar     = avatarMediator.UserAvatarInfo;
			var closet         = avatarMediator.AvatarCloset;

			if (closet.CurrentAvatarInfo != userAvatar)
			{
				var typeText = closet.GetTypeTapTextKey(prevType);
				UIManager.Instance.ShowPopupYesNo(Localization.Instance.GetString(ChangeTypePopupTitleKey),
				                                  Localization.Instance.GetString(ChangeTypePopupMessageKey, typeText),
				                                  _ => closet.UpdateAvatarModel(userAvatar!, () =>
				                                  {
					                                  closet.SyncAvatarInfo();
					                                  typeChangeAction?.Invoke();
				                                  }).Forget());
			}
			else
			{
				typeChangeAction?.Invoke();
			}
		}
	}
}
