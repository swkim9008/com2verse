/*===============================================================
* Product:		Com2Verse
* File Name:	WholeSerializer.cs
* Developer:	yangsehoon
* Date:			2023-04-18 17:46
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.PhysicsAssetSerialization;
using Com2VerseEditor.PhysicsAssetSerialization;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using eColliderType = Com2Verse.PhysicsAssetSerialization.eColliderType;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Com2VerseEditor.PhysicsSerializer
{
	[CreateAssetMenu(fileName = "WholeSerializer", menuName = "Com2Verse/Physics Serialization/Whole Serializer")]
	[CustomEditor(typeof(WholeSerializer))]
	public class WholeSerializer : Editor
	{
#region Editor
		private static bool _isWorking = false;
		private static bool _isLoading = false;

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("ObjectInteractionValue 저장 폴더", GUILayout.MinWidth(200));
			EditorGUILayout.LabelField(_interactionValuePath, GUILayout.MinWidth(150));
			if (GUILayout.Button("폴더 설정"))
				_interactionValuePath = EditorUtility.SaveFolderPanel("저장 폴더", _interactionValuePath, null);
			EditorGUILayout.EndHorizontal();

			if (!_isLoading && GUILayout.Button("Load Table Data"))
			{
				EditorCoroutineUtility.StartCoroutineOwnerless(InitCsv());
			}
			if (_isLoading)
			{
				EditorGUILayout.HelpBox("Loading Table Data..", MessageType.Info);
			}
			
			if (_objectTable == null || _objectTable.Count == 0 || _spaceTemplateTable == null || _spaceTemplateTable.Count == 0)
				return;

			DrawOptionCreate();
			
			if (!_isWorking && GUILayout.Button("CSV 추출 (Space, BaseObject, Arrangement)", GUILayout.Height(20)))
			{
				Debug.Log("Start Serialize");
				_isWorking = true;
				_currentProgress = 0;
				EditorCoroutineUtility.StartCoroutine(GenerateCsv(), this);
			}

			if (!_isWorking && GUILayout.Button("InteractionLink 추출", GUILayout.Height(20)))
			{
				Debug.Log("Start Serialize");
				_isWorking = true;
				_currentProgress = 0;
				EditorCoroutineUtility.StartCoroutine(ExtractObjectInteractionLink(), this);
			}

			if (!_isWorking && GUILayout.Button("아트 Prefab의 BaseObjectId 맞는지 확인", GUILayout.Height(20)))
			{
				Debug.Log("Start Validation");
				_isWorking = true;
				_currentProgress = 0;
				EditorCoroutineUtility.StartCoroutine(ValidateBaseObjectId(), this);
			}

			if (_isWorking)
			{
				EditorGUILayout.LabelField($"Loaded Scene");
				EditorGUI.indentLevel++;
				EditorGUILayout.BeginVertical();
				foreach (var template in _spaceTemplateTable.Values)
				{
					EditorGUILayout.LabelField(template.SpaceTemplateName);
				}
				EditorGUILayout.EndVertical();
				EditorGUI.indentLevel--;
				EditorGUILayout.HelpBox("Now Working...", MessageType.Info);
				ShowProgress();
			}

			if (serializedObject.hasModifiedProperties)
				serializedObject.ApplyModifiedProperties();
		}
		
		private void DrawOptionCreate()
		{
			if (_sceneNameArray == null) return;

			EditorGUILayout.LabelField("추출 대상 (비어있으면 전체 추출)");
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.LabelField("Scene List");
			for (int i = 0; i < _targetSpaceTemplates.Count; ++i)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(_targetSpaceTemplates[i].SpaceTemplateName);
				if (GUILayout.Button("Remove"))
				{
					_targetSpaceTemplates.RemoveAt(i);
					Repaint();
					return;
				}

				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.BeginHorizontal();
			_optionTarget = EditorGUILayout.Popup("Scene Name", System.Math.Min(_optionTarget, _sceneNameArray.Length - 1), _sceneNameArray);
			if (GUILayout.Button("Add"))
			{
				var target = _spaceTemplates.Find((template) => template.SpaceTemplateName.Equals(_sceneNameArray[_optionTarget]));
				if (_targetSpaceTemplates.Contains(target))
					Debug.LogError("리스트에 중복값이 존재합니다.");
				else
					_targetSpaceTemplates.Add(target);
			}
			if (GUILayout.Button("Clear"))
			{
				_targetSpaceTemplates.Clear();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();
		}

		private void ShowProgress()
		{
			if (_isWorking)
			{
				EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), (float)_currentProgress / Math.Max(1, _totalProgress), "Progress");
			}
		}
		
		private IEnumerator GenerateCsv()
		{
			_totalProgress = _spaceTemplateTable.Count;

			var mappingTable = AssetDatabase.LoadAssetAtPath<SpaceMappingTable>("Assets/Project/Editor/PhysicsSerializer/SpaceMappingTable.asset");
			if (mappingTable != null)
			{
				SpaceMappingTable!.Clear();
				foreach (var mappingInfo in mappingTable.MappingInfoTable)
					SpaceMappingTable.Add(mappingInfo.MapTemplateId, mappingInfo.MappingId);
			}

			var targetTemplates = new List<SpaceTemplate>();
			targetTemplates.AddRange(_targetSpaceTemplates.Count == 0 ? _spaceTemplates : _targetSpaceTemplates);
			foreach (var spaceTemplate in targetTemplates)
			{
				Debug.Log($"Start Processing {spaceTemplate.SpaceTemplateName}");
				yield return _waitForEndOfFrame;
				
				yield return CsvExtractCurrentMap(spaceTemplate.SpaceTemplateName);

				yield return _waitForEndOfFrame;

				_currentProgress++;
				Repaint();
			}

			yield return ExtractNavmeshCut();

			MakeCsv();
			_isWorking = false;
		}
#endregion

		private class ObjectInteractionContainer
		{
			public string MappingId;
			public long SpaceObjectId;
			public int InteractionNo;
			public long InteractionLink;
			public string InteractionValue;
		}

		private static string _interactionValuePath;
		private readonly int _fixedCellSize = 16;

		private Vector2 _worldSize;
		private Vector2 _worldCenter;
		private Vector2Int _cellDivideNumber;
		
		private static Dictionary<long, BaseObject> _objectTable = null;
		private static Dictionary<long, SpaceTemplate> _spaceTemplateTable = null;

		private static int                 _optionTarget;
		private static string[]            _sceneNameArray;
		private static List<SpaceTemplate> _targetSpaceTemplates = new List<SpaceTemplate>();
		
		private static readonly Dictionary<long, string> SpaceMappingTable = new();

		// exception case of enum name conversion
		private readonly Dictionary<eLogicType, string> _maliciousEnumNameTable = new Dictionary<eLogicType, string>()
		{
			{eLogicType.ELEVATOR, "ELEVATOR"},
			{eLogicType.ENTER__MEETING, "ENTER_MEETING"},
			{eLogicType.EXIT__MEETING, "EXIT_MEETING"},
			{eLogicType.CHAIR, "CHAIR"},
			{eLogicType.CHAT__LOCATION, "CHAT_LOCATION"},
			{eLogicType.BOARD__WRITE, "BOARD_WRITE"},
			{eLogicType.BOARD__READ, "BOARD_READ"},
			{eLogicType.BOARD__AI, "BOARD_AI"},
			{eLogicType.YOUTUBE__VIDEO, "YOUTUBE_VIDEO"},
			{eLogicType.SMALLTALK__INTERNAL1, "SMALLTALK_INTERNAL1"},
			{eLogicType.SMALLTALK__INTERNAL2, "SMALLTALK_INTERNAL2"},
			{eLogicType.SMALLTALK__INTERNAL3, "SMALLTALK_INTERNAL3"},
			{eLogicType.SMALLTALK__INTERNAL4, "SMALLTALK_INTERNAL4"},
			{eLogicType.GO__WORK, "GO_WORK"},
			{eLogicType.WARP__BUILDING, "WARP_BUILDING"},
			{eLogicType.WARP__BUILDING__LIST, "WARP_BUILDING_LIST"},
			{eLogicType.WARP__SPACE, "WARP_SPACE"},
			{eLogicType.WARP__BUILDING__EXIT, "WARP_BUILDING_EXIT"},
			{eLogicType.WARP__SELECT, "WARP_SELECT"},
			{eLogicType.WARP__AUTO, "WARP_AUTO"},
			{eLogicType.WARP__NAMETAG, "WARP_NAMETAG"}
		};

		// CSV serialization
		private Dictionary<string, BaseObject> _prefabDictionary = new Dictionary<string, BaseObject>();
		private List<BaseObject> _internalObjectData = new List<BaseObject>();
		private List<SpaceTemplate> _spaceTemplates = new List<SpaceTemplate>();
		private List<SpaceTemplateArrangement> _spaceTemplateArrangements = new List<SpaceTemplateArrangement>();
		private List<ObjectInteractionContainer> _objectInteractionContainers = new List<ObjectInteractionContainer>();
		
		private long _objectIndex = 1000;
		private WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();
		private static int _totalProgress;
		private static int _currentProgress;

		public static IEnumerator TurboLoadingHandle(AsyncOperationHandle handle)
		{
			while (handle.PercentComplete < 1)
			{
				EditorApplication.QueuePlayerLoopUpdate();
				EditorApplication.QueuePlayerLoopUpdate();
				yield return null;
			}
		}
		
		[MenuItem("Com2Verse/Physics Asset Serialization/Serialize Everything")]
		private static void SelectWizard()
		{
			var thisObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/Project/Editor/PhysicsSerializer/WholeSerializer.asset");
			Selection.activeObject = thisObject;
			EditorGUIUtility.PingObject(thisObject);
		}

		public static string GetSceneAssetPath(string key)
		{
			string[] guids = AssetDatabase.FindAssets(key);
			
			foreach (var guid in guids)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				if (assetPath.Contains("ArtRawAssets")) continue;
				
				if (string.Equals(assetPath.Split('/')[^1].Split('.')[0], key) && assetPath.EndsWith(".unity"))
				{
					return Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length));
				}
			}
			return null;
		}
		
		private void OpenScanStudio()
		{
			EditorSceneManager.OpenScene($"{Application.dataPath}/Project/Editor/PathFinding/ScanStudio.unity");
		}

		public IEnumerator ValidateBaseObjectId()
		{
			OpenScanStudio();

			_totalProgress = _objectTable.Count;

			foreach (var template in _objectTable.Values)
			{
				if (string.IsNullOrEmpty(template.Name))
				{
					_currentProgress++;
					continue;
				}

				var instanceHandle = Addressables.InstantiateAsync(template.Name);
				yield return TurboLoadingHandle(instanceHandle);
				_currentProgress++;
				Repaint();

				if (instanceHandle.Status != AsyncOperationStatus.Succeeded)
				{
					Debug.LogError($"{template.Name} (#{template.ID})을(를) 로딩하는데 실패했습니다 (지워진 아트 에셋인 것이면 BaseObject 테이블에서 지워야 합니다)");
					continue;
				}

				if (!instanceHandle.Result.IsUnityNull())
				{
					var serverObject = instanceHandle.Result.GetComponent<ServerObject>();

					if (serverObject.IsReferenceNull())
					{
						Debug.LogWarning($"prefab {template.Name} (#{template.ID})은(는) ServerObject가 아닙니다. (초창기에 넣은거라서, BaseObject 테이블에서 지워도 무방합니다)");
					}
					else
					{
						long prefabId = serverObject.ObjectTypeID;
						if (prefabId != template.ID)
						{
							Debug.LogError($"prefab {template.Name} (#{template.ID})안의 BaseObjectId가 잘못되었습니다. prefab을 열어서 수정하고 저장하세요. (열기만 하면 커스텀 에디터에서 자동 수정됨)");
						}
					}

					DestroyImmediate(instanceHandle.Result);
				}
			}

			_isWorking = false;
		}

		private IEnumerator ExtractNavmeshCut()
		{
			OpenScanStudio();
			
			_totalProgress = _prefabDictionary.Count;

			foreach (var baseObject in _prefabDictionary.Values)
			{
				if (string.IsNullOrEmpty(baseObject.Name)) continue;

				var instanceHandle = Addressables.InstantiateAsync(baseObject.Name);
				yield return TurboLoadingHandle(instanceHandle);

				if (instanceHandle.Status != AsyncOperationStatus.Succeeded)
				{
					Debug.LogError($"Navmesh cut 추출 중 {baseObject.Name}을 로딩하는데 실패했습니다");
					_currentProgress++;
					Repaint();
					continue;
				}

				var instance = instanceHandle.Result;
				var bound = PathGeneratorEditor.GetNavmeshCut(instance);

				baseObject.NavMeshCenter = bound.Center;
				baseObject.NavMeshHeight = bound.Height;
				baseObject.NavMeshRadius = bound.Radius;

				DestroyImmediate(instance);

				_currentProgress++;
				Repaint();
			}

			_isWorking = false;
		}
		
		public IEnumerator ExtractObjectInteractionLink()
		{
			EditorSceneManager.OpenScene($"{Application.dataPath}/Project/Editor/PathFinding/ScanStudio.unity");
			
			Dictionary<long, InteractionLink> interactionLinks = new Dictionary<long, InteractionLink>();

			_totalProgress = _objectTable.Count;

			foreach (var template in _objectTable.Values)
			{
				if (string.IsNullOrEmpty(template.Name))
				{
					_currentProgress++;
					continue;
				}

				var instanceHandle = Addressables.InstantiateAsync(template.Name);
				yield return TurboLoadingHandle(instanceHandle);
				_currentProgress++;
				Repaint();

				if (instanceHandle.Status != AsyncOperationStatus.Succeeded)
				{
					Debug.LogError($"{template.Name}을 로딩하는데 실패했습니다");
					continue;
				}

				if (!instanceHandle.Result.IsUnityNull())
				{
					ExtractDataFromObject(instanceHandle.Result, template.ID);
					DestroyImmediate(instanceHandle.Result);
				}
			}

			using (FileStream linkCsvStream = File.Create(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Space - InteractionLink.csv")))
			{
				linkCsvStream.Write(Encoding.UTF8.GetBytes("Primary/long,long,short,short,Get/LogicType,Get/ColliderType,int,Vector3\n"));
				linkCsvStream.Write(Encoding.UTF8.GetBytes("InteractionLinkID,BaseObjectID,TriggerIndex,CallbackIndex,InteractionID,ColliderType,ColliderSize,TriggerPosition\n"));
				foreach (var linkData in interactionLinks.Values)
				{
					linkCsvStream.Write(Encoding.UTF8.GetBytes(
						                    $"{linkData.InteractionLinkID},{linkData.BaseObjectID},{linkData.TriggerIndex},{linkData.CallbackIndex},{ToCsvLogicType(linkData.InteractionID)},{ToCsvEnumName(linkData.ColliderType.ToString())},{linkData.ColliderSize},{Vector3ToDataString(linkData.TriggerPosition)}\n"));
				}
			}

			void ExtractDataFromObject(GameObject targetObject, long baseObjectId)
			{
				var triggerRoot = targetObject.transform.Find("Trigger");
				if (triggerRoot.IsReferenceNull()) return;
				
				var triggers = triggerRoot.GetComponentsInChildren<C2VEventTrigger>();
				for (short triggerIndex = 0; triggerIndex < triggers.Length; ++triggerIndex)
				{
					var trigger = triggers[triggerIndex];
					var triggerLocalPosition = targetObject.transform.InverseTransformPoint(trigger.transform.position);
					int colliderSize = Mathf.CeilToInt(trigger.Type switch
					{
						eColliderType.BOX => trigger.BoxSize.magnitude,
						_ => trigger.Radius
					});
					Com2Verse.Data.eColliderType colliderType = trigger.Type switch
					{
						eColliderType.BOX => Com2Verse.Data.eColliderType.BOX,
						_ => Com2Verse.Data.eColliderType.SPHERE
					};

					for (short callbackIndex = 0; callbackIndex < trigger.Callback.Length; callbackIndex++)
					{
						long linkIndex = long.Parse($"{baseObjectId}{triggerIndex + 1}0{callbackIndex + 1}");
						var targetCallback = trigger.Callback[callbackIndex];

						interactionLinks.Add(linkIndex, new InteractionLink()
						{
							InteractionLinkID = linkIndex,
							BaseObjectID = baseObjectId,
							TriggerIndex = (short)(triggerIndex + 1),
							CallbackIndex = (short)(callbackIndex + 1),
							ColliderSize = colliderSize,
							ColliderType = colliderType,
							InteractionID = (eLogicType)targetCallback.Function,
							TriggerPosition = triggerLocalPosition
						});
					}
				}
			}

			_isWorking = false;
		}

		public IEnumerator CsvExtractCurrentMap(string name = null)
		{
			var baseScenePath = GetSceneAssetPath(name);

			if (baseScenePath != null)
			{
				var additiveScenePath = GetSceneAssetPath($"{name}_ServerObjects");

				Scene baseScene = EditorSceneManager.OpenScene(baseScenePath, OpenSceneMode.Single);

				if (additiveScenePath != null)
				{
					EditorSceneManager.OpenScene(additiveScenePath, OpenSceneMode.Additive);
					SceneManager.SetActiveScene(baseScene);
				}

				Init();
				ExtractMap();
			}
			else
			{
				Debug.LogError($"Base Scene - {name}.unity 를 찾지 못했습니다");
				yield break;
			}
		}
		
		private void Init()
		{
			(Vector3 worldCenter, Vector3 worldSize) = BoundaryFiller.GetWorldBound();

			_worldCenter = new Vector2(worldCenter.x, worldCenter.z);
			_cellDivideNumber = new Vector2Int(Mathf.Max(1, Mathf.CeilToInt(worldSize.x / _fixedCellSize)), Mathf.Max(1, Mathf.CeilToInt(worldSize.z / _fixedCellSize)));
			_worldSize = new Vector2(_cellDivideNumber.x * _fixedCellSize, _cellDivideNumber.y * _fixedCellSize);
		}

		private string ToCsvLogicType(eLogicType logicType)
		{
			if (_maliciousEnumNameTable.TryGetValue(logicType, out string value))
			{
				return value;
			}

			return ToCsvEnumName(logicType.ToString());
		}

		private string ToCsvEnumName(string origin)
		{
			var tokens = origin.Split('_');
			StringBuilder stringBuilder = new StringBuilder();
			foreach (var token in tokens)
			{
				if (token.Length > 1)
				{
					stringBuilder.Append(string.Concat(token[0], token.Substring(1).ToLower()));
				}
			}

			return stringBuilder.ToString();
		}

		private void RemoveOriginArrangementData(long templateID)
		{
			_spaceTemplateArrangements.RemoveAll(data => data.TemplateID == templateID);
		}

		public void MakeCsv()
		{
			var resultObject = _prefabDictionary.Values.ToList();
			resultObject = _internalObjectData.Concat(resultObject).ToList();
			resultObject.Sort((l, r) => l.ID.CompareTo(r.ID));
			
			using (FileStream objectCsvStream = File.Create(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Space - BaseObject.csv")))
			{
				objectCsvStream.Write(Encoding.UTF8.GetBytes("Primary/long,#번호,Get/ObjectType,#참고,String,String,String,#참고,Short,Get/ArrangementType,String,float,float,Vector3\n"));
				objectCsvStream.Write(Encoding.UTF8.GetBytes("ID,#no,ObjectType,#오브젝트 타입,Title,Description,Name,#오브젝트 이름,StackLevel,ArrangementType,TextureRes,NavMeshRadius,NavMeshHeight,NavMeshCenter\n"));
				foreach (var objectData in resultObject)
				{
					objectCsvStream.Write(Encoding.UTF8.GetBytes(
						                      $"{objectData.ID},,{ToCsvEnumName(objectData.ObjectType.ToString())},미정의,UI_Object_Name_{objectData.ID},UI_Object_Desc_{objectData.ID},{objectData.Name},없음,{objectData.StackLevel},{ToCsvEnumName(objectData.ArrangementType.ToString())},{objectData.TextureRes},{Math.Round(objectData.NavMeshRadius, 4)},{Math.Round(objectData.NavMeshHeight, 4)},\"[{Math.Round(objectData.NavMeshCenter.x, 4)}, {Math.Round(objectData.NavMeshCenter.y, 4)}, {Math.Round(objectData.NavMeshCenter.z, 4)}]\"\n"));
				}
			}

			using (FileStream spaceTemplateCsvStream = File.Create(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Space - SpaceTemplateArrangement.csv")))
			{
				spaceTemplateCsvStream.Write(Encoding.UTF8.GetBytes("Primary/long,long,long,Vector3,Vector3,String,long\n"));
				spaceTemplateCsvStream.Write(Encoding.UTF8.GetBytes("ID,TemplateID,BaseObjectID,Position,Rotation,Neighbor,SpaceObjectID\n"));
				foreach (var spaceTemplate in _spaceTemplateArrangements)
				{
					spaceTemplateCsvStream.Write(Encoding.UTF8.GetBytes(
						                             $"{spaceTemplate.ID},{spaceTemplate.TemplateID},{spaceTemplate.BaseObjectID},{Vector3ToDataString(spaceTemplate.Position)},{Vector3ToDataString(spaceTemplate.Rotation)},{spaceTemplate.Neighbor},{spaceTemplate.SpaceObjectID}\n"));
				}
			}

			using (FileStream spaceTemplateCsvStream = File.Create(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Space - SpaceTemplate.csv")))
			{
				spaceTemplateCsvStream.Write(Encoding.UTF8.GetBytes("Primary/long,String,String,String,#참고,Get/SpaceType,Get/SpaceCode,Vector3,Vector3,string,int\n"));
				spaceTemplateCsvStream.Write(Encoding.UTF8.GetBytes("ID,SpaceTemplateName,Title,Description,#템플릿 이름,SpaceType,SpaceCode,MapBound,MapCenter,ImgRes,BuildingID\n"));
				foreach (var value in _spaceTemplates)
				{
					spaceTemplateCsvStream.Write(Encoding.UTF8.GetBytes(
						                             $"{value.ID},{value.SpaceTemplateName},UI_Template_Name_{value.ID},UI_Template_Desc_{value.ID},,{ToCsvEnumName(value.SpaceType.ToString())},{ToCsvEnumName(value.SpaceCode.ToString())},{Vector3ToDataString(value.MapBound)},{Vector3ToDataString(value.MapCenter)},{value.ImgRes},{value.BuildingID}\n"));
				}
			}

			using (FileStream interactionValueStream = File.Create(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SpaceInteractionValue.csv")))
			{
				interactionValueStream.Write(Encoding.UTF8.GetBytes("mapping_id,space_object_id,interaction_link,interaction_no,interaction_value\n"));
				foreach (var value in _objectInteractionContainers)
				{
					interactionValueStream.Write(Encoding.UTF8.GetBytes(
						                             $"{value.MappingId},{value.SpaceObjectId},{value.InteractionLink},{value.InteractionNo},{value.InteractionValue}\n"));
				}
				// NOTE: temp data(마이스 수동)
				interactionValueStream.Write(Encoding.UTF8.GetBytes("m-0001,20131001008,1143101,1,4\n"));
			}

			var worldFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "World_Interaction.csv");
			if (File.Exists(worldFilePath))
			{
				var worldValues = File.ReadAllLines(worldFilePath);
				foreach (var worldValue in worldValues)
				{
					if (worldValue.StartsWith("mapping_id,")) continue;
					var split = worldValue.Split(',');
					_objectInteractionContainers.Add(new()
					{
						MappingId = split[0],
						SpaceObjectId = long.Parse(split[1]),
						InteractionLink = long.Parse(split[2]),
						InteractionNo = int.Parse(split[3]),
						InteractionValue = split[4],
					});
				}
			}
			
			using (FileStream mergedStream = File.Create(
				       Path.Combine(string.IsNullOrEmpty(_interactionValuePath) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : _interactionValuePath, "ObjectInteractionValue.csv")))
			{
				mergedStream.Write(Encoding.UTF8.GetBytes("mapping_id,space_object_id,interaction_link,interaction_no,interaction_value\n"));
				foreach (var value in _objectInteractionContainers)
				{
					mergedStream.Write(Encoding.UTF8.GetBytes(
						                   $"{value.MappingId},{value.SpaceObjectId},{value.InteractionLink},{value.InteractionNo},{value.InteractionValue}\n"));
				}
			}
		}

		private string Vector3ToDataString(Vector3 data)
		{
			return $"\"[{data.x:F4}, {data.y:F4}, {data.z:F4}]\"";
		}

		public IEnumerator InitCsv()
		{
			Debug.Log("Start Loading Table Data");

			_objectTable?.Clear();
			_prefabDictionary.Clear();
			_spaceTemplates.Clear();
			_spaceTemplateArrangements.Clear();
			_objectInteractionContainers.Clear();

			_objectIndex = 1000;

			var objectDataHandle = Addressables.LoadAssetAsync<TextAsset>("BaseObject.bytes");
			yield return TurboLoadingHandle(objectDataHandle);

			if (objectDataHandle.Status == AsyncOperationStatus.Succeeded)
			{
				var objectData = objectDataHandle.Result;
				var objectTableTaskHandle = TableBaseObject.LoadAsync(objectData?.bytes);
				yield return objectTableTaskHandle;

				_objectTable = objectTableTaskHandle.Result.Datas;

				foreach (var template in _objectTable.Values)
				{
					var objectInfo = new BaseObject()
					{
						ArrangementType = template.ArrangementType,
						ID = template.ID,
						Name = template.Name,
						ObjectType = template.ObjectType,
						StackLevel = template.StackLevel,
						TextureRes = template.TextureRes,
						NavMeshCenter = template.NavMeshCenter,
						NavMeshHeight = template.NavMeshHeight,
						NavMeshRadius = template.NavMeshRadius
					};
					
					// skip internal data
					if (template.ID < 1000)
					{
						_internalObjectData.Add(objectInfo);
						continue;
					}

					_prefabDictionary.Add(template.Name, objectInfo);
					_objectIndex = Math.Max(_objectIndex, template.ID) + 1;
				}
			}
			else
			{
				Debug.LogError("BaseObject 기획데이터를 로딩하는데 실패했습니다!!!");
				_objectTable = new Dictionary<long, BaseObject>();
				yield break;
			}

			var spaceDataHandle = Addressables.LoadAssetAsync<TextAsset>("SpaceTemplate.bytes");
			yield return TurboLoadingHandle(spaceDataHandle);

			if (spaceDataHandle.Status == AsyncOperationStatus.Succeeded)
			{
				var spaceData = spaceDataHandle.Result;
				var spaceTableTaskHandle = TableSpaceTemplate.LoadAsync(spaceData?.bytes);
				yield return spaceTableTaskHandle;

				_spaceTemplateTable = spaceTableTaskHandle.Result.Datas;

				List<string> sceneName = new List<string>();
				foreach (var template in _spaceTemplateTable.Values)
				{
					_spaceTemplates.Add(new SpaceTemplate()
					{
						ID = template.ID,
						SpaceTemplateName = template.SpaceTemplateName,
						SpaceType = template.SpaceType,
						SpaceCode = template.SpaceCode,
						MapBound = template.MapBound,
						MapCenter = template.MapCenter,
						ImgRes = template.ImgRes,
						BuildingID = template.BuildingID
					});
					sceneName.Add(template.SpaceTemplateName);
				}
				_sceneNameArray = sceneName.ToArray();
			}
			else
			{
				Debug.LogError("SpaceTemplate 기획데이터를 로딩하는데 실패했습니다!!!");
				_spaceTemplateTable = new Dictionary<long, SpaceTemplate>();
				yield break;
			}

			var spaceArrangementDataHandle = Addressables.LoadAssetAsync<TextAsset>("SpaceTemplateArrangement.bytes");
			yield return TurboLoadingHandle(spaceArrangementDataHandle);

			if (spaceArrangementDataHandle.Status == AsyncOperationStatus.Succeeded)
			{
				var spaceArrangementData            = spaceArrangementDataHandle.Result;
				var spaceArrangementTableTaskHandle = TableSpaceTemplateArrangement.LoadAsync(spaceArrangementData?.bytes);
				yield return spaceArrangementTableTaskHandle;

				var spaceArrangementDic = spaceArrangementTableTaskHandle.Result.Datas;

				foreach (var arrangement in spaceArrangementDic.Values)
				{
					_spaceTemplateArrangements.Add(new SpaceTemplateArrangement
					{
						ID = arrangement.ID,
						TemplateID = arrangement.TemplateID,
						BaseObjectID = arrangement.BaseObjectID,
						Position = arrangement.Position,
						Rotation = arrangement.Rotation,
						Neighbor = arrangement.Neighbor,
						SpaceObjectID = arrangement.SpaceObjectID,
					});
				}
			}
			else
			{
				Debug.LogError("SpaceTemplateArrangement 기획데이터를 로딩하는데 실패했습니다!!!");
				yield break;
			}

			Debug.Log("Done");
		}
		
		private void ExtractMap()
		{
			Dictionary<int, SpaceTemplateArrangement> prefabInstanceMap = new Dictionary<int, SpaceTemplateArrangement>();

			var currentScene = SceneManager.GetActiveScene();

			SpaceTemplate matchTemplate = null;
			foreach (var existTemplate in _spaceTemplates)
			{
				if (existTemplate.SpaceTemplateName.Equals(currentScene.name))
				{
					matchTemplate = existTemplate;
					break;
				}
			}

			if (matchTemplate == null)
			{
				Debug.LogError($"{currentScene.name} : SpaceTemplate에 존재하지 않는 씬입니다.");
				return;
			}
			
			{
				matchTemplate.MapBound = _worldSize;
				matchTemplate.MapCenter = _worldCenter;
			}

			long templateID = matchTemplate.ID;

			int sceneCount = EditorSceneManager.sceneCount;
			for (int sceneIndex = 0; sceneIndex < sceneCount; sceneIndex++)
			{
				currentScene = SceneManager.GetSceneAt(sceneIndex);
				List<GameObject> rootGameObjects = new List<GameObject>(currentScene.rootCount);
				currentScene.GetRootGameObjects(rootGameObjects);

				if (currentScene.name.EndsWith("_ServerObjects"))
					RemoveOriginArrangementData(templateID);

				foreach (GameObject root in rootGameObjects)
				{
					var targetPrefabInstance = PrefabUtility.GetOutermostPrefabInstanceRoot(root);
					if (targetPrefabInstance.IsReferenceNull()) continue;
					if (targetPrefabInstance.GetComponentInChildren<Camera>()) continue;

					var targetPrefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(targetPrefabInstance);

					if (targetPrefab == null)
					{
						Debug.LogError($"{targetPrefabInstance.name} : Original Prefab이 리소스에서 삭제되었습니다! 씬에서도 삭제를..");
						continue;
					}

					var serverObject = targetPrefabInstance.GetComponent<ServerObject>();
					if (serverObject == null) continue;

					string addressableName = $"{targetPrefab.name}.prefab";
					if (!_prefabDictionary.TryGetValue(addressableName, out var objectInfo))
					{
						long baseObjectId = serverObject.ObjectTypeID;
						if (baseObjectId <= 0)
						{
							baseObjectId = _objectIndex++;
							Debug.LogWarning($"{targetPrefab.name} : 프리팝이 기획데이터에 없어서 추가합니다!");
						}

						objectInfo = new BaseObject()
						{
							ArrangementType = eArrangementType.FLOOR,
							ID = baseObjectId,
							Name = addressableName,
							ObjectType = eObjectType.OBJECT,
							StackLevel = 1,
							TextureRes = string.Empty
						};
						_prefabDictionary.Add(addressableName, objectInfo);
					}

					var triggers = root.GetComponentsInChildren<C2VEventTrigger>();
					for (short triggerIndex = 0; triggerIndex < triggers.Length; ++triggerIndex)
					{
						var trigger = triggers[triggerIndex];

						for (short callbackIndex = 0; callbackIndex < trigger.Callback.Length; callbackIndex++)
						{
							long linkIndex = long.Parse($"{objectInfo.ID}{triggerIndex + 1}0{callbackIndex + 1}");
							var targetCallback = trigger.Callback[callbackIndex];

							for (int parameterIndex = 0; parameterIndex < targetCallback.TagInternal.Count; parameterIndex++)
							{
								if (!SpaceMappingTable.ContainsKey(templateID))
								{
									Debug.LogError($"{templateID} : 공간 매핑테이블에 존재하지 않는 템플릿 아이디 입니다. 추가해주세요.");
									continue;
								}
								_objectInteractionContainers.Add(new ObjectInteractionContainer()
								{
									InteractionLink = linkIndex,
									InteractionNo = parameterIndex + 1,
									InteractionValue = targetCallback.TagInternal[parameterIndex].Value,
									MappingId = SpaceMappingTable[templateID],
									SpaceObjectId = serverObject.ObjectInstanceID
								});
							}
						}
					}

					int instanceID = targetPrefabInstance.GetInstanceID();
					if (!serverObject.IsReferenceNull() && !prefabInstanceMap.ContainsKey(instanceID))
					{
						if (serverObject.ObjectInstanceID <= 0)
						{
							Debug.LogError($"{currentScene.name} 씬 : {objectInfo.Name}의 SpaceObjectId가 잘못되어서 스킵합니다. (수동으로 씬을 열어 id를 할당하세요)");
							continue;
						}
					
						long pk = templateID * 10000000 + prefabInstanceMap.Count;
						long baseObjectId = objectInfo.ID;
						var position = targetPrefabInstance.transform.position;
						var rotation = targetPrefabInstance.transform.rotation;

						prefabInstanceMap.Add(instanceID, new SpaceTemplateArrangement()
						{
							ID = pk,
							Neighbor = string.Empty,
							BaseObjectID = baseObjectId,
							Position = position,
							Rotation = rotation.eulerAngles,
							TemplateID = templateID,
							SpaceObjectID = serverObject.ObjectInstanceID
						});
					}
				}

				foreach (var spaceTemplateArrangement in prefabInstanceMap.Values)
				{
					_spaceTemplateArrangements.Add(spaceTemplateArrangement);
				}
			}
		}
	}
}
