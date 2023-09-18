// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	TransformExtension.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-22 오후 4:12
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System.Runtime.CompilerServices;
using Com2Verse.Extension;
using UnityEngine;

namespace Com2Verse.Builder
{
	public static class TransformExtension
	{
		private const int BasePrecision = 200;
		
		/// <summary>
		/// 서버에서 CSU가 전달될때 어차피 소숫점 2자리 까지만 보존되기 때문에, 공간빌더상에서도
		/// 낮은 소수점 자리는 버리고 보여줘야 내가 저장한걸 실제로 게임에서 봤을때
		/// 공간빌더 상의 배치와 차이가 없게 된다.
		/// </summary>
		/// <param name="transform"></param>
		public static void RoundTransform(this Transform transform)
		{
			Vector3 additionalPrecision = Vector3.one;
			if (!transform.parent.IsReferenceNull())
				additionalPrecision = transform.parent.lossyScale;
			
			transform.localScale = RoundVector3(transform.localScale, Vector3.one);
			transform.localPosition = RoundVector3(transform.localPosition, additionalPrecision);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Vector3 RoundVector3(Vector3 target, Vector3 additionalPrecision)
		{
			return new Vector3(Mathf.Round(target.x * BasePrecision * additionalPrecision.x) / (BasePrecision * additionalPrecision.x),
			                   Mathf.Round(target.y * BasePrecision * additionalPrecision.y) / (BasePrecision * additionalPrecision.y),
			                   Mathf.Round(target.z * BasePrecision * additionalPrecision.z) / (BasePrecision * additionalPrecision.z));
		}
	}
}
