/*===============================================================
* Product:		Com2Verse
* File Name:	AnimatorBuilderDefine.cs
* Developer:	eugene9721
* Date:			2022-07-13 18:50
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.IO;
using Com2Verse.Data;

namespace Com2VerseEditor.AvatarAnimation
{
	public enum eAnimationClipType
	{
		NONE,
		EMOTION,
		INTERACTIVE,
		LEAN,
		MOVEMENT,
		SELECT,
		SIT,
	}

	public static class AnimatorBuilderDefine
	{
		private static readonly string AnimatorExtension          = ".controller";
		private static readonly string AnimatorOverrideExtension = ".overrideController";

		private static readonly string AnimatorPath =
			"Assets/Project/Bundles/Com2us/ArtAsset/Character/Animation/AnimatorController";

		public static readonly string PreviewHeader    = "__preview__";
		public static readonly string PreviewAnimation = "Take 001";

		public static readonly string AnimFBXFolderName  = "AnimationFBX";
		public static readonly string AnimClipFolderName = "Animation_AAG";

		public static readonly  string TempDirectoryName = "Temp";
		private static readonly string BaseAnimatorID    = "Woman";

		public static string GetAnimatorName(string id, eAnimatorType type) => type switch
		{
			eAnimatorType.WORLD            => $"{id}WorldAnimator",
			eAnimatorType.SIT_STATE        => $"{id}SitAnimator",
			eAnimatorType.AVATAR_CUSTOMIZE => $"{id}AvatarCustomize",
			_                              => "",
		};

		public static string GetBaseAnimatorName(eAnimatorType type) =>
			GetAnimatorName(BaseAnimatorID, type);

		public static string GetAnimatorFullPath(string name) =>
			Path.Combine(AnimatorPath, GetAnimatorFullName(name));

		public static string GetAnimatorOverrideFullPath(string name) =>
			Path.Combine(AnimatorPath, GetAnimatorOverrideFullName(name));

		public static string GetAnimatorFullName(string name) =>
			$"{name}{AnimatorExtension}";

		public static string GetAnimatorOverrideFullName(string name) =>
			$"{name}{AnimatorOverrideExtension}";

		public static string GetBaseAnimatorPath(eAnimatorType type) =>
			Path.Combine(AnimatorPath, GetAnimatorFullName(GetBaseAnimatorName(type)));
	}
}
