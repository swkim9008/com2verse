/*===============================================================
* Product:		Com2Verse
* File Name:	GeneralData.cs
* Developer:	haminjeong
* Date:			2023-07-19 10:07
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Cysharp.Threading.Tasks;

namespace Com2Verse.Data
{
	public static class GeneralData
	{
		public static General General { get; private set; }

		public static void Initialize() => LoadGeneralTable();

		private static void LoadGeneralTable()
		{
			var generalTable = TableDataManager.Instance.Get<TableGeneral>();
			if (generalTable is not { Datas: { } } || generalTable.Datas.Count == 0) return;
			General = generalTable.Datas[0];
		}
	}
}
