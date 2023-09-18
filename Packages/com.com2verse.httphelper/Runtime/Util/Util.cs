/*===============================================================
* Product:		Com2Verse
* File Name:	Util.cs
* Developer:	jhkim
* Date:			2023-02-06 10:14
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Linq;
using System.Text;

namespace Com2Verse.HttpHelper
{
	public static class Util
	{
#region Variables
		public const string KeyID = "id";
		public const string KeyPassword = "password";
		public const string KeyAuthtoken = "Bearer";
#endregion // Variables

#region Public Methods
		public static (string, string)[] MakeBasicAuthInfo(string id, string password) => new[]
		{
			(KEY_ID: KeyID, id),
			(KEY_PASSWORD: KeyPassword, password),
		};

		public static (string, string)[] MakeTokenAuthInfo(string token) => new[]
		{
			(KEY_AUTHTOKEN: KeyAuthtoken, token),
		};

		public static string MakeUrlWithParam(string url, (string, string)[] pairs)
		{
			StringBuilder sb = new StringBuilder(url);
			var appendParam = false;
			if (HasParam())
				sb.Append("?");
			else
				return sb.ToString();

			for (int i = 0; i < pairs.Length; ++i)
			{
				if (!IsValidParam(pairs[i])) continue;

				if (appendParam)
					sb.Append("&");

				sb.Append($"{pairs[i].Item1}={pairs[i].Item2}");
				appendParam = true;
			}

			return sb.ToString();

			bool HasParam() => pairs.Any(p => !string.IsNullOrWhiteSpace(p.Item1) && !string.IsNullOrWhiteSpace(p.Item2));
			bool IsValidParam((string, string) pair) => !string.IsNullOrWhiteSpace(pair.Item1) && !string.IsNullOrWhiteSpace(pair.Item2);
		}
#endregion // Public Methods
	}
}
