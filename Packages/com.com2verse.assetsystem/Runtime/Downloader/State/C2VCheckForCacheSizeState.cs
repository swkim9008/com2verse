/*===============================================================
* Product:		Com2Verse
* File Name:	C2VCheckForCacheSizeState.cs
* Developer:	tlghks1009
* Date:			2023-07-12 11:50
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/


namespace Com2Verse.AssetSystem
{
	public sealed class C2VCheckForCacheSizeState : C2VAddressablesStateHandler<C2VAddressablesDownloader>
	{
		private C2VDownloadAssetProviderOperation.AssetBundleCollection _assetBundleCollection;

		public C2VCheckForCacheSizeState(C2VDownloadAssetProviderOperation.AssetBundleCollection assetBundleCollection) => _assetBundleCollection = assetBundleCollection;

		public override void OnStateEnter()
		{
			base.OnStateEnter();

			var result = C2VCaching.CheckCachingAvailability(_assetBundleCollection.TotalSize);

			if (!result)
			{
				Downloader.RaiseAssetBundleDownloadResponseCode(eResponseCode.CACHE_SPACE_ERROR);
				StateMachine.Dispose();

				return;
			}

			MoveToNextState();
		}


		private void MoveToNextState()
		{
			StateMachine.ChangeState(new C2VDownloadDependenciesAsyncState(_assetBundleCollection));
		}
	}
}
