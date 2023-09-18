/*===============================================================
* Product:		Com2Verse
* File Name:	OfficeInfo.cs
* Developer:	haminjeong
* Date:			2022-12-09 11:16
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Data;
using JetBrains.Annotations;

namespace Com2Verse
{
	public sealed class OfficeInfo : Singleton<OfficeInfo>, IDisposable
	{
		private TableOfficeFloorInfo _tableOfficeFloorInfo;
		
		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private OfficeInfo()
		{
		}

		public void Initialize()
		{
			LoadTable();
		}

		public void Dispose()
		{
			_tableOfficeFloorInfo = null;
		}

		private void LoadTable()
		{
			_tableOfficeFloorInfo = TableDataManager.Instance.Get<TableOfficeFloorInfo>();
		}

		public OfficeFloorInfo GetFloorInfo(long mapID)
		{
			if (_tableOfficeFloorInfo == null) return null;
			if (!_tableOfficeFloorInfo.Datas.TryGetValue(mapID, out var value))
				return null;
			return value;
		}
	}
}
