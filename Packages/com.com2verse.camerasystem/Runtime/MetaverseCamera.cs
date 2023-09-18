/*===============================================================
* Product:		Com2Verse
* File Name:	MetaverseCamera.cs
* Developer:	eugene9721
* Date:			2022-06-10 09:53
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Cinemachine;
using Com2Verse.Extension;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.CameraSystem
{
	[RequireComponent(typeof(Camera))]
	public sealed class MetaverseCamera : MonoBehaviour
	{
		private Dictionary<eCullingGroupType, CullingGroupProxy> _cullingGroupProxies = new();

		public Camera? Camera { get; private set; }

		public CinemachineBrain? Brain { get; private set; }

		private float _defaultBlendTime;

#region MonoBehaviour
		private void Awake()
		{
			Camera               = GetComponent<Camera>();
			Brain                = Util.GetOrAddComponent<CinemachineBrain>(gameObject);
			Brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.ManualUpdate;
			_defaultBlendTime    = Brain.m_DefaultBlend.m_Time;
		}

		private void Start()
		{
			var mainVirtualCamera = CameraManager.Instance.MainVirtualCamera;
			if (!mainVirtualCamera.IsUnityNull())
				mainVirtualCamera.gameObject!.SetActive(true);

			gameObject.tag = "MainCamera";
		}

		private void OnDestroy()
		{
			var cameraManager = CameraManager.InstanceOrNull;
			if (cameraManager == null)
				return;

			if (cameraManager.MainCamera != Camera)
				return;

			cameraManager.MainCameraTarget = null;

			var mainVirtualCamera = cameraManager.MainVirtualCamera;
			if (!mainVirtualCamera.IsUnityNull())
				mainVirtualCamera.gameObject.SetActive(false);
		}

		public void OnLateUpdate()
		{
			if (!Brain.IsUnityNull())
				Brain!.ManualUpdate();
		}
#endregion MonoBehaviour

		public CullingGroupProxy GetOrAddCullingGroupProxy(eCullingGroupType cullingGroupType)
		{
			if (_cullingGroupProxies.TryGetValue(cullingGroupType, out var cullingGroupProxy) && !cullingGroupProxy.IsUnityNull())
				return cullingGroupProxy!;

			var newCullingGroupProxy = CameraManager.Instance.SetCullingGroupProxy(gameObject, cullingGroupType);
			_cullingGroupProxies[cullingGroupType] = newCullingGroupProxy;

			return newCullingGroupProxy;
		}

		public void OnBlending(Action? oncomplete)
		{
			if (oncomplete == null) return;
			if (Brain.IsUnityNull()) return;

			BlendingCompleteChecker(oncomplete).Forget();
		}

		private async UniTask BlendingCompleteChecker(Action oncomplete)
		{
			await UniTask.NextFrame();
			await UniTask.WaitUntil(() => !Brain.IsUnityNull() && !Brain!.IsBlending);
			oncomplete.Invoke();
		}

		public void SetDefaultBlendTime()
		{
			if (!Brain.IsUnityNull())
				Brain!.m_DefaultBlend.m_Time = _defaultBlendTime;
		}
	}
}
