/*===============================================================
* Product:    Com2Verse
* File Name:  GuiViewExtension.cs
* Developer:  tlghks1009
* Date:       2022-04-12 16:30
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System;

namespace Com2Verse.UI
{
	public static class GuiViewExtension
	{
		public static T OnCompleted<T>(this T thisGuiView, Action<GUIView> onCompleted) where T : GUIView
		{
			thisGuiView.OnCompletedEvent += onCompleted;
			return thisGuiView;
		}
	}
}
