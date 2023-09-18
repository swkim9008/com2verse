/*===============================================================
* Product:		Com2Verse
* File Name:	SmallTalkData.cs
* Developer:	eugene9721
* Date:			2022-10-20 11:39
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Data;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;

namespace Com2Verse.SmallTalk
{
	public class BlobData
	{
		public event Action<BlobData>? BlobDataLoaded;

#region TableData
		public float MaxHeadRadius { get; private set; }
		public float MaxBodyRadius { get; private set; }

		public bool  SoftColloidMovement  { get; private set; }
		public float SoftColloidMoveSpeed { get; private set; }

		public float BlobClearTimeout { get; private set; } = 3000f;

		public TableCommunicationBlob? Table { get; private set; }
#endregion TableData

#region Table
		public void LoadTable()
		{
			Table = TableDataManager.Instance.Get<TableCommunicationBlob>();
			if (Table == null)
			{
				C2VDebug.LogError("TableCommunicationBlob is null");
				BlobDataLoaded?.Invoke(this);
				return;
			}

			UpdateFallbackValues(Table);
			BlobDataLoaded?.Invoke(this);
		}

		private void UpdateFallbackValues(TableCommunicationBlob table)
		{
			var data = table.Datas?[Utils.Define.DEFAULT_TABLE_INDEX];
			if (data == null)
			{
				C2VDebug.LogError("TableCommunicationBlob data of default index is null");
				return;
			}

			MaxHeadRadius = data.MaxHeadRadius;
			MaxBodyRadius = data.MaxBodyRadius;

			SoftColloidMovement  = data.SoftColloidMovement;
			SoftColloidMoveSpeed = data.SoftColloidMoveSpeed;

			BlobClearTimeout = data.BlobClearTimeout;
		}
#endregion Table
	}
}
