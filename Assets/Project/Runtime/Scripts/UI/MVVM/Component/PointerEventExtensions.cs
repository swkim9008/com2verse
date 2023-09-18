/*===============================================================
* Product:		Com2Verse
* File Name:	PointerEventExtensions.cs
* Developer:	tlghks1009
* Date:			2022-07-26 14:35
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Com2Verse.UI
{
    [AddComponentMenu("[DB]/[DB] PointerEventExtensions")]
    public sealed class PointerEventExtensions : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [HideInInspector] public UnityEvent<bool> _isPointerEnterEvent;
        [HideInInspector] public UnityEvent<bool> _isPointerExitEvent;
        [HideInInspector] public UnityEvent<bool> _isPointerOnlyEnterEvent;
        [HideInInspector] public UnityEvent<bool> _isPointerOnlyExitEvent;
        
        [HideInInspector] public UnityEvent _isPointerClickEvent;
        [HideInInspector] public UnityEvent _isPointerMiddleClickEvent;
        [HideInInspector] public UnityEvent _isPointerRightClickEvent;

        public bool IsPointerEnter { get; set; }
        public bool IsPointerExit { get; set; }


        public void OnPointerEnter(PointerEventData eventData)
        {
            IsPointerEnter = true;
            _isPointerEnterEvent.Invoke(IsPointerEnter);
            IsPointerExit = false;
            _isPointerExitEvent.Invoke(IsPointerExit);

            _isPointerOnlyEnterEvent?.Invoke(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsPointerExit = true;
            _isPointerExitEvent.Invoke(IsPointerExit);
            IsPointerEnter = false;
            _isPointerEnterEvent.Invoke(IsPointerEnter);

            _isPointerOnlyExitEvent?.Invoke(true);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    _isPointerClickEvent?.Invoke();
                    break;
                case PointerEventData.InputButton.Middle:
                    _isPointerMiddleClickEvent?.Invoke();
                    break;
                case PointerEventData.InputButton.Right:
                    _isPointerRightClickEvent?.Invoke();
                    break;
            }
        }
    }
}
