/*===============================================================
* Product:		Com2Verse
* File Name:	MetaverseOptionViewModel_Graphics.cs
* Developer:	tlghks1009
* Date:			2022-10-04 15:59
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.AssetSystem;
using Com2Verse.Logger;
using Com2Verse.Option;
using Com2Verse.PlatformControl;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.UI
{
    public partial class MetaverseOptionViewModel
    {
        // TODO : 사이즈 논의 필요
        private readonly List<string> _resolutionList = new List<string>();
        
        // TODO : 키값 확인 필요
        private readonly List<string> _optionList = new();
        private static string QualityLow => Localization.Instance.GetString("UI_Setting_Effect_Quality_Low");
        private static string QualityMedium => Localization.Instance.GetString("UI_Setting_Effect_Quality_Medium");
        private static string QualityHigh => Localization.Instance.GetString("UI_Setting_Effect_Quality_High");
        
        
        private ResolutionOption _resolutionOption;
        private GraphicsOption _graphicsOption;
        public CommandHandler GraphicTabClicked { get; private set; }
        public CommandHandler ResetGraphicOptionButtonClicked { get; private set; }
        public CommandHandler<int> ScreenModeToggleClicked { get; private set; }
        public CommandHandler<bool> MotionBlurToggleClicked { get; private set; }
        public CommandHandler<bool> ObjectTransparentToggleClicked { get; private set; }
        
        public bool IsFullScreen
        {
            get => _resolutionOption.ScreenModeIndex != 0;
            set => C2VDebug.LogWarningCategory("GraphicOption", "Can't set ScreenModeType");
        }

        [UsedImplicitly]
        public int ScreenModeIndex
        {
            get => _resolutionOption.ScreenModeIndex;
            set
            {
                if (_resolutionOption == null) return;
                _resolutionOption.ScreenModeIndex = value;
                base.InvokePropertyValueChanged(nameof(IsFullScreen), IsFullScreen);
                base.InvokePropertyValueChanged(nameof(ScreenModeIndex), ScreenModeIndex);
            }
        }       
        
        public List<string> ResolutionLevelNames
        {
            get => _resolutionList;
            set { }
        }
        
        public List<string> QualityLevelNames
        {
            get
            {
                if (_optionList.Count == 0)
                {
                    RegisterGraphicOption();
                }
                return _optionList;
            }
        }

        [UsedImplicitly]
        public bool IsValidateQualityLevel
        {
            get => _graphicsOption.IsValidateQualityLevel;
            set => base.InvokePropertyValueChanged(nameof(QualityLevel), QualityLevel);
        }
        
        [UsedImplicitly]
        public int ResolutionLevel
        {
            get => _resolutionOption.ResolutionIndex;
            set
            {
                if (value >= ResolutionLevelNames?.Count)
                {
                    _resolutionOption.ResolutionIndex = ResolutionLevelNames.Count - 1;
                }
                else
                {
                    _resolutionOption.ResolutionIndex = value;
                }

                base.InvokePropertyValueChanged(nameof(ResolutionLevel), ResolutionLevel);
            }
        }
        
        [UsedImplicitly]
        public int QualityLevel
        {
            get => _graphicsOption.QualityLevel;
            set
            {
                _graphicsOption.QualityLevel = value;

                base.InvokePropertyValueChanged(nameof(QualityLevel), QualityLevel);
            }
        }
        
        private void InitializeGraphicsOption()
        {
            PlatformController.Instance.AddEvent(eApplicationEventType.CHANGE_SCREEN_MODE, OnScreenModeToggleEvent);

            _resolutionOption = OptionController.Instance.GetOption<ResolutionOption>();
            _graphicsOption = OptionController.Instance.GetOption<GraphicsOption>();
            _resolutionOption.CancelEvent = OnScreenResolutionReverted;

            GraphicTabClicked = new CommandHandler(CheckGraphicsOption);
            ResetGraphicOptionButtonClicked = new CommandHandler(OnResetGraphicOptionButtonClicked);
            ScreenModeToggleClicked = new CommandHandler<int>(OnScreenModeToggleClicked);
            MotionBlurToggleClicked = new CommandHandler<bool>(OnMotionBlurToggleClicked);
            ObjectTransparentToggleClicked = new CommandHandler<bool>(OnObjectTransparentToggleClicked);

            var lastWidth = 0;
            var maxResolutionIndex = Screen.resolutions.Length;
            for (int i = 0; i < maxResolutionIndex; i++)
            {
                // if (Screen.resolutions[i].width == Screen.resolutions[maxResolutionIndex].width) continue;
                if (Screen.resolutions[i].width == lastWidth) continue;
                
                lastWidth = Screen.resolutions[i].width;
                _resolutionList.Insert(0, $"{Screen.resolutions[i].width} * {Screen.resolutions[i].height}");
            }
        }

        private void LanguageChangeGraphicOption()
        {
            RegisterGraphicOption();
            base.InvokePropertyValueChanged(nameof(QualityLevelNames), QualityLevelNames);
        }

        private void DisposeGraphicOption()
        {
            if (PlatformController.InstanceExists)
                PlatformController.Instance.RemoveEvent(eApplicationEventType.CHANGE_SCREEN_MODE, OnScreenModeToggleEvent);
        }

        private void CheckGraphicsOption()
        {
            ScreenModeIndex = (Screen.fullScreen) ? 0 : 1;
        }

        private void OnResetGraphicOptionButtonClicked()
        {
            if (_resolutionOption != null)
            {
                ScreenModeIndex = 0;
                ResolutionLevel = 0;
            }

            if (_graphicsOption != null)
            {
                QualityLevel = 0;
            }
        }

        private void OnScreenModeToggleClicked(int index)
        {
            if (ScreenModeIndex != index) ScreenModeIndex = index;
        }
        
        private void OnScreenModeToggleEvent(bool isFullScreen)
        {
            if (_resolutionOption == null) return;
            _resolutionOption.OnScreenModeToggleEvent(isFullScreen);
            base.InvokePropertyValueChanged(nameof(IsFullScreen),    IsFullScreen);
            base.InvokePropertyValueChanged(nameof(ScreenModeIndex), ScreenModeIndex);
        }

        private void OnScreenResolutionReverted(int index)
        {
            if (ResolutionLevel != index) ResolutionLevel = index;
        }

        private void OnMotionBlurToggleClicked(bool isOn)
        {
            C2VDebug.LogCategory("GraphicOption", $"{nameof(OnMotionBlurToggleClicked)} : {isOn}");
        }
        
        private void OnObjectTransparentToggleClicked(bool isOn)
        {
            C2VDebug.LogCategory("GraphicOption", $"{nameof(OnObjectTransparentToggleClicked)} : {isOn}");
        }

        private void RegisterGraphicOption()
        {
            _optionList.Clear();
            _optionList.Add(QualityHigh);
            _optionList.Add(QualityMedium);
            _optionList.Add(QualityLow);
        }
    }
}
