/*===============================================================
* Product:		Com2Verse
* File Name:	ChildRectSizeFitter.cs
* Developer:	haminjeong
* Date:			2022-07-20 17:37
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Utils;
using UnityEngine;

namespace Com2Verse.UIExtension
{
	/// <summary>
	/// 자식들의 크기를 조회하여 현재 오브젝트의 크기를 변화시킵니다.
	/// 자식들의 크기를 판단하는 기준은 RectSizeChangedEventCallback 컴포넌트가 붙어 있는지 여부이므로,
	/// 자식들의 위치관계를 파악하진 않습니다.
	/// 자식은 바로 아래 단계 깊이만 탐색합니다.
	/// </summary>
	[RequireComponent(typeof(RectTransform))][ExecuteInEditMode]
	public sealed class ChildRectSizeFitter : MonoBehaviour
	{
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
#endregion	// Mono

		private void RefreshRegisterCallbacks()
		{
			foreach (Transform child in transform)
			{
				var observer = child.GetComponent<RectSizeChangedEventCallback>();
				if (observer == null) continue;
				observer.SetCallback(OnSizeUpdated);
			}
		}

		private void OnSizeUpdated(RectTransform rect)
		{
			Vector2 size = Vector2.zero;
			foreach (Transform child in transform)
			{
				if (!child.gameObject.activeSelf) continue;
				var childRect = child.GetComponent<RectTransform>();
				var observer = child.GetComponent<RectSizeChangedEventCallback>();
				if (childRect == null || observer == null) continue;
				size.x += _horizontal ? childRect.rect.size.x : 0;
				size.y += _vertical ? childRect.rect.size.y : 0;
			}
			size -= _paddingOffset;
			_rect.sizeDelta = new Vector2(_horizontal ? size.x : _rect.sizeDelta.x, _vertical ? size.y : _rect.sizeDelta.y);
		}
	}
}
