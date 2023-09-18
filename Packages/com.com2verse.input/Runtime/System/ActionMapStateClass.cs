/*===============================================================
* Product:    Com2Verse
* File Name:  StateClass.cs
* Developer:  mikeyid77
* Date:       2022-03-10 17:25
* History:    
* Documents:  
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using UnityEngine;

namespace Com2Verse.InputSystem
{
	public enum eSTATE
	{
		IDLE,
		SET,
		REBIND,
		SAVE,
		LOAD,
		RESET,
	}

	public sealed class ActionMapStateMachine
	{
#region Fields
		private readonly Dictionary<eSTATE, ActionMapState> _stateDict = new();

		private ActionMapState _state;
		private eSTATE         _previousState;
		private eSTATE         _currentState;
#endregion Fields

#region Methods
		public void Init(eSTATE currentState)
		{
			_stateDict.Add(eSTATE.IDLE, new IdleState());
			_stateDict.Add(eSTATE.SET, new SetState());
			_stateDict.Add(eSTATE.REBIND, new RebindState());
			_stateDict.Add(eSTATE.SAVE, new SaveState());
			_stateDict.Add(eSTATE.LOAD, new LoadState());
			_stateDict.Add(eSTATE.RESET, new ResetState());

			_currentState = currentState;
			_state        = _stateDict[_currentState];
			_state.OnStateEnter(this);
		}

		public void ChangeState(eSTATE nextState)
		{
			if (_currentState == nextState) return;
			_state.OnStateExit();
			_previousState = _currentState;
			_currentState  = nextState;
			_state         = _stateDict[_currentState];
			_state.OnStateEnter(this);
		}

		public void ChangePreviousState()
		{
			ChangeState(_previousState);
		}
#endregion Methods
	}

	public abstract class ActionMapState
	{
		protected ActionMapStateMachine _machine;

		public virtual void OnStateEnter(ActionMapStateMachine machine)
		{
			_machine = machine;
			OnState();
		}

		protected abstract void OnState();
		public abstract    void OnStateExit();
	}

	public sealed class IdleState : ActionMapState
	{
		public override void OnStateEnter(ActionMapStateMachine machine)
		{
			base.OnStateEnter(machine);
		}

		protected override void OnState()
		{
			InputSystemManager.Instance.RefreshBindingUi();
		}
		public override    void OnStateExit() { }
	}

	public sealed class SetState : ActionMapState
	{
		public override void OnStateEnter(ActionMapStateMachine machine)
		{
			base.OnStateEnter(machine);
		}

		protected override void OnState()
		{
			InputSystemManager.Instance.SetActionMap();
			_machine.ChangeState(eSTATE.IDLE);
		}

		public override void OnStateExit() { }
	}

	public sealed class RebindState : ActionMapState
	{
		public override void OnStateEnter(ActionMapStateMachine machine)
		{
			Debug.Log("Start Rebind");
			InputSystemManager.Instance.DisableActionMap();
			base.OnStateEnter(machine);
		}

		protected override void OnState()
		{
			var inputSystemManager = InputSystemManager.Instance;
			RebindController.Instance.SetRebinding(
				inputSystemManager.Index,
				inputSystemManager.TargetActionMap,
				inputSystemManager.Device,
				(result) => _machine.ChangeState(eSTATE.SET));
		}

		public override void OnStateExit() { }
	}

	public sealed class SaveState : ActionMapState
	{
		public override void OnStateEnter(ActionMapStateMachine machine)
		{
			Debug.Log("Save ActionMap");
			base.OnStateEnter(machine);
		}

		protected override void OnState()
		{
			InputSystemManager.Instance.SaveActionMap(
				() => _machine.ChangeState(eSTATE.IDLE));
		}

		public override void OnStateExit() { }
	}

	public sealed class LoadState : ActionMapState
	{
		public override void OnStateEnter(ActionMapStateMachine machine)
		{
			Debug.Log("Load ActionMap");
			base.OnStateEnter(machine);
		}

		protected override void OnState()
		{
			InputSystemManager.Instance.LoadActionMap(
				() => _machine.ChangeState(eSTATE.SET));
		}

		public override void OnStateExit() { }
	}

	public sealed class ResetState : ActionMapState
	{
		public override void OnStateEnter(ActionMapStateMachine machine)
		{
			Debug.Log("Reset ActionMap");
			base.OnStateEnter(machine);
		}

		protected override void OnState()
		{
			var inputSystemManager = InputSystemManager.Instance;
			InputSystemManager.Instance.ResetActionMap(
				() => _machine.ChangeState(eSTATE.SET));
		}

		public override void OnStateExit() { }
	}
}
