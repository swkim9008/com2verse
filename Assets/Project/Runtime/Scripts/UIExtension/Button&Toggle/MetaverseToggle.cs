/*===============================================================
* Product:		Com2Verse
* File Name:	Toggletest.cs
* Developer:	tlghks1009
* Date:			2022-05-24 16:47
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.InputSystem;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Com2Verse.UI
{
	public sealed class MetaverseToggle : Toggle
	{
		[Serializable]
		public class ToggleClickedEvent : UnityEvent { }

		private Action _onPressAnimationCompletedEvent;
		private Action _onSelectAnimationCompletedEvent;


		public void OnUnityAnimationEventPressed()
		{
			_onPressAnimationCompletedEvent?.Invoke();
		}


		public void OnUnityAnimationEventSelected()
		{
			_onSelectAnimationCompletedEvent?.Invoke();
		}

		public bool IsInteractableInversed
		{
			get => !interactable;
			set => interactable = !value;
		}

		public override void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;
			if (!InputSystemManager.CanClick())
				return;

			base.OnPointerClick(eventData);

			if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
		}
	}
}
