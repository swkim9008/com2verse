/*===============================================================
* Product:		Com2Verse
* File Name:	ShareOfficeSpaceExtract.cs
* Developer:	jhkim
* Date:			2023-05-17 10:17
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using UnityEditor;

namespace Com2VerseEditor.SpaceExtract
{
    public sealed class ShareOfficeSpaceExtract : SpaceExtractBase<ShareOfficeSpaceExtract.eShareOfficeTemplateType, ShareOfficeSpaceExtract.eShareOfficeSpaceObjectInteractionType>
    {
        private static readonly int ShareOfficeBuildingId = 3;
        private static readonly int ShareOfficeBuildingType = 2;
        private static readonly int ShareOfficeBuildingCode = 3;
        private static readonly string ProgressTaskTitle = "공유오피스 공간 추출";
        private static readonly string Com2UsAccountID = "컴투스 계정 필요";

        private static readonly long TemplateLobby = 10300101001;
        private static readonly long TemplateModelHuse = 10300401006;

        private static readonly string SpaceTypeOffice = "Office";
        private static readonly string SpaceCodeLobby = "Lobby";
        private static readonly string SpaceCodeTeam = "Team";
        public enum eShareOfficeTemplateType
        {
            LOBBY,
            MODEL_HOUSE,
        }

        public enum eShareOfficeSpaceObjectInteractionType
        {
            LOBBY_SCREEN,
            LOBBY_WARP_MODEL_HOUSE,
            LOBBY_WARP_MODEL_HOUSE2,

            MODEL_HOUSE_EXIT,
        }

#region Properties
        protected override IEnumerable<SpaceInfo<eShareOfficeTemplateType>> SpaceInfos { get; } = new List<SpaceInfo<eShareOfficeTemplateType>>
        {
            new SpaceInfo<eShareOfficeTemplateType> {SpaceId = "f-200", BuildingTableInfo = BuildingTableInfos?[0], SpaceTableInfo = SpaceTableInfos?[0], SpaceDetailTableInfo = SpaceDetailTableInfos?[0], Type = eShareOfficeTemplateType.LOBBY},
            new SpaceInfo<eShareOfficeTemplateType> {SpaceId = "f-201", BuildingTableInfo = BuildingTableInfos?[1], SpaceTableInfo = SpaceTableInfos?[1], SpaceDetailTableInfo = SpaceDetailTableInfos?[1], Type = eShareOfficeTemplateType.MODEL_HOUSE},
        };

        protected override Dictionary<eShareOfficeTemplateType, eShareOfficeSpaceObjectInteractionType[]> SpaceInteractionMap { get; } = new Dictionary<eShareOfficeTemplateType, eShareOfficeSpaceObjectInteractionType[]>
        {
            {
                eShareOfficeTemplateType.LOBBY,
                new[]
                {
                    eShareOfficeSpaceObjectInteractionType.LOBBY_SCREEN,
                    eShareOfficeSpaceObjectInteractionType.LOBBY_WARP_MODEL_HOUSE,
                    eShareOfficeSpaceObjectInteractionType.LOBBY_WARP_MODEL_HOUSE2,
                }
            },
            {
                eShareOfficeTemplateType.MODEL_HOUSE,
                new[]
                {
                    eShareOfficeSpaceObjectInteractionType.MODEL_HOUSE_EXIT,
                }
            },
        };

        protected override Dictionary<eShareOfficeSpaceObjectInteractionType, KeyValuePair<string, string>> ObjectInteractionMap { get; } = new Dictionary<eShareOfficeSpaceObjectInteractionType, KeyValuePair<string, string>>
        {
            {eShareOfficeSpaceObjectInteractionType.LOBBY_SCREEN, new KeyValuePair<string, string>("Office_Screen_Video_01", "Video_Trigger")},
            {eShareOfficeSpaceObjectInteractionType.LOBBY_WARP_MODEL_HOUSE, new KeyValuePair<string, string>("Common_Warp_Main_01", "Common_Warp_Main_01")}, // TODO : 모델하우스 워프 프리펍 교체 후 이름 변경
            {eShareOfficeSpaceObjectInteractionType.LOBBY_WARP_MODEL_HOUSE2, new KeyValuePair<string, string>("Common_Warp_Main_01 (1)", "Common_Warp_Main_01")}, // TODO : 모델하우스 워프 프리펍 교체 후 이름 변경
            {eShareOfficeSpaceObjectInteractionType.MODEL_HOUSE_EXIT, new KeyValuePair<string, string>("Common_Warp_Main_01", "Common_Warp_Main_01")},
        };

        protected override Dictionary<(string, eShareOfficeSpaceObjectInteractionType), string> ObjectInteractionValueMap { get; } = new Dictionary<(string, eShareOfficeSpaceObjectInteractionType), string>
        {
            // LOBBY
            {("f-200", eShareOfficeSpaceObjectInteractionType.LOBBY_SCREEN), "ILL_NHTSFBo"},
            {("f-200", eShareOfficeSpaceObjectInteractionType.LOBBY_WARP_MODEL_HOUSE), "f-201"},
            {("f-200", eShareOfficeSpaceObjectInteractionType.LOBBY_WARP_MODEL_HOUSE2), "f-201"},

            // MODEL_HOUSE
            {("f-201", eShareOfficeSpaceObjectInteractionType.MODEL_HOUSE_EXIT), "f-200"},
        };

        // protected override IEnumerable<eLogicType> CandidateLogicType { get; } = new[]
        // {
        // 	eLogicType.WARP__SPACE,
        // };
        protected override string GetTemplateSceneName(eShareOfficeTemplateType type) => ShareOfficeTemplateSceneNameMap?[type];
#endregion // Properties

#region Private Fields
        private static readonly Dictionary<eShareOfficeTemplateType, string> ShareOfficeTemplateSceneNameMap = new Dictionary<eShareOfficeTemplateType, string>()
        {
            {eShareOfficeTemplateType.LOBBY, "Office_Lobby_WeWork_1_ServerObjects"},
            {eShareOfficeTemplateType.MODEL_HOUSE, "Office_ModelHouse_WeWork_1_ServerObjects"},
        };

        private static readonly BuildingTableInfo[] BuildingTableInfos = new[]
        {
            new BuildingTableInfo {BuildingId = ShareOfficeBuildingId, FloorNo = 1, Location = 1},
            new BuildingTableInfo {BuildingId = ShareOfficeBuildingId, FloorNo = 2, Location = 1},
        };

        private static readonly SpaceTableInfo[] SpaceTableInfos = new[]
        {
            new SpaceTableInfo {No = 1, AccountId = Com2UsAccountID, Name = "공유 오피스 로비", Description = "공유 오피스 로비", TemplateId = TemplateLobby},
            new SpaceTableInfo {No = 2, AccountId = Com2UsAccountID, Name = "모델 하우스", Description = "공유 오피스 모델 하우스", TemplateId = TemplateModelHuse},
        };

        private static readonly SpaceDetailTableInfo[] SpaceDetailTableInfos = new[]
        {
            new SpaceDetailTableInfo {BuildingType = ShareOfficeBuildingType, BuildingCode = ShareOfficeBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeLobby},
            new SpaceDetailTableInfo {BuildingType = ShareOfficeBuildingType, BuildingCode = ShareOfficeBuildingCode, SpaceType = SpaceTypeOffice, SpaceCode = SpaceCodeTeam},
        };
#endregion // Private Fields

#region Menu
        [MenuItem("Com2Verse/SpaceData Extraction/ShareOffice/모두 추출", priority = 0)]
        private static async void RunAll()
        {
            var instance = new ShareOfficeSpaceExtract();
            await instance.ExtractAllAsync(ProgressTaskTitle);
        }

        [MenuItem("Com2Verse/SpaceData Extraction/ShareOffice/공간 상호작용 맵 데이터 복사")]
        private static void CopySpaceInteractionMap() => new ShareOfficeSpaceExtract().PrintSpaceInteractionMapStr();
#endregion // Menu
    }
}
