/*===============================================================
* Product:		Com2Verse
* File Name:	AesEncryptor.cs
* Developer:	jhkim
* Date:			2023-08-11 12:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.IO;
using System.Security.Cryptography;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Com2Verse.Utils
{
	public sealed class AesEncryptor : IEncryptKey<Aes>
	{
		private static readonly string LogCategory = "AesEncryptor";
		private static readonly int BufferSize = 204800; // 주의: 버퍼 크기가 너무 작으면 복호화 성능이 저하됨

		[NotNull] private readonly string _keyPath;
		[NotNull] private readonly RsaEncryptor _rsaEncryptor;

		public AesEncryptor(string keyPath, string rsaContainerName)
		{
			_keyPath = keyPath ?? string.Empty;
			_rsaEncryptor = new RsaEncryptor(rsaContainerName);
		}

#region Encrypt
#region Sync
		/// <summary>
		/// 데이터를 암호화 합니다.
		/// </summary>
		/// <param name="plainData">평문</param>
		/// <returns>암호화된 문자열 (Base64로 인코딩된)</returns>
		public string Encrypt(string plainData)
		{
			return EncryptInternal(OnEncrypt);

			void OnEncrypt(CryptoStream cs)
			{
				if (cs == null) return;

				using var sw = new StreamWriter(cs);
				sw.Write(plainData);
			}
		}

		/// <summary>
		/// 데이터를 암호화 합니다.
		/// </summary>
		/// <param name="bytes">바이트 데이터</param>
		/// <returns>암호화된 문자열 (Base64로 인코딩된)</returns>
		public string Encrypt(byte[] bytes)
		{
			return EncryptInternal(OnEncrypt);

			void OnEncrypt(CryptoStream cs) => cs?.Write(bytes);
		}

		private string EncryptInternal([NotNull] Action<CryptoStream> onEncrypt)
		{
			var aes = GetOrCreate();

			using var encryptor = aes?.CreateEncryptor(aes.Key, aes.IV);

			using var ms = new MemoryStream();
			using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
				onEncrypt.Invoke(cs);

			var encrypted = Convert.ToBase64String(ms.ToArray());
			return encrypted;
		}
#endregion // Sync

#region Async
		/// <summary>
		/// 데이터를 암호화 합니다. (비동기)
		/// </summary>
		/// <param name="plainData">평문</param>
		/// <returns>암호화된 문자열 (Base64로 인코딩된)</returns>
		public async UniTask<string> EncryptAsync(string plainData)
		{
			return await EncryptInternalAsync(OnEncrypt);

			async UniTask OnEncrypt(CryptoStream cs)
			{
				if (cs == null) return;

				await using var sw = new StreamWriter(cs);

				await sw.WriteAsync(plainData)!;
			}
		}

		/// <summary>
		/// 데이터를 암호화 합니다. (비동기)
		/// </summary>
		/// <param name="bytes">바이트 데이터</param>
		/// <returns>암호화된 문자열 (Base64로 인코딩된)</returns>
		public async UniTask<string> EncryptAsync(byte[] bytes)
		{
			return await EncryptInternalAsync(OnEncrypt);

			async UniTask OnEncrypt(CryptoStream cs)
			{
				if (cs == null) return;

				await cs.WriteAsync(bytes);
			}
		}

		private async UniTask<string> EncryptInternalAsync([NotNull] Func<CryptoStream, UniTask> onEncrypt)
		{
			var aes = GetOrCreate();

			using var encryptor = aes?.CreateEncryptor(aes.Key, aes.IV);
			using var ms = new MemoryStream();
			await using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
				await onEncrypt.Invoke(cs);

			var encrypted = Convert.ToBase64String(ms.ToArray());
			return encrypted;
		}
#endregion // Async
#endregion // Encrypt

#region Decrypt
#region Sync
		/// <summary>
		/// 데이터를 복호화 합니다.
		/// </summary>
		/// <param name="base64Encrypted">암호화된 문자열 (Base64로 인코딩된)</param>
		/// <returns>복호화된 문자열</returns>
		public Security.DecryptResult<string> DecryptToString(string base64Encrypted)
		{
			return DecryptInternal(base64Encrypted, OnDecrypt);

			Security.DecryptResult<string> OnDecrypt([NotNull] CryptoStream cs)
			{
				try
				{
					using var sr = new StreamReader(cs);
					var value = sr.ReadToEnd();

					var result = new Security.DecryptResult<string>
					{
						Status = Security.eDecryptStatus.SUCCESS,
						Value = value,
					};
					return result;
				}
				catch (CryptographicException e)
				{
					C2VDebug.LogWarningCategory(LogCategory, $"Decrypt AES Failed...\n{e}");
					return Security.DecryptResult<string>.DecryptFailed;
				}
			}
		}

		/// <summary>
		/// 데이터를 복호화 합니다.
		/// </summary>
		/// <param name="base64Encrypted">암호화된 문자열 (Base64로 인코딩된)</param>
		/// <returns>복호화된 바이트 데이터</returns>
		public Security.DecryptResult<byte[]> DecryptToBytes(string base64Encrypted)
		{
			return DecryptInternal(base64Encrypted, OnDecrypt);

			Security.DecryptResult<byte[]> OnDecrypt(CryptoStream cs)
			{
				var buffer = new byte[BufferSize];
				var writeStream = new MemoryStream();
				int read;
				while (true)
				{
					try
					{
						if (cs == null)
							return Security.DecryptResult<byte[]>.DecryptFailed;

						read = cs.Read(buffer, 0, buffer.Length);
						if (read == 0)
							break;

						writeStream.Write(buffer, 0, read);
					}
					catch (CryptographicException e)
					{
						C2VDebug.LogWarning(e);
						return Security.DecryptResult<byte[]>.DecryptFailed;
					}
				}

				var value = writeStream.ToArray();
				var result = new Security.DecryptResult<byte[]>
				{
					Status = Security.eDecryptStatus.SUCCESS,
					Value = value,
				};
				return result;
			}
		}

		private Security.DecryptResult<T> DecryptInternal<T>(string base64Encrypted, [NotNull] Func<CryptoStream, Security.DecryptResult<T>> onDecrypt)
		{
			if (string.IsNullOrWhiteSpace(base64Encrypted)) 
				return Security.DecryptResult<T>.DecryptFailed;

			var aes = GetOrCreate();
			var bytes = Convert.FromBase64String(base64Encrypted);
			using var decryptor = aes?.CreateDecryptor(aes.Key, aes.IV);
			using var ms = new MemoryStream(bytes);
			using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
			return onDecrypt(cs);
		}
#endregion // Sync

#region Async
		/// <summary>
		/// 데이터를 복호화 합니다. (비동기)
		/// </summary>
		/// <param name="base64Encrypted">암호화된 문자열 (Base64로 인코딩된)</param>
		/// <returns>복호화된 문자열</returns>
		public async UniTask<Security.DecryptResult<string>> DecryptToStringAsync(string base64Encrypted)
		{
			return await DecryptInternalAsync(base64Encrypted, OnDecrypt);

			async UniTask<Security.DecryptResult<string>> OnDecrypt([NotNull] CryptoStream cs)
			{
				try
				{
					using var sr = new StreamReader(cs);
					var value = await sr.ReadToEndAsync()!;
					var result = new Security.DecryptResult<string>
					{
						Status = Security.eDecryptStatus.SUCCESS,
						Value = value,
					};
					return result;
				}
				catch (CryptographicException e)
				{
					C2VDebug.LogWarningCategory(LogCategory, $"Decrypt AES Failed...\n{e}");
					return Security.DecryptResult<string>.DecryptFailed;
				}
			}
		}

		/// <summary>
		/// 데이터를 복호화 합니다. (비동기)
		/// </summary>
		/// <param name="base64Encrypted">암호화된 문자열 (Base64로 인코딩된)</param>
		/// <returns>복호화된 바이트 데이터</returns>
		public async UniTask<Security.DecryptResult<byte[]>> DecryptToBytesAsync(string base64Encrypted)
		{
			return await DecryptInternalAsync(base64Encrypted, OnDecrypt);

			async UniTask<Security.DecryptResult<byte[]>> OnDecrypt([NotNull] CryptoStream cs)
			{
				var buffer = new byte[BufferSize];
				var writeStream = new MemoryStream();
				int read;
				while (true)
				{
					try
					{
						read = await cs.ReadAsync(buffer, 0, buffer.Length)!;
						if (read == 0)
							break;

						await writeStream.WriteAsync(buffer, 0, read)!;
					}
					catch (CryptographicException e)
					{
						C2VDebug.LogWarning(e);
						return Security.DecryptResult<byte[]>.DecryptFailed;
					}
				}

				var value = writeStream.ToArray();
				var result = new Security.DecryptResult<byte[]>
				{
					Status = Security.eDecryptStatus.SUCCESS,
					Value = value,
				};
				return result;
			}
		}
		private async UniTask<Security.DecryptResult<T>> DecryptInternalAsync<T>(string base64Encrypted, [NotNull] Func<CryptoStream, UniTask<Security.DecryptResult<T>>> onDecrypt)
		{
			if (string.IsNullOrWhiteSpace(base64Encrypted))
				return Security.DecryptResult<T>.DecryptFailed;

			var aes = GetOrCreate();
			var bytes = Convert.FromBase64String(base64Encrypted);
			using var decryptor = aes?.CreateDecryptor(aes.Key, aes.IV);
			using var ms = new MemoryStream(bytes);
			await using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
			return await onDecrypt(cs);
		}
#endregion // Async
#endregion // Decrypt

#region IEncryptKey
		public Aes GetOrCreate() => LoadKey() ?? CreateKey();
		public Aes CreateKey()
		{
			var key = Aes.Create();
			SaveKey(AesKeyInfo.Create(key));
			return key;
		}

		public Aes RecreateKey()
		{
			DeleteKey();
			return CreateKey();
		}
		public void DeleteKey()
		{
			if (IsKeyExist())
				File.Delete(_keyPath);
		}

		public Aes LoadKey()
		{
			if (IsKeyExist())
			{
				Aes aes;
				var base64Encrypted = File.ReadAllText(_keyPath);
				if (_rsaEncryptor.TryDecrypt(base64Encrypted, out var bytes))
				{
					try
					{
						var json = System.Text.Encoding.UTF8.GetString(bytes!);
						var keyInfo = JsonConvert.DeserializeObject<AesKeyInfo>(json)!;

						aes = Aes.Create();
						aes.Key = keyInfo.Key!;
						aes.IV = keyInfo.IV!;
					}
					catch (Exception e)
					{
						C2VDebug.LogWarningCategory(LogCategory, $"Load Key Failed...\n{e}");
						aes = LoadKeyFallback();
					}
				}
				else
				{
					return LoadKeyFallback();
				}
				return aes;
			}

			return null;

			Aes LoadKeyFallback()
			{
				_rsaEncryptor.RecreateKey();
				return RecreateKey();
			}
		}
		public bool IsKeyExist() => !string.IsNullOrWhiteSpace(_keyPath) && File.Exists(_keyPath);
#endregion // IEncryptKey

#region Key
		private void SaveKey(AesKeyInfo keyInfo)
		{
			CreateKeyDir();

			var json = JsonConvert.SerializeObject(keyInfo);
			var base64Encrypted = _rsaEncryptor.Encrypt(System.Text.Encoding.UTF8.GetBytes(json));
			File.WriteAllText(_keyPath, base64Encrypted);
		}
		private void CreateKeyDir()
		{
			if (string.IsNullOrWhiteSpace(_keyPath)) return;

			var dir = Path.GetDirectoryName(_keyPath);
			if (string.IsNullOrWhiteSpace(dir)) return;

			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);
		}

		[Serializable]
		private class AesKeyInfo
		{
			public byte[] Key;
			public byte[] IV;

			public static AesKeyInfo Create(Aes aes) =>
				new()
				{
					Key = aes?.Key,
					IV = aes?.IV,
				};
		}
#endregion // Key

#region Debug
		public void DebugRevokeKey()
		{
			_rsaEncryptor.RecreateKey();
			RecreateKey();
		}
#endregion // Debug
	}
}
