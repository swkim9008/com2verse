/*===============================================================
* Product:		Com2Verse
* File Name:	RedDotViewModel.cs
* Developer:	NGSG
* Date:			2023-04-17 16:40
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com2Verse.Extension;

namespace Com2Verse.UI
{
	public interface IRedDotBase
	{
		public RedDotData RedDot { get; set; }
	}
	public interface IRedDot : IRedDotBase
	{
		public bool IsRedDot   { get; set; }
		public string BadgeCount { get; set; }
	}
	public interface IRedDotCollection : IRedDotBase
	{
		public bool IsCreate { get; set; }
	}
	
	public class RedDotViewModel : ViewModel, IRedDot
	{
		public RedDotData RedDot    { get; set; }

		private bool _isRedDot = false;
		public bool IsRedDot
		{
			get => _isRedDot;
			//get => RedDot.IsRedDot;
			set
			{
				_isRedDot       = value;
				//RedDot.IsRedDot = value;
				base.InvokePropertyValueChanged(nameof(IsRedDot), value);
				Debug.Log($"[RedDotViewModel] IsRedDot = {value}");
			}
		}

		private string _badgeCount;
		public string BadgeCount
		{
			get => _badgeCount;
			set
			{
				_badgeCount = value;
				base.InvokePropertyValueChanged(nameof(BadgeCount), value);
			}
		}
		
		public RedDotViewModel()
		{
		}

		public RedDotViewModel(RedDotData data) : this()
		{
			RedDot   = data;
		}
	}
}
