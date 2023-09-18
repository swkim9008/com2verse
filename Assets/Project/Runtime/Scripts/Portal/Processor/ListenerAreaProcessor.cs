/*===============================================================
* Product:		Com2Verse
* File Name:	ListenerAreaProcessor.cs
* Developer:	haminjeong
* Date:			2023-06-13 10:47
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Communication;
using Com2Verse.Data;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.AUDIENCE)]
	public sealed class ListenerAreaProcessor : BaseLogicTypeProcessor
	{
		public override void OnZoneEnter(ServerZone zone, int callbackIndex)
		{
			AuditoriumController.Instance.CurrentAudienceZone = zone;
			
			var    callback    = zone.Callback[callbackIndex];
			string spaceObjectIDString = null;
			if (callback is { InteractionValue: { Count: > 0 } })
				spaceObjectIDString = callback.InteractionValue[0];
			if (!string.IsNullOrEmpty(spaceObjectIDString))
			{
				// TODO: 마이크와 존의 관계성이 필요할 때 다시 체크
				// string[] splitIDs = spaceObjectIDString.Split(',');
				// for (int i = 0; i < splitIDs!.Length; ++i)
				// {
				// 	string idString = splitIDs[i];
				// 	if (long.TryParse(idString, out var spaceID))
				// 		AuditoriumController.Instance.AddMicID(spaceID);
				// }
			}
		}

		public override void OnZoneExit(ServerZone zone, int callbackIndex)
		{
			AuditoriumController.Instance.CurrentAudienceZone = null;
			AuditoriumController.Instance.LeaveChannel(false);
		}
	}
}
