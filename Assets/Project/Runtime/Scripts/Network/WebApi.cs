/*===============================================================
* Product:		Com2Verse
* File Name:	WebApi.cs
* Developer:	jhkim
* Date:			2023-04-28 22:41
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;

namespace Com2Verse.Network
{
	public static class WebApi
	{
		static WebApi()
		{
			SetCallback();
		}

		public static void SetCallback()
		{
			BaseUserData.OnAccessTokenChanged -= OnAccessTokenChanged;
			BaseUserData.OnAccessTokenChanged += OnAccessTokenChanged;
		}
		private static void OnAccessTokenChanged(string accessToken)
		{
			Com2Verse.WebApi.Util.Instance.AccessToken = accessToken;
		}
#if UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Reset()
		{
			SetCallback();
		}
#endif // UNITY_EDITOR
	}
}
