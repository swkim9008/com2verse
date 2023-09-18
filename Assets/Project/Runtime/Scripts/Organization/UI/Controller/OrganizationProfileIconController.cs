/*===============================================================
* Product:		Com2Verse
* File Name:	OrganizationProfileIconController.cs
* Developer:	jhkim
* Date:			2022-09-30 17:57
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
using UnityEngine;

namespace Com2Verse.UI
{
	public sealed class OrganizationProfileIconController : MonoBehaviour, IBindingContainer
	{
		public ViewModelContainer ViewModelContainer { get; } = new();
		public Transform GetTransform() => this.transform;
		private Binder[] _binders;

		public OrganizationProfileIconViewModel ProfileIconViewModel
		{
			get => ViewModelContainer.GetViewModel<OrganizationProfileIconViewModel>();
			set
			{
				if (value == null) return;

				ViewModelContainer.ClearAll();
				ViewModelContainer.AddViewModel(value);
				Bind();
			}
		}

		public void Bind()
		{
			ViewModelContainer.InitializeViewModel();

			_binders ??= GetComponentsInChildren<Binder>(true);

			foreach (var binder in _binders)
			{
				if (binder.gameObject == this.gameObject)
					continue;

				binder.SetViewModelContainer(ViewModelContainer, true);

				binder.Bind();
			}
		}

		public void Unbind()
		{
			foreach (var binder in _binders)
			{
				if (binder.IsUnityNull()) continue;
				if (binder.gameObject == this.gameObject) continue;

				binder.Unbind();
			}

			ViewModelContainer.ClearAll();
		}
	}
}
