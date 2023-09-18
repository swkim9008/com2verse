/*===============================================================
* Product:		Com2Verse
* File Name:	ConferenceModeAction.cs
* Developer:	haminjeong
* Date:			2023-05-25 18:19
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Reflection;
using Com2Verse.Data;
using JetBrains.Annotations;

namespace Com2Verse.Project.InputSystem
{
	[UsedImplicitly]
	[InputMode(eModeType.CONFERENCE)]
	public sealed class ConferenceModeAction : BaseModeAction
	{
		public override eModeType CurrentMode => eModeType.CONFERENCE;
		
		public override void Initialize()
		{
			CameraState = eCameraState.FIXED_CAMERA;
			AnimatorId  = 0;
			InputState  = eInputSystemState.SIT_DOWN;
		}
	}
}
