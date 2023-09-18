/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAsyncOperationHandleExtensions.cs
* Developer:	tlghks1009
* Date:			2023-02-17 17:23
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Cysharp.Threading.Tasks;

namespace Com2Verse.AssetSystem
{
    public static class C2VAsyncOperationHandleExtensions
    {
        public static UniTask<T> ToUniTask<T>(this C2VAsyncOperationHandle<T> thisHandle) => thisHandle.ToUniTask();

        public static UniTask ToUniTask(this C2VAsyncOperationHandle thisHandle) => thisHandle.ToUniTask();
    }
}
