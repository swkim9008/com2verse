/*===============================================================
* Product:		Com2Verse
* File Name:	Security.cs
* Developer:	jhkim
* Date:			2022-10-20 09:46
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace Com2Verse.Utils
{
	// https://learn.microsoft.com/ko-kr/dotnet/standard/security/encrypting-data
	// https://son10001.blogspot.com/2014/12/c-rsa.html?view=flipcard
	// https://stackoverflow.com/questions/17640055/how-to-check-if-a-key-already-exists-in-container
	public class Security
	{
		public enum eDecryptStatus
		{
			SUCCESS,
			BAD_PKCS, // 암호화 키가 맞지 않음
		}
#region Variables
		private static readonly string KeyContainerName = "MetaverseSecurity";
		private static readonly string KeyCrypto = ".MetaverseSecurity.CryptoKey";
		private static readonly string KeyCryptoFilePath = $"KeyCrypto/{KeyCrypto}";
		private static readonly string KeyCryptoPath = LocalSave.PersistentGlobal.GetPath(KeyCryptoFilePath);

		[NotNull] private static Lazy<Security> _instance = new(() => new Security());
		[NotNull] public static Security Instance => _instance.Value!;
		[NotNull] private readonly AesEncryptor _aesEncryptor;
#endregion // Variables

#region Initialization
#if UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Reset()
		{
			_instance = new(() => new Security());
		}
#endif // UNITY_EDITOR

		private Security()
		{
			_aesEncryptor = new AesEncryptor(KeyCryptoPath, KeyContainerName);
		}
#endregion // Initialization

#region Public Methods
		// Sync
		public string EncryptAes(string plainData) => _aesEncryptor.Encrypt(plainData);
		public string EncryptAes(byte[] bytes) => _aesEncryptor.Encrypt(bytes);
		public DecryptResult<string> DecryptToString(string base64Encrypted) => _aesEncryptor.DecryptToString(base64Encrypted);
		public DecryptResult<byte[]> DecryptToBytes(string base64Encrypted) => _aesEncryptor.DecryptToBytes(base64Encrypted);

		// Async
		public async UniTask<string> EncryptAesAsync(string plainData) => await _aesEncryptor.EncryptAsync(plainData);
		public async UniTask<string> EncryptAesAsync(byte[] bytes) => await _aesEncryptor.EncryptAsync(bytes);
		public async UniTask<DecryptResult<string>> DecryptToStringAsync(string base64Encrypted) => await _aesEncryptor.DecryptToStringAsync(base64Encrypted);
		public async UniTask<DecryptResult<byte[]>> DecryptToBytesAsync(string base64Encrypted) => await _aesEncryptor.DecryptToBytesAsync(base64Encrypted);
#endregion // Public Methods

#region DecryptResult
		public class DecryptResult<T>
		{
			public eDecryptStatus Status;
			public T Value;

			public static DecryptResult<T> DecryptFailed { get; } = new DecryptResult<T> {Status = eDecryptStatus.BAD_PKCS, Value = default};
		}
#endregion // DecryptResult
	}
}
