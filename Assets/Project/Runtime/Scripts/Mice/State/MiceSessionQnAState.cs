/*===============================================================
* Product:		Com2Verse
* File Name:	MiceConferenceQnAState.cs
* Developer:	ikyoung
* Date:			2023-07-11 14:12
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Com2Verse.Mice
{
	public sealed class MiceSessionQnAState : MiceServiceState
	{
		public MiceSessionQnAState()
		{
			ServiceStateType = eMiceServiceState.SESSION_QnA;
		}
	}
}
