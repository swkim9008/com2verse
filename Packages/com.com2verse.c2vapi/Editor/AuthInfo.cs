/*===============================================================
* Product:		Com2Verse
* File Name:	AuthInfo.cs
* Developer:	jhkim
* Date:			2023-04-06 18:12
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Newtonsoft.Json;

namespace Com2Verse.WebApi
{
	public static class AuthInfo
	{
#region Variables
		// https://test-auth.com2verse.com/api/auth/v1/sign-dev
		private static readonly string C2VAuthServer = "https://test-auth.com2verse.com"; // "http://34.64.204.148:20011";

		private static readonly string APIAuthV1 = "/api/auth/v1";

		private static readonly string APIAuthSignDev = $"{APIAuthV1}/sign-dev";
		private static readonly string APIServicePeek = $"{APIAuthV1}/service/peek";

		public static readonly string C2VAuthSignURL = $"{C2VAuthServer}{APIAuthSignDev}";
		public static readonly string C2VServicePeekURL = $"{C2VAuthServer}{APIServicePeek}";

		public static readonly string ContentJson = "application/json";
#endregion // Variables

#region Request
		[Serializable]
		public class LoginRequest
		{
			[JsonProperty("did")]
			public string Did;

			[JsonProperty("hiveToken")]
			public string HiveToken;

			[JsonProperty("hiveValidate")]
			public bool HiveValidate;

			[JsonProperty("pid")]
			public string PId;

			[JsonProperty("platform")]
			public int Platform;

			[JsonProperty("accessExpires")]
			public long AccessExpires;

			[JsonProperty("refreshExpires")]
			public long RefreshExpires;
		}
#endregion // Request

#region Response
		[Serializable]
		public class BaseResponse
		{
			public int code;
			public string msg;
		}

		[Serializable]
		public class LoginResponse : BaseResponse
		{
			public AuthSignInResponse data;
		}

		[Serializable]
		public class AuthSignInResponse
		{
			public int accountId;
			public string c2vAccessToken;
			public string c2vRefreshToken;
		}

		[Serializable]
		public class ServicePeekResponse : BaseResponse
		{
			public AuthServicePeekResponse data;
		}

		[Serializable]
		public class AuthServicePeekResponse
		{
			public string serviceUri;
		}
#endregion // Response
	}
}
