/*===============================================================
* Product:		Com2Verse
* File Name:	C2VCheckForCatalogUpdateState.cs
* Developer:	tlghks1009
* Date:			2023-02-17 17:20
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Com2Verse.AssetSystem
{
    public class C2VCheckForCatalogUpdateState : C2VAddressablesStateHandler<C2VAddressablesDownloader>
    {
        public override void OnStateEnter()
        {
            base.OnStateEnter();

            CheckForCatalogUpdatesAsync();
        }


        private void CheckForCatalogUpdatesAsync()
        {
            var handle = Downloader.CheckForCatalogUpdate(false);

            handle.OnCompleted += OnCheckForCatalogUpdateCompleted;
        }


        private void OnCheckForCatalogUpdateCompleted(C2VAsyncOperationHandle<List<string>> handle)
        {
            handle.OnCompleted -= OnCheckForCatalogUpdateCompleted;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                MoveToNextState(handle.Result);
            }
            else
            {
                Downloader.RaiseAssetBundleDownloadResponseCode(eResponseCode.CATALOG_UPDATE_ERROR);
                StateMachine.Dispose();
            }

            handle.Release();
        }


        private void MoveToNextState(List<string> catalogs)
        {
            StateMachine.ChangeState(new C2VUpdateCatalogState(catalogs));
        }
    }
}
