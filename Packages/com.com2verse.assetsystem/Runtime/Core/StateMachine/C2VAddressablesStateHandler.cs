/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesStateHandler.cs
* Developer:	tlghks1009
* Date:			2023-02-17 17:21
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Logger;
using UnityEngine;

namespace Com2Verse.AssetSystem
{
    public class C2VAddressablesStateHandler<T> : IStateHandler<T>
    {
        protected IStateMachine<T> StateMachine { get; private set; }

        protected T Downloader => StateMachine.Owner;

        public void SetMachine(IStateMachine<T> stateMachine) => StateMachine = stateMachine;

        public virtual void OnStateEnter()
        {
            C2VDebug.LogCategory("AssetBundle", $"State Enter : {this.GetType().Name}");
        }

        public virtual void OnStateUpdate() { }

        public virtual void OnStateExit()
        {
            C2VDebug.LogCategory("AssetBundle", $"State Exit : {this.GetType().Name}");
        }
    }
}

