/*===============================================================
* Product:		Com2Verse
* File Name:	TestRedDotCollectionViewModel.cs
* Developer:	NGSG
* Date:			2023-04-27 14:45
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
	public class RedDotCollectionTestData
	{
		public int _id;
		public string _data;

		public RedDotCollectionTestData(int id, string data)
		{
			_id   = id;
			_data = data;
		}
	}
	
	[ViewModelGroup("TestRedDot")]
	public sealed class TestRedDotCollectionViewModel : ViewModelBase, IRedDotCollection
	{
		public RedDotData RedDot { get; set; }
		public bool IsCreate { get; set; }

		public RedDotCollectionTestData Info { get; set; }
		//private readonly BindableProperty<RedDotCollectionTestData> _Info = new();

		public  CommandHandler OnSelectClick { get; private set; }
		private Action<TestRedDotCollectionViewModel> _onSelected;

		private bool _selected = false;
		public bool Selected
		{
			get => _selected;
			set
			{
				_selected = value;
				if (value)
				{
					_onSelected?.Invoke(this);
				}

				base.InvokePropertyValueChanged(nameof(Selected), value);
			}
		}

		public string InfoData
		{
			get => Info._data;
			set
			{
				Info._data = value;
				base.InvokePropertyValueChanged(nameof(InfoData), value);
			}
		}
		public TestRedDotCollectionViewModel(RedDotCollectionTestData info, RedDotData redDot, Action<TestRedDotCollectionViewModel> onSelected)
		{
			Selected = false;
			
			Info = info;
			RedDot = redDot;
			_onSelected = onSelected;
			
			OnSelectClick = new CommandHandler(() =>
			{
				Selected = true;
			});
		}
	}
}
