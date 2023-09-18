/*===============================================================
* Product:		Com2Verse
* File Name:	UIView.cs
* Developer:	tlghks1009
* Date:			2022-07-12 11:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using JetBrains.Annotations;

namespace Com2Verse.UI
{
	public class UIView : GUIView
	{
		[UsedImplicitly] public bool SetVisibleState
		{
			get => VisibleState is eVisibleState.OPENED or eVisibleState.OPENING;
			set => SetActive(value);
		}

		[UsedImplicitly] public bool SetVisibleStateReversed
		{
			get => VisibleState is eVisibleState.CLOSED or eVisibleState.CLOSING;
			set => SetActive(!value);
		}

		public bool IsClosed
		{
			get => VisibleState is eVisibleState.CLOSED;
			set { }
		}
	}
}
