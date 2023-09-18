/*===============================================================
* Product:		Com2Verse
* File Name:	ChatColor.cs
* Developer:	eugene9721
* Date:			2023-08-11 13:07
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Data;
using Com2Verse.UI;
using Cysharp.Text;

namespace Com2Verse.Chat
{
	public static class ChatColor
	{
		// Title and Tap
		public const string AreaColorPrefix    = "<color=#0D8460>";
		public const string NearbyColorPrefix  = "<color=#535872>";
		public const string WhisperColorPrefix = "<color=#FF5B00>";

		// Common
		public const string DisableColorPrefix = "<color=#adadad>"; // 미정

		public const string ColorSuffix = "</color>";

		public static string ApplyChatTitleColor(ChatCoreBase.eMessageType type, string value)
		{
			return type switch
			{
				ChatCoreBase.eMessageType.AREA    => ZString.Format("{0}{1}{2}", AreaColorPrefix,    value, ColorSuffix),
				ChatCoreBase.eMessageType.NEARBY  => ZString.Format("{0}{1}{2}", NearbyColorPrefix,  value, ColorSuffix),
				ChatCoreBase.eMessageType.WHISPER => ZString.Format("{0}{1}{2}", WhisperColorPrefix, value, ColorSuffix),
				_                                 => value,
			};
		}

		public static string ApplyChatTitleColor(ChatRootViewModel.eChatType type, string value)
		{
			return type switch
			{
				ChatRootViewModel.eChatType.AREA    => ApplyChatTitleColor(ChatCoreBase.eMessageType.AREA,    value),
				ChatRootViewModel.eChatType.NEARBY  => ApplyChatTitleColor(ChatCoreBase.eMessageType.NEARBY,  value),
				ChatRootViewModel.eChatType.WHISPER => ApplyChatTitleColor(ChatCoreBase.eMessageType.WHISPER, value),
				_                                   => value,
			};
		}

		public static string ApplyMessageColor(eChatColorType type, string value, bool isUserName = false)
		{
			var chatColorSetList = ChatManager.InstanceOrNull?.TableColor?.Datas;
			if (chatColorSetList == null) return value;

			foreach (var chatColorSet in chatColorSetList)
			{
				if (chatColorSet.ChatColorType == type)
				{
					// TODO: 프리팹에서 제어 가능한지 확인
					var isToolbar = SceneManager.InstanceOrNull?.CurrentScene.SpaceCode is eSpaceCode.MICE_CONFERENCE_HALL or eSpaceCode.MEETING;
					var colorCode = (isToolbar, isUserName) switch
					{
						(false, false) => chatColorSet.ColorCodeAreaMsg,
						(false, true)  => chatColorSet.ColorCodeAreaName,
						(true, false)  => chatColorSet.ToolbarChatMsg,
						(true, true)   => chatColorSet.ToolbarChatName,
					};

					return ZString.Format("<color={0}>{1}{2}", colorCode, value, ColorSuffix);
				}
			}

			return value;
		}

		public static string ApplyDisableColor(string value) => ZString.Format("{0}{1}{2}", DisableColorPrefix, value, ColorSuffix);
	}
}
