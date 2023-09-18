/*===============================================================
* Product:		Com2Verse
* File Name:	Enviroment.cs
* Developer:	pjhara
* Date:			2023-03-20 19:08
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.BuildHelper
{
	public enum eBuildEnv
	{
		DEV,
		QA,
		STAGING,
		PRODUCTION,
		DEV_INTEGRATION,
	};

	public enum eAssetBuildType
	{
		LOCAL,         // Default (bundle include)
		REMOTE,        // Real (cdn)
		REMOTE_TEST,   // Test (cdn)
		EDITOR_HOSTED, // local remote test
	}

	public enum ePackageType
	{
		ZIP,
		MSI,
		LAUNCHER,
		ASSET,
		NOT,
	}

	public enum eHiveEnvType
	{
		NOT,
		SANDBOX,
		LIVE
	}
}