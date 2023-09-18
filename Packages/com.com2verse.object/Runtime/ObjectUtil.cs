/*===============================================================
* Product:		Com2Verse
* File Name:	ObjectUtil.cs
* Developer:	haminjeong
* Date:			2023-07-06 16:04
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;

namespace Com2Verse.Network
{
	public sealed class ObjectUtil
	{
		private static TableBaseObject _tableBaseObject;

		public static void LoadTable()
		{
			if (_tableBaseObject != null) return;
			_tableBaseObject = TableDataManager.Instance.Get<TableBaseObject>();
		}

		public static eObjectType GetObjectType(long objectID)
		{
			if (!_tableBaseObject.Datas.TryGetValue(objectID, out var baseObject)) return eObjectType.OBJECT;
			return baseObject.ObjectType;
		}
	}
}
