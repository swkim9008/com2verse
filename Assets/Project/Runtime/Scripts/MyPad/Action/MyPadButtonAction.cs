/*===============================================================
* Product:		Com2Verse
* File Name:	MyPadButtonAction.cs
* Developer:	mikeyid77
* Date:			2023-04-06 17:49
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com2Verse.Logger;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Com2Verse.UI
{
	public sealed class MyPadButtonAction : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		private eState _currentState = eState.IDLE;
		private enum eState
		{
			IDLE,
			PRESS_START,
			PRESS_END
		}

		private float _pressTime = 0f;
		private bool _isHold = false;
		public UnityEvent ButtonPressCheckEvent;
		
		public bool IsHold
		{
			get => _isHold;
			set => _isHold = value;
		}

		private void Update()
		{
			switch (_currentState)
			{
				case eState.IDLE:
					return;
				case eState.PRESS_START:
					_pressTime += Time.deltaTime;
					if (_pressTime >= 1f) 
						_currentState = eState.PRESS_END;
					break;
				case eState.PRESS_END:
					IsHold = (_pressTime >= 1f);
					_currentState = eState.IDLE;
					_pressTime = 0f;
					ButtonPressCheckEvent?.Invoke();
					break;
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			if (_currentState == eState.IDLE) 
				_currentState = eState.PRESS_START;
		}
 
		public void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			if (_currentState == eState.PRESS_START)
				_currentState = eState.PRESS_END;
		}
	}
}
