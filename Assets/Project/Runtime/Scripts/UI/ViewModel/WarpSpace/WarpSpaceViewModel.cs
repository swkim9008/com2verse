/*===============================================================
* Product:		Com2Verse
* File Name:	WarpSpaceViewModel.cs
* Developer:	jhkim
* Date:			2023-06-30 14:39
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Cheat;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.Office.WarpSpace;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;

namespace Com2Verse.UI
{
    [ViewModelGroup("WarpSpace")]
    public sealed class WarpSpaceViewModel : ViewModelBase, IDisposable
    {
#region Variable
        private static readonly string ResName = "UI_Popup_Elevator";
        private static GUIView _view;

        private readonly WarpSpaceController _controller;

        private string _groupName;

        private static readonly int UnselectedPid = -1;
        private Collection<WarpSpaceGroupViewModel> _groups = new();
#endregion // Variable

#region Properties
        [UsedImplicitly]
        public string GroupName
        {
            get => _groupName;
            set => SetProperty(ref _groupName, value);
        }

        [UsedImplicitly]
        public Collection<WarpSpaceGroupViewModel> Groups
        {
            get => _groups;
            set
            {
                _groups = value;
                SetProperty(ref _groups, value);
            }
        }

        [UsedImplicitly] public CommandHandler Move { get; private set; }
        [UsedImplicitly] public CommandHandler Close { get; private set; }

        private int _selectedSpacePid = UnselectedPid;
#endregion // Properties

#region Initialize
        public WarpSpaceViewModel()
        {
            _controller = new();

            Move = new CommandHandler(OnMove, null);
            Close = new CommandHandler(OnClose, null);
        }
#endregion // Initialize

#region View
        public static async UniTask ShowAsync(Action<GUIView> onSetCustomInfo = null, bool useDummy = false)
        {
            Hide();

            // UIManager.Instance.ShowWaitingResponsePopup();

            var viewModel = ViewModelManager.Instance.GetOrAdd<WarpSpaceViewModel>();
            var success = await viewModel.LoadAndSetGroupsAsync(useDummy);
            if (success)
            {
                UIManager.Instance.CreatePopup(ResName, async view =>
                {
                    if (view.IsUnityNull()) return;

                    _view = view;
                    _view.Show();

                    if (onSetCustomInfo != null)
                        _view.OnCompletedEvent += onSetCustomInfo;

                    viewModel.Clear();
                    // UIManager.Instance.HideWaitingResponsePopup();
                }).Forget();
            }
            else
            {
                // UIManager.Instance.HideWaitingResponsePopup();
                MyPadManager.Instance.ShowErrorPopup(MyPadManager.eErrorPopup.INVALID_SERVICE);
            }
        }
        private static void Hide()
        {
            _view?.Hide();
            _view = null;
        }
#endregion // View

        private void Clear()
        {
            _selectedSpacePid = UnselectedPid;
        }

        private async UniTask<bool> LoadAndSetGroupsAsync(bool useDummy = false)
        {
            var success = await _controller.LoadAsync(useDummy);
            if (!success) return false;

            GroupName = _controller.GroupName;
            SetGroups();
            return true;
        }
        private void SetGroups()
        {
            Groups.Reset();

            foreach (var groupModel in _controller.Groups)
                Groups.AddItem(new WarpSpaceGroupViewModel(groupModel, OnSelectItem));
        }

        private void OnSelectItem(int pid)
        {
            _selectedSpacePid = pid;
            foreach (var item in Groups.Value)
                item.DeselectItemsExcept(_selectedSpacePid);
        }
#region Binding Events
        private async void OnMove()
        {
            C2VDebug.Log($"MOVE = {Convert.ToString(_selectedSpacePid)}");

            if (_selectedSpacePid == UnselectedPid)
                return;

            Hide();
            await _controller.WarpToAsync(_selectedSpacePid);
        }

        private void OnClose() => Hide();
#endregion // Binding Events

#region Dispose
        public void Dispose()
        {
            Hide();
        }
#endregion // Dispose

#region Cheat
        [MetaverseCheat("Cheat/Office/WarpSpace")]
        private static void ShowWarpSpaceCheat()
        {
            ShowAsync(null, true).Forget();
        }
#endregion // Cheat
    }
}
