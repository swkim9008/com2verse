/*===============================================================
* Product:		Com2Verse
* File Name:	RecyclableCellExtensions.cs
* Developer:	ksw
* Date:			2023-05-12 10:59
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse.UI
{
	[RequireComponent(typeof(RectTransformPropertyExtensions), typeof(LayoutElement))]
	public sealed class RecyclableCellExtensions : MonoBehaviour
	{
		private LayoutElement                   _layoutElement;
		private RectTransformPropertyExtensions _rectTransformPropertyExtensions;
		private void Awake()
		{
			_layoutElement                   = GetComponent<LayoutElement>();
			_rectTransformPropertyExtensions = GetComponent<RectTransformPropertyExtensions>();

			_rectTransformPropertyExtensions._onChangeHeight.AddListener(OnChangedHeight);
		}

		private void OnChangedHeight(float height)
		{
			_layoutElement.minHeight = height;
		}
	}
}
