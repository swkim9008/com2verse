// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	StickyObjectBase.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-09 오후 3:07
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using Com2Verse.Extension;
using UnityEngine;

namespace Com2Verse.Builder
{
	public abstract class BaseWallObject : MonoBehaviour
	{
		public MeshRenderer CurrentRenderer { get; private set; }
		public Material AssignedMaterial { get; set; }
		public int Index { get; set; } = -1;
		public Vector3 InnerNormalDirection { get; set; }
		public BaseWallObject[] Neighbor { get; set; }
		public long AppliedTextureId { get; set; }
		
		public virtual void PropagateAction(Action<BaseWallObject> action)
		{
			action(this);
		}
		
		public void Start()
		{
			CurrentRenderer = GetComponent<MeshRenderer>();
			if (AssignedMaterial.IsReferenceNull())
			{
				AssignedMaterial = CurrentRenderer.sharedMaterial;
			}
		}

		public void ResetTexture()
		{
			CurrentRenderer.material = AssignedMaterial;
		}
	}
}
