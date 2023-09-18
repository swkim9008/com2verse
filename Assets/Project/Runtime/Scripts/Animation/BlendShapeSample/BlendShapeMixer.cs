/*===============================================================
* Product:		Com2Verse
* File Name:	BlendShapeMixer.cs
* Developer:	tlghks1009
* Date:			2022-05-20 11:59
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;

namespace Com2Verse
{
	[ExecuteInEditMode]
	public sealed class BlendShapeMixer : MonoBehaviour
	{
		private SkinnedMeshRenderer _skinnedMeshRenderer;

		private void Awake()
		{
			_skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
		}


		private void Update()
		{
			float oneBlendShape = _skinnedMeshRenderer.GetBlendShapeWeight(0);
			float twoBlendShape = _skinnedMeshRenderer.GetBlendShapeWeight(1);


			float min = Math.Min(oneBlendShape, twoBlendShape);
			_skinnedMeshRenderer.SetBlendShapeWeight(2, min);
		}
	}
}
