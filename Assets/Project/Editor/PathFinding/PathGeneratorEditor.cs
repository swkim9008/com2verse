// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	PathGeneratorEditor.cs
//  * Developer:	yangsehoon
//  * Date:		2023-04-11 오후 4:58
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
using Com2Verse.Pathfinder;
using Com2VerseEditor.PhysicsSerializer;
using Pathfinding;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Com2VerseEditor
{
	[CustomEditor(typeof(PathGenerator))]
	public class PathGeneratorEditor : Editor
	{
		public struct NavmeshCutInfo
		{
			public float Radius;
			public float Height;
			public Vector3 Center;
		}
		
		private static string _dataPath;
		private static string _studioScenePath;

		private readonly string DEFAULT_OPTION_SETTINGS      = "DefaultOptionSettings"; 
		private readonly string CHARACTER_RADIUS     = "CharacterRadius";
		private readonly string WALKABLE_HEIGHT     = "WalkableHeight";
		private readonly string WALKABLE_CLIMB      = "WalkableClimb";
		private readonly string MAX_SLOPE = "MaxSlope";
		private readonly string MAX_BORDER_EDGE_LENGTH = "MaxBorderEdgeLength";
		private readonly string MAX_EDGE_ERROR = "MaxEdgeError";
		private readonly string CELL_SIZE           = "CellSize";
		private readonly string MIN_REGION_SIZE     = "MinRegionSize";
		private readonly string RASTERIZE_TERRAIN   = "RasterizeTerrain";
		private readonly string RASTERIZE_COLLIDERS = "RasterizeColliders";
		private readonly string RASTERIZE_MESHES    = "RasterizeMeshes";

		private SerializedProperty _pathGenerateOptions;
		private SerializedProperty _optionValues;
		private SerializedProperty _optionKeys;
		private int _defaultOptionIndex;
		private int _optionTarget;
		private string[] _sceneNameArray;
		private bool _forceScan = false;
		private static Dictionary<long, SpaceTemplate> _spaceTemplates = null;
		private static Dictionary<long, BaseObject> _baseObjects = null;
		private static bool _isLoading = false;
		private static bool _isWorking = false;

		private static int _totalProgress;
		private static int _currentProgress;

		private static Pathfinding.Serialization.SerializeSettings _serializationSettings = new Pathfinding.Serialization.SerializeSettings()
		{
			nodes = true
		};
		private Vector3 _ceilingBound = new Vector3(0, 0.1f, 0);

		[MenuItem("Com2Verse/PathFinding/Generate NavMesh")]
		private static void SelectGenerator()
		{
			var thisObject = AssetDatabase.LoadAssetAtPath<Object>("Assets/Project/Editor/PathFinding/PathGenerator.asset");
			Selection.activeObject = thisObject;
			EditorGUIUtility.PingObject(thisObject);
		}
		
		public static NavmeshCutInfo GetNavmeshCut(GameObject root)
		{
			var colliders = root.transform.Find("Collider");
			if (colliders.IsReferenceNull()) return new NavmeshCutInfo();
			
			Vector3 boundsMin = Vector3.positiveInfinity;
			Vector3 boundsMax = Vector3.negativeInfinity;
			bool foundCollider = false;
			foreach (Transform colliderContainer in colliders)
			{
				var anyCollider = colliderContainer.GetComponentsInChildren<Collider>();
				foreach (var collider in anyCollider)
				{
					boundsMin = Vector3.Min(collider.bounds.min, boundsMin);
					boundsMax = Vector3.Max(collider.bounds.max, boundsMax);
					foundCollider = true;
				}
			}

			if (foundCollider)
			{
				Bounds newBound = new Bounds();
				newBound.SetMinMax(boundsMin, boundsMax);
				return new NavmeshCutInfo()
				{
					Center = newBound.center,
					Height = newBound.extents.y * 2,
					Radius = Mathf.Sqrt(newBound.extents.x * newBound.extents.x + newBound.extents.z * newBound.extents.z) / Mathf.Sin((ClientPathFinding.NavmeshCutResolution - 2) * Mathf.PI / 2 / ClientPathFinding.NavmeshCutResolution)
				};
			}
			else
			{
				return new NavmeshCutInfo();
			}
		}
		
		private void OnEnable()
		{
			SetDataLocation();
			_pathGenerateOptions = serializedObject.FindProperty("_pathGenerateOptions");
			_optionValues = _pathGenerateOptions.FindPropertyRelative("Values");
			_optionKeys = _pathGenerateOptions.FindPropertyRelative("Keys");

			var pathGenerator = target as PathGenerator;
			CreateDefaultOption(pathGenerator);
			_defaultOptionIndex = GetDefaultOptionIndex();
			_isLoading = false;
		}

		private static void SetDataLocation()
		{
			_dataPath = $"{Application.dataPath}/Project/Editor/PathFinding/Data~/";
			_studioScenePath = $"{Application.dataPath}/Project/Editor/PathFinding/ScanStudio.unity";
		}

		private IEnumerator LoadTableData()
		{
			_isLoading = true;
			var spaceDataHandle = Addressables.LoadAssetAsync<TextAsset>("SpaceTemplate.bytes");
			yield return WholeSerializer.TurboLoadingHandle(spaceDataHandle);

			if (spaceDataHandle.Status == AsyncOperationStatus.Succeeded)
			{
				var spaceData = spaceDataHandle.Result;
				var spaceTableTaskHandle = TableSpaceTemplate.LoadAsync(spaceData?.bytes);
				yield return spaceTableTaskHandle;

				_spaceTemplates = spaceTableTaskHandle.Result.Datas;
			}
			else
			{
				Debug.LogError("SpaceTemplate 기획데이터를 로딩하는데 실패했습니다!!!");
				_spaceTemplates = new Dictionary<long, SpaceTemplate>();
			}
			
			var objectDataHandle = Addressables.LoadAssetAsync<TextAsset>("BaseObject.bytes");
			yield return WholeSerializer.TurboLoadingHandle(objectDataHandle);

			if (objectDataHandle.Status == AsyncOperationStatus.Succeeded)
			{
				var objectData = objectDataHandle.Result;
				var objectTableTaskHandle = TableBaseObject.LoadAsync(objectData?.bytes);
				yield return objectTableTaskHandle;

				_baseObjects = objectTableTaskHandle.Result.Datas;
			}
			else
			{
				Debug.LogError("BaseObject 기획데이터를 로딩하는데 실패했습니다!!!");
				_baseObjects = new Dictionary<long, BaseObject>();
			}

			UpdateSceneSelector();
			_isLoading = false;
		}

		private void CreateDefaultOption(PathGenerator pathGenerator)
		{
			if (!pathGenerator.PathGenerateOptions.Keys.Contains(DEFAULT_OPTION_SETTINGS))
			{
				int index = _optionKeys.arraySize;
				_optionKeys.InsertArrayElementAtIndex(index);
				_optionValues.InsertArrayElementAtIndex(index);

				_optionKeys.GetArrayElementAtIndex(index).stringValue = DEFAULT_OPTION_SETTINGS;
				var valueProperty   = _optionValues.GetArrayElementAtIndex(index);
				var newOptionValues = new PathGenerateOption();
				valueProperty.FindPropertyRelative(CHARACTER_RADIUS).floatValue    = newOptionValues.CharacterRadius;
				valueProperty.FindPropertyRelative(WALKABLE_HEIGHT).floatValue    = newOptionValues.WalkableHeight;
				valueProperty.FindPropertyRelative(WALKABLE_CLIMB).floatValue     = newOptionValues.WalkableClimb;
				valueProperty.FindPropertyRelative(MAX_SLOPE).floatValue = newOptionValues.MaxSlope;
				valueProperty.FindPropertyRelative(MAX_BORDER_EDGE_LENGTH).floatValue = newOptionValues.MaxBorderEdgeLength;
				valueProperty.FindPropertyRelative(MAX_EDGE_ERROR).floatValue = newOptionValues.MaxEdgeError;
				valueProperty.FindPropertyRelative(CELL_SIZE).floatValue          = newOptionValues.CellSize;
				valueProperty.FindPropertyRelative(MIN_REGION_SIZE).floatValue    = newOptionValues.MinRegionSize;
				valueProperty.FindPropertyRelative(RASTERIZE_TERRAIN).boolValue   = newOptionValues.RasterizeTerrain;
				valueProperty.FindPropertyRelative(RASTERIZE_COLLIDERS).boolValue = newOptionValues.RasterizeColliders;
				valueProperty.FindPropertyRelative(RASTERIZE_MESHES).boolValue    = newOptionValues.RasterizeMeshes;

				serializedObject.ApplyModifiedProperties();
			}
		}

		private int GetDefaultOptionIndex()
		{
			var ret = -1;

			for (int i = 0; i < _optionKeys.arraySize; i++)
			{
				if (_optionKeys.GetArrayElementAtIndex(i).stringValue.Equals(DEFAULT_OPTION_SETTINGS))
				{
					ret = i;
					break;
				}
			}

			return ret;
		}

		private void DrawOptionCreate(PathGenerator pathGenerator)
		{
			if (_sceneNameArray == null) return;
			
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			_optionTarget = EditorGUILayout.Popup("Create Option", System.Math.Min(_optionTarget, _sceneNameArray.Length - 1), _sceneNameArray);
			if (GUILayout.Button("Add Option"))
			{
				string targetSceneName = _sceneNameArray[_optionTarget];

				if (!pathGenerator.PathGenerateOptions.Keys.Contains(targetSceneName))
				{
					_pathGenerateOptions.isExpanded = true;
					int index = _optionKeys.arraySize;
					_optionKeys.InsertArrayElementAtIndex(index);
					_optionValues.InsertArrayElementAtIndex(index);

					_optionKeys.GetArrayElementAtIndex(index).stringValue = targetSceneName;
					var valueProperty = _optionValues.GetArrayElementAtIndex(index);
					var newOptionValues = new PathGenerateOption();
					valueProperty.FindPropertyRelative(CHARACTER_RADIUS).floatValue    = newOptionValues.CharacterRadius;
					valueProperty.FindPropertyRelative(WALKABLE_HEIGHT).floatValue    = newOptionValues.WalkableHeight;
					valueProperty.FindPropertyRelative(WALKABLE_CLIMB).floatValue     = newOptionValues.WalkableClimb;
					valueProperty.FindPropertyRelative(MAX_SLOPE).floatValue          = newOptionValues.MaxSlope;
					valueProperty.FindPropertyRelative(MAX_BORDER_EDGE_LENGTH).floatValue = newOptionValues.MaxBorderEdgeLength;
					valueProperty.FindPropertyRelative(MAX_EDGE_ERROR).floatValue = newOptionValues.MaxEdgeError;
					valueProperty.FindPropertyRelative(CELL_SIZE).floatValue          = newOptionValues.CellSize;
					valueProperty.FindPropertyRelative(MIN_REGION_SIZE).floatValue    = newOptionValues.MinRegionSize;
					valueProperty.FindPropertyRelative(RASTERIZE_TERRAIN).boolValue   = newOptionValues.RasterizeTerrain;
					valueProperty.FindPropertyRelative(RASTERIZE_COLLIDERS).boolValue = newOptionValues.RasterizeColliders;
					valueProperty.FindPropertyRelative(RASTERIZE_MESHES).boolValue    = newOptionValues.RasterizeMeshes;
				}
			}

			EditorGUILayout.EndVertical();
		}

		private void DrawOptionList()
		{
			_pathGenerateOptions.isExpanded = EditorGUILayout.Foldout(_pathGenerateOptions.isExpanded, "Generate Option");
			if (_pathGenerateOptions.isExpanded)
			{
				EditorGUILayout.HelpBox("생성 옵션이 없는 씬은 기본 설정으로 스캔합니다.", MessageType.Info);
				if(_defaultOptionIndex == -1)
					EditorGUILayout.HelpBox("기본 옵션 에러가 발생했습니다.", MessageType.Error);
				EditorGUI.indentLevel++;
				EditorGUILayout.BeginVertical();
				int removeIndex = -1;

				if (_defaultOptionIndex != -1)
				{
					var id = _optionKeys.GetArrayElementAtIndex(_defaultOptionIndex).stringValue;
					var defaultOptionValueProperty = _optionValues.GetArrayElementAtIndex(_defaultOptionIndex);
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PropertyField(defaultOptionValueProperty, new GUIContent(id));
					EditorGUILayout.EndHorizontal();

					var characterRadius = defaultOptionValueProperty.FindPropertyRelative(CHARACTER_RADIUS).floatValue;
					var cellSize        = defaultOptionValueProperty.FindPropertyRelative(CELL_SIZE).floatValue;

					if (characterRadius < cellSize * 2)
					{
						EditorGUILayout.HelpBox(
							"For best navmesh quality, it is recommended to keep the character radius at least 2 times as large as the cell size. Smaller cell sizes will give you higher quality navmeshes, but it will take more time to scan the graph.",
							MessageType.Warning);
					}
				}

				for (int i = 0; i < _optionKeys.arraySize; i++)
				{
					if(_defaultOptionIndex == i)
						continue; 

					var id = _optionKeys.GetArrayElementAtIndex(i).stringValue;
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PropertyField(_optionValues.GetArrayElementAtIndex(i), new GUIContent(id));
					if (GUILayout.Button("Remove"))
					{
						removeIndex = i;
					}

					EditorGUILayout.EndHorizontal();
				}

				if (removeIndex >= 0)
				{
					_optionKeys.DeleteArrayElementAtIndex(removeIndex);
					_optionValues.DeleteArrayElementAtIndex(removeIndex);
					_defaultOptionIndex = GetDefaultOptionIndex();
				}

				EditorGUILayout.EndVertical();
				EditorGUI.indentLevel--;
			}
		}

		private void ShowGenerated()
		{
			if (GUILayout.Button("저장된 폴더 열기"))
			{
				Application.OpenURL($"file://{_dataPath}");
			}
		}

		private void DrawGenerate(PathGenerator pathGenerator)
		{
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			_forceScan = EditorGUILayout.Toggle("이미 스캔된 것도 다시 스캔", _forceScan);
			
			if (GUILayout.Button("스캔"))
			{
				EditorSceneManager.OpenScene(_studioScenePath);

				InitializeAstar();

				EditorCoroutineUtility.StartCoroutineOwnerless(DoScan(pathGenerator));
			}
			
			EditorGUILayout.EndVertical();
		}

		public static void InitializeAstar()
		{
			// Initialize Astar
			AstarPath.active.data.FindGraphTypes();
			GraphModifier.FindAllModifiers();
			AstarPath.active.data.OnDestroy();
		}
		
		private IEnumerator DoScan(PathGenerator pathGenerator)
		{
			_isWorking = true;
			
			while (_spaceTemplates == null) yield return null;

			PathGenerateOption option = null;

			_currentProgress = 0;
			_totalProgress = _spaceTemplates.Count;
			foreach (var template in _spaceTemplates.Values)
			{
				long id = template.ID;

				string fileName = $"{id}_navmesh.bytes";
				if (!_forceScan && File.Exists($"{_dataPath}{fileName}"))
				{
					_currentProgress++;
					Repaint();
					continue;
				}

				Debug.Log($"스캔 작업 : {template.SpaceTemplateName}");
				string sceneName = WholeSerializer.GetSceneAssetPath(template.SpaceTemplateName);
				if (sceneName == null)
				{
					Debug.LogError("존재하지 않는 씬이라서 건너 뜁니다.");
					continue;
				}
				
				Scene opened = EditorSceneManager.OpenScene(sceneName, OpenSceneMode.Additive);
				option = pathGenerator.PathGenerateOptions.ContainsKey(template.SpaceTemplateName) ? pathGenerator.PathGenerateOptions[template.SpaceTemplateName] : pathGenerator.PathGenerateOptions[DEFAULT_OPTION_SETTINGS];
				Scan(fileName, option, _ceilingBound);

				EditorSceneManager.CloseScene(opened, true);
				yield return null;
				_currentProgress++;
				Repaint();
			}

			_isWorking = false;
		}

		public static void Scan(string fileName, PathGenerateOption option, Vector3 ceilingBound)
		{
			var graph = CreateRecastGraph(option);

			graph.SnapForceBoundsToScene();

			graph.forcedBoundsCenter -= ceilingBound; // remove ceiling
			AstarPath.active.Scan();
			graph.forcedBoundsCenter += ceilingBound; // recover offset

			var bytes = AstarPath.active.data.SerializeGraphs(_serializationSettings);
			if (string.IsNullOrEmpty(_dataPath)) SetDataLocation();
			File.WriteAllBytes($"{_dataPath}{fileName}", bytes);
			AstarPath.active.data.RemoveGraph(graph);
		}

		public static RecastGraph CreateRecastGraph(PathGenerateOption option)
		{
			RecastGraph newGraph = AstarPath.active.data.AddGraph(typeof(RecastGraph)) as RecastGraph;

			newGraph.editorTileSize = 32;
			newGraph.characterRadius = option.CharacterRadius;
			newGraph.walkableHeight = option.WalkableHeight;
			newGraph.walkableClimb = option.WalkableClimb;
			newGraph.maxSlope = option.MaxSlope;
			newGraph.maxEdgeLength = option.MaxBorderEdgeLength;
			newGraph.contourMaxError = option.MaxEdgeError;

			newGraph.rasterizeTerrain = option.RasterizeTerrain;
			newGraph.rasterizeColliders = option.RasterizeColliders;
			newGraph.rasterizeMeshes = option.RasterizeMeshes;

			newGraph.cellSize = option.CellSize;
			newGraph.minRegionSize = option.MinRegionSize;

			return newGraph;
		}

		private void UpdateSceneSelector()
		{
			List<string> sceneName = new List<string>();
			foreach (var template in _spaceTemplates.Values)
			{
				sceneName.Add(template.SpaceTemplateName);
			}

			_sceneNameArray = sceneName.ToArray();
		}

		private void ShowProgress()
		{
			if (_isWorking)
			{
				EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), (float)_currentProgress / Math.Max(1, _totalProgress), "Progress");
			}
		}

		public override void OnInspectorGUI()
		{
			var pathGenerator = target as PathGenerator;
			serializedObject.Update();

			if (_spaceTemplates == null)
			{
				if (!_isLoading && GUILayout.Button("Load Table Data"))
				{
					EditorCoroutineUtility.StartCoroutineOwnerless(LoadTableData());
				}

				if (_isLoading)
				{
					EditorGUILayout.HelpBox("Loading Table Data..", MessageType.Info);
				}
				return;
			}

			DrawOptionCreate(pathGenerator);
			DrawOptionList();
			DrawGenerate(pathGenerator);
			EditorGUILayout.Separator();
			ShowGenerated();
			ShowProgress();

			if (serializedObject.hasModifiedProperties)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}