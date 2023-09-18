/*===============================================================
* Product:		Com2Verse
* File Name:	CommonPopupEvent.cs
* Developer:	ksw
* Date:			2023-07-20 10:19
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;

namespace Com2Verse.UI
{
	[ViewModelGroup("CommonPopup")]
	public sealed class CommonPopupEventViewModel : CommonPopupBaseViewModel
	{
		public Action OnCloseEvent { private get; set; }

		public CommandHandler OnCloseClick { get; }

		public CommonPopupEventViewModel()
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
