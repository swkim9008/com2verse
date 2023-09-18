/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressables.cs
* Developer:	tlghks1009
* Date:			2023-02-17 17:22
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Com2Verse.AssetSystem
{
    public static class C2VAddressables
    {
        public static async UniTask InitializeAsync()
        {
            await new C2VAsyncOperationHandle<IResourceLocator>(Addressables.InitializeAsync()).ToUniTask();

            await UniTask.Yield();
        }


        public static C2VAsyncOperationHandle<T> LoadAsset<T>(string address, bool isErrorLog = true)
        {
            if (!AddressableResourceExists<T>(address))
            {
                if (isErrorLog)
                    C2VDebug.LogError($"Unable to load asset. Address : {address}");
                else
                    C2VDebug.LogWarning($"Unable to load asset. Address : {address}");

                return null;
            }

            if (string.IsNullOrEmpty(address))
            {
                C2VDebug.LogError($"Unable to load asset. Address : {address}");
                return null;
            }

            var handle = new C2VAsyncOperationHandle<T>(Addressables.LoadAssetAsync<T>(address));
            handle.WaitForCompletion();

            return handle;
        }


        public static C2VAsyncOperationHandle<T> LoadAssetAsync<T>(AssetReference assetReference, bool isErrorLog = true)
        {
            if (!AddressableResourceExists<T>(assetReference.RuntimeKey))
            {
#if UNITY_EDITOR
                if (assetReference.editorAsset == null)
                {
                    if (isErrorLog)
                        C2VDebug.LogError($"Unable to load asset. AssetGUID : {assetReference.AssetGUID}");
                    else
                        C2VDebug.LogWarning($"Unable to load asset. AssetGUID : {assetReference.AssetGUID}");
                }
                else
                {
                    if (isErrorLog)
                        C2VDebug.LogError($"Unable to load asset. Address : {assetReference.editorAsset.name}");
                    else
                        C2VDebug.LogWarning($"Unable to load asset. AssetGUID : {assetReference.AssetGUID}");
                }
#else
                if (isErrorLog)
                    C2VDebug.LogError($"Unable to load asset. Address : {assetReference.RuntimeKey}");
                else
                    C2VDebug.LogWarning($"Unable to load asset. Address : {assetReference.RuntimeKey}");
#endif
                return null;
            }

            return new C2VAsyncOperationHandle<T>(Addressables.LoadAssetAsync<T>(assetReference));
        }


        public static C2VAsyncOperationHandle<T> LoadAssetAsync<T>(string address, bool isErrorLog = true)
        {
            if (!AddressableResourceExists<T>(address))
            {
                if (isErrorLog)
                    C2VDebug.LogError($"Unable to load asset. Address : {address}");
                else
                    C2VDebug.LogWarning($"Unable to load asset. Address : {address}");

                return null;
            }

            return new C2VAsyncOperationHandle<T>(Addressables.LoadAssetAsync<T>(address));
        }


        public static C2VAsyncOperationHandle<GameObject> InstantiateAsync(string address, bool isErrorLog = true)
        {
            if (!AddressableResourceExists<GameObject>(address))
            {
                if (isErrorLog)
                    C2VDebug.LogError($"Unable to load asset. Address : {address}");
                else
                    C2VDebug.LogWarning($"Unable to load asset. Address : {address}");

                return null;
            }

            var handle             = Addressables.InstantiateAsync(address);
            var c2VOperationHandle = new C2VAsyncOperationHandle<GameObject>(handle);

            void EventHandler(AsyncOperationHandle<GameObject> handleInternal)
            {
                handleInternal.Completed -= EventHandler;

                if (handleInternal.Status != AsyncOperationStatus.Succeeded)
                {
                    C2VDebug.LogError($"Unable to load asset. Address : {address}");
                    return;
                }

                var createdObj = handleInternal.Result;
                createdObj.AddComponent<C2VAddressableAssetTracker>();
            }

            handle.Completed += EventHandler;

            return c2VOperationHandle;
        }


        public static C2VAsyncOperationHandle<SceneInstance> LoadSceneAsync(string address, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            if (!AddressableResourceExists<SceneInstance>(address))
            {
                C2VDebug.LogError($"Unable to load scene. Address : {address}");
                return null;
            }

            return new C2VAsyncOperationHandle<SceneInstance>(Addressables.LoadSceneAsync(address, loadSceneMode));
        }


        public static C2VAsyncOperationHandle<SceneInstance> UnloadSceneAsync(SceneInstance sceneInstance)
        {
            return new C2VAsyncOperationHandle<SceneInstance>(Addressables.UnloadSceneAsync(sceneInstance, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects));
        }


        public static void ReleaseInstance(C2VAsyncOperationHandle asyncOperation) => Addressables.ReleaseInstance(asyncOperation.Handle);

        public static void ReleaseInstance<T>(C2VAsyncOperationHandle<T> asyncOperation) => Addressables.ReleaseInstance(asyncOperation.Handle);

        public static void ReleaseInstance(GameObject gameObject) => Addressables.ReleaseInstance(gameObject);

        public static void Release<T>(T obj) => Addressables.Release(obj);

        public static void Release(C2VAsyncOperationHandle handle) => handle.Release();

        public static void Release<T>(C2VAsyncOperationHandle<T> handle) => handle.Release();


        public static bool AddressableResourceExists<T>(object key)
        {
            foreach (var resourceLocator in Addressables.ResourceLocators)
            {
                if (resourceLocator.Locate(key, typeof(T), out var locs))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
