/*===============================================================
* Product:		Com2Verse
* File Name:	AnimationEventParser.cs
* Developer:	eugene9721
* Date:			2023-02-10 18:29
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;

namespace Com2Verse.AvatarAnimation
{
	// TODO: 테이블 데이터와 같은 형식으로 사용하자
	public enum eAnimationEvent
	{
		NONE,
		BEGIN_FX,
		END_FX,
		SOUND,
		SOUND_IS_MINE,
		CUSTOM_EVENT,
	}

	[Flags]
	public enum eEffectType
	{
		NONE               = 1 << 0,
		SET_PARENT_TO_BONE = 1 << 1,
	}

	public static class AnimationEventParser
	{
		private const string ParseUnit = ";";

		/// <summary>
		/// 애니메이션 이벤트의 'Function'을 이용하여 애니메이션 이벤트의 타입을 반환
		/// </summary>
		/// <param name="functionName">파싱할 이벤트의 Function 필드의 값</param>
		/// <returns>반환된 애니메이션 이벤트 타입</returns>
		public static eAnimationEvent ParseFunctionName(string functionName)
		{
			if (string.IsNullOrEmpty(functionName)) return eAnimationEvent.NONE;
			var result = Enum.TryParse(functionName, out eAnimationEvent myStatus);
			return result ? myStatus : eAnimationEvent.NONE;
		}

		/// <summary>
		/// 애니메이션 이펙트 이벤트를 위한 파싱 함수
		/// </summary>
		/// <param name="stringParam">파싱할 원본의 stringParam</param>
		/// <param name="boneName">이펙트가 시작될 본의 이름</param>
		/// <param name="assetName">이펙트 에셋의 이름</param>
		/// <param name="duration">이펙트의 지속 시간</param>
		/// <param name="type">이펙트의 타입을 지정한 플래그 값</param>
		/// <returns></returns>
		public static bool ParseFx(string stringParam, out string boneName, out string assetName, out float duration, out eEffectType type)
		{
			boneName  = string.Empty;
			assetName = string.Empty;
			duration  = 0f;
			type      = eEffectType.NONE;

			// TODO: 캐싱

			if (string.IsNullOrEmpty(stringParam)) return false;

			var split = stringParam.Split(ParseUnit);
			if (split.Length < 2) return false;

			boneName  = split[0];
			assetName = split[1];

			if (split.Length > 2)
				if (float.TryParse(split[2], out duration) == false)
					duration = 0f;

			if (split.Length > 3)
				if (int.TryParse(split[3], out var typeInt))
					type = (eEffectType)typeInt;
			return true;
		}

		/// <summary>
		/// 애니메이션 사운드 이벤트를 위한 파싱 함수
		/// </summary>
		/// <param name="stringParam">파싱할 원본의 string Param</param>
		/// <param name="soundString">사운드 에셋을 로드할떄 사용할 문자열</param>
		/// <param name="isAssetName">true면 'soundString' 그대로가 로드할 에셋의 이름, 아니면 에셋을 로드할 키 값</param>
		/// <returns></returns>
		public static bool ParseSound(string stringParam, out string soundString, out bool isAssetName)
		{
			soundString = string.Empty;
			isAssetName = false;
			return true;
		}
	}
}
