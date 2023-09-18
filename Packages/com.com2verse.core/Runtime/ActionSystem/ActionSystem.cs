// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	ActionSystem.cs
//  * Developer:	yangsehoon
//  * Date:		2023-03-07 오후 2:05
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

#nullable enable
using System.Collections.Generic;

namespace Com2Verse
{
	public class ActionSystem : Singleton<ActionSystem>
	{
		private List<ReversibleAction> _actions = new ();
		private int _actionIndex = -1;
		public event System.Action? ActionPerformed;

		private ActionSystem() { }

		public void Initialize()
		{
			ActionPerformed = null;
			_actionIndex = -1;
			_actions.Clear();
		}

		public void PerformAction()
		{
			ActionPerformed?.Invoke();
		}

		internal void RegisterAction(ReversibleAction action)
		{
			for (int i = _actions.Count - 1; i > _actionIndex; i--)
			{
				ReversibleAction targetAction = _actions[i];
				_actions.RemoveAt(i);
				targetAction.Clean();
			}

			_actionIndex++;
			_actions.Add(action);
		}

		public void Redo()
		{
			if (!CanRedo) return;
			
			_actions[++_actionIndex].Do(false);
		}

		public void Undo()
		{
			if (!CanUndo) return;
			
			_actions[_actionIndex--].Undo();
		}

		public bool CanUndo => _actionIndex >= 0;

		public bool CanRedo => _actionIndex < _actions.Count - 1;
	}
}
