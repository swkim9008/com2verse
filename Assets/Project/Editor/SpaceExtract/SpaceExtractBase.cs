/*===============================================================
* Product:		Com2Verse
* File Name:	OfficeSpaceExport.cs
* Developer:	jhkim
* Date:			2023-05-12 18:29
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.PhysicsAssetSerialization;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using eColliderType = Com2Verse.Data.eColliderType;

namespace Com2VerseEditor.SpaceExtract
{
	public abstract class SpaceExtractBase<TSpaceTemplateType, TObjectInteractionType>
	{
		private static readonly string ServerObjectSuffix = "_ServerObjects";

		private static class Constant
		{
			public static class CsvHeader
			{
				public static readonly string ServerObject = "scene_name,object_name,trigger_object_name,base_object_id,space_object_id,interaction_count,logic_type";
				public static readonly string EventTrigger = "scene_name,object_name,base_object_id,space_object_id,interaction_no,logic_type,key,value,trigger_id,server_check";
				public static readonly string BuildingDetail = "building_id,floor_no,location,space_id";
				public static readonly string Space = "space_id,account_id,space_no,space_name,space_description,parent_template_id";
				public static readonly string SpaceDetail = "space_id,building_type,building_code,space_type,space_code";
				public static readonly string ObjectInteraction = "mapping_id,space_object_id,interaction_link_id,interaction_no,interaction_value,comment";
			}

			public static class Name
			{
				public static readonly string SceneServerObject = "SceneServerObject";
				public static readonly string EventTrigger = "EventTrigger";
				public static readonly string BuildingDetail = "BuildingDetail";
				public static readonly string Space = "Space";
				public static readonly string SpaceDetail = "SpaceDetail";
				public static readonly string ObjectInteraction = "ObjectInteraction";
				public static readonly string Csv = "csv";
			}
		}

		protected SpaceExtractBase()
		{
			SetAllLogicTypeCandidates();
		}
#region Protected
		/* SpaceInfos
		 *	- 공간 ID별 로 맵핑된 공간 템플릿 목록
		 *
		 * SpaceInteractionMap
		 *	- 공간 템플릿 내의 상호작용 오브젝트 정보 맵핑 테이블
		 *
		 * ObjectInteractionMap
		 *	- 상호작용 오브젝트 타입 - 씬 내의 게임오브젝트 이름 맵핑 테이블
		 *    <상호작용 타입, KeyValuePair<오브젝트 이름, 트리거 오브젝트 이름>>
		 *
		 * ObjectInteractionValueMap
		 *	- 특정 공간 내 상호작용 오브젝트에 들어있는 상호작용 Value (현재는 1개만 등록 가능)
		 *
		 * CandidateLogicType
		 *  - 데이터 추출 대상 로직 타입(들)
		 *
		 * GetSceneName
		 *  - 공간 템플릿의 실제 씬 이름 가져오기
		 */
		protected virtual IEnumerable<SpaceInfo<TSpaceTemplateType>> SpaceInfos => throw new NotImplementedException();
		protected virtual Dictionary<TSpaceTemplateType, TObjectInteractionType[]> SpaceInteractionMap => throw new NotImplementedException();
		protected virtual Dictionary<TObjectInteractionType, KeyValuePair<string, string>> ObjectInteractionMap => throw new NotImplementedException();
		protected virtual Dictionary<(string, TObjectInteractionType), string> ObjectInteractionValueMap => throw new NotImplementedException();
		protected virtual IEnumerable<eLogicType> CandidateLogicType => LogicTypes;

		protected virtual string GetTemplateSceneName(TSpaceTemplateType type) => throw new NotImplementedException();
		protected List<eLogicType> LogicTypes = new();
#endregion // Protected

#region Extract
		protected async UniTask ExtractServerObjectTableAsync(IEnumerable<SceneServerObjectInfo> sceneServerObjects, string savePath = "", string customName = "")
		{
			var sb = new StringBuilder();
			sb.AppendLine(Constant.CsvHeader.ServerObject);
			foreach (var sso in sceneServerObjects)
			{
				foreach (var serverObject in sso.ServerObjects)
				{
					foreach (var triggerObject in serverObject.TriggerObjects)
					{
						sb.AppendLine($"{sso.SceneName},{serverObject.Name},{triggerObject.Name},{serverObject.BaseObjectId},{serverObject.SpaceObjectId},{triggerObject.Tags.Length},{triggerObject.LogicType}");
					}
				}
			}

			var name = string.IsNullOrWhiteSpace(customName) ? Constant.Name.SceneServerObject : customName;
			await SaveOrOverwrite(sb.ToString(), name, Constant.Name.Csv, savePath);
		}

		protected async UniTask ExtractServerObjectEventTriggerTableAsync(IEnumerable<SceneServerObjectInfo> sceneServerObjects, string savePath = "", string customName = "")
		{
			var sb = new StringBuilder();
			sb.AppendLine(Constant.CsvHeader.EventTrigger);
			foreach (var sso in sceneServerObjects)
			{
				foreach (var serverObject in sso.ServerObjects)
				{
					foreach (var triggerObject in serverObject.TriggerObjects)
					{
						for (var i = 0; i < triggerObject.Tags.Length; i++)
						{
							var tag = triggerObject.Tags[i];
							sb.AppendLine($"{sso.SceneName},{serverObject.Name},{serverObject.BaseObjectId},{serverObject.SpaceObjectId},{i + 1},{triggerObject.LogicType},{tag.Key},{tag.Value},{triggerObject.TriggerId},{triggerObject.ServerCheck}");
						}
					}
					// for (var i = 0; i < serverObject.TriggerObjects.Tags.Length; i++)
					// {
					// 	var tag = serverObject.Tags[i];
					// 	sb.AppendLine($"{sso.SceneName},{serverObject.Name},{serverObject.BaseObjectId},{serverObject.SpaceObjectId},{i + 1},{serverObject.LogicType},{tag.Key},{tag.Value},{serverObject.TriggerId},{serverObject.ServerCheck}");
					// }
				}
			}

			var name = string.IsNullOrWhiteSpace(customName) ? Constant.Name.EventTrigger : customName;
			await SaveOrOverwrite(sb.ToString(), name, Constant.Name.Csv, savePath);
		}

		private async UniTask ExtractBuildingDetailTableAsync(string savePath = "")
		{
			var sb = new StringBuilder();
			sb.AppendLine(Constant.CsvHeader.BuildingDetail);

			foreach (var spaceInfo in SpaceInfos)
			{
				if (spaceInfo.BuildingTableInfo == null) continue;

				var buildingInfo = spaceInfo.BuildingTableInfo.Value;
				sb.AppendLine($"{buildingInfo.BuildingId},{buildingInfo.FloorNo},{buildingInfo.Location},{spaceInfo.SpaceId}");
			}

			await SaveOrOverwrite(sb.ToString(), Constant.Name.BuildingDetail, Constant.Name.Csv, savePath);
		}

		private async UniTask ExtractSpaceTableAsync(string savePath = "")
		{
			var sb = new StringBuilder();
			sb.AppendLine(Constant.CsvHeader.Space);

			foreach (var spaceInfo in SpaceInfos)
			{
				if (spaceInfo.SpaceTableInfo == null) continue;

				var space = spaceInfo.SpaceTableInfo.Value;
				sb.AppendLine($"{spaceInfo.SpaceId},{space.AccountId},{space.No},{space.Name},{space.Description},{space.TemplateId}");
			}

			await SaveOrOverwrite(sb.ToString(), Constant.Name.Space, Constant.Name.Csv, savePath);
		}

		private async UniTask ExtractSpaceDetailTableAsync(string savePath = "")
		{
			var sb = new StringBuilder();
			sb.AppendLine(Constant.CsvHeader.SpaceDetail);

			foreach (var spaceInfo in SpaceInfos)
			{
				if (spaceInfo.SpaceDetailTableInfo == null) continue;

				var spaceDetail = spaceInfo.SpaceDetailTableInfo.Value;
				sb.AppendLine($"{spaceInfo.SpaceId},{spaceDetail.BuildingType},{spaceDetail.BuildingCode},{spaceDetail.SpaceType},{spaceDetail.SpaceCode}");
			}

			await SaveOrOverwrite(sb.ToString(), Constant.Name.SpaceDetail, Constant.Name.Csv, savePath);
		}
		protected async UniTask ExtractObjectInteractionTableAsync(IEnumerable<SceneServerObjectInfo> sceneServerObjects, string savePath = "")
		{
			var sb = new StringBuilder();
			sb.AppendLine(Constant.CsvHeader.ObjectInteraction);

			var interactionTable = new List<ObjectInteractionRow>();

			var tableInteractionLink = await LoadTableAsync<TableInteractionLink>("InteractionLink.bytes");
			if (tableInteractionLink == null)
				return;

			var interactionLinksMap = tableInteractionLink.Datas;

			// 공간별로 순회
			foreach (var spaceInfo in SpaceInfos)
			{
				// 현재 공간에 맞는 씬 정보 찾기
				var sceneName = GetTemplateSceneName(spaceInfo.Type);
				var ssoi = FindSceneServerObject(sceneName);
				if (ssoi == null)
				{
					C2VDebug.LogWarning($"SceneServerObject not found... {sceneName}");
					continue;
				}

				// 씬에 속한 서버 오브젝트 조사
				foreach (var sso in ssoi.ServerObjects)
				{
					var interactionValue = string.Empty;
					if (!TryGetInteractionType(spaceInfo.Type, sso.Name, out var interactionType))
						continue;

					interactionValue = GetInteractionValue(spaceInfo.SpaceId, interactionType);

					if (string.IsNullOrWhiteSpace(interactionValue)) continue;

					foreach (var triggerObjectInfo in sso.TriggerObjects)
					{
						var id = interactionLinksMap.Where(item =>
							                                   item.Value.BaseObjectID == sso.BaseObjectId &&
							                                   item.Value.TriggerIndex == triggerObjectInfo.TriggerId &&
							                                   item.Value.CallbackIndex == triggerObjectInfo.CallbackIdx)
						                            .Select(item => item.Key).FirstOrDefault();
						if (id == default)
						{
							C2VDebug.LogWarning($"Cannot find ID = {sso.BaseObjectId} {triggerObjectInfo.TriggerId} {triggerObjectInfo.CallbackIdx} | {triggerObjectInfo.Name}, {triggerObjectInfo.LogicType}");
							continue;
						}

						var objectInteractionRow = new ObjectInteractionRow
						{
							MappingId = spaceInfo.SpaceId,
							SpaceObjectId = sso.SpaceObjectId,
							InteractionNo = triggerObjectInfo.CallbackIdx,
							// InteractionLink = 1,
							InteractionLinkId = id,
							InteractionValue = interactionValue,
							ObjectName = sso.Name,
							SceneName = sceneName.Replace(ServerObjectSuffix, string.Empty),
						};
						interactionTable.Add(objectInteractionRow);
					}
				}
			}

			foreach (var row in interactionTable)
				sb.AppendLine($"{row.MappingId},{row.SpaceObjectId},{row.InteractionLinkId},{row.InteractionNo},{row.InteractionValue},#{row.SceneName}@{row.ObjectName}");

			await SaveOrOverwrite(sb.ToString(), Constant.Name.ObjectInteraction, Constant.Name.Csv, savePath);

			SceneServerObjectInfo FindSceneServerObject(string sceneName) => sceneServerObjects.FirstOrDefault(sso => sso.SceneName == sceneName);

			bool TryGetInteractionType(TSpaceTemplateType spaceType, string objectName, out TObjectInteractionType interactionType)
			{
				interactionType = default;
				var interactionTypes = SpaceInteractionMap[spaceType];

				foreach (var type in interactionTypes)
				{
					if (ObjectInteractionMap.TryGetValue(type, out var kvp))
					{
						if (kvp.Key.ToLower().Equals(objectName.ToLower()))
						{
							interactionType = type;
							return true;
						}
					}
				}
				return false;
			}
		}

		protected void PrintSpaceInteractionMapStr()
		{
			var sb = new StringBuilder();
			foreach (var spaceInfo in SpaceInfos)
			{
				var spaceId = spaceInfo.SpaceId;
				var interactions = SpaceInteractionMap[spaceInfo.Type];
				sb.AppendLine($"// {spaceInfo.Type.ToString()}");
				foreach (var interactionType in interactions)
					sb.AppendLine($@"{{(""{spaceId}"", {interactionType.GetType().Name}.{interactionType}), """"}},");
				sb.AppendLine();
			}

			var result = sb.ToString();
			C2VDebug.Log(result);
			GUIUtility.systemCopyBuffer = result;

			EditorUtility.DisplayDialog("완료", "클립보드로 복사 되었습니다.", "확인");
		}

		protected async UniTask<SceneServerObjectInfo[]> GetSceneServerObjectsInActiveSceneAsync()
		{
			var scenePath = SceneManager.GetActiveScene().path;
			var result = await GetSceneServerObjectsAsync(scenePath);
			return result;
		}

		protected async UniTask<SceneServerObjectInfo[]> GetSceneServerObjectsInSelectionAsync()
		{
			var scenePaths = Selection.objects.OfType<SceneAsset>().Select(AssetDatabase.GetAssetPath).ToArray();
			var result = await GetSceneServerObjectsAsync(scenePaths);
			return result;
		}

		protected async UniTask<SceneServerObjectInfo[]> GetSceneServerObjectsInAllSceneAsync()
		{
			var scenePaths = GetAllScenePaths();
			var result = await GetSceneServerObjectsAsync(scenePaths);
			return result;
		}

		protected async UniTask ExtractAllAsync(string title, SceneServerObjectInfo[] serverObjects = null)
		{
			serverObjects ??= await GetSceneServerObjectsInAllSceneAsync();
			var savePath = EditorUtility.SaveFolderPanel("저장 폴더 선택", string.Empty, string.Empty);
			var tasks = new ProgressTask.ProgressTaskInfo[]
			{
				new ProgressTask.ProgressTaskInfo {Title = title, Info = "서버 오브젝트 목록 추출", Job = JobExtractServerObjectTableAsync},
				new ProgressTask.ProgressTaskInfo {Title = title, Info = "이벤트 트리거 목록 추출", Job = JobExtractServerObjectEventTriggerTableAsync},
				new ProgressTask.ProgressTaskInfo {Title = title, Info = "건물 상세정보 테이블 추출", Job = JobExtractBuildingDetailTableAsync},
				new ProgressTask.ProgressTaskInfo {Title = title, Info = "공간 정보 테이블 추출", Job = JobExtractSpaceTableAsync},
				new ProgressTask.ProgressTaskInfo {Title = title, Info = "공간 상세정보 테이블 추출", Job = JobExtractSpaceDetailTableAsync},
				new ProgressTask.ProgressTaskInfo {Title = title, Info = "오브젝트 상호작용 테이블 추출", Job = JobExportObjectInteractionTableAsync},
			};
			await ProgressTask.ExecuteAsync(tasks);

			async UniTask JobExtractServerObjectTableAsync() => await ExtractServerObjectTableAsync(serverObjects, savePath);
			async UniTask JobExtractServerObjectEventTriggerTableAsync() => await ExtractServerObjectEventTriggerTableAsync(serverObjects, savePath);
			async UniTask JobExtractBuildingDetailTableAsync() => await ExtractBuildingDetailTableAsync(savePath);
			async UniTask JobExtractSpaceTableAsync() => await ExtractSpaceTableAsync(savePath);
			async UniTask JobExtractSpaceDetailTableAsync() => await ExtractSpaceDetailTableAsync(savePath);
			async UniTask JobExportObjectInteractionTableAsync() => await ExtractObjectInteractionTableAsync(serverObjects, savePath);
		}
#endregion // Extract

#region Private Methods
		protected async UniTask<SceneServerObjectInfo[]> GetSceneServerObjectsAsync(params string[] scenePaths)
		{
			if (scenePaths == null) return Array.Empty<SceneServerObjectInfo>();

			var result = new List<SceneServerObjectInfo>();
			var items = new List<ServerObjectInfo>();

			foreach (var scenePath in scenePaths)
			{
				if (!IsServerObjectScene(scenePath)) continue;

				EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
				await ValidateIdsAsync();

				var serverObjects = GameObject.FindObjectsOfType<ServerObject>();
				items.Clear();

				foreach (var serverObject in serverObjects)
				{
					var eventTriggers = serverObject.gameObject.GetComponentsInChildren<C2VEventTrigger>();
					if (eventTriggers == null || eventTriggers.Length == 0) continue;

					var triggerObjects = new List<TriggerObjectInfo>();
					for (var triggerIdx = 0; triggerIdx < eventTriggers.Length; triggerIdx++)
					{
						var eventTrigger = eventTriggers[triggerIdx];
						if (eventTrigger.Callback.Length == 0)
						{
							C2VDebug.LogWarning($"Callback이 지정되지 않았습니다. {Path.GetFileNameWithoutExtension(scenePath)} = {serverObject.name} : {eventTrigger.name}");
							continue;
						}

						for (var callbackIdx = 0; callbackIdx < eventTrigger.Callback.Length; callbackIdx++)
						{
							var enumValue = eventTrigger.Callback[callbackIdx].Function;
							if (enumValue < 0) continue;

							var logicType = UnsafeUtility.As<long, eLogicType>(ref enumValue);
							if (!CandidateLogicType.Contains(logicType)) continue;

							var tags = Array.Empty<KeyValue>();
							if (eventTrigger.callbacks.TagInternal != null)
								tags = eventTrigger.callbacks.TagInternal.ToArray();

							var triggerObject = new TriggerObjectInfo
							{
								Name = eventTrigger.name,
								CallbackIdx = callbackIdx + 1,
								LogicType = logicType,
								Tags = tags,
								TriggerId = triggerIdx + 1,
								ServerCheck = eServerCheck.ONLY_CLIENT,
							};
							triggerObjects.Add(triggerObject);
						}
					}

					var serverObjectInfo = new ServerObjectInfo
					{
						Name = serverObject.name,
						BaseObjectId = serverObject.ObjectTypeID,
						SpaceObjectId = serverObject.ObjectInstanceID,
						TriggerObjects = triggerObjects.ToArray(),
					};
					items.Add(serverObjectInfo);
				}

				result.Add(new SceneServerObjectInfo
				{
					SceneName = Path.GetFileNameWithoutExtension(scenePath),
					ServerObjects = items.ToArray(),
				});
			}

			return result.ToArray();

			bool IsServerObjectScene(string sceneName)
			{
				var name = sceneName.Replace(".unity", string.Empty);
				return name.EndsWith(ServerObjectSuffix);
			}
		}

		// PrintSpaceInteractionMapStr로 데이터 뽑아서 붙여넣기
		private string GetInteractionValue(string spaceId, TObjectInteractionType interactionType) => ObjectInteractionValueMap.TryGetValue((spaceId, interactionType), out var value) ? value : string.Empty;

		// ServerObjectEditor - OrderServerObject
		private async UniTask ValidateIdsAsync()
		{
			var currentScene = SceneManager.GetActiveScene();
			List<GameObject> rootGameObjects = new List<GameObject>(currentScene.rootCount);
			currentScene.GetRootGameObjects(rootGameObjects);

			List<long> remIds = new List<long>(rootGameObjects.Count);
			List<long> existIds = new List<long>();

			for (long i = 1; i <= rootGameObjects.Count; i++)
				remIds.Add(i);

			foreach (var rootObject in rootGameObjects)
			{
				if (!rootObject.TryGetComponent<ServerObject>(out var component)) return;

				var currentInstanceId = component.ObjectInstanceID;
				existIds.Add(currentInstanceId);
				remIds.Remove(currentInstanceId);
			}

			rootGameObjects.Reverse();
			foreach (var rootObject in rootGameObjects)
			{
				var serverObject = rootObject.GetComponent<ServerObject>();
				var instanceId = serverObject.ObjectInstanceID;

				if (existIds.FindAll((value) => value == instanceId).Count > 1)
				{
					var newId = remIds[0];
					Debug.Log($"{rootObject.name} : 컨플릭트 (id : {instanceId}). 새로 id({newId})를 발급합니다. ");
					serverObject.ObjectInstanceID = newId;
					remIds.RemoveAt(0);
					existIds.Remove(instanceId);

					EditorUtility.SetDirty(serverObject);
					await UniTask.Yield();
				}
			}

			await UniTask.Yield();
			EditorSceneManager.SaveScene(currentScene);
		}

		protected static string[] GetAllScenePaths() => Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories);
		protected static string[] GetAllScenePaths(string[] filteredSceneNames) => GetAllScenePaths().Where(path => filteredSceneNames.Any(path.Contains)).ToArray();
		protected void SetAllLogicTypeCandidates()
		{
			var type = typeof(eLogicType);
			var values = type.GetEnumValues();
			LogicTypes = values.Cast<eLogicType>().ToList();
		}
#endregion // Private Methods

#region Util
		private async UniTask SaveOrOverwrite(string contents, string name, string ext, string savePath = "")
		{
			if (string.IsNullOrWhiteSpace(savePath))
				await SaveAsync(contents, name, ext);
			else
			{
				var filePath = Path.Combine(savePath, $"{name}.{ext}");
				await WriteFileAsync(contents, filePath);
			}
		}
		private async UniTask SaveAsync(string contents, string name, string ext)
		{
			var title = $"{name} 저장";
			var fileName = $"{name}.{ext}";
			var savePath = EditorUtility.SaveFilePanel(title, string.Empty, fileName, ext);
			if (!string.IsNullOrWhiteSpace(savePath))
				await File.WriteAllTextAsync(savePath, contents)!;
		}

		private async UniTask WriteFileAsync(string contents, string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath)) return;

			if (File.Exists(filePath))
				File.Delete(filePath);

			await File.WriteAllTextAsync(filePath, contents)!;
		}

		protected static async UniTask<T> LoadTableAsync<T>(string key) where T : Loader<T>
		{
			var asset = await Addressables.LoadAssetAsync<TextAsset>(key);
			if (asset == null)
			{
				C2VDebug.LogWarning($"테이블 로드 실패 {typeof(T)}");
				return null;
			}

			var bytes = asset.bytes;
			var table = await Loader<T>.LoadAsync(bytes);
			if (table == null)
			{
				C2VDebug.LogWarning($"테이블 로드 실패 {typeof(T)}");
				return null;
			}
			return table;
		}
#endregion // Util

#region Data
		protected class SceneServerObjectInfo
		{
			public string SceneName;
			public ServerObjectInfo[] ServerObjects;
		}

		protected class ServerObjectInfo
		{
			public string Name;
			public long BaseObjectId;
			public long SpaceObjectId;
			public TriggerObjectInfo[] TriggerObjects;
		}

		protected class TriggerObjectInfo
		{
			public string Name;
			public int CallbackIdx;
			public eLogicType LogicType;
			public KeyValue[] Tags;
			public int TriggerId;
			public eServerCheck ServerCheck;
		}
		private class ObjectInteractionRow
		{
			public string MappingId;
			public long SpaceObjectId;
			public int InteractionNo;
			// public int InteractionLink;
			public long InteractionLinkId;
			public string InteractionValue;
			public string SceneName;
			public string ObjectName;
		}

		protected struct SpaceInfo<T>
		{
			public string SpaceId;
			public T Type;
			public BuildingTableInfo? BuildingTableInfo;
			public SpaceTableInfo? SpaceTableInfo;
			public SpaceDetailTableInfo? SpaceDetailTableInfo;
		}

		protected struct BuildingTableInfo
		{
			public int BuildingId;
			public int FloorNo;
			public int Location;
		}

		protected struct SpaceTableInfo
		{
			public string AccountId;
			public int No;
			public string Name;
			public string Description;
			public long TemplateId;
		}

		protected struct SpaceDetailTableInfo
		{
			public int BuildingType;
			public int BuildingCode;
			public string SpaceType;
			public string SpaceCode;
		}
#endregion // Data
	}
}
