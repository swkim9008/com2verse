/*===============================================================
* Product:		Com2Verse
* File Name:	AvatarTurntable.cs
* Developer:	minujeong
* Date:			2023-03-17 16:11
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.Project.Runtime.Scripts.Avatar.Etc
{
	public sealed class AvatarTurntable : MonoBehaviour
	{
		[SerializeField] private Vector3 _rotationSpeed;

		private void Update()
		{
			transform.Rotate(_rotationSpeed * Time.deltaTime, Space.Self);
		}
	}
}
