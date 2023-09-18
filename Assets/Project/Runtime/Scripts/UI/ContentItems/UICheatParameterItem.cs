/*===============================================================
* Product:		Com2Verse
* File Name:	UICheatParameterItem.cs
* Developer:	jehyun
* Date:			2022-07-14 11:42
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.UI;
using UnityEngine;

namespace Com2Verse
{
	public sealed class UICheatParameterItem : MonoBehaviour, IBindingContainer
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
