/*===============================================================
* Product:		Com2Verse
* File Name:	Billboard.cs
* Developer:	haminjeong
* Date:			2022-07-18 14:56
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.CameraSystem;
using Com2Verse.Extension;
using UnityEngine;

namespace Com2Verse.UI
{
	public sealed class Billboard : MonoBehaviour
	{
		private Camera _camera;
#region Mono
		private void OnEnable()
		{
			_camera = CameraManager.Instance.MainCamera;
			if (_camera.IsReferenceNull()) return;
			Canvas canvas = GetComponent<Canvas>();
			if (canvas.IsReferenceNull()) return;
			canvas.worldCamera = _camera;
		}

		private void LateUpdate()
		{
			if (_camera.IsUnityNull())
				return;

			var cameraRotation = _camera!.transform.rotation;
			transform.LookAt(transform.position + cameraRotation * Vector3.forward, cameraRotation * Vector3.up);
		}
#endregion	// Mono
	}
}
