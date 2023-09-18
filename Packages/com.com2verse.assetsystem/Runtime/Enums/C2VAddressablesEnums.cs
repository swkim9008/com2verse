/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesGlobalEnums.cs
* Developer:	tlghks1009
* Date:			2023-03-28 10:46
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;

namespace Com2Verse.AssetSystem
{
	public enum eAssetBundleType
	{
		NONE = 0,
		BUILT_IN = 1,
		OFFICE = 2,
		MICE = 3,
		WORLD = 4,
		COMMON = 5
	}


	public enum eRequestAssetBundleType
	{
		LOGIN,
		OFFICE,
		WORLD,
		MICE
	}
}
