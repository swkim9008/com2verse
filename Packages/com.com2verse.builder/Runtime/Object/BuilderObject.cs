// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	BuilderObject.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-08 오전 10:34
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System.Threading;
using Com2Verse.Extension;
using Com2Verse.Rendering.Utility;
using UnityEngine;

namespace Com2Verse.Builder
{
	public class BuilderObject : DraggableObject, IObjectRaycastTarget
	{
		public static long IndexCounter = 1;

		public long SerializationId
		{
			get
			{
				if (_serialId == -1)
				{
					_serialId = Interlocked.Add(ref IndexCounter, 1);
				}

				return _serialId;
			}
			set => _serialId = value;
		}

		private long _serialId = -1; // Space Object Id
		private bool IsSelected => ReferenceEquals(_selectedObject, gameObject);
		private static GameObject _selectedObject;
		private static BuilderObject _lastClickTarget = null;
		
		private Transform _initialParent = null;
		private bool _releasedInThisFrame = false;

		private Vector3 _initialPosition;
		private Quaternion _initialRotation;

		public void CreateRaycastBound()
		{
			// Create raycast box when the model does not have any collider.
			if (GetComponentInChildren<Collider>().IsReferenceNull())
			{
				var boxCollider = gameObject.AddComponent<BoxCollider>();
				boxCollider.center = ModelBounds.center;
				boxCollider.size = ModelBounds.size;
			}
		}

		public void Init(GameObject targetModel, BuilderInventoryItem data)
		{
			TargetModel = targetModel;
			Item = data;
			CreateRaycastBound();
		}

		private bool HandleMouseInput()
		{
			if (!_releasedInThisFrame)
			{
				if (Input.GetMouseButtonUp(0))
				{
					ReleaseObject();
					return true;
				}

				if (Input.GetMouseButtonUp(1))
				{
					CancelMove();
					return true;
				}
			}

			_releasedInThisFrame = false;
			return false;
		}

		private void Update()
		{
			if (!IsSelected) return;

			if (HandleMouseInput()) return;
			ProcessRotateObject();
			
			if (Input.GetKeyDown(KeyCode.Delete))
			{
				CancelMove();
				new RemoveObjectAction(TargetModel, TargetModel.transform.parent, SpaceManager.Instance.TrashCan.transform).Do(true);
				return;
			}

			DoRaycastMoveCheck();
		}

		public static void ClearSelectedObject()
		{
			if (!_selectedObject.IsUnityNull())
			{
				_selectedObject.FlagRenderingLayerMask(false);
			}
			_selectedObject = null;
			CameraMouseController.Instance.InvokeClickEvent = true;
		}

		public void OnClick(bool click)
		{
			_lastClickTarget = this;
		}

		public void OnRelease(bool click)
		{
			bool isSelected = IsSelected; 
			if (click)
			{
				if (!ReferenceEquals(_lastClickTarget, this)) return;
				_lastClickTarget = null;
				
				ResetMouseDelta();
				
				if (!isSelected)
				{
					if (!_selectedObject.IsUnityNull())
					{
						_selectedObject.FlagRenderingLayerMask(false);
					}
					_selectedObject = gameObject;
					SetInitialPosition();
					SetInitialRotation();

					gameObject.FlagRenderingLayerMask(!isSelected);
					_releasedInThisFrame = true;
					CameraMouseController.Instance.InvokeClickEvent = false;
				}
			}
		}

		private void SetInitialPosition()
		{
			_initialParent = transform.parent;
			_initialPosition = TargetModel.transform.position;
			_initialRotation = TargetModel.transform.rotation;
		}

		private void ReleaseObject()
		{
			if (!CanRelease()) return;
			ClearSelectedObject();
			new MoveObjectAction(this, _initialPosition, TargetModel.transform.position, _initialRotation, TargetModel.transform.rotation, _initialParent, transform.parent).Register();
		}

		private void CancelMove()
		{
			ClearSelectedObject();
			TargetModel.transform.SetParent(_initialParent);
			TargetModel.transform.SetPositionAndRotation(_initialPosition, _initialRotation);
		}

		private bool CanRelease()
		{
			// (TODO) Wall collide check
			return true;
		}
	}
}
