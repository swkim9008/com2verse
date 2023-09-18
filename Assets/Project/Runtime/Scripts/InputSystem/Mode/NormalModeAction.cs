/*===============================================================
* Product:		Com2Verse
* File Name:	NormalModeAction.cs
* Developer:	haminjeong
* Date:			2023-05-25 18:19
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Data;
using JetBrains.Annotations;

namespace Com2Verse.Project.InputSystem
{
	[UsedImplicitly]
	[InputMode(eModeType.NORMAL)]
	public sealed class NormalModeAction : BaseModeAction
	{
		public override eModeType CurrentMode => eModeType.NORMAL;
		
		public override void Initialize()
		{
			CameraState = eCameraState.FOLLOW_CAMERA;
			AnimatorId  = 0;
			InputState  = eInputSystemState.CHARACTER_CONTROL;
		}
	}
}
