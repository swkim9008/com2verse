/*===============================================================
* Product:		Com2Verse
* File Name:	DebugSpaceExtract.cs
* Developer:	jhkim
* Date:			2023-05-17 13:02
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Com2Verse.Data;
using Cysharp.Threading.Tasks;
using UnityEditor;

namespace Com2VerseEditor.SpaceExtract
{
	public sealed class DebugSpaceExtract : SpaceExtractBase<DebugSpaceExtract.eDebugSpaceType, DebugSpaceExtract.eDebugSpaceObjectInteractionType>
	{
		public enum eDebugSpaceType
		{
		}

		public enum eDebugSpaceObjectInteractionType
		{
		}

#region Menu
		private static string _prevSelectedFolder = string.Empty;
		[MenuItem("Com2Verse/SpaceData Extraction/Debug/모든 서버 오브젝트, 트리거 추출")]
		private static async void ExtractAll()
		{
			var instance = new DebugSpaceExtract();
			var serverObjectInfos = await instance.GetSceneServerObjectsInAllSceneAsync();

			_prevSelectedFolder = EditorUtility.SaveFolderPanel("저장 폴더 선택", _prevSelectedFolder, string.Empty);
			if (string.IsNullOrWhiteSpace(_prevSelectedFolder)) return;

			await instance.ExtractServerObjectTableAsync(serverObjectInfos, _prevSelectedFolder, "AllSceneServerObject");
			await instance.ExtractServerObjectEventTriggerTableAsync(serverObjectInfos, _prevSelectedFolder, "AllEventTriggers");
		}

		[MenuItem("Com2Verse/SpaceData Extraction/Misc/오피스 공간 오브젝트 정보 복사")]
		private static async UniTaskVoid PrintSpaceTemplateArrangement()
		{
			var table = await LoadTableAsync<TableSpaceTemplateArrangement>($"{nameof(SpaceTemplateArrangement)}.bytes");

			var sb = new StringBuilder();
			sb.AppendLine("SpaceTemplateID, SpaceTemplateArrangementID, SpaceObjectID, InteractionLinkID, InteractionNo");

			var templateIdFilter = new long[]
			{
				30300101000,
				30300401000,
				10300401002,
				10300401003,
				10300401004,
				10300401005,
			};

			var interactionBaseObjectMap = new Dictionary<long, long[]>()
			{
				{30300101000, new long[] {1186}},
				{30300401000, new long[] {2131, 1221, 1211, 1270}},
			};

			var interactionInfoMap = new Dictionary<(long, long), (long, long, long)[]>
			{
				{(30300101000, 1186), new (long, long, long)[] {(1, 1186101, 1)}},
				{(30300401000, 1213), new (long, long, long)[] {(4, 1213102, 1), (4, 1213102, 2)}},
				{(30300401000, 1221), new (long, long, long)[] {(27, 1221101, 1)}},
				{(30300401000, 1211), new (long, long, long)[] {(3, 1211101, 1)}},
				{(30300401000, 1270), new (long, long, long)[] {(31, 1270101, 1)}},
			};

			foreach (var (key, value) in table.Datas)
			{
				if (templateIdFilter.Contains(value.TemplateID))
				{
					if (interactionInfoMap.TryGetValue((value.TemplateID, value.BaseObjectID), out var interactionInfo))
					{
						foreach (var (spaceObjectId, interactionLinkId, interactionNo) in interactionInfo)
							sb.AppendLine($"{value.TemplateID}, {key}, {spaceObjectId}, {interactionLinkId}, {interactionNo}");
					}
				}
			}

			EditorGUIUtility.systemCopyBuffer = sb.ToString();
			EditorUtility.DisplayDialog("알림", "클립보드로 복사되었습니다.", "확인");
		}
#endregion // Menu
	}
}
