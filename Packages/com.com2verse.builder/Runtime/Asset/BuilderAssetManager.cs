// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	AssetManager.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-13 오후 12:40
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using Com2Verse.AssetSystem;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.PhysicsAssetSerialization;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Com2Verse.Builder
{
	public class BuilderAssetManager : Singleton<BuilderAssetManager>
	{
		private BuilderAssetManager() { }

		public Dictionary<eInventoryItemCategory, Dictionary<long, BuilderInventoryItem>> ItemListMap { get; private set; } = new Dictionary<eInventoryItemCategory, Dictionary<long, BuilderInventoryItem>>();
		
		private readonly Dictionary<string, BuilderModelInstanceModel> _assetMap = new();
		
		public void Initialize()
		{
			int maxCategory = Enum.GetValues(typeof(eInventoryItemCategory)).Cast<int>().Max();
			for (int i = 0; i <= maxCategory; i++)
			{
				ItemListMap.Add((eInventoryItemCategory)i, new Dictionary<long, BuilderInventoryItem>());
			}
		}

		public BuilderModelInstanceModel GetAsset(eInventoryItemCategory category, long objectId)
		{
			if (ItemListMap[category].TryGetValue(objectId, out var item))
			{
				if (_assetMap.TryGetValue(item.AddressableId, out var asset))
					return asset;

				BuilderModelInstanceModel newModel = null;
				switch (category)
				{
					case eInventoryItemCategory.NONE:
						return null;
					case eInventoryItemCategory.OBJECT:
						newModel = GetGameObjectAsset(item);
						break;
					case eInventoryItemCategory.TEXTURE:
						newModel = GetMaterial(item);
						break;
				}
				
				if (newModel != null)
					_assetMap.Add(item.AddressableId, newModel);

				return newModel;
			}

			return null;
		}
		
		public async UniTask<BuilderModelInstanceModel> GetAssetAsync(eInventoryItemCategory category, long objectId)
		{
			if (ItemListMap[category].TryGetValue(objectId, out var item))
			{
				if (_assetMap.TryGetValue(item.AddressableId, out var asset))
					return asset;

				BuilderModelInstanceModel newModel = null;
				switch (category)
				{
					case eInventoryItemCategory.NONE:
						return null;
					case eInventoryItemCategory.OBJECT:
						newModel = await GetGameObjectAssetAsync(item);
						break;
					case eInventoryItemCategory.TEXTURE:
						newModel = await GetMaterialAsync(item);
						break;
				}
				
				if (newModel != null)
					_assetMap.Add(item.AddressableId, newModel);

				return newModel;
			}

			return null;
		}

		private bool ValidateBound(Bounds bound)
		{
			return ValidateVector3(bound.center) && ValidateVector3(bound.size);
		}

		private bool ValidateVector3(Vector3 vector3)
		{
			return float.IsFinite(vector3.x) && float.IsFinite(vector3.y) && float.IsFinite(vector3.z);
		}

		private async UniTask LoadPrefab(BuilderInventoryItem item, BuilderModelInstanceModel model)
		{
			var modelLoadHandle = C2VAddressables.LoadAssetAsync<GameObject>(item.AddressableId).Handle;
			await modelLoadHandle;

			if (modelLoadHandle.Status != AsyncOperationStatus.Succeeded) return;

			var modelObject = modelLoadHandle.Result;
			model.LoadedAsset = modelObject;

			Bounds? bound = BuilderInventoryManager.Instance.GetModelBound(item.BaseObjectId);
			Bounds targetBound = new Bounds(Vector3.zero, Vector3.zero);
			if (bound != null)
			{
				Bounds storedBound = bound.Value;
				if (ValidateBound(storedBound))
				{
					targetBound = bound.Value;	
				}
			}
			else
			{
				// Find center of the model in parent(world) coordinate
				var filters = modelObject.GetComponentsInChildren<MeshFilter>();

				Vector3 min = Vector3.positiveInfinity;
				Vector3 max = Vector3.negativeInfinity;
				foreach (var filter in filters)
				{
					if (!filter.sharedMesh.isReadable)
						throw new Exception($"Mesh({item.AddressableId}) must be readable.");
#if UNITY_EDITOR
					if (!filter.GetComponent<C2VEventTrigger>().IsReferenceNull()) C2VDebug.LogError($"{modelObject} : 이벤트 트리거 컴포넌트에는 Mesh Filter 컴포넌트가 필요하지 않습니다. 삭제해주세요~");
#endif
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

				if (ValidateBound(modelBounds))
				{
					targetBound = modelBounds;
				}
			}
			
			model.Bound = targetBound;
			
			// Disable event trigger component;
			var triggers = modelObject.GetComponentsInChildren<C2VEventTrigger>();
			foreach (var trigger in triggers)
			{
				trigger.gameObject.SetActive(false);
			}
		}
		
		private BuilderModelInstanceModel GetGameObjectAsset(BuilderInventoryItem item)
		{
			var prefabModel = new BuilderModelInstanceModel()
			{
				LoadedAsset = null,
				Bound = new Bounds()
			};
			
			LoadPrefab(item, prefabModel).Forget();

			return prefabModel;
		}
		
		private async UniTask<BuilderModelInstanceModel> GetGameObjectAssetAsync(BuilderInventoryItem item)
		{
			var prefabModel = new BuilderModelInstanceModel()
			{
				LoadedAsset = null,
				Bound = new Bounds()
			};
			
			await LoadPrefab(item, prefabModel);

			return prefabModel;
		}

		private BuilderModelInstanceModel GetMaterial(BuilderInventoryItem item)
		{
			var model = new BuilderModelInstanceModel();
			LoadMaterialTask(model, item.AddressableId).Forget();

			return model;
		}

		private async UniTask<BuilderModelInstanceModel> GetMaterialAsync(BuilderInventoryItem item)
		{
			var model = new BuilderModelInstanceModel();
			await LoadMaterialTask(model, item.AddressableId);

			return model;
		}

		private async UniTask LoadMaterialTask(BuilderModelInstanceModel model, string address)
		{
			model.LoadedMaterial = await C2VAddressables.LoadAssetAsync<Material>(address).ToUniTask();
		}
	}
}
