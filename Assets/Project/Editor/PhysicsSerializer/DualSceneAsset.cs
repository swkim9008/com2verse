/*===============================================================
* Product:		Com2Verse
* File Name:	DualSceneAsset.cs
* Developer:	yangsehoon
* Date:			2023-04-18 16:00
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEditor;
using UnityEngine;

namespace Com2VerseEditor
{
	[Serializable]
	public class DualSceneAsset
	{
		public SceneAsset BaseScene;
		public SceneAsset ServerObjectScene;
	}
}
