/*===============================================================
* Product:		Com2Verse
* File Name:	ILocalizationUI.cs
* Developer:	tlghks1009
* Date:			2022-12-14 10:57
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.UI
{
	public interface ILocalizationUI
	{
		void OnLanguageChanged();

		public void InitializeLocalization()
		{
			Localization.Instance.AddLocalizationUI(this);
		}

		public void ReleaseLocalization()
		{
			Localization.InstanceOrNull?.RemoveLocalizationUI(this);
		}
	}
}
