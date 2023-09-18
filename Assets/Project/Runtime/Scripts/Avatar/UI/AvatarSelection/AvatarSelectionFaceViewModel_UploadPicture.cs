/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarSelectionFaceViewModel_UploadPicture.cs
* Developer:	eugene9721
* Date:			2023-03-29 19:36
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.AssetSystem;
using Com2Verse.Avatar;
using Com2Verse.Avatar.UI;
using Com2Verse.Communication.Unity;
using Com2Verse.Data;
using UnityEngine;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using SimpleFileBrowser;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Com2Verse.UI
{
	// Phase 4-2~3: AI 얼굴 생성
	public partial class AvatarSelectionFaceViewModel
	{
		private const string AnalyzingKey = "UI_AvatarCreate_Face_Text_Analyzing";
		private static readonly Lazy<Shader> CropShader = new(() => Shader.Find("Com2Verse/UI/AIThumbnailCrop(UI)"));
		private static readonly int PropInputImage = Shader.PropertyToID("_InputImage");
		private static readonly int PropInputResolution = Shader.PropertyToID("_InputResolution");
		private static readonly int PropOutputResolution = Shader.PropertyToID("_OutputResolution");

		private const int AiCreatedItemId = 0x3f3f3f3f;

#region Fields
		private float _uploadPictureSliderValue;

		private Texture? _selectedPictureTexture;
		private Texture? _aiCreatedPreset;
#endregion Fields

#region Command Properties
		[UsedImplicitly] public CommandHandler ReUploadPictureButtonClick { get; private set; }
		[UsedImplicitly] public CommandHandler EndEditButtonClick         { get; private set; }
		[UsedImplicitly] public CommandHandler ReGenerateButtonClick      { get; private set; }
		[UsedImplicitly] public CommandHandler LikeItAvatarButtonClick    { get; private set; }
		[UsedImplicitly] public CommandHandler TakePictureButtonClick     { get; private set; }
#endregion Command Properties

#region Field Properties
		[UsedImplicitly]
		public float UploadPictureSliderValue
		{
			get => _uploadPictureSliderValue;
			set
			{
				SetProperty(ref _uploadPictureSliderValue, Mathf.Clamp(value, 0f, 1f));
				InvokePropertyValueChanged(nameof(UploadPictureSliderValueText), UploadPictureSliderValueText);
			}
		}

		public string UploadPictureSliderValueText => Localization.Instance.GetString(AnalyzingKey, $"{UploadPictureSliderValue * 100f:0}");

		[UsedImplicitly]
		public Texture? SelectedPictureTexture
		{
			get => _selectedPictureTexture;
			set
			{
				SetProperty(ref _selectedPictureTexture, value);
				InvokePropertyValueChanged(nameof(IsPictureSelected), IsPictureSelected);
			}
		}

		[UsedImplicitly]
		public Texture? AiCreatedPreset
		{
			get => _aiCreatedPreset;
			set => SetProperty(ref _aiCreatedPreset, value);
		}

		[UsedImplicitly]
		public bool IsPictureSelected => !SelectedPictureTexture.IsUnityNull();

		public int LastSelectedPresetId { get; set; }

		public CustomizeItemViewModel? CurrentAiCreateItem { get; private set; }
#endregion Field Properties

#region AvatarSelectionViewModelBase
		private void OnShowTakePicture()
		{
			IsSelectMethodPhase  = false;
			IsUploadPicturePhase = false;
			IsEditPhase          = false;

			IsOnVideoImage        = true;
			IsOnTakePictureButton = true;

			CleanUploadTexture();
		}

		private void OnShowUploadPicture()
		{
			IsSelectMethodPhase = false;
			IsTakePicturePhase  = false;
			IsEditPhase         = false;

			IsOnVideoImage  = true;
			IsOnAvatarImage = false;

			CleanUploadTexture();
		}

		private void OnHideUploadPicture()
		{
			if (FileBrowser.IsOpen)
				FileBrowser.HideDialog();
			if (!_isUploadPicturePhase) return;

			CleanUploadTexture();
			Resources.UnloadUnusedAssets();
		}

		private void OnClearUploadPicture()
		{
			LastSelectedPresetId = 0;
			CurrentAiCreateItem  = null;
		}
#endregion AvatarSelectionViewModelBase

#region Handlers
		private void OnReUploadPictureButtonClick()
		{
			OpenFileBrowser();
		}

		private void OnEndEditButtonClick()
		{
			IsOnEditImage = false;
			OnStartAnalysis();
		}

		private void OnReGenerateButtonClick()
		{
			IsSelectMethodPhase = true;
		}

		private void OnLikeItAvatarButtonClick()
		{
			if (IsTakePicturePhase && IsOnVideoImage)
			{
				OnStartAnalysis();
				return;
			}

			GoToFaceEditPhase();

			if (CurrentAiCreateItem != null)
			{
				InvokePropertyValueChanged(nameof(NoPictureButtonTextKey), NoPictureButtonTextKey);

				// 프리셋 리스트 초기화를 위한 처리
				_isInitializedSubMenu = false;
			}
		}

		private void OnTakePictureButtonClick()
		{
			var webCamTexture = ModuleManager.Instance.Camera.Texture;

			if (webCamTexture.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(nameof(AvatarSelectionFaceViewModel), "WebcamTexture is null.");
				return;
			}

			IsOnConfirmGeneratedAvatar = true;

			var targetTexture = new Texture2D(webCamTexture!.width, webCamTexture.height);
			webCamTexture.Copy(targetTexture);
			SelectedPictureTexture = targetTexture;
		}
#endregion Handlers

#region Analysis
		private void OnStartAnalysis()
		{
			if (IsOnAvatarGenerateSlider)
				return;

			IsOnAvatarGenerateSlider = true;
			UploadPictureSliderValue = 0f;

			OnStartAnalysisAsync().Forget();

			C2VDebug.LogCategory(GetType().Name, "OnStart FaceAnalysis");
		}

		private async UniTask OnStartAnalysisAsync()
		{
			var avatarManager = AvatarMediator.Instance;

			var jigController = avatarManager.AvatarCloset.Controller?.AvatarJigController;
			if (jigController.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "can't access avatarJigController");
				return;
			}
			jigController!.ClearAiJig();

			var modelLoadResult = await avatarManager.LoadAiModelAsync(AiAnalysisProgressAction);
			if (!modelLoadResult)
			{
				ShowErrorPopupMessage();
				IsOnAvatarGenerateSlider = false;
				return;
			}

			var result = false;
			if (!SelectedPictureTexture.IsUnityNull())
				result = GenerateAvatarFromAI(SelectedPictureTexture!);
			if (!result)
			{
				ShowErrorPopupMessage();
				IsOnAvatarGenerateSlider = false;
				return;
			}

			var avatarInfo = avatarManager.AvatarCloset.CurrentAvatarInfo;

			if (avatarInfo == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "avatarInfo is null.");
				IsOnAvatarGenerateSlider = false;
				return;
			}

			await CaptureAiPreviewImage(avatarInfo, jigController);

			CurrentAiCreateItem = new CustomizeItemViewModel
			{
				IsCreatedAI            = true,
				AvatarInfo             = avatarInfo.Clone(),
				ItemId                 = AiCreatedItemId,
				AICreatedPresetTexture = AiCreatedPreset,
			};
			CurrentAiCreateItem.AvatarInfo.ClearFashionItem();
			LastSelectedPresetId = CurrentAiCreateItem.ItemId;

			OnEndAnalysis(avatarInfo);
		}

		private async UniTask CaptureAiPreviewImage(AvatarInfo avatarInfo, AvatarJigController jigController)
		{
			var aiJigAvatarInfo = avatarInfo.Clone();
			aiJigAvatarInfo.SetBaseFashionItem();
			aiJigAvatarInfo.UpdateBodyShapeItem(AvatarTable.GetBodyShapeId(avatarInfo.AvatarType, MetaverseAvatarDefine.DefaultBodyShapeId));

			var avatarController = await AvatarCreator.CreateAvatarAsync(aiJigAvatarInfo, eAnimatorType.AVATAR_CUSTOMIZE, Vector3.zero, (int)Define.eLayer.CHARACTER);
			if (avatarController.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "avatarController is null.");
				return;
			}

			jigController.SetAvatarToAiJig(avatarController!);

			SelectedPictureTexture = jigController.RenderAvatarImage(avatarInfo.AvatarType);
			AiCreatedPreset        = ResizeToThumbnail(jigController.RenderAvatarImage(avatarInfo.AvatarType));

			Object.Destroy(avatarController!.gameObject);
		}

		private static Texture2D ResizeToThumbnail(Texture tex)
		{
			const int width = 256, height = 426;

			var thumbnailRt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.BGRA32);

			var cropMaterial = new Material(CropShader.Value);
			cropMaterial.SetTexture(PropInputImage, tex);
			cropMaterial.SetVector(PropInputResolution, new Vector4(tex.width, tex.height, 0.0f, 0.0f));
			cropMaterial.SetVector(PropOutputResolution, new Vector4(width, height, 0.0f, 0.0f));
			Graphics.Blit(null, thumbnailRt, cropMaterial);
			RenderTexture.active = null;

			var req = AsyncGPUReadback.Request(thumbnailRt);
			req.WaitForCompletion();
			thumbnailRt.Release();

			var thumbnailTex = new Texture2D(width, height, TextureFormat.BGRA32, false, false);
			thumbnailTex.SetPixelData(req.GetData<byte>(), 0);
			thumbnailTex.Apply(false, true);
			return thumbnailTex;
		}

		private void AiAnalysisProgressAction(float value)
		{
			UploadPictureSliderValue = value;
		}

		private void OnEndAnalysis(AvatarInfo avatarInfo)
		{
			IsOnConfirmGeneratedAvatarByAI = true;
			IsOnAvatarImage                = true;

			var avatarMediator  = AvatarMediator.Instance;
			var prevAvatarId    = avatarMediator.UserAvatarInfo?.AvatarId ?? 0;
			var prevFashionInfo = avatarMediator.UserAvatarInfo?.GetFashionItemList();

			avatarInfo.AvatarId = prevAvatarId;
			if (prevFashionInfo != null)
				avatarInfo.InitializeFashionItem(prevFashionInfo);
		}
#endregion Analysis

#region FileBrowser
		private void OpenFileBrowser()
		{
			if (FileBrowser.IsOpen)
			{
				C2VDebug.LogWarningCategory(nameof(AvatarSelectionFashionViewModel), "FileBrowser is already open");
				return;
			}

			FileBrowser.ShowLoadDialog(OnSuccessLoadFile, OnCancelLoadFile, FileBrowser.PickMode.Files, false, "Load File", "Load");

			var fileBrowser = FileBrowser.Instance;
			if (!fileBrowser.IsUnityNull())
				if (fileBrowser!.TryGetComponent(out Canvas canvas))
					canvas!.sortingOrder = 6000;
		}

		private void OnSuccessLoadFile(string[] paths)
		{
			var filePath = paths[0];
			var isValidTexture = TextureUtil.CheckFileIsValidTexture(filePath);

			if (!isValidTexture)
			{
				// TODO: Localization
				UIManager.Instance.ShowPopupCommon("이미지 파일을 선택해주세요!");
				return;
			}

			ApplyTexture(filePath).Forget();
		}

		private async UniTask ApplyTexture(string filePath)
		{
			var textureSize = TextureUtil.GetTextureSize(filePath);
			if (textureSize == (-1, -1))
			{
				// TODO: Localization
				UIManager.Instance.ShowPopupCommon("잘못된 이미지 파일입니다.");
				return;
			}

			try
			{
				using var request = UnityWebRequestTexture.GetTexture(filePath);
				await request.SendWebRequest();
				if (request.result != UnityWebRequest.Result.Success)
				{
					C2VDebug.LogErrorCategory(GetType().Name, $"LOAD TEXTURE FROM FILE FAILED. RESULT = {request.result}");
				}

				var tex = DownloadHandlerTexture.GetContent(request);
				if (!tex.IsUnityNull())
					tex = tex!.ChangeFormat(TextureFormat.RGBA32);
				if (!TextureUtil.IsValidTexture(tex))
				{
					// TODO: Localization
					UIManager.Instance.ShowPopupCommon("잘못된 이미지 파일입니다.");
					return;
				}

				IsUploadPicturePhase   = true;
				SelectedPictureTexture = tex;
			}
			catch (Exception e)
			{
				C2VDebug.LogErrorCategory(GetType().Name, $"LOAD TEXTURE FROM FILE ERROR\n{e}");
				return;
			}

			// TODO: 8월 빌드 이전 임시 처리
			// IsOnEditImage = true;
			OnEndEditButtonClick();
		}

		private void OnCancelLoadFile()
		{
		}

		private void CleanUploadTexture()
		{
			if (!_selectedPictureTexture.IsUnityNull())
				Object.DestroyImmediate(_selectedPictureTexture!);

			SelectedPictureTexture = null;
		}
#endregion FileBrowser
	}
}
