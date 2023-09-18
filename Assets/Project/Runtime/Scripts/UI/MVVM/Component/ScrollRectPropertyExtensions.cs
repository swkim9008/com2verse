/*===============================================================
* Product:		Com2Verse
* File Name:	ScrollViewExtensions.cs
* Developer:	ksw
* Date:			2022-12-08 17:52
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Extension;
using Com2Verse.Project.RecyclableScroll;
using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse
{
	[RequireComponent(typeof(ScrollRect))]
	public sealed class ScrollRectPropertyExtensions : MonoBehaviour
	{
		private ScrollRect           _scrollRect;
		private RecyclableScrollRect _recyclableScrollRect;

		public ScrollRect ScrollRect
		{
			get
			{
				if (_scrollRect.IsReferenceNull())
				{
					_scrollRect = GetComponent<ScrollRect>();
				}

				return _scrollRect;
			}
			set => _scrollRect = value;
		}

		public RecyclableScrollRect RecyclableScrollRect
		{
			get
			{
				if (_recyclableScrollRect.IsReferenceNull())
				{
					_recyclableScrollRect = GetComponent<RecyclableScrollRect>();
				}

				return _recyclableScrollRect;
			}
			set => _recyclableScrollRect = value;
		}
	}
}
