/*===============================================================
* Product:		Com2Verse
* File Name:	UIPopupExitServerObjectViewModel.cs
* Developer:	sprite
* Date:			2023-07-11 17:59
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Com2Verse.UI;
using System;
using Com2Verse.Network;
using Com2Verse.Data;

namespace Com2Verse.Mice
{
    [ViewModelGroup("ServerObject")]
    public sealed class UIPopupExitServerObjectViewModel : ViewModel
    {
        private static readonly string UI_ASSET = "UI_Popup_ExitSeverObject";

        public CommandHandler UIPopupExitServerObject_CloseButton { get; private set; }
        public GameObject target { get; private set; }

        private bool _isShowPopup;

        public bool IsShowPopup
        {
            get => _isShowPopup;
            set => SetProperty(ref _isShowPopup, value);
        }


        public static async UniTask<GUIView> ShowView(GameObject target, Action<GUIView> onShow = null, Action<GUIView> onHide = null)
        {
            GUIView view = await UI_ASSET.AsGUIView();

            void OnOpenedEvent(GUIView view)
            {
                onShow?.Invoke(view);

                LayerMaskRemove(Camera.main, LayerMask.NameToLayer("Character"));

                var viewModel = view.ViewModelContainer.GetViewModel<UIPopupExitServerObjectViewModel>();
                if (viewModel != null)
                {
                    viewModel.target = target;
                    viewModel._isShowPopup = true;

                    UIStackManager.Instance.AddByName(nameof(UIPopupExitServerObjectViewModel), viewModel.OnClickCloseButton, eInputSystemState.INTERACTION, true);
                }
            }

            void OnClosedEvent(GUIView view)
            {
                onHide?.Invoke(view);

                view.OnOpenedEvent -= OnOpenedEvent;
                view.OnClosedEvent -= OnClosedEvent;

                LayerMaskAdd(Camera.main, LayerMask.NameToLayer("Character"));

                var viewModel = view.ViewModelContainer.GetViewModel<UIPopupExitServerObjectViewModel>();
                if (viewModel != null)
                {
                    viewModel._isShowPopup = true;

                    UIStackManager.Instance.RemoveByName(nameof(UIPopupExitServerObjectViewModel));
                }
            }

            view.OnOpenedEvent += OnOpenedEvent;
            view.OnClosedEvent += OnClosedEvent;

            view.Show();

            return view;
        }


        public UIPopupExitServerObjectViewModel()
        {
            _isShowPopup = true;

            this.UIPopupExitServerObject_CloseButton = new CommandHandler(OnClickCloseButton);
        }


        void OnClickCloseButton()
        {
            if (this.target == null) return;
            {
                var go = this.target.GetComponent<GachaMachineObject>();
                if(go != null)
                {
                    if (!go.OnCloseAction()) return;
                }

                var go2 = this.target.GetComponent<LeafletScreenObject>();
                if (go2 != null)
                {
                    go2.StopLeafletStandInteraction();
                }
            }

            // 팝업을 끈다.
            IsShowPopup = false;
        }

        public static void LayerMaskRemove(Camera camera, int layerIndex) => camera.cullingMask &= ~(1 << layerIndex);
        public static void LayerMaskAdd(Camera camera, int layerIndex) => camera.cullingMask |= 1 << layerIndex;
    }

}



//if (view.ViewModelContainer.TryGetViewModel(typeof(UIPopupExitServerObjectViewModel), out var viewModel))
//{
//    var myVm = viewModel as UIPopupExitServerObjectViewModel;
//    if (myVm != null)
//    {
//        myVm.target = target;
//    }
//}

////await UniTask.WaitUntil( ()=>view.ViewModelContainer.TryGetViewModel(typeof(UIPopupExitServerObjectViewModel), out viewModel) );
//var viewModel = await view.ViewModelContainer.GetViewModelAsync<UIPopupExitServerObjectViewModel>();
//if (viewModel != null)
//{
//    viewModel.target = target;
//    viewModel._isShowPopup = true;

//    //var myVm = viewModel as UIPopupExitServerObjectViewModel;
//    //if (myVm != null)
//    //{
//    //    myVm.target = target;
//    //    myVm._isShowPopup = true;
//    //}
//}

//if (view.ViewModelContainer.TryGetViewModel(typeof(UIPopupExitServerObjectViewModel), out var viewModel))
//{
//    var myVm = viewModel as UIPopupExitServerObjectViewModel;
//    if (myVm != null)
//    {
//        myVm.target = target;
//        myVm._isShowPopup = true;
//    }
//}



//void OnClickClose()
//{
//    if (this.target == null) return;
//    {
//        var go = this.target.GetComponent<GachaMachineObject>();
//        if (go != null)
//        {
//            go.OnCloseButtonClick();
//        }

//        var go2 = this.target.GetComponent<LeafletScreenObject>();
//        if (go2 != null)
//        {
//            go2.StopLeafletStandInteraction();
//        }
//    }
//}

//if (view.ViewModelContainer.TryGetViewModel(typeof(UIPopupExitServerObjectViewModel), out var viewModel))
//{
//    var myVm = viewModel as UIPopupExitServerObjectViewModel;
//    if (myVm != null)
//    {
//        myVm.target = target;
//    }
//}



//if (view.ViewModelContainer.TryGetViewModel(typeof(UIPopupExitServerObjectViewModel), out var viewModel))
//{
//    var myVm = viewModel as UIPopupExitServerObjectViewModel;
//    if (myVm != null)
//    {
//        myVm.OnClickClose();
//    }
//}