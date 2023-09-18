/*===============================================================
* Product:		Com2Verse
* File Name:	RecyclableCellViewModel.cs
* Developer:	eugene9721
* Date:			2022-07-20 16:53
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.UI
{
	/// <summary>
	/// 다국어 지원이 필요한 경우 RecyclableCellViewModelBase 사용
	/// </summary>
	public abstract class RecyclableCellViewModel : ViewModel
	{
		private readonly BindableProperty<RectTransform> _contentRect;
		private readonly BindableProperty<Vector2>       _sizeDelta;

		[CanBeNull] [UsedImplicitly]
		public RectTransform ContentRect
		{
			get => _contentRect.Value;
			set
			{
				_contentRect.Value = value;
				InvokePropertyValueChanged("ContentRect");
			}
		}

		[UsedImplicitly]
		public Vector2 SizeDelta
		{
			get => _sizeDelta.Value;
			set
			{
				_sizeDelta.Value = value;
				InvokePropertyValueChanged("SizeDelta");
			}
		}
	}
}
