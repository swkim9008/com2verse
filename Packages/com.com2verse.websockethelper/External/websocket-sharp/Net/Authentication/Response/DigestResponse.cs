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
	internal class DigestResponse : AuthenticationResponse
	{
		public DigestResponse(NetworkCredential credentials) : base(credentials) { }
		public DigestResponse(AuthenticationSchemes scheme, NameValueCollection parameters) : base(scheme, parameters) { }
		public DigestResponse(AuthenticationChallenge challenge, NetworkCredential credentials, uint nonceCount) : base(challenge, credentials, nonceCount) { }
		public DigestResponse(AuthenticationSchemes scheme, NameValueCollection parameters, NetworkCredential credentials, uint nonceCount) : base(scheme, parameters, credentials, nonceCount) { }

		public override string ToAuthenticationString()
		{
			var output = new StringBuilder(256);
			output.AppendFormat(
				"Digest username=\"{0}\", realm=\"{1}\", nonce=\"{2}\", uri=\"{3}\", response=\"{4}\"",
				Parameters["username"],
				Parameters["realm"],
				Parameters["nonce"],
				Parameters["uri"],
				Parameters["response"]);

			var opaque = Parameters["opaque"];
			if (opaque != null)
				output.AppendFormat(", opaque=\"{0}\"", opaque);

			var algo = Parameters["algorithm"];
			if (algo != null)
				output.AppendFormat(", algorithm={0}", algo);

			var qop = Parameters["qop"];
			if (qop != null)
				output.AppendFormat(
					", qop={0}, cnonce=\"{1}\", nc={2}", qop, Parameters["cnonce"], Parameters["nc"]);

			return output.ToString();
		}
	}
}
