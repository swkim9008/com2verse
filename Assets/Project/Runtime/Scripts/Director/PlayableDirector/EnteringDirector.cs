/*===============================================================
* Product:		Com2Verse
* File Name:	EnteringDirector.cs
* Developer:	eugene9721
* Date:			2023-02-28 12:19
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Cinemachine;
using Com2Verse.AvatarAnimation;
using Com2Verse.CameraSystem;
using Com2Verse.CustomizeLayer;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.Director
{
	public sealed class EnteringDirector : TimelineDirector
	{
		private const float CameraBlendingTime = 1f;

		[Header("Tracks")]
		[SerializeField] private string _cinemachineTrackName = "CinemachineTrack";
		[SerializeField] private string _enterAnimationTrack = "EnterAnimationTrack";
		[SerializeField] private string _fadeInFxTrack       = "FadeInFxTrack";

		[Header("Etc")]
		[SerializeField] private string _mainVirtualCameraClipName = "MainVirtualCamera";

		[SerializeField] private CinemachineVirtualCamera[] _cinemachineVirtualCameras = Array.Empty<CinemachineVirtualCamera>();

		private AvatarCustomizeLayer? _avatarCustomizeLayer;

		public override bool Play(IDirectorMessage? message = null)
		{
			if (message is not EnteringMessage enteringMessage)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "Message is not EnteringMessage");
				return false;
			}

			if (TimelineAsset.IsReferenceNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "TimelineAsset is null");
				return false;
			}

			if (!SetCharacterTracks(enteringMessage))
				return false;

			SetObjectToTrack(enteringMessage.CinemachineBrain!.gameObject, _cinemachineTrackName);
			SetDefaultCameraToClip(_mainVirtualCameraClipName, CameraManager.Instance.MainVirtualCamera);
			SetDirectorCameraTarget(enteringMessage.TargetTransform!);

			transform.SetPositionAndRotation(enteringMessage.TargetTransform!.position, enteringMessage.TargetTransform.rotation);

			base.Play(message);

			return true;
		}

		/// <summary>
		/// 타임라인의 실행길이를 리턴하는 함수
		/// 연출 이후 virtualCamera로 블랜딩되는 타임에 조작이 가능해야 자연스러운것같아서 블랜딩타임을 뺀 값을 리턴하였습니다.
		/// </summary>
		public override double GetDuration()
		{
			if (TimelineAsset.IsUnityNull()) return 0;

			return Mathf.Max((float)TimelineAsset!.duration - CameraBlendingTime, 0);
		}

		private bool SetCharacterTracks(EnteringMessage enteringMessage)
		{
			var avatarType = enteringMessage.AvatarType;
			if (avatarType != eAvatarType.PC01_M && avatarType != eAvatarType.PC01_W)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "AvatarType is not PC01_M or PC01_W");
				return false;
			}

			var characterTransform = enteringMessage.TargetTransform;
			var animator = characterTransform.GetComponentInChildren<Animator>();
			if (animator.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "Animator is null");
				return false;
			}

			var baseBodyTransform = enteringMessage.ActiveObject.BaseBodyTransform;
			if (baseBodyTransform.IsUnityNull())
			{
				C2VDebug.LogErrorCategory(GetType().Name, "BaseBodyTransform is null");
				return false;
			}

			_avatarCustomizeLayer = baseBodyTransform!.GetComponent<AvatarCustomizeLayer>();
			if (!_avatarCustomizeLayer.IsReferenceNull())
			{
				_avatarCustomizeLayer!.UseFadeIn = true;
				_avatarCustomizeLayer.FadeProgress = 0;
			}

			var genderKeyword        = avatarType == eAvatarType.PC01_M ? AnimationDefine.ManGenderKeyword : AnimationDefine.WomanGenderKeyword;
			var reverseGenderKeyword = avatarType == eAvatarType.PC01_M ? AnimationDefine.WomanGenderKeyword : AnimationDefine.ManGenderKeyword;

			SetMuteUnmuteTrack($"{genderKeyword}_{_enterAnimationTrack}", false);
			SetMuteUnmuteTrack($"{genderKeyword}_{_fadeInFxTrack}",       false);

			SetObjectToTrack(animator!, $"{genderKeyword}_{_enterAnimationTrack}");
			SetObjectToTrack(animator!, $"{genderKeyword}_{_fadeInFxTrack}");

			SetMuteUnmuteTrack($"{reverseGenderKeyword}_{_enterAnimationTrack}", true);
			SetMuteUnmuteTrack($"{reverseGenderKeyword}_{_fadeInFxTrack}",       true);
			return true;
		}

		private void SetDirectorCameraTarget(Transform targetTransform)
		{
			if (TimelineAsset.IsReferenceNull()) return;

			foreach (var virtualCamera in _cinemachineVirtualCameras)
			{
				virtualCamera.Follow = targetTransform;
				virtualCamera.LookAt = targetTransform;
			}
		}

#region Signals
		[UsedImplicitly]
		public void OnDirectionEnd()
		{
			gameObject.SetActive(false);

			if (!_avatarCustomizeLayer.IsUnityNull())
				_avatarCustomizeLayer!.UseFadeIn = false;
		}

		[UsedImplicitly]
		public void OnDataInitialize()
		{
			CameraManager.InstanceOrNull?.InitializeValueCurrentCamera();
		}
#endregion Signals
	}
}
