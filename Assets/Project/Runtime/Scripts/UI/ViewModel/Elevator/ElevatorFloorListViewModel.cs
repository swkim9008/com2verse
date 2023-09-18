/*===============================================================
* Product:		Com2Verse
* File Name:	ElevatorFloorListViewModel.cs
* Developer:	tlghks1009
* Date:			2022-09-07 15:57
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;

namespace Com2Verse.UI
{
	[ViewModelGroup("Elevator")]
	public sealed class ElevatorFloorListViewModel : ViewModelBase
	{
		private Collection<ElevatorFloorViewModel> _elevatorFloorCollection = new();

		private string _floor;

		private Action<ElevatorFloorViewModel> _onSelected;

		private string _floorName;

		public ElevatorFloorListViewModel(Action<ElevatorFloorViewModel> onSelected)
		{
			_onSelected = onSelected;
		}


		public Collection<ElevatorFloorViewModel> ElevatorFloorCollection
		{
			get => _elevatorFloorCollection;
			set
			{
				_elevatorFloorCollection = value;
				base.InvokePropertyValueChanged(nameof(_elevatorFloorCollection), value);
			}
		}

		public string Floor
		{
			get => _floor;
			set
			{
				_floor = value;
				base.InvokePropertyValueChanged(nameof(Floor), value);
			}
		}

		public string FloorName
		{
			get => _floorName;
			set
			{
				_floorName = value;
				base.InvokePropertyValueChanged(nameof(FloorName), value);
			}
		}


		public void AddFloor(long mapId, string floorName, int logicType, long fieldId)
		{
			var elevatorFloorViewModel = new ElevatorFloorViewModel(OnFloorSelected)
			{
				MapId = mapId,
				FloorName = floorName,
				LogicType = logicType,
				FieldId = (int)fieldId
			};

			_elevatorFloorCollection.AddItem(elevatorFloorViewModel);
		}

		public void Clear()
		{
			_elevatorFloorCollection.Reset();
		}


		public void DisableAll ()
		{
			foreach(var elevatorFloorViewModel in _elevatorFloorCollection.Value)
			{	
				elevatorFloorViewModel.VisibleState = false;
			}	
		}


		private void OnFloorSelected(ElevatorFloorViewModel selectedElevatorFloorViewModel)
		{
			_onSelected?.Invoke(selectedElevatorFloorViewModel);
		}
	}
}
