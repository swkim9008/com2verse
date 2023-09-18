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
	internal class BasicResponse : AuthenticationResponse
	{
		public BasicResponse(NetworkCredential credentials) : base(credentials) { }
		public BasicResponse(AuthenticationSchemes scheme, NameValueCollection parameters) : base(scheme, parameters) { }
		public BasicResponse(AuthenticationChallenge challenge, NetworkCredential credentials, uint nonceCount) : base(challenge, credentials, nonceCount) { }
		public BasicResponse(AuthenticationSchemes scheme, NameValueCollection parameters, NetworkCredential credentials, uint nonceCount) : base(scheme, parameters, credentials, nonceCount) { }

		public override string ToAuthenticationString()
		{
			var userPass = String.Format("{0}:{1}", Parameters["username"], Parameters["password"]);
			var cred = Convert.ToBase64String(Encoding.UTF8.GetBytes(userPass));

			return "Basic " + cred;
		}
		
		internal static NameValueCollection ParseBasicCredentials(string value)
		{
			// Decode the basic-credentials (a Base64 encoded string).
			var userPass = Encoding.Default.GetString(Convert.FromBase64String(value));

			// The format is [<domain>\]<username>:<password>.
			var i = userPass.IndexOf(':');
			var user = userPass.Substring(0, i);
			var pass = i < userPass.Length - 1 ? userPass.Substring(i + 1) : String.Empty;

			// Check if 'domain' exists.
			i = user.IndexOf('\\');
			if (i > -1)
				user = user.Substring(i + 1);

			var res = new NameValueCollection();
			res["username"] = user;
			res["password"] = pass;

			return res;
		}
	}
}
