/*===============================================================
* Product:		Com2Verse
* File Name:	MiceSessionIdleState.cs
* Developer:	ikyoung
* Date:			2023-07-11 13:56
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Com2Verse.Mice
{
	public sealed class MiceSessionIdleState : MiceServiceState
	{
		public MiceSessionIdleState()
		{
			ServiceStateType = eMiceServiceState.SESSION_IDLE;
		}
	}
}
