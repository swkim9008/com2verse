/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesAssetCacheInfos.cs
* Developer:	tlghks1009
* Date:			2023-03-28 17:39
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com2Verse.AssetSystem
{
	[Serializable]
	public class C2VAssetBundleCacheCollection
	{
		[field: SerializeField] private List<C2VAssetBundleCacheEntity> _entities;

		public IReadOnlyList<C2VAssetBundleCacheEntity> Entities => _entities;

		public C2VAssetBundleCacheCollection() => _entities = new List<C2VAssetBundleCacheEntity>();

		public void AddLayout(C2VAssetBundleCacheEntity cacheInfo) => _entities.Add(cacheInfo);

		public void Clear() => _entities.Clear();
	}

	[Serializable]
	public class C2VAssetBundleCacheEntity
	{
		[field: SerializeField] public string BundleName { get; set; }

		[field: SerializeField] public string CacheName { get; set; }
	}
}
