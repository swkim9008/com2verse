/*===============================================================
* Product:    Com2Verse
* File Name:  TabNavigation.cs
* Developer:  tlghks1009
* Date:       2022-04-11 16:53
* History:    
* Documents:  
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Com2Verse.UI
{
    [AddComponentMenu("[CVUI]/[CVUI] TabNavigator")]
	public sealed class TabNavigator : MonoBehaviour
	{   
        private EventSystem _eventSystem;

        private Selectable _currentSelectable = null;

        private bool _isShiftHeld => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        

        private void OnEnable()
        {
            _eventSystem = EventSystem.current;
            _currentSelectable = null;
            
            UIManager.Instance.AddUpdateListener( OnUpdate, true );
        }

        private void OnDisable()
        {
            _eventSystem = null;
            _currentSelectable = null;
            
            UIManager.InstanceOrNull?.RemoveUpdateListener( OnUpdate );
        }


        private void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                FocusNextSelectable();
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                SimulateClick ( _currentSelectable );
            }
        }
        

        private void FocusNextSelectable()
        {
            var nextSelectable = GetNextSelectable();

            if (!nextSelectable)
            {
                return;
            }

            _currentSelectable = nextSelectable;

            _eventSystem.SetSelectedGameObject(nextSelectable.gameObject);
        
        }

        
        private Selectable GetNextSelectable()
        {

            if ( object.ReferenceEquals( _eventSystem.currentSelectedGameObject, null ) )
                return null;

            var selectable = _eventSystem.currentSelectedGameObject.GetComponent<Selectable>();

            if ( object.ReferenceEquals( selectable, null ) )
                return null;

            return _isShiftHeld ? selectable.FindSelectableOnUp() : selectable.FindSelectableOnDown();
        }
        
        
        private void SimulateClick(Selectable selectable)
        {
            if (!selectable)
            {
                return;
            }
            
            var clickableElement = selectable.GetComponent<IPointerClickHandler>();

            if (clickableElement == null)
                return;
            

            var eventData = new PointerEventData(_eventSystem);
            
            clickableElement.OnPointerClick(eventData);
        }
    }
}
