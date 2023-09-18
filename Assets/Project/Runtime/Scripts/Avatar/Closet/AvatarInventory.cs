/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarInventory.cs
* Developer:	eugene9721
* Date:			2023-04-19 12:30
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

namespace Com2Verse.Avatar
{
	/// <summary>
	/// TODO: 아바타 생성경로 추가시 아이템 리스트 변경
	/// </summary>
	public sealed class AvatarInventory : AvatarInventoryBase
	{
		public override bool Initialize() => InitializeByTable();
	}
}
