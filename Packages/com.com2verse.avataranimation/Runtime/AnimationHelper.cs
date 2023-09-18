/*===============================================================
* Product:		Com2Verse
* File Name:	AnimationHelper.cs
* Developer:	eugene9721
* Date:			2023-01-09 17:20
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Logger;
using UnityEngine;

namespace Com2Verse.AvatarAnimation
{
	public record AnimationClipModel
	{
		public string AvatarCode { get; }
		public string GenderCode { get; }
		public string ClipCode   { get; }
		public bool   IsValid    { get; }

		public AnimationClipModel(string avatarCode, string genderCode, string clipCode, string assetExtension)
		{
			AvatarCode = avatarCode;
			GenderCode = genderCode;
			ClipCode   = clipCode;
			IsValid    = assetExtension == AnimationDefine.ClipExtension;
		}

		public AnimationClipModel(string genderCode, string clipCode, string assetExtension)
		{
			AvatarCode = string.Empty;
			GenderCode = genderCode;
			ClipCode   = clipCode;
			IsValid    = assetExtension == AnimationDefine.ClipExtension;
		}

		public string GetAssetName() => $"{AvatarCode}_{GenderCode}_{ClipCode}_anim.{AnimationDefine.ClipExtension}";
	}

	public static class AnimationHelper
	{
		public static bool IsIdle(Animator animator) => animator.GetCurrentAnimatorStateInfo(0).shortNameHash == AnimationDefine.HashIdle ||
		                                                animator.GetCurrentAnimatorStateInfo(0).shortNameHash == AnimationDefine.HashWait;

		public static bool IsJumpStart(Animator animator) => animator.GetInteger(AnimationDefine.HashState) == 2;
		public static bool IsInAir(Animator animator)     => animator.GetInteger(AnimationDefine.HashState) == 3;
		public static bool IsJumpLand(Animator animator)  => animator.GetInteger(AnimationDefine.HashState) == 4;
		public static bool IsSitDown(Animator animator)   => animator.GetInteger(AnimationDefine.HashState) == 5;

		public static AnimationClipModel GetAnimationClipModel(string clipName)
		{
			if (string.IsNullOrEmpty(clipName)) return null;

			var codes = clipName.Split(AnimationDefine.Separator);

			if (codes.Length == 3)
				return new AnimationClipModel(codes[0], codes[1], codes[2]);
			if (codes.Length == 4)
				return new AnimationClipModel(codes[0], codes[1], codes[2], codes[3]);

			return null;
		}

		public static string GetClipSubName(string clipName)
		{
			if (string.IsNullOrEmpty(clipName)) return string.Empty;

			var model = GetAnimationClipModel(clipName);
			if (model != null) return model.ClipCode;

			C2VDebug.LogErrorCategory(nameof(Animator), $"Invalid clip name : {clipName}");
			return string.Empty;
		}

		public static string GetManClipNameFromWomanClip(string clipName)
		{
			if (string.IsNullOrEmpty(clipName)) return string.Empty;

			var model = GetAnimationClipModel(clipName);
			if (model != null)
				return $"{model.AvatarCode}_M_{model.ClipCode}_anim.{AnimationDefine.ClipExtension}";

			C2VDebug.LogErrorCategory(nameof(Animator), $"Invalid clip name : {clipName}");
			return string.Empty;
		}
	}
}
