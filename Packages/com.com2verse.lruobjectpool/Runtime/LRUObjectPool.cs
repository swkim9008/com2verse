/*===============================================================
* Product:		Com2Verse
* File Name:	ObjectPool.cs
* Developer:	NGSG
* Date:			2023-05-23 15:19
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Com2Verse.Logger;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;

namespace Com2Verse.LruObjectPool
{
	public sealed class LRUObjectPool : MonoBehaviour
	{
		public class LRUObject
		{
			public UnityEngine.Object _obj;
			public long _deleteTime;

			public LRUObject()
			{
			}
			
			public LRUObject(UnityEngine.Object obj, long deleteTime)
			{
				_obj = obj;
				_deleteTime = deleteTime;
			}

			public void Release()
			{
				_deleteTime = 0;

				if (RuntimeObjectManager.InstanceExists)
					RuntimeObjectManager.Instance.DestoryObject(_obj);
				_obj = null;
			}
		}

		private const string ROOT_NAME = "LRU_ObjectPool";
		private const int CHECK_LIFE_TIME_TICK = 60000;	// 1분마다 오브젝트 오브젝트 삭제할 지를 체크한다 
		private const int MAX_POOL_COUNT = 1000;	// 한 종류의 오브젝트가 1000개 이상이면 제일 오래된 오브젝트를 강제로 삭제한다
		private const int RELEASE_MEMORY_CONDITION_COUNT = 100; // 100 개씩 Destory 되면 사용하지 않는 어셋 정리한다
		
		private GameObject _poolRoot;
		private readonly Dictionary<string, LRUStack<LRUObject>> _dicPool = new Dictionary<string, LRUStack<LRUObject>>();

		private readonly List<LRUObject> _reserveDestroyList = new List<LRUObject>();
		private int _destroyCount = 0;

#region Mono
		private void OnDestroy()
		{
			ClearGameObjectPool();

			Destroy(_poolRoot);
		}
#endregion	// Mono

		public void StartProc(Transform parent)
		{
			_poolRoot = new GameObject(ROOT_NAME);
			_poolRoot.transform.SetParent(parent);

			AsyncCheckObjectLifeTime().Forget();
		}

		public void ClearGameObjectPool()
		{
			var it = _dicPool.GetEnumerator();
			while (it.MoveNext() == true)
			{
				while(it.Current.Value.Count > 0)
				{
					LRUObject lruObject = it.Current.Value.Pop();
					lruObject.Release();
				}
				it.Current.Value.Clear();
			}
			it.Dispose();
			_dicPool.Clear();

			DestroyObjects(true);
		}

		public UnityEngine.Object Get(string objectName)
		{
			if (string.IsNullOrEmpty(objectName))
				return null;
			
			// 풀어서 찾아서 리턴한다
			LRUObject lruObject = GetFromPool(objectName);
			if (lruObject == null)
			{
				return null;
			}
			else
			{
				lruObject._deleteTime = 0; // 사용 시간 초기화
				if (lruObject._obj is GameObject go)
				{
					go.SetActive(true);
					go.transform.SetParent(null, false);
				}
				return lruObject._obj;
			}
		}

		private LRUObject CreateLruObject(UnityEngine.Object obj, float duration)
		{
			long deleteTime = DateTime.Now.AddSeconds(duration).Ticks;
			if (duration == -1f)
				deleteTime = -1;
			return new LRUObject(obj, deleteTime);
		}
		
		public void Release(UnityEngine.Object obj, float duration)
		{
			if (obj == null || string.IsNullOrEmpty(obj.name))
				return;

			if (_poolRoot == null)
			{
				Destroy(obj);
				return;
			}

			// LoadAsset 이거나, 즉시 삭제할 GameObject 이면, 바로 강제로 삭제한다
			if (duration == 0 || obj is not GameObject)
			{
				// 번들 핸들 찾아서 오브젝트와 같이 등록해준다
				LRUObject lruObject = CreateLruObject(obj, duration);
				
				// 바로 삭제한다
				lruObject.Release();
				lruObject = null;
			}
			else
			{
				// GameObject 
				if (InsertPool(obj.name, obj, duration))
				{
					if (obj is GameObject go)
					{
						// 풀 하이라키에 돌려줌
						go.transform.SetParent(_poolRoot.transform, false);
						go.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
						go.transform.localScale = Vector3.one;
						go.SetActive(false);
					}
				}
				// else
				// {
				// 	C2VDebug.LogWarningCategory("LRUPool", "{0} fail insertPool", obj.name);
				// }
			}
		}

		private LRUObject GetFromPool(string objectName)
		{
			if (string.IsNullOrEmpty(objectName))
				return null;

			if (_dicPool.ContainsKey(objectName))
			{
				if (_dicPool[objectName].Count == 0)
				{
					return null;
				}
				else
				{
					LRUObject lruObject = _dicPool[objectName].Pop();
					return lruObject;
				}
			}
			else
			{
				AddPool(objectName);
				return null;
			}
		}

		public bool AddPool(string objectName, int capacity = 4)
		{
			if (_dicPool.ContainsKey(objectName) == false)
			{
				_dicPool.Add(objectName, new LRUStack<LRUObject>(capacity));
				return true;
			}

			return false;
		}

		public bool Contains(string objectName, UnityEngine.Object obj)
		{
			if (_dicPool.ContainsKey(objectName))
			{
				foreach (var lruObject in _dicPool[objectName])
				{
					if (lruObject._obj == obj)
						return true;
				}
			}
			return false;
		}
		
		private bool InsertPool(string objectName, UnityEngine.Object obj, float duration)
		{
			// 폴에 돌려준다
			if (_dicPool.ContainsKey(objectName))
			{
				// 이미 풀에 있는걸 다시 풀에 넣으려면 리턴함
				if (Contains(objectName, obj))
				{
					// 이미 풀에 존재함.... 너무 많아 느리기에 로그도 남기지 않는다 
					//C2VDebug.LogErrorCategory("LRUPool", "{0} gameObject already exist pool", objectName);
					UnityEngine.Debug.LogErrorFormat("[LRUPool]  {0} gameObject already exist pool", objectName);
					return false;
				}

				// 최대 개수 체크
				if(duration != -1f)
					CheckMaxCount(objectName);
				
				// 번들 핸들 찾아서 오브젝트와 같이 등록해준다
				LRUObject lruObject = CreateLruObject(obj, duration);
				
				// 추가
				_dicPool[objectName].Push(lruObject);
				return true;
			}
			else
			{
				// 풀에 없는 오브젝트가 넘어옴
 				C2VDebug.LogWarningCategory("LRUPool", "{0} is not LRUObject", objectName);
                LRUObject lruObject = CreateLruObject(obj, duration);
                // 바로 삭제한다
                lruObject.Release();
                lruObject = null;
                
                //_reserveDestoryList.Add(lruObject);
				return false;
			}
		}

		private void CheckMaxCount(string objectName)
		{
			// 최대 개수 넘으면 제일 오래된 오브젝트 삭제한다
			if (_dicPool[objectName].Count >= MAX_POOL_COUNT)
			{
				UnityEngine.Debug.LogWarningFormat("[LRUPool], {0} pool count over MAX_POOL_COUNT", objectName);
				
				//LRUObject lruObject = _dicPool[objectName].Peek();
				LRUObject lruObject = _dicPool[objectName].GetFront();
	
				lruObject._deleteTime = 0;
				_reserveDestroyList.Add(lruObject);
			}
		}

		private void ProcessObjectsDestroy(bool force = false)
		{
			List<LRUObject> removeList = new List<LRUObject>();
			
			// 삭제할 오브젝트 등록
			var it = _dicPool.GetEnumerator();
			while (it.MoveNext() == true)
			{
				foreach (var lruObject in it.Current.Value)
				{
					// 삭제시간이 -1이면 삭제 안함
					if(lruObject._deleteTime == -1f)
						continue;
					
					// 오래된것들 순서로 가져오기에 시간이 지나지 않은 object 라면 다음 object 들도 모두 지나지 않았기에 순회 정지함
					if (lruObject._deleteTime < DateTime.Now.Ticks || force)
					{
						removeList.Add(lruObject);
						_reserveDestroyList.Add(lruObject);
					}
					else
						break;
				}

				// 삭제
				if (removeList.Count > 0 )
				{
					it.Current.Value.RemoveRange(0, removeList.Count);
					removeList.Clear();	
				}
			}

			DestroyObjects(force);
		}


		private void DestroyObjects(bool force = false)
		{
			// 삭제할 오브젝트 삭제
			for (int i = 0; i < _reserveDestroyList.Count; i++)
			{
				//_reserveDestoryList[i].ReleaseInstance();
				_reserveDestroyList[i].Release();
				_destroyCount++;
			}
			_reserveDestroyList.Clear();

			// 메모리 정리
			// 굳이 하지 않아도 Ref count 가 0 이 되면 번들파일 자체도 내려감
			if (force || _destroyCount > RELEASE_MEMORY_CONDITION_COUNT)
			{
#if ENABLE_LOGGING
				Stopwatch sw = new Stopwatch();
				sw.Start();
#endif
				Resources.UnloadUnusedAssets();
				
#if ENABLE_LOGGING
				sw.Stop();
				UnityEngine.Debug.Log($"[LRUPool] Resources.UnloadUnusedAssets = {sw.ElapsedMilliseconds}, _destoryCount = {_destroyCount}");
#endif
				_destroyCount = 0;
			}
		}
		
		private async UniTask AsyncCheckObjectLifeTime()
		{
			while (true)
			{
				ProcessObjectsDestroy();
				await UniTask.Delay(CHECK_LIFE_TIME_TICK);
			}
		}
	}
}
