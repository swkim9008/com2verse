/*===============================================================
* Product:		Com2Verse
* File Name:	FixedCameraJig.cs
* Developer:	eugene9721
* Date:			2022-09-16 17:08
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Cinemachine;
using Com2Verse.Extension;
using Com2Verse.Utils;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Com2Verse.CameraSystem
{
	// Cinemachine 카메라 간 Priority 정의 필요
	// 현재 Follow Camera기본 Priority 10
	// Fixed Camera는 0으로 설정 후, 사용시 10 이상의 값으로 설정

	public sealed class FixedCameraJig : MonoBehaviour
	{
		[field: SerializeField] public string JigKey { get; set; } = string.Empty;

		CinemachineVirtualCamera? _virtualCamera;

		private void Awake()
		{
			var go = gameObject;
			_virtualCamera          = Util.GetOrAddComponent<CinemachineVirtualCamera>(go);
			_virtualCamera.Priority = FixedCameraManager.Instance.InactivePriority;
		}

#region Mono
		private void OnEnable()
		{
			FixedCameraManager.Instance.Register(this);
		}

		private void OnDisable()
		{
			FixedCameraManager.InstanceOrNull?.Unregister(this);
		}
#endregion Mono

		public void OnActive()
		{
			if (_virtualCamera.IsUnityNull())
				_virtualCamera = Util.GetOrAddComponent<CinemachineVirtualCamera>(gameObject);
			_virtualCamera!.Priority = FixedCameraManager.Instance.ActivePriority;
		}

		public void OnInactive()
		{
			if (_virtualCamera.IsUnityNull()) return;
			_virtualCamera!.Priority = FixedCameraManager.Instance.InactivePriority;
		}

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			Texture2D background = new Texture2D(1, 1);
			background.SetPixel(1, 1, Color.black);
			background.Apply();

			GUIStyle style = new GUIStyle
			{
				normal =
				{
					textColor  = Color.white,
					background = background
				}
			};

			var sourceCameraPosition = transform.position;
			Handles.Label(sourceCameraPosition, $"FixedCameraJig \n- name: {gameObject.name}", style);
		}
#endif
	}
}
