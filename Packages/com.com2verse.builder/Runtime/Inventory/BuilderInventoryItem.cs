// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	BuilderInventoryItem.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-08 오전 10:53
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using UnityEngine;

namespace Com2Verse.Builder
{
	public enum eInventoryItemCategory
	{
		NONE = -1,
		OBJECT,
		TEXTURE
	}
	
	public class BuilderInventoryItem
	{
		public eInventoryItemCategory Category { get; set; }
		public int StackLevel { get; set; }
		public long BaseObjectId { get; set; }
		public string AddressableId { get; set; }
		public Sprite Thumbnail { get; set; }

		public BuilderModelInstanceModel Instance
		{
			get
			{
				_instance ??= BuilderAssetManager.Instance.GetAsset(Category, BaseObjectId);

				return _instance;
			}
		}
		private BuilderModelInstanceModel _instance;
	}
}
