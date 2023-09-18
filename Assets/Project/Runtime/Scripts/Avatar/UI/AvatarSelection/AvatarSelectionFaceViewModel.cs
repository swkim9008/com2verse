/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarSelectionFaceViewModel.cs
* Developer:	eugene9721
* Date:			2023-03-17 16:40
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using Com2Verse.Avatar;
using Com2Verse.Bridge.Runtime.MachineRecognition;
using Com2Verse.Communication.Unity;
using Com2Verse.CustomizeLayer.Data;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.RenderFeatures.CaptureTasks;
using Com2Verse.RenderFeatures.Data;
using Com2Verse.Communication;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	// Phase 4-1: 얼굴 생성 방식 선택
	[ViewModelGroup("AvatarCustomize")]
	public partial class AvatarSelectionFaceViewModel : AvatarSelectionViewModelBase, IDisposable
	{
		private const string AIGenerateTextKey = "UI_AvatarCreate_Face_Text_AiGenerate";

		private const string FaceRecognizeTitleStringKey         = "UI_AvatarCreate_Popup_Title_ImageAnalysis";
		private const string FaceRecognizeFailedTitleStringKey   = "UI_AvatarCreate_Popup_Msg_Unrecognized";
		private const string FaceRecognizeFailedContextStringKey = "UI_AatarCreate_Popup_Msg_Explanations";

		private const string FaceNoPictureButtonStringKey      = "UI_AvatarCreate_Face_Btn_NoPicture";
		private const string FaceCancelGenerateButtonStringKey = "UI_AvatarCreate_Face_Btn_CancelGenerate";

		private const int RecognizeFailValue = -2;

#region Fields
		private bool _isSelectMethodPhase;
		private bool _isTakePicturePhase;
		private bool _isUploadPicturePhase;
		private bool _isEditPhase;

		private bool _isOnTakePictureButton;
		private bool _isOnConfirmGeneratedAvatar;
		private bool _isOnConfirmGeneratedAvatarByAI;
		private bool _isOnAvatarGenerateSlider;
		private bool _isOnEditImage;

		private bool _isOnVideoImage;
		private bool _isOnUploadImage;
		private bool _isOnAvatarImage;

		private bool _isVideoRunning;
#endregion Fields

#region Command Properties
		/// <summary>
		/// 4-2-1로 이동
		/// </summary>
		[UsedImplicitly] public CommandHandler GoToTakePictureButtonClick    { get; }

		/// <summary>
		/// TODO: 사진을 업로드하기 위한 탐색기 실행, 탐색기를 통해 사진을 업로드하면 4-3-1로 이동
		/// 탐색기 추가 전에는 해당 버튼 클릭시 4-3-1로 이동
		/// </summary>
		[UsedImplicitly] public CommandHandler GoToUploadPictureButtonClick  { get; }

		/// <summary>
		/// 4-4-1로 이동
		/// </summary>
		[UsedImplicitly] public CommandHandler GoToWithoutPictureButtonClick { get; }

		/// <summary>
		/// 4-1. 얼굴 생성 방식 선택으로 이어짐
		/// </summary>
		[UsedImplicitly] public CommandHandler RegenerateAiButtonClick { get; }
#endregion Command Properties

#region Field Properties
		[UsedImplicitly]
		public bool IsSelectMethodPhase
		{
			get => _isSelectMethodPhase;
			set
			{
				SetProperty(ref _isSelectMethodPhase, value);
				if (value)
				{
					IsTakePicturePhase    = false;
					IsUploadPicturePhase  = false;
					IsEditPhase           = false;

					IsOnVideoImage        = false;
					IsOnAvatarImage       = false;

					IsOnConfirmGeneratedAvatar     = false;
					IsOnConfirmGeneratedAvatarByAI = false;

					if (Owner != null)
						Owner.SubTitleTextKey = AvatarSelectionManagerViewModel.FaceFirstScreenSubTitleTextKey;

					CleanUploadTexture();
				}
			}
		}

		[UsedImplicitly]
		public bool IsTakePicturePhase
		{
			get => _isTakePicturePhase;
			set
			{
				if (value) OnShowTakePicture();
				SetProperty(ref _isTakePicturePhase, value);
			}
		}

		[UsedImplicitly]
		public bool IsUploadPicturePhase
		{
			get => _isUploadPicturePhase;
			set
			{
				if (value) OnShowUploadPicture();
				else OnHideUploadPicture();
				SetProperty(ref _isUploadPicturePhase, value);
			}
		}

		[UsedImplicitly]
		public bool IsEditPhase
		{
			get => _isEditPhase;
			set
			{
				AvatarMediator.Instance.AvatarCloset.IsFaceEditing = value;
				SetProperty(ref _isEditPhase, value);
				if (value) OnShowEdit();
				else OnHideEdit();
			}
		}

		[UsedImplicitly]
		public bool IsOnTakePictureButton
		{
			get => _isOnTakePictureButton;
			set
			{
				SetProperty(ref _isOnTakePictureButton, value);
				var videoRecorderViewModel = ViewModelManager.Instance.Get<VideoRecorderViewModel>();
				if (value)
				{
					IsOnConfirmGeneratedAvatar     = false;
					IsOnConfirmGeneratedAvatarByAI = false;
					IsOnAvatarGenerateSlider       = false;
					IsOnEditImage                  = false;

					if (videoRecorderViewModel != null)
					{
						var device = videoRecorderViewModel.Device;
						device.DeviceChanged += OnDeviceChanged;
						device.DeviceFailed  += OnDeviceFailed;
					}

					NotifyDeviceChanged();
				}
				else
				{
					if (videoRecorderViewModel != null)
					{
						var device = videoRecorderViewModel.Device;
						device.DeviceChanged -= OnDeviceChanged;
						device.DeviceFailed  -= OnDeviceFailed;
					}
				}
			}
		}

		[UsedImplicitly]
		public bool IsOnConfirmGeneratedAvatar
		{
			get => _isOnConfirmGeneratedAvatar;
			set
			{
				SetProperty(ref _isOnConfirmGeneratedAvatar, value);
				if (value)
				{
					IsOnTakePictureButton          = false;
					IsOnAvatarGenerateSlider       = false;
					IsOnEditImage                  = false;
					IsOnConfirmGeneratedAvatarByAI = false;
				}
			}
		}

		[UsedImplicitly]
		public bool IsOnConfirmGeneratedAvatarByAI
		{
			get => _isOnConfirmGeneratedAvatarByAI;
			set
			{
				SetProperty(ref _isOnConfirmGeneratedAvatarByAI, value);
				if (value)
				{
					IsOnTakePictureButton      = false;
					IsOnAvatarGenerateSlider   = false;
					IsOnEditImage              = false;
					IsOnConfirmGeneratedAvatar = false;
				}
			}
		}

		[UsedImplicitly]
		public bool IsOnAvatarGenerateSlider
		{
			get => _isOnAvatarGenerateSlider;
			set
			{
				SetProperty(ref _isOnAvatarGenerateSlider, value);
				if (value)
				{
					IsOnTakePictureButton          = false;
					IsOnConfirmGeneratedAvatar     = false;
					IsOnConfirmGeneratedAvatarByAI = false;
					IsOnEditImage                  = false;
					if (Owner != null)
						Owner.SubTitleTextKey = AvatarSelectionManagerViewModel.FaceAnalyzingSubTitleTextKey;
				}
			}
		}

		[UsedImplicitly]
		public bool IsOnEditImage
		{
			get => _isOnEditImage;
			set
			{
				SetProperty(ref _isOnEditImage, value);
				if (value)
				{
					IsOnTakePictureButton          = false;
					IsOnConfirmGeneratedAvatar     = false;
					IsOnConfirmGeneratedAvatarByAI = false;
					IsOnAvatarGenerateSlider       = false;
					if (Owner != null)
						Owner.SubTitleTextKey = AvatarSelectionManagerViewModel.FaceEditPictureSubTitleTextKey;
				}
			}
		}

		[UsedImplicitly]
		public bool IsOnVideoImage
		{
			get => _isOnVideoImage;
			set
			{
				SetProperty(ref _isOnVideoImage, value);
				if (value)
				{
					IsOnAvatarImage = false;
				}
			}
		}

		[UsedImplicitly]
		public bool IsOnAvatarImage
		{
			get => _isOnAvatarImage;
			set
			{
				SetProperty(ref _isOnAvatarImage, value);
				if (value)
				{
					IsOnVideoImage = false;
					if (Owner != null)
						Owner.SubTitleTextKey = AvatarSelectionManagerViewModel.FaceAnalyzedSubTitleTextKey;
				}
			}
		}

		[UsedImplicitly]
		public bool IsVideoRunning
		{
			get => _isVideoRunning;
			set => SetProperty(ref _isVideoRunning, value);
		}

		public bool SetForceLayoutRebuild => true;

		[UsedImplicitly]
		public string AIGenerateText => Localization.Instance.GetString(AIGenerateTextKey);

		[UsedImplicitly]
		public string NoPictureButtonTextKey => CurrentAiCreateItem != null ? Localization.Instance.GetString(FaceCancelGenerateButtonStringKey) : Localization.Instance.GetString(FaceNoPictureButtonStringKey);
#endregion Field Properties

#region Initialize
		public AvatarSelectionFaceViewModel()
		{
			GoToTakePictureButtonClick    = new CommandHandler(OnGoToTakePictureButtonClick);
			GoToUploadPictureButtonClick  = new CommandHandler(OnGoToUploadPictureButtonClick);
			GoToWithoutPictureButtonClick = new CommandHandler(OnGoToWithoutPictureButtonClick);

			ClickSetForceLayoutRebuildNextFrame = new CommandHandler(OnClickSetForceLayoutRebuildNextFrame);

			RegenerateAiButtonClick = new CommandHandler(OnReGenerateAiButtonClick);

			// Upload Picture
			ReUploadPictureButtonClick = new CommandHandler(OnReUploadPictureButtonClick);
			EndEditButtonClick         = new CommandHandler(OnEndEditButtonClick);
			ReGenerateButtonClick      = new CommandHandler(OnReGenerateButtonClick);
			LikeItAvatarButtonClick    = new CommandHandler(OnLikeItAvatarButtonClick);
			TakePictureButtonClick     = new CommandHandler(OnTakePictureButtonClick);

			InvokePropertyValueChanged(nameof(AIGenerateText), AIGenerateText);
		}

		public void Dispose()
		{
			if (AvatarMediator.InstanceExists)
				AvatarMediator.Instance.DisposeAiModel();
		}

		private void OnDeviceChanged(DeviceInfo prevDevice, int prevIndex, DeviceInfo device, int index) => NotifyDeviceChanged();
		private void OnDeviceFailed(DeviceInfo device, int index) => NotifyDeviceChanged();

		private void NotifyDeviceChanged()
		{
			if (IsOnTakePictureButton && Owner != null)
				Owner.SubTitleTextKey = DeviceManager.Instance.VideoRecorder.Current.IsAvailable
					? AvatarSelectionManagerViewModel.FaceTakePictureSubTitleTextKey
					: AvatarSelectionManagerViewModel.FaceNoCamSubTitleTextKey;
		}
#endregion Initialize

#region AvatarSelectionViewModelBase
		public override void Show()
		{
			if (Owner == null) return;

			Owner.AvatarCloset.Controller?.SetFaceVirtualCamera();
			if (Owner.IsCreatedAvatar || !Owner.IsCreateScene)
				GoToFaceEditPhase();
			else
				IsSelectMethodPhase = true;
		}

		public override void Hide()
		{
			OffProperties();
			ClearMenuItems();
			_isInitializedSubMenu = false;
		}

		private void OffProperties()
		{
			IsSelectMethodPhase            = false;
			IsTakePicturePhase             = false;
			IsUploadPicturePhase           = false;
			IsEditPhase                    = false;
			IsOnTakePictureButton          = false;
			IsOnConfirmGeneratedAvatar     = false;
			IsOnConfirmGeneratedAvatarByAI = false;
			IsOnAvatarGenerateSlider       = false;
			IsOnEditImage                  = false;
			IsOnVideoImage                 = false;
			IsOnAvatarImage                = false;
			HasSubMenu                     = false;
		}

		public override void Clear()
		{
			OnClearUploadPicture();
			OnClearEdit();
		}
#endregion AvatarSelectionViewModelBase

#region Handlers
		private void OnGoToTakePictureButtonClick()
		{
			IsTakePicturePhase = true;
		}

		private void OnGoToUploadPictureButtonClick()
		{
			OpenFileBrowser();
		}

		private void OnGoToWithoutPictureButtonClick()
		{
			LastSelectedPresetId = 0;
			CurrentAiCreateItem  = null;

			AvatarMediator.Instance.AvatarCloset.SetAvatarInfo(Owner?.AvatarCloset.Controller?.GetAvatarInfo());

			GoToFaceEditPhase();
		}
#endregion Handlers

		private bool GenerateAvatarFromAI(Texture texture)
		{
			var avatarMediator = AvatarMediator.Instance;

			if (texture.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "입력 텍스쳐가 없습니다.");
				return false;
			}

			var faceResult = avatarMediator.ProcessFaceLandmarkModel(texture!);
			if (faceResult == null || faceResult.Count == 0)
				return false;

			var hairResult = avatarMediator.ProcessHairModel(texture);
			if (hairResult.Item1 == RecognizeFailValue || hairResult.Item2 == RecognizeFailValue)
				return false;

			var avatarType = avatarMediator.AvatarCloset.AvatarEditData.AvatarType;
			if (avatarType != eAvatarType.PC01_M && avatarType != eAvatarType.PC01_W)
				return false;

			var avatar3Type = avatarType == eAvatarType.PC01_M ? Avatar3Types.PC01_M : Avatar3Types.PC01_W;

			var avatar = MachineRecognition.Resolve(avatar3Type, faceResult, hairResult.Item1, hairResult.Item2);

			var avatarManager = AvatarManager.Instance;
			var customizeInfo = avatarManager.SetCustomizeInfoFromItemKey(avatar!);
			var avatarInfo    = avatarManager.GetAvatarInfoFromCustomizeInfo(customizeInfo);

			var currentAvatarInfo = avatarMediator.AvatarCloset.CurrentAvatarInfo;
			if (currentAvatarInfo == null)
				currentAvatarInfo = AvatarTable.GetBaseAvatarInfo(avatarType);

			var aiFaceItems = avatarInfo.GetFaceOptionList();
			foreach (var aiFaceItem in aiFaceItems)
				currentAvatarInfo.UpdateFaceItem(aiFaceItem);

			avatarMediator.AvatarCloset.SetAvatarInfo(currentAvatarInfo);
			return true;
		}

		private async UniTask Capture(Avatar3AssemblerContext avatar, Avatar3Types avatar3Type)
		{
			avatar!.Pose             = "Idle";
			_captureTask!.CameraPos  = CameraPositions.PreviewAll;
			_captureTask.Avatar3Type = avatar3Type;
			_captureTask.Width       = 128;
			_captureTask.Height      = 213;
			var capturedTexture = await _captureTask!.Capture(avatar, false);
			AiCreatedPreset = capturedTexture;
		}

		private readonly CaptureTask _captureTask = ScriptableObject.CreateInstance<CaptureTask>();

		private void ShowErrorPopupMessage()
		{
			var title = Localization.Instance.GetString(FaceRecognizeTitleStringKey);

			var context = ZString.CreateStringBuilder();
			context.Append($"<size=25><style=\"Title\">{Localization.Instance.GetString(FaceRecognizeFailedTitleStringKey)}</style></size>");
			context.Append("\n");
			context.Append($"<align=\"left\"><indent=13%>{Localization.Instance.GetString(FaceRecognizeFailedContextStringKey)}</indent></align>");

			UIManager.Instance.ShowPopupConfirm(title, context.ToString(), BackToSelectMethodPhase);
		}

		private void BackToSelectMethodPhase()
		{
			IsSelectMethodPhase = true;
		}
	}
}
