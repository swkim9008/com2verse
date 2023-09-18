/*===============================================================
* Product:		Com2Verse
* File Name:	LeaveWorkMessage.cs
* Developer:	eugene9721
* Date:			2022-12-07 17:36
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Avatar;

namespace Com2Verse.Director
{
	public sealed class LeaveWorkMessage : IDirectorMessage
	{
		public AvatarInfo AvatarInfo { get; }

		public LeaveWorkMessage(AvatarInfo avatarInfo)
		{
			AvatarInfo = avatarInfo;
		}
	}
}
