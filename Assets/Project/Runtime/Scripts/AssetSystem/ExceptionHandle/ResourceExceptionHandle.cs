/*===============================================================
* Product:		Com2Verse
* File Name:	ResourceExceptionHandle.cs
* Developer:	tlghks1009
* Date:			2023-03-20 17:43
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Logger;
using Com2Verse.UI;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Com2Verse.AssetSystem
{
	public static class ResourceExceptionHandle
	{
		public static void OnExceptionHandler(AsyncOperationHandle handle, Exception exception)
		{
			string errorMessage = string.Empty;

			if (C2VAddressablesExceptionHelper.TryGetExceptionType(exception.GetType(), out eC2VAddressablesErrorCode errorCode))
			{
				errorMessage = $"[C2VAddressablesException] ErrorCode : {(int) errorCode}, Message : {exception.Message}";
			}
			else
			{
				errorMessage = $"[C2VAddressablesException] ExceptionType : {exception.GetType()}, Message : {exception.Message}";
			}

			C2VDebug.LogError(errorMessage);
			UIManager.Instance.ShowPopupCommon(errorMessage);
		}
	}
}
