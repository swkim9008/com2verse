/*===============================================================
* Product:		Com2Verse
* File Name:	MiceNoticeInfo.cs
* Developer:	klizzard
* Date:			2023-07-14 15:36
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;

namespace Com2Verse.Mice
{
	public sealed partial class MiceNoticeInfo : MiceBaseInfo
	{
		public MiceWebClient.Entities.NoticeBoardEntity NoticeEntity { get; private set; }

		public MiceNoticeInfo(MiceWebClient.Entities.NoticeBoardEntity noticeEntity)
		{
			Sync(noticeEntity);
		}

		public void Sync(MiceWebClient.Entities.NoticeBoardEntity noticeEntity)
		{
			NoticeEntity = noticeEntity;
		}
	}
}

namespace Com2Verse.Mice
{
	public static partial class MiceWebClient
	{
		public static partial class Entities
		{
			public partial class NoticeBoardEntity
			{
				public string StrArticeType
				{
					get
					{
						//@SerializedName("1") ALL(1),
						//@SerializedName("2") INTEGRATED(2),
						//@SerializedName("3") NORMAL(3);
						switch (ArticleType)
						{
							case 1:
							case 2:
								return Data.Localization.eKey.MICE_UI_NormalKiosk_Notice_Type_all.ToLocalizationString();
							case 3:
								return Data.Localization.eKey.MICE_UI_NormalKiosk_Notice_Type_Normal.ToLocalizationString();
						}
						return ArticleType.ToString();
					}
				}
			}
		}
	}
}
