/*===============================================================
* Product:    Com2Verse
* File Name:  RecyclableCell.cs
* Developer:  hyj
* Date:       2022-04-18 09:46
* History:    
* Documents:  
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.EnhancedScroller;
using Com2Verse.UI;
using UnityEngine;

namespace Com2Verse.RecyclableScroll
{
	[AddComponentMenu("[CVUI]/[CVUI] RecyclableCell")]
	public class RecyclableCell : EnhancedScrollerCellView, IBindingContainer
	{
		public ViewModelContainer ViewModelContainer { get; } = new();

		public Transform GetTransform() => this.transform;

		public void Bind()
		{
			var binders = GetComponentsInChildren<Binder>(true);
			foreach (var binder in binders)
			{
				binder.SetViewModelContainer(ViewModelContainer);
				binder.Bind();
			}
		}

		public void Unbind() { }
	}
}
