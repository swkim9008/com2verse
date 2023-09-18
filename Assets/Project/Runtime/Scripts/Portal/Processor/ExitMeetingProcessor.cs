/*===============================================================
* Product:		Com2Verse
* File Name:	ExitMeetingProcessor.cs
* Developer:	tlghks1009
* Date:			2022-09-28 13:12
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using Com2Verse.EventTrigger;
using Com2Verse.InputSystem;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Com2Verse.PlayerControl;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using Protocols.OfficeMeeting;
using Protocols.GameLogic;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.EXIT__MEETING)]
	public sealed class ExitMeetingProcessor : BaseLogicTypeProcessor
	{
		public override void OnInteraction(TriggerInEventParameter triggerInParameter)
		{
			base.OnInteraction(triggerInParameter);

			RequestLeaveChannel();
		}

		private void RequestLeaveChannel()
		{
			// 이동중 OSR/Input 막기
			PlayerController.Instance.SetStopAndCannotMove(true);
			User.Instance.DiscardPacketBeforeStandBy();
			Commander.Instance.RequestRoomLeaveAsync(MeetingReservationProvider.EnteredMeetingInfo.MeetingId, MeetingReservationProvider.RoomId).Forget();
		}
	}
}
