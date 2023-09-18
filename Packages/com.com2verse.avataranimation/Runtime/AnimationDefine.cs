/*===============================================================
* Product:		Com2Verse
* File Name:	AnimationDefine.cs
* Developer:	eugene9721
* Date:			2022-06-15 16:39
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Text.RegularExpressions;
using UnityEngine;

namespace Com2Verse.AvatarAnimation
{
	public enum eAnimationState
	{
		STAND,
		SIT,
		WALK,
		RUN,
		JUMP,
	}

	public enum eIdleState
	{
		NORMAL,
		HAND_UP_ON,
		HAND_UP_OFF,
	}

	public static class AnimationDefine
	{
		public static readonly string AnimationKeyword   = "Animation";
		public static readonly string ManGenderKeyword   = "M";
		public static readonly string WomanGenderKeyword = "W";

		public static readonly int BaseLayerIndex      = 0;
		public static readonly int FullBodyLayerIndex  = 1;
		public static readonly int UpperBodyLayerIndex = 2;

		// Masks
		public static readonly string ManCinematicAvatarMaskAssetName   = "PC01_M_AvatarMask_Cinematic.mask";
		public static readonly string WomanCinematicAvatarMaskAssetName = "PC01_W_AvatarMask_Cinematic.mask";
		public static readonly string UpperBodyMaskAssetName   = "UpperBody Avatar Mask.mask";

		public static readonly string EmotionParameter = "Emotion";

		public static readonly string AvatarCode      = "PC01";
		public static readonly Regex  AnimClipPattern = new("^PC01_[?:M|W]_.*_anim$");

		public static float DefaultCrossFadeDuration = 0.2f;

		// Bones
		public static readonly string HeadBoneName      = "Bip001 Head";
		public static readonly string FxEmitterBoneName = "fx_emitter";
		public static readonly string ManEyeBoneName    = "PC01_M_EYE";
		public static readonly string WomanEyeBoneName  = "PC01_W_Eye";

		// Separator
		public static readonly string Separator        = "_";
		public static readonly string ClipExtension    = "anim";
		public static readonly int    AvatarCodeLength = 4; // ex: PC01
		public static readonly int    GenderCodeLength = 1; // ex: M, W

		// States
		public static readonly int HashDefaultState = Animator.StringToHash("Default State");
		public static readonly int HashIdle         = Animator.StringToHash("Idle");
		public static readonly int HashWait         = Animator.StringToHash("Wait");
		public static readonly int HashSitBlend     = Animator.StringToHash("SitDown Blend");

		// Jump State
		public static readonly int HashFall     = Animator.StringToHash("Fall");
		public static readonly int HashFallLand = Animator.StringToHash("Fall Land");

		public static readonly int HashJumpUp    = Animator.StringToHash("JumpUp");
		public static readonly int HashJumpReady = Animator.StringToHash("JumpReady");

		// Parameters
		public static readonly int HashSpeed           = Animator.StringToHash("Speed");
		public static readonly int HashFallingDistance = Animator.StringToHash("FallingDistance");
		public static readonly int HashState           = Animator.StringToHash("State"); // Protocol - CharacterState
		public static readonly int HashEmotion         = Animator.StringToHash("Emotion");
		public static readonly int HashEmotionEnd      = Animator.StringToHash("EmotionEnd");

		public static readonly int HashSetWait     = Animator.StringToHash("SetWait");
		public static readonly int HashHandsUpIdle = Animator.StringToHash("HandsUpIdle");

		// Customize
		public static readonly int HashIdleCustomize = Animator.StringToHash("IdleCustomize");
		public static readonly int HashIsSelected    = Animator.StringToHash("IsSelected");
		public static readonly int HashSelectBody    = Animator.StringToHash("SelectBody");
		public static readonly int HashSelectHead    = Animator.StringToHash("SelectHead");
		public static readonly int HashSetStart      = Animator.StringToHash("SetStart");

		// Rigging
		public static readonly string RigObjectName        = "Rig";
		public static readonly string LookAtConstraintName = "LookAtConstraint";

		// Fx Layer
		public static readonly int HashFadeIn  = Animator.StringToHash("FadeIn");
		public static readonly int HashFadeOut = Animator.StringToHash("FadeOut");

		public static readonly float MoveThreshold = 0.1f;
	}
}
