/*===============================================================
* Product:		Com2Verse
* File Name:	C2VAddressablesExceptionHandler.cs
* Developer:	tlghks1009
* Date:			2023-02-17 17:19
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Logger;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Exceptions;

namespace Com2Verse.AssetSystem
{
    public enum eC2VAddressablesErrorCode
    {
        [AddressableException(typeof(RemoteProviderException))] REMOTE_PROVIDER_EXCEPTION = 1,

        [AddressableException(typeof(OperationException))] OPERATION_EXCEPTION = 2,

        [AddressableException(typeof(InvalidKeyException))] INVALID_KEY_EXCEPTION = 2,
    }

    public class C2VAddressablesExceptionHandler
    {
        public static void OnExceptionHandler(AsyncOperationHandle handle, Exception exception)
        {
            if (C2VAddressablesExceptionHelper.TryGetExceptionType(exception.GetType(), out eC2VAddressablesErrorCode errorCode))
            {
                C2VDebug.LogError($"[C2VAddressablesException] ErrorCode : {(int) errorCode}, Message : {exception.Message}");
            }
            else
            {
                C2VDebug.LogError($"[C2VAddressablesException] ExceptionType : {exception.GetType()}, Message : {exception.Message}");
            }
        }
    }
}