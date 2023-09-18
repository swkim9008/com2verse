/*===============================================================
* Product:		Com2Verse
* File Name:	FadeController.cs
* Developer:	tlghks1009
* Date:			2022-06-22 11:52
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.UI
{
	public sealed class UIFadeView : GUIView
	{
		public enum eFadeType
		{
			IN,
			OUT
		}

		[SerializeField] private CanvasGroup _canvasGroup;
		
		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
		}
	}
}
