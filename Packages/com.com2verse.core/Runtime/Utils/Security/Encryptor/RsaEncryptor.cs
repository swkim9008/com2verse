/*===============================================================
* Product:		Com2Verse
* File Name:	RsaEncryptor.cs
* Developer:	jhkim
* Date:			2023-08-11 12:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Security.Cryptography;
using Com2Verse.Logger;

namespace Com2Verse.Utils
{
	public sealed class RsaEncryptor : IEncryptKey<RSACryptoServiceProvider>
	{
		private static readonly string LogCategory = "RsaEncryptor";
		private static readonly int KeySizeInBits = 2048;
		private readonly string _keyContainerName = string.Empty;

		public RsaEncryptor(string keyContainerName)
		{
			_keyContainerName = keyContainerName;
		}
#region Encrypt / Decrypt
		/// <summary>
		/// 데이터를 암호화 합니다.
		/// </summary>
		/// <param name="bytes">바이트 데이터</param>
		/// <returns>암호화된 문자열 (Base64로 인코딩된)</returns>
		public string Encrypt(byte[] bytes)
		{
			var rsa = GetOrCreate();
			var encrypted = rsa.Encrypt(bytes, RSAEncryptionPadding.Pkcs1);
			rsa.Dispose();
			return Convert.ToBase64String(encrypted);
		}

		/// <summary>
		/// 복호화를 시도합니다.
		/// </summary>
		/// <param name="base64Str">암호화된 문자열 (Base64로 인코딩된)</param>
		/// <param name="result">복호화 결과 (바이트 데이터)</param>
		/// <returns>성공 / 실패</returns>
		public bool TryDecrypt(string base64Str, out byte[] result)
		{
			byte[] bytes;
			result = Array.Empty<byte>();

			if (string.IsNullOrWhiteSpace(base64Str)) return false;

			try
			{
				bytes = Convert.FromBase64String(base64Str);
			}
			catch (Exception e)
			{
				C2VDebug.LogWarningCategory(LogCategory, $"Decrypt RSA Failed...\n{e}");
				return false;
			}

			try
			{
				var rsa = GetOrCreate();
				result = rsa.Decrypt(bytes, RSAEncryptionPadding.Pkcs1);
				rsa.Dispose();
				return result != null;
			}
			catch (CryptographicException e)
			{
				C2VDebug.LogWarningCategory(LogCategory, $"Decrypt RSA Failed....\n{e}");
				return false;
			}
		}
#endregion // Encrypt / Decrypt

#region IEncryptKey
		public RSACryptoServiceProvider GetOrCreate() => LoadKey() ?? CreateKey();
		public RSACryptoServiceProvider CreateKey()
		{
			var parameters = new CspParameters
			{
				KeyContainerName = _keyContainerName,
			};

			var rsa = new RSACryptoServiceProvider(KeySizeInBits, parameters);
			return rsa;
		}

		public RSACryptoServiceProvider RecreateKey()
		{
			DeleteKey();
			return CreateKey();
		}

		public void DeleteKey()
		{
			var parameters = new CspParameters
			{
				KeyContainerName = _keyContainerName,
			};
			var rsa = new RSACryptoServiceProvider(KeySizeInBits, parameters) {PersistKeyInCsp = false};
			rsa.Clear();
			rsa.Dispose();
		}

		public RSACryptoServiceProvider LoadKey()
		{
			var parameters = new CspParameters
			{
				Flags = CspProviderFlags.UseExistingKey,
				KeyContainerName = _keyContainerName,
			};
			try
			{
				return new RSACryptoServiceProvider(KeySizeInBits, parameters);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public bool IsKeyExist() => LoadKey() != null;
#endregion // IEncryptKey
	}
}
