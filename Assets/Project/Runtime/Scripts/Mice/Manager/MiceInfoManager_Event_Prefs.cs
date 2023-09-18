/*===============================================================
* Product:		Com2Verse
* File Name:	MiceInfoManager_Event_Prefs.cs
* Developer:	klizzard
* Date:			2023-08-02 15:38
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.Mice
{
	public sealed partial class MiceInfoManager
	{
		public ClickedPrefs<long, long> EventIntroSkipClickedPrefs { get; } = new()
		{
			OnGetJsonKey    = (key) => key / 1000L,
			OnGetPrefsKey   = () => "EventIntroSkipClicked",
			OnGetUpdateTime = (key) => default
		};

		public ClickedPrefs<long, long> EventTutorialSkipClickedPrefs { get; } = new()
		{
			OnGetJsonKey    = (key) => key / 1000L,
			OnGetPrefsKey   = () => "EventTutorialSkipClicked",
			OnGetUpdateTime = (key) => default
		};
	}
}
