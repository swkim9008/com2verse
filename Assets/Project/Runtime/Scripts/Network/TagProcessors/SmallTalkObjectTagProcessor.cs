// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	SmallTalkObjectTagProcessor.cs
//  * Developer:	yangsehoon
//  * Date:		2023-06-23 오후 12:50
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using Com2Verse.Data;
using Com2Verse.Interaction;
using Com2Verse.SmallTalk.SmallTalkObject;

namespace Com2Verse.Network
{
	public abstract class LimitInteractionTagProcessor : BaseTagProcessor
	{
		private const string CurrentUserCountKey   = "CurInteractionUseCount";

		public override void Initialize()
		{
			SetDelegates(CurrentUserCountKey,   CurrentUserCountKeyProcess);
		}

		protected abstract void CurrentUserCountKeyProcess(string value, BaseMapObject mapObject);
	}
	
	[TagObjectType(eObjectType.SMALL_TALK_VOICE)]
	public class SmallTalkObjectTagProcessor : LimitInteractionTagProcessor
	{
		protected override void CurrentUserCountKeyProcess(string value, BaseMapObject mapObject)
		{
			if (!SmallTalkObjectManager.Instance.ParameterSourcesMap.TryGetValue(mapObject, out var source))
			{
				source = new InteractionStringParameterSource(2);
				SmallTalkObjectManager.Instance.ParameterSourcesMap.Add(mapObject, source);
			}

			source.SetParameter(0, value);
		}
	}
}
