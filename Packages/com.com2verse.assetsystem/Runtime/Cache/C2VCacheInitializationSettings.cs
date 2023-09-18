/*===============================================================
* Product:		Com2Verse
* File Name:	C2VCacheInitializationSettings.cs
* Developer:	tlghks1009
* Date:			2023-03-27 18:42
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.IO;
using Com2Verse.BuildHelper;
using Com2Verse.Logger;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Util;

namespace Com2Verse.AssetSystem
{
#if UNITY_EDITOR
	[CreateAssetMenu(fileName = "CacheInitializationSettings.asset", menuName = "Addressables/Initialization/C2V Cache Initialization Settings")]
	public sealed class C2VCacheInitializationSettings : ScriptableObject, IObjectInitializationDataProvider
	{
		[SerializeField]
		private C2VCacheInitializationData _data;

		public C2VCacheInitializationData Data => _data;

		public string Name => "C2V Asset Bundle Cache Settings";

		public ObjectInitializationData CreateObjectInitializationData()
		{
			return ObjectInitializationData.CreateSerializedInitializationData<C2VCacheInitialization>(nameof(C2VCacheInitialization), _data);
		}
	}
#endif


	[Serializable]
	public class C2VCacheInitializationData
	{
		[SerializeField]
		private bool _compressionEnabled = true;

		public bool CompressionEnabled => _compressionEnabled;


		[SerializeField]
		private C2VCacheDirectory[] _cacheDirectories;

		public C2VCacheDirectory[] CacheDirectories => _cacheDirectories;
	}


	[Serializable]
	public class C2VCacheDirectory
	{
		[field: SerializeField] public eAssetBundleType CacheDirectoryOverride { get; set; }

		[field: SerializeField] public bool LimitCacheSize { get; set; }

		[field: SerializeField, DrawIf(nameof(LimitCacheSize), true)] public long MaximumAvailableStorageSpace { get; set; }
	}


	public class C2VCacheInitialization : IInitializableObject
	{
		public bool Initialize(string id, string dataStr)
		{
			LoadCacheInitializationData(id, dataStr);

			return true;
		}


		private void LoadCacheInitializationData(string id, string dataStr)
		{
			var data = JsonUtility.FromJson<C2VCacheInitializationData>(dataStr);
			if (data != null)
			{
				Caching.compressionEnabled = data.CompressionEnabled;

				foreach (var cacheDirectory in data.CacheDirectories)
				{
					var buildEnvironment = C2VAssetBundleManager.Instance.AppBuildEnvironment;
					var dir = $"{Application.temporaryCachePath}/Bundles/{buildEnvironment}/{cacheDirectory.CacheDirectoryOverride}";

					var activeCache = C2VCaching.AddCache(dir);

					if (cacheDirectory.CacheDirectoryOverride == eAssetBundleType.BUILT_IN)
						Caching.currentCacheForWriting = activeCache;

					activeCache.maximumAvailableStorageSpace = cacheDirectory.LimitCacheSize ? cacheDirectory.MaximumAvailableStorageSpace : long.MaxValue;
				}
			}
		}


		public AsyncOperationHandle<bool> InitializeAsync(ResourceManager rm, string id, string data)
		{
			var initOperation = new CacheInitOperation();
			initOperation.Initialize(() => Initialize(id, data));

			return rm.StartOperation(initOperation, default);
		}
	}


	class CacheInitOperation : AsyncOperationBase<bool>
	{
		private Func<bool> _callback;

		public void Initialize(Func<bool> callback)
		{
			_callback = callback;
		}


		protected override void Execute()
		{
			OnCacheInitTask().Forget();
		}


		private async UniTask OnCacheInitTask()
		{
			while (!Caching.ready)
			{
				await UniTask.Yield();
			}

			C2VDebug.LogCategory("AssetBundle", "Cache Initialization Complete");
			C2VCaching.Initialize();

			Complete(_callback(), true, string.Empty);
		}
	}
}
