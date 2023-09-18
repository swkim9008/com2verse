/*===============================================================
* Product:		Com2Verse
* File Name:	RuntimeObjectManager.cs
* Developer:	NGSG
* Date:			2023-05-23 18:51
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Com2Verse.AssetSystem;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using SimpleFileBrowser;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

namespace Com2Verse.LruObjectPool
{
	public sealed class RuntimeObjectManager : MonoSingleton<RuntimeObjectManager>
	{
		private const int CREATE_CAPACITY = 4096 * 2;
		private const float DELETE_TIME = 600.0f;	// 10분간 사용하지 않으면 오브젝트 삭제한다(기본값)
		
		private LRUObjectPool _lruObjectPool;

		// LoadAssetAsync 으로 호출 되는 로드되어 있는 어셋들 관리
		public Dictionary<string, C2VAsyncOperationHandle> _loadedAssetDictionary;
		// LoadAssetAsync 을 이용하여 로딩중인 어셋 핸들 관리
		private Dictionary<string, C2VAsyncOperationHandle> _loadingAssetDictionary;
		
		// LoadAssetAsync 을 사용하여 로드한후  instantiate를 하여 생성한 객체 Ref 관리
		private Dictionary<string, int> _instantiateRefDictionary;
		// 로드한 어셋의 어드레스별 키 목록들
		private Dictionary<string, string> _adreassableKeyDictionary;

		// Don't UnLoad Asset 씬변경시 Unload 하지말고 항상 메모리에 있을 번들들
		public List<string> _dontUnloadedAssetList;

		public int IsLoadingCount => _loadingAssetDictionary.Count;
		
		protected override void AwakeInvoked()
		{
			base.AwakeInvoked();

			if (_lruObjectPool == null)
				_lruObjectPool = gameObject.GetOrAddComponent<LRUObjectPool>();
			_lruObjectPool.StartProc(transform);
			//_lruObjectPool.StartProc(null);
			
			_loadedAssetDictionary = new Dictionary<string, C2VAsyncOperationHandle>(CREATE_CAPACITY);
			_loadingAssetDictionary = new Dictionary<string, C2VAsyncOperationHandle>(CREATE_CAPACITY);
			_instantiateRefDictionary = new Dictionary<string, int>(CREATE_CAPACITY);
			_adreassableKeyDictionary = new Dictionary<string, string>(CREATE_CAPACITY);
			_dontUnloadedAssetList = new List<string>();
		}

		protected override void OnDestroyInvoked()
		{
			//_lruObjectPool.ClearGameObjectPool();
			C2VDebug.LogCategory("LRUPool", $"RuntimeObjectManager destory");
		}

		public void Clear(bool destoryAll = false)
		{
			// GameObjectPool 삭제
			_lruObjectPool.ClearGameObjectPool();

			// Load 한 Asset 들 삭제
			ClearLoadObject(destoryAll);
			
			_loadingAssetDictionary.Clear();
			_instantiateRefDictionary.Clear();
		}

		private void ClearLoadObject(bool destoryAll = false)
		{
			if (destoryAll)
			{
				var it = _loadedAssetDictionary.GetEnumerator();
				while (it.MoveNext() == true)
					it.Current.Value.Release();

				_loadedAssetDictionary.Clear();
				_adreassableKeyDictionary.Clear();
			}
			else
			{
				Dictionary<string, C2VAsyncOperationHandle> removeList = new Dictionary<string, C2VAsyncOperationHandle>();
				var it = _loadedAssetDictionary.GetEnumerator();
				while (it.MoveNext() == true)
				{
					if(_dontUnloadedAssetList.Contains(it.Current.Key))
						continue;
					
					removeList.Add(it.Current.Key, it.Current.Value);
				}
			
				foreach (var item in removeList)
				{
					item.Value.Release();
					_loadedAssetDictionary.Remove(item.Key);
					_adreassableKeyDictionary.Remove(GetGameObjectName(item.Key));
				}
			}
		}
		
		// private bool isUseingAsset(string objName)
		// {
		// 	// 로드한 asset 중 프리팹인데 아직 Ref 가 남아 있다면 번들 내리지 않는다
		// 	if (string.IsNullOrEmpty(objName) == false)
		// 	{
		// 		if (_instantiateRefDictionary.ContainsKey(objName))
		// 			return true;
		// 	}
		//
		// 	return false;
		// }
		
		/// <summary>
		/// 다수의 GameObject를 미리 생성하게끔하여 처리해봤는데...실제 해보면 속도상 크게 이점이 없는거 같다
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="capacity"></param>
		public void Warmup(UnityEngine.Object obj, int capacity)
		{
			if (obj == null || string.IsNullOrEmpty(obj.name))
				return;

			if (obj is GameObject go)
			{
				if (_lruObjectPool.AddPool(go.name, capacity))
				{
					for (int i = 0; i < capacity; i++)
					{
						GameObject newObj = Instantiate(go);
						newObj.name = go.name;
					 	Remove(newObj, -1f);
					}
				}
			}
		}

		private GameObject InstantiateObject(GameObject original)
		{
			if (original == null)
				return null;

			GameObject newObj = Instantiate(original);
			if (newObj != null)
			{
				newObj.name = original.name;
				
				if (_instantiateRefDictionary.ContainsKey(original.name) == false)
					_instantiateRefDictionary.Add(original.name, 1);
				else
					_instantiateRefDictionary[original.name]++;
			}

			return newObj;
		}

		public void DestoryObject(UnityEngine.Object obj)
		{
			if (_instantiateRefDictionary.ContainsKey(obj.name))
			{
				_instantiateRefDictionary[obj.name]--;
				if (_instantiateRefDictionary[obj.name] < 0)
					C2VDebug.LogErrorCategory("LRUPool",$" {obj.name}  refCount = {_instantiateRefDictionary[obj.name]}");

				// 번들 메모리 삭제
				if (_instantiateRefDictionary[obj.name] == 0)
				{
					_instantiateRefDictionary.Remove(obj.name);
					ReleaseHandle(obj);
				}

				Destroy(obj);
				obj = null;
			}
			else
			{
				if (obj is GameObject go)
				{
					C2VAddressables.ReleaseInstance(go);
					Destroy(obj);
				}
				else
					ReleaseHandle(obj);
			}
		}

		private void ReleaseHandle(UnityEngine.Object obj)
		{
			if(obj.IsReferenceNull())
				return;
			
			// 로드한 어셋번들 핸들 찾아서 삭제한다 (Ref = 0)
			string addressableKey = GetAdreassableKey(obj);
			if (string.IsNullOrEmpty(addressableKey) == false)
			{
				C2VAsyncOperationHandle asyncOperationHandle = null;
				_loadedAssetDictionary.TryGetValue(addressableKey, out asyncOperationHandle);
				if (asyncOperationHandle != null)
				{
					_loadedAssetDictionary.Remove(addressableKey);
					_adreassableKeyDictionary.Remove(obj.name);

					asyncOperationHandle.Release();
					asyncOperationHandle = null;
				}
			}
			else
			{
				UnityEngine.Debug.LogError($"[LRUPool], {obj.name} is not find GetAdreassableKey");
			}
		}

		/// <summary>
		/// Pool 로 돌려준다. 삭제가 아니다...Pool 로 회수임
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="duration"></param>
		public void Remove([CanBeNull] UnityEngine.Object obj, float duration = DELETE_TIME)
		{
			if (obj == null)
			{
				// 너무 많아 느리기에 로그도 남기지 않는다
				//C2VDebug.LogWarningCategory("LRUPool", $"obj is null");
				//UnityEngine.Debug.LogWarningFormat("[LRUPool] obj is null");
				return;
			}

			// 폴에 돌려준다
			if (_lruObjectPool)
				_lruObjectPool.Release(obj, duration);
			else
			{
				C2VDebug.LogWarningCategory("LRUPool", $"obj is not puolObject");
				Destroy(obj);
			}
		}

		/// <summary>
		/// Load 한 asset 의 메모리를 내린다. LoadAsset, LoadAssetAsync 를 효출했을때 해제시 사용함
		/// </summary>
		/// <param name="addressableKey"></param>
		public void Remove(string addressableKey)
		{
			C2VAsyncOperationHandle handle = FindLoadList(addressableKey);
			if (handle != null)
				Remove((UnityEngine.Object)handle.Handle.Result, 0);
		}
		
		public C2VAsyncOperationHandle FindLoadList(string addressableKey)
		{
			if (string.IsNullOrEmpty(addressableKey))
				return null;

			if (_loadedAssetDictionary.ContainsKey(addressableKey))
				return _loadedAssetDictionary[addressableKey];

			return null;
		}

		/// <summary>
		/// 동기 방식 로딩, 로딩 완료를 대기함
		/// </summary>
		/// <param name="addressableKey"></param>
		/// <param name="onComplete"></param>
		public T LoadAsset<T>(string addressableKey, bool dontUnload = false) where T : UnityEngine.Object
		{
			C2VAsyncOperationHandle handle = FindLoadList(addressableKey);
			if (handle != null)
				return (T)handle.Handle.Result;

			C2VAsyncOperationHandle <T> newHandle = C2VAddressables.LoadAsset<T>(addressableKey);
			if (newHandle != null)
			{
				AddLoadAssetDictionary<T>(addressableKey, newHandle, dontUnload);
				return (T)newHandle.Handle.Result;
			}
				
			return null;
		}


		/// <summary>
		/// 비동기 로딩, 로딩 완료를 기다리지 않는다
		/// </summary>
		/// <param name="addressableKey"></param>
		/// <param name="onComplete"></param>
		public void LoadAssetAsync<T>(string addressableKey, Action<T> onComplete = null, Action onFail = null, bool dontUnload = false) where T : UnityEngine.Object
		{
			// 이미 로딩을 했다면 그걸 리턴
			C2VAsyncOperationHandle handle = FindLoadList(addressableKey);
			if (handle != null)
				onComplete?.Invoke((T)handle.Handle.Result);
			
			else
			{
				// 로딩하고 있는 중인가
				if (_loadingAssetDictionary.ContainsKey(addressableKey) == false)
				{
					// 로드
					C2VAsyncOperationHandle<T> newHandle = C2VAddressables.LoadAssetAsync<T>(addressableKey);
					if (newHandle != null)
					{
						// 로딩리스트에 넣는다
						_loadingAssetDictionary.Add(addressableKey, newHandle);

						newHandle.OnCompleted += (resultHandle) =>
						{
							AddLoadAssetDictionary<T>(addressableKey, resultHandle, dontUnload);
							onComplete?.Invoke((T)resultHandle.Handle.Result);
						};
					}
					else
					{
						if (onFail != null)
							onFail();
						return;
					}
				}
				else
				{
					// 아직 로딩이 완료되지 않은 로딩중인 것들 처리,  로딩 완료되면 등록된 콜백 호출하게 처리
					if (_loadingAssetDictionary.TryGetValue(addressableKey, out C2VAsyncOperationHandle value))
					{
						// event 델리게이트 체인으로 연결
						value.OnCompleted += (resultHandle) =>
						{
							onComplete?.Invoke((T)resultHandle.Handle.Result);
						};
					}
				}
			}
		}

		/// <summary>
		/// 비동기 로딩, 로딩 완료되길 await 후에 결과를 리턴함
		/// 두번 같은걸 로딩하게끔 최초 로딩이 완료되길 기다렸다가 리턴함
		/// </summary>
		/// <param name="addressableKey"></param>
		/// <param name="cancellationTokenSource"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public async UniTask<T> LoadAssetAsyncAwait<T>(string addressableKey, [CanBeNull] CancellationTokenSource cancellationTokenSource = null, bool dontUnload = false) where T : UnityEngine.Object
		{
			//await UniTask.Delay(TimeSpan.FromSeconds(0.1f), false, PlayerLoopTiming.Update, cancellationTokenSource != null ? cancellationTokenSource.Token : default);
			// 이미 로딩을 했다면 그걸 리턴
			C2VAsyncOperationHandle handle = FindLoadList(addressableKey);
			if (handle != null)
			{
				//return await UniTask.FromResult((T)handle.Handle.Result);
				return (T)handle.Handle.Result;
			}

			else
			{
				// 로딩하고 있는 중인가
				if (_loadingAssetDictionary.ContainsKey(addressableKey) == false)
				{
					// 로드
					C2VAsyncOperationHandle<T> newHandle = C2VAddressables.LoadAssetAsync<T>(addressableKey);
					if (newHandle != null)
					{
						// 로딩리스트에 넣는다
						_loadingAssetDictionary.Add(addressableKey, newHandle);

						newHandle.OnCompleted += (resultHandle) =>
						{
							AddLoadAssetDictionary<T>(addressableKey, resultHandle, dontUnload);
						};
						return await newHandle.ToUniTask(null, PlayerLoopTiming.Update, cancellationTokenSource?.Token ?? default);
					}
				}
				else
				{
					if(cancellationTokenSource != null)
						C2VDebug.LogErrorCategory("LRUPool", $"{addressableKey} cancellation 하려면 호출하는 API 에서도 반드시 await 을 사용하여야함, 또는 LoadAssetAsync 로드한걸 LoadAssetAsyncAwait 로 다시 로드하려함");
					else
					{
						if (_loadingAssetDictionary.TryGetValue(addressableKey, out C2VAsyncOperationHandle value))
						{
							C2VAsyncOperationHandle<T> newHandle = value.Convert<T>();
							return await newHandle.ToUniTask();
						}
					}
				}
			}

			return null;
		}

		/// <summary>
		/// AssetReference reference 용 Load 함수들
		/// </summary>
		/// <param name="reference"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T LoadAsset<T>(AssetReference reference, bool dontUnload = false) where T : UnityEngine.Object
		{
			string addressableKey = (string)reference.RuntimeKey;
			return LoadAsset<T>(addressableKey, dontUnload);
		}
		
		public void LoadAssetAsync<T>(AssetReference reference, Action<T> onComplete = null, Action onFail = null, bool dontUnload = false) where T : UnityEngine.Object
		{
			string addressableKey = (string)reference.RuntimeKey;
			LoadAssetAsync<T>(addressableKey, onComplete, onFail, dontUnload);
		}

		public async UniTask<T> LoadAssetAsyncAwait<T>(AssetReference reference, [CanBeNull] CancellationTokenSource cancellationTokenSource = null, bool dontUnload = false) where T : UnityEngine.Object
		{
			string addressableKey = (string)reference.RuntimeKey;
			return await LoadAssetAsyncAwait<T>(addressableKey, cancellationTokenSource, dontUnload);
		}
		
		private void AddLoadAssetDictionary<T>(string addressableKey, C2VAsyncOperationHandle<T> resultHandle, bool dontUnload = false) where T : UnityEngine.Object
		{
			if (_loadedAssetDictionary.ContainsKey(addressableKey))
				_loadedAssetDictionary[addressableKey] = resultHandle;
			else
			{
				_loadedAssetDictionary.Add(addressableKey, resultHandle);
				_adreassableKeyDictionary.Add(resultHandle.Handle.Result.name, addressableKey);
			}

			// 로딩 완료되면 로딩중 리스트에서 제거
			if (_loadingAssetDictionary.ContainsKey(addressableKey))
				_loadingAssetDictionary.Remove(addressableKey);
			
			// 씬 변경시 삭제하지 말아야할 어셋 등록
			if (dontUnload)
			{
				if (_dontUnloadedAssetList.Contains(addressableKey) == false)
					_dontUnloadedAssetList.Add(addressableKey);
			}
		}
		/// <summary>
		/// LoadAsset 한 프리팹의 사본을 생성한다, 반드시 LoadAsset, LoadAssetAsync 호출 후에 사용해야함
		/// </summary>
		/// <param name="original"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T Instantiate<T>(GameObject original) where T : UnityEngine.Object
		{
			if (original == null)
				return null;

			UnityEngine.Object obj = NewObjectPool(original);
			if (null == obj)
				return null;

			return (obj as T);
		}

		public T Instantiate<T>(GameObject original, Transform parent) where T : UnityEngine.Object
		{
			if (original == null)
				return null;

			UnityEngine.Object obj = NewObjectPool(original);
			if (null == obj)
				return null;

			if (obj is GameObject go)
				go.transform.SetParent(parent);

			return (obj as T);
		}
		
		/// <summary>
		/// LoadAsset 한 프리팹의 사본을 생성한다, 반드시 LoadAsset, LoadAssetAsync 호출한후에 사용해야함
		/// </summary>
		/// <param name="original"></param>
		/// <param name="pos"></param>
		/// <param name="rot"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T Instantiate<T>(GameObject original, Vector3 pos, Quaternion rot) where T : UnityEngine.Object
		{
			if (original == null)
				return null;
			
			UnityEngine.Object obj = NewObjectPool(original);
			if (null == obj)
				return null;

			if (obj is GameObject go)
				go.transform.SetPositionAndRotation(pos, rot);
			
			return (obj as T);
		}
		
		private GameObject NewObjectPool(GameObject original)
		{
			if (original == null)
				return null;
			
			GameObject newObj = (GameObject)_lruObjectPool.Get(original.name);
			if (newObj == null)
				newObj = InstantiateObject(original);
			
			return newObj;
		}

		/// <summary>
		/// 비동기 GameObject 생성으로 생성완료를 기다리지 않는다, InstantiateAsync
		/// </summary>
		/// <param name="addressableKey"></param>
		/// <param name="onComplete"></param>
		public void InstantiateAsync(string addressableKey, Action<UnityEngine.Object> onComplete = null)
		{
			NewObjectPoolAsync(addressableKey, onComplete).Forget();
		}
		
		/// <summary>
		/// 비동기 GameObject 생성으로 생성 완료를 await 하기에 완료하고 리턴함, InstantiateAsync
		/// </summary>
		/// <param name="addressableKey"></param>
		/// <param name="onComplete"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public async UniTask<T> InstantiateAsyncAwait<T>(string addressableKey) where T : UnityEngine.Object
		{
			UnityEngine.Object obj = await NewObjectPoolAsync(addressableKey);
			if (null == obj)
				return null;

			return (obj as T);
		}
		
		public async UniTask<T> InstantiateAsyncAwait<T>(string addressableKey, Vector3 pos, Quaternion rot) where T : UnityEngine.Object
		{
			UnityEngine.Object obj = await NewObjectPoolAsync(addressableKey);
			if (null == obj)
				return null;

			if (obj is GameObject go)
				go.transform.SetPositionAndRotation(pos, rot);

			return (obj as T);
		}

		private async UniTask<UnityEngine.Object> NewObjectPoolAsync(string addressableKey, Action<UnityEngine.Object> onComplete = null)
		{
			string objectName = GetGameObjectName(addressableKey);
			GameObject newObj = (GameObject)_lruObjectPool.Get(objectName);
			if (newObj == null)
			{
				C2VAsyncOperationHandle<GameObject> handle = C2VAddressables.InstantiateAsync(addressableKey);
				handle.OnCompleted += (resultHandle) =>
				{
					resultHandle.Handle.Result.name = objectName;
					onComplete?.Invoke(resultHandle.Handle.Result);
				};

				newObj = await handle.ToUniTask();
				if (newObj == null)
				{
					C2VDebug.LogErrorCategory("LRUPool", $"{addressableKey} is null");
					return null;
				}
			}
			else
				onComplete?.Invoke(newObj);

			return newObj;
		}

		public string GetAdreassableKey(UnityEngine.Object obj)
		{
			if (obj.IsReferenceNull())
				return null;

			if (_adreassableKeyDictionary.TryGetValue(obj.name, out string adreassableKey))
				return adreassableKey;

			return null;
		}

		public string GetGameObjectName(string adreassableKey)
		{
			if (string.IsNullOrEmpty(adreassableKey))
				return null;
			
			return _adreassableKeyDictionary.FirstOrDefault(x => x.Value == adreassableKey).Key;
		}
		
#region CheatTest
		private List<GameObject> _testParentsList = new List<GameObject>();
		private List<GameObject> _testList = new List<GameObject>();
		private GameObject _testObject = null;
		SimpleFileBrowser.FileSystemEntry[] GetDirectoryFiles(string path)
		{
			string strPath = string.Format("{0}/{1}", Application.dataPath, path);
			return FileBrowserHelpers.GetEntriesInDirectory(strPath, false);
		}
		public void TestNew()
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();

			if (_testObject == null)
			{
				_testObject = new GameObject();
				_testObject.name = "GameObject";
				//Warmup(_testObject, 8192 * 2);
			}
			
			string testPath = "Building_Mice_L0.prefab";
			for (int i = 0; i < 2; i++)
			{
				// GameObject goParent = RuntimeObjectManager.Instance.Instantiate<GameObject>(_testObject);
				// _testParentsList.Add(goParent);

				GameObject loadPrefabs = LoadAsset<GameObject>(testPath);
				if (loadPrefabs)
				{
					Vector3 pos = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
					GameObject go = RuntimeObjectManager.Instance.Instantiate<GameObject>(loadPrefabs, pos, Quaternion.identity);
					_testList.Add(go);

					//go.transform.parent = goParent.transform;
				}

				LoadAssetAsync<GameObject>(testPath, (original)=>
				{
					Vector3 pos = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
					GameObject go = RuntimeObjectManager.Instance.Instantiate<GameObject>((GameObject)original, pos, Quaternion.identity);
					_testList.Add(go);

					//go.transform.parent = goParent.transform;
				});
			}

			sw.Stop();
			UnityEngine.Debug.Log($"[LRUPool] Loading time = {sw.ElapsedMilliseconds}");
		}

		public async UniTaskVoid TestNewAsync()
		{
			CancellationTokenSource cts = new CancellationTokenSource();
			string testPath = "Building_Mice_L0.prefab";
			for (int i = 0; i < 300; i++)
			{
				//await TestInstantiate();
				//TestInstantiate2().Forget();
				//C2VDebug.LogCategory("LRUPool", "TestInstantiate end call");

				Vector3 pos = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
				GameObject loadPrefabs = await LoadAssetAsyncAwait<GameObject>(testPath, cts);
				if (loadPrefabs)
				{
					GameObject go = RuntimeObjectManager.Instance.Instantiate<GameObject>(loadPrefabs, pos, Quaternion.identity);
					_testList.Add(go);
			
					UnityEngine.Debug.Log($"[LRUPool] LoadAssetAsyncAwait go.Name = {go.name}");
					//go.transform.parent = goParent.transform;
					await UniTask.Delay(TimeSpan.FromMilliseconds(10));
				}
				//C2VDebug.LogCategory("LRUPool", "LoadAssetAsync end call");
			}

			cts.Dispose();

			// for (int i = 0; i < 300; i++)
			// {
			// 	//await TestInstantiate();
			// 	//TestInstantiate2().Forget();
			// 	//C2VDebug.LogCategory("LRUPool", "TestInstantiate end call");
			//
			// 	Vector3 pos = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
			// 	LoadAssetAsync<GameObject>(testPath, (original) =>
			// 	{
			// 		Vector3 pos = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
			// 		GameObject go = RuntimeObjectManager.Instance.Instantiate<GameObject>((GameObject)original, pos, Quaternion.identity);
			// 		_testList.Add(go);
			// 	
			// 		UnityEngine.Debug.Log($"[LRUPool] go.Name = {go.name}");
			// 	});
			// 	
			// 	//C2VDebug.LogCategory("LRUPool", "LoadAssetAsync end call");
			// }
			
			// string testPath = "Building_Mice_L0.prefab";
			// for (int i = 0; i < 2; i++)
			// {
			// 	Vector3 pos = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
			// 	GameObject loadPrefabs = await LoadAssetAsyncAwait<GameObject>(testPath);
			// 	if (loadPrefabs)
			// 	{
			// 		GameObject go = RuntimeObjectManager.Instance.Instantiate<GameObject>(loadPrefabs, pos, Quaternion.identity);
			// 		_testList.Add(go);
			//
			// 		UnityEngine.Debug.Log($"[LRUPool] LoadAssetAsyncAwait go.Name = {go.name}");
			// 		//go.transform.parent = goParent.transform;
			// 	}
			// 	C2VDebug.LogCategory("LRUPool", "LoadAssetAsync end call");
			// }
		}

		private async UniTask TestCallBack(GameObject original)
		{
			Vector3 pos = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
			GameObject go = RuntimeObjectManager.Instance.Instantiate<GameObject>((GameObject)original, pos, Quaternion.identity);
			_testList.Add(go);

			UnityEngine.Debug.Log($"[LRUPool] go.Name = {go.name}");
		}
		
		private async UniTask TestInstantiate2()
		{
			string testPath = "Building_Mice_L0.prefab";
			for (int i = 0; i < 2; i++)
			{
				Vector3 pos = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
				GameObject loadPrefabs = await LoadAssetAsyncAwait<GameObject>(testPath);
				if (loadPrefabs)
				{
					GameObject go = RuntimeObjectManager.Instance.Instantiate<GameObject>(loadPrefabs, pos, Quaternion.identity);
					_testList.Add(go);
			
					UnityEngine.Debug.Log($"[LRUPool] LoadAssetAsyncAwait go.Name = {go.name}");
					//go.transform.parent = goParent.transform;
				}

				UnityEngine.Debug.Log("[LRUPool] LoadAssetAsync end call");
			}
		}

		CancellationTokenSource _testCts;
		public async UniTaskVoid TestLoadCancellationTokenSource()
		{
			if (_testCts != null)
			{
				_testCts.Cancel();
				_testCts.Dispose();
				_testCts = null;	
			}
		}
		
		public async UniTask TestInstantiate()
		{
			// 속도 테스트함.. 아래 문서 링크 첨부함
			// https://docs.unity3d.com/Packages/com.unity.addressables@1.3/manual/MemoryManagement.html
			// Addressables.InstantiateAsync약간의 관련 오버헤드가 있으므로 프레임당 동일한 개체를 수백 번 인스턴스화해야 하는 경우
			// API를 통해 로드한 Addressables 다른 방법을 통해 인스턴스화하는 것이 좋습니다.
			// 이 경우 를 호출한 Addressables.LoadAssetAsync다음 결과를 저장하고 GameObject.Instantiate()해당 결과를 호출합니다
			string testPath2 = "Project/Bundles/02_Construction/02_World/Object_Round_0_Asset/Building_AAG/Prefabs";
			SimpleFileBrowser.FileSystemEntry[] allFileEntries = GetDirectoryFiles(testPath2);
			UnityEngine.Debug.Log($"[LRUPool] allFileEntries.Length count = {allFileEntries.Length}");
			for (int i = 0; i < allFileEntries.Length; i++)
			{
				if(allFileEntries[i].Name.Contains(".meta"))
					continue;
				
				Vector3 pos = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
				
				////////// test_1 //////////
				// InstantiateAsync 하나씩 생성하기에 모두 완료되기까지는 느리지만 프레임이 끊기지 않는다
				// GameObject go = await NewAsyncAwait<GameObject>(allFileEntries[i].Name, pos, Quaternion.identity);
				// _testList.Add(go);
				

				////////// test_2 //////////
				// InstantiateAsync 로만 생성인데 다수를 InstantiateAsync(addressableKey) 호출하기에 조금 느리다.
				// NewAsync(allFileEntries[i].Name, (obj) =>
				// {
				// 	if(obj is GameObject go)
				// 		_testList.Add(go);
				// 	UnityEngine.Debug.Log($"[LRUPool] NewAsync go.Name = {obj.name}");
				// });


				////////// test_3 //////////
				// 비동기로 Load 후 Instantiate 로 생성하는데.. 이게 속도가 가장 빠르고 프레임이 끊기지 않는다.
				//LoadAsset<GameObject>(allFileEntries[i].Name, (original) =>
				// LoadAssetAsync<GameObject>(allFileEntries[i].Name, (original) =>
				// {
				// 	Vector3 pos = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
				// 	GameObject go = RuntimeObjectManager.Instance.Instantiate<GameObject>((GameObject)original, pos, Quaternion.identity);
				// 	_testList.Add(go);
				//
				// 	UnityEngine.Debug.Log($"[LRUPool] go.Name = {go.name}");
				// });

				
				////////// test_4 //////////
				try
				{
					TestLoadCancellationTokenSource();
					_testCts = new CancellationTokenSource();

					GameObject loadPrefabs = await LoadAssetAsyncAwait<GameObject>(allFileEntries[i].Name, _testCts);
					if (loadPrefabs)
					{
						GameObject go = RuntimeObjectManager.Instance.Instantiate<GameObject>(loadPrefabs, pos, Quaternion.identity);
						_testList.Add(go);

						UnityEngine.Debug.Log($"[LRUPool] LoadAssetAsyncAwait go.Name = {go.name}");
						//go.transform.parent = goParent.transform;
					}
					else
					{
						UnityEngine.Debug.Log($"[LRUPool] LoadAssetAsyncAwait TestLoadCancellationTokenSource");
					}
				}
				catch (Exception e)
				{
					if (e is OperationCanceledException)
						C2VDebug.LogCategory("[AvatarLoading]", $"Cancel");
					else
						C2VDebug.LogErrorCategory("[AvatarLoading]", $"Error");
					throw;
				}
				// TestLoadCancellationTokenSource();
				// _testCts = new CancellationTokenSource();
				//
				// GameObject loadPrefabs = await LoadAssetAsyncAwait<GameObject>(allFileEntries[i].Name, _testCts);
				// if (loadPrefabs)
				// {
				// 	GameObject go = RuntimeObjectManager.Instance.Instantiate<GameObject>(loadPrefabs, pos, Quaternion.identity);
				// 	_testList.Add(go);
				//
				// 	UnityEngine.Debug.Log($"[LRUPool] LoadAssetAsyncAwait go.Name = {go.name}");
				// 	//go.transform.parent = goParent.transform;
				// }
				// else
				// {
				// 	UnityEngine.Debug.Log($"[LRUPool] LoadAssetAsyncAwait TestLoadCancellationTokenSource");
				// }
				
				//await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
			}
		}

		public void TestRemove()
		{
			foreach (var go in _testList)
				Remove(go);
			_testList.Clear();
		
			foreach (var goParent in _testParentsList)
				Remove(goParent, 0);
			_testParentsList.Clear();
		}
		
		public void TestRemoveCount(int count)
		{
			count = Math.Min(_testList.Count, count);
			List<GameObject> removeList = new List<GameObject>();
			for(int i = 0; i < count; i++)
			{
				removeList.Add(_testList[i]);
				Remove(_testList[i]);
			}
			_testList.RemoveAll(removeList.Contains);
			
			removeList.Clear();
			
			for (int i = 0; i < count; i++)
			{
				removeList.Add(_testParentsList[i]);
				Remove(_testParentsList[i]);
			}
			_testParentsList.RemoveAll(removeList.Contains);
		}

		private CancellationTokenSource _cts;
		public async UniTaskVoid TestLoadAsset()
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
			
			// 머트리얼
			string adreassableKey1 = "De_Sc1x4_001.mat";
			Material loarMtrl = await LoadAssetAsyncAwait<Material>(adreassableKey1);
			Renderer ren = go.GetOrAddComponent<Renderer>();
			ren.material = loarMtrl;
		
		
			// 쉐이더
			// string adreassableKey2 = "M_Sd_Fresnel_01.shader";
			// Shader loadShader = await LoadAssetAsyncAwait<Shader>(adreassableKey2);
			// ren.material.shader = loadShader;
		
			// 텍스쳐
			string adreassableKey3 = "W_O_Display_1x4_001.tga";
			LoadAssetAsync<Texture>(adreassableKey3, (original) =>
			{
				Renderer ren = go.GetOrAddComponent<Renderer>();
				ren.material.mainTexture = original;
			});
			
			// 사운드
			string adreassableKey4 = "BGM_Lounge.wav";
			// LoadAssetAsync<AudioClip>(adreassableKey4, (original) =>
			// {
			// 	AudioSource audioSource = go.GetOrAddComponent<AudioSource>();
			// 	AudioClip clip = original;
			// 	audioSource.PlayOneShot(clip);
			// });
		
			// text 
			string adreassableKey5 = "testTextAsset.txt";
			LoadAssetAsync<TextAsset>(adreassableKey5, (original) =>
			{
				string date = original.text;
				UnityEngine.Debug.Log(date);
			});


			CancellationTokenSource cts = new CancellationTokenSource();
			C2VAddressables.LoadAssetAsync<AudioClip>(adreassableKey4).ToUniTask(null, PlayerLoopTiming.Update, cts.Token);
			//await C2VAddressables.LoadAssetAsync<AudioClip>(adreassableKey4).WithCancellation(cts.Token);

			if (_cts != null)
				_cts.Dispose();
			_cts = new CancellationTokenSource();

			int time = 1;
			//time = await TestCts1(_cts.Token);
			TestCts1(_cts.Token);
			UnityEngine.Debug.Log($"TestCts1 time ={time}");


			time = await TestCts2();
			//TestCts2();
			UnityEngine.Debug.Log($"TestCts2 time ={time}");
			

			//cts.Cancel();

			//await C2VAddressables.LoadAssetAsync<AudioClip>(adreassableKey4).WithCancellation(this.GetCancellationTokenOnDestroy());

			//C2VAsyncOperationHandle<AudioClip> newHandle = C2VAddressables.LoadAssetAsync<AudioClip>(adreassableKey4);
			//await newHandle.ToUniTask().W;

			// AudioSource audioSource = go.GetOrAddComponent<AudioSource>();
			// AudioClip clip = newHandle.Result;
			// audioSource.PlayOneShot(clip);
		}

		
		private static async UniTask<int> TestCts1(CancellationToken cancellationToken)
		{
			UnityEngine.Debug.Log("TestCts1 Start");
			await UniTask.Delay(TimeSpan.FromSeconds(10), DelayType.Realtime, PlayerLoopTiming.Update, cancellationToken);
			UnityEngine.Debug.Log("TestCts1 End");
			//throw new OperationCanceledException();
			return 10;
		}

		private static async UniTask<int> TestCts2()
		{
			UnityEngine.Debug.Log("TestCts2 Start");
			await UniTask.Delay(TimeSpan.FromSeconds(5), DelayType.Realtime, PlayerLoopTiming.Update);
			UnityEngine.Debug.Log("TestCts2 End");
			//throw new OperationCanceledException();
			return 5;
		}

		public void TestRemoveAsset()
		{
			string adreassableKey1 = "De_Sc1x4_001.mat";
			Remove(adreassableKey1);
		
			string adreassableKey2 = "M_Sd_Fresnel_01.shader";
			Remove(adreassableKey2);
		
			string adreassableKey3 = "W_O_Display_1x4_001.tga";
			Remove(adreassableKey3);
		
			string adreassableKey4 = "BGM_Lounge.wav";
			Remove(adreassableKey4);
		
			string adreassableKey5 = "testTextAsset.txt";
			Remove(adreassableKey5);

			_cts.Cancel();
		}
		
		
		// https://forum.unity.com/threads/textures-left-in-memory-after-calling-addressables-release-handle.860149/
		// Texture 는 어드레서블로 로드후 Release 해도 메모리에 남아있는 버그가 있다. 심지어 REf = 0 인데도 남아있다
		// public void TestLoadAsset()
		// {
		// 	string testPath = "W_O_Display_1x4_001.tga";
		// 	LoadAssetAsync<Texture>(testPath, (original) =>
		// 	{
		// 		GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
		// 		if (go)
		// 		{
		// 			Renderer ren = go.GetOrAddComponent<Renderer>();
		// 			ren.material.mainTexture = original;
		// 		}
		// 	});
		// }
		//
		// public void TestRemoveAsset()
		// {
		// 	string testPath = "W_O_Display_1x4_001.tga";
		// 	Remove(testPath);
		// 	// Resources.UnloadUnusedAssets();	// 이 함수 호출해도 남아있다
		// }

		
		// Resources.Load 를 사용한후 Resources.UnloadAsset 호출하면 정상적으로 삭제됨
		// private Texture _original;
		// public void TestLoadTexture()
		// {
		// 	string testPath = "W_O_Display_1x4_001";
		// 	_original = Resources.Load<Texture>(testPath);
		// 	GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
		// 	if (go)
		// 	{
		// 		Renderer ren = go.GetOrAddComponent<Renderer>();
		// 		ren.material.mainTexture = _original;
		// 	}
		// }
		//
		// public void TestRemoveTexture()
		// {
		// 	string testPath = "W_O_Display_1x4_001.tga";
		// 	Resources.UnloadAsset(_original);
		// }
#endregion

	}
}
