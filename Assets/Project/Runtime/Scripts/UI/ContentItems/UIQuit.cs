/*===============================================================
* Product:		Com2Verse
* File Name:	UIQuit.cs
* Developer:	jhkim
* Date:			2022-09-13 23:30
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.UI;
using Com2VerseEditor;
using UnityEditor;
using UnityEngine;

namespace Com2Verse
{
	public sealed class UIQuit : MonoBehaviour
	{
		private string _titleKey = "UI_Exit_Popup_Title";
		private string _contextKey = "UI_Exit_Popup_Msg";
		
		public void OnClick()
		{
#if UNITY_EDITOR
			UIManager.Instance.ShowPopupYesNo(Localization.Instance.GetString(_titleKey), 
				Localization.Instance.GetString(_contextKey),
				(guiView) =>
				{
					EditorApplicationUtil.ExitPlayMode();
				});
#else
			UIManager.Instance.ShowPopupYesNo(
				Localization.Instance.GetString(_titleKey),
				Localization.Instance.GetString(_contextKey),
				(guiView) =>
				{
					Application.Quit();
				});
#endif // UNITY_EDITOR
		}
	}
}
