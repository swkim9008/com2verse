/*===============================================================
* Product:		Com2Verse
* File Name:	ExceptionUtils.cs
* Developer:	urun4m0r1
* Date:			2022-05-30 11:08
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Logger;

namespace Com2Verse.Utils
{
	public static class ExceptionUtils
	{
		public static void ThrowIfFails<T>(int result, string? methodName = null) where T : Exception, new()
		{
			if (IsFailed(result))
			{
				var message   = GetFailureMessage(result, methodName);
				var exception = Activator.CreateInstance(typeof(T), message) as T;
				throw exception ?? throw new Exception(message);
			}
		}

		public static void LogErrorIfFails(int result, string? methodName = null)
		{
			if (IsFailed(result)) C2VDebug.LogError(GetFailureMessage(result, methodName));
		}

		public static bool IsFailed(int result) => result < 0;

		private static string GetFailureMessage(int result, string? methodName = null)
		{
			var method = methodName ?? "Unknown method";
			return $"{method} failed with result: {result.ToString()}";
		}
	}
}
