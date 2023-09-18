// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	ReversibleAction.cs
//  * Developer:	yangsehoon
//  * Date:		2023-04-24 오후 12:27
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using System.Collections.Generic;

namespace Com2Verse
{
	public abstract class ReversibleAction
	{
		/// <summary>
		/// 액션을 되돌립니다.
		/// </summary>
		public virtual void Undo()
		{
			ActionSystem.Instance.PerformAction();
		}

		/// <summary>
		/// 액션을 실행합니다.
		/// </summary>
		/// <param name="isInitial">등록되는 액션인지 여부 (Redo로 실행하는 경우 false)</param>
		public virtual void Do(bool isInitial = true)
		{
			if (isInitial) ActionSystem.Instance.RegisterAction(this);
			ActionSystem.Instance.PerformAction();
		}

		/// <summary>
		/// 액션을 등록만 합니다. (이미 액션이 로직을 통해 실행되어있는 경우 action system에 등록만 합니다)
		/// </summary>
		public void Register()
		{
			ActionSystem.Instance.RegisterAction(this);
			ActionSystem.Instance.PerformAction();
		}

		/// <summary>
		/// 액션을 시스템에서 해당 액션이 완전히 제거되었을 때(Redo로 도달 할 수 없게 되었을 때) 실행됩니다.
		/// </summary>
		public virtual void Clean() { }
	}


	/// <summary>
	/// 액션의 그룹을 한번에 처리하기 위한 클래스
	/// </summary>
	public class ActionGroup : ReversibleAction
	{
		private List<ReversibleAction> _actionList;

		public ActionGroup(List<ReversibleAction> actions)
		{
			_actionList = new List<ReversibleAction>();
			foreach (var action in actions)
			{
				_actionList.Add(action);
			}
		}

		public override void Undo()
		{
			base.Undo();
			foreach (var action in _actionList)
			{
				action.Undo();
			}
		}

		public override void Do(bool isInitial = true)
		{
			base.Do(isInitial);
			foreach (var action in _actionList)
			{
				action.Do(false);
			}
		}
	}
}
