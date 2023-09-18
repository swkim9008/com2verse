/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarCreateManager.cs
* Developer:	eugene9721
* Date:			2023-04-19 18:16
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.AssetSystem;
using Com2Verse.Avatar.UI;
using Com2Verse.AvatarAnimation;
using UnityEngine;
using Com2Verse.CameraSystem;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.InputSystem;
using Com2Verse.Loading;
using Com2Verse.Logger;
using Com2Verse.LruObjectPool;
using Com2Verse.Network;
using Com2Verse.Project.InputSystem;
using Com2Verse.UI;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using Protocols.GameLogic;
using UnityEngine.AddressableAssets;
using Localization = Com2Verse.UI.Localization;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Com2Verse.Avatar
{
	public sealed class AvatarCreateManager : DestroyableMonoSingleton<AvatarCreateManager>, IAvatarClosetController
	{
#region TextKey
		private const string BackPopupTitleKey   = "UI_AvatarCreate_Popup_Title_Back";
		private const string BackPopupMessageKey = "UI_AvatarCreate_Popup_Msg_Back";

		private const string ChangeTypePopupTitleKey   = "UI_AvatarCreate_Popup_Title_ChangeType";
		private const string ChangeTypePopupMessageKey = "UI_AvatarCreate_Popup_Msg_ChangeType";

		public string TitleTextKey  => "UI_AvatarCreate_Title";

		public string DisableToastTextKey => "UI_AvatarCreate_Toast_Msg_CanNotMove";

		public string ButtonTextKey(eViewType viewType) => viewType switch
		{
			eViewType.TYPE    => "UI_AvatarCreate_Btn_Next",
			eViewType.FACE    => "UI_AvatarCreate_Btn_Next",
			eViewType.BODY    => "UI_AvatarCreate_Btn_Next",
			eViewType.FASHION => "UI_AvatarCreate_Btn_Generate",
			_                 => string.Empty,
		};
#endregion TextKey

		private const float AvatarSelectRayMaxDistance = 1000.0f;
		public const float AvatarColliderHeight       = 1.8f;
		private const float AvatarColliderRadius       = 0.3f;
		private const float AvatarColliderCenterXDiff  = 0.55f;
		/// <summary>
		/// 실제 파티클 시스템의 지속 시간과 관계 없이 해당 시간만 재생하고 오브젝트 제거할 것
		/// </summary>
		private const float AvatarCreateFxDuration     = 2.166667f;

		private readonly int _isEndLoadingHash        = Animator.StringToHash("IsEndLoading");
		private readonly int _isCharacterSelectedHash = Animator.StringToHash("IsCharacterSelected");

		private Animator? _manAnimatorController;
		private Animator? _womanAnimatorController;

		private readonly Vector3 _avatarJigPosition = new(0.89f, 0, -4.1f);

		private readonly float _avatarCreateDelay = 0.1033333f;
		private readonly float _avatarCreateDelay2 = 0.1833333f;

		private bool _responseCreateAvatar;

		[SerializeField] private GameObject _directingObject = null!;

		[SerializeField] private AssetReference _avatarJigPrefabReference = null!;

		[Header("Avatar Npc")]
		[SerializeField] private Transform _manRoot = null!;
		[SerializeField] private Transform _womanRoot = null!;

		[Header("Directing")]
		[SerializeField] private AssetReference _avatarCreateFxReference = null!;
		[SerializeField] private AssetReference _avatarSelectFxReference = null!;

		[Space]
		[SerializeField] private Transform _manAvatarCreateTransform = null!;
		[SerializeField] private Transform _womanAvatarCreateTransform = null!;

		[Space]
		[SerializeField] private Transform _manAvatarSelectTransform = null!;
		[SerializeField] private Transform _womanAvatarSelectTransform = null!;

		[SerializeField] private Animator _cameraJigAnimator = null!;

		[Header("Interaction")]
		[SerializeField] private float _zoomDuration = 1.1f;

		[SerializeField] private float _rotateMarkerHideTime = 3f;

		private ActionMapUIControl? _actionMapUIControl;

		private AvatarJigController? _avatarJigController;

		private Animator? _manRootAnimator;
		private Animator? _womanRootAnimator;

		private GameObject? _avatarCreateFxPrefab;
		private GameObject? _avatarSelectFxPrefab;

		private readonly List<GameObject> _afterShockFxObjects = new();

		private bool _prevDitherValue;

		private AvatarSelectionManagerViewModel? _viewModel;

#region IAvatarClosetControllerProperties
		public eViewType StartView => eViewType.TYPE;

		private eAvatarType _selectedType = eAvatarType.PC01_W;

		public bool NeedAvatarCameraFrame => false;

		public bool IsOnUndoButton => false;

		public bool IsUseAdditionalInfoAtItem => false;

		public AvatarJigController? AvatarJigController => _avatarJigController;

		public float RotateMarkerHideTime => _rotateMarkerHideTime;
#endregion IAvatarClosetControllerProperties

#region UnityEvent
		protected override void AwakeInvoked()
		{
			InputSystemManagerHelper.ChangeState(eInputSystemState.UI);

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

			_manRootAnimator   = _manRoot.GetComponent<Animator>();
			_womanRootAnimator = _womanRoot.GetComponent<Animator>();

			PacketReceiver.Instance.OnCreateAvatarResponseEvent += ResponseCreateAvatar;
			_responseCreateAvatar = false;

			IAvatarClosetController.AvatarClosetSetting.SetSetting();
		}

		private void ShowDirecting()
		{
			_directingObject.SetActive(true);
		}

		private async UniTask WaitLoadTable()
		{
			await UniTask.WaitUntil(IsTableLoaded);
			if (_avatarJigController.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "AvatarJigController is null!");
				return;
			}

			var avatarManger = AvatarMediator.Instance;
			avatarManger.AvatarCloset.Initialize(new AvatarCreateInventory(), _avatarJigController!, this);
			ShowDirecting();

			_viewModel ??= ViewModelManager.Instance.GetOrAdd<AvatarSelectionManagerViewModel>();
			_viewModel.OnEnable();
		}

		private bool IsTableLoaded() => AvatarTable.IsTableLoaded;

		private void Update()
		{
			if (Input.GetMouseButtonDown(0))
				RaycastCharacter();

			_viewModel ??= ViewModelManager.Instance.Get<AvatarSelectionManagerViewModel>();
			_viewModel?.OnUpdate();
		}

		protected override void OnApplicationQuitInvoked() => Destroy();
		protected override void OnDestroyInvoked()         => Destroy();

		private void Destroy()
		{
			IAvatarClosetController.AvatarClosetSetting.ResetSetting();

			if (_actionMapUIControl != null)
			{
				_actionMapUIControl.LeftDragAction  -= OnDragActionChanged;
				_actionMapUIControl.RightDragAction -= OnDragActionChanged;
				_actionMapUIControl                 =  null;
			}

			if (PacketReceiver.InstanceExists)
				PacketReceiver.Instance.OnCreateAvatarResponseEvent -= ResponseCreateAvatar;
			AvatarMediator.InstanceOrNull?.AvatarCloset.Clear();
		}
#endregion UnityEvent

#region Load Assets
		private async UniTask LoadAvatarJig()
		{
			GameObject? avatarJigPrefab = null;
			// var avatarJigPrefabHandle = C2VAddressables.LoadAssetAsync<GameObject>(_avatarJigPrefabReference);
			// if (avatarJigPrefabHandle != null)
			// 	avatarJigPrefab = await avatarJigPrefabHandle.ToUniTask();

			avatarJigPrefab = await RuntimeObjectManager.Instance.LoadAssetAsyncAwait<GameObject>(_avatarJigPrefabReference);
			if (avatarJigPrefab.IsReferenceNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"Failed to load avatarJigPrefab!");
				return;
			}

			var jigObject = Instantiate(avatarJigPrefab, transform);
			_avatarJigController = Util.GetOrAddComponent<AvatarJigController>(jigObject!.gameObject!);

			_avatarJigController.transform.position = _avatarJigPosition;
		}

		public async UniTask LoadResources()
		{
			await LoadFxAssets();
			await LoadAvatarJig();
			await WaitLoadTable();
			ShowStartDirecting().Forget();
		}

		private async UniTask LoadFxAssets()
		{
			// var avatarCreateFxHandle = C2VAddressables.LoadAssetAsync<GameObject>(_avatarCreateFxReference);
			// var avatarSelectFxHandle = C2VAddressables.LoadAssetAsync<GameObject>(_avatarSelectFxReference);
			//
			// if (avatarCreateFxHandle != null)
			// 	_avatarCreateFxPrefab = await avatarCreateFxHandle.ToUniTask();
			// if (avatarSelectFxHandle != null)
			// 	_avatarSelectFxPrefab = await avatarSelectFxHandle.ToUniTask();
			
			_avatarCreateFxPrefab = await RuntimeObjectManager.Instance.LoadAssetAsyncAwait<GameObject>(_avatarCreateFxReference);
			_avatarSelectFxPrefab = await RuntimeObjectManager.Instance.LoadAssetAsyncAwait<GameObject>(_avatarSelectFxReference);
		}

		private async UniTask LoadNpcPrefabs()
		{
			await SetStartAnimationAvatar(eAvatarType.PC01_M);
			await SetStartAnimationAvatar(eAvatarType.PC01_W);
			CompleteLoad();
		}

		/// <summary>
		/// 비동기 로딩이 모두 완료되면 호출한다.
		/// </summary>
		/// <param name="go"></param>
		private void CompleteLoad()
		{
			PlayStartAnimation().Forget();
			Sound.SoundManager.Instance.PlayUISound("SE_PC01_Entry_01.wav");
		}

		/// <summary>
		/// 연출용 아바타를 생성합니다.
		/// CustomizeLayer을 조절해야 해서 Npc프리팹이 아닌 Default 테이블에 입력된 AvatarInfo를 기반으로 생성합니다.
		/// </summary>
		/// <param name="avatarType"></param>
		private async UniTask SetStartAnimationAvatar(eAvatarType avatarType)
		{
			var isMan      = avatarType == eAvatarType.PC01_M;
			var avatarInfo = AvatarManager.Instance.GetDefaultAvatarInfo(avatarType);

			var avatarController = await AvatarCreator.CreateAvatarAsync(avatarInfo, eAnimatorType.AVATAR_CUSTOMIZE, Vector3.zero, (int)Define.eLayer.CHARACTER);
			if (avatarController.IsReferenceNull())
			{
				C2VDebug.LogError(GetType().Name, "Can't find avatarController");
				return;
			}

			var avatarRoot   = isMan ? _manRoot : _womanRoot;
			var avatarObject = avatarController!.gameObject;
			var avatarTransform = avatarObject.transform;

			avatarTransform.SetParent(avatarRoot, false);
			avatarTransform.localPosition = Vector3.zero;
			avatarTransform.localRotation = Quaternion.identity;
			avatarObject.SetActive(false);

			var animator = Util.GetOrAddComponent<Animator>(avatarController.gameObject);
			if (isMan) _manAnimatorController = animator;
			else _womanAnimatorController     = animator;

			var coll = avatarObject.AddComponent<CapsuleCollider>()!;
			var collCenterX = isMan ? -AvatarColliderCenterXDiff : AvatarColliderCenterXDiff;
			coll.center = Vector3.up * (AvatarColliderHeight * 0.5f) + Vector3.right * collCenterX;
			coll.height = AvatarColliderHeight;
			coll.radius = AvatarColliderRadius;

			var mainCamera = Camera.main;
			if (!mainCamera.IsUnityNull())
			{
				avatarController.SetFaceCustomizeEnable(true);
				avatarController.EnableLookAtEye(mainCamera!.transform);
			}
		}

		private async UniTask PlayAvatarCreateFx(Transform createRoot, Transform selectRoot)
		{
			var avatarCreateFxObject = Instantiate(_avatarCreateFxPrefab, createRoot);

			if (!avatarCreateFxObject.IsReferenceNull())
			{
				await UniTask.Delay(TimeSpan.FromSeconds(AvatarCreateFxDuration));
				Destroy(avatarCreateFxObject!);

				if (createRoot.gameObject.activeSelf)
					PlayAfterShockFx(selectRoot);
			}
		}

		private void PlayAfterShockFx(Transform root)
		{
			var avatarSelectFxObject = Instantiate(_avatarSelectFxPrefab, root);

			if (!avatarSelectFxObject.IsReferenceNull())
				_afterShockFxObjects.Add(avatarSelectFxObject!);
		}

		private void ClearAvatarSelectFx()
		{
			foreach (var afterShockFxObject in _afterShockFxObjects)
				UnityEngine.Object.Destroy(afterShockFxObject);
			_afterShockFxObjects.Clear();
		}
#endregion Load Assets

#region AvatarAnimation
		private async UniTask ShowStartDirecting()
		{
			var loadingPage = UIManager.Instance.GetSystemView(eSystemViewType.UI_LOADING_PAGE);
			if (!loadingPage.IsUnityNull())
				await UniTask.WaitUntil(() => loadingPage!.VisibleState == GUIView.eVisibleState.CLOSING);

			_viewModel ??= ViewModelManager.Instance.Get<AvatarSelectionManagerViewModel>();
			_viewModel?.PlayEnteringDirecting().Forget();

			await UniTask.Delay(TimeSpan.FromSeconds(_avatarCreateDelay));
			await LoadNpcPrefabs();
			// PlayStartAnimation().Forget();
			// Sound.SoundManager.Instance.PlayUISound("SE_PC01_Entry_01.wav");
		}

		private async UniTask PlayStartAnimation()
		{
			if (!_manAnimatorController.IsUnityNull())
				_manAnimatorController!.gameObject.SetActive(false);

			if (!_womanAnimatorController.IsUnityNull())
				_womanAnimatorController!.gameObject.SetActive(false);

			await UniTask.NextFrame();

			_cameraJigAnimator.SetBool(_isEndLoadingHash, true);

			if (!_manAnimatorController.IsUnityNull())
			{
				_manAnimatorController!.gameObject.SetActive(true);
				_manAnimatorController.SetBool(AnimationDefine.HashIsSelected, false);
				_manAnimatorController.SetTrigger(AnimationDefine.HashSetStart);
				PlayAvatarCreateFx(_manAvatarCreateTransform, _manAvatarSelectTransform).Forget();
			}

			await UniTask.Delay(TimeSpan.FromSeconds(_avatarCreateDelay2));

			if (!_womanAnimatorController.IsUnityNull())
			{
				_womanAnimatorController!.gameObject.SetActive(true);
				_womanAnimatorController.SetBool(AnimationDefine.HashIsSelected, false);
				_womanAnimatorController.SetTrigger(AnimationDefine.HashSetStart);
				PlayAvatarCreateFx(_womanAvatarCreateTransform, _womanAvatarSelectTransform).Forget();
			}
		}

		private void SetAnimatorIsSelected(eAvatarType avatarType)
		{
			if (!_manAnimatorController.IsUnityNull())
				_manAnimatorController!.SetBool(AnimationDefine.HashIsSelected, avatarType == eAvatarType.PC01_M);

			if (!_manRootAnimator.IsUnityNull())
				_manRootAnimator!.SetBool(AnimationDefine.HashIsSelected, avatarType == eAvatarType.PC01_M);

			if (!_womanAnimatorController.IsUnityNull())
				_womanAnimatorController!.SetBool(AnimationDefine.HashIsSelected, avatarType == eAvatarType.PC01_W);

			if (!_womanRootAnimator.IsUnityNull())
				_womanRootAnimator!.SetBool(AnimationDefine.HashIsSelected, avatarType == eAvatarType.PC01_W);

			_cameraJigAnimator.SetBool(_isCharacterSelectedHash, true);
		}
#endregion AvatarAnimation

#region Interaction
		private void OnDragActionChanged(Vector2 value)
		{
			if (_avatarJigController.IsUnityNull()) return;

			_avatarJigController!.ReserveRotate(value.x);
		}
#endregion Interaction

		private void RaycastCharacter()
		{
			var cameraManager = CameraManager.InstanceOrNull;
			if (cameraManager == null) return;

			var mainCamera = cameraManager.MainCamera;
			if (mainCamera.IsUnityNull()) return;

			var ray = mainCamera!.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out RaycastHit hitInfo, AvatarSelectRayMaxDistance, Define.LayerMask(Define.eLayer.CHARACTER)))
			{
				if (hitInfo.transform.IsReferenceNull()) return;

				OnClickCharacter(hitInfo.transform!);
			}
		}

		private void OnClickCharacter(Transform character)
		{
			var avatarController = character.GetComponent<AvatarController>();
			if (avatarController.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "AvatarController is null.");
				return;
			}

			var avatarInfo = avatarController!.Info;
			if (avatarController.Info == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "AvatarInfo is null.");
				return;
			}

			_selectedType = avatarInfo!.AvatarType;
			ViewModelManager.Instance.GetOrAdd<AvatarSelectionTypeViewModel>().OnCharacterClicked(avatarInfo.AvatarType);
			SetAnimatorIsSelected(avatarInfo.AvatarType);
		}

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
			ClearAvatarSelectFx();
		}

		private void OnCustomize()
		{
			_directingObject.SetActive(false);
			ClearAvatarSelectFx();
		}

		public void UpdateCustomize(eViewType type)
		{
			_viewModel ??= ViewModelManager.Instance.Get<AvatarSelectionManagerViewModel>();
			switch (type)
			{
				case eViewType.TYPE:
					OnCustomize();
					_viewModel?.SetNextType();
					break;
				case eViewType.BODY:
				case eViewType.FACE:
					_viewModel?.SetNextType();
					break;
				case eViewType.FASHION:
					if (_responseCreateAvatar)
						LoginManager.Instance.CheckLoginQueue(ReadyToEnter, true);
					else
						NicknameRule.ShowNicknameRulePopup();
					break;
			}
		}

		public AvatarInfo GetAvatarInfo()
		{
			var avatarManager = AvatarMediator.Instance;
			return avatarManager.AvatarCloset.GetDefaultAvatarInfo(_selectedType);
		}

		public void OnCreatedViewModel()
		{
		}

		public void OnCreatedAvatar(AvatarController avatarController)
		{
			if (_avatarJigController.IsUnityNull())
				return;

			PlayAfterShockFx(_avatarJigController!.FxJig);
			_avatarJigController!.ResetRotationImmediate();
		}

		public void OnClickBackButton(eViewType type)
		{
			UIManager.Instance.ShowPopupYesNo(Localization.Instance.GetString(BackPopupTitleKey),
			                                  Localization.Instance.GetString(BackPopupMessageKey),
			                                  OnClickBackButtonConfirm);
		}

		private void OnClickBackButtonConfirm(GUIView _)
		{
			NetworkManager.Instance.Disconnect(true);
			LoadingManager.Instance.ChangeScene<SceneLogin>();
		}

		public void ChangeTypeTap(eViewType prevType, eViewType nextType, Action? typeChangeAction)
		{
			switch (nextType)
			{
				case eViewType.TYPE:
					UIManager.Instance.ShowPopupYesNo(Localization.Instance.GetString(ChangeTypePopupTitleKey),
					                                  Localization.Instance.GetString(ChangeTypePopupMessageKey),
					                                  _ =>
					                                  {
						                                  typeChangeAction?.Invoke();
						                                  ShowDirecting();
						                                  PlayStartAnimation().Forget();
					                                  });
					break;
				case eViewType.FACE:
				case eViewType.BODY:
				case eViewType.FASHION:
					typeChangeAction?.Invoke();
					break;
			}
		}

		private void ResponseCreateAvatar(CreateAvatarResponse response)
		{
			_responseCreateAvatar = true;
		}

		public void ReadyToEnter()
		{
			NetworkUIManager.Instance.OnFieldChangeEvent += OnFieldChange;
			Commander.Instance.RequestEnterWorld();
			UIManager.Instance.ShowWaitingResponsePopup();
		}

		private void OnFieldChange()
		{
			NetworkUIManager.Instance.OnFieldChangeEvent -= OnFieldChange;
			UIManager.Instance.HideWaitingResponsePopup();
		}
	}
}
