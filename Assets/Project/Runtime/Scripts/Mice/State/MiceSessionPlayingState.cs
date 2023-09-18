/*===============================================================
* Product:		Com2Verse
* File Name:	MiceConferenceMainState.cs
* Developer:	ikyoung
* Date:			2023-07-11 13:59
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Com2Verse.Mice
{
	public sealed class MiceSessionPlayingState : MiceServiceState
	{
		public MiceSessionPlayingState()
		{
			ServiceStateType = eMiceServiceState.SESSION_PLAYING;
		}
		
		public override bool CanPlayingLectureVideo()
		{
			return true;
		}
	}
}
