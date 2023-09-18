/*===============================================================
* Product:		Com2Verse
* File Name:	ChatOption.cs
* Developer:	haminjeong
* Date:			2022-10-24 18:06
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.Utils;
using Protocols.CommonLogic;
using UnityEngine;

namespace Com2Verse.Option
{
	[Flags]
	public enum eChattingFilter
	{
		NONE       = 0,
		EVERYTHING = ~NONE,
		SYSTEM     = 1 << 0,
		AREA       = 1 << 1,
		NEARBY     = 1 << 2,
		WHISPER    = 1 << 3,
	}

	[Flags]
	public enum eChattingBubbleFilter
	{
		NONE       = 0,
		EVERYTHING = ~NONE,
		NEARBY     = 1 << 0,
		AREA       = 1 << 1,
	}

	[Serializable] [MetaverseOption("ChatOption")]
	public class ChatOption : BaseMetaverseOption
	{
#region Fields
		[SerializeField] private bool _isInitialized = false;

		[SerializeField] private eChattingFilter       _chattingFilter   = eChattingFilter.EVERYTHING;
		[SerializeField] private eChattingBubbleFilter _showBubbleOption = eChattingBubbleFilter.EVERYTHING;

		[SerializeField] private bool  _isAutoSimplify  = false;
		[SerializeField] private bool  _isOnLinkWarning = true;
		[SerializeField] private float _chatTransparent = 50;
		[SerializeField] private float _fontSize        = 13;
		[SerializeField] private bool  _isOnTimeStamp   = true;
#endregion Fields

#region Properties
		public eChattingFilter ChattingFilter
		{
			get => _chattingFilter;
			set
			{
				_chattingFilter = value;
				OnChattingFilterChanged?.Invoke(value);
			}
		}

		public eChattingBubbleFilter ShowBubbleOption
		{
			get => _showBubbleOption;
			set
			{
				_showBubbleOption = value;
				OnChattingBubbleFilterChanged?.Invoke(value);
			}
		}

		public bool IsAutoSimplify
		{
			get => _isAutoSimplify;
			set
			{
				_isAutoSimplify = value;
				OnAutoSimplifyChanged?.Invoke(value);
			}
		}

		public bool IsOnLinkWarning
		{
			get => _isOnLinkWarning;
			set
			{
				_isOnLinkWarning = value;
				OnLinkWarningChanged?.Invoke(value);
			}
		}

		public float ChatTransparent
		{
			get => _chatTransparent;
			set
			{
				_chatTransparent = value;
				OnChatTransparentChanged?.Invoke(value);
			}
		}

		public float FontSize
		{
			get => _fontSize;
			set
			{
				_fontSize = value;
				OnFontSizeChanged?.Invoke(value);
			}
		}

		public bool IsOnTimeStamp
		{
			get => _isOnTimeStamp;
			set
			{
				_isOnTimeStamp = value;
				OnTimeStampChanged?.Invoke(value);
			}
		}
#endregion Properties

#region events
		public event Action<eChattingFilter>?       OnChattingFilterChanged;
		public event Action<eChattingBubbleFilter>? OnChattingBubbleFilterChanged;

		public event Action<bool>?  OnAutoSimplifyChanged;
		public event Action<bool>?  OnLinkWarningChanged;
		public event Action<float>? OnChatTransparentChanged;
		public event Action<float>? OnFontSizeChanged;
		public event Action<bool>?  OnTimeStampChanged;
#endregion events

		public override string ToString() => $"IsOnTimeStamp: {IsOnTimeStamp}, ChattingFilter: {ChattingFilter}, ShowBubbleOption: {ShowBubbleOption}, IsOnLinkWarning: {IsOnLinkWarning}, ChatTransparent: {ChatTransparent}, FontSize: {FontSize}";

		/// <summary>
		/// 테이블 데이터의 기본값으로 데이터를 초기화
		/// </summary>
		public void Reset()
		{
			ResetDataFromTable();
			Apply();
			SaveData();
		}

		private void ResetDataFromTable()
		{
			var tableData = OptionController.Instance.SettingTableData;
			if (tableData == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "SettingTableData is null");
				return;
			}

			if (tableData.TryGetValue(eSetting.CHAT_FILTER, out var chatFilter) && chatFilter != null && chatFilter.Default != null)
				ChattingFilter = (eChattingFilter)Util.StringToBitmaskWithComma(chatFilter.Default);

			if (tableData.TryGetValue(eSetting.CHAT_LINKWARNING, out var linkWarning) && linkWarning != null && linkWarning.Default != null)
				if (int.TryParse(linkWarning.Default, out var linkWarningValue))
					IsOnLinkWarning = linkWarningValue >= 1;

			if (tableData.TryGetValue(eSetting.CHAT_SIMPLIFY, out var chatSimplify) && chatSimplify != null && chatSimplify.Default != null)
				if (int.TryParse(chatSimplify.Default, out var chatSimplifyValue))
					IsAutoSimplify = chatSimplifyValue >= 1;

			if (tableData.TryGetValue(eSetting.CHAT_TRANSPARENT, out var transparent) && transparent != null && transparent.Default != null)
				if (float.TryParse(transparent.Default, out var transparentValue))
					ChatTransparent = transparentValue;

			if (tableData.TryGetValue(eSetting.CHAT_FONTSIZE, out var fontSize) && fontSize != null && fontSize.Default != null)
				if (float.TryParse(fontSize.Default, out var fontSizeValue))
					FontSize = fontSizeValue;

			if (tableData.TryGetValue(eSetting.CHAT_TIMESTAMP, out var timeStamp) && timeStamp != null && timeStamp.Default != null)
				if (int.TryParse(timeStamp.Default, out var timeStampValue))
					IsOnTimeStamp = timeStampValue >= 1;

			if (tableData.TryGetValue(eSetting.CHAT_BUBBLEDISPLAY, out var bubbleDisplay) && bubbleDisplay != null && bubbleDisplay.Default != null)
				ShowBubbleOption = (eChattingBubbleFilter)Util.StringToBitmaskWithComma(bubbleDisplay.Default);
		}

		public override void OnInitialize()
		{
			base.Apply();
		}

		public override void Apply()
		{
			base.Apply();

			if (!_isInitialized)
			{
				ResetDataFromTable();
				_isInitialized = true;
			}

			if (User.InstanceExists && User.Instance.Connected)
				Commander.Instance.ChattingSettingRequest(this);
		}

		/// <summary>
		/// 서버에서 설정된 셋팅값을 로그인 시점에 받아온다
		/// 뷰모델이 활성화되기 전에 서버에서 값을 받는다는 가정하에 뷰모델 프로퍼티를 갱신시켜주지는 않음
		/// </summary>
		public override void SetStoredOption(SettingValueResponse response)
		{
			base.SetStoredOption(response);
			IsOnTimeStamp    = response.Timestamp;
			ShowBubbleOption = (eChattingBubbleFilter)response.SpeechBubble;
			ChatTransparent  = response.ChatOpacity;
			FontSize         = response.ChatFontSize;
			IsOnLinkWarning  = response.LinkWarnYn;
			IsAutoSimplify   = response.ChatSimplify;
			ChattingFilter   = (eChattingFilter)response.ChatFilter;
		}
	}
}
