/*===============================================================
* Product:		Com2Verse
* File Name:	ActiveObject_Animator.cs
* Developer:	eugene9721
* Date:			2022-07-01 10:01
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Avatar;
using Com2Verse.AvatarAnimation;
using Com2Verse.CameraSystem;
using Com2Verse.Chat;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Project.CameraSystem;
using Com2Verse.PlayerControl;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.Network
{
	public partial class ActiveObject : IAvatarTarget
	{
		private static readonly float SitHeightRatio = 0.7f;
		private Renderer _headRenderer;

		public Protocols.CharacterState JumpPrevState => AnimatorController.JumpPrevState;
		public bool IsGesturing => AnimatorController.IsGesturing;
		public eAnimationState CurrentAnimationState => AnimatorController.CurrentAnimationState;

#region Initialize
		private void FindRenderer()
		{
			var avatarType = eAvatarType.NONE;
			if (!AvatarController.IsUnityNull())
				avatarType = AvatarController!.Info?.AvatarType ?? eAvatarType.NONE;

			if (avatarType == eAvatarType.NONE)
				C2VDebug.LogWarningCategory(nameof(ActiveObject), "Avatar Type is NONE");

			var headRendererName = MetaverseAvatarDefine.GetHeadRendererName(avatarType);
			if (string.IsNullOrWhiteSpace(headRendererName))
				C2VDebug.LogWarningCategory(nameof(ActiveObject), "Head Renderer Name is Empty");

			_headRenderer = transform.FindRecursive(headRendererName)?.GetComponent<Renderer>();
			if (_headRenderer.IsReferenceNull())
				C2VDebug.LogErrorCategory(nameof(ActiveObject), "Head Renderer is null");
		}

		private void SetAvatarHeight()
		{
			if (_headRenderer.IsReferenceNull())
			{
				ObjectHeight = FallbackHeight;
				return;
			}
			ObjectHeight = (_headRenderer.bounds.max.y - transform.position.y);
			Height       = ObjectHeight;
		}

		private void SetDefaultAnimationParameter()
		{
			_prevPartsInfo = default;
			if (CharacterState != 1 && HasAnimator)
				ObjAnimator.SetInteger(AnimationDefine.HashState, (int)Protocols.CharacterState.IdleWalkRun);
		}
#endregion Initialize

		private void SetLatestAnimator()
		{
			if (ControlPoints[1].AnimatorID == 0 || ControlPoints[1].AnimatorID == -1) return;
			AnimatorController.LoadAnimatorAsync(ControlPoints[1].AnimatorID).Forget();
		}

		private void SetLatestAvatarParts()
		{
			var currentValue = ControlPoints[1].AvatarCustomizeInfo;
			if (currentValue.IsDefault || _prevPartsInfo == currentValue) return;

			if (_avatarController.IsUnityNull())
				return;

			if (!_avatarController!.IsCompletedLoadAvatarParts) return;

			AvatarManager.Instance.UpdateAvatarParts(_avatarController!, ControlPoints[1].AvatarCustomizeInfo);
			_prevPartsInfo = currentValue.Clone();
		}

		private void SetCharacterStateForMovement()
		{
			if (CharacterState == -1) return;
			var playerController = PlayerController.Instance;
			if (IsMine && !playerController.IsReferenceNull())
				playerController.SetCharacterState((Protocols.CharacterState)CharacterState);
		}

#region AvatarStateEvent
		private void OnAvatarStateChange(Protocols.CharacterState state, bool isOn)
		{
			switch (state)
			{
				case Protocols.CharacterState.IdleWalkRun:
					OnIdleWalkRun(isOn);
					break;
				case Protocols.CharacterState.JumpStart:
					break;
				case Protocols.CharacterState.InAir:
					break;
				case Protocols.CharacterState.JumpLand:
					break;
				case Protocols.CharacterState.Sit:
					OnSitDown(isOn);
					OnSitDownIsMine(isOn);
					break;
			}

			if (!IsMine || !isOn) return;
			var user = User.InstanceOrNull;
			user?.CharacterStateViewModel?.SetCharacterState(state);
		}

		private void OnIdleWalkRun(bool isOn)
		{
			var uiRoot = GetUIRoot();
			if (uiRoot.IsUnityNull()) return;
			if (isOn)
				uiRoot.localPosition = Vector3.up * FallbackHeight;
		}

		private void OnSitDown(bool isOn)
		{
			if (!User.InstanceExists) return;

			var uiRoot = GetUIRoot();
			if (uiRoot.IsUnityNull()) return;

			if (isOn)
				uiRoot!.localPosition = Vector3.up * (FallbackHeight * SitHeightRatio);
		}

		private void OnSitDownIsMine(bool isOn)
		{
			if (!IsMine) return;
			if (!User.InstanceExists) return;

			if (CurrentScene.SpaceCode is eSpaceCode.MEETING)
			{
				if (isOn && ViewModelManager.InstanceOrNull?.Get<SharedScreenViewModel>()?.IsRunning is true)
				{
					CameraManager.Instance.ChangeState(eCameraState.FIXED_CAMERA, CameraJigKey.MeetingScreenView);
				}
				else
				{
					CameraManager.Instance.ChangeState(eCameraState.FOLLOW_CAMERA);
				}
			}
		}
#endregion AvatarStateEvent

#region Emotion
		private void OnSetEmotion(Emotion emotion, int emotionState)
		{
			if (emotion.EmotionType != eEmotionType.GESTURE)
			{
				ChatManager.Instance.CreateSpeechBubble(this, null, 0, emotionState);
			}
		}

		public void CancelEmotion()
		{
			if (!IsGesturing) return;

			AnimatorController.SetGestureEnd();
			if (IsMine) Commander.Instance.SetEmotion(0, ObjectID);
		}
#endregion Emotion

#region IAvatarTarget
		public float     Height       { get; set; }
		public Transform AvatarTarget { get; set; }
#endregion IAvatarTarget
	}
}
