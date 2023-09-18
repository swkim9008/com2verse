/*===============================================================
* Product:		Com2Verse
* File Name:	HashUtil.cs
* Developer:	jhkim
* Date:			2023-02-06 10:14
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Com2Verse.HttpHelper
{
	internal class HashUtil
	{
		public static string ComputeHash(Stream stream)
		{
			var hash = new MD5CryptoServiceProvider().ComputeHash(stream);
			return ByteArrayToString(hash);
		}

		public static string ComputeHash(byte[] bytes)
		{
			var hash = new MD5CryptoServiceProvider().ComputeHash(bytes);
			return ByteArrayToString(hash);
		}

		// https://docs.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/compute-hash-values
		public static string ByteArrayToString(byte[] arrInput)
		{
			int i;
			StringBuilder sOutput = new StringBuilder(arrInput.Length);
			for (i = 0; i < arrInput.Length - 1; i++)
			{
				sOutput.Append(arrInput[i].ToString("X2"));
			}

			return sOutput.ToString();
		}
	}
}
