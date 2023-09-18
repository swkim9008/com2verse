/*===============================================================
* Product:		Com2Verse
* File Name:	Tutorial.cs
* Developer:	ydh
* Date:			2023-01-04 14:03
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;

namespace Com2Verse.UserState
{
	public class TutorialAttentionState
	{
		public DateTime LastAttentionTime = DateTime.MinValue;
		public static string SaveKey { get; set; }
		
		private Dictionary<string, bool> _tutorial = new()
		{
			{ "", false},
		};

		public bool IsAlreadyAttended()
		{
			if (DateTime.Now.Year == LastAttentionTime.Year)
			{
				if (DateTime.Now.Month == LastAttentionTime.Month)
				{
					return DateTime.Now.Day - LastAttentionTime.Day < 1;
				}
			}
			
			return false;
		}
		
		public static async UniTaskVoid TutorialCheck(string tutorialKey, bool isCached = true, Action tutorialOkAction = null, Action tutorialNoAction = null)
		{
			if (isCached)
			{
				tutorialOkAction?.Invoke();
				return;
			}
			
			var saveData = await LocalSave.Persistent.LoadJsonAsync<TutorialAttentionState>(SaveKey);

			if (saveData == null)
			{
				TutorialAttentionState state = new();
				tutorialOkAction?.Invoke();
				if(state._tutorial.ContainsKey(tutorialKey))
					state._tutorial[tutorialKey] = true;
				await LocalSave.Persistent.SaveJsonAsync(SaveKey, state);
			}
			else
			{
				if (!saveData._tutorial.ContainsKey(tutorialKey))
					return;

				if (!saveData._tutorial[tutorialKey])
				{
					tutorialOkAction?.Invoke();
					saveData._tutorial[tutorialKey] = true;
					await LocalSave.Persistent.SaveJsonAsync(SaveKey, saveData);
				}
				else
					tutorialNoAction?.Invoke();
			}
		}
	}
}