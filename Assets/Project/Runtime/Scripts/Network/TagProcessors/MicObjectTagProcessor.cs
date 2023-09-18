// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	MicObjectTagProcessor.cs
//  * Developer:	haminjeong
//  * Date:		2023-07-10 오후 12:50
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using Com2Verse.Communication;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Interaction;

namespace Com2Verse.Network
{
	[TagObjectType(eObjectType.SPEECH_MIC)]
	public class MicObjectTagProcessor : LimitInteractionTagProcessor
	{
		protected override void CurrentUserCountKeyProcess(string value, BaseMapObject mapObject)
		{
			if (!AuditoriumController.Instance.ParameterSourcesMap!.TryGetValue(mapObject, out var source))
			{
				source = new InteractionStringParameterSource(2);
				AuditoriumController.Instance.ParameterSourcesMap.Add(mapObject, source);
			}

			source.SetParameter(0, value);
		}

		public override void UpdateUseObject(BaseMapObject targetObject, bool isUse)
		{
			if (targetObject.IsUnityNull()) return;
			AuditoriumController.Instance.UpdateSpeecherList(targetObject!.ObjectID, isUse);
		}
	}
}
