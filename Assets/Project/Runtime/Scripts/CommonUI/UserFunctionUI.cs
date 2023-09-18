/*===============================================================
* Product:		Com2Verse
* File Name:	UserFunctionUI.cs
* Developer:	eugene9721
* Date:			2022-08-19 17:25
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Collections.Generic;
using System.Threading;
using Com2Verse.CameraSystem;
using Com2Verse.EventTrigger;
using Com2Verse.Extension;
using Com2Verse.InputSystem;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.PhysicsAssetSerialization;
using Com2Verse.Project.InputSystem;
using Com2Verse.UI;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse
{
    public sealed class UserFunctionUI
    {
        private static readonly string[] GuiViewNameList = new[] { "UI_AvatarInfo", "UI_Communication_Layout_Default" };

        private static readonly int UserFunctionUISortOrder = 0;
        private static readonly int HUD_CHECK_INTERVAL      = 100;

        public GUIView? GuiView { get; set; }

        private readonly Dictionary<long, ActiveObject> _nearByObjects   = new Dictionary<long, ActiveObject>();
        private readonly List<long>                     _nearByObjectIds = new List<long>();

        private CancellationTokenSource?      _cancellationToken;
        private UserFunctionViewModel?        _viewModel;
        private ActiveObjectManagerViewModel? _activeObjectViewModel;

        private long _prevSelectedUserId = 0;

        private async UniTask HudDistanceChecker(CancellationTokenSource cancellationToken)
        {
            while (await UniTaskHelper.Delay(HUD_CHECK_INTERVAL, cancellationToken))
            {
                SetHudSortOrder();
                _viewModel?.RefreshAvatarInfoUI();
            }
        }

        public void AddNearByObject(ActiveObject activeObject)
        {
            _nearByObjects.TryAdd(activeObject.OwnerID, activeObject);
        }

        public void RemoveNearByObject(ActiveObject activeObject)
        {
            _nearByObjects.Remove(activeObject.OwnerID);
        }

        private void SetHudSortOrder()
        {
            if (_activeObjectViewModel == null) return;

            _nearByObjectIds.Clear();
            foreach (var key in _nearByObjects.Keys)
                _nearByObjectIds.Add(key);
            _nearByObjectIds.Sort(SortDistanceComparision);

            for (int i = 0; i < _nearByObjectIds.Count; ++i)
            {
                var currId = _nearByObjectIds[i];
                if (!_activeObjectViewModel.TryGet(currId, out var viewModel)) continue;

                int index = UserFunctionUISortOrder + i;

                if (viewModel.SortOrder != index) viewModel.SortOrder = index;
            }
        }

        /// <summary>
        /// 오브젝트의 거리를 기준으로 내림차순 정렬
        /// </summary>
        /// <param name="a">a오브젝트의 ownerId</param>
        /// <param name="b">b오브젝트의 ownerId</param>
        /// <returns>정렬을 위한 거리 비교 결과</returns>
        private int SortDistanceComparision(long a, long b)
        {
            var mainCamera = CameraManager.InstanceOrNull?.MainCamera;
            if (mainCamera.IsUnityNull()) return 0;
            var cameraPosition = mainCamera!.transform.position;

            if (!_nearByObjects.TryGetValue(a, out var objectA))
                return -1;
            if (!_nearByObjects.TryGetValue(b, out var objectB))
                return 1;

            var disA = Vector3.SqrMagnitude(objectA.transform.position - cameraPosition);
            var disB = Vector3.SqrMagnitude(objectB.transform.position - cameraPosition);
            if (Mathf.Approximately(disA, disB)) return 0;
            return disA < disB ? 1 : -1;
        }

        public void Enable()
        {
            if (_cancellationToken != null) return;
            _cancellationToken = new CancellationTokenSource();
            HudDistanceChecker(_cancellationToken).Forget();

            _activeObjectViewModel = ViewModelManager.Instance.GetOrAdd<ActiveObjectManagerViewModel>();
            AddEvents();
        }

        public void Disable()
        {
            _viewModel             = null;
            _activeObjectViewModel = null;
            _nearByObjects.Clear();

            RemoveEvents();

            if (_cancellationToken == null) return;
            _cancellationToken.Cancel();
            _cancellationToken.Dispose();
            _cancellationToken = null;
        }

        private void AddEvents()
        {
            var actionMapCharacterControl =
                InputSystemManager.InstanceOrNull?.GetActionMap<ActionMapCharacterControl>();
            if (actionMapCharacterControl == null)
            {
                C2VDebug.LogErrorCategory(nameof(UserFunctionUI), " ActionMap Not Found");
                return;
            }

            actionMapCharacterControl.ClickObjectAction        += OnClickObject;
            actionMapCharacterControl.RightClickObjectAction   += OnRightClickObject;
            actionMapCharacterControl.ClickWorldPositionAction += OnClickWorldPositionAction;
            actionMapCharacterControl.CharacterMoveAction      += OnCharacterMove;
        }

        private void RemoveEvents()
        {
            var actionMapCharacterControl =
                InputSystemManager.InstanceOrNull?.GetActionMap<ActionMapCharacterControl>();
            if (actionMapCharacterControl == null)
            {
                return;
            }

            actionMapCharacterControl.ClickObjectAction        -= OnClickObject;
            actionMapCharacterControl.RightClickObjectAction   -= OnRightClickObject;
            actionMapCharacterControl.ClickWorldPositionAction -= OnClickWorldPositionAction;
            actionMapCharacterControl.CharacterMoveAction      -= OnCharacterMove;
        }

        private bool GetViewModel()
        {
            if (_viewModel != null) return true;
            _viewModel = ViewModelManager.Instance.Get<UserFunctionViewModel>();
            return _viewModel != null;
        }

#region ActionMap Events

        private void OnClickObject(Collider collider)
        {
            if (collider.TryGetComponent(out ClickDelegate clickDelegate))
            {
                if (collider.gameObject.activeInHierarchy)
                    clickDelegate.OnClickEvent();
            }

            if (collider.TryGetComponent(out IClickableObject clickableObject))
            {
                if (collider.gameObject.activeInHierarchy && clickableObject.IsClickableEnable)
                    clickableObject.OnClickObject();
            }

            if (collider.TryGetComponent(out C2VEventTrigger eventTrigger))
            {
                if (collider.gameObject.activeInHierarchy)
                    TriggerEventManager.Instance.OnClick(eventTrigger);
            }
            if (!collider.TryGetComponent(out ActiveObject activeObject)) return;
            if (!GetViewModel()) return;
            OffUserMenuUI();
            OffAnotherUserMenuUI();
            OffAnotherUserMenuUIOnCommunication();
        }

        private void OnRightClickObject(Collider collider)
        {
            if (!collider.TryGetComponent(out ActiveObject activeObject)) return;
            if (!GetViewModel()) return;

            if (activeObject.OwnerID == User.Instance.CurrentUserData.ID)
            {
                ToggleUserMenuUI();
                return;
            }

            ToggleAnotherUserMenuUI(activeObject.OwnerID);
        }

        private void OnClickWorldPositionAction(Vector3 position)
        {
            if (!GetViewModel()) return;
            OffUserMenuUI();
            OffAnotherUserMenuUI();
            OffAnotherUserMenuUIOnCommunication();
        }

        private void OnCharacterMove()
        {
            if (!GetViewModel()) return;
            OffUserMenuUI();
            OffAnotherUserMenuUI();
            OffAnotherUserMenuUIOnCommunication();
        }

#endregion ActionMap Events

#region UserFunctionUI On/Off

        public void ToggleUserMenuUI()
        {
            if (!GetViewModel()) return;
            if (_viewModel!.IsOnUserMenuUI) OffUserMenuUI();
            else OnUserMenuUI();
        }

        private void OnUserMenuUI()
        {
            if (!GetViewModel()) return;
            OffAnotherUserMenuUI();
            OffAnotherUserMenuUIOnCommunication();
            _viewModel!.IsOnUserMenuUI = true;
        }

        private void OffUserMenuUI()
        {
            if (_viewModel == null) return;
            _viewModel.IsOnUserMenuUI = false;
        }

        public void ToggleAnotherUserMenuUI(long anotherUserID)
        {
            if (_activeObjectViewModel == null) return;
            if (anotherUserID == 0) return;

            if (_activeObjectViewModel.TryGet(anotherUserID, out var anotherUserViewmodel))
            {
                var prevValue = anotherUserViewmodel.IsOnAnotherUserMenuUI;
                if (prevValue) OffAnotherUserMenuUI();
                else OnAnotherUserMenuUI(anotherUserID);
            }
        }

        private void OnAnotherUserMenuUI(long anotherUserID)
        {
            if (_activeObjectViewModel == null) return;
            if (anotherUserID == 0) return;

            OffAnotherUserMenuUI();
            OffAnotherUserMenuUIOnCommunication();
            OffUserMenuUI();

            if (_activeObjectViewModel.TryGet(anotherUserID, out var anotherUserViewmodel))
                anotherUserViewmodel.IsOnAnotherUserMenuUI = true;

            _prevSelectedUserId = anotherUserID;

            if (_viewModel == null) return;
            _viewModel.IsOnAnotherUserUiOnCommunication = false;
        }

        public void OffAnotherUserMenuUI()
        {
            OffAnotherUserMenuUI(_prevSelectedUserId);
            _prevSelectedUserId = 0;
        }

        private void OffAnotherUserMenuUI(long anotherUserID)
        {
            if (_activeObjectViewModel == null) return;
            if (anotherUserID == 0) return;

            if (_activeObjectViewModel.TryGet(anotherUserID, out var anotherUserViewmodel))
                anotherUserViewmodel.IsOnAnotherUserMenuUI = false;
        }

        private void OffAnotherUserMenuUIOnCommunication()
        {
            if (_viewModel == null) return;
            _viewModel.IsOnAnotherUserUiOnCommunication = false;
        }

#endregion UserFunctionUI On/Off

        public static void SetGUIView(UICanvasRoot uiCanvasRoot)
        {
            if (uiCanvasRoot._guiViewList is not { Count: > 0 })
            {
                C2VDebug.LogWarningCategory(nameof(UserFunctionUI), " Can't find GUIView List!");
            }

            foreach (var guiView in uiCanvasRoot._guiViewList)
            {
                foreach (var guiViewName in GuiViewNameList)
                {
                    if (guiView.name != guiViewName) continue;
                    SetGUIView(guiView);
                    return;
                }
            }

            C2VDebug.LogCategory(nameof(UserFunctionUI), " Can't find GUIView for UserFunction");
            if (uiCanvasRoot != null && uiCanvasRoot._guiViewList.Count > 0)
                SetGUIView(uiCanvasRoot._guiViewList[0]);
        }

        public static void SetGUIView(GUIView guiView)
        {
            if (!User.InstanceExists || User.Instance.UserFunctionUI == null) return;
            User.Instance.UserFunctionUI.GuiView = guiView;
            if (guiView is not DynamicResolutionUIView)
            {
                C2VDebug.LogCategory(nameof(UserFunctionUI), " GuiView is not Dynamic Resolution UI View");
            }
        }

        public static void RemoveGUIView()
        {
            if (!User.InstanceExists) return;
            User.Instance.UserFunctionUI.GuiView = null;
        }
    }
}
