/*===============================================================
* Product:		Com2Verse
* File Name:	MiceLobbyKioskViewModel.cs
* Developer:	ikyoung
* Date:			2023-08-30 21:21
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Mice;

namespace Com2Verse.UI
{
	[ViewModelGroup("Mice")]
	public sealed partial class MiceLobbyKioskViewModel : ViewModelBase
	{
		private StackRegisterer _guiRegisterer;

		public StackRegisterer GuiRegisterer
		{
			get => _guiRegisterer;
			set
			{
				_guiRegisterer             =  value;
				if (value.IsUnityNull()) return;
				_guiRegisterer!.WantsToQuit += OnBackButtonClick;
			}
		}
		
		private void OnBackButtonClick()
		{
			C2VDebug.LogCategory(GetType().Namespace, "OnBackButtonClick");
			MiceService.Instance.OnEscInput(_guiRegisterer);
		}
	}
}
