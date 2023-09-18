/*===============================================================
* Product:		Com2Verse
* File Name:	UIGraphicSettingItem.cs
* Developer:	ljk
* Date:			2022-07-22 18:26
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.UI
{
	public sealed class UIGraphicSettingItem : MonoBehaviour,IBindingContainer
	{
		private ViewModelContainer _viewModelContainer = new();
		public ViewModelContainer ViewModelContainer => _viewModelContainer;
		public Transform GetTransform() => this.transform;

		public void Bind()
		{
			var binders = GetComponentsInChildren<Binder>(true);
			foreach (var binder in binders)
			{
				_viewModelContainer.CreateInstanceOfViewModel(binder);
				binder.SetViewModelContainer(_viewModelContainer);
				binder.Bind();
			}
		}

		public void Unbind() { }
	}
}
