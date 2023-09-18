/*===============================================================
* Product:		Com2Verse
* File Name:	ImageHelper.cs
* Developer:	klizzard
* Date:			2023-04-05 12:56
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.IO;
using System.Net;
using System.Text;

namespace Com2Verse.QrCode.Helpers
{
	public static class ImageHelper
	{
		public static string ConvertImageURLToBase64(string url)
		{
			StringBuilder sb = new StringBuilder();

			byte[] bytes = GetImage(url);
			if (bytes != null)
				sb.Append(Convert.ToBase64String(bytes, 0, bytes.Length));

			return sb.ToString();
		}

		private static byte[] GetImage(string url)
		{
			byte[] buf;

			try
			{
				WebProxy myProxy = new WebProxy();
				HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

				HttpWebResponse response = (HttpWebResponse)req.GetResponse();
				var stream = response.GetResponseStream();

				using (BinaryReader br = new BinaryReader(stream))
				{
					int len = (int)(response.ContentLength);
					buf = br.ReadBytes(len);
					br.Close();
				}

				stream.Close();
				response.Close();
			}
			catch (Exception exp)
			{
				buf = null;
			}

			return (buf);
		}
	}
}
