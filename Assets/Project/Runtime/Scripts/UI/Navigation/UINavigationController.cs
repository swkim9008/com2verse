/*===============================================================
* Product:    Com2Verse
* File Name:  UINavigationController.cs
* Developer:  tlghks1009
* Date:       2022-04-12 15:01
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Utils;

namespace Com2Verse.UI
{
    public sealed class UINavigationController
    {
        private readonly List<GUIView> _usingDimmedPopupViewList = new();
        private readonly List<GUIView> _focusedViewList = new();
        private GUIView _lastFocusedGuiView = null;
        private readonly int _interval = 5;

        private IUpdateOrganizer _updateOrganizer;

        private int _globalSortingOrder = Define.POPUP_SORTING_ORDER;

        public void Initialize(IUpdateOrganizer updateOrganizer)
        {
            HideDimmedPopup();

            _updateOrganizer = updateOrganizer;
            _updateOrganizer?.AddUpdateListener(OnUpdate);

            _focusedViewList.Clear();
            _usingDimmedPopupViewList.Clear();

            _globalSortingOrder = Define.POPUP_SORTING_ORDER;
        }


        public void Release()
        {
            _updateOrganizer?.RemoveUpdateListener(OnUpdate);
            _updateOrganizer = null;

            _focusedViewList.Clear();
            _usingDimmedPopupViewList.Clear();

            _globalSortingOrder = Define.POPUP_SORTING_ORDER;
        }


        public void RegisterEvent(GUIView guiView)
        {
            if (UIManager.Instance.IsLoadedSystemView(guiView))
            {
                return;
            }

            guiView.OnOpeningEvent += OnGuiViewOpening;
            guiView.OnFocusEvent += OnGuiViewFocused;
            guiView.OnClosedEvent += OnGuiViewClosed;
        }

        private void OnGuiViewOpening(GUIView guiView)
        {
            if (UIManager.Instance.IsLoadedSystemView(guiView))
            {
                return;
            }

            if (guiView == _lastFocusedGuiView)
            {
                return;
            }

            UIStackManager.Instance.SetTopmostByObject(guiView.gameObject);
            OnDimmedWithSortingOrderStateWhenFocused(guiView);
            OnFocusedViewStateWhenFocused(guiView);
        }

        private void OnGuiViewFocused(GUIView guiView)
        {
            if (!guiView.UseFocusEvent)
                return;

            if (guiView.VisibleState == GUIView.eVisibleState.OPENED)
            {
                OnGuiViewOpening(guiView);
            }
        }


        private void OnGuiViewClosed(GUIView guiView)
        {
            OnDimmedStateWhenClosed(guiView);

            OnFocusedViewStateWhenClosed(guiView);
        }


        private void OnDimmedWithSortingOrderStateWhenFocused(GUIView guiView)
        {
            if (!guiView.IsStatic)
            {
                guiView.SetSortingOrder(GetSortingOrder());
            }

            if (guiView.NeedDimmedPopup && !_usingDimmedPopupViewList.Contains(guiView))
            {
                ShowDimmedPopup(guiView);

                _usingDimmedPopupViewList.Add(guiView);
            }
        }

        private void OnFocusedViewStateWhenFocused(GUIView guiView)
        {
            if (guiView.IsStatic) return;

            _lastFocusedGuiView = guiView;

            if (!_focusedViewList.Contains(guiView))
            {
                _focusedViewList.Add(_lastFocusedGuiView);
            }
            else
            {
                var oldIndex = _focusedViewList.IndexOf(guiView);

                _focusedViewList.Move(oldIndex, _focusedViewList.Count - 1);
            }
        }


        private void OnDimmedStateWhenClosed(GUIView guiView)
        {
            if (guiView.NeedDimmedPopup)
            {
                _usingDimmedPopupViewList.Remove(guiView);

                if (_usingDimmedPopupViewList.Count > 0)
                {
                    var previousView = _usingDimmedPopupViewList.LastItem();

                    ShowDimmedPopup(previousView);
                }
                else
                    HideDimmedPopup();
            }
        }


        private void OnFocusedViewStateWhenClosed(GUIView guiView)
        {
            if (guiView.IsStatic) return;

            _focusedViewList.Remove(guiView);

            if (_focusedViewList.Count > 0)
            {
                _lastFocusedGuiView = _focusedViewList.LastItem();

                if (!_lastFocusedGuiView.IsUnityNull())
                    _globalSortingOrder = _lastFocusedGuiView!.GetSortingOrder();
            }
            else
            {
                _lastFocusedGuiView = null;

                _globalSortingOrder = Define.POPUP_SORTING_ORDER;
            }
        }


        private void ShowDimmedPopup(GUIView guiView)
        {
            var dimmedPopup = UIManager.Instance.GetSystemView(eSystemViewType.UI_POPUP_DIMMED);

            if (dimmedPopup.IsReferenceNull())
            {
                C2VDebug.LogError("Unable to load DimmedPopup");
                return;
            }

            dimmedPopup.Show();

            dimmedPopup.SetSortingOrder(guiView.GetSortingOrder() - 1);
        }

        private void HideDimmedPopup()
        {
            UIManager.Instance.GetSystemView(eSystemViewType.UI_POPUP_DIMMED)?.Hide();
        }


        private int GetSortingOrder()
        {
            _globalSortingOrder += _interval;

            return _globalSortingOrder;
        }


        private void OnUpdate()
        {
            // TODO : 추 후 인풋시스템과 연결
            // if (Input.GetKeyDown(KeyCode.Escape))
            // 	if (_focusedUINavigation != null)
            // 		Navigate();
        }
    }
}
