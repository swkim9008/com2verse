/*===============================================================
* Product:		Com2Verse
* File Name:	ActionMapWaitingBlockControl.cs
* Developer:	mikeyid77
* Date:			2023-07-27 11:06
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.InputSystem;

namespace Com2Verse
{
	public sealed class ActionMapWaitingBlockControl : ActionMap
	{
		public ActionMapWaitingBlockControl()
		{
			_name = "WaitingBlock";
		}

		public override void ClearActions() { }
	}
}
