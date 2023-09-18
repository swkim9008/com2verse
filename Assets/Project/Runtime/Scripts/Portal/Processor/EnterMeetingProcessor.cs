/*===============================================================
* Product:		Com2Verse
* File Name:	EnterMeetingProcessor.cs
* Developer:	tlghks1009
* Date:			2022-09-28 13:12
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/


using Com2Verse.Data;
using Com2Verse.EventTrigger;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using Protocols.GameLogic;

namespace Com2Verse.EventTrigger
{
    [LogicType(eLogicType.ENTER__MEETING)]
    public sealed class EnterMeetingProcessor : BaseLogicTypeProcessor
    {
        public override void OnInteraction(TriggerInEventParameter triggerInParameter)
        {
            base.OnInteraction(triggerInParameter);

            ShowMeetingRoomEnterPopup();
        }


        private void ShowMeetingRoomEnterPopup()
        {
            UIManager.Instance.CreatePopup("UI_ConnectingApp", (reservationPopup) => { reservationPopup.Show(); }).Forget();
        }
    }
}
