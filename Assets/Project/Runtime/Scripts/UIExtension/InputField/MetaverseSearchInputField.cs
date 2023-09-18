/*===============================================================
* Product:		Com2Verse
* File Name:	MetaverseSearchInputField.cs
* Developer:	jhkim
* Date:			2022-10-31 14:41
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.InputSystem;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Com2Verse.UI
{
	public sealed class MetaverseSearchInputField : TMP_InputField
	{
		public event Action<MoveDirection> OnMoveCursor = direction => { };
		protected override void Awake()
		{
			base.Awake();
			onSelect.AddListener(OnSelected);
			onDeselect.AddListener(OnDeselected);
		}

		private void OnSelected(string arg0) => InputSystemManager.Instance.AnyButtonPressEvent += OnKeyboardInput;
		private void OnDeselected(string arg0) => InputSystemManager.Instance.AnyButtonPressEvent -= OnKeyboardInput;
		private void OnKeyboardInput(InputControl inputControl)
		{
			switch (inputControl.name)
			{
				case InputSystemManager.Constant.ArrowUp:
					OnMoveCursor?.Invoke(MoveDirection.Up);
					break;
				case InputSystemManager.Constant.ArrowDown:
					OnMoveCursor?.Invoke(MoveDirection.Down);
					break;
				case InputSystemManager.Constant.ArrowLeft:
					OnMoveCursor?.Invoke(MoveDirection.Left);
					break;
				case InputSystemManager.Constant.ArrowRight:
					OnMoveCursor?.Invoke(MoveDirection.Right);
					break;
			}
		}
	}
}
