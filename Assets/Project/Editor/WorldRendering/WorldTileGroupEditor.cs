// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	WorldTileGroupEditor.cs
//  * Developer:	yangsehoon
//  * Date:		2023-04-11 오전 11:47
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.PhysicsAssetSerialization;
using Com2Verse.Rendering.World;
using Com2VerseEditor.PhysicsSerializer;
using Com2VerseEditor.Rendering.World;
using Unity.EditorCoroutines.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Com2VerseEditor.WorldRendering
{
	[CustomEditor(typeof(WorldTileGroup)), CanEditMultipleObjects]
	public class WorldTileGroupEditor : Editor
	{
		private Dictionary<string, EditorUtil.PagedArrayContext> _pageMap = new();
		private Dictionary<string, BaseObject> _objectTable = null;
		private Dictionary<long, List<InteractionLink>> _linkTable = null;
		private bool _extractWorkStarted = false;

		private static string _interactionValuePath;

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("ObjectInteractionValue 저장 폴더", GUILayout.MinWidth(200));
			EditorGUILayout.LabelField(_interactionValuePath,          GUILayout.MinWidth(150));
			if (GUILayout.Button("폴더 설정"))
				_interactionValuePath = EditorUtility.SaveFolderPanel("저장 폴더", _interactionValuePath, null);
			EditorGUILayout.EndHorizontal();
			
			SerializedProperty currentProperty = serializedObject.GetIterator();
			currentProperty.Next(true);
			while (currentProperty.NextVisible(false))
			{
				if (currentProperty.isArray)
				{
					string address = currentProperty.propertyPath;
					if (!_pageMap.ContainsKey(address))
						_pageMap.Add(address, new EditorUtil.PagedArrayContext());

					_pageMap[address] = EditorUtil.ReadOnlyPagedArray(currentProperty, _pageMap[address]);
				}
				else
				{
					EditorGUILayout.PropertyField(currentProperty);
				}
			}

			if (GUILayout.Button("Scan Navmesh"))
			{
				ScanWorldNavGraph();
			}

			if (_extractWorkStarted)
			{
				EditorGUILayout.HelpBox("추출중...", MessageType.Info);
			}
			else
			{
				if (GUILayout.Button("Extract CSV"))
				{
					_extractWorkStarted = true;
					EditorCoroutineUtility.StartCoroutine(ExtractWorldObjectData(), this);
				}
			}

			if (serializedObject.hasModifiedProperties)
				serializedObject.ApplyModifiedProperties();
		}

		private void ScanWorldNavGraph()
		{
			WorldTileGroup zoneData = target as WorldTileGroup;

			EditorSceneManager.OpenScene($"{Application.dataPath}/Project/Editor/PathFinding/ScanStudio.unity");

			WorldDataUtility.PopulatePhysicsGeometry(zoneData, parent =>
			{
				if (parent.IsReferenceNull())
				{
					Debug.LogError($"{zoneData.name} 의 지형정보를 로딩하는데 실패했습니다!");
					return;
				}

				PathGeneratorEditor.InitializeAstar();
				string navmeshName = (target as WorldTileGroup).name.Split('_')[1];
				PathGeneratorEditor.Scan($"World_{navmeshName}_navmesh.bytes", new PathGenerateOption()
				{
					MaxBorderEdgeLength = 3,
					MaxEdgeError = 1
				}, Vector3.zero);
				
				DestroyImmediate(parent);
			});
		}

		private IEnumerator LoadTableData()
		{
			_objectTable = new Dictionary<string, BaseObject>();
			
			var objectDataLoadHandle = Addressables.LoadAssetAsync<TextAsset>("BaseObject.bytes");
			yield return objectDataLoadHandle;

			if (objectDataLoadHandle.Status == AsyncOperationStatus.Succeeded)
			{
				var objectData = objectDataLoadHandle.Result;
				var objectTableTaskHandle = TableBaseObject.LoadAsync(objectData?.bytes);
				yield return objectTableTaskHandle;

				var dataTable = objectTableTaskHandle.Result.Datas;
				
				foreach (var data in dataTable.Values)
				{
					if (!string.IsNullOrEmpty(data.Name))
						_objectTable.Add(data.Name, data);
				}
			}

			_linkTable = new Dictionary<long, List<InteractionLink>>();

			var linkDataLoadHandle = Addressables.LoadAssetAsync<TextAsset>("InteractionLink.bytes");
			yield return linkDataLoadHandle;

			if (linkDataLoadHandle.Status == AsyncOperationStatus.Succeeded)
			{
				var linkData = linkDataLoadHandle.Result;
				var linkTableTaskHandle = TableInteractionLink.LoadAsync(linkData?.bytes);
				yield return linkTableTaskHandle;

				var dataTable = linkTableTaskHandle.Result.Datas;

				foreach (var data in dataTable.Values)
				{
					if (_linkTable.TryGetValue(data.BaseObjectID, out var list))
					{
						list.Add(data);
					}
					else
					{
						list = new List<InteractionLink>() { data };
						_linkTable.Add(data.BaseObjectID, list);
					}
				}
			}
		}

		private long FindLinkId(long baseObjectId, short triggerIndex, short callbackIndex)
		{
			if (_linkTable.TryGetValue(baseObjectId, out List<InteractionLink> list))
			{
				foreach (var interaction in list)
				{
					if (interaction.TriggerIndex == triggerIndex && interaction.CallbackIndex == callbackIndex)
					{
						return interaction.InteractionLinkID;
					}
				}
			}

			throw new Exception($"{baseObjectId}의 {triggerIndex} / {callbackIndex}에 해당하는 interaction이 InteractionLink 테이블에 등록되지 않아있습니다. 등록 후 추출 하세요.");
		}

		private IEnumerator ExtractWorldObjectData()
		{
			yield return LoadTableData();

			WorldTileGroup zoneData = target as WorldTileGroup;
			
			// serverobject data 분리됨 . 2023.08. ljk
			string serverObjectFilename =
				WorldRenderingConstants.SERVEROBJECT_FILENAME( int.Parse(zoneData.GroupKey) );
			WorldServerObjectData serverObjectData = AssetDatabase.LoadAssetAtPath<WorldServerObjectData>(WorldRenderingConstants.WORLD_SERVEROBJECT_FILE_STORAGE+"/"+ serverObjectFilename);

			if (serverObjectData == null)
			{
				Debug.LogWarning($"{WorldRenderingConstants.WORLD_SERVEROBJECT_FILE_STORAGE}/{serverObjectFilename} 가 없음!!.");
				_extractWorkStarted = false;
				yield break;
			}
			
			ObjectDatabaseModel[] data = new ObjectDatabaseModel[serverObjectData.ServerObjectDatas.Length];
			List<ObjectInteractionDatabaseModel> interactionList = new List<ObjectInteractionDatabaseModel>();
			C2VEventTrigger triggerHolder = new GameObject("Dummy").AddComponent<C2VEventTrigger>();

			for (int index = 0; index < data.Length; index++)
			{
				var savedData = serverObjectData.ServerObjectDatas[index];
				if (!_objectTable.TryGetValue(savedData.PrefabAddress, out BaseObject targetObject))
				{
					Debug.LogWarning($"{savedData.PrefabAddress}가 Object 기획데이터 테이블에 등록되지 않아서 스킵합니다.");
					continue;
				}
				data[index] = new ObjectDatabaseModel()
				{
					ObjectId = $"m-0001-{savedData.ServerObjectId}",
					BaseObjectId = targetObject.ID,
					Location = savedData.TransformMatrix.GetT(),
					Size = savedData.TransformMatrix.GetS(),
					Rotation = savedData.TransformMatrix.GetR().eulerAngles,
					HexCode = "#000000",
					ObjectWeight = 1,
					ObjectGravity = 10,
					ObjectPath = "",
				};
				
				for (short triggerIndex = 1; triggerIndex <= savedData.EventTriggerDatas.Length; triggerIndex++)
				{
					var triggerData = savedData.EventTriggerDatas[triggerIndex - 1];
					JsonUtility.FromJsonOverwrite(triggerData.RawData, triggerHolder);

					if (triggerHolder.Callback == null) continue;
					
					for (short callbackIndex = 1; callbackIndex <= triggerHolder.Callback.Length; callbackIndex++)
					{
						var parameter = triggerHolder.Callback[callbackIndex - 1];
						if (parameter.TagInternal != null)
						{
							for (int parameterIndex = 1; parameterIndex <= parameter.TagInternal.Count; parameterIndex++)
							{
								interactionList.Add(new ObjectInteractionDatabaseModel()
								{
									MappingId = "m-0001",
									SpaceObjectId = savedData.ServerObjectId,
									InteractionValue = parameter.TagInternal[parameterIndex - 1].Value,
									InteractionNo = parameterIndex,
									InteractionLink = FindLinkId(targetObject.ID, triggerIndex, callbackIndex),
								});
							}
						}
					}
				}
			}

			using (FileStream interactionValueStream = File.Create(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "World_Interaction.csv")))
			{
				interactionValueStream.Write(Encoding.UTF8.GetBytes("mapping_id,space_object_id,interaction_link,interaction_no,interaction_value\n"));
				foreach (var value in interactionList)
				{
					interactionValueStream.Write(Encoding.UTF8.GetBytes(
						                             $"{value.MappingId},{value.SpaceObjectId},{value.InteractionLink},{value.InteractionNo},{value.InteractionValue}\n"));
				}
			}

			var spaceFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SpaceInteractionValue.csv");
			if (File.Exists(spaceFilePath))
			{
				var spaceValues = File.ReadAllLines(spaceFilePath);
				foreach (var spaceValue in spaceValues)
				{
					if (spaceValue.StartsWith("mapping_id,")) continue;
					var split = spaceValue.Split(',');
					interactionList.Add(new()
					{
						MappingId        = split[0],
						SpaceObjectId    = long.Parse(split[1]),
						InteractionLink  = long.Parse(split[2]),
						InteractionNo    = int.Parse(split[3]),
						InteractionValue = split[4],
					});
				}
			}

			using (FileStream mergedStream = File.Create(
				       Path.Combine(string.IsNullOrEmpty(_interactionValuePath) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : _interactionValuePath,
				                    "ObjectInteractionValue.csv")))
			{
				mergedStream.Write(Encoding.UTF8.GetBytes("mapping_id,space_object_id,interaction_link,interaction_no,interaction_value\n"));
				foreach (var value in interactionList)
				{
					mergedStream.Write(Encoding.UTF8.GetBytes(
						                   $"{value.MappingId},{value.SpaceObjectId},{value.InteractionLink},{value.InteractionNo},{value.InteractionValue}\n"));
				}
			}
			
			using (FileStream objectStream = File.Create(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "World_Object.csv")))
			{
				objectStream.Write(Encoding.UTF8.GetBytes("object_id,mapping_id,base_object_id,location_x,location_y,location_z,size_x,size_y,size_z,rotation_x,rotation_y,rotation_z,hex_code,object_weight,object_gravity\n"));
				foreach (var value in data)
				{
					if (value == null) continue;
					
					objectStream.Write(Encoding.UTF8.GetBytes(
						                   $"{value.ObjectId},m-0001,{value.BaseObjectId},{Math.Round(value.Location.x, 3)},{Math.Round(value.Location.y, 3)},{Math.Round(value.Location.z, 3)},{Math.Round(value.Size.x, 3)},{Math.Round(value.Size.y, 3)},{Math.Round(value.Size.z, 3)},{Math.Round(value.Rotation.x, 3)},{Math.Round(value.Rotation.y, 3)},{Math.Round(value.Rotation.z, 3)},{value.HexCode},{value.ObjectWeight},{value.ObjectGravity}\n"));
				}
				// NOTE: temp data(마이스 수동)
				objectStream.Write(Encoding.UTF8.GetBytes("m-0001-20131001008,m-0001,1143,-409,0.02,39,1,1,1,0,270,0,#000000,1,10\n"));
				objectStream.Write(Encoding.UTF8.GetBytes("m-0001-20111001007,m-0001,1150,-351,0,39,1,1,1,0,0,0,#000000,1,10\n"));
			}
			DestroyImmediate(triggerHolder.gameObject);
			
			Debug.Log("World Scan Done");
			_extractWorkStarted = false;
		}
	}
}
