// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	InventoryItem.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-07 오후 4:39
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using Com2Verse.Extension;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Com2Verse.Builder
{
	public class InventoryItemComponent : MonoBehaviour, IPointerClickHandler
	{
		public RectTransform CanvasRoot { get; set; }
		public BuilderInventoryItem Data { private get; set; }

		private readonly Vector2 _imageAbsoluteSize = new Vector2(128, 128);
		private Image _image;
		private static ObjectDragItem _objectDrag;

		private void Awake()
		{
			_image = GetComponent<Image>();
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			BuilderObject.ClearSelectedObject();

			if (_objectDrag.IsUnityNull())
			{
				var dragObject = new GameObject("DragObject");
				_objectDrag = dragObject.AddComponent<ObjectDragItem>();
				dragObject.AddComponent<RectTransform>().sizeDelta = _imageAbsoluteSize;
				_objectDrag.ThumbnailImage = dragObject.AddComponent<Image>();
				_objectDrag.ThumbnailImage.raycastTarget = false;
				_objectDrag.transform.SetParent(CanvasRoot);
				_objectDrag.transform.localScale = Vector3.one;
			}
			else
			{
				_objectDrag.gameObject.SetActive(true);

				if (!_objectDrag.TargetModel.IsUnityNull() && !_objectDrag.TargetModel.activeSelf)
				{
					_objectDrag.AssetLoaded = false;
					Destroy(_objectDrag.TargetModel);
				}
			}

			_objectDrag.Initialize();

			var imageComponent = _objectDrag.GetComponent<Image>();
			imageComponent.sprite = _image.sprite;
			imageComponent.enabled = true;
			_objectDrag.transform.position = transform.position;
			
			switch (Data.Category)
			{
				case eInventoryItemCategory.OBJECT:
					GameObject parentObject = new GameObject(Data.AddressableId);

					if (!Data.Instance.LoadedAsset.IsReferenceNull())
					{
						var newObject = Instantiate(Data.Instance.LoadedAsset);
						newObject.transform.SetParent(parentObject.transform);
						_objectDrag.AssetLoaded = true;
					}
					else
					{
						CheckAssetLoading(parentObject).Forget();
						_objectDrag.AssetLoaded = false;
					}

					_objectDrag.TargetModel = parentObject;
					_objectDrag.TargetModel.SetActive(false);
					break;
				case eInventoryItemCategory.TEXTURE:
					if (Data.Instance.LoadedMaterial.IsReferenceNull())
					{
						CheckAssetLoading().Forget();
						_objectDrag.AssetLoaded = false;
					}
					else
					{
						_objectDrag.AssetLoaded = true;
					}
					
					break;
			}

			_objectDrag.Item = Data;
			CameraMouseController.Instance.InvokeClickEvent = false;
		}

		private async UniTask CheckAssetLoading(GameObject parentObject = null)
		{
			switch (Data.Category)
			{
				case eInventoryItemCategory.OBJECT:
					while (Data.Instance.LoadedAsset.IsReferenceNull())
					{
						await UniTask.Yield();
					}

					// Check canceled before model loading
					if (!parentObject.IsUnityNull())
					{
						var newObject = Instantiate(Data.Instance.LoadedAsset);
						newObject.transform.SetParent(parentObject.transform);
					}
					break;
				case eInventoryItemCategory.TEXTURE:
					while (Data.Instance.LoadedMaterial.IsReferenceNull())
					{
						await UniTask.Yield();
					}
					
					break;
			}

			_objectDrag.AssetLoaded = true;
		}
	}
}
