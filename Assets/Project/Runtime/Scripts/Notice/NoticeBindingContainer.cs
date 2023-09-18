/*===============================================================
 * Product:		Com2Verse
 * File Name:	NoticeBindingContainer.cs
 * Developer:	yangsehoon
 * Date:		2022-12-09 오후 5:11
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
using UnityEngine;

namespace Com2Verse.UI
{
	public class NoticeBindingContainer : MonoBehaviour, IBindingContainer
	{
		public ViewModelContainer ViewModelContainer { get; } = new();
		public Transform GetTransform() => this.transform;

		private Binder[] _binders;

		public void Start()
		{
			ViewModelContainer.ClearAll();

			Bind();
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

		private void OnDestroy()
		{
			_binders = null;
			ViewModelContainer.ClearAll();
		}
	}
}
