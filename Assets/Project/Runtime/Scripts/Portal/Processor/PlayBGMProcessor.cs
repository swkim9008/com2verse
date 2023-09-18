/*===============================================================
* Product:		Com2Verse
* File Name:	PlayBGMProcessor.cs
* Developer:	haminjeong
* Date:			2023-06-26 16:02
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using Com2Verse.SoundSystem;

namespace Com2Verse.EventTrigger
{
	[LogicType(eLogicType.PLAY_BGM)]
	public sealed class PlayBGMProcessor : BaseLogicTypeProcessor
	{
		private eSoundIndex _prevBGM;
		
		public override void OnZoneEnter(ServerZone zone, int callbackIndex)
		{
			var    callback    = zone.Callback[callbackIndex];
			string bgmIDString = null;
			if (callback is { InteractionValue: { Count: > 0 } })
				bgmIDString = callback.InteractionValue[0];
			if (!string.IsNullOrEmpty(bgmIDString) && int.TryParse(bgmIDString, out var bgmID))
			{
				_prevBGM = SoundManager.CurrentBGM;
				SoundManager.PlayBGM((eSoundIndex)bgmID, 2, 0);
			}
		}

		public override void OnZoneExit(ServerZone zone, int callbackIndex)
		{
			SoundManager.PlayBGM(_prevBGM, 2, 0);
		}
	}
}
