/*===============================================================
* Product:		Com2Verse
* File Name:	IBindingContainer.cs
* Developer:	tlghks1009
* Date:			2022-07-26 10:35
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.UI
{
	public interface IBindingContainer : IBinding
	{
		ViewModelContainer ViewModelContainer { get; }
		Transform GetTransform();
	}

	public interface IViewModelContainerBridge : IBindingContainer
	{
		void SetViewModelContainer(ViewModelContainer viewModelContainer);
	}
}
