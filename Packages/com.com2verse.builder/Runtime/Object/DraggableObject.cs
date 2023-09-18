// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	DraggableObject.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-08 오후 3:52
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using Com2Verse.Extension;
using UnityEngine;

namespace Com2Verse.Builder
{
	public abstract class DraggableObject : MonoBehaviour
	{
		private const float NormalChangedThreshold = 0.001f;
		private const float RotationDegree = 90f;

		public GameObject TargetModel { get; set; }
		public BuilderInventoryItem Item { get; set; }
		public int StackLevel => Item.StackLevel;
		public Bounds ModelBounds => Item.Instance.LoadedAsset.IsReferenceNull() ? default : Item.Instance.Bound;
		
		protected Camera BuilderCamera => CameraMouseController.Instance.MainCamera;

		protected Vector3 WorldModelCenter => TargetModel.transform.TransformPoint(ModelBounds.center);
		protected Vector3 WorldModelCenterVector => TargetModel.transform.TransformVector(ModelBounds.center);
		protected Vector3 WorldModelExtents => TargetModel.transform.TransformVector(ModelBounds.extents);

		protected Vector3 LastMousePosition = Vector3.negativeInfinity;
		private Vector3 _mouseDelta = Vector3.zero;
		private Vector3 _lastNormal = Vector3.negativeInfinity;
		private Quaternion _initialRotationInternal = Quaternion.identity;
		
		// rotation helper
		protected Transform TrsTest;
		
		private void Start()
		{
			TrsTest = new GameObject("Transformation").transform;
			TrsTest.parent = transform;
			ResetTester();
		}

		private void ResetTester()
		{
			TrsTest.localPosition = Vector3.zero;
			TrsTest.localRotation = Quaternion.identity;
			TrsTest.localScale = Vector3.one;
		}

		private void Update()
		{
			DoRaycastMoveCheck();
		}

		protected virtual bool DoRaycastMoveCheck()
		{
			Vector3 currentMousePosition = Input.mousePosition;
			bool mouseMoved = LastMousePosition != currentMousePosition;
			LastMousePosition = currentMousePosition;
			
			var ray = BuilderCamera.ScreenPointToRay(currentMousePosition + _mouseDelta);

			if (DoRaycast() && RaycastHelper.FindTopMostCollision(ray, TargetModel.transform, StackLevel, out var hit, out var parent))
			{
				// 마우스 안 움직였으면 오브젝트도 움직이지 않음
				if (mouseMoved)
					MoveObject(ref hit, parent);
				
				return true;
			}

			return false;
		}

		protected virtual bool DoRaycast()
		{
			return true;
		}
		
		protected Vector3 ClipPosition(Vector3 vector, float stepSize, Vector3 normal)
		{
			// Use only if the normal is a world space basis
			return vector - Vector3.Scale(new Vector3(vector.x % stepSize, vector.y % stepSize, vector.z % stepSize), Vector3.one - normal);
		}

		protected void ResetMouseDelta()
		{
			_mouseDelta = BuilderCamera.WorldToScreenPoint(WorldModelCenter - Vector3.Project(WorldModelExtents, TargetModel.transform.up)) - Input.mousePosition;
		}

		protected void SetModelCenterToMouse()
		{
			_mouseDelta = Vector3.zero;
		}
		
		protected virtual void MoveObject(ref RaycastHit hit, Transform parent)
		{
			TargetModel.transform.SetParent(parent, true);

			// Normal이 변경되면 맞춰서 자동 회전하나, 이미 회전이 먹혀있을때는 초기값으로 돌아가지 않도록 함
			if (Vector3.SqrMagnitude(_lastNormal - hit.normal) > NormalChangedThreshold)
				TargetModel.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
			else
				TargetModel.transform.rotation = _initialRotationInternal;
			
			Vector3 hitPoint = hit.point;
			if (Input.GetKey(KeyCode.LeftControl))
				hitPoint = ClipPosition(hitPoint, SpaceManager.GridSize, hit.normal);
			
			TargetModel.transform.position = hitPoint - WorldModelCenterVector + Vector3.Project(WorldModelExtents, hit.normal);
			TargetModel.transform.RoundTransform();
		}

		protected virtual void ProcessRotateObject()
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				Transform targetTransform = TargetModel.transform;
				int sensitiveFactor = Input.GetKey(KeyCode.LeftControl) ? 2 : 1;

				Vector3 basePosition = TargetModel.transform.position;
				Quaternion baseRotation = TargetModel.transform.rotation;

				// Rotate by COM
				TrsTest.SetPositionAndRotation(basePosition, baseRotation);

				TrsTest.RotateAround(TrsTest.TransformPoint(ModelBounds.center), TrsTest.transform.up, RotationDegree / sensitiveFactor);

				targetTransform.SetPositionAndRotation(TrsTest.position, TrsTest.rotation);
				targetTransform.RoundTransform();

				ResetTester();
				SetInitialRotation();
			}
		}

		protected void SetInitialRotation()
		{
			_initialRotationInternal = TargetModel.transform.rotation;
			_lastNormal = TargetModel.transform.up;
		}
	}
}
