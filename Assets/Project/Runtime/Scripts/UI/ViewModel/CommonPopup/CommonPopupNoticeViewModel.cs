/*===============================================================
* Product:		Com2Verse
* File Name:	CommonPopupNotice.cs
* Developer:	ydh
* Date:			2022-12-07 19:34
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;

namespace Com2Verse.UI
{
	[ViewModelGroup("CommonPopup")]	
	public sealed class CommonPopupNoticeViewModel : CommonPopupBaseViewModel
	{
		public Action OnCloseEvent { private get; set; }

		public CommandHandler OnCloseClick { get; }

		public CommonPopupNoticeViewModel()
		{
			OnCloseClick = new CommandHandler(OnCloseClicked);
		}

		private void OnCloseClicked()
		{
			OnCloseEvent?.Invoke();
			OnCloseEvent = null;
		}
	}
}
