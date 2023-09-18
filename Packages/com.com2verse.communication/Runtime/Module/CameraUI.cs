/*===============================================================
 * Product:		Com2Verse
 * File Name:	CameraUI.cs
 * Developer:	urun4m0r1
 * Date:		2023-05-12 14:35
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using UnityEngine;

namespace Com2Verse.Communication
{
	[AddComponentMenu("[Communication]/[Communication] Camera UI")]
	public class CameraUI : MonoBehaviour
	{
		protected virtual void OnEnable()  => RegisterCameraUI();
		protected virtual void OnDisable() => UnregisterCameraUI();

		private void RegisterCameraUI()
		{
			DeviceModuleManager.Instance.TryAddCameraUI(this);
		}

		private void UnregisterCameraUI()
		{
			DeviceModuleManager.InstanceOrNull?.RemoveCameraUI(this);
		}
	}
}
