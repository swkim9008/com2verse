/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarMediator.cs
* Developer:	eugene9721
* Date:			2023-06-15 20:23
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using com.com2vers.hairrecognition;
using com.com2verse.facelandmark;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.HttpHelper;
using Com2Verse.Loading;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.Rendering.Utility;
using Com2Verse.UI;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Protocols.CommonLogic;
using Protocols.GameLogic;
using UnityEngine;

namespace Com2Verse.Avatar
{
	[UsedImplicitly]
	public class AvatarMediator : Singleton<AvatarMediator>, IDisposable
	{
		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private AvatarMediator() { }

		public readonly AvatarCloset AvatarCloset = new();

		private DetectionAndLandmark3D?   _face;
		private HairDetectAndRecognition? _hair;

		private bool _isAiModelLoaded;

		/// <summary>
		/// 서버에 동기화 되고 있는 유저의 아바타 인포
		/// </summary>
		public AvatarInfo? UserAvatarInfo { get; private set; }

		public void Initialize()
		{
			// Public Login Response
			PacketReceiver.Instance.OnLoginResponseEvent        += OnResponseAvatarLogin;
			PacketReceiver.Instance.OnUpdateAvatarResponseEvent += OnUpdateAvatarResponse;
			NetworkManager.Instance.OnDisconnected              += OnDisconnected;

			LoadTable();
			SetLayerData();
		}

		private void LoadTable()
		{
			AvatarManager.Instance.LoadTable();
		}

		public void Dispose()
		{
			if (PacketReceiver.InstanceExists)
			{
				PacketReceiver.Instance.OnLoginResponseEvent        -= OnResponseAvatarLogin;
				PacketReceiver.Instance.OnUpdateAvatarResponseEvent -= OnUpdateAvatarResponse;
			}
			if (NetworkManager.InstanceExists)
				NetworkManager.Instance.OnDisconnected -= OnDisconnected;

			var mapController = MapController.InstanceOrNull;
			if (mapController != null)
				mapController.AvatarDtoToStruct = null;
		}

		private void SetLayerData()
		{
			var avatarManager = AvatarManager.Instance;
			avatarManager.AvatarBodyLayer          = (int)Define.eLayer.CHARACTER;
			avatarManager.AvatarRenderingLayerMask = (uint)RenderStateUtility.eRenderingLayerMask.UNUSED_8;
		}

#region Network
		/// <summary>
		/// Public 로그인이 완료 되면 호출
		/// </summary>
		private void OnResponseAvatarLogin(LoginCom2verseResponse loginResponse)
		{
			RegisterAvatar(loginResponse);
		}

		private void OnUpdateAvatarResponse(UpdateAvatarResponse updateAvatarResponse)
		{
			var updatedAvatar = updateAvatarResponse.UpdatedAvatar;
			if (updatedAvatar != null)
				SetUserAvatar(updatedAvatar);
		}

		/// <summary>
		/// 서버에서 받은 아바타 리스트를 캐싱
		/// </summary>
		private void RegisterAvatar(LoginCom2verseResponse loginResponse)
		{
			var userAvatar = loginResponse.Avatar;
			if (userAvatar != null)
			{
				LoginManager.Instance.CheckLoginQueue(() => EnterWorld(loginResponse), false);
			}
			else
			{
				ChangeSceneAsync().Forget();
			}
		}

		private void EnterWorld(LoginCom2verseResponse loginResponse)
		{
			SetUserAvatar(loginResponse.Avatar);

			NetworkUIManager.Instance.OnFieldChangeEvent += OnFieldChange;
			Commander.Instance.RequestEnterWorld();
			UIManager.Instance.ShowWaitingResponsePopup();
		}

		private void OnFieldChange()
		{
			NetworkUIManager.Instance.OnFieldChangeEvent -= OnFieldChange;
			Client.OnTimerClearEvent -= LoginManager.Instance.TimerClearAction;
			UIManager.Instance.HideWaitingResponsePopup();
		}

		private async UniTask ChangeSceneAsync()
		{
			await UniTask.DelayFrame(1);
			Client.OnTimerClearEvent -= LoginManager.Instance.TimerClearAction;
			UIManager.Instance.HideWaitingResponsePopup();
			LoadingManager.Instance.ChangeScene<SceneAvatarSelection>();
		}

		public void SetUserAvatar(Protocols.Avatar? avatar)
		{
			if (avatar == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "avatar is null");
				return;
			}

			UserAvatarInfo = new AvatarInfo(avatar);

			// 필수로 착용해야하는 패션 아이템이 없는 경우, 기본 아이템으로 설정
			foreach (eFashionSubMenu fashionSubMenu in Enum.GetValues(typeof(eFashionSubMenu)))
			{
				if (AvatarTable.FashionSubMenuFeatures[fashionSubMenu].HasEmptySlot)
					continue;

				var avatarType = UserAvatarInfo.AvatarType;
				if (UserAvatarInfo.GetFashionItem(fashionSubMenu) == null)
					UserAvatarInfo.UpdateFashionItem(FashionItemInfo.GetDefaultItemInfo(avatarType, fashionSubMenu));
			}

			SetPresetFaceInfo(UserAvatarInfo);
			AvatarCloset.SetAvatarInfo(UserAvatarInfo.Clone());
		}

		private void SetPresetFaceInfo(AvatarInfo avatarInfo)
		{
			var faceOptions = avatarInfo.GetFaceOptionList();
			if (faceOptions.Count == 1)
			{
				var presetData = avatarInfo.GetFaceOption(eFaceOption.PRESET_LIST);
				if (presetData != null)
				{
					var newInfo = AvatarManager.Instance.GetFacePresetInfo(presetData.ItemId, avatarInfo, false); 
					if (newInfo != null)
						avatarInfo.DeepCopy(newInfo);
				}
			}

			avatarInfo.RemoveFaceItem(eFaceOption.PRESET_LIST);
			var avatarSelectViewModel = ViewModelManager.Instance.Get<AvatarSelectionManagerViewModel>();
			avatarSelectViewModel?.RefreshButtons();
		}

		private void OnDisconnected()
		{
			UserAvatarInfo = null;
		}
#endregion Network

#region Ai Model
		private const int RecognizeFailValue = -2;

		public async UniTask<bool> LoadAiModelAsync(Action<float>? progressEvent = null)
		{
			// TODO: AI 리소스 로드 관련 개선, 씬 이동/아바타 앱 종료시 리소스 해제하도록 변경
			_face?.Dispose();
			_hair?.Dispose();

			_face = new DetectionAndLandmark3D();
			_hair = new HairDetectAndRecognition();

			float progress     = 0f;
			float progressStep = 1f / 6f;

			var modelDataDetection = await Resources.LoadAsync("C2V_FaceDetection");
			progress += progressStep;
			progressEvent?.Invoke(progress);

			var modelDataLandmark2D = await Resources.LoadAsync("C2V_Landmark");
			progress += progressStep;
			progressEvent?.Invoke(progress);

			var modelDataLandmark3D = await Resources.LoadAsync("MP_Landmark");
			progress += progressStep;
			progressEvent?.Invoke(progress);

			if (modelDataDetection == null || modelDataLandmark2D == null || modelDataLandmark3D == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "Load FaceModel Fail");
				DisposeAiModel();
				return false;
			}

			var createFaceResult    = await _face.CreateModelAsync(modelDataDetection, modelDataLandmark2D, modelDataLandmark3D);
			var facePredictorResult = _face.CreatePredictor();

			if (!createFaceResult || !facePredictorResult)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "Load FaceModel Fail");
				DisposeAiModel();
				return false;
			}

			var c2VDetection = await Resources.LoadAsync("C2V_Detection");
			progress += progressStep;
			progressEvent?.Invoke(progress);

			var preprocessedHairSegmentation = await Resources.LoadAsync("preprocessed_hair_segmentation");
			progress += progressStep;
			progressEvent?.Invoke(progress);

			var preprocessedHairRecognition = await Resources.LoadAsync("preprocessed_hair_recognition");
			progress = 1f;
			progressEvent?.Invoke(progress);

			if (c2VDetection == null || preprocessedHairSegmentation == null || preprocessedHairRecognition == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "Load HairModel Fail");
				DisposeAiModel();
				return false;
			}

			var createHairResult    = await _hair.CreateHairRecognitionModelAsync(c2VDetection, preprocessedHairSegmentation, preprocessedHairRecognition);
			_hair.CreateHairRecognitionPredictor();
			var hairPredictorResult = _hair.GetState();

			if (!createHairResult || !hairPredictorResult)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "Load HairModel Fail");
				DisposeAiModel();
				return false;
			}

			_isAiModelLoaded = true;
			return true;
		}

		public List<DetectionAndLandmark3DPredictor.DetectionLandmark>? ProcessFaceLandmarkModel(Texture texture)
		{
			if (!_isAiModelLoaded || _face == null)
			{
				C2VDebug.LogWarningCategory(GetType().Name, "Ai Model is not loaded");
				return null;
			}

			var rt = RenderTexture.GetTemporary(texture.width, texture.height, 0);
			if (rt.IsReferenceNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "Can't create RenderTexture");
				return null;
			}

			Graphics.Blit(texture, rt!);

			var value = _face.ProcessImage(rt!);
			rt!.Release();
			return value;
		}

		public (int, int) ProcessHairModel(Texture texture)
		{
			if (!_isAiModelLoaded || _hair == null)
			{
				C2VDebug.LogWarningCategory(GetType().Name, "Ai Model is not loaded");
				return (RecognizeFailValue, RecognizeFailValue);
			}

			var rt = RenderTexture.GetTemporary(texture.width, texture.height, 0);
			if (rt.IsReferenceNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "Can't create RenderTexture");
				return (RecognizeFailValue, RecognizeFailValue);
			}

			Graphics.Blit(texture, rt!);

			var value = _hair.ProcessImage(rt!);
			rt!.Release();
			return value;
		}

		public string GetHairLengthString(int value)
		{
			if (!_isAiModelLoaded || _hair == null)
			{
				C2VDebug.LogWarningCategory(GetType().Name, "Ai Model is not loaded");
				return string.Empty;
			}

			return _hair.GetLengthString(value);
		}

		public string GetHairBangString(int value)
		{
			if (!_isAiModelLoaded || _hair == null)
			{
				C2VDebug.LogWarningCategory(GetType().Name, "Ai Model is not loaded");
				return string.Empty;
			}

			return _hair.GetBangString(value);
		}

		public void DisposeAiModel()
		{
			_face?.Dispose();
			_hair?.Dispose();

			_face = null;
			_hair = null;

			_isAiModelLoaded = false;
		}
#endregion Ai Model
	}
}
