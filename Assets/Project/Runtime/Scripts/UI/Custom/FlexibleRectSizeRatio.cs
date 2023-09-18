/*===============================================================
* Product:		Com2Verse
* File Name:	FlexibleRectSizeRatio.cs
* Developer:	haminjeong
* Date:			2022-07-20 22:17
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
using Com2Verse.Utils;
using UnityEngine;

namespace Com2Verse.UIExtension
{
	/// <summary>
	/// 부모의 전체 크기를 고정하고 현재 오브젝트를 제외한 자식들의 크기를 뺀 결과를 현재 오브젝트 크기로 정하여,
	/// 현재 오브젝트의 크기가 가변적으로 변하는 효과를 얻습니다.
	/// 한 부모 아래 해당 컴포넌트를 가진 오브젝트가 한 개일 경우에만 정상 동작됩니다.
	/// 자식은 바로 아래 단계 깊이만 탐색합니다.
	/// </summary>
	[RequireComponent(typeof(RectTransform))] [ExecuteInEditMode]
	public sealed class FlexibleRectSizeRatio : MonoBehaviour
	{
		[field: Tooltip("부모의 크기 변화도 이벤트로 반영할 지 여부"), SerializeField] private bool _containsParent = true;
		[field: Tooltip("세로 반영 여부"), SerializeField] private bool _vertical = false;
		[field: Tooltip("가로 반영 여부"), SerializeField] private bool _horizontal = false;
		[field: Tooltip("사이즈에서 차감될 총 패딩"), SerializeField] private Vector2 _paddingOffset = Vector2.zero;
		private RectTransform _rect;

#region Mono
		private void Awake()
		{
			_rect = Util.GetOrAddComponent<RectTransform>(gameObject);
			RefreshRegisterCallbacks();
		}

		private void OnValidate()
		{
			RefreshRegisterCallbacks();
		}
#endregion // Mono

		private void RefreshRegisterCallbacks()
		{
			if (transform.parent.IsReferenceNull()) return;
			if (_containsParent)
			{
				var parentObserver = transform.parent.GetComponent<RectSizeChangedEventCallback>();
				if (parentObserver != null)
					parentObserver.SetCallback(OnSizeUpdated);
			}
			foreach (Transform child in transform.parent)
			{
				if (child == transform) continue;
				var observer = child.GetComponent<RectSizeChangedEventCallback>();
				if (observer == null) continue;
				observer.SetCallback(OnSizeUpdated);
			}
		}

		private void OnSizeUpdated(RectTransform rect)
		{
			var parentRect = transform.parent.GetComponent<RectTransform>();
			if (parentRect == null) return;
			Vector2 size = parentRect.rect.size;
			foreach (Transform child in transform.parent)
			{
				if (child == transform) continue;
				if (!child.gameObject.activeSelf) continue;
				var childRect = child.GetComponent<RectTransform>();
				var observer = child.GetComponent<RectSizeChangedEventCallback>();
				if (childRect == null || observer == null) continue;
				size.x -= _horizontal ? childRect.rect.size.x : 0;
				size.y -= _vertical ? childRect.rect.size.y : 0;
			}
			size -= _paddingOffset;
			_rect.sizeDelta = new Vector2(_horizontal ? size.x : _rect.sizeDelta.x, _vertical ? size.y : _rect.sizeDelta.y);
		}
	}
}
