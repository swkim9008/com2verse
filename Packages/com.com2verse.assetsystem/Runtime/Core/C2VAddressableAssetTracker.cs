/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressableAssetTracker.cs
* Developer:	tlghks1009
* Date:			2023-03-16 14:14
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.AssetSystem
{
	public sealed class C2VAddressableAssetTracker : MonoBehaviour
	{
		private void OnDestroy()
		{
			C2VAddressables.ReleaseInstance(this.gameObject);
		}
	}
}
