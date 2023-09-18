// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	BasicResponse.cs
//  * Developer:	yangsehoon
//  * Date:		2023-07-14 오후 3:51
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using System.Collections.Specialized;
using System.Text;

namespace WebSocketSharp.Net
{
	internal class BearerResponse : AuthenticationResponse
	{
		public BearerResponse(AuthenticationChallenge challenge, NetworkCredential credentials, uint nonceCount) : base(challenge, credentials, nonceCount) { }
		public BearerResponse(AuthenticationSchemes scheme, NameValueCollection parameters, NetworkCredential credentials, uint nonceCount) : base(scheme, parameters, credentials, nonceCount) { }

		public override string ToAuthenticationString()
		{
			return string.Format("Bearer {0}", Parameters["token"]);
		}
	}
}
