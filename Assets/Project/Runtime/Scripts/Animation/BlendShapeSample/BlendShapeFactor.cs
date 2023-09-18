/*===============================================================
* Product:		Com2Verse
* File Name:	BlendShapeMixer.cs
* Developer:	tlghks1009
* Date:			2022-05-20 11:59
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse
{
	[ExecuteInEditMode]
	public sealed class BlendShapeFactor : MonoBehaviour
	{
		private SkinnedMeshRenderer _skinnedMeshRenderer;
		[Range(0f, 1f )]
		[SerializeField] private float _factor;

		private void Awake()
		{
			_skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
		}


		private void Update()
		{
			if (_factor < 0.5f)
			{
				_skinnedMeshRenderer.SetBlendShapeWeight(0, ((_factor - 0.5f ) * -2f ) * 100f );
				_skinnedMeshRenderer.SetBlendShapeWeight( 1, 0 );	
			}	
			else if (_factor > 0.5f)
			{
				_skinnedMeshRenderer.SetBlendShapeWeight(1, ( (0.5f - _factor ) * -2f) * 100f );
				_skinnedMeshRenderer.SetBlendShapeWeight(0, 0);
			}
			else
			{
				_skinnedMeshRenderer.SetBlendShapeWeight(0, 0 );
				_skinnedMeshRenderer.SetBlendShapeWeight(1, 0);
			}
		}
	}
}
