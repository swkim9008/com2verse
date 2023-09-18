/*===============================================================
* Product:		Com2Verse
* File Name:	ElevatorFloorViewModel.cs
* Developer:	tlghks1009
* Date:			2022-06-20 12:45
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;

namespace Com2Verse.UI
{
	[ViewModelGroup("Elevator")]
	public sealed class ElevatorFloorViewModel : ViewModelBase
	{
		private Action<ElevatorFloorViewModel> _onSelectedHandler;

		private string _floorName;
		private bool _visibleState;

		public CommandHandler Selected { get; }

		public int FieldId { get; set; }
		public long MapId { get; set; }
		public int LogicType { get; set; }


		public ElevatorFloorViewModel(Action<ElevatorFloorViewModel> onSelectedHandler)
		{
			_onSelectedHandler = onSelectedHandler;

			Selected = new CommandHandler(OnSelected);
			VisibleState = false;
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

		public bool VisibleState
		{
			get => _visibleState;
			set
			{
				_visibleState = value;
				base.InvokePropertyValueChanged(nameof(VisibleState), value);
			}
		}

		private void OnSelected()
		{
			_onSelectedHandler?.Invoke(this);
		}
	}
}
