// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	DragItem.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-07 오후 4:49
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using Com2Verse.Extension;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Com2Verse.Builder
{
	public class ObjectDragItem : DraggableObject
	{
		public Image ThumbnailImage { get; set; }
		public bool AssetLoaded { get; set; }

		private static BaseWallObject _lastTargetWallFloorObject = null;
		private bool ModelLoaded => AssetLoaded && Item.Category == eInventoryItemCategory.OBJECT;
		
#if UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void InitStatic()
		{
			_lastTargetWallFloorObject = null;
		}
#endif

		private bool HandleMouseInput(bool isNotOverUI)
		{
			if (AssetLoaded)
			{
				if (Input.GetMouseButtonUp(0))
				{
					if (isNotOverUI)
					{
						PlaceItem();
						CameraMouseController.Instance.InvokeClickEvent = true;
					}
					else
					{
						Cancel();
					}

					return true;
				}
			}

			if (Input.GetMouseButtonUp(1))
			{
				Cancel();
				return true;
			}

			transform.position = Input.mousePosition;
			return false;
		}

		private void ProcessItem(bool isNotOverUI)
		{
			if (AssetLoaded)
			{
				switch (Item.Category)
				{
					case eInventoryItemCategory.OBJECT:
						if (isNotOverUI)
						{
							ProcessRotateObject();
							// 임시 포지션 조정
							LastMousePosition = Vector3.negativeInfinity;
						}

						bool canPlaceItem = isNotOverUI && DoRaycastMoveCheck();
						ThumbnailImage.enabled = !canPlaceItem;
						TargetModel.SetActive(canPlaceItem);
						break;
					case eInventoryItemCategory.TEXTURE:
						if (isNotOverUI)
						{
							ShowPreviewMaterial();
						}

						break;
				}
			}
		}
		
		private void Update()
		{
			bool isNotOverUI = !EventSystem.current.IsPointerOverGameObject();

			if (HandleMouseInput(isNotOverUI)) return;

			ProcessItem(isNotOverUI);
		}

		protected override void MoveObject(ref RaycastHit hit, Transform parent)
		{
			if (ModelLoaded)
				base.MoveObject(ref hit, parent);
		}

		protected override bool DoRaycast() => ModelLoaded;

		private void ShowPreviewMaterial()
		{
			var targetObject = FindMouseOverObject()?.GetComponent<BaseWallObject>();

			_lastTargetWallFloorObject?.PropagateAction((wall) => wall.ResetTexture());
			_lastTargetWallFloorObject = targetObject;

			if (!targetObject.IsReferenceNull())
			{
				if (Input.GetKey(KeyCode.LeftShift))
				{
					var wallObject = targetObject as BuilderWall;
					if (!wallObject.IsReferenceNull())
					{
						wallObject.PropagateAction((wall) => { wall.CurrentRenderer.material = Item.Instance.LoadedMaterial; });
						return;
					}
				}
				
				targetObject.CurrentRenderer.material = Item.Instance.LoadedMaterial;
			}
		}

		private void Cancel()
		{
			if (!TargetModel.IsUnityNull())
				Destroy(TargetModel);

			Initialize();
			gameObject.SetActive(false);

			_lastTargetWallFloorObject?.PropagateAction((wall) => wall.ResetTexture());
			
			CameraMouseController.Instance.InvokeClickEvent = true;
		}
		
		public void PlaceItem()
		{
			switch (Item.Category)
			{
				case eInventoryItemCategory.OBJECT:
					if (!TargetModel.activeSelf)
						Destroy(TargetModel);
					else
						SpaceManager.Instance.AddObject(TargetModel, Item);

					Initialize();
					break;
				case eInventoryItemCategory.TEXTURE:
					if (!_lastTargetWallFloorObject.IsUnityNull())
						SpaceManager.Instance.ChangeMaterial(_lastTargetWallFloorObject.gameObject, Item.Instance.LoadedMaterial, Item);
					break;
			}
			
			gameObject.SetActive(false);
		}

		public void Initialize()
		{
			TargetModel = null;
			AssetLoaded = false;
			SetModelCenterToMouse();
		}

		private GameObject FindMouseOverObject()
		{
			Ray ray = BuilderCamera.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, (1 << SpaceManager.StackBaseObjectLayer) | (1 << SpaceManager.GroundLayer)))
			{
				return hit.collider.gameObject;
			}

			return null;
		}
	}
}
