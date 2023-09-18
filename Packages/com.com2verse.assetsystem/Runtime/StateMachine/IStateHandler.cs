/*===============================================================
* Product:		Com2Verse
* File Name:	IStateHandler.cs
* Developer:	tlghks1009
* Date:			2023-02-17 17:23
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com2Verse.AssetSystem
{
    public interface IStateHandler<T>
    {
        void SetMachine(IStateMachine<T> stateMachine);

        void OnStateEnter();

        void OnStateUpdate();

        void OnStateExit();
    }
}
