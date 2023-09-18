/*===============================================================
* Product:		Com2Verse
* File Name:	CheatKey_LruObjectPool.cs
* Developer:	NGSG
* Date:			2023-07-27 11:53
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Cheat;
using Com2Verse.LruObjectPool;
using UnityEngine;

namespace Com2Verse
{
	public static partial class CheatKey
	{
#region RunTimeObjectManagerTest
		[MetaverseCheat("Cheat/RunTimeObjectManager/TestNewAsync")]
		private static void TestNewAsync()
		{
			RuntimeObjectManager.Instance.TestNewAsync().Forget();
			//Resources.UnloadUnusedAssets();
			UnityEngine.Debug.Log("[LRUPool] TestNewAsync");
		}

		[MetaverseCheat("Cheat/RunTimeObjectManager/TestInstantiate")]
		private static void TestInstantiate()
		{
			RuntimeObjectManager.Instance.TestInstantiate();
			//GC.Collect();
			UnityEngine.Debug.Log("[LRUPool] TestInstantiate");
		}
#endregion
	}
}
