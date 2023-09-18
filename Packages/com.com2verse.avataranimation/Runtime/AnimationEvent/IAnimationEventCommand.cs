/*===============================================================
* Product:		Com2Verse
* File Name:	IAnimationEventCommand.cs
* Developer:	eugene9721
* Date:			2023-02-10 15:45
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Com2Verse.AvatarAnimation
{
	public interface IAnimationEventCommand
	{
		public void OnAnimationEvent(eAnimationEvent eventType, string stringParam, int intParam, bool isInvoked);
	}
}
