/*===============================================================
* Product:		Com2Verse
* File Name:	CommonPopupYesNoViewModel.cs
* Developer:	yangsehoon
* Date:			2022-06-29 12:16
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Utils;

namespace Com2Verse.UI
{
	[ViewModelGroup("CommonPopup")]
	public class CommonPopupYesNoViewModel : CommonPopupBaseViewModel
	{
		public Action<GUIView> OnYesEvent   { private get; set; }
		public Action<GUIView> OnNoEvent    { private get; set; }
		public Action<GUIView> OnCancelEvent { private get; set; }

		public GUIView GuiView { get; set; }


		private string _yes;
		private string _no;
		private string _cancel;

		public CommandHandler OnYesClick   { get; }
		public CommandHandler OnNoClick    { get; }
		public CommandHandler OnCancelClick { get; }

		public CommonPopupYesNoViewModel()
		{
			OnYesClick   = new CommandHandler(OnYesClicked);
			OnNoClick    = new CommandHandler(OnNoClicked);
			OnCancelClick = new CommandHandler(OnCancelClicked);
		}

		private void OnYesClicked()
		{
			OnYesEvent?.Invoke(GuiView);
			OnYesEvent = null;
		}

		private void OnNoClicked()
		{
			OnNoEvent?.Invoke(GuiView);
			OnNoEvent = null;
		}

		private void OnCancelClicked()
		{
			if (OnCancelClick != null)
				OnCancelEvent?.Invoke(GuiView);
			else
				OnNoClicked();
			OnCancelEvent = null;
		}

		public string Yes
		{
			get => string.IsNullOrEmpty(_yes) ? Define.String.UI_Common_Btn_OK : _yes;
			set
			{
				_yes = value;
				base.InvokePropertyValueChanged(nameof(Yes), Yes);
			}
		}

		public string No
		{
			get => string.IsNullOrEmpty(_no) ? Define.String.UI_Common_Btn_Cancel : _no;
			set
			{
				_no = value;
				base.InvokePropertyValueChanged(nameof(No), No);
			}
		}
	}
}
