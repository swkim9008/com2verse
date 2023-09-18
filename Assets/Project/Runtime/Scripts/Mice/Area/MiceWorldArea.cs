/*===============================================================
* Product:		Com2Verse
* File Name:	MiceWorldArea.cs
* Developer:	ikyoung
* Date:			2023-03-31 18:31
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Threading;
using Com2Verse.Network;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Mice
{
    public sealed class MiceWorldArea : MiceArea
    {
        public MiceWorldArea() { MiceAreaType = eMiceAreaType.WORLD; }
        
        public override async UniTask RequestEnterMiceLobby(CancellationTokenSource cts)
        {
            await base.RequestEnterMiceLobby(cts);
            C2VDebug.LogMethod(GetType().Name);
            
            Commander.Instance.RequestEnterMiceLobby();
        }
    }
}
