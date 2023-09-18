/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarSelectionManagerViewModel.cs
* Developer:	eugene9721
* Date:			2023-03-17 16:42
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using UnityEngine;
using System.Collections.Generic;
using Com2Verse.Avatar;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using JetBrains.Annotations;
using Protocols.CommonLogic;
using UnityEngine.UI;

namespace Com2Verse.UI
{
	[ViewModelGroup("AvatarCustomize")]
	public sealed class AvatarSelectionManagerViewModel : ViewModelBase, IDisposable
	{
		private const float EnteringDirectingDelay    = 3.5f;
		private const float EnteringDirectingDuration = 0.6f;
		private const float EnteringPositionPadding   = 5f;

		public const string TypeSubTitleTextKey            = "UI_AvatarCreate_Msg_Type";
		public const string FaceFirstScreenSubTitleTextKey = "UI_AvatarCreate_Face_Msg_FirstScreen";
		public const string FaceNoCamSubTitleTextKey       = "UI_AvatarCreate_Face_Msg_NoCam";
		public const string FaceTakePictureSubTitleTextKey = "UI_AvatarCreate_Face_Msg_TakePicture";
		public const string FaceAnalyzingSubTitleTextKey   = "UI_AvatarCreate_Face_Msg_Analyzing";
		public const string FaceAnalyzedSubTitleTextKey    = "UI_AvatarCreate_Face_Msg_Analyzed";
		public const string FaceEditPictureSubTitleTextKey = "UI_AvatarCreate_Face_Msg_EditPicture";
		public const string FaceEditFaceSubTitleTextKey    = "UI_AvatarCreate_Face_Msg_EditFace";
		public const string FaceBodyShapeSubTitleTextKey   = "UI_AvatarCreate_Face_Msg_BodyShape";
		public const string FaceFashionSubTitleTextKey     = "UI_AvatarCreate_Face_Msg_Fashion";

		private const string AvatarUpdateToastTextKey = "UI_AvatarCustomize_Toast_Msg_Save";

		public enum eStepButtonState
		{
			DISABLE    = 0,
			COMPLETION = 1,
			CHOICE     = 2,
		}

		private readonly Vector2 _nextButtonPositionOnTypeSelect = new(0, -32);
		private readonly Vector2 _nextButtonPositionOnCustomize  = new(-220.8001f, -32);

		private StackRegisterer? _guiRegisterer;

		public StackRegisterer? GuiRegisterer
		{
			get => _guiRegisterer;
			set
			{
				_guiRegisterer             =  value;
				if (value.IsUnityNull()) return;
				_guiRegisterer!.WantsToQuit += OnBackButtonClick;
			}
		}

#region Fields
		private eStepButtonState _typeStepButtonState;
		private eStepButtonState _faceStepButtonState;
		private eStepButtonState _bodyStepButtonState;
		private eStepButtonState _fashionStepButtonState;

		private readonly Dictionary<eViewType, AvatarSelectionViewModelBase> _avatarSelectionViewModels = new();

		private eViewType _currentViewType = eViewType.TYPE;

		private bool _isCreatedAvatar;

		private Collection<CustomizeMenuViewModel> _itemMenuList = new();

		private string _subTitleTextKey = string.Empty;

		private Texture? _avatarTexture;

		private RawImage? _rawImage;

		private RectTransform? _topRoot;
		private RectTransform? _leftRoot;
		private RectTransform? _bottomRoot;

		private bool _isSelectedPreset;

		private bool _isWaitingCreatePreviewAvatar;

		private float _rotateMarkerAlpha = 1;
		private float _rotateMarkerDisplayTime;

		private bool _isOnLookAt = true;
#endregion Fields

#region Command Properties
		[UsedImplicitly] public CommandHandler TypeButtonClick    { get; }
		[UsedImplicitly] public CommandHandler FaceButtonClick    { get; }
		[UsedImplicitly] public CommandHandler BodyButtonClick    { get; }
		[UsedImplicitly] public CommandHandler FashionButtonClick { get; }

		[UsedImplicitly] public CommandHandler BackToLobbyButtonClick { get; }
		[UsedImplicitly] public CommandHandler NextButtonClick { get; }

		/// <summary>
		/// 바라보기 버튼 클릭
		/// </summary>
		[UsedImplicitly] public CommandHandler<bool> LookAtToggleClick { get; private set; }

		/// <summary>
		/// 감정표현(제스쳐) 버튼 클릭
		/// </summary>
		[UsedImplicitly] public CommandHandler GestureButtonClick { get;     private set; }
		[UsedImplicitly] public CommandHandler AvatarUndoButtonClick  { get; private set; }
		[UsedImplicitly] public CommandHandler AvatarRedoButtonClick  { get; private set; }
		[UsedImplicitly] public CommandHandler AvatarResetButtonClick { get; private set; }
		[UsedImplicitly] public CommandHandler BackButtonClick        { get; private set; }
#endregion Command Properties

#region Field Properties
		[UsedImplicitly]
		public Collection<CustomizeMenuViewModel> ItemMenuList
		{
			get => _itemMenuList;
			set => SetProperty(ref _itemMenuList, value);
		}

		[UsedImplicitly]
		public bool IsOnType => _typeStepButtonState == eStepButtonState.CHOICE;

		[UsedImplicitly]
		public bool IsDisableType => _typeStepButtonState == eStepButtonState.DISABLE;

		[UsedImplicitly]
		public bool IsCompletionType => _typeStepButtonState == eStepButtonState.COMPLETION;

		[UsedImplicitly]
		public bool IsOnFace => _faceStepButtonState == eStepButtonState.CHOICE;

		[UsedImplicitly]
		public bool IsDisableFace => _faceStepButtonState == eStepButtonState.DISABLE;

		[UsedImplicitly]
		public bool IsCompletionFace => _faceStepButtonState == eStepButtonState.COMPLETION;

		[UsedImplicitly]
		public bool IsOnBody => _bodyStepButtonState == eStepButtonState.CHOICE;

		[UsedImplicitly]
		public bool IsDisableBody => _bodyStepButtonState == eStepButtonState.DISABLE;

		[UsedImplicitly]
		public bool IsCompletionBody => _bodyStepButtonState == eStepButtonState.COMPLETION;

		[UsedImplicitly]
		public bool IsOnFashion => _fashionStepButtonState == eStepButtonState.CHOICE;

		[UsedImplicitly]
		public bool IsDisableFashion => _fashionStepButtonState == eStepButtonState.DISABLE;

		[UsedImplicitly]
		public bool IsCompletionFashion => _fashionStepButtonState == eStepButtonState.COMPLETION;

		[UsedImplicitly]
		public Vector2 NextButtonPosition => IsOnType ? _nextButtonPositionOnTypeSelect : _nextButtonPositionOnCustomize;

		[UsedImplicitly]
		public int TypeStepButtonState
		{
			get => (int)_typeStepButtonState;
			set
			{
				SetProperty(ref _typeStepButtonState, (eStepButtonState)value);
				InvokePropertyValueChanged(nameof(IsOnType),               IsOnType);
				InvokePropertyValueChanged(nameof(IsDisableType),          IsDisableType);
				InvokePropertyValueChanged(nameof(IsCompletionType),       IsCompletionType);
				InvokePropertyValueChanged(nameof(IsOnAiRegenerateButton), IsOnAiRegenerateButton);
				InvokePropertyValueChanged(nameof(NextButtonPosition),     NextButtonPosition);
			}
		}

		[UsedImplicitly]
		public int FaceStepButtonState
		{
			get => (int)_faceStepButtonState;
			set
			{
				SetProperty(ref _faceStepButtonState, (eStepButtonState)value);
				InvokePropertyValueChanged(nameof(IsOnFace),               IsOnFace);
				InvokePropertyValueChanged(nameof(IsDisableFace),          IsDisableFace);
				InvokePropertyValueChanged(nameof(IsCompletionFace),       IsCompletionFace);
				InvokePropertyValueChanged(nameof(IsOnAiRegenerateButton), IsOnAiRegenerateButton);
				InvokePropertyValueChanged(nameof(NextButtonPosition),     NextButtonPosition);
			}
		}

		[UsedImplicitly]
		public int BodyStepButtonState
		{
			get => (int)_bodyStepButtonState;
			set
			{
				SetProperty(ref _bodyStepButtonState, (eStepButtonState)value);
				InvokePropertyValueChanged(nameof(IsOnBody),               IsOnBody);
				InvokePropertyValueChanged(nameof(IsDisableBody),          IsDisableBody);
				InvokePropertyValueChanged(nameof(IsCompletionBody),       IsCompletionBody);
				InvokePropertyValueChanged(nameof(IsOnAiRegenerateButton), IsOnAiRegenerateButton);
				InvokePropertyValueChanged(nameof(NextButtonPosition),     NextButtonPosition);
			}
		}

		[UsedImplicitly]
		public int FashionStepButtonState
		{
			get => (int)_fashionStepButtonState;
			set
			{
				SetProperty(ref _fashionStepButtonState, (eStepButtonState)value);
				InvokePropertyValueChanged(nameof(IsOnFashion),            IsOnFashion);
				InvokePropertyValueChanged(nameof(IsDisableFashion),       IsDisableFashion);
				InvokePropertyValueChanged(nameof(IsCompletionFashion),    IsCompletionFashion);
				InvokePropertyValueChanged(nameof(IsOnAiRegenerateButton), IsOnAiRegenerateButton);
				InvokePropertyValueChanged(nameof(NextButtonPosition),     NextButtonPosition);
			}
		}

		/// <summary>
		/// 다음 버튼의 게임오브젝트를 활성화 할지 판단
		/// </summary>
		[UsedImplicitly]
		public bool OnNextButton
		{
			get
			{
				var avatarEditData = AvatarMediator.InstanceOrNull?.AvatarCloset.AvatarEditData;
				if (avatarEditData == null) return false;

				return _currentViewType switch
				{
					eViewType.TYPE    => avatarEditData.HasAvatarType,
					eViewType.FACE    => avatarEditData.HasFaceItem && (AvatarMediator.InstanceOrNull?.AvatarCloset.IsFaceEditing ?? false),
					eViewType.BODY    => avatarEditData.HasBodyType,
					eViewType.FASHION => avatarEditData.HasFashionItem,
					_                 => false
				};
			}
		}

		/// <summary>
		/// 다음 버튼의 interactable을 활성화 할지 판단
		/// </summary>
		[UsedImplicitly]
		public bool IsActiveNextButton
		{
			get
			{
				if (!IsCreateScene && IsCreatedAvatar)
					return AvatarCloset.CurrentAvatarInfo != AvatarMediator.InstanceOrNull?.UserAvatarInfo;

				return true;
			}
		}

		[UsedImplicitly]
		public bool IsCreatedAvatar
		{
			get => _isCreatedAvatar;
			set => SetProperty(ref _isCreatedAvatar, value);
		}

		[UsedImplicitly]
		public string AvatarSelectionTitleText => Localization.Instance.GetString(AvatarCloset.Controller?.TitleTextKey ?? "");

		[UsedImplicitly]
		public string AvatarSelectionButtonText => Localization.Instance.GetString(AvatarCloset.Controller?.ButtonTextKey(_currentViewType) ?? "");

		[UsedImplicitly]
		public string SubTitleTextKey
		{
			get => _subTitleTextKey;
			set
			{
				SetProperty(ref _subTitleTextKey, value);
				InvokePropertyValueChanged(nameof(SubTitle), SubTitle);
			}
		}

		[UsedImplicitly]
		public string SubTitle => Localization.Instance.GetString(_subTitleTextKey);

		public AvatarCloset AvatarCloset { get; }

		public bool IsCreateScene { get; }

		[UsedImplicitly] public bool IsOnUndoButton => AvatarCloset.Controller?.IsOnUndoButton ?? false;

		[UsedImplicitly]
		public RectTransform? TopRoot
		{
			get => _topRoot;
			set => SetProperty(ref _topRoot, value);
		}

		[UsedImplicitly]
		public RectTransform? LeftRoot
		{
			get => _leftRoot;
			set => SetProperty(ref _leftRoot, value);
		}

		[UsedImplicitly]
		public RectTransform? BottomRoot
		{
			get => _bottomRoot;
			set => SetProperty(ref _bottomRoot, value);
		}

		[UsedImplicitly]
		public bool IsOnAiRegenerateButton => IsSelectedPreset && IsOnFace;

		public bool IsSelectedPreset
		{
			get => _isSelectedPreset;
			set
			{
				_isSelectedPreset = value;
				InvokePropertyValueChanged(nameof(IsOnAiRegenerateButton), IsOnAiRegenerateButton);
			}
		}

		[UsedImplicitly]
		public float RotateMarkerAlpha
		{
			get => _rotateMarkerAlpha;
			set => SetProperty(ref _rotateMarkerAlpha, value);
		}
#endregion Field Properties

#region Initialize
		public AvatarSelectionManagerViewModel()
		{
			TypeButtonClick    = new CommandHandler(OnTypeButtonClick);
			FaceButtonClick    = new CommandHandler(OnFaceButtonClick);
			BodyButtonClick    = new CommandHandler(OnBodyButtonClick);
			FashionButtonClick = new CommandHandler(OnFashionButtonClick);

			BackToLobbyButtonClick = new CommandHandler(OnBackToLobbyButtonClick);

			NextButtonClick = new CommandHandler(OnNextButtonClick);

			LookAtToggleClick  = new CommandHandler<bool>(OnLookAtToggleClick);
			GestureButtonClick = new CommandHandler(OnGestureButtonClick);

			AvatarUndoButtonClick  = new CommandHandler(OnAvatarUndoButtonClick);
			AvatarRedoButtonClick  = new CommandHandler(OnAvatarRedoButtonClick);
			AvatarResetButtonClick = new CommandHandler(OnAvatarResetButtonClick);
			BackButtonClick        = new CommandHandler(OnBackButtonClick);

			AvatarCloset  = AvatarMediator.Instance.AvatarCloset;
			IsCreateScene = AvatarCloset.Controller is AvatarCreateManager;
			AvatarCloset.Controller?.OnCreatedViewModel();

			PacketReceiver.Instance.OnUpdateAvatarResponseEvent += OnUpdateAvatarResponse;

			InitializeDict();
		}

		public void OnEnable()
		{
			AvatarCloset.OnAvatarItemInfoChanged += OnAvatarItemInfoChanged;
			AvatarCloset.OnAvatarCreated         += OnAvatarCreated;

			InitializeActionSystem();
			var controller = AvatarCloset.Controller!;
			ChangeCurrentViewType(controller.StartView);

			InvokePropertyValueChanged(nameof(AvatarSelectionTitleText), AvatarSelectionTitleText);
			InvokePropertyValueChanged(nameof(IsOnUndoButton),           IsOnUndoButton);
			RefreshButtons();

			var avatarCloset = AvatarMediator.Instance.AvatarCloset;
			avatarCloset.EnableLookAt(_isOnLookAt);
			avatarCloset.ChangeCurrentView(controller.StartView);
		}

		/// <summary>
		/// 액션시스템 초기화 시점
		/// 1. 탭 변경시
		/// 2. 나가기 클릭시 -> OnDisable()
		/// 3. 아바타 저장시 (아바타 앱)
		/// </summary>
		private void InitializeActionSystem(bool isDisable = false)
		{
			if (isDisable)
				ActionSystem.InstanceOrNull?.Initialize();
			else
				ActionSystem.Instance.Initialize();
			RefreshButtons();
		}

		public void OnDisable()
		{
			var avatarMediator = AvatarMediator.InstanceOrNull;
			if (avatarMediator != null)
				avatarMediator.AvatarCloset.OnAvatarItemInfoChanged -= OnAvatarItemInfoChanged;

			OnAvatarResetButtonClick();
			InitializeActionSystem(true);
		}

		public void Dispose()
		{
			OnDisable();
			Clear();
			var packetReceiver = PacketReceiver.InstanceOrNull;
			if (packetReceiver != null)
				packetReceiver.OnUpdateAvatarResponseEvent -= OnUpdateAvatarResponse;

			var avatarMediator = AvatarMediator.InstanceOrNull;
			if (avatarMediator != null)
				avatarMediator.AvatarCloset.OnAvatarCreated -= OnAvatarCreated;
		}

		private void InitializeDict()
		{
			_avatarSelectionViewModels.Add(eViewType.TYPE,    ViewModelManager.Instance.GetOrAdd<AvatarSelectionTypeViewModel>());
			_avatarSelectionViewModels.Add(eViewType.FACE,    ViewModelManager.Instance.GetOrAdd<AvatarSelectionFaceViewModel>());
			_avatarSelectionViewModels.Add(eViewType.BODY,    ViewModelManager.Instance.GetOrAdd<AvatarSelectionBodyViewModel>());
			_avatarSelectionViewModels.Add(eViewType.FASHION, ViewModelManager.Instance.GetOrAdd<AvatarSelectionFashionViewModel>());

			_avatarSelectionViewModels[eViewType.TYPE]!.Initialize(this);
			_avatarSelectionViewModels[eViewType.FACE]!.Initialize(this);
			_avatarSelectionViewModels[eViewType.BODY]!.Initialize(this);
			_avatarSelectionViewModels[eViewType.FASHION]!.Initialize(this);
		}

		public bool CanAvatarCustomizeRedo => ActionSystem.InstanceOrNull?.CanRedo ?? false;
		public bool CanAvatarCustomizeUndo => ActionSystem.InstanceOrNull?.CanUndo ?? false;
		public bool CanAvatarCustomizeReset
		{
			get
			{
				var userInfo   = AvatarMediator.InstanceOrNull?.UserAvatarInfo;
				var closetInfo = AvatarMediator.InstanceOrNull?.AvatarCloset.CurrentAvatarInfo;
				if (userInfo == null || closetInfo == null)
					return false;

				// FIXME: 임시처리, 제거 필요
				userInfo.RemoveFaceItem(eFaceOption.PRESET_LIST);
				closetInfo.RemoveFaceItem(eFaceOption.PRESET_LIST);

				return userInfo != closetInfo;
			}
		}

		public async UniTask ShowPreviewAvatarAsync(Action? callback = null)
		{
			if (_isWaitingCreatePreviewAvatar) return;

			if (!IsCreatedAvatar)
			{
				_isWaitingCreatePreviewAvatar = true;
				var avatarInfo   = AvatarCloset.CurrentAvatarInfo;

				if (avatarInfo == null)
					avatarInfo = AvatarTable.GetBaseAvatarInfo(AvatarCloset.AvatarEditData.AvatarType);
				await AvatarCloset.CreateAvatar(avatarInfo, true);
			}

			_isWaitingCreatePreviewAvatar = false;
			callback?.Invoke();
		}
#endregion Initialize

#region Handlers
		private void OnTypeButtonClick()
		{
			if (_typeStepButtonState == eStepButtonState.DISABLE)
			{
				ShowToastMessageWhenClickedDisableButton();
				return;
			}

			if (_currentViewType == eViewType.TYPE)
				return;

			AvatarCloset.Controller?.ChangeTypeTap(_currentViewType, eViewType.TYPE, () =>
			{
				Clear();
				ChangeCurrentViewType(eViewType.TYPE);
			});
		}

		private void OnFaceButtonClick()
		{
			if (_faceStepButtonState == eStepButtonState.DISABLE)
			{
				ShowToastMessageWhenClickedDisableButton();
				return;
			}

			if (_currentViewType == eViewType.FACE)
				return;

			AvatarCloset.Controller?.ChangeTypeTap(_currentViewType, eViewType.FACE, () => ChangeCurrentViewType(eViewType.FACE));
		}

		private void OnBodyButtonClick()
		{
			if (_bodyStepButtonState == eStepButtonState.DISABLE)
			{
				ShowToastMessageWhenClickedDisableButton();
				return;
			}

			if (_currentViewType == eViewType.BODY)
				return;

			AvatarCloset.Controller?.ChangeTypeTap(_currentViewType, eViewType.BODY, () => ChangeCurrentViewType(eViewType.BODY));
		}

		private void OnFashionButtonClick()
		{
			if (_fashionStepButtonState == eStepButtonState.DISABLE)
			{
				ShowToastMessageWhenClickedDisableButton();
				return;
			}

			if (_currentViewType == eViewType.FASHION)
				return;

			AvatarCloset.Controller?.ChangeTypeTap(_currentViewType, eViewType.FASHION, () => ChangeCurrentViewType(eViewType.FASHION));
		}

		private void ShowToastMessageWhenClickedDisableButton()
		{
			var controller = AvatarMediator.Instance.AvatarCloset.Controller;
			if (controller == null || string.IsNullOrEmpty(controller.DisableToastTextKey!))
			{
				C2VDebug.LogWarning(GetType().Name, "controller or DisableToastTextKey is null");
				return;
			}

			UIManager.Instance.SendToastMessage(Localization.Instance.GetString(controller.DisableToastTextKey), toastMessageType: UIManager.eToastMessageType.WARNING);
		}

		private void OnBackToLobbyButtonClick()
		{
			Clear();
		}

		private void OnNextButtonClick()
		{
			AvatarCloset.Controller?.UpdateCustomize(_currentViewType);
		}

		private void OnLookAtToggleClick(bool value)
		{
			_isOnLookAt = value;
			AvatarMediator.Instance.AvatarCloset.EnableLookAt(value);
		}

		private void OnGestureButtonClick()
		{
			var jigController = AvatarCloset.Controller?.AvatarJigController;
			if (!jigController.IsUnityNull())
			{
				// jigController!.PlayRandomGestureAsync(AvatarCloset.CurrentAvatarInfo.AvatarType).Forget();
				jigController!.PlayRandomGestureByAnimator();
			}
		}

		private void OnAvatarUndoButtonClick()
		{
			ActionSystem.Instance.Undo();
		}

		private void OnAvatarRedoButtonClick()
		{
			ActionSystem.Instance.Redo();
		}

		private void OnAvatarResetButtonClick()
		{
			if (!AvatarMediator.InstanceExists) return;
			switch (_currentViewType)
			{
				case eViewType.FACE:
					AvatarMediator.Instance.AvatarCloset.ResetCustomizeAvatar(AvatarCloset.eResetCustomizeOption.ONLY_FACE);
					break;
				case eViewType.BODY:
					AvatarMediator.Instance.AvatarCloset.ResetCustomizeAvatar(AvatarCloset.eResetCustomizeOption.ONLY_BODY);
					break;
				case eViewType.FASHION:
					AvatarMediator.Instance.AvatarCloset.ResetCustomizeAvatar(AvatarCloset.eResetCustomizeOption.ONLY_FASHION);
					break;
				default:
					AvatarMediator.Instance.AvatarCloset.ResetCustomizeAvatar();
					break;
			}
		}

		private void OnBackButtonClick()
		{
			AvatarCloset.Controller?.OnClickBackButton(_currentViewType);
		}
#endregion Handlers

#region Network
		private void OnUpdateAvatarResponse(UpdateAvatarResponse updateAvatarResponse)
		{
			if (updateAvatarResponse.UpdatedAvatar != null)
				AvatarMediator.Instance.SetUserAvatar(updateAvatarResponse.UpdatedAvatar);

			UIManager.Instance.SendToastMessage(Localization.Instance.GetString(AvatarUpdateToastTextKey), toastMessageType: UIManager.eToastMessageType.NORMAL);
			InitializeActionSystem();
		}
#endregion Network

		public void OnUpdate()
		{
			_rotateMarkerDisplayTime -= Time.deltaTime;
			_rotateMarkerDisplayTime =  Mathf.Max(_rotateMarkerDisplayTime, 0);
			RotateMarkerAlpha        =  Mathf.Lerp(RotateMarkerAlpha, _rotateMarkerDisplayTime > 0 ? 1 : 0, Time.deltaTime * 10);
		}

		private void OnAvatarCreated(AvatarController avatarController)
		{
			IsCreatedAvatar = true;
			InvokePropertyValueChanged(nameof(IsActiveNextButton), IsActiveNextButton);
		}

		private void OnAvatarItemInfoChanged(AvatarInfo avatarItemInfo)
		{
			foreach (var viewModel in _avatarSelectionViewModels.Values)
				viewModel.OnAvatarItemInfoChanged(avatarItemInfo);

			RefreshButtons();
		}

		public void RefreshButtons()
		{
			InvokePropertyValueChanged(nameof(OnNextButton),            OnNextButton);
			InvokePropertyValueChanged(nameof(IsActiveNextButton),      IsActiveNextButton);
			InvokePropertyValueChanged(nameof(CanAvatarCustomizeReset), CanAvatarCustomizeReset);
			InvokePropertyValueChanged(nameof(CanAvatarCustomizeRedo),  CanAvatarCustomizeRedo);
			InvokePropertyValueChanged(nameof(CanAvatarCustomizeUndo),  CanAvatarCustomizeUndo);
		}

		private void RefreshRotateMarker()
		{
			_rotateMarkerDisplayTime = AvatarCloset.Controller?.RotateMarkerHideTime ?? 0;
		}

		private void ChangeCurrentViewType(eViewType viewType)
		{
			if (!AvatarCloset.AvatarJig.IsUnityNull())
				AvatarCloset.AvatarJig!.SetNeedZoomRefresh();

			AvatarCloset.ChangeCurrentView(viewType);

			_avatarSelectionViewModels[_currentViewType]!.Hide();
			_avatarSelectionViewModels[viewType]!.Show();
			PlayChangeViewTypeAnimation(_currentViewType, viewType);
			_currentViewType = viewType;
			ChangeViewProperties(viewType);

			InvokePropertyValueChanged(nameof(AvatarSelectionButtonText), AvatarSelectionButtonText);
			RefreshRotateMarker();
			InitializeActionSystem();
			RefreshButtons();
		}

		private void PlayChangeViewTypeAnimation(eViewType prevType, eViewType nextType)
		{
			var controller = AvatarCloset.Controller?.AvatarJigController;
			if (controller.IsUnityNull()) return;

			if (prevType == eViewType.TYPE)
			{
				if (nextType == eViewType.FACE)
					controller!.SetAnimatorSelectHead();
				else if (nextType == eViewType.BODY)
					controller!.SetAnimatorSelectBody();
				else if (nextType == eViewType.FASHION)
					controller!.SetAnimatorSelectBody();
			}
			else if (prevType == eViewType.FACE)
			{
				if (nextType == eViewType.BODY)
					controller!.SetAnimatorSelectBody();
				else if (nextType == eViewType.FASHION)
					controller!.SetAnimatorSelectBody();
			}
			else if (prevType == eViewType.BODY)
			{
				if (nextType == eViewType.FACE)
					controller!.SetAnimatorSelectHead();
			}
			else if (prevType == eViewType.FASHION)
				if (nextType == eViewType.FACE)
					controller!.SetAnimatorSelectHead();
		}

		private void ChangeViewProperties(eViewType viewType)
		{
			if (IsCreateScene)
			{
				TypeStepButtonState = viewType == eViewType.TYPE           ? (int)eStepButtonState.CHOICE :
					_typeStepButtonState       == eStepButtonState.DISABLE ? (int)eStepButtonState.DISABLE : (int)eStepButtonState.COMPLETION;
			}
			else
			{
				// 6월 빌드 임시 기능
				TypeStepButtonState = (int)eStepButtonState.DISABLE;
			}
			FaceStepButtonState = viewType == eViewType.FACE           ? (int)eStepButtonState.CHOICE :
				_faceStepButtonState       == eStepButtonState.DISABLE ? (int)eStepButtonState.DISABLE : (int)eStepButtonState.COMPLETION;
			BodyStepButtonState = viewType == eViewType.BODY           ? (int)eStepButtonState.CHOICE :
				_bodyStepButtonState       == eStepButtonState.DISABLE ? (int)eStepButtonState.DISABLE : (int)eStepButtonState.COMPLETION;
			FashionStepButtonState = viewType == eViewType.FASHION        ? (int)eStepButtonState.CHOICE :
				_fashionStepButtonState       == eStepButtonState.DISABLE ? (int)eStepButtonState.DISABLE : (int)eStepButtonState.COMPLETION;
		}

		public void SetNextType()
		{
			switch (_currentViewType)
			{
				case eViewType.TYPE:
					ChangeCurrentViewType(eViewType.FACE);
					break;
				case eViewType.FACE:
					ChangeCurrentViewType(eViewType.BODY);
					break;
				case eViewType.BODY:
					ChangeCurrentViewType(eViewType.FASHION);
					break;
			}
		}

		private void ClearStepButtons()
		{
			TypeStepButtonState    = (int)eStepButtonState.DISABLE;
			FaceStepButtonState    = (int)eStepButtonState.DISABLE;
			BodyStepButtonState    = (int)eStepButtonState.DISABLE;
			FashionStepButtonState = (int)eStepButtonState.DISABLE;
		}

		private void Clear()
		{
			ClearStepButtons();

			foreach (var viewModel in _avatarSelectionViewModels.Values)
				viewModel.Clear();

			AvatarCloset.ClearAvatar();
			AvatarCloset.SetAvatarInfo(null);
			AvatarCloset.AvatarEditData.Clear();

			AvatarCloset.Controller?.OnClear();
		}

		public async UniTask PlayEnteringDirecting()
		{
			if (!TopRoot.IsUnityNull())
				TopRoot!.anchoredPosition = TopRoot.sizeDelta;

			if (!LeftRoot.IsUnityNull())
				LeftRoot!.anchoredPosition = new Vector2(-LeftRoot.sizeDelta.x - EnteringPositionPadding, LeftRoot.anchoredPosition.y);

			if (!BottomRoot.IsUnityNull())
				BottomRoot!.anchoredPosition = -BottomRoot.sizeDelta;

			await UniTask.Delay(TimeSpan.FromSeconds(EnteringDirectingDelay));

			if (!TopRoot.IsUnityNull())
				TopRoot!.DOAnchorPos(Vector2.zero, EnteringDirectingDuration).SetEase(Ease.InSine);

			if (!LeftRoot.IsUnityNull())
				LeftRoot!.DOAnchorPos(Vector2.up * LeftRoot!.anchoredPosition.y, EnteringDirectingDuration).SetEase(Ease.InSine);

			if (!BottomRoot.IsUnityNull())
				BottomRoot!.DOAnchorPos(Vector2.zero, EnteringDirectingDuration).SetEase(Ease.InSine);
		}
	}
}
