/*===============================================================
* Product:		Com2Verse
* File Name:	CameraMediator.cs
* Developer:	eugene9721
* Date:			2023-01-04 16:52
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.CameraSystem;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.InputSystem;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.PlayerControl;
using Com2Verse.Project.InputSystem;
using Com2Verse.UI;
using Com2Verse.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using FollowCamera = Com2Verse.CameraSystem.FollowCamera;

namespace Com2Verse.Project.CameraSystem
{
	public sealed class CameraMediator : Singleton<CameraMediator>
	{
		public event Action<Define.eRenderer>? CurrentRendererChanged;

		private Define.eRenderer _currentRenderer = Define.eRenderer.DEFAULT;

		public Define.eRenderer CurrentRenderer
		{
			get => _currentRenderer;
			private set
			{
				_currentRenderer = value;
				CurrentRendererChanged?.Invoke(value);
			}
		}

		private readonly ObservableHashSet<WorldSpaceBlockingObject> _worldSpaceBlockingObjects = new();

		[UsedImplicitly] private CameraMediator()
		{
			_worldSpaceBlockingObjects.ItemExistenceChanged += OnWorldSpaceBlockingObjectsExistenceChanged;
		}

		public static void Initialize()
		{
			var cameraManager = CameraManager.Instance;
			cameraManager.OnCameraStateChange += OnCameraStateChange;
			cameraManager.OnUpdateCameraView  += OnUpdateCameraView;

			var cameraCollisionFilter = Define.LayerMask(Define.eLayer.GROUND) | Define.LayerMask(Define.eLayer.WALL);
			cameraManager.CameraCollisionFilter = cameraCollisionFilter;
		}

		public static CameraFrame GetCameraFrame(RenderTextureFormat format, int defaultWidth = 0, int defaultHeight = 0, float renderScale = 1f, GameObject? cameraPrefab = null)
		{
			var cameraFrame  = CameraFrame.Create(format, defaultWidth, defaultHeight, renderScale, cameraPrefab);
			var sourceCamera = cameraFrame.SourceCamera;
			sourceCamera.GetUniversalAdditionalCameraData()?.SetRenderer((int)Define.eRenderer.FRAME);
			CameraManager.OffCullingMaskLayer(sourceCamera, (int)Define.eLayer.UI);
			return cameraFrame;
		}

#region RendererController
		/// <summary>
		/// 3D 렌더링 화면을 블로킹하는 오브젝트 (ex. 전체화면 UI) 가 단 1개라도 있으면 UI 전용 카메라를 사용한다.
		/// </summary>
		private void OnWorldSpaceBlockingObjectsExistenceChanged(bool isAnyItemExists)
		{
			SetUICamera(isAnyItemExists);
		}

		private void SetUICamera(bool isUiCamera)
		{
			var cameraManager = CameraManager.Instance;
			if (isUiCamera)
			{
				cameraManager.SetRenderer((int)Define.eRenderer.UI);
				cameraManager.SetCullingMask((int)Define.eLayer.UI);
				CurrentRenderer = Define.eRenderer.UI;
			}
			else
			{
				cameraManager.SetRenderer((int)Define.eRenderer.DEFAULT);
				cameraManager.SetPrevCullingMask();
				CurrentRenderer = Define.eRenderer.DEFAULT;
			}
		}

		public void TryAddWorldSpaceBlockingObject(WorldSpaceBlockingObject worldSpaceBlockingObject)
		{
			_worldSpaceBlockingObjects.TryAdd(worldSpaceBlockingObject);
		}

		public void RemoveWorldSpaceBlockingObject(WorldSpaceBlockingObject worldSpaceBlockingObject)
		{
			_worldSpaceBlockingObjects.Remove(worldSpaceBlockingObject);
		}
#endregion // RendererController

#region CameraManagerEvent
		private static FollowCamera? _currentFollowCamera;
		private static void OnCameraStateChange(CameraBase? prevCamera, CameraBase nextCamera)
		{
			if (prevCamera is FollowCamera prevFollowTmpCamera)
			{
				RemoveFollowCameraActions(prevFollowTmpCamera);
				var playerController = PlayerController.InstanceOrNull;
				if (!playerController.IsUnityNull())
				{
					playerController!.OnChangeWaypoints  -= prevFollowTmpCamera.UpdateBezierPoint;
					playerController.OnProgressWaypoints -= prevFollowTmpCamera.UpdateMoveProgress;
				}
			}

			if (nextCamera is FollowCamera nextFollowTmpCamera)
			{
				AddFollowCameraActions(nextFollowTmpCamera);
				PlayerController.Instance.OnChangeWaypoints   += nextFollowTmpCamera.UpdateBezierPoint;
				PlayerController.Instance.OnProgressWaypoints += nextFollowTmpCamera.UpdateMoveProgress;
			}
		}

		private static void OnUpdateCameraView(int currentViewIndex)
		{
			if (CurrentScene.ViewportType == eSpaceOptionViewport.NONE) return;
			Commander.Instance.UpdateCameraView(currentViewIndex);
		}

		private static void AddFollowCameraActions(FollowCamera? followCamera)
		{
			var actionMapCharacterControl = InputSystemManager.Instance.GetActionMap<ActionMapCharacterControl>();
			if (actionMapCharacterControl == null)
			{
				C2VDebug.LogErrorCategory(nameof(CameraMediator), "ActionMap Not Found");
				return;
			}
			
			_currentFollowCamera = followCamera;

			if (followCamera == null) return;
			
			actionMapCharacterControl.ClickMoveStartAction += followCamera.OnClickMoveStart;
			actionMapCharacterControl.ClickMoveEndAction   += followCamera.OnClickMoveEnd;
			actionMapCharacterControl.LookStartAction      += followCamera.OnLookStart;
			actionMapCharacterControl.LookEndAction        += followCamera.OnLookEnd;
			actionMapCharacterControl.ZoomAction           += followCamera.OnZoom;
			actionMapCharacterControl.MoveAction           += OnMove;
			actionMapCharacterControl.LookAction           += followCamera.OnLook;
		}

		private static void RemoveFollowCameraActions(FollowCamera followCamera)
		{
			var actionMapCharacterControl = InputSystemManager.Instance.GetActionMap<ActionMapCharacterControl>();
			if (actionMapCharacterControl == null) return;

			_currentFollowCamera = null;
			
			actionMapCharacterControl.ClickMoveStartAction -= followCamera.OnClickMoveStart;
			actionMapCharacterControl.ClickMoveEndAction   -= followCamera.OnClickMoveEnd;
			actionMapCharacterControl.LookStartAction      -= followCamera.OnLookStart;
			actionMapCharacterControl.LookEndAction        -= followCamera.OnLookEnd;
			actionMapCharacterControl.ZoomAction           -= followCamera.OnZoom;
			actionMapCharacterControl.MoveAction           -= OnMove;
			actionMapCharacterControl.LookAction           -= followCamera.OnLook;
		}

		private static void OnMove(Vector2 value)
		{
			if (!User.Instance.Standby || InputFieldExtensions.IsExistFocused)
				value = Vector2.zero;
			_currentFollowCamera?.OnMove(value);
		}
#endregion CameraManagerEvent
	}
}
