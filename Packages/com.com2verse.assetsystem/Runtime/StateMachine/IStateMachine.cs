/*===============================================================
* Product:		Com2Verse
* File Name:	IStateMachine.cs
* Developer:	tlghks1009
* Date:			2023-02-17 17:23
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;

namespace Com2Verse.AssetSystem
{
    public interface IStateMachine<T> : IDisposable
    {
        event Action OnDisposed;

        T Owner { get; }

        void StartMachine(IStateHandler<T> initialState);

        void ChangeState(IStateHandler<T> nextState);
    }
}