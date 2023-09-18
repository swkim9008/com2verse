/*===============================================================
* Product:		Com2Verse
* File Name:	CommonPopupViewModel.cs
* Developer:	tlghks1009
* Date:			2022-06-20 13:19
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Utils;

namespace Com2Verse.UI
{
	[ViewModelGroup("CommonPopup")]
	public class CommonPopupViewModel : CommonPopupBaseViewModel
	{
		public Action OnYesEvent { private get; set; }

		private string _yes;

		public CommandHandler OnYesClick { get; }

		public CommonPopupViewModel()
		{
			OnYesClick = new CommandHandler(OnYesClicked);
		}

		private void OnYesClicked()
		{
			OnYesEvent?.Invoke();
			OnYesEvent = null;
		}

		public string Yes
		{
			get => _yes ?? Define.String.UI_Common_Btn_OK;
			set
			{
				_yes = value;
				base.InvokePropertyValueChanged(nameof(Yes), Yes);
			}
		}
	}
}
