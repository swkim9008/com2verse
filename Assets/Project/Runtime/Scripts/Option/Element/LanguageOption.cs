/*===============================================================
* Product:		Com2Verse
* File Name:	LanguageOption.cs
* Developer:	tlghks1009
* Date:			2022-12-13 10:08
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.Option;
using Protocols.CommonLogic;
using UnityEngine;

namespace Com2Verse.UI
{
    [Serializable] [MetaverseOption("LanguageOption")]
    public sealed class LanguageOption : BaseMetaverseOption
    {
        [SerializeField] private int _languageIndex = (int)Localization.eLanguage.UNKNOWN;

        public int LanguageIndex
        {
            get => _languageIndex;
            set
            {
                if (_languageIndex != value)
                {
                    _languageIndex = value;
                    SetLanguage(value);
                    SaveData();
                    OptionController.Instance.RequestStoreLanguageOption(value);
                }
            }
        }

        public override void OnInitialize()
        {
            base.OnInitialize();

            if (_languageIndex == (int)Localization.eLanguage.UNKNOWN)
            {
                _languageIndex = (int)GetCurrentDeviceLanguage();
                C2VDebug.LogCategory("OptionController", "Set System Language");
            }
        }

        public override void Apply()
        {
            base.Apply();

            var language = GetLanguage();
            if (_languageIndex != (int)language)
            {
                C2VDebug.LogCategory("OptionController", "Set Local Save Language");
                Localization.Instance.ChangeLanguage(language);
            }
        }

        public Localization.eLanguage GetLanguage() => (Localization.eLanguage)LanguageIndex;

        private void SetLanguage(int index)
        {
#if !UNITY_EDITOR
            hive.Configuration.updateGameLanguage(GetHiveLanguageCode(index));
#endif
            Localization.Instance.ChangeLanguage((Localization.eLanguage)index);
        }

        public override void SetStoredOption(SettingValueResponse response)
        {
            if (response == null)
            {
                C2VDebug.LogErrorCategory("OptionController", "Setting Value Response is NULL");
                return;
            }

            if (LoginManager.Instance.NeedLanguageSave)
            {
                SaveData();
                OptionController.Instance.RequestInitLanguageOption(_languageIndex, response.AlramCount);
            }
            else
            {
                _languageIndex = response.Language;
                SetLanguage(_languageIndex);
                SaveData();
            }
        }

#region Utils
        private string GetHiveLanguageCode(int index)
        {
            return index switch
            {
                0 => "ko",
                1 => "en",
                _ => "ko",
            };
        }

        private Localization.eLanguage GetCurrentDeviceLanguage()
        {
            var systemLanguage = Application.systemLanguage;

            return systemLanguage switch
            {
                SystemLanguage.Korean  => Localization.eLanguage.KOR,
                SystemLanguage.English => Localization.eLanguage.ENG,
                _                      => Localization.eLanguage.KOR
            };
        }
#endregion
    }
}
