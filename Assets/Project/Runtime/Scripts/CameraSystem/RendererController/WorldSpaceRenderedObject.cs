/*===============================================================
 * Product:		Com2Verse
 * File Name:	WorldSpaceRenderedObject.cs
 * Developer:	urun4m0r1
 * Date:		2023-05-12 14:35
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Utils;
using UnityEngine;

namespace Com2Verse.Project.CameraSystem
{
	/// <summary>
	/// UI 전용 카메라가 비활성화 된 경우만 활성화되는 오브젝트 (ex. 아바타 HUD)
	/// </summary>
	[AddComponentMenu("[CameraSystem]/[CameraSystem] World Space Rendered Object")]
	public class WorldSpaceRenderedObject : MonoBehaviour
	{
		private bool _wasActive;
		private bool _isRendererChangingActiveState;

		private void Awake()     => RegisterEvents();
		private void OnDestroy() => UnregisterEvents();

		private void RegisterEvents()
		{
			CameraMediator.Instance.CurrentRendererChanged += OnCurrentRendererChanged;
			OnCurrentRendererChanged(CameraMediator.Instance.CurrentRenderer);
		}

		private void UnregisterEvents()
		{
			var cameraMediator = CameraMediator.InstanceOrNull;
			if (cameraMediator != null)
			{
				cameraMediator.CurrentRendererChanged -= OnCurrentRendererChanged;
			}
		}

		private void OnEnable()
		{
			if (_isRendererChangingActiveState)
				return;

			_wasActive = gameObject.activeSelf;
		}

		private void OnDisable()
		{
			if (_isRendererChangingActiveState)
				return;

			_wasActive = gameObject.activeSelf;
		}

		private void OnCurrentRendererChanged(Define.eRenderer currentRenderer)
		{
			_isRendererChangingActiveState = true;

			if (currentRenderer == Define.eRenderer.UI)
			{
				gameObject.SetActive(false);
			}
			else
			{
				gameObject.SetActive(_wasActive);
			}

			_isRendererChangingActiveState = false;
		}
	}
}
