/*===============================================================
* Product:		Com2Verse
* File Name:	UIChatMessageItem.cs
* Developer:	haminjeong
* Date:			2022-07-14 14:17
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Com2Verse
{
	public sealed class UIChatMessageItem : MonoBehaviour, IBindingContainer
	{
		public ViewModelContainer ViewModelContainer { get; } = new();

		[SerializeField] private UnityEvent _onBindingFinishedEvent;
		public Transform GetTransform() => this.transform;

		public void Bind()
		{
			var binders = GetComponentsInChildren<Binder>(true);
			foreach (var binder in binders)
			{
				binder.SetViewModelContainer(ViewModelContainer);
				binder.Bind();
			}

			_onBindingFinishedEvent?.Invoke();
		}


		public void Unbind() { }
	}
}
