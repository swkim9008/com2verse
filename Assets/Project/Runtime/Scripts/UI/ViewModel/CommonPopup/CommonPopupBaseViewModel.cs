/*===============================================================
* Product:		Com2Verse
* File Name:	CommonPopupBaseViewModel.cs
* Developer:	tlghks1009
* Date:			2023-06-13 17:58
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Com2Verse.UI
{
	public class CommonPopupBaseViewModel : ViewModelBase
	{
		private string _title;
		private string _context;

		private bool _allowCloseArea;

		public bool AllowCloseArea
		{
			get => _allowCloseArea;
			set => SetProperty(ref _allowCloseArea, value);
		}

		public string Title
		{
			get => _title;
			set
			{
				_title = value;
				base.InvokePropertyValueChanged(nameof(Title), value);
			}
		}

		public string Context
		{
			get => _context;
			set
			{
				_context = value;
				base.InvokePropertyValueChanged(nameof(Context), value);
			}
		}
	}
}
