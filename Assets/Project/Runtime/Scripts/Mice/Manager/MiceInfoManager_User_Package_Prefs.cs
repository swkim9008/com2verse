/*===============================================================
* Product:		Com2Verse
* File Name:	MiceInfoManager_User_Package_Prefs.cs
* Developer:	klizzard
* Date:			2023-08-02 10:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.Mice
{
    public sealed partial class MiceInfoManager
    {
        public ClickedPrefs<string, int> MyPackageClickedPrefs { get; } = new()
        {
            OnGetJsonKey = (key) => key.GetHashCode() % 10,
            OnGetPrefsKey = () => "MyPackageClicked",
            OnGetUpdateTime = (key) => default
        };
    }
}