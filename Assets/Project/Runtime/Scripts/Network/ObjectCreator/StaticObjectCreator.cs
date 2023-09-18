/*===============================================================
* Product:		Com2Verse
* File Name:	StaticObjectCreator.cs
* Developer:	haminjeong
* Date:			2022-12-29 09:44
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.AssetSystem;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.LruObjectPool;
using Com2Verse.Pathfinder;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Pathfinding;
using UnityEngine;

namespace Com2Verse.Network
{
	[UsedImplicitly]
	[Definition(2)]
	public sealed class StaticObjectCreator : BaseObjectCreator
	{
		private Dictionary<long, BaseObject> _prefabMapping = null;

		public override void Initialize(Func<long, long, bool> checkExist, Func<long, int, BaseMapObject> checkPool, Transform rootTrans)
		{
			SetDelegates((serial, definition, data, initialPosition, onCompleted) =>
			{
				if (checkExist.Invoke(serial, definition))
					return;
				var avatar = data as Protocols.Avatar;
				var mapObject = checkPool.Invoke(definition, avatar.AvatarType);
				if (mapObject.IsReferenceNull())
				{
					GetMapObjectPrefab(serial, definition, initialPosition, (serialID, initPos, prefabObj) =>
					{
						if (prefabObj.IsReferenceNull())
						{
							C2VDebug.LogError($"[MapController] Failed to create object. serialID : {serial}, type : {definition}");
							return;
						}
						
						mapObject = UnityEngine.Object.Instantiate(prefabObj, initPos, Quaternion.identity).GetOrAddComponent<MapObject>();
						mapObject.transform.SetParent(rootTrans);
						
						if (_prefabMapping.TryGetValue(definition, out var model))
						{
							ClientPathFinding.Instance.SettingNavmeshCut(mapObject.gameObject, model.NavMeshRadius, model.NavMeshHeight, model.NavMeshCenter);
						}
						
						onCompleted?.Invoke(serialID, mapObject);
					}).Forget();
					return;
				}

				mapObject.transform.position = initialPosition;
				mapObject.gameObject.SetActive(true);
				mapObject.transform.SetParent(rootTrans);
				onCompleted?.Invoke(serial, mapObject);
			});
		}

		private async UniTaskVoid GetMapObjectPrefab(long serial, long type, Vector3 initPos, Action<long, Vector3, GameObject> onCompleted)
		{
			if (_prefabMapping == null)
			{
				var tableData = TableDataManager.Instance.Get<TableBaseObject>();
				_prefabMapping = new Dictionary<long, BaseObject>();
				if (tableData != null)
				{
					foreach (var data in tableData.Datas)
					{
						_prefabMapping.Add(data.Key, data.Value);
					}
				}
			}
			
			if (!_prefabMapping.TryGetValue(type, out BaseObject baseModel))
			{
				C2VDebug.LogError($"Model not exist {type}");
				onCompleted?.Invoke(serial, initPos, null);
				return;
			}

			//GameObject prefabObj = await C2VAddressables.LoadAssetAsync<GameObject>(baseModel.Name).ToUniTask();
			//씬 변경시 로드한 어셋 자동 삭제하기 위해 아래 함수로 수정함
			GameObject prefabObj = await RuntimeObjectManager.Instance.LoadAssetAsyncAwait<GameObject>(baseModel.Name);
			onCompleted?.Invoke(serial, initPos, prefabObj);
		}
	}
}
