using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace Com2Verse.AssetSystem
{
    public class C2VLoadContentCatalogAsyncState : C2VAddressablesStateHandler<C2VAddressablesDownloader>
    {
        public override void OnStateEnter()
        {
            base.OnStateEnter();

            LoadContentCatalogAsync();
        }


        private void LoadContentCatalogAsync()
        {
            Addressables.ClearResourceLocators();

            var appVersion     = Downloader.LoadInfo.AppVersion;
            var bundleVersion  = Downloader.LoadInfo.BundleVersion;
            var buildTarget    = Downloader.LoadInfo.BuildTarget;
            var appEnvironment = Downloader.LoadInfo.AppEnvironment;
            var catalogName    = $"catalog_Com2Verse_{bundleVersion}.json";
            var catalogPath    = $"{C2VPaths.RemoteAssetBundleUrl}/{appEnvironment}/{buildTarget}/{bundleVersion}/{catalogName}";

            var handle = Downloader.LoadContentCatalogAsync(catalogPath, false);

            handle.OnCompleted += OnLoadContentCatalogCompleted;
        }


        private void OnLoadContentCatalogCompleted(C2VAsyncOperationHandle<IResourceLocator> handle)
        {
            handle.OnCompleted -= OnLoadContentCatalogCompleted;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                MoveToNextState();
            }
            else
            {
                StateMachine.Dispose();
            }

            handle.Release();
        }


        private void MoveToNextState()
        {
            StateMachine.ChangeState(new C2VCheckForCatalogUpdateState());
        }
    }
}
