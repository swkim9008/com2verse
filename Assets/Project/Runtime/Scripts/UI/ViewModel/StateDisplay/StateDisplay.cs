/*===============================================================
* Product:		Com2Verse
* File Name:	StateDisplay.cs
* Developer:	ydh
* Date:			2023-07-19 13:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Com2Verse.UI
{
	[ViewModelGroup("Display")]	
	public sealed class StateDisplay : ViewModel
	{
		private string _displayText;

		public string DisplayText
		{
			get => _displayText;
			set => SetProperty(ref _displayText, value);
		}
	}
}