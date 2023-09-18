/*===============================================================
* Product:		Com2Verse
* File Name:	GoWorkProcessor.cs
* Developer:	sej
* Date:			2022-12-06 19:48
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Data;
using Com2Verse.EventListen;
using Com2Verse.EventTrigger;
using Com2Verse.UserState;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using Protocols.GameLogic;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.GO__WORK)]
	public class GoWorkProcessor : BaseLogicTypeProcessor
	{
		public override void OnTriggerEnter(TriggerInEventParameter triggerInParameter)
		{
			base.OnTriggerEnter(triggerInParameter);

			this.ShowGoWorkCheckMessage().Forget();
		}
		
		private async UniTaskVoid ShowGoWorkCheckMessage()
		{
			var saveKey = TutorialAttentionState.SaveKey;
			var saveData = await LocalSave.Persistent.LoadJsonAsync<TutorialAttentionState>(saveKey);

			if (saveData == null)
			{
				TutorialAttentionState state = new();
				state.LastAttentionTime = DateTime.Now;
				await LocalSave.Persistent.SaveJsonAsync(saveKey, state);
			}
			else
			{
				if (saveData.IsAlreadyAttended())
					return;

				saveData.LastAttentionTime = DateTime.Now;
				await LocalSave.Persistent.SaveJsonAsync(saveKey, saveData);	
			}
		}
	}
}
