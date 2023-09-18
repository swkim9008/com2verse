/*===============================================================
* Product:		Com2Verse
* File Name:	MiceEffectControllerTagProcessor.cs
* Developer:	ikyoung
* Date:			2023-07-18 19:32
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Mice;
using System.Collections.Generic;
using System;

namespace Com2Verse.Network
{
	[Serializable]
	public sealed class MiceEffectControlTag
	{
		public int  EffectMiceID; // EffectMICE.csv의 PrimaryKey
		public bool IsOn;
		public long UpdateTimeTick;
	}

	[TagObjectType(eObjectType.MICE_EFFECT_CONTROLLER)]
	public sealed class MiceEffectControllerTagProcessor : BaseTagProcessor
	{
		private static readonly Dictionary<string, eEffectMICEType> KeyMaps = new()
		{
			{ "BGM", eEffectMICEType.BGM },
			{ "SoundEffect", eEffectMICEType.SOUND_EFFECT },
			{ "StageEffect", eEffectMICEType.STAGE_EFFECT },
			{ "StageLighting", eEffectMICEType.STAGE_LIGHTING },
			{ "CombEffect", eEffectMICEType.COMB_EFFECT },
		};

		public override void Initialize()
		{
			foreach (var kvp in KeyMaps)
			{
				var key        = kvp.Key;
				var effectType = kvp.Value;
				SetDelegates(key, (value, mapObject) =>
				{
					var tagValue = JsonUtility.FromJson<MiceEffectControlTag>(value);
					if (tagValue == null) return;

					if (!IsTagValid(effectType, tagValue)) return;
					ProcessTag(tagValue);
				});
			}
		}

		private bool IsTagValid(eEffectMICEType type, MiceEffectControlTag tag)
		{
			var tableData = TableDataManager.Instance.Get<TableMiceEffect>();
			if (tableData == null) return false;
			if (!tableData.Datas.TryGetValue(tag.EffectMiceID, out var data)) return false;

			return data.EffectMICEType == type;
		}

		private void ProcessTag(MiceEffectControlTag tag)
		{
			var instance = MiceEffectManager.Instance;
			if (instance.IsUnityNull()) return;

			if (tag.IsOn)
			{
				instance.PlayEffect(tag.EffectMiceID, tag.UpdateTimeTick);
			}
			else
			{
				instance.StopEffect(tag.EffectMiceID, tag.UpdateTimeTick);
			}
		}
	}
}
