// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	BuilderSerializer.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-13 오후 2:19
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Extension;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Com2Verse.Builder
{
	public static class BuilderSerializer
	{
		private static readonly JsonConverter[] JsonConverters = new JsonConverter[] { new CustomVector2Converter(), new CustomVector3Converter(), new CustomVector4Converter() };
		private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore, Converters = JsonConverters, Formatting = Formatting.Indented };

		public static string SerializeSpace()
		{
			SpaceManager spaceManager = SpaceManager.Instance;
			BuilderSaveDataModel dataModel = new BuilderSaveDataModel();

			dataModel.type = spaceManager.SpaceType;
			dataModel.templateId = spaceManager.TemplateId;
			var serializeTargetObjects = spaceManager.MapRoot.GetComponentsInChildren<BuilderObject>();
			dataModel.objects = new BuilderGameObjectSaveDataModel[serializeTargetObjects.Length];
			for (int i = 0; i < serializeTargetObjects.Length; i++)
			{
				var targetObject = serializeTargetObjects[i];

				var parent = targetObject.transform.parent.GetComponentInParent<BuilderObject>();
				Quaternion localRotation = targetObject.transform.localRotation;
				dataModel.objects[i] = new BuilderGameObjectSaveDataModel()
				{
					objectId = targetObject.Item.BaseObjectId,
					parentId = parent.IsReferenceNull() ? 0 : parent.SerializationId,
					serializationId = targetObject.SerializationId
				};
				SetLocalTransform(ref dataModel.objects[i].localTransform, targetObject.transform);
			}
			dataModel.objectMaxIndex = BuilderObject.IndexCounter;

			dataModel.wallMaxIndex = BuilderWall.IndexCounter;
			dataModel.walls = new BuilderWallSaveDataModel[spaceManager.Walls.Length];
			for (int i = 0; i < dataModel.walls.Length; i++)
			{
				var wall = spaceManager.Walls[i];
				int[] neighbor = new int[wall.Neighbor.Length];
				if (!wall.IsCorner)
				{
					for (int j = 0; j < neighbor.Length; j++)
					{
						neighbor[j] = wall.Neighbor[j].Index;
					}
				}

				dataModel.walls[i] = new BuilderWallSaveDataModel()
				{
					appliedTextureId = wall.AppliedTextureId,
					index = wall.Index,
					innerNormalDirection = (Float3)wall.InnerNormalDirection,
					isCorner = wall.IsCorner,
					neighbor = neighbor
				};
				SetLocalTransform(ref dataModel.walls[i].localTransform, wall.transform);
			}

			dataModel.floors = new BuilderFloorSaveDataModel[spaceManager.Floors.Length];
			dataModel.floorMaxIndex = BuilderFloor.IndexCounter;
			for (int i = 0; i < dataModel.floors.Length; i++)
			{
				var floor = spaceManager.Floors[i];
				int[] neighbor = new int[floor.Neighbor.Length];
				for (int j = 0; j < neighbor.Length; j++)
				{
					neighbor[j] = floor.Neighbor[j].Index;
				}

				dataModel.floors[i] = new BuilderFloorSaveDataModel()
				{
					appliedTextureId = floor.AppliedTextureId,
					index = floor.Index,
					innerNormalDirection = (Float3)floor.InnerNormalDirection,
					floorScale = (Float2)floor.FloorScale,
					neighbor = neighbor
				};
				SetLocalTransform(ref dataModel.floors[i].localTransform, floor.transform);
			}

			return JsonConvert.SerializeObject(dataModel, JsonSerializerSettings);
		}

		private static void SetLocalTransform(ref BuilderTransform transform, Transform targetTransform)
		{
			Quaternion localRotation = targetTransform.localRotation;
			transform.localPosition = (Float3)targetTransform.localPosition;
			transform.localRotation = new Float4(localRotation.x, localRotation.y, localRotation.z, localRotation.w);
			transform.localScale = (Float3)targetTransform.localScale;
		}

		private static GameObject SetLocalTransform(GameObject prefab, ref BuilderTransform localTransform, Transform parent)
		{
			GameObject rootObject = new GameObject(prefab.name);
			rootObject.transform.SetParent(parent);
			
			rootObject.transform.localPosition = localTransform.localPosition;
			rootObject.transform.localRotation = new Quaternion(localTransform.localRotation.x,
			                                                    localTransform.localRotation.y, localTransform.localRotation.z,
			                                                    localTransform.localRotation.w);
			rootObject.transform.localScale = localTransform.localScale;
			
			GameObject newObject = GameObject.Instantiate<GameObject>(prefab, rootObject.transform);

			return rootObject;
		}
		
		public static async UniTask<string> DeserializeSpace(string spaceId, string json)
		{
			SpaceManager spaceManager = SpaceManager.Instance;
			spaceManager.SpaceID = spaceId;
			BuilderSaveDataModel dataModel = JsonConvert.DeserializeObject<BuilderSaveDataModel>(json, JsonSerializerSettings);
			spaceManager.SpaceType = dataModel.type;
			spaceManager.TemplateId = dataModel.templateId;
			Dictionary<long, Transform> parentMap = new Dictionary<long, Transform>();

			BuilderObject.IndexCounter = dataModel.objectMaxIndex;
			for (int i = 0; i < dataModel.objects.Length; i++)
			{
				var targetObject = dataModel.objects[i];
				var targetModel = await BuilderAssetManager.Instance.GetAssetAsync(eInventoryItemCategory.OBJECT, targetObject.objectId);

				Transform parent = targetObject.parentId == 0 ? SpaceManager.Instance.MapRoot.transform : parentMap[targetObject.parentId];

				GameObject newObject = SetLocalTransform(targetModel.LoadedAsset, ref targetObject.localTransform, parent);
				
				var children = newObject.GetComponentsInChildren<Transform>(true);
				foreach (var child in children)
				{
					child.gameObject.layer = SpaceManager.SelectableObjectLayer;
				}
				
				parentMap.Add(targetObject.serializationId, newObject.transform);
				var builderObject = newObject.AddComponent<BuilderObject>();
				builderObject.SerializationId = targetObject.serializationId;
				if (BuilderAssetManager.Instance.ItemListMap[eInventoryItemCategory.OBJECT].TryGetValue(targetObject.objectId, out var item))
				{
					builderObject.Init(newObject, item);
				}
				else
				{
					throw new Exception($"#{targetObject.objectId} :오브젝트 시스템에 등록되지 않은 아이템입니다.");
				}
			}

			if (spaceManager.SpaceType == eBuilderSpaceType.EMPTY)
			{
				spaceManager.Walls = new BuilderWall[dataModel.walls.Length];
				Dictionary<int, BuilderWall> wallLookup = new Dictionary<int, BuilderWall>();
				for (int i = 0; i < dataModel.walls.Length; i++)
				{
					var data = dataModel.walls[i];
					if (data.isCorner)
						spaceManager.Walls[i] = spaceManager.CreateCorner(
							new Vector2(data.localTransform.localPosition.x, data.localTransform.localPosition.z),
							new Vector2(data.innerNormalDirection.x, data.innerNormalDirection.z));
					else
						spaceManager.Walls[i] = spaceManager.CreateWall(
							new Vector2(data.localTransform.localPosition.x, data.localTransform.localPosition.z),
							new Vector2(data.innerNormalDirection.x, data.innerNormalDirection.z));

					wallLookup.Add(data.index, spaceManager.Walls[i]);

					BuilderWall newWall = spaceManager.Walls[i];
					newWall.IsCorner = data.isCorner;
					newWall.Index = data.index;
					newWall.InnerNormalDirection = data.innerNormalDirection;
					newWall.AppliedTextureId = data.appliedTextureId;

					newWall.transform.position = data.localTransform.localPosition;
					if (data.appliedTextureId != 0)
					{
						var loadedMaterial =
							(await BuilderAssetManager.Instance.GetAssetAsync(eInventoryItemCategory.TEXTURE,
								data.appliedTextureId)).LoadedMaterial;
						newWall.GetComponent<MeshRenderer>().sharedMaterial = loadedMaterial;
						newWall.AssignedMaterial = loadedMaterial;
					}
				}

				for (int i = 0; i < dataModel.walls.Length; i++)
				{
					var data = dataModel.walls[i];
					spaceManager.Walls[i].Neighbor = new BaseWallObject[data.neighbor.Length];
					for (int j = 0; j < data.neighbor.Length; j++)
					{
						int neighborIndex = data.neighbor[j];
						if (wallLookup.TryGetValue(neighborIndex, out BuilderWall neighborWall))
						{
							spaceManager.Walls[i].Neighbor[j] = neighborWall;
						}
						else
						{
							if (neighborIndex != 0)
								throw new Exception($"Neighbor wall does not exists {neighborIndex}");
						}
					}
				}

				BuilderWall.IndexCounter = dataModel.wallMaxIndex;

				spaceManager.Floors = new BuilderFloor[dataModel.floors.Length];
				Dictionary<int, BuilderFloor> floorLookup = new Dictionary<int, BuilderFloor>();
				for (int i = 0; i < dataModel.floors.Length; i++)
				{
					var data = dataModel.floors[i];
					spaceManager.Floors[i] = spaceManager.CreateFloor(data.floorScale.x, data.floorScale.y,
						new Vector2(data.localTransform.localPosition.x, data.localTransform.localPosition.z));
					floorLookup.Add(data.index, spaceManager.Floors[i]);

					BuilderFloor newFloor = spaceManager.Floors[i];
					newFloor.FloorScale = data.floorScale;
					newFloor.Index = data.index;
					newFloor.InnerNormalDirection = data.innerNormalDirection;
					newFloor.AppliedTextureId = data.appliedTextureId;

					newFloor.transform.localPosition = data.localTransform.localPosition;
					if (data.appliedTextureId != 0)
					{
						var loadedMaterial =
							(await BuilderAssetManager.Instance.GetAssetAsync(eInventoryItemCategory.TEXTURE,
								data.appliedTextureId)).LoadedMaterial;
						newFloor.GetComponent<MeshRenderer>().sharedMaterial = loadedMaterial;
						newFloor.AssignedMaterial = loadedMaterial;
					}
				}

				for (int i = 0; i < dataModel.floors.Length; i++)
				{
					var data = dataModel.floors[i];
					spaceManager.Floors[i].Neighbor = new BaseWallObject[data.neighbor.Length];
					for (int j = 0; j < data.neighbor.Length; j++)
					{
						int neighborIndex = data.neighbor[j];
						if (floorLookup.TryGetValue(neighborIndex, out BuilderFloor neighborFloor))
						{
							spaceManager.Floors[i].Neighbor[j] = neighborFloor;
						}
						else
						{
							throw new Exception("Neighbor wall does not exists");
						}
					}
				}

				BuilderFloor.IndexCounter = dataModel.floorMaxIndex;
			}
			else if (spaceManager.SpaceType == eBuilderSpaceType.TEMPLATE)
			{
				await spaceManager.LoadTemplateObject();
			}

			return JsonConvert.SerializeObject(dataModel, JsonSerializerSettings);
		}
	}
}
