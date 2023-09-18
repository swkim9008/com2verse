/*===============================================================
* Product:		Com2Verse
* File Name:	RectSizeChangedEventCallback.cs
* Developer:	haminjeong
* Date:			2022-07-20 17:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Utils;
using UnityEngine;

namespace Com2Verse.UIExtension
{
	/// <summary>
	/// Rect의 크기가 변경될 때 호출될 이벤트를 받기 위해 사용됩니다.
	/// 이 컴포넌트를 제외함으로써 특정 오브젝트를 계산에서 제외시킬 수 있습니다.
	/// </summary>
	[RequireComponent(typeof(RectTransform))][ExecuteInEditMode]
	public sealed class RectSizeChangedEventCallback : MonoBehaviour
	{
		private RectTransform _rect;
		private Action<RectTransform> _callback;
		
#region Mono
		private void Awake()
		{
			_rect = Util.GetOrAddComponent<RectTransform>(gameObject);
		}

		private void OnRectTransformDimensionsChange()
		{
			_callback?.Invoke(_rect);
		}

		private void OnDestroy()
		{
			_callback = null;
		}
#endregion	// Mono

		public void SetCallback(Action<RectTransform> cb)
		{
			_callback = cb;
		}
	}
}
