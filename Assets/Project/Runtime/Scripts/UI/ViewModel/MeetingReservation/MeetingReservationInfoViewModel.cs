/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingReservationInfoViewModel.cs
* Developer:	tlghks1009
* Date:			2022-09-04 11:56
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.UI
{
	[ViewModelGroup("MeetingReservation")]
	public sealed class MeetingReservationInfoViewModel : ViewModelBase
	{
		private Vector2 _height;
		private string _hour;
		private bool _setActiveUnableToReserveText;


		public MeetingReservationInfoViewModel(int hour, int minute)
		{
			Hour = minute != 30 ? $"{hour:00}:00" : "-";
		}

		public string Hour
		{
			get => _hour;
			set => SetProperty(ref _hour, value);
		}

		public bool SetActiveUnableToReserveText
		{
			get => _setActiveUnableToReserveText;
			set => SetProperty(ref _setActiveUnableToReserveText, value);
		}

		public Vector2 Height
		{
			get => _height;
			set => SetProperty(ref _height, value);
		}
	}
}
