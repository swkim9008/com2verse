/*===============================================================
* Product:		Com2Verse
* File Name:	MiceSpaceExtract.cs
* Developer:	jhkim
* Date:			2023-05-17 10:11
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.Linq;
using Com2Verse.Data;
using UnityEditor;

namespace Com2VerseEditor.SpaceExtract
{
	public sealed class MiceSpaceExtract : SpaceExtractBase<MiceSpaceExtract.eMiceTemplateType, MiceSpaceExtract.eMiceSpaceObjectInteractionType>
	{
		private static readonly int MiceBuildingId = 4;
		private static readonly int MiceBuildingCode = 1;
		private static readonly string ProgressTaskTitle = "MICE 공간 추출";

		public enum eMiceTemplateType
		{
			LOBBY,
			LOUNGE,
			CONFERENCE_HALL,
			MEET_UP,
		}

		public enum eMiceSpaceObjectInteractionType
		{
			CONFERENCE_HALL_LECTURE,
			CONFERENCE_HALL_EXIT,
			CONFERENCE_HALL_AD,
			LOBBY_AD,
			LOBBY_KIOSK_NORMAL,
			LOBBY_LEAFLET,
			LOBBY_KIOSK_SPECIAL,
			LOUNGE_SPAWN,
			LOUNGE_ENTRANCE_1,
			LOUNGE_ENTRANCE_2,
			LOUNGE_ENTRANCE_3,
			LOUNGE_EXIT,
			LOUNGE_KIOSK_NORMAL,
			LOUNGE_LEAFLET,
			LOUNGE_KIOSK_SPECIAL,
			LOUNGE_AD,
			MEET_UP_LECTURE,
			MEET_UP_AD,
		}

#region Properties
		protected override IEnumerable<SpaceInfo<eMiceTemplateType>> SpaceInfos { get; } = new List<SpaceInfo<eMiceTemplateType>>
		{
			new SpaceInfo<eMiceTemplateType> {SpaceId = "f-0300", Type = eMiceTemplateType.LOBBY},
			new SpaceInfo<eMiceTemplateType> {SpaceId = "f-0301", Type = eMiceTemplateType.LOUNGE},
			new SpaceInfo<eMiceTemplateType> {SpaceId = "f-0302", Type = eMiceTemplateType.CONFERENCE_HALL},
			new SpaceInfo<eMiceTemplateType> {SpaceId = "f-0303", Type = eMiceTemplateType.MEET_UP},
			new SpaceInfo<eMiceTemplateType> {SpaceId = "f-0304", Type = eMiceTemplateType.CONFERENCE_HALL},
			new SpaceInfo<eMiceTemplateType> {SpaceId = "f-0305", Type = eMiceTemplateType.CONFERENCE_HALL},
		};

		protected override Dictionary<eMiceTemplateType, eMiceSpaceObjectInteractionType[]> SpaceInteractionMap { get; } = new Dictionary<eMiceTemplateType, eMiceSpaceObjectInteractionType[]>
		{
			{
				eMiceTemplateType.CONFERENCE_HALL,
				new[]
				{
					eMiceSpaceObjectInteractionType.CONFERENCE_HALL_LECTURE,
					eMiceSpaceObjectInteractionType.CONFERENCE_HALL_EXIT,
					eMiceSpaceObjectInteractionType.CONFERENCE_HALL_AD,
				}
			},
			{
				eMiceTemplateType.LOBBY,
				new[]
				{
					eMiceSpaceObjectInteractionType.LOBBY_AD,
					eMiceSpaceObjectInteractionType.LOBBY_KIOSK_NORMAL,
					eMiceSpaceObjectInteractionType.LOBBY_LEAFLET,
					eMiceSpaceObjectInteractionType.LOBBY_KIOSK_SPECIAL,
				}
			},
			{
				eMiceTemplateType.LOUNGE,
				new[]
				{
					eMiceSpaceObjectInteractionType.LOUNGE_SPAWN,
					eMiceSpaceObjectInteractionType.LOUNGE_ENTRANCE_1,
					eMiceSpaceObjectInteractionType.LOUNGE_ENTRANCE_2,
					eMiceSpaceObjectInteractionType.LOUNGE_ENTRANCE_3,
					eMiceSpaceObjectInteractionType.LOUNGE_EXIT,
					eMiceSpaceObjectInteractionType.LOUNGE_KIOSK_NORMAL,
					eMiceSpaceObjectInteractionType.LOUNGE_LEAFLET,
					eMiceSpaceObjectInteractionType.LOUNGE_KIOSK_SPECIAL,
					eMiceSpaceObjectInteractionType.LOUNGE_AD,
				}
			},
			{
				eMiceTemplateType.MEET_UP,
				new[]
				{
					eMiceSpaceObjectInteractionType.MEET_UP_LECTURE,
					eMiceSpaceObjectInteractionType.MEET_UP_AD,
				}
			},
		};

		protected override Dictionary<eMiceSpaceObjectInteractionType, KeyValuePair<string, string>> ObjectInteractionMap { get; } = new Dictionary<eMiceSpaceObjectInteractionType, KeyValuePair<string, string>>
		{
			// {eMiceSpaceObjectInteractionType.CONFERENCE_HALL_LECTURE, "MICE_Screen_Lecture_01"},
			// {eMiceSpaceObjectInteractionType.CONFERENCE_HALL_EXIT, "Common_Spawn_Main_01"},
			// {eMiceSpaceObjectInteractionType.CONFERENCE_HALL_AD, "MICE_Screen_AD_02"},
			// {eMiceSpaceObjectInteractionType.LOBBY_AD, "MICE_Screen_AD_04"},
			// {eMiceSpaceObjectInteractionType.LOBBY_KIOSK_NORMAL, "MICE_Kiosk_Normal_02"},
			// {eMiceSpaceObjectInteractionType.LOBBY_LEAFLET, "MICE_LeafletStand_Normal_02"},
			// {eMiceSpaceObjectInteractionType.LOBBY_KIOSK_SPECIAL, "MICE_Kiosk_Spacial_02"},
			// {eMiceSpaceObjectInteractionType.LOUNGE_SPAWN, "Common_Spawn_Main_01"},
			// {eMiceSpaceObjectInteractionType.LOUNGE_ENTRANCE_1, "MICE_Warp_SelectHall_01"},
			// {eMiceSpaceObjectInteractionType.LOUNGE_ENTRANCE_2, "MICE_Warp_SelectHall_01 (2)"},
			// {eMiceSpaceObjectInteractionType.LOUNGE_ENTRANCE_3, "MICE_Warp_SelectHall_01 (1)"},
			// {eMiceSpaceObjectInteractionType.LOUNGE_EXIT, "Common_Warp_BuildingExit_01"},
			// {eMiceSpaceObjectInteractionType.LOUNGE_KIOSK_NORMAL, "MICE_Kiosk_Normal_01"},
			// {eMiceSpaceObjectInteractionType.LOUNGE_LEAFLET, "MICE_LeafletStand_Normal_01"},
			// {eMiceSpaceObjectInteractionType.LOUNGE_KIOSK_SPECIAL, "MICE_Kiosk_Spacial_01"},
			// {eMiceSpaceObjectInteractionType.LOUNGE_AD, "MICE_Screen_AD_01"},
			// {eMiceSpaceObjectInteractionType.MEET_UP_LECTURE, "MICE_Screen_Lecture_02"},
			// {eMiceSpaceObjectInteractionType.MEET_UP_AD, "MICE_Screen_AD_03"},
		};

		protected override Dictionary<(string, eMiceSpaceObjectInteractionType), string> ObjectInteractionValueMap { get; } = new Dictionary<(string, eMiceSpaceObjectInteractionType), string>
		{
			{("f-0300", eMiceSpaceObjectInteractionType.LOBBY_AD), ""},
			{("f-0300", eMiceSpaceObjectInteractionType.LOBBY_KIOSK_NORMAL), ""},
			{("f-0300", eMiceSpaceObjectInteractionType.LOBBY_LEAFLET), ""},
			{("f-0300", eMiceSpaceObjectInteractionType.LOBBY_KIOSK_SPECIAL), ""},
			{("f-0301", eMiceSpaceObjectInteractionType.LOUNGE_SPAWN), ""},
			{("f-0301", eMiceSpaceObjectInteractionType.LOUNGE_ENTRANCE_1), ""},
			{("f-0301", eMiceSpaceObjectInteractionType.LOUNGE_ENTRANCE_2), ""},
			{("f-0301", eMiceSpaceObjectInteractionType.LOUNGE_ENTRANCE_3), ""},
			{("f-0301", eMiceSpaceObjectInteractionType.LOUNGE_EXIT), ""},
			{("f-0301", eMiceSpaceObjectInteractionType.LOUNGE_KIOSK_NORMAL), ""},
			{("f-0301", eMiceSpaceObjectInteractionType.LOUNGE_LEAFLET), ""},
			{("f-0301", eMiceSpaceObjectInteractionType.LOUNGE_KIOSK_SPECIAL), ""},
			{("f-0301", eMiceSpaceObjectInteractionType.LOUNGE_AD), ""},
			{("f-0302", eMiceSpaceObjectInteractionType.CONFERENCE_HALL_LECTURE), ""},
			{("f-0302", eMiceSpaceObjectInteractionType.CONFERENCE_HALL_EXIT), ""},
			{("f-0302", eMiceSpaceObjectInteractionType.CONFERENCE_HALL_AD), ""},
			{("f-0303", eMiceSpaceObjectInteractionType.MEET_UP_LECTURE), ""},
			{("f-0303", eMiceSpaceObjectInteractionType.MEET_UP_AD), ""},
			{("f-0304", eMiceSpaceObjectInteractionType.CONFERENCE_HALL_LECTURE), ""},
			{("f-0304", eMiceSpaceObjectInteractionType.CONFERENCE_HALL_EXIT), ""},
			{("f-0304", eMiceSpaceObjectInteractionType.CONFERENCE_HALL_AD), ""},
			{("f-0305", eMiceSpaceObjectInteractionType.CONFERENCE_HALL_LECTURE), ""},
			{("f-0305", eMiceSpaceObjectInteractionType.CONFERENCE_HALL_EXIT), ""},
			{("f-0305", eMiceSpaceObjectInteractionType.CONFERENCE_HALL_AD), ""},
		};

		// protected override IEnumerable<eLogicType> CandidateLogicType { get; } = new[]
		// {
		// 	eLogicType.WARP__SPACE,
		// 	eLogicType.WARP__BUILDING__EXIT,
		// };

		protected override string GetTemplateSceneName(eMiceTemplateType type) => MiceTemplateSceneNameMap?[type];
#endregion // Properties

#region Private Fields
		private static readonly Dictionary<eMiceTemplateType, string> MiceTemplateSceneNameMap = new Dictionary<eMiceTemplateType, string>
		{
			{eMiceTemplateType.LOBBY, "Mice_Lobby_Common_1"},
			{eMiceTemplateType.LOUNGE, "Mice_Lounge_Common_1"},
			{eMiceTemplateType.CONFERENCE_HALL, "Mice_ConferenceHall_Common_1"},
			{eMiceTemplateType.MEET_UP, "Mice_Meetup_Common_1"},
		};
#endregion // Private Fields

#region Menu
		[MenuItem("Com2Verse/SpaceData Extraction/MICE/모두 추출", priority = 0)]
		private static async void RunAll()
		{
			var instance = new MiceSpaceExtract();
			var miceScenePaths = GetAllScenePaths(MiceTemplateSceneNameMap.Values.ToArray());
			var miceSceneServerObjects = await instance.GetSceneServerObjectsAsync(miceScenePaths);
			await instance.ExtractAllAsync(ProgressTaskTitle, miceSceneServerObjects);
		}

		[MenuItem("Com2Verse/SpaceData Extraction/MICE/공간 상호작용 맵 데이터 복사")]
		private static void CopySpaceInteractionMap() => new MiceSpaceExtract().PrintSpaceInteractionMapStr();
#endregion // Menu
	}
}
