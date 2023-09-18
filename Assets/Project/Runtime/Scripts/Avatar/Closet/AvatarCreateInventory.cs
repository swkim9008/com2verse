/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarCreateInventory.cs
* Developer:	eugene9721
* Date:			2023-04-19 12:31
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

namespace Com2Verse.Avatar
{
	public sealed class AvatarCreateInventory : AvatarInventoryBase
	{
		/// <summary>
		/// 아바타 선택씬 진입시 AvatarManager의 테이블 데이터를 받아 저장
		/// </summary>
		/// <returns>테이블 데이터가 로드되지 않은 경우 false</returns>
		public override bool Initialize() => InitializeByTable();
	}
}
