// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	BuilderSaveDataModel.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-07 오후 12:53
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

namespace Com2Verse.Builder
{
	public enum eBuilderSpaceType
	{
		EMPTY,
		TEMPLATE,
	}

	[System.Serializable]
	public struct BuilderSaveDataModel
	{
		public eBuilderSpaceType type;
		public long templateId;
		
		public long objectMaxIndex;
		public BuilderGameObjectSaveDataModel[] objects;

		public int wallMaxIndex;
		public BuilderWallSaveDataModel[] walls;
		public int floorMaxIndex;
		public BuilderFloorSaveDataModel[] floors;
	}

	[System.Serializable]
	public struct BuilderWallSaveDataModel
	{
		public BuilderTransform localTransform;
		public bool isCorner;
		public int index;
		public Float3 innerNormalDirection;
		public int[] neighbor;
		public long appliedTextureId;
	}

	[System.Serializable]
	public struct BuilderFloorSaveDataModel
	{
		public BuilderTransform localTransform;
		public Float2 floorScale;
		public int index;
		public Float3 innerNormalDirection;
		public int[] neighbor;
		public long appliedTextureId;
	}

	[System.Serializable]
	public struct BuilderGameObjectSaveDataModel
	{
		public BuilderTransform localTransform;
		public long serializationId;
		public long parentId;
		public long objectId;
	}

	[System.Serializable]
	public struct BuilderTransform
	{
		public Float3 localPosition;
		public Float4 localRotation;
		public Float3 localScale;
	}

	[System.Serializable]
	public struct Float2
	{
		public Float2(float x, float y)
		{
			this.x = x;
			this.y = y;
		}
		
		public float x;
		public float y;
		
#if UNITY_2021_3_OR_NEWER
		public static explicit operator Float2(UnityEngine.Vector2 v) => new Float2(v.x, v.y);
		public static implicit operator UnityEngine.Vector2(Float2 v) => new UnityEngine.Vector2(v.x, v.y);
#endif
	}

	[System.Serializable]
	public struct Float3
	{
		public Float3(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
		
		public float x;
		public float y;
		public float z;
		
#if UNITY_2021_3_OR_NEWER
		public static explicit operator Float3(UnityEngine.Vector3 v) => new Float3(v.x, v.y, v.z);
		public static implicit operator UnityEngine.Vector3(Float3 v) => new UnityEngine.Vector3(v.x, v.y, v.z);
#endif
	}

	[System.Serializable]
	public struct Float4
	{
		public Float4(float x, float y, float z, float w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}
		
		public float x;
		public float y;
		public float z;
		public float w;

#if UNITY_2021_3_OR_NEWER
		public static explicit operator Float4(UnityEngine.Vector4 v) => new Float4(v.x, v.y, v.z, v.w);
		public static implicit operator UnityEngine.Vector4(Float4 v) => new UnityEngine.Vector4(v.x, v.y, v.z, v.w);
#endif
	}
}