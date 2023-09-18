/*===============================================================
* Product:		Com2Verse
* File Name:	PathGenerator.cs
* Developer:	yangsehoon
* Date:			2023-04-11 15:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/


using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Com2VerseEditor
{
	[CreateAssetMenu(fileName = "PathGenerator", menuName = "Com2Verse/Physics Serialization/Path Generator")]
	public sealed class PathGenerator : ScriptableObject
	{
		[SerializeField] private SerializedDictionary<string, PathGenerateOption> _pathGenerateOptions = new SerializedDictionary<string, PathGenerateOption>();
		
		public SerializedDictionary<string, PathGenerateOption> PathGenerateOptions => _pathGenerateOptions;
	}

	[System.Serializable]
	public sealed class SerializedDictionary<TKey, TValue> where TValue : new()
	{
		[SerializeField] public List<TKey> Keys;
		[SerializeField] public List<TValue> Values;
		public int Count => Keys.Count;
		
		public SerializedDictionary()
		{
			Keys = new();
			Values = new();
		}

		public bool ContainsKey(TKey key) => Keys.Contains(key);

		public KeyValuePair<TKey, TValue> this[int index] => new KeyValuePair<TKey, TValue>(Keys[index], Values[index]);

		public TValue this[TKey key]
		{
			get
			{
				int index = Keys.IndexOf(key);
				if (index >= 0)
					return Values[index];

				return new TValue();
			}
		}

		public void Add(TKey key, TValue value)
		{
			Keys.Add(key);
			Values.Add(value);
		}

		public void Remove(TKey key)
		{
			int index = Keys.IndexOf(key);
			Keys.RemoveAt(index);
			Values.RemoveAt(index);
		}
	}

	[System.Serializable]
	public sealed class PathGenerateOption
	{
		public float CharacterRadius = 0.5f;
		public float WalkableHeight = 2;
		public float WalkableClimb = 0.3f;
		public float MaxSlope = 50f;
		public float MaxBorderEdgeLength = 20;
		public float MaxEdgeError = 2;
		[HideInInspector] public bool RasterizeTerrain = false;
		[HideInInspector] public bool RasterizeColliders = true;
		[HideInInspector] public bool RasterizeMeshes = false;
		public float CellSize = 0.35f;
		public float MinRegionSize = 0.2f;
	}
}
