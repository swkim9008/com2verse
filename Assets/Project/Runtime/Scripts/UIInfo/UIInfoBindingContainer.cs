/*===============================================================
* Product:		Com2Verse
* File Name:	UIInfoBindingContainer.cs
* Developer:	tlghks1009
* Date:			2022-12-13 11:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.UI
{
	public sealed class UIInfoBindingContainer : MonoBehaviour, IBindingContainer
	{
		public ViewModelContainer ViewModelContainer { get; } = new();
		public Transform GetTransform() => this.transform;

		private GUIView _rootView;
		private Binder[] _binderList;

		private UIInfo _uiInfo;

		[UsedImplicitly]
		public UIInfo UIInfoViewModel
		{
			get => _uiInfo;
			set
			{
				if (value == null)
					return;

				_uiInfo = value;
				Initialize();
			}
		}

		private void Initialize()
		{
			_rootView = GetComponentInParent<GUIView>();
			_rootView.OnClosedEvent += OnRootViewClosed;

			ViewModelContainer.ClearAll();
			ViewModelContainer.AddViewModel(_uiInfo);

			Bind();
		}

		public void Bind()
		{
			ViewModelContainer.InitializeViewModel();

			_binderList ??= this.GetComponentsInChildren<Binder>(true);

			foreach (var binder in _binderList)
			{
				if (binder.gameObject == this.gameObject)
					continue;

				binder.SetViewModelContainer(ViewModelContainer, true);
				binder.Bind();
			}
		}

		public void Unbind()
		{
			foreach (var binder in _binderList)
			{
				if (binder.IsUnityNull())
				{
					continue;
				}

				if (binder.gameObject == this.gameObject)
				{
					continue;
				}

				binder.Unbind();
			}

			ViewModelContainer.ClearAll();
		}

		private void OnDestroy()
		{
			ViewModelContainer.ClearAll();
			_binderList = null;
		}

		private void OnRootViewClosed(GUIView guiView)
		{
			guiView.OnClosedEvent -= OnRootViewClosed;

			Unbind();
			_binderList = null;
		}
	}
}
