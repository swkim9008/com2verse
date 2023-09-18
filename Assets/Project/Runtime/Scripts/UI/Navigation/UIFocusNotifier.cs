/*===============================================================
* Product:		Com2Verse
* File Name:	UIFocusNotifier.cs
* Developer:	tlghks1009
* Date:			2022-08-18 16:17
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using UnityEngine.EventSystems;

namespace Com2Verse.UI
{
	public sealed class UIFocusNotifier : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
	{
		private GUIView _guiView;

		private void Awake()
		{
			_guiView = GetComponentInParent<GUIView>(true);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			_guiView.OnFocused();
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (_guiView.WillChangeInputSystem)
			{
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (_guiView.WillChangeInputSystem)
			{
			}
		}
	}
}
