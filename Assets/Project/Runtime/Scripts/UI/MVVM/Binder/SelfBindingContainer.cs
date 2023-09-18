/*===============================================================
 * Product:		Com2Verse
 * File Name:	SelfBindingContainer.cs
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
	[AddComponentMenu("[CVUI]/[CVUI] SelfBindingContainer")]
	public class SelfBindingContainer : MonoBehaviour, IBindingContainer
	{
		public ViewModelContainer ViewModelContainer { get; } = new();
		public Transform GetTransform() => this.transform;

		private Binder[] _binders;

		public void Start()
		{
			ViewModelManager.Instance.OnClearedHandler += OnCleared;
			Unbind();
			Bind();
		}

		private void OnCleared()
		{
			Unbind();
		}

		public void Bind()
		{
			ViewModelContainer.InitializeViewModel();

			_binders ??= GetComponentsInChildren<Binder>(true);

			foreach (var binder in _binders)
			{
				if (binder.gameObject == this.gameObject)
					continue;

				binder.SetViewModelContainer(ViewModelContainer);

				binder.Bind();
			}
		}

		public void Unbind()
		{
			if (_binders == null)
			{
				ViewModelContainer.ClearAll();
				return;
			}

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
			if (ViewModelManager.InstanceOrNull != null) ViewModelManager.InstanceOrNull.OnClearedHandler -= OnCleared;
			Unbind();
			_binders = null;
		}
	}
}
