/*===============================================================
* Product:		Com2Verse
* File Name:	MiceInfoManager_Notice.cs
* Developer:	klizzard
* Date:			2023-07-14 15:35
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Mice
{
	public sealed partial class MiceInfoManager
	{
		public Dictionary<int, MiceNoticeInfo> NoticeInfos = new();

		public async UniTask SyncNoticeInfo()
		{
			NoticeInfos.Clear();

			var result = await MiceWebClient.Notice.NoticeGet(Localization.Instance.CurrentLanguage.ToMiceLanguageCode());
			if (result)
			{
				var datas = result.Data;
				foreach (var entry in datas)
				{
					if (NoticeInfos.TryGetValue(entry.BoardSeq, out MiceNoticeInfo value))
						value.Sync(entry);
					else
						NoticeInfos.Add(entry.BoardSeq, new MiceNoticeInfo(entry));

					NoticeClickedPrefs.Load(entry.BoardSeq);
				}
			}

			await UniTask.CompletedTask;
		}
	}
}
