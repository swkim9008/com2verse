/*===============================================================
* Product:		Com2Verse
* File Name:	ModelBoundExtractor.cs
* Developer:	yangsehoon
* Date:			2023-05-26 11:58
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections.Generic;

namespace Com2Verse.Builder
{
	[CreateAssetMenu(fileName = "ModelBoundDatabase", menuName = "Com2Verse/Builder/ModelBoundDatabase")]
	public sealed class ModelBoundDatabase : ScriptableObject
	{
		[System.Serializable]
		public struct Bound
		{
			public long ObjectId;
			public Vector3 Center;
			public Vector3 Size;
		}

#if UNITY_EDITOR
		public Bound[] InternalBounds
		{
			get => _bounds;
			set => _bounds = value;
		}
#endif
		
		[SerializeField] private Bound[] _bounds;
		public Dictionary<long, Bounds> Bounds { get; } = new Dictionary<long, Bounds>();

		public void LoadData()
		{
			foreach (var bound in _bounds)
			{
				if (bound.ObjectId != 0)
					Bounds.Add(bound.ObjectId, new Bounds(bound.Center, bound.Size));
			}
		}
	}
}
