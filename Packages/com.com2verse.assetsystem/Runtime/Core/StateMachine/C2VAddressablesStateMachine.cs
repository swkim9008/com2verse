/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesStateMachine.cs
* Developer:	tlghks1009
* Date:			2023-02-17 17:22
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Logger;
using UnityEngine;

namespace Com2Verse.AssetSystem
{
    public class C2VAddressablesStateMachine<T> : IStateMachine<T>
    {
        public event Action OnDisposed;

        private T _owner;

        private StateMachineBehaviour _stateMachineBehaviour;

        private IStateHandler<T> _currentState;

        private bool _disposed = false;

        public T Owner => _owner;

        public static IStateMachine<T> Create(T owner)
        {
            var behaviour = new GameObject(owner.ToString()).AddComponent<StateMachineBehaviour>();

            var stateMachine = new C2VAddressablesStateMachine<T>();
            stateMachine._owner = owner;
            stateMachine._stateMachineBehaviour = behaviour;

            behaviour.OnUpdateListener += stateMachine.OnUpdate;

            return stateMachine;
        }


        public void StartMachine(IStateHandler<T> initialState)
        {
            if (initialState == null)
            {
                return;
            }

            ChangeState(initialState);
        }


        public void ChangeState(IStateHandler<T> nextState)
        {
            if (_currentState != null)
            {
                if (_currentState == nextState)
                {
                    // TODO : log warning.
                }

                _currentState.OnStateExit();

                _currentState.SetMachine(null);

                _currentState = null;
            }

            _currentState = nextState;

            C2VDebug.LogCategory("AssetBundle",$"CurrentState : {_currentState}");

            _currentState.SetMachine(this);

            _currentState.OnStateEnter();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _owner = default;

            _currentState.OnStateExit();
            _currentState = null;

            _stateMachineBehaviour.Dispose();

            OnDisposed?.Invoke();
            OnDisposed = null;

            C2VDebug.LogCategory("AssetBundle", "StateMachine Disposed");
        }

        private void OnUpdate() => _currentState?.OnStateUpdate();
    }
}
