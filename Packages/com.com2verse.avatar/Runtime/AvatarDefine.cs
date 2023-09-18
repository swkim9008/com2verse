/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarDefine.cs
* Developer:	eugene9721
* Date:			2022-12-02 19:52
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Data;
using UnityEngine;

namespace Com2Verse.Avatar
{
	public static class AvatarDefine
	{
		/// <summary>
		/// Unity Humanoid Avatar Mapping Essential Bone
		/// </summary>
		public static readonly HumanBodyBones[] EssentialBones =
		{
			HumanBodyBones.Hips, HumanBodyBones.Spine, HumanBodyBones.Head,
			HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand,
			HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand,
			HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot,
			HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot
		};

		public static readonly HumanBodyBones[] BodyOptionalBones =
		{
			HumanBodyBones.Chest,
			HumanBodyBones.UpperChest
		};

		public static readonly HumanBodyBones[] ArmOptionalBones =
		{
			HumanBodyBones.LeftShoulder,
			HumanBodyBones.RightShoulder
		};

		public static readonly HumanBodyBones[] LegOptionalBones =
		{
			HumanBodyBones.LeftToes,
			HumanBodyBones.RightToes
		};

		public static readonly HumanBodyBones[] FaceOptionalBones =
		{
			HumanBodyBones.Neck,
			HumanBodyBones.LeftEye,
			HumanBodyBones.RightEye,
			HumanBodyBones.Jaw
		};
	}

	public static class MetaverseAvatarDefine
	{
		public static readonly string BaseBodyObjectName = "BASEBODY";

		// Renderers
		public static readonly string WomanHeadRendererName = "PC01_W_Head";
		public static readonly string ManHeadRendererName   = "PC01_M_HEAD";

		public static int DefaultBodyShapeId = 2;

		public static string GetHeadRendererName(eAvatarType avatarType) => avatarType switch
		{
			eAvatarType.PC01_W => WomanHeadRendererName,
			eAvatarType.PC01_M => ManHeadRendererName,
			_                  => string.Empty
		};
	}
}
