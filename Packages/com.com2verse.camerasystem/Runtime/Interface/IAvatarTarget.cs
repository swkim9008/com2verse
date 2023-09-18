/*===============================================================
* Product:		Com2Verse
* File Name:	IAvatarTarget.cs
* Developer:	eugene9721
* Date:			2023-01-06 12:14
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.CameraSystem
{
	public interface IAvatarTarget : IVariableHeightTarget
	{
		Transform AvatarTarget { get; set; }
	}
}
