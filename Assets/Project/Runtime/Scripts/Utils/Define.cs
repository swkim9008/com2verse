/*===============================================================
* Product:		Com2Verse
* File Name:	Defines.cs
* Developer:	eugene9721
* Date:			2022-08-05 14:16
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Com2Verse.UI;
using UnityEngine;
using AuthorityCode = Com2Verse.WebApi.Service.Components.AuthorityCode;

namespace Com2Verse.Utils
{
	public static class Define
	{
		public static readonly LazyThreadSafetyMode SingletonInstanceSafetyMode = LazyThreadSafetyMode.PublicationOnly;

		public static readonly Vector2 INACTIVE_UI_POSITION = new Vector2(-1000, -1000);

		public static readonly int DEFAULT_TABLE_INDEX   = 1;
		public static readonly int POPUP_SORTING_ORDER   = 5000;
		public static readonly int MINIMODE_TARGET_FRAME = 15;

		public static readonly int DEFAULT_SCREEN_WIDTH  = 1920;
		public static readonly int DEFAULT_SCREEN_HEIGHT = 1080;

		public static readonly int DEFAULT_WEBVIEW_HEADER_SIZE = 64;

		public enum eLayer
		{
			DEFAULT        = 0,
			TRANSPARENT_FX = 1,
			IGNORE_RAYCAST = 2,
			CHARACTER      = 3,
			WATER          = 4,
			UI             = 5,
			OBJECT         = 6,
			GROUND         = 7,
			WALL           = 8,
		}

		public enum eRenderer
		{
			DEFAULT = 0,
			UI      = 1,
			FRAME   = 2,
		}

		public static string LayerName(eLayer layer) => layer switch
		{
			eLayer.DEFAULT        => "Default",
			eLayer.TRANSPARENT_FX => "TransparentFX",
			eLayer.IGNORE_RAYCAST => "Ignore Raycast",
			eLayer.CHARACTER      => "Character",
			eLayer.WATER          => "Water",
			eLayer.UI             => "UI",
			eLayer.OBJECT         => "Object",
			eLayer.GROUND         => "Ground",
			eLayer.WALL           => "Wall",
			_                     => "",
		};

		public static int LayerMask(eLayer layer) => 1 << (int)layer;

		/// <summary>
		/// 스몰토크, 디버그 등 AuthorityCode 자체가 할당되지 않은 경우 화면 공유 등 기능을 활성화 할 것인지 여부
		/// </summary>
		public static readonly bool AllowShareFeatureOnNullAuthority = true;

		/// <summary>
		/// 화면 공유 등 기능을 이용 가능한 유저 권한
		/// </summary>
		public static readonly IEnumerable<AuthorityCode> ShareFeatureAvailableAuthorities = new List<AuthorityCode>
		{
			AuthorityCode.Organizer,
			AuthorityCode.Presenter,
		};

		public static class Matting
		{
			public static readonly int HdTextureHeightThreshold = 360;
		}

		public static readonly int AudioInputLevelUiMultiplier = 10;

		[SuppressMessage("ReSharper", "InconsistentNaming")]
		public static class String
		{
			public static string UI_Common_Notice_Popup_Title => Localization.Instance.GetString(nameof(UI_Common_Notice_Popup_Title));
			public static string UI_Common_Btn_OK             => Localization.Instance.GetString(nameof(UI_Common_Btn_OK))!;
			public static string UI_Common_Btn_Cancel         => Localization.Instance.GetString(nameof(UI_Common_Btn_Cancel))!;

			public static string UI_MeetingRoomSharing_SharingStopped      => Localization.Instance.GetString(nameof(UI_MeetingRoomSharing_SharingStopped))!;
			public static string UI_MeetingRoomSharing_Popup_NoAuthority   => Localization.Instance.GetString(nameof(UI_MeetingRoomSharing_Popup_NoAuthority))!;
			public static string UI_MeetingRoomSharing_Popup_ChoiceSharing => Localization.Instance.GetString(nameof(UI_MeetingRoomSharing_Popup_ChoiceSharing))!;

			public static string Information_Title_ScreenSharing => Localization.Instance.GetString(nameof(Information_Title_ScreenSharing))!;
			public static string Information_Text_ScreenSharing  => Localization.Instance.GetString(nameof(Information_Text_ScreenSharing))!;
		}

		[SuppressMessage("ReSharper", "InconsistentNaming")]
		public static class TextKey
		{
			public static string UI_MeetingRoomCommon_DateTime_Format => nameof(UI_MeetingRoomCommon_DateTime_Format);
		}
	}
}
