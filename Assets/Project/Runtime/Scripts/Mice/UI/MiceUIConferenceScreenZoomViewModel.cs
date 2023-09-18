/*===============================================================
* Product:		Com2Verse
* File Name:	MiceUIConferenceScreenZoomViewModel.cs
* Developer:	sprite
* Date:			2023-06-19 12:44
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using Cysharp.Threading.Tasks;
using Com2Verse.Logger;
using System;
using Com2Verse.UI;
using System.IO;
using System.Threading;

namespace Com2Verse.Mice
{
	public sealed partial class MiceUIConferenceScreenZoomViewModel : MiceViewModel, IInputSystemHelper
    {
        /// <summary>
        /// View 리소스
        /// </summary>
        public enum PopupAssets
        {
            UI_Conference_ScreenZoom
        }

        private RectTransform _videoRenderer;

        public RectTransform VideoRenderer
        {
            get => _videoRenderer;
            set
            {
                _videoRenderer = value;
                this.InvokePropertyValueChanged(nameof(VideoRenderer), VideoRenderer);
            }
        }

        public void OnShowGUIView(GUIView guiView)
        {
            this.ChangeUIInputSystemState();
        }

        public void OnHideGUIView(GUIView guiView)
        {
            this.ResetInputSystemState();
        }
    }

    public sealed partial class MiceUIConferenceScreenZoomViewModel // Show/Hide View
    {
        // 이 GUIView 는 Only One이므로, 가능.
        private static Func<GUIView> _hideView;

        public static UniTask<GUIView> ShowView(Action<Transform> onShow, Action onHide = null)
            => MiceViewModel.ShowView
            (
                PopupAssets.UI_Conference_ScreenZoom,
                onShow: v =>
                {
                    C2VDebug.LogCategory("ConferenceScreenZoom", "[GUIView]: onShow");

                    UnityEngine.Assertions.Assert.IsTrue(onShow != null);

                    _hideView = v.Hide;

                    var vm = v.ViewModelContainer.GetViewModel<MiceUIConferenceScreenZoomViewModel>();
                    vm.OnShowGUIView(v);

                    onShow(vm._videoRenderer.transform);
                },
                onHide: v =>
                {
                    C2VDebug.LogCategory("ConferenceScreenZoom", "[GUIView]: onHide");

                    _hideView = null;

                    var vm = v.ViewModelContainer.GetViewModel<MiceUIConferenceScreenZoomViewModel>();
                    vm.OnHideGUIView(v);

                    onHide?.Invoke();
                }
            );

        public static async UniTask HideView(int millisecondsDelay = 0, CancellationToken cancellationToken = default)
        {
            if (millisecondsDelay > 0)
            {
                await UniTask.Delay(millisecondsDelay, true, cancellationToken: cancellationToken);
            }

            _hideView?.Invoke();

            // View가 감춰 질 때 까지 대기...
            await UniTask.WaitUntil(() => _hideView != null, cancellationToken: cancellationToken);
        }

        public static void ToggleView(bool value, Action<Transform> onShow, Action onHide = null)
        {
            if (value)
            {
                MiceUIConferenceScreenZoomViewModel.ShowView(onShow, onHide).Forget();
            }
            else
            {
                MiceUIConferenceScreenZoomViewModel.HideView().Forget();
            }
        }
    }

    public interface IInputSystemHelper
    { 
    }

    public static partial class InputSystemHelperExtensions
    {
        /// <summary>
        /// 주어진 상태로 입력 시스템 상태를 변경한다.
        /// </summary>
        /// <param name="placeHolder"></param>
        /// <param name="state"></param>
        public static void ChangeInputSystemState(this IInputSystemHelper placeHolder, Data.eInputSystemState state)
            => Project.InputSystem.InputSystemManagerHelper.ChangeState(state);

        /// <summary>
        /// UI 입력 시스템 상태로 변경한다.
        /// </summary>
        /// <param name="placeHolder"></param>
        public static void ChangeUIInputSystemState(this IInputSystemHelper placeHolder)
            => Project.InputSystem.InputSystemManagerHelper.ChangeState(Data.eInputSystemState.UI);

        /// <summary>
        /// 이전 입력 시스템 상태로 변경한다.
        /// </summary>
        /// <param name="placeHolder"></param>
        public static void ResetInputSystemState(this IInputSystemHelper placeHolder)
            => InputSystem.InputSystemManager.InstanceOrNull?.ChangePreviousActionMap();
    }
}
