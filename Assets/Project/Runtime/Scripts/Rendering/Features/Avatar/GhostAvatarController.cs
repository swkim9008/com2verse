/*===============================================================
* Product:		Com2Verse
* File Name:	GhostAvatarController.cs
* Developer:	ljk
* Date:			2023-01-12 15:49
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.AssetSystem;
using Com2Verse.Avatar;
using Com2Verse.CustomizeLayer;
using Com2Verse.Logger;
using Com2Verse.LruObjectPool;
using Com2Verse.Network;
using Com2Verse.Rendering.Instancing;
using Com2Verse.Rendering.Utility;
using UnityEngine;
using Com2Verse.Extension;

namespace Com2Verse.Rendering
{
	public sealed class GhostAvatarController : MonoBehaviour
	{
		private enum WORKING_STATE
		{
			NOT_INIT,
			COMPONENT_CHECKED,
			ON_INIT,
			NOT_USE,
			UPDATE_BASE_POSITION_ONLY,
			READY
		}

		private static readonly int PropGhostBasePos = Shader.PropertyToID("_GHOST_BASE_POS");
		public static Vector3 GhostAvatarSwapBase => _swapBasePos;
		private static Vector3 _swapBasePos = Vector3.zero;
		public static bool EnableGhostFeature = true;

		public bool IsTestAvatar = false;
		[Header("Test: Ghost Model Type")]
		public string ModelTypeString = "PC01_M";
		[Header("Test: Ghost Transition x:Near - y:Far")]
		public Vector2 GhostTransitionDistance = new Vector2(8,10);
		
		
		private GPUInstancingAnimatorHumanoid _instancedHumanoid;
		private GameObject _ghostGameObject;
		private WORKING_STATE _workingState = WORKING_STATE.NOT_INIT;
		private Renderer[] _originalRenderers;
		private int _spacialColorArrayIndex = -1;
		private bool _rendererReservedDisable = false;
		private bool _textureNeedCapture = false;
		private bool _isUser;
		private Animator _animator;
		private AvatarController _avatarController;
		private AvatarCustomizeLayer _customizeLayer;
		private BoneResolver _boneResolver;
		private float updateStack = 0;

		private bool _rendererUpdateLoop = false;

		private void Update()
		{
			if(!_rendererUpdateLoop)
				return;
			OnGhostFeatureUpdate();
		}

		private void OnEnable()
		{
			_rendererUpdateLoop = true;
		}

		private void OnDestroy()
		{
			if (!IsTestAvatar && _avatarController != null)
			{
				_avatarController.OnCompletedLoadAvatarParts -= UpdateRenderers;
				_avatarController.OnClearAvatarParts -= ClearRenderer;
				_avatarController.OnGameObjectActive -= OnGameObjectActive;
			}
			ClearRenderer();
		}

		private void OnGameObjectActive(bool active)
		{
			if (active)
				_rendererUpdateLoop = true;
			else
				ReleaseGhost(true);
		}
		
		private void ReleaseGhost(bool isDestroy = false)
		{
			if (_ghostGameObject != null)
			{
				Destroy(_ghostGameObject);
				//RuntimeObjectManager.Instance.Remove(_ghostGameObject);
				
				EnableRenderers(true);
				_originalRenderers = null;
				
				if(_boneResolver != null)
					Destroy(_boneResolver);
				if (_spacialColorArrayIndex >= 0)
				{
					GPUInstancingManager.Instance.GetSpacialColorHandler().Return(_spacialColorArrayIndex);
				}
				_spacialColorArrayIndex = -1;
				if (isDestroy && !IsTestAvatar)
				{
					_avatarController.OnCompletedLoadAvatarParts -= UpdateRenderers;
					_avatarController.OnClearAvatarParts -= ClearRenderer;
				}
			}
			if(_animator != null)
				_animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			_workingState = WORKING_STATE.NOT_INIT;
		}

		private void OnGhostModelLoaded(C2VAsyncOperationHandle<GameObject> operation)
		{
			var ghostPrefab = operation.Result;
			
			ReleaseGhost();
			
			_ghostGameObject = Instantiate(ghostPrefab, transform, false);

			Vector3 exSize = Vector3.one;
			exSize.x = 1 / transform.localScale.x;
			exSize.y = 1 / transform.localScale.y;
			exSize.z = 1 / transform.localScale.z;
			
			// 본 추출 전 unitsize 정리 
			_ghostGameObject.transform.localScale = exSize;
			_boneResolver = gameObject.AddComponent<BoneResolver>();
			_instancedHumanoid = _ghostGameObject.GetComponentInChildren<GPUInstancingAnimatorHumanoid>();
			_instancedHumanoid.transform.localScale = Vector3.one;
			_instancedHumanoid.ForceUpdate(_boneResolver);
			// 본 추출 후 원본사이즈로 복구
			_ghostGameObject.transform.localScale = Vector3.one;
			_instancedHumanoid.enabled = false;
			_textureNeedCapture = true;
			_rendererReservedDisable = false;
			_workingState = WORKING_STATE.READY;
		}
		// private void OnGhostModelLoaded(GameObject loadAsset)
		// {
		// 	//_ghostGameObject = RuntimeObjectManager.Instance.Instantiate<GameObject>(loadAsset);
		// 	//_ghostGameObject.transform.SetParent(transform, false);
		// 	
		// 	_ghostGameObject = Instantiate(loadAsset, transform, false);
		// 	_boneResolver = gameObject.GetOrAddComponent<BoneResolver>();
		// 	
		// 	_instancedHumanoid = _ghostGameObject.GetComponentInChildren<GPUInstancingAnimatorHumanoid>();
		// 	_instancedHumanoid.ForceUpdate(_boneResolver);
		// 	
		// 	_instancedHumanoid.enabled = false;
		// 	_textureNeedCapture = true;
		// 	_rendererReservedDisable = false;
		// 	_workingState = WORKING_STATE.READY;
		// }

		private void ClearRenderer()
		{
			ReleaseGhost(true);

			_rendererUpdateLoop = false;
		}

		private void UpdateRenderers()
		{
			if (_workingState == WORKING_STATE.ON_INIT)
				return;
			_rendererUpdateLoop = true;
			ReleaseGhost(true);

			if (!IsTestAvatar)
			{
				MapObject mo = transform.parent.gameObject.GetComponent<MapObject>();
				if (mo == null)
				{
					ReleaseGhost(true);
					return;
				}

				if (mo != null)
				{
					if(!mo.IsInitialized) // Map Object Set 대기
						return;
					
					if (mo.IsMine)
					{
						ReleaseGhost(true);
						_workingState = WORKING_STATE.UPDATE_BASE_POSITION_ONLY;
						return;
					}
				}
			}

			_workingState = WORKING_STATE.ON_INIT;
			_originalRenderers = GetComponentsInChildren<Renderer>();

			string targetModelPath = "";

			if (IsTestAvatar)
				targetModelPath = $"{ModelTypeString}_Ghost_animated.prefab";
			else
				targetModelPath = $"{_avatarController.Info.AvatarType.ToString()}_Ghost_animated.prefab";

			C2VAddressables.LoadAssetAsync<GameObject>(targetModelPath).OnCompleted += OnGhostModelLoaded;
			//RuntimeObjectManager.Instance.LoadAssetAsync<GameObject>(targetModelPath, OnGhostModelLoaded);
		}

		private void CheckNeedUpdateTexture()
		{
			if (!GPUInstancingManager.Instance.GetSpacialColorHandler().CanUpdateTexture)
			{
				if(_spacialColorArrayIndex != -1)
					GPUInstancingManager.Instance.GetSpacialColorHandler().Return(_spacialColorArrayIndex);
				_spacialColorArrayIndex = -1;
				_textureNeedCapture = true;
				return;
			}

			if (_textureNeedCapture)
			{
				if(_originalRenderers == null)
					return;
				_spacialColorArrayIndex = GPUInstancingManager.Instance.GetSpacialColorHandler().GetReservedSpace(this.GetInstanceID());
				_instancedHumanoid.AddVariationPerTarget(1,_spacialColorArrayIndex,"_TextureIndex");
				GPUInstancingManager.Instance.GetSpacialColorHandler().CaptureModel(_originalRenderers,_spacialColorArrayIndex,UpdateVariant);
				_textureNeedCapture = false;
			}
		}

		private void UpdateVariant()
		{
			_instancedHumanoid.RefreshVariantPerTarget(1);
		}

		/// <summary>
		/// Ghost Humanoid 상태 업데이트
		/// </summary>
		/// <returns> 원본 아바타 visibility </returns>
        private bool RefreshGhostHumanoid()
        {
	        bool enable = _instancedHumanoid.IsVisible && !_instancedHumanoid.IsCloseInvisible;

	        if (_instancedHumanoid.IsCloseInvisible)
	        {
		        _instancedHumanoid.OnUpdate();
		        return true;
	        }
		       
            bool isUpdateFrame = _instancedHumanoid.IsUpdateFrame;
            AnimatorCullingMode cullingMode = enable && _instancedHumanoid.IsVisible && isUpdateFrame
                ? AnimatorCullingMode.AlwaysAnimate
                : AnimatorCullingMode.CullCompletely;
            if(_animator.cullingMode != cullingMode)
                _animator.cullingMode =cullingMode;
            updateStack += Time.deltaTime;
            if (isUpdateFrame)
            {
                _animator.Update(updateStack);
                _instancedHumanoid.OnUpdate();
                updateStack = 0;
            }
            else
				_instancedHumanoid.SuperOnUpdate();
            
            return false;
        }

		private void EnableRenderers(bool enable)
		{
			try
			{
				if (_originalRenderers != null)
				{
					for (int i = 0; i < _originalRenderers.Length;i++)
					{
						if(_originalRenderers[i] != null)
							_originalRenderers[i].enabled = enable;
					}
				}
			}
			catch
			{
				// renderer unset
		//		_originalRenderers = null;
			}
			if(_customizeLayer != null)
				_customizeLayer.enabled = enable;
		}

		private bool CheckStatus()
		{
			if (_workingState == WORKING_STATE.NOT_INIT)
			{
				if (_animator == null)
					_animator = GetComponent<Animator>();

				if (_customizeLayer == null)
					_customizeLayer = GetComponent<AvatarCustomizeLayer>();
				
				if (!IsTestAvatar)
				{
					if (_avatarController == null)
						_avatarController = GetComponent<AvatarController>();

					if (_animator == null || _avatarController == null)
						return false;

					if (!_avatarController.IsCompletedLoadAvatarParts)
						return false;
				}
				else
				{
					if (_animator == null)
						return false;
				}
				
				_workingState = WORKING_STATE.COMPONENT_CHECKED;
				
				if (!IsTestAvatar)
				{
					_avatarController.OnCompletedLoadAvatarParts -= UpdateRenderers;
					_avatarController.OnCompletedLoadAvatarParts += UpdateRenderers;
					_avatarController.OnClearAvatarParts -= ClearRenderer;
					_avatarController.OnClearAvatarParts += ClearRenderer;
					
					_avatarController.OnGameObjectActive -= OnGameObjectActive;
					_avatarController.OnGameObjectActive += OnGameObjectActive;
				}
				UpdateRenderers();
			}

			if (_workingState == WORKING_STATE.UPDATE_BASE_POSITION_ONLY)
			{
				if (GPUInstancingManager.GhostCheckType ==
				    GPUInstancingManager.eGhostDistanceCheckType.PLAYER_CHARACTER)
				{
					var pos = transform.position;
					Shader.SetGlobalVector(PropGhostBasePos, pos);
					GPUInstancingManager.Instance.SetDistanceCullingOrigin(transform);
					_swapBasePos = pos;
				}
				else if (GPUInstancingManager.GhostCheckType == GPUInstancingManager.eGhostDistanceCheckType.TARGET)
				{
					if(GPUInstancingManager.Instance.GhostBaseTarget == null)
						GPUInstancingManager.Instance.SetGhostBase();
					else
					{
						var pos = GPUInstancingManager.Instance.GhostBaseTarget.position;
						Shader.SetGlobalVector(PropGhostBasePos, pos);
						_swapBasePos = pos;
					}
				}
			}

			return _workingState == WORKING_STATE.READY;
		}

		private void UpdateVisibility()
		{
			if (_rendererReservedDisable)
			{
				EnableRenderers(false);
				_rendererReservedDisable = false;
			}

	//		float camDistance = (transform.position - GhostAvatarSwapBase).magnitude;

	//		float ghostTransitionClose = 
	//			IsTestAvatar ? GhostTransitionDistance.x : GPUInstancingManager.GhostAvatarStartDistance;
	//		float ghostTransitionFar =
	//			IsTestAvatar ? GhostTransitionDistance.y : GPUInstancingManager.GhostAvatarCompleteDistance;
			
			//bool ghostEnable = ghostTransitionClose < camDistance && ghostTransitionClose > 0;
			//bool rendererEnable = ghostTransitionFar > camDistance || ghostTransitionFar == 0;
			
			bool rendererNeedEnable = RefreshGhostHumanoid();

			if (!rendererNeedEnable)
				_rendererReservedDisable = true;
			else
				EnableRenderers(rendererNeedEnable);
			
			
			CheckNeedUpdateTexture();
		}

		public void OnGhostFeatureUpdate()
		{
			if (!EnableGhostFeature)
			{
				if (_workingState == WORKING_STATE.NOT_USE)
					return;
				
				ClearRenderer();
				_workingState = WORKING_STATE.NOT_USE;
				_rendererUpdateLoop = true;
				return;
			}else
			{
				if (_workingState == WORKING_STATE.NOT_USE)
					_workingState = WORKING_STATE.NOT_INIT;
			}
			
			if(CheckStatus())
				UpdateVisibility();
		}
	}
}
