// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	ModelBoundDatabaseEditor.cs
//  * Developer:	yangsehoon
//  * Date:		2023-05-26 오후 12:35
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System.Collections;
using System.Collections.Generic;
using Com2Verse.Builder;
using Com2Verse.Data;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Com2VerseEditor.Builder
{
	[CustomEditor(typeof(ModelBoundDatabase))]
	public class ModelBoundDatabaseEditor : Editor
	{
		private static Dictionary<long, BaseObject> _objectData;
		private WaitForEndOfFrame _delay = new WaitForEndOfFrame();
		private bool _isProcessing = false;

		public static IEnumerator TurboLoadingHandle(AsyncOperationHandle handle)
		{
			while (handle.PercentComplete < 1)
			{
				EditorApplication.QueuePlayerLoopUpdate();
				EditorApplication.QueuePlayerLoopUpdate();
				yield return null;
			}
		}
		
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_bounds"));
			
			if (_isProcessing)
			{
				EditorGUILayout.HelpBox("추출중..", MessageType.Info);
			}
			else
			{
				if (GUILayout.Button("Extract Bounds"))
				{
					_isProcessing = true;
					EditorCoroutineUtility.StartCoroutineOwnerless(ExtractData());
				}
			}

			if (serializedObject.hasModifiedProperties)
				serializedObject.ApplyModifiedProperties();
		}

		private IEnumerator LoadTableData()
		{
			var objectDataHandle = Addressables.LoadAssetAsync<TextAsset>("BaseObject.bytes");
			yield return TurboLoadingHandle(objectDataHandle);

			if (objectDataHandle.Status == AsyncOperationStatus.Succeeded)
			{
				var objectData = objectDataHandle.Result;
				var objectTableTaskHandle = TableBaseObject.LoadAsync(objectData?.bytes);
				yield return objectTableTaskHandle;

				_objectData = objectTableTaskHandle.Result.Datas;
			}
			else
			{
				UnityEngine.Debug.LogError("BaseObject 기획데이터를 로딩하는데 실패했습니다!!!");
				_objectData = new Dictionary<long, BaseObject>();
			}
		}

		public IEnumerator ExtractData()
		{
			yield return LoadTableData();

			var boundDatabase = target as ModelBoundDatabase;
			boundDatabase.InternalBounds = new ModelBoundDatabase.Bound[_objectData.Count];

			Dictionary<string, long> targetPrefabNames = new Dictionary<string, long>();
			foreach (var objectData in _objectData.Values)
			{
				if (!string.IsNullOrEmpty(objectData.Name))
					targetPrefabNames.Add(objectData.Name, objectData.ID);
			}

			var settings = AddressableAssetSettingsDefaultObject.Settings;
			
			int i = 0;
			foreach (var group in settings.groups)
			{
				foreach (var entry in group.entries)
				{
					if (targetPrefabNames.TryGetValue(entry.address, out long id))
					{
						var targetObject = (GameObject)entry.TargetAsset;
						// Find center of the model in parent(world) coordinate
						var filters = targetObject.GetComponentsInChildren<MeshFilter>();

						Vector3 min = Vector3.positiveInfinity;
						Vector3 max = Vector3.negativeInfinity;
						foreach (var filter in filters)
						{
							if (filter.sharedMesh == null) continue;
							
							var vertices = filter.sharedMesh.vertices;
							foreach (var vertex in vertices)
							{
								Vector3 worldCoordinateVertex = filter.transform.TransformPoint(vertex);
								min = Vector3.Min(worldCoordinateVertex, min);
								max = Vector3.Max(worldCoordinateVertex, max);
							}
						}

						Bounds modelBounds = new Bounds();
						modelBounds.SetMinMax(min, max);

						boundDatabase.InternalBounds[i++] = new ModelBoundDatabase.Bound()
						{
							ObjectId = id,
							Center = modelBounds.center,
							Size = modelBounds.size
						};
					}
				}

				yield return _delay;
			}

			_isProcessing = false;
			serializedObject.ApplyModifiedProperties();
		}	
	}
}
