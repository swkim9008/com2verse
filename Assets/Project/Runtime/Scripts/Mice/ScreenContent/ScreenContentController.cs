// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	ScreenContentController.cs
//  * Developer:	seaman2000
//  * Date:		2023-04-26 오후 3:48
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using Com2Verse.Extension;
using Com2Verse.UI;
using UnityEngine;

//NoticeBindingContainer

namespace Com2Verse.Mice.ScreenContent
{
	public class ScreenContentController : MonoBehaviour, IBindingContainer
    {
        public ViewModelContainer ViewModelContainer { get; } = new();

        [SerializeField] private int _screenID;

        public Transform GetTransform() => this.transform;

        private Binder[] _binders;

        public int ScreenID
		{
			get => _screenID;
			set { _screenID = value; }
		}

        public void Start()
        {
            ViewModelContainer.ClearAll();

            Bind();

            var viewModel = ViewModelContainer.GetViewModel<MicePdfViewModel>();
            viewModel?.SetScreenID(ScreenID);
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
