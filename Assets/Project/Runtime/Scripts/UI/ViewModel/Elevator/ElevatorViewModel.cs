/*===============================================================
* Product:		Com2Verse
* File Name:	ElevatorViewModel.cs
* Developer:	tlghks1009
* Date:			2022-06-17 13:12
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Com2Verse.InputSystem;
using Com2Verse.Network;
using Protocols.GameLogic;

namespace Com2Verse.UI
{
    [ViewModelGroup("Elevator")]
    public sealed class ElevatorViewModel : ViewModelBase
    {
        private Dictionary<string, ElevatorFloorListViewModel> _elevatorFloorListDict = new();

        private Collection<ElevatorFloorListViewModel> _elevatorFloorListCollection = new();

        public CommandHandler Command_MoveToButtonClick { get; }

        private ElevatorFloorViewModel _selectedElevatorFloorViewModel;

        private bool _isRequestedUsePortal;

        private bool _setActive;

        public float ScrollReset
        {
            get => 0;
            set { }
        }

        public bool SetActive
        {
            get => _setActive;
            set
            {
                ViewModelManager.Instance.Get<InteractionUIListViewModel>()?.Show(!value);
                _setActive = value;
                base.InvokePropertyValueChanged(nameof(SetActive), value);
            }
        }


        public ElevatorViewModel()
        {
            Command_MoveToButtonClick = new CommandHandler(OnMoveToButtonClicked);
        }


        public override void OnInitialize()
        {
            base.OnInitialize();

            _isRequestedUsePortal = false;
            _setActive = true;

            _selectedElevatorFloorViewModel = null;
        }

        public override void OnRelease()
        {
            base.OnRelease();

            if (_isRequestedUsePortal)
            {
                
            }
        }

#region Elevator
        public void InitializeElevator(int logicType, List<Portal> portalList)
        {
            portalList.Reverse();
            foreach (var portal in portalList) // (TODO) Floor List
            {
                // 현재 서버에서는 그냥 parameter id를 주고있슴
                // FIXME : text Key
                if (portal.MapId == 1)
                    AddFloorList(portal.MapId, "1F", "월드(테스트 입장입니다)", "컴투버스 월드(테스트 입니다)", logicType, portal.FieldId);
                else
                {
                    var floorInfo = OfficeInfo.Instance.GetFloorInfo(portal.MapId);
                    if (floorInfo == null) continue;
                    AddFloorList(portal.MapId, floorInfo.Floor, 
                                 $"{Localization.Instance.GetString(floorInfo.CompanyName)} {Localization.Instance.GetString(floorInfo.FloorName)}",
                                 Localization.Instance.GetString(floorInfo.DeptName), logicType, portal.FieldId);
                }
            }
        }
#endregion Elevator

        private void AddFloorList(long mapId, string floor, string totalFloorName, string floorName, int logicType, long fieldId)
        {
            if (!_elevatorFloorListDict.TryGetValue(floor!, out var elevatorFloorListViewModel))
            {
                elevatorFloorListViewModel = new ElevatorFloorListViewModel(OnSelected)
                {
                    FloorName = totalFloorName,
                    Floor = floor
                };
                elevatorFloorListViewModel.AddFloor(mapId, floorName, logicType, fieldId);

                _elevatorFloorListDict.Add(floor, elevatorFloorListViewModel);

                _elevatorFloorListCollection.AddItem(elevatorFloorListViewModel);
                return;
            }

            elevatorFloorListViewModel.AddFloor(mapId, floorName, logicType, fieldId);
        }


        public void ClearElevator()
        {
            _elevatorFloorListDict.Clear();

            foreach (var elevatorFloorListViewModel in _elevatorFloorListCollection.Value)
            {
                elevatorFloorListViewModel.Clear();
            }

            _elevatorFloorListCollection.Reset();
        }


        public Collection<ElevatorFloorListViewModel> ElevatorFloorListCollection
        {
            get => _elevatorFloorListCollection;
            set
            {
                _elevatorFloorListCollection = value;
                base.InvokePropertyValueChanged(nameof(ElevatorFloorListCollection), value);
            }
        }

        private void OnMoveToButtonClicked()
        {
            if (_selectedElevatorFloorViewModel == null)
            {
                return;
            }

            RequestUsePortal();
        }

        private void RequestUsePortal()
        {
            if (NetworkUIManager.Instance.CurrentMapId != _selectedElevatorFloorViewModel.MapId)
            {
                _isRequestedUsePortal = true;

                Commander.Instance.TeleportUserRequest(_selectedElevatorFloorViewModel.MapId, _selectedElevatorFloorViewModel.FieldId);
            }

            SetActive = false;

            _selectedElevatorFloorViewModel = null;
        }


        private void OnSelected(ElevatorFloorViewModel selectedElevatorFloorViewModel)
        {
            foreach (var elevatorFloorListViewModel in _elevatorFloorListCollection.Value)
            {
                elevatorFloorListViewModel.DisableAll();
            }

            _selectedElevatorFloorViewModel = selectedElevatorFloorViewModel;

            _selectedElevatorFloorViewModel.VisibleState = true;
        }
    }
}
