// /*===============================================================
//  * Product:		Com2Verse
//  * File Name:	BasicChallenge.cs
//  * Developer:	yangsehoon
//  * Date:		2023-07-14 오후 3:39
//  * History:
//  * Documents:
//  * Copyright ⓒ Com2Verse. All rights reserved.
//  ================================================================*/

using System;
using System.Collections.Specialized;

namespace WebSocketSharp.Net
{
	internal class BasicChallenge : AuthenticationChallenge
	{
		internal BasicChallenge(AuthenticationSchemes scheme, string realm) : base(scheme, realm) { }
		internal BasicChallenge(AuthenticationSchemes scheme, NameValueCollection collection) : base(scheme, collection) { }

		public override string ToAuthenticationString()
		{
			return String.Format("Basic realm=\"{0}\"", Parameters["realm"]);
		}
	}
}
