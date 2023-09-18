/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingReservationViewModel_Response.cs
* Developer:	tlghks1009
* Date:			2022-09-02 16:30
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Logger;
using Com2Verse.Organization;
using Cysharp.Text;
using Protocols;
using Protocols.OfficeMeeting;
using ResponseReservation = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.ReservationResponseResponseFormat>;

namespace Com2Verse.UI
{
    public partial class MeetingReservationViewModel
    {
        private void OnResponseMeetingReservation(ResponseReservation response)
        {
            ResetCloseReservationPopup();
            SetActive = false;
            _reservationCallback?.Invoke(response.Value.Data.Meeting);
        }

        private void OnResponseMeetingReservationStatus(MeetingReservationStatusResponse response)
        {
            Network.Communication.PacketReceiver.Instance.MeetingReservationStatusResponse -= OnResponseMeetingReservationStatus;
        }

        private void OnResponseMeetingOrganizerChange(MeetingOrganizerChangeResponse response)
        {
            ResetCloseReservationPopup();
            SetActive = false;
        }

        private void OnResponseEmployeeDeleted(MeetingUserDeleteResponse response)
        {
            // TODO : NEW_ORGANIZATION WEB API로 대체 (임시)
            // var deletedEmployeeTagViewModel = GetMeetingTagElement(response.DeleteEmployeeNo);
            if (!long.TryParse(response.DeleteEmployeeNo, out var accountId))
            {
                C2VDebug.LogWarning("EmployeeNo 대신 AccountId 사용");
                return;
            }
            var deletedEmployeeTagViewModel = GetMeetingTagElement(accountId);
            if (deletedEmployeeTagViewModel == null)
            {
                return;
            }

            RemoveMeetingTagElement(deletedEmployeeTagViewModel);
        }
    }
}
