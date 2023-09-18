/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingReservationHelper.cs
* Developer:	tlghks1009
* Date:			2022-10-07 18:23
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Network;
using Com2Verse.UI;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;
using MeetingStatus = Com2Verse.WebApi.Service.Components.MeetingStatus;

namespace Com2Verse.MeetingReservation
{
	public sealed class MeetingReservationHelper
	{
		public static bool CanChangeOption(MeetingInfoType meetingInfo, bool wantOpenPopup = true)
		{
			if (meetingInfo.MeetingStatus is MeetingStatus.MeetingExpired or MeetingStatus.MeetingPassed)
			{
				if (wantOpenPopup)
					UIManager.Instance.ShowPopupCommon(Localization.Instance.GetString("UI_MeetingApp_Desc_AlreadyEnd"), null);
				return false;
			}

			if (meetingInfo.CancelYn == "Y")
			{
				if (wantOpenPopup)
					UIManager.Instance.ShowPopupCommon(Localization.Instance.GetString("UI_MeetingApp_Desc_Deleted"));
				return false;
			}

			if (MetaverseWatch.NowDateTime > meetingInfo.StartDateTime.AddMinutes(-MeetingReservationProvider.AdmissionTime))
			{
				if(wantOpenPopup)
					UIManager.Instance.ShowPopupCommon(Localization.Instance.GetString("UI_MeetingApp_Popup_AlreadyStart"));
				return false;
			}

			return true;
		}
	}
}
