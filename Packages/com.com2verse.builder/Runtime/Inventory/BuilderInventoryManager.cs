// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	BuilderInventory.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-08 오전 10:31
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System.Collections.Generic;
using Com2Verse.AssetSystem;
using Com2Verse.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.Builder
{
	public class BuilderInventoryManager : Singleton<BuilderInventoryManager>
	{
		private const long NormalObjectBaseIndex = 1000;
	
		private BuilderInventoryManager() { }
		private ThumbnailDatabase _thumbnailDatabase;
		private ModelBoundDatabase _modelBoundDatabase;
		private Dictionary<long, BaseObject> _baseObjects;

		public string GetPrefabName(long baseObjectId)
		{
			if (_baseObjects.TryGetValue(baseObjectId, out var baseObject))
			{
				return baseObject.Name;
			}

			return string.Empty;
		}
		
		public Bounds? GetModelBound(long baseObjectId)
		{
			if (_modelBoundDatabase.Bounds.TryGetValue(baseObjectId, out Bounds bound))
			{
				return bound;
			}

			return null;
		}
		
		public async UniTask FetchInventoryData()
		{
			// (TODO) 서버 혹은 기획데이터에서 받아오기?
			_thumbnailDatabase = await C2VAddressables.LoadAssetAsync<ThumbnailDatabase>("BuilderThumbnailDatabase.asset").ToUniTask();
			_modelBoundDatabase = await C2VAddressables.LoadAssetAsync<ModelBoundDatabase>("ModelBoundDatabase.asset").ToUniTask();
			
			_thumbnailDatabase.Initialize();
			_modelBoundDatabase.LoadData();
			
			var objectData = TableDataManager.Instance.Get<TableBaseObject>();
			_baseObjects = objectData.Datas;

			foreach (var baseObject in _baseObjects.Values)
			{
				if (baseObject.ID >= NormalObjectBaseIndex) // (TODO) 공간빌더에서 사용 가능 여부 등이 생기면 그것으로 대체
				{
					InsertItem(eInventoryItemCategory.OBJECT,baseObject.ID, baseObject.Name, baseObject.StackLevel);
				}
			}

			// (TODO) Load materials from table data. (currently not exists)
			InsertItem(eInventoryItemCategory.TEXTURE, 100000, "T_Grass_001.mat");
		}

		private void InsertItem(eInventoryItemCategory category, long objectId, string address, int stackLevel = 1)
		{
			BuilderAssetManager.Instance.ItemListMap[category].Add(objectId, new BuilderInventoryItem()
			{
				Category = category,
				AddressableId = address,
				BaseObjectId = objectId,
				Thumbnail = _thumbnailDatabase.GetThumbnail(address),
				StackLevel = stackLevel
			});
		}
	}
}
