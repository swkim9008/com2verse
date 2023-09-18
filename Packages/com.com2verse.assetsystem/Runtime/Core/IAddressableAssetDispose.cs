/*===============================================================
* Product:		Com2Verse
* File Name:	IAddressableAssetDispose.cs
* Developer:	tlghks1009
* Date:			2023-03-16 16:06
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.AssetSystem
{
	public interface IAddressableAssetDisposable
	{
		void ReleaseAddressableAssetInstance();

		void ReleaseAddressableAsset();
	}
}
