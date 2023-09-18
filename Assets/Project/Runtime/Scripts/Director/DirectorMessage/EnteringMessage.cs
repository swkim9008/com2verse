/*===============================================================
* Product:		Com2Verse
* File Name:	EnteringMessage.cs
* Developer:	eugene9721
* Date:			2023-02-28 12:21
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Cinemachine;
using Com2Verse.Data;
using Com2Verse.Network;
using UnityEngine;

namespace Com2Verse.Director
{
	public sealed class EnteringMessage : IDirectorMessage
	{
		public ActiveObject     ActiveObject     { get; }
		public Transform        TargetTransform  { get; }
		public CinemachineBrain CinemachineBrain { get; }
		public eAvatarType      AvatarType       { get; }

		public EnteringMessage(ActiveObject activeObject, CinemachineBrain brain, eAvatarType avatarType)
		{
			ActiveObject     = activeObject;
			TargetTransform  = activeObject.transform;
			CinemachineBrain = brain;
			AvatarType       = avatarType;
		}
	}
}
