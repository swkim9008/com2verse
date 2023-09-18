// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	BasicChallenge.cs
//  * Developer:	yangsehoon
//  * Date:		2023-07-14 오후 3:39
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System.Collections.Specialized;
using System.Text;

namespace WebSocketSharp.Net
{
	internal class DigestChallenge : AuthenticationChallenge
	{
		internal DigestChallenge(AuthenticationSchemes scheme, string realm) : base(scheme, realm) { }
		internal DigestChallenge(AuthenticationSchemes scheme, NameValueCollection collection) : base(scheme, collection) { }

		public override string ToAuthenticationString()
		{
			var output = new StringBuilder(128);

			var domain = Parameters["domain"];
			if (domain != null)
				output.AppendFormat(
					"Digest realm=\"{0}\", domain=\"{1}\", nonce=\"{2}\"",
					Parameters["realm"],
					domain,
					Parameters["nonce"]);
			else
				output.AppendFormat(
					"Digest realm=\"{0}\", nonce=\"{1}\"", Parameters["realm"], Parameters["nonce"]);

			var opaque = Parameters["opaque"];
			if (opaque != null)
				output.AppendFormat(", opaque=\"{0}\"", opaque);

			var stale = Parameters["stale"];
			if (stale != null)
				output.AppendFormat(", stale={0}", stale);

			var algo = Parameters["algorithm"];
			if (algo != null)
				output.AppendFormat(", algorithm={0}", algo);

			var qop = Parameters["qop"];
			if (qop != null)
				output.AppendFormat(", qop=\"{0}\"", qop);

			return output.ToString();
		}
	}
}
