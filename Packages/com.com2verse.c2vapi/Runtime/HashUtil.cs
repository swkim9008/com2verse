/*===============================================================
* Product:		Com2Verse
* File Name:	HashUtil.cs
* Developer:	jhkim
* Date:			2023-06-07 16:27
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.IO;
using UnityEngine;

namespace Com2Verse.WebApi
{
	public static class HashUtil
	{
#region Hash
		public static bool TryGenerateHash(string filePath, out string hashStr)
		{
			hashStr = string.Empty;
			try
			{
				using (var fs = File.OpenRead(filePath))
				{
					var hash = System.Security.Cryptography.MD5.Create().ComputeHash(fs);
					hashStr = Convert.ToBase64String(hash);
				}

				return true;
			}
			catch (Exception e)
			{
				// C2VDebug.LogWarning($"Hash 생성 실패...\n{e}");
				return false;
			}
		}

		public static bool TryGenerateHash(byte[] bytes, out string hashStr)
		{
			hashStr = string.Empty;
			if (bytes == null)
			{
				// C2VDebug.LogWarning("Hash 생성 실패");
				return false;
			}

			var hash = System.Security.Cryptography.MD5.Create().ComputeHash(bytes);
			hashStr = Convert.ToBase64String(hash);
			return true;
		}

		public static bool TryGenerateHash(AudioClip clip, out string hashStr)
		{
			var bytes = GetBytes(clip);
			return TryGenerateHash(bytes, out hashStr);
		}

		private static byte[] GetBytes(AudioClip audioClip)
		{
			if (audioClip == null) return null;

			// https://forum.unity.com/threads/convert-audioclip-to-byte.996356/
			var samples = new float[audioClip.samples];
			audioClip.GetData(samples, 0);

			var ms = new MemoryStream();
			var bw = new BinaryWriter(ms);

			var length = samples.Length;
			bw.Write(length);

			foreach (var sample in samples)
				bw.Write(sample);
			return ms.ToArray();
		}
#endregion // Hash
	}
}
