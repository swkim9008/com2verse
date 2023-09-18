/*===============================================================
* Product:		Com2Verse
* File Name:	OverrideMetaverseCamera.cs
* Developer:	eugene9721
* Date:			2023-08-08 17:55
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using Com2Verse.Extension;

namespace Com2Verse.CameraSystem
{
	[RequireComponent(typeof(Camera))]
	public sealed class OverrideMetaverseCamera : MonoBehaviour
	{
		private void OnEnable()
		{
			SetMetaverseCameraActive(false);
		}

		private void OnDisable()
		{
			SetMetaverseCameraActive(true);
		}

		private void SetMetaverseCameraActive(bool value)
		{
			var metaverseCamera = CameraManager.InstanceOrNull?.MetaverseCamera;
			if (!metaverseCamera.IsUnityNull())
				metaverseCamera!.gameObject.SetActive(value);
		}
	}
}
