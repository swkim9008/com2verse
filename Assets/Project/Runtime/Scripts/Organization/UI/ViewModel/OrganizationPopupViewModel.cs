/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationPopupViewModel.cs
* Developer:	jhkim
* Date:			2022-10-12 18:27
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Organization;
using UnityEngine;

namespace Com2Verse.UI
{
	[ViewModelGroup("Organization")]
	public sealed class OrganizationPopupViewModel : ViewModelBase
	{
		public struct TextKey
		{
			public static string Yes = "UI_Common_Btn_Yes";
			public static string No = "UI_Common_Btn_No";
			public static string Accept = "UI_TeamWorkGroup_Btn_InviteAccept";
			public static string Decline = "UI_TeamWorkGroup_Btn_InviteDeny";
		}

#region Variables
		private string _text;
		private string _yes;
		private string _no;
		private Vector3 _popupPosition = Constant.DefaultPopupPosition;
		public CommandHandler ClickYes { get; }
		public CommandHandler ClickNo { get; }

		private Action<GUIView, bool> _onClick;
		private GUIView _view;
#endregion // Variables

#region Properties
		public string Text
		{
			get => _text;
			set
			{
				_text = value;
				InvokePropertyValueChanged(nameof(Text), value);
			}
		}

		public string Yes
		{
			get => _yes ??= GetLocalizationText(TextKey.Yes);
			set
			{
				_yes = GetLocalizationText(value);
				InvokePropertyValueChanged(nameof(Yes), value);
			}
		}

		public string No
		{
			get => _no ??= GetLocalizationText(TextKey.No);
			set
			{
				_no = GetLocalizationText(value);
				InvokePropertyValueChanged(nameof(No), value);
			}
		}

		public Vector3 PopupPosition
		{
			get => _popupPosition;
			set
			{
				if (value == Vector3.zero)
					_popupPosition = Constant.DefaultPopupPosition;
				else
					_popupPosition = value;
				InvokePropertyValueChanged(nameof(PopupPosition), PopupPosition);
			}
		}
#endregion // Properties

#region View
#endregion // View

#region Initialize
		public OrganizationPopupViewModel()
		{
			ClickYes = new CommandHandler(OnClickYes);
			ClickNo = new CommandHandler(OnClickNo);
		}
#endregion // Initialize

#region Binding Events
		private void OnClickYes() => _onClick?.Invoke(_view, true);
		private void OnClickNo() => _onClick?.Invoke(_view, false);
#endregion // Binding Events

		public void SetOnClick(GUIView view, Action<GUIView, bool> onClick)
		{
			_view = view;
			_onClick = onClick;
		}

		private string GetLocalizationText(string textKey) => Localization.Instance.GetString(textKey);
	}
}
