/*===============================================================
* Product:		Com2Verse
* File Name:	OfficeSpaceExtract.cs
* Developer:	jhkim
* Date:			2023-05-13 17:16
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Com2VerseEditor.SpaceExtract
{
	public class OfficeSpaceExtract : SpaceExtractBase<OfficeSpaceExtract.eOfficeSpaceType, OfficeSpaceExtract.eOfficeSpaceObjectInteractionType>
	{
		private static readonly int Com2UsBuildingId = 2;
		private static readonly string ProgressTaskTitle = "오피스 공간 추출";
		private static readonly string Com2UsAccountID = "컴투스 계정 필요";

#pragma warning disable CS0414
		private static readonly long TemplateLobby = 30300101000;
		private static readonly long TemplateLounge = 30300301000;

		private static readonly long TemplateTeam  = 30300401000;
		private static readonly long TemplateTeam2 = 10300401002;
		private static readonly long TemplateTeam3 = 10300401003;
		private static readonly long TemplateTeam4 = 10300401004;
		private static readonly long TemplateTeam5 = 10300401005;

		private static readonly long TemplateSingle = 30300601000;

		private static readonly long TemplateConnecting1 = 30300501000;
		private static readonly long TemplateConnecting2 = 30300501001;
		private static readonly long TemplateConnecting3 = 30300501002;
#pragma warning restore CS0414

		private static readonly int Com2UsBuildingCode = 2;

		private static readonly string SpaceTypeOffice = "Office";
		private static readonly string SpaceCodeLobby = "Lobby";
		private static readonly string SpaceCodeLounge = "Lounge";
		private static readonly string SpaceCodeTeam = "Team";
		private static readonly string SpaceCodeSingle = "Single";
		private static readonly string SpaceCodeMeeting = "Meeting";

		public enum eOfficeSpaceType
		{
			LOBBY,
			LOUNGE,
			TEAM,
			SINGLE,
			CONNECTING_ROOM_1,
			CONNECTING_ROOM_2,
			CONNECTING_ROOM_3,
		}

		public enum eOfficeSpaceObjectInteractionType
		{
			LOBBY_SCREEN,
			LOBBY_HOLOGRAM,
			LOBBY_HOLOGRAM2,
			LOBBY_AUTH_1,
			LOBBY_AUTH_2,
			LOBBY_AUTH_3,
			LOBBY_AUTH_4,
			LOBBY_EXIT_1,
			LOBBY_EXIT_2,

			LOUNGE_SCREEN_1,
			LOUNGE_SCREEN_2,
			LOUNGE_SCREEN_3,
			LOUNGE_HOLOGRAM,
			LOUNGE_OFFICE_1,
			LOUNGE_OFFICE_2,
			LOUNGE_OFFICE_3,
			LOUNGE_OFFICE_4,
			LOUNGE_OFFICE_5,
			LOUNGE_OFFICE_6,
			LOUNGE_EXIT,

			TEAM_VOICE_RECORD,
			TEAM_BOARD_WRITE,
			TEAM_BOARD_READ,
			TEAM_WHITE_BOARD,
			TEAM_EXIT,

			SINGLE_VOICE_RECORD,
			SINGLE_BOARD_WRITE,
			SINGLE_BOARD_READ,
			SINGLE_EXIT,

			CONNECTING_ROOM_1_EXIT,
			CONNECTING_ROOM_2_EXIT,
			CONNECTING_ROOM_3_EXIT,
		}

#region Properties
		protected override IEnumerable<SpaceInfo<eOfficeSpaceType>> SpaceInfos { get; } = new List<SpaceInfo<eOfficeSpaceType>>
		{
			new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-100", BuildingTableInfo = BuildingTableInfos?[0], SpaceTableInfo = SpaceTableInfos?[0], SpaceDetailTableInfo = SpaceDetailTableInfos?[0], Type = eOfficeSpaceType.LOBBY},
			// new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-101", BuildingTableInfo = BuildingTableInfos?[1], SpaceTableInfo = SpaceTableInfos?[1], SpaceDetailTableInfo = SpaceDetailTableInfos?[1], Type = eOfficeSpaceType.LOUNGE},
			new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-102", BuildingTableInfo = BuildingTableInfos?[2], SpaceTableInfo = SpaceTableInfos?[2], SpaceDetailTableInfo = SpaceDetailTableInfos?[2], Type = eOfficeSpaceType.TEAM},
			new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-103", BuildingTableInfo = BuildingTableInfos?[3], SpaceTableInfo = SpaceTableInfos?[3], SpaceDetailTableInfo = SpaceDetailTableInfos?[3], Type = eOfficeSpaceType.TEAM},
			// new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-104", BuildingTableInfo = BuildingTableInfos?[4], SpaceTableInfo = SpaceTableInfos?[4], SpaceDetailTableInfo = SpaceDetailTableInfos?[4], Type = eOfficeSpaceType.LOUNGE},
			// new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-105", BuildingTableInfo = BuildingTableInfos?[5], SpaceTableInfo = SpaceTableInfos?[5], SpaceDetailTableInfo = SpaceDetailTableInfos?[5], Type = eOfficeSpaceType.SINGLE},
			// new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-106", BuildingTableInfo = BuildingTableInfos?[6], SpaceTableInfo = SpaceTableInfos?[6], SpaceDetailTableInfo = SpaceDetailTableInfos?[6], Type = eOfficeSpaceType.SINGLE},
			// new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-107", BuildingTableInfo = BuildingTableInfos?[7], SpaceTableInfo = SpaceTableInfos?[7], SpaceDetailTableInfo = SpaceDetailTableInfos?[7], Type = eOfficeSpaceType.SINGLE},
			new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-108", BuildingTableInfo = BuildingTableInfos?[8], SpaceTableInfo = SpaceTableInfos?[8], SpaceDetailTableInfo = SpaceDetailTableInfos?[8], Type = eOfficeSpaceType.TEAM},
			new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-109", BuildingTableInfo = BuildingTableInfos?[9], SpaceTableInfo = SpaceTableInfos?[9], SpaceDetailTableInfo = SpaceDetailTableInfos?[9], Type = eOfficeSpaceType.TEAM},

			new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-110", BuildingTableInfo = BuildingTableInfos?[10], SpaceTableInfo = SpaceTableInfos?[10], SpaceDetailTableInfo = SpaceDetailTableInfos?[10], Type = eOfficeSpaceType.TEAM},
			// new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-111", BuildingTableInfo = BuildingTableInfos?[11], SpaceTableInfo = SpaceTableInfos?[11], SpaceDetailTableInfo = SpaceDetailTableInfos?[11], Type = eOfficeSpaceType.LOUNGE},
			new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-112", BuildingTableInfo = BuildingTableInfos?[12], SpaceTableInfo = SpaceTableInfos?[12], SpaceDetailTableInfo = SpaceDetailTableInfos?[12], Type = eOfficeSpaceType.TEAM},
			new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-113", BuildingTableInfo = BuildingTableInfos?[13], SpaceTableInfo = SpaceTableInfos?[13], SpaceDetailTableInfo = SpaceDetailTableInfos?[13], Type = eOfficeSpaceType.TEAM},
			new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-114", BuildingTableInfo = BuildingTableInfos?[14], SpaceTableInfo = SpaceTableInfos?[14], SpaceDetailTableInfo = SpaceDetailTableInfos?[14], Type = eOfficeSpaceType.TEAM},
			new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-115", BuildingTableInfo = BuildingTableInfos?[15], SpaceTableInfo = SpaceTableInfos?[15], SpaceDetailTableInfo = SpaceDetailTableInfos?[15], Type = eOfficeSpaceType.TEAM},
			new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-116", BuildingTableInfo = BuildingTableInfos?[16], SpaceTableInfo = SpaceTableInfos?[16], SpaceDetailTableInfo = SpaceDetailTableInfos?[16], Type = eOfficeSpaceType.TEAM},
			new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-117", BuildingTableInfo = BuildingTableInfos?[17], SpaceTableInfo = SpaceTableInfos?[17], SpaceDetailTableInfo = SpaceDetailTableInfos?[17], Type = eOfficeSpaceType.TEAM},
			// new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-118", BuildingTableInfo = BuildingTableInfos?[18], SpaceTableInfo = SpaceTableInfos?[18], SpaceDetailTableInfo = SpaceDetailTableInfos?[18], Type = eOfficeSpaceType.LOUNGE},
			// new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-119", BuildingTableInfo = BuildingTableInfos?[19], SpaceTableInfo = SpaceTableInfos?[19], SpaceDetailTableInfo = SpaceDetailTableInfos?[19], Type = eOfficeSpaceType.SINGLE},

			new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-120", BuildingTableInfo = BuildingTableInfos?[20], SpaceTableInfo = SpaceTableInfos?[20], SpaceDetailTableInfo = SpaceDetailTableInfos?[20], Type = eOfficeSpaceType.TEAM},
			new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-121", BuildingTableInfo = BuildingTableInfos?[21], SpaceTableInfo = SpaceTableInfos?[21], SpaceDetailTableInfo = SpaceDetailTableInfos?[21], Type = eOfficeSpaceType.TEAM},
			// new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-122", BuildingTableInfo = BuildingTableInfos?[22], SpaceTableInfo = SpaceTableInfos?[22], SpaceDetailTableInfo = SpaceDetailTableInfos?[22], Type = eOfficeSpaceType.LOUNGE},
			// new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-123", BuildingTableInfo = BuildingTableInfos?[23], SpaceTableInfo = SpaceTableInfos?[23], SpaceDetailTableInfo = SpaceDetailTableInfos?[23], Type = eOfficeSpaceType.SINGLE},
			new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-124", BuildingTableInfo = BuildingTableInfos?[24], SpaceTableInfo = SpaceTableInfos?[24], SpaceDetailTableInfo = SpaceDetailTableInfos?[24], Type = eOfficeSpaceType.TEAM},
			new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-125", BuildingTableInfo = BuildingTableInfos?[25], SpaceTableInfo = SpaceTableInfos?[25], SpaceDetailTableInfo = SpaceDetailTableInfos?[25], Type = eOfficeSpaceType.TEAM},
			new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-126", BuildingTableInfo = BuildingTableInfos?[26], SpaceTableInfo = SpaceTableInfos?[26], SpaceDetailTableInfo = SpaceDetailTableInfos?[26], Type = eOfficeSpaceType.TEAM},
			new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-127", BuildingTableInfo = BuildingTableInfos?[27], SpaceTableInfo = SpaceTableInfos?[27], SpaceDetailTableInfo = SpaceDetailTableInfos?[27], Type = eOfficeSpaceType.TEAM},
			// new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-128", BuildingTableInfo = BuildingTableInfos?[28], SpaceTableInfo = SpaceTableInfos?[28], SpaceDetailTableInfo = SpaceDetailTableInfos?[28], Type = eOfficeSpaceType.LOUNGE},
			// new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-129", BuildingTableInfo = BuildingTableInfos?[29], SpaceTableInfo = SpaceTableInfos?[29], SpaceDetailTableInfo = SpaceDetailTableInfos?[29], Type = eOfficeSpaceType.SINGLE},
			//
			// new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-130", BuildingTableInfo = BuildingTableInfos?[30], SpaceTableInfo = SpaceTableInfos?[30], SpaceDetailTableInfo = SpaceDetailTableInfos?[30], Type = eOfficeSpaceType.SINGLE},
			// new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-131", BuildingTableInfo = BuildingTableInfos?[31], SpaceTableInfo = SpaceTableInfos?[31], SpaceDetailTableInfo = SpaceDetailTableInfos?[31], Type = eOfficeSpaceType.SINGLE},

			// new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-132", BuildingTableInfo = BuildingTableInfos?[32], SpaceTableInfo = SpaceTableInfos?[32], SpaceDetailTableInfo = SpaceDetailTableInfos?[32], Type = eOfficeSpaceType.CONNECTING_ROOM_1},
			// new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-133", BuildingTableInfo = BuildingTableInfos?[33], SpaceTableInfo = SpaceTableInfos?[33], SpaceDetailTableInfo = SpaceDetailTableInfos?[33], Type = eOfficeSpaceType.CONNECTING_ROOM_2},
			// new SpaceInfo<eOfficeSpaceType> {SpaceId = "f-134", BuildingTableInfo = BuildingTableInfos?[34], SpaceTableInfo = SpaceTableInfos?[34], SpaceDetailTableInfo = SpaceDetailTableInfos?[34], Type = eOfficeSpaceType.CONNECTING_ROOM_3},
		};

		protected override Dictionary<eOfficeSpaceType, eOfficeSpaceObjectInteractionType[]> SpaceInteractionMap { get; } = new Dictionary<eOfficeSpaceType, eOfficeSpaceObjectInteractionType[]>
		{
			{
				eOfficeSpaceType.LOBBY,
				new[]
				{
					eOfficeSpaceObjectInteractionType.LOBBY_SCREEN,
					eOfficeSpaceObjectInteractionType.LOBBY_HOLOGRAM,
					eOfficeSpaceObjectInteractionType.LOBBY_HOLOGRAM2,
					// eOfficeSpaceObjectInteractionType.LOBBY_AUTH_1,
					// eOfficeSpaceObjectInteractionType.LOBBY_AUTH_2,
					// eOfficeSpaceObjectInteractionType.LOBBY_AUTH_3,
					// eOfficeSpaceObjectInteractionType.LOBBY_AUTH_4,
					// eOfficeSpaceObjectInteractionType.LOBBY_EXIT_1,
					// eOfficeSpaceObjectInteractionType.LOBBY_EXIT_2,
				}
			},
			{
				eOfficeSpaceType.LOUNGE,
				new[]
				{
					/*	[  휴게 공간  ]
					 *      6   1
					 *      5   2
					 *      4   3
					 *		  X
					 */
					eOfficeSpaceObjectInteractionType.LOUNGE_SCREEN_1,
					eOfficeSpaceObjectInteractionType.LOUNGE_SCREEN_2,
					eOfficeSpaceObjectInteractionType.LOUNGE_SCREEN_3,
					eOfficeSpaceObjectInteractionType.LOUNGE_HOLOGRAM,
					eOfficeSpaceObjectInteractionType.LOUNGE_OFFICE_1,
					eOfficeSpaceObjectInteractionType.LOUNGE_OFFICE_2,
					eOfficeSpaceObjectInteractionType.LOUNGE_OFFICE_3,
					eOfficeSpaceObjectInteractionType.LOUNGE_OFFICE_4,
					eOfficeSpaceObjectInteractionType.LOUNGE_OFFICE_5,
					eOfficeSpaceObjectInteractionType.LOUNGE_OFFICE_6,
					eOfficeSpaceObjectInteractionType.LOUNGE_EXIT,
				}
			},
			{
				eOfficeSpaceType.TEAM,
				new[]
				{
					eOfficeSpaceObjectInteractionType.TEAM_VOICE_RECORD,
					eOfficeSpaceObjectInteractionType.TEAM_BOARD_WRITE,
					eOfficeSpaceObjectInteractionType.TEAM_BOARD_READ,
					eOfficeSpaceObjectInteractionType.TEAM_WHITE_BOARD,
					eOfficeSpaceObjectInteractionType.TEAM_EXIT,
				}
			},
			{
				eOfficeSpaceType.SINGLE,
				new[]
				{
					eOfficeSpaceObjectInteractionType.SINGLE_VOICE_RECORD,
					eOfficeSpaceObjectInteractionType.SINGLE_BOARD_WRITE,
					eOfficeSpaceObjectInteractionType.SINGLE_BOARD_READ,
					eOfficeSpaceObjectInteractionType.SINGLE_EXIT,
				}
			},
			{
				eOfficeSpaceType.CONNECTING_ROOM_1,
				new[]
				{
					eOfficeSpaceObjectInteractionType.CONNECTING_ROOM_1_EXIT,
				}
			},
			{
				eOfficeSpaceType.CONNECTING_ROOM_2,
				new[]
				{
					eOfficeSpaceObjectInteractionType.CONNECTING_ROOM_2_EXIT,
				}
			},
			{
				eOfficeSpaceType.CONNECTING_ROOM_3,
				new[]
				{
					eOfficeSpaceObjectInteractionType.CONNECTING_ROOM_3_EXIT,
				}
			}
		};

		// TODO: 씬 오브젝트 이름 변경시 수정 필요
		protected override Dictionary<eOfficeSpaceObjectInteractionType, KeyValuePair<string, string>> ObjectInteractionMap { get; } = new Dictionary<eOfficeSpaceObjectInteractionType, KeyValuePair<string, string>>
		{
			{eOfficeSpaceObjectInteractionType.LOBBY_SCREEN, new KeyValuePair<string, string>("Office_Screen_Video_01", "Video_Trigger")},
			{eOfficeSpaceObjectInteractionType.LOBBY_HOLOGRAM, new KeyValuePair<string, string>("Office_Hologram_AD_02", "Trigger_Hologram_AD_02")},
			{eOfficeSpaceObjectInteractionType.LOBBY_HOLOGRAM2, new KeyValuePair<string, string>("Office_Hologram_AD_02 (1)", "Trigger_Hologram_AD_02")},
			{eOfficeSpaceObjectInteractionType.LOBBY_AUTH_1, new KeyValuePair<string, string>("Office_Auth_01", "Auth")},
			{eOfficeSpaceObjectInteractionType.LOBBY_AUTH_2, new KeyValuePair<string, string>("Office_Auth_01 (1)", "Auth")},
			{eOfficeSpaceObjectInteractionType.LOBBY_AUTH_3, new KeyValuePair<string, string>("Office_Auth_01 (2)", "Auth")},
			{eOfficeSpaceObjectInteractionType.LOBBY_AUTH_4, new KeyValuePair<string, string>("Office_Auth_01 (3)", "Auth")},
			{eOfficeSpaceObjectInteractionType.LOBBY_EXIT_1, new KeyValuePair<string, string>("Common_Warp_BuildingExit_01", "Common_Warp_BuildingExit_01")},
			{eOfficeSpaceObjectInteractionType.LOBBY_EXIT_2, new KeyValuePair<string, string>("Common_Warp_BuildingExit_01 (1)", "Common_Warp_BuildingExit_01")},

			{eOfficeSpaceObjectInteractionType.LOUNGE_SCREEN_1, new KeyValuePair<string, string>("Office_Screen_AD_02", "Video_Trigger")},
			{eOfficeSpaceObjectInteractionType.LOUNGE_SCREEN_2, new KeyValuePair<string, string>("Office_Screen_AD_01", "Video_Trigger")},
			{eOfficeSpaceObjectInteractionType.LOUNGE_SCREEN_3, new KeyValuePair<string, string>("Office_Screen_AD_01 (1)", "Video_Trigger")},
			{eOfficeSpaceObjectInteractionType.LOUNGE_HOLOGRAM, new KeyValuePair<string, string>("Office_Hologram_AD_02", "Trigger_Hologram_AD_02")},
			{eOfficeSpaceObjectInteractionType.LOUNGE_OFFICE_1, new KeyValuePair<string, string>("Common_Warp_Main_01", "Common_Warp_Main_01")},
			{eOfficeSpaceObjectInteractionType.LOUNGE_OFFICE_2, new KeyValuePair<string, string>("Common_Warp_Main_01 (1)", "Common_Warp_Main_01")},
			{eOfficeSpaceObjectInteractionType.LOUNGE_OFFICE_3, new KeyValuePair<string, string>("Common_Warp_Main_01 (2)", "Common_Warp_Main_01")},
			{eOfficeSpaceObjectInteractionType.LOUNGE_OFFICE_4, new KeyValuePair<string, string>("Common_Warp_Main_01 (3)", "Common_Warp_Main_01")},
			{eOfficeSpaceObjectInteractionType.LOUNGE_OFFICE_5, new KeyValuePair<string, string>("Common_Warp_Main_01 (4)", "Common_Warp_Main_01")},
			{eOfficeSpaceObjectInteractionType.LOUNGE_OFFICE_6, new KeyValuePair<string, string>("Common_Warp_Main_01 (5)", "Common_Warp_Main_01")},

			{eOfficeSpaceObjectInteractionType.TEAM_VOICE_RECORD, new KeyValuePair<string, string>("Office_Hologram_VoiceMessage_01", "Trigger_Record")},
			{eOfficeSpaceObjectInteractionType.TEAM_BOARD_WRITE, new KeyValuePair<string, string>("Office_Kiosk_TodaysWord_01", "Trigger_BOARD")},
			{eOfficeSpaceObjectInteractionType.TEAM_BOARD_READ, new KeyValuePair<string, string>("Office_Kiosk_TodaysWord_01", "Trigger_BOARD")},
			{eOfficeSpaceObjectInteractionType.TEAM_WHITE_BOARD, new KeyValuePair<string, string>("Office_Screen_WhiteBoard_01", "Video_Trigger")},
			{eOfficeSpaceObjectInteractionType.TEAM_EXIT, new KeyValuePair<string, string>("Office_Warp_BuildingElevator_01", "Office_Warp_BuildingElevator_01")},

			{eOfficeSpaceObjectInteractionType.SINGLE_VOICE_RECORD, new KeyValuePair<string, string>("Office_Hologram_VoiceMessage_01", "Trigger_Record")},
			{eOfficeSpaceObjectInteractionType.SINGLE_BOARD_WRITE, new KeyValuePair<string, string>("Office_Kiosk_TodaysWord_01", "Trigger_BOARD_WRITE")},
			{eOfficeSpaceObjectInteractionType.SINGLE_BOARD_READ, new KeyValuePair<string, string>("Office_Kiosk_TodaysWord_01", "Trigger_BOARD_READ")},
			{eOfficeSpaceObjectInteractionType.SINGLE_EXIT, new KeyValuePair<string, string>("Common_Warp_Main_01", "Common_Warp_Main_01")},

			{eOfficeSpaceObjectInteractionType.CONNECTING_ROOM_1_EXIT, new KeyValuePair<string, string>("Common_Warp_Main_01", "Common_Warp_Main_01")},
			{eOfficeSpaceObjectInteractionType.CONNECTING_ROOM_2_EXIT, new KeyValuePair<string, string>("Common_Warp_Main_01", "Common_Warp_Main_01")},
			{eOfficeSpaceObjectInteractionType.CONNECTING_ROOM_3_EXIT, new KeyValuePair<string, string>("Common_Warp_Main_01", "Common_Warp_Main_01")},
		};

		// PrintSpaceInteractionMapStr 로 데이터 뽑아서 붙여넣기
		protected override Dictionary<(string, eOfficeSpaceObjectInteractionType), string> ObjectInteractionValueMap { get; } = new Dictionary<(string, eOfficeSpaceObjectInteractionType), string>
		{
			// LOBBY
			{("f-100", eOfficeSpaceObjectInteractionType.LOBBY_SCREEN), "ILL_NHTSFBo"},
			{("f-100", eOfficeSpaceObjectInteractionType.LOBBY_HOLOGRAM), ""},
			{("f-100", eOfficeSpaceObjectInteractionType.LOBBY_HOLOGRAM2), ""},

			// TEAM
			{("f-102", eOfficeSpaceObjectInteractionType.TEAM_VOICE_RECORD), "f-102"},
			{("f-102", eOfficeSpaceObjectInteractionType.TEAM_BOARD_WRITE), "f-102"},
			{("f-102", eOfficeSpaceObjectInteractionType.TEAM_BOARD_READ), "f-102"},
			{("f-102", eOfficeSpaceObjectInteractionType.TEAM_WHITE_BOARD), "uXjVMHEpaLc="},
			{("f-102", eOfficeSpaceObjectInteractionType.TEAM_EXIT), "f-101"},

			// TEAM
			{("f-103", eOfficeSpaceObjectInteractionType.TEAM_VOICE_RECORD), "f-103"},
			{("f-103", eOfficeSpaceObjectInteractionType.TEAM_BOARD_WRITE), "f-103"},
			{("f-103", eOfficeSpaceObjectInteractionType.TEAM_BOARD_READ), "f-103"},
			{("f-103", eOfficeSpaceObjectInteractionType.TEAM_WHITE_BOARD), "uXjVMHEpbjw="},
			{("f-103", eOfficeSpaceObjectInteractionType.TEAM_EXIT), "f-101"},

			// TEAM
			{("f-108", eOfficeSpaceObjectInteractionType.TEAM_VOICE_RECORD), "f-108"},
			{("f-108", eOfficeSpaceObjectInteractionType.TEAM_BOARD_WRITE), "f-108"},
			{("f-108", eOfficeSpaceObjectInteractionType.TEAM_BOARD_READ), "f-108"},
			{("f-108", eOfficeSpaceObjectInteractionType.TEAM_WHITE_BOARD), "uXjVMHEpbvE="},
			{("f-108", eOfficeSpaceObjectInteractionType.TEAM_EXIT), "f-101"},

			// TEAM
			{("f-109", eOfficeSpaceObjectInteractionType.TEAM_VOICE_RECORD), "f-109"},
			{("f-109", eOfficeSpaceObjectInteractionType.TEAM_BOARD_WRITE), "f-109"},
			{("f-109", eOfficeSpaceObjectInteractionType.TEAM_BOARD_READ), "f-109"},
			{("f-109", eOfficeSpaceObjectInteractionType.TEAM_WHITE_BOARD), "uXjVMHEpbr4="},
			{("f-109", eOfficeSpaceObjectInteractionType.TEAM_EXIT), "f-101"},

			// TEAM
			{("f-110", eOfficeSpaceObjectInteractionType.TEAM_VOICE_RECORD), "f-110"},
			{("f-110", eOfficeSpaceObjectInteractionType.TEAM_BOARD_WRITE), "f-110"},
			{("f-110", eOfficeSpaceObjectInteractionType.TEAM_BOARD_READ), "f-110"},
			{("f-110", eOfficeSpaceObjectInteractionType.TEAM_WHITE_BOARD), "uXjVMHEScaY="},
			{("f-110", eOfficeSpaceObjectInteractionType.TEAM_EXIT), "f-101"},

			// TEAM
			{("f-112", eOfficeSpaceObjectInteractionType.TEAM_VOICE_RECORD), "f-112"},
			{("f-112", eOfficeSpaceObjectInteractionType.TEAM_BOARD_WRITE), "f-112"},
			{("f-112", eOfficeSpaceObjectInteractionType.TEAM_BOARD_READ), "f-112"},
			{("f-112", eOfficeSpaceObjectInteractionType.TEAM_WHITE_BOARD), "uXjVMHESdkE="},
			{("f-112", eOfficeSpaceObjectInteractionType.TEAM_EXIT), "f-101"},

			// TEAM
			{("f-113", eOfficeSpaceObjectInteractionType.TEAM_VOICE_RECORD), "f-113"},
			{("f-113", eOfficeSpaceObjectInteractionType.TEAM_BOARD_WRITE), "f-113"},
			{("f-113", eOfficeSpaceObjectInteractionType.TEAM_BOARD_READ), "f-113"},
			{("f-113", eOfficeSpaceObjectInteractionType.TEAM_WHITE_BOARD), "uXjVMHESdm8="},
			{("f-113", eOfficeSpaceObjectInteractionType.TEAM_EXIT), "f-101"},

			// TEAM
			{("f-114", eOfficeSpaceObjectInteractionType.TEAM_VOICE_RECORD), "f-114"},
			{("f-114", eOfficeSpaceObjectInteractionType.TEAM_BOARD_WRITE), "f-114"},
			{("f-114", eOfficeSpaceObjectInteractionType.TEAM_BOARD_READ), "f-114"},
			{("f-114", eOfficeSpaceObjectInteractionType.TEAM_WHITE_BOARD), "uXjVMHESdto="},
			{("f-114", eOfficeSpaceObjectInteractionType.TEAM_EXIT), "f-101"},

			// TEAM
			{("f-115", eOfficeSpaceObjectInteractionType.TEAM_VOICE_RECORD), "f-115"},
			{("f-115", eOfficeSpaceObjectInteractionType.TEAM_BOARD_WRITE), "f-115"},
			{("f-115", eOfficeSpaceObjectInteractionType.TEAM_BOARD_READ), "f-115"},
			{("f-115", eOfficeSpaceObjectInteractionType.TEAM_WHITE_BOARD), "uXjVMHESdvo="},
			{("f-115", eOfficeSpaceObjectInteractionType.TEAM_EXIT), "f-101"},

			// TEAM
			{("f-116", eOfficeSpaceObjectInteractionType.TEAM_VOICE_RECORD), "f-116"},
			{("f-116", eOfficeSpaceObjectInteractionType.TEAM_BOARD_WRITE), "f-116"},
			{("f-116", eOfficeSpaceObjectInteractionType.TEAM_BOARD_READ), "f-116"},
			{("f-116", eOfficeSpaceObjectInteractionType.TEAM_WHITE_BOARD), "uXjVMHESdqI="},
			{("f-116", eOfficeSpaceObjectInteractionType.TEAM_EXIT), "f-101"},

			// TEAM
			{("f-117", eOfficeSpaceObjectInteractionType.TEAM_VOICE_RECORD), "f-117"},
			{("f-117", eOfficeSpaceObjectInteractionType.TEAM_BOARD_WRITE), "f-117"},
			{("f-117", eOfficeSpaceObjectInteractionType.TEAM_BOARD_READ), "f-117"},
			{("f-117", eOfficeSpaceObjectInteractionType.TEAM_WHITE_BOARD), "uXjVMHESd00="},
			{("f-117", eOfficeSpaceObjectInteractionType.TEAM_EXIT), "f-101"},

			// TEAM
			{("f-120", eOfficeSpaceObjectInteractionType.TEAM_VOICE_RECORD), "f-120"},
			{("f-120", eOfficeSpaceObjectInteractionType.TEAM_BOARD_WRITE), "f-120"},
			{("f-120", eOfficeSpaceObjectInteractionType.TEAM_BOARD_READ), "f-120"},
			{("f-120", eOfficeSpaceObjectInteractionType.TEAM_WHITE_BOARD), "uXjVMHESd1k="},
			{("f-120", eOfficeSpaceObjectInteractionType.TEAM_EXIT), "f-101"},

			// TEAM
			{("f-121", eOfficeSpaceObjectInteractionType.TEAM_VOICE_RECORD), "f-121"},
			{("f-121", eOfficeSpaceObjectInteractionType.TEAM_BOARD_WRITE), "f-121"},
			{("f-121", eOfficeSpaceObjectInteractionType.TEAM_BOARD_READ), "f-121"},
			{("f-121", eOfficeSpaceObjectInteractionType.TEAM_WHITE_BOARD), "uXjVMHESdwM="},
			{("f-121", eOfficeSpaceObjectInteractionType.TEAM_EXIT), "f-101"},

			// TEAM
			{("f-124", eOfficeSpaceObjectInteractionType.TEAM_VOICE_RECORD), "f-124"},
			{("f-124", eOfficeSpaceObjectInteractionType.TEAM_BOARD_WRITE), "f-124"},
			{("f-124", eOfficeSpaceObjectInteractionType.TEAM_BOARD_READ), "f-124"},
			{("f-124", eOfficeSpaceObjectInteractionType.TEAM_WHITE_BOARD), "uXjVMHESdyI="},
			{("f-124", eOfficeSpaceObjectInteractionType.TEAM_EXIT), "f-101"},

			// TEAM
			{("f-125", eOfficeSpaceObjectInteractionType.TEAM_VOICE_RECORD), "f-125"},
			{("f-125", eOfficeSpaceObjectInteractionType.TEAM_BOARD_WRITE), "f-125"},
			{("f-125", eOfficeSpaceObjectInteractionType.TEAM_BOARD_READ), "f-125"},
			{("f-125", eOfficeSpaceObjectInteractionType.TEAM_WHITE_BOARD), "uXjVMHESd8c="},
			{("f-125", eOfficeSpaceObjectInteractionType.TEAM_EXIT), "f-101"},

			// TEAM
			{("f-126", eOfficeSpaceObjectInteractionType.TEAM_VOICE_RECORD), "f-126"},
			{("f-126", eOfficeSpaceObjectInteractionType.TEAM_BOARD_WRITE), "f-126"},
			{("f-126", eOfficeSpaceObjectInteractionType.TEAM_BOARD_READ), "f-126"},
			{("f-126", eOfficeSpaceObjectInteractionType.TEAM_WHITE_BOARD), "uXjVMHESd9o="},
			{("f-126", eOfficeSpaceObjectInteractionType.TEAM_EXIT), "f-101"},

			// TEAM
			{("f-127", eOfficeSpaceObjectInteractionType.TEAM_VOICE_RECORD), "f-127"},
			{("f-127", eOfficeSpaceObjectInteractionType.TEAM_BOARD_WRITE), "f-127"},
			{("f-127", eOfficeSpaceObjectInteractionType.TEAM_BOARD_READ), "f-127"},
			{("f-127", eOfficeSpaceObjectInteractionType.TEAM_WHITE_BOARD), "uXjVMHESd5o="},
			{("f-127", eOfficeSpaceObjectInteractionType.TEAM_EXIT), "f-101"},


		};

		// protected override IEnumerable<eLogicType> CandidateLogicType { get; } = new[]
		// {
		// 	eLogicType.WARP__SPACE,
		// 	eLogicType.WARP__SELECT,
		// 	eLogicType.WARP__BUILDING__LIST,
		// 	eLogicType.WARP__BUILDING__EXIT,
		// };
		protected override string GetTemplateSceneName(eOfficeSpaceType type) => OfficeTemplateSceneNameMap?[type];
#endregion // Properties

#region Private Fields
		private static readonly Dictionary<eOfficeSpaceType, string> OfficeTemplateSceneNameMap = new Dictionary<eOfficeSpaceType, string>
		{
			{eOfficeSpaceType.LOBBY, "Office_Lobby_Common_1_ServerObjects"},
			// {eOfficeSpaceType.LOUNGE, "Office_Department_Common_1_ServerObjects"},
			{eOfficeSpaceType.TEAM, "Office_Team_Common_1_ServerObjects"},
			// {eOfficeSpaceType.SINGLE, "Office_Single_Common_1_ServerObjects"},
		};

		private static readonly BuildingTableInfo[] BuildingTableInfos = new[]
		{
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 1, Location = 1},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 2, Location = 1},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 2, Location = 2},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 2, Location = 3},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 3, Location = 1},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 3, Location = 2},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 3, Location = 3},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 3, Location = 4},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 3, Location = 5},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 3, Location = 6},

			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 3, Location = 7},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 4, Location = 1},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 4, Location = 2},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 4, Location = 3},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 4, Location = 4},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 4, Location = 5},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 4, Location = 6},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 4, Location = 7},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 5, Location = 1},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 5, Location = 2},

			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 5, Location = 3},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 5, Location = 4},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 6, Location = 1},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 6, Location = 2},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 6, Location = 3},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 6, Location = 4},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 6, Location = 5},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 6, Location = 6},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 7, Location = 1},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 7, Location = 2},

			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 7, Location = 3},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 7, Location = 4},

			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 8, Location = 1},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 8, Location = 2},
			new BuildingTableInfo {BuildingId = Com2UsBuildingId, FloorNo = 8, Location = 3},
		};

		private static readonly SpaceTableInfo[] SpaceTableInfos = new[]
		{
			new SpaceTableInfo {No = 1, AccountId = Com2UsAccountID, Name = "컴투스 오피스 로비", Description = "컴투스 오피스 로비", TemplateId = TemplateLobby},
			new SpaceTableInfo {No = 9, AccountId = Com2UsAccountID, Name = "휴게공간", Description = "컴투스 오피스_2층_휴게공간", TemplateId = TemplateLounge},
			new SpaceTableInfo {No = 10, AccountId = Com2UsAccountID, Name = "사업1팀", Description = "컴투스 오피스_2층_사업1팀", TemplateId = TemplateTeam},
			new SpaceTableInfo {No = 11, AccountId = Com2UsAccountID, Name = "사업2팀", Description = "컴투스 오피스_2층_사업2팀", TemplateId = TemplateTeam},
			new SpaceTableInfo {No = 12, AccountId = Com2UsAccountID, Name = "휴게공간", Description = "컴투스 오피스_3층_휴게공간", TemplateId = TemplateLounge},
			new SpaceTableInfo {No = 13, AccountId = Com2UsAccountID, Name = "개발담당", Description = "컴투스 오피스_3층_개발담당", TemplateId = TemplateSingle},
			new SpaceTableInfo {No = 14, AccountId = Com2UsAccountID, Name = "개발본부", Description = "컴투스 오피스_3층_개발본부", TemplateId = TemplateSingle},
			new SpaceTableInfo {No = 15, AccountId = Com2UsAccountID, Name = "개발운영실", Description = "컴투스 오피스_3층_개발운영실", TemplateId = TemplateSingle},
			new SpaceTableInfo {No = 16, AccountId = Com2UsAccountID, Name = "PM팀", Description = "컴투스 오피스_3층_PM팀", TemplateId = TemplateTeam},
			new SpaceTableInfo {No = 17, AccountId = Com2UsAccountID, Name = "전략기획팀", Description = "컴투스 오피스_3층_전략기획팀", TemplateId = TemplateTeam},
			new SpaceTableInfo {No = 18, AccountId = Com2UsAccountID, Name = "서비스운영팀", Description = "컴투스 오피스_3층_서비스운영팀", TemplateId = TemplateTeam},
			new SpaceTableInfo {No = 19, AccountId = Com2UsAccountID, Name = "휴게공간", Description = "컴투스 오피스_4층_휴게공간", TemplateId = TemplateLounge},
			new SpaceTableInfo {No = 20, AccountId = Com2UsAccountID, Name = "인프라스트럭처개발실", Description = "컴투스 오피스_4층_인프라스트럭처개발실", TemplateId = TemplateTeam},
			new SpaceTableInfo {No = 21, AccountId = Com2UsAccountID, Name = "기획팀", Description = "컴투스 오피스_4층_기획팀", TemplateId = TemplateTeam},
			new SpaceTableInfo {No = 22, AccountId = Com2UsAccountID, Name = "클라이언트팀", Description = "컴투스 오피스_4층_클라이언트팀", TemplateId = TemplateTeam},
			new SpaceTableInfo {No = 23, AccountId = Com2UsAccountID, Name = "서버팀", Description = "컴투스 오피스_4층_서버팀", TemplateId = TemplateTeam},
			new SpaceTableInfo {No = 24, AccountId = Com2UsAccountID, Name = "오피스개발팀", Description = "컴투스 오피스_4층_오피스개발팀", TemplateId = TemplateTeam},
			new SpaceTableInfo {No = 25, AccountId = Com2UsAccountID, Name = "MICE개발팀", Description = "컴투스 오피스_4층_MICE개발팀", TemplateId = TemplateTeam},
			new SpaceTableInfo {No = 26, AccountId = Com2UsAccountID, Name = "휴게공간", Description = "컴투스 오피스_5층_휴게공간", TemplateId = TemplateLounge},
			new SpaceTableInfo {No = 27, AccountId = Com2UsAccountID, Name = "플랫폼실", Description = "컴투스 오피스_5층_플랫폼실", TemplateId = TemplateSingle},
			new SpaceTableInfo {No = 28, AccountId = Com2UsAccountID, Name = "플랫폼개발팀", Description = "컴투스 오피스_5층_플랫폼개발팀", TemplateId = TemplateTeam},
			new SpaceTableInfo {No = 29, AccountId = Com2UsAccountID, Name = "솔루션팀", Description = "컴투스 오피스_5층_솔루션팀", TemplateId = TemplateTeam},
			new SpaceTableInfo {No = 30, AccountId = Com2UsAccountID, Name = "휴게공간", Description = "컴투스 오피스_6층_휴게공간", TemplateId = TemplateLounge},
			new SpaceTableInfo {No = 31, AccountId = Com2UsAccountID, Name = "아트실", Description = "컴투스 오피스_6층_아트실", TemplateId = TemplateSingle},
			new SpaceTableInfo {No = 32, AccountId = Com2UsAccountID, Name = "아트1팀", Description = "컴투스 오피스_6층_아트1팀", TemplateId = TemplateTeam},
			new SpaceTableInfo {No = 33, AccountId = Com2UsAccountID, Name = "아트2팀", Description = "컴투스 오피스_6층_아트2팀", TemplateId = TemplateTeam},
			new SpaceTableInfo {No = 34, AccountId = Com2UsAccountID, Name = "DevOpsSec팀", Description = "컴투스 오피스_6층_DevOpsSec팀", TemplateId = TemplateTeam},
			new SpaceTableInfo {No = 35, AccountId = Com2UsAccountID, Name = "멀티플랫폼팀", Description = "컴투스 오피스_6층_멀티플랫폼팀", TemplateId = TemplateTeam},
			new SpaceTableInfo {No = 36, AccountId = Com2UsAccountID, Name = "휴게공간", Description = "컴투스 오피스_7층_휴게공간", TemplateId = TemplateLounge},
			new SpaceTableInfo {No = 37, AccountId = Com2UsAccountID, Name = "GSO", Description = "컴투스 오피스_7층_GSO", TemplateId = TemplateSingle},
			new SpaceTableInfo {No = 38, AccountId = Com2UsAccountID, Name = "GCIO", Description = "컴투스 오피스_7층_GCIO", TemplateId = TemplateSingle},
			new SpaceTableInfo {No = 39, AccountId = Com2UsAccountID, Name = "대표이사", Description = "컴투스 오피스_7층_대표이사", TemplateId = TemplateSingle},
			new SpaceTableInfo {No = 39, AccountId = Com2UsAccountID, Name = "커넥팅 룸 1", Description = "커넥팅 룸 1", TemplateId = TemplateConnecting1},
			new SpaceTableInfo {No = 39, AccountId = Com2UsAccountID, Name = "커넥팅 룸 2", Description = "커넥팅 룸 2", TemplateId = TemplateConnecting2},
			new SpaceTableInfo {No = 39, AccountId = Com2UsAccountID, Name = "커넥팅 룸 3", Description = "커넥팅 룸 3", TemplateId = TemplateConnecting3},
		};

		private static readonly SpaceDetailTableInfo[] SpaceDetailTableInfos = new[]
		{
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeLobby},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeLounge},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeTeam},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeTeam},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeLounge},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeSingle},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeSingle},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeSingle},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeTeam},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeTeam},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeTeam},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeLounge},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeTeam},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeTeam},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeTeam},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeTeam},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeTeam},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeTeam},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeLounge},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeSingle},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeTeam},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeTeam},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeLounge},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeSingle},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeTeam},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeTeam},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeTeam},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeTeam},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeLounge},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeSingle},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeSingle},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeSingle},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeMeeting},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeMeeting},
			new SpaceDetailTableInfo {BuildingType = Com2UsBuildingId, BuildingCode = Com2UsBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeMeeting},
		};
#endregion // Private Fields

#region Menus
		[MenuItem("Com2Verse/SpaceData Extraction/Office/모두 추출", priority = 0)]
		private static async void RunAll()
		{
			var instance = new OfficeSpaceExtract();
			var officeScenePaths = GetAllScenePaths(OfficeTemplateSceneNameMap.Values.ToArray());
			var officeSceneServerObjects = await instance.GetSceneServerObjectsAsync(officeScenePaths);
			await instance.ExtractAllAsync(ProgressTaskTitle, officeSceneServerObjects);
		}

		[MenuItem("Com2Verse/SpaceData Extraction/Office/공간 상호작용 맵 데이터 복사")]
		private static void CopySpaceInteractionMap() => new OfficeSpaceExtract().PrintSpaceInteractionMapStr();
		//
		// [MenuItem("Com2Verse/SpaceData Extraction/Office/서버 오브젝트 추출/선택된 씬", priority = 1)]
		// private static async UniTask ExtractServerObjects()
		// {
		// 	var instance = new OfficeSpaceExtract();
		// 	var serverObjects = instance.GetSceneServerObjectsInSelection();
		// 	await instance.ExportServerObjectTableAsync(serverObjects);
		// }
		//
		// [MenuItem("Com2Verse/SpaceData Extraction/Office/서버 오브젝트 추출/현재 씬", priority = 2)]
		// private static async UniTask ExtractServerObjectsInScene()
		// {
		// 	var instance = new OfficeSpaceExtract();
		// 	var serverObjects = instance.GetSceneServerObjectsInActiveScene();
		// 	await instance.ExportServerObjectTableAsync(serverObjects);
		// }
		//
		// [MenuItem("Com2Verse/SpaceData Extraction/Office/서버 오브젝트 추출/모든 씬", priority = 3)]
		// private static async UniTask ExtractServerObjectsInAllScene()
		// {
		// 	var instance = new OfficeSpaceExtract();
		// 	var serverObjects = instance.GetSceneServerObjectsInAllScene();
		// 	await instance.ExportServerObjectTableAsync(serverObjects);
		// }
		//
		// [MenuItem("Com2Verse/SpaceData Extraction/Office/오브젝트 상호작용 테이블 추출/선택된 씬", priority = 1)]
		// private static async UniTask ExtractObjectInteractionTable()
		// {
		// 	var instance = new OfficeSpaceExtract();
		// 	var serverObjects = instance.GetSceneServerObjectsInSelection();
		// 	await instance.ExportObjectInteractionTableAsync(serverObjects);
		// }
		//
		//
		// [MenuItem("Com2Verse/SpaceData Extraction/Office/오브젝트 상호작용 테이블 추출/현재 씬", priority = 2)]
		// private static async UniTask ExtractObjectInteractionTableInScene()
		// {
		// 	var instance = new OfficeSpaceExtract();
		// 	var serverObjects = instance.GetSceneServerObjectsInActiveScene();
		// 	await instance.ExportObjectInteractionTableAsync(serverObjects);
		// }
		//
		// [MenuItem("Com2Verse/SpaceData Extraction/Office/오브젝트 상호작용 테이블 추출/모든 씬", priority = 3)]
		// private static async UniTask ExtractObjectInteractionTableInAllScene()
		// {
		// 	var instance = new OfficeSpaceExtract();
		// 	var serverObjects = instance.GetSceneServerObjectsInAllScene();
		// 	await instance.ExportObjectInteractionTableAsync(serverObjects);
		// }
		//
		// private static async UniTask ExportBuildingDetailTable()
		// {
		// 	var instance = new OfficeSpaceExtract();
		// 	await instance.ExportBuildingDetailTableAsync();
		// }
		//
		// private static async UniTask ExportSpaceTable()
		// {
		// 	var instance = new OfficeSpaceExtract();
		// 	await instance.ExportSpaceTableAsync();
		// }
		//
		// private static async UniTask ExportSpaceDetailTable()
		// {
		// 	var instance = new OfficeSpaceExtract();
		// 	await instance.ExportSpaceDetailTableAsync();
		// }
		//
		// [MenuItem("Com2Verse/SpaceData Extraction/Misc/Office/Print SpaceInteractionMapStr")]
		// private static void PrintSpaceInteractionMapStrMenu()
		// {
		// 	var officeExtract = new OfficeSpaceExtract();
		// 	officeExtract.PrintSpaceInteractionMapStr();
		// }
#endregion // Menus
	}
}
