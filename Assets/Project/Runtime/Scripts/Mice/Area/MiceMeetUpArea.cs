/*===============================================================
* Product:		Com2Verse
* File Name:	MiceMeetUpArea.cs
* Developer:	ikyoung
* Date:			2023-03-31 17:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.Mice
{
    public sealed class MiceMeetUpArea : MiceArea
    {
        public MiceMeetUpArea() { MiceAreaType = eMiceAreaType.MEET_UP; }
        
        public override MiceWebClient.MiceType GetMiceType()
        {
            return MiceWebClient.MiceType.ConferenceSession;
        }
    }
}
