/*===============================================================
* Product:		Com2Verse
* File Name:	StackRegisterer.cs
* Developer:	mikeyid77
* Date:			2023-06-07 14:57
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Data;
using Com2Verse.InputSystem;
using Com2Verse.Logger;
using UnityEngine;

namespace Com2Verse.UI
{
	public abstract class StackRegisterer : MonoBehaviour
	{
		public bool NeedCharacterMove = false;
		public bool NeedEscControl    = true;

		public StackRegisterer Registerer
		{
			get => this;
			set => C2VDebug.LogWarningCategory("UIStackManager", "Can't Set EscapeRegisterer");
		}

		private Action _wantsToQuit = null;
		public event Action WantsToQuit
		{
			add
			{
				_wantsToQuit -= value;
				_wantsToQuit += value;
			}
			remove => _wantsToQuit -= value;
		}

		public abstract void HideComplete();

		protected void AddToManager(string targetName, eInputSystemState targetState)
		{
			UIStackManager.Instance.AddByObject(gameObject, targetName, OnQuit, targetState, NeedEscControl);
		}

		protected void RemoveFromManager()
		{
			UIStackManager.InstanceOrNull?.RemoveByObject(gameObject);
		}

		protected void FinishViewEvent()
		{
			// TODO : 필요 시 사용
			// ...
		}

		protected void OnQuit()
		{
			if (_wantsToQuit != null)
				_wantsToQuit?.Invoke();
			else
				HideComplete();
		}
	}
}
