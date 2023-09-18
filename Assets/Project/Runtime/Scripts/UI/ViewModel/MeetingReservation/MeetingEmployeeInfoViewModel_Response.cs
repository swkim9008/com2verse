/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingEmployeeInfoViewModel_Response.cs
* Developer:	tlghks1009
* Date:			2022-10-18 13:14
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Protocols.OfficeMeeting;

namespace Com2Verse.UI
{
    public partial class MeetingEmployeeInfoViewModel
    {
        private void OnResponseMeetingOrganizerChange(MeetingOrganizerChangeResponse response)
        {
            Network.Communication.PacketReceiver.Instance.MeetingOrganizerChangeResponse -= OnResponseMeetingOrganizerChange;

            _onOrganizerChangedEvent?.Invoke(this);
        }
    }
}
