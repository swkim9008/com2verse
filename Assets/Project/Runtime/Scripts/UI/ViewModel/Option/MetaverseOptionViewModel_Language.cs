/*===============================================================
* Product:		Com2Verse
* File Name:	MetaverseOptionViewModel_Language.cs
* Developer:	tlghks1009
* Date:			2022-12-13 11:03
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.Option;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	public partial class MetaverseOptionViewModel
	{
		private static string UI_Setting_Desc_Language_100002 => Localization.Instance.GetString("UI_Setting_Desc_Language_100002");
		private static string UI_Setting_Desc_Language_100003 => Localization.Instance.GetString("UI_Setting_Desc_Language_100003");
		
		private LanguageOption _languageOption;
		
		private List<string> _languageOptionList = new();
		private int _selectedLanguageIndex;


		public List<string> LanguageOptionList
		{
			get
			{
				if (_languageOptionList.Count == 0)
				{
					RegisterLanguage();
				}
				return _languageOptionList;
			}
		}


		[UsedImplicitly]
		public int SelectedLanguageIndex
		{
			get => _selectedLanguageIndex;
			set
			{
				if (_selectedLanguageIndex != value)
				{
					_selectedLanguageIndex = value;

					_languageOption.LanguageIndex = value;
				}

				base.InvokePropertyValueChanged(nameof(SelectedLanguageIndex), SelectedLanguageIndex);
			}
		}

		private void InitializeLanguage()
		{
			_languageOption = OptionController.Instance.GetOption<LanguageOption>();

			SelectedLanguageIndex = _languageOption.LanguageIndex;
		}


		private void RegisterLanguage()
		{
			_languageOptionList.Clear();

			_languageOptionList.Add(UI_Setting_Desc_Language_100002);
			_languageOptionList.Add(UI_Setting_Desc_Language_100003);
		}

		private void LanguageChangeLanguageOption()
		{
			RegisterLanguage();
			base.InvokePropertyValueChanged(nameof(LanguageOptionList), LanguageOptionList);
		}
	}
}
