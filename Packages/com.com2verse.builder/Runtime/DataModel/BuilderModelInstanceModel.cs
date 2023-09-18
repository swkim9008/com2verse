// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	BuilderAssetDataModel.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-13 오후 12:39
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using UnityEngine;

namespace Com2Verse.Builder
{
	public class BuilderModelInstanceModel
	{
		public GameObject LoadedAsset { get; set; }
		public Bounds Bound { get; set; }
		public Material LoadedMaterial { get; set; }
	}
}
