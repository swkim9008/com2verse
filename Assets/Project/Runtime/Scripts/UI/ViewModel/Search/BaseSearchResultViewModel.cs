/*===============================================================
* Product:		Com2Verse
* File Name:	BaseSearchResultViewModel.cs
* Developer:	jhkim
* Date:			2022-10-07 12:26
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;

namespace Com2Verse.UI
{
	[ViewModelGroup("SearchResult")]
	public abstract class BaseSearchResultViewModel : RecyclableCellViewModelBase
	{
#region Variables
		private Transform _transform;
		protected string _searchText;
		private Action<Transform> _onTransformReady;
#endregion // Variables

#region Properties
		public Transform Transform
		{
			get => _transform;
			set
			{
				_transform = value;
				_onTransformReady?.Invoke(_transform);
				InvokePropertyValueChanged(nameof(Transform), value);
			}
		}
		public string SearchText
		{
			get => _searchText;
			set
			{
				_searchText = value;
				InvokePropertyValueChanged(nameof(SearchText), value);
			}
		}
#endregion // Properties

		public void SetOnTransformReady(Action<Transform> onTransformReady) => _onTransformReady = onTransformReady;

		public RectTransform ContentRect { get; set; }
		public Vector2       SizeDelta   { get; set; }
	}
}
