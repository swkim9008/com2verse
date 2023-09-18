/*===============================================================
* Product:		Com2Verse
* File Name:	MetaverseOptionViewModel_Chat.cs
* Developer:	haminjeong
* Date:			2022-10-24 18:09
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using Com2Verse.Option;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.UI
{
    public partial class MetaverseOptionViewModel
    {
        private ChatOption? _chatOption;

        [UsedImplicitly] public CommandHandler ResetChatSettingsCommand { get; }

        private bool _isChatCommunityOn;
        public bool IsChatCommunityOn
        {
            get => _isChatCommunityOn;
            set
            {
                _isChatCommunityOn = value;
                InvokePropertyValueChanged(nameof(IsChatCommunityOn), IsChatCommunityOn);
            }
        }

        /// <summary>
        /// [System(1)][Area(1)][Nearby(1)][Whisper(1)]
        /// </summary>
        [UsedImplicitly]
        public int ChattingFilter
        {
            get => (int)(_chatOption?.ChattingFilter ?? eChattingFilter.EVERYTHING);
            set
            {
                if (_chatOption == null) return;

                _chatOption.ChattingFilter = (eChattingFilter)value;
                InvokePropertyValueChanged(nameof(ChattingFilter),            ChattingFilter);
                InvokePropertyValueChanged(nameof(IsOnSystemChattingFilter),  IsOnSystemChattingFilter);
                InvokePropertyValueChanged(nameof(IsOnAreaChattingFilter),    IsOnAreaChattingFilter);
                InvokePropertyValueChanged(nameof(IsOnNearbyChattingFilter),  IsOnNearbyChattingFilter);
                InvokePropertyValueChanged(nameof(IsOnWhisperChattingFilter), IsOnWhisperChattingFilter);
            }
        }

        [UsedImplicitly]
        public bool IsOnSystemChattingFilter
        {
            get => (_chatOption?.ChattingFilter & eChattingFilter.SYSTEM) == eChattingFilter.SYSTEM;
            set
            {
                if (_chatOption == null) return;

                if (value) _chatOption.ChattingFilter |= eChattingFilter.SYSTEM;
                else _chatOption.ChattingFilter       &= ~eChattingFilter.SYSTEM;

                InvokePropertyValueChanged(nameof(ChattingFilter),           ChattingFilter);
                InvokePropertyValueChanged(nameof(IsOnSystemChattingFilter), IsOnSystemChattingFilter);
            }
        }

        [UsedImplicitly]
        public bool IsOnAreaChattingFilter
        {
            get => (_chatOption?.ChattingFilter & eChattingFilter.AREA) == eChattingFilter.AREA;
            set
            {
                if (_chatOption == null) return;

                if (value) _chatOption.ChattingFilter |= eChattingFilter.AREA;
                else _chatOption.ChattingFilter       &= ~eChattingFilter.AREA;

                InvokePropertyValueChanged(nameof(ChattingFilter),         ChattingFilter);
                InvokePropertyValueChanged(nameof(IsOnAreaChattingFilter), IsOnAreaChattingFilter);
            }
        }

        [UsedImplicitly]
        public bool IsOnNearbyChattingFilter
        {
            get => (_chatOption?.ChattingFilter & eChattingFilter.NEARBY) == eChattingFilter.NEARBY;
            set
            {
                if (_chatOption == null) return;

                if (value) _chatOption.ChattingFilter |= eChattingFilter.NEARBY;
                else _chatOption.ChattingFilter       &= ~eChattingFilter.NEARBY;

                InvokePropertyValueChanged(nameof(ChattingFilter),           ChattingFilter);
                InvokePropertyValueChanged(nameof(IsOnNearbyChattingFilter), IsOnNearbyChattingFilter);
            }
        }

        [UsedImplicitly]
        public bool IsOnWhisperChattingFilter
        {
            get => (_chatOption?.ChattingFilter & eChattingFilter.WHISPER) == eChattingFilter.WHISPER;
            set
            {
                if (_chatOption == null) return;

                if (value) _chatOption.ChattingFilter |= eChattingFilter.WHISPER;
                else _chatOption.ChattingFilter       &= ~eChattingFilter.WHISPER;

                InvokePropertyValueChanged(nameof(ChattingFilter),            ChattingFilter);
                InvokePropertyValueChanged(nameof(IsOnWhisperChattingFilter), IsOnWhisperChattingFilter);
            }
        }

        [UsedImplicitly]
        public bool IsAutoSimplify
        {
            get => _chatOption?.IsAutoSimplify ?? true;
            set
            {
                if (_chatOption == null) return;

                _chatOption.IsAutoSimplify = value;
                InvokePropertyValueChanged(nameof(IsAutoSimplify), IsAutoSimplify);
            }
        }

        [UsedImplicitly]
        public bool IsOnLinkWarning
        {
            get => _chatOption?.IsOnLinkWarning ?? true;
            set
            {
                if (_chatOption == null) return;

                _chatOption.IsOnLinkWarning = value;
                InvokePropertyValueChanged(nameof(IsOnLinkWarning), IsOnLinkWarning);
            }
        }

        [UsedImplicitly]
        public float ChatTransparentValue
        {
            get => _chatOption?.ChatTransparent ?? 0;
            set
            {
                if (_chatOption == null) return;

                _chatOption.ChatTransparent = Mathf.Clamp(value, ChatTransParentMinValue, ChatTransParentMaxValue);
                InvokePropertyValueChanged(nameof(ChatTransparentValue),  ChatTransparentValue);
                InvokePropertyValueChanged(nameof(ChatTransparentString), ChatTransparentString);
            }
        }

        [UsedImplicitly] public string ChatTransparentString => $"{Localization.Instance.GetString("UI_Common_Percent", $"{ChatTransparentValue:f00}")}";

        [UsedImplicitly] public float ChatTransParentMaxValue => 100f;

        [UsedImplicitly] public float ChatTransParentMinValue => 0f;

        // 현재 ChatRootViewModel의 ChattingBackgroundTransparent로 투명도 조절중입니다.
        // [UsedImplicitly]
        // public float ChatTransparent
        // {
        //     get => Mathf.Clamp((100 - ChatTransparentValue) / ChatTransParentMaxValue, 0, 1f);
        //     set => ChatTransparentValue = 100 - Mathf.Clamp(value * ChatTransParentMaxValue, ChatTransParentMinValue, ChatTransParentMaxValue);
        // }

        [UsedImplicitly]
        public float FontSize
        {
            get => _chatOption?.FontSize ?? 13;
            set
            {
                if (_chatOption == null) return;

                _chatOption.FontSize = Mathf.Clamp(value, FontSizeMinValue, FontSizeMaxValue);
                InvokePropertyValueChanged(nameof(FontSize),       FontSize);
                InvokePropertyValueChanged(nameof(FontSizeString), FontSizeString);
            }
        }

        [UsedImplicitly] public string FontSizeString => $"{Localization.Instance.GetString("UI_Common_FontSize", $"{FontSize:f00}")}";

        [UsedImplicitly] public int FontSizeMaxValue => 20;

        [UsedImplicitly] public int FontSizeMinValue => 12;

        [UsedImplicitly]
        public bool IsOnTimeStamp
        {
            get => _chatOption?.IsOnTimeStamp ?? true;
            set
            {
                if (_chatOption == null) return;

                _chatOption.IsOnTimeStamp = value;
                InvokePropertyValueChanged(nameof(IsOnTimeStamp), IsOnTimeStamp);
            }
        }

        /// <summary>
        /// [Nearby(1)][Area(1)]
        /// </summary>
        [UsedImplicitly]
        public int ShowBubbleOption
        {
            get => (int)(_chatOption?.ShowBubbleOption ?? eChattingBubbleFilter.EVERYTHING);
            set
            {
                if (_chatOption == null) return;

                _chatOption.ShowBubbleOption = (eChattingBubbleFilter)value;
                InvokePropertyValueChanged(nameof(ShowBubbleOption), ShowBubbleOption);
                InvokePropertyValueChanged(nameof(IsOnNearbyBubble), IsOnNearbyBubble);
                InvokePropertyValueChanged(nameof(IsOnAreaBubble),   IsOnAreaBubble);
            }
        }

        [UsedImplicitly]
        public bool IsOnNearbyBubble
        {
            get => (_chatOption?.ShowBubbleOption & eChattingBubbleFilter.NEARBY) == eChattingBubbleFilter.NEARBY;
            set
            {
                if (_chatOption == null) return;

                if (value) _chatOption.ShowBubbleOption |= eChattingBubbleFilter.NEARBY;
                else _chatOption.ShowBubbleOption       &= ~eChattingBubbleFilter.NEARBY;

                InvokePropertyValueChanged(nameof(ShowBubbleOption), ShowBubbleOption);
                InvokePropertyValueChanged(nameof(IsOnNearbyBubble), IsOnNearbyBubble);
            }
        }

        [UsedImplicitly]
        public bool IsOnAreaBubble
        {
            get => (_chatOption?.ShowBubbleOption & eChattingBubbleFilter.AREA) == eChattingBubbleFilter.AREA;
            set
            {
                if (_chatOption == null) return;

                if (value) _chatOption.ShowBubbleOption |= eChattingBubbleFilter.AREA;
                else _chatOption.ShowBubbleOption       &= ~eChattingBubbleFilter.AREA;

                InvokePropertyValueChanged(nameof(ShowBubbleOption), ShowBubbleOption);
                InvokePropertyValueChanged(nameof(IsOnAreaBubble),   IsOnAreaBubble);
            }
        }

        private void ResetChatSettings()
        {
            if (_chatOption != null)
            {
                _chatOption.Reset();
                SetChatSettingProperties(_chatOption);
            }
        }

        private void InitializeChatOption()
        {
            _isChatCommunityOn = false;
            _chatOption        = OptionController.Instance.GetOption<ChatOption>();

            if (_chatOption != null)
                SetChatSettingProperties(_chatOption);
            RefreshReadOnlyProperties();
        }

        private void RefreshReadOnlyProperties()
        {
            InvokePropertyValueChanged(nameof(ChatTransParentMaxValue), ChatTransParentMaxValue);
            InvokePropertyValueChanged(nameof(ChatTransParentMinValue), ChatTransParentMinValue);
            InvokePropertyValueChanged(nameof(FontSizeMaxValue),        FontSizeMaxValue);
            InvokePropertyValueChanged(nameof(FontSizeMinValue),        FontSizeMinValue);
        }

        private void SetChatSettingProperties(ChatOption chatOption)
        {
            ChattingFilter       = (int)chatOption.ChattingFilter;
            IsAutoSimplify       = chatOption.IsAutoSimplify;
            IsOnLinkWarning      = chatOption.IsOnLinkWarning;
            ChatTransparentValue = chatOption.ChatTransparent;
            FontSize             = chatOption.FontSize;
            IsOnTimeStamp        = chatOption.IsOnTimeStamp;
            ShowBubbleOption     = (int)chatOption.ShowBubbleOption;
        }
    }
}
