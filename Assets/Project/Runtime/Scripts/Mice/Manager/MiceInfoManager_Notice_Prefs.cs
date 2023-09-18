/*===============================================================
* Product:		Com2Verse
* File Name:	MiceInfoManager_Notice_Prefs.cs
* Developer:	klizzard
* Date:			2023-08-01 16:46
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.Mice
{
    public sealed partial class MiceInfoManager
    {
        public ClickedPrefs<int, int> NoticeClickedPrefs { get; } = new()
        {
            OnGetJsonKey = (key) => key / 1000,
            OnGetPrefsKey = () => "NoticeClicked",
            OnGetUpdateTime = (key) =>
                Instance.NoticeInfos.TryGetValue(key, out var noticeInfo)
                    ? noticeInfo.NoticeEntity.UpdateDatetime
                    : default
        };
    }
}