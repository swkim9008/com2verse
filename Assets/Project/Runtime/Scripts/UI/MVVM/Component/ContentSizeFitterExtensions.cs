/*===============================================================
* Product:		Com2Verse
* File Name:	ContentSizeFitterExtensions.cs
* Developer:	ydh
* Date:			2023-05-11 18:01
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using UnityEngine.UI;

namespace Com2Verse.UI
{
	[AddComponentMenu("[DB]/[DB] ContentSizeFitterExtensions")]
	[RequireComponent(typeof(ContentSizeFitter))]
	public sealed class ContentSizeFitterExtensions : MonoBehaviour
	{
		private ContentSizeFitter _contentSizeFitter;

		private void Awake()
		{
			_contentSizeFitter = GetComponent<ContentSizeFitter>();
		}

		public bool Refresh
		{
			get => false;
			set
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_contentSizeFitter.transform);
			}
		}
	}
}