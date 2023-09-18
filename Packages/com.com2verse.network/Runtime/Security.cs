/*===============================================================
* Product:		Com2Verse
* File Name:	Security.cs
* Developer:	haminjeong
* Date:			2023-08-16 13:23
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Security.Cryptography;

namespace Com2Verse.Network
{
	public sealed class Security
	{
#region AES
		private static Aes  _aes;
		private static Aes  _aesCSU;
		public static  bool IsAESInitialized => _aesCSU != null;
		public static  bool IsActivatedEncrypt   = false;
		public static  bool IsActivatedIfEncrypt = false;
		
#if ENABLE_CHEATING
#if UNITY_EDITOR
		[UnityEditor.MenuItem("Com2Verse/Security/Print AES keys")]
#endif
		public static void ShowKeys()
		{
			UnityEngine.Debug.Log($"AES1 Key: {Convert.ToBase64String(_aes.Key)}\nIV: {Convert.ToBase64String(_aes.IV)}");
			UnityEngine.Debug.Log($"AES2 Key: {Convert.ToBase64String(_aesCSU.Key)}\nIV: {Convert.ToBase64String(_aesCSU.IV)}");
		}
#endif
		
		public struct AESKeyInfo
		{
			public byte[] Key;
			public byte[] IV;
		}

		public static AESKeyInfo CreateNewKeyAndIV()
		{
			_aes = Aes.Create();
			var currentKeyInfo = new AESKeyInfo
			{
				Key = _aes.Key,
				IV  = _aes.IV,
			};
			return currentKeyInfo;
		}

		public static void SetDecryptorCSU(byte[] key, byte[] iv)
		{
			_aesCSU     = Aes.Create();
			_aesCSU.Key = key;
			_aesCSU.IV  = iv;
		}

		public static void ClearTransforms()
		{
			_aes?.Clear();
			_aes = null;
			_aesCSU?.Clear();
			_aesCSU              = null;
			IsActivatedEncrypt   = false;
			IsActivatedIfEncrypt = false;
		}

		public static LogicPacket.IBuffer EncryptByAES(LogicPacket.IBuffer message)
		{
			var    encryptTransform = _aes!.CreateEncryptor();
			byte[] cipherBytes      = encryptTransform.TransformFinalBlock(message.Array, message.BodyOffset, message.BodyCount);
			var    ret              = LogicPacket.MakeVariableHeaderPacket(LogicPacket.ClientMessageType.Normal, cipherBytes.Length, out var _);
			Buffer.BlockCopy(cipherBytes, 0, ret.Array, ret.BodyOffset, cipherBytes.Length);
			return ret;
		}

		public static LogicPacket.IBuffer DecryptByAES(ArraySegment<byte> cipherBytes)
		{
			var    decryptTransform = _aes!.CreateDecryptor();
			byte[] message          = decryptTransform.TransformFinalBlock(cipherBytes.Array, cipherBytes.Offset, cipherBytes.Count);
			var    ret              = LogicPacket.MakeVariableHeaderPacket(LogicPacket.ClientMessageType.Normal, message.Length, out var _);
			Buffer.BlockCopy(message, 0, ret.Array, ret.BodyOffset, message.Length);
			return ret;
		}
		
		public static LogicPacket.IBuffer DecryptCSUByAES(ArraySegment<byte> cipherBytes)
		{
			var    decryptTransform = _aesCSU!.CreateDecryptor();
			byte[] message          = decryptTransform.TransformFinalBlock(cipherBytes.Array, cipherBytes.Offset, cipherBytes.Count);
			var    ret              = LogicPacket.MakeVariableHeaderPacket(LogicPacket.ClientMessageType.Simple, message.Length, out var _);
			Buffer.BlockCopy(message, 0, ret.Array, ret.BodyOffset, message.Length);
			return ret;
		}
#endregion AES

#region RSA
		private static readonly string PublicKey =
	"<RSAKeyValue><Modulus>2fFGV8I2uLv1I3w4IejScYTBNBfyC6Lgq7SPFWm7Noh8AdcNqnihz9gm9vHs07wFGEgV6DegtzFTg4M+YWV1hkLxbbT917ibHjINwW3/Y2xHyAIfE7sHIg4UhNNyhVUlBA6bvB/9Pm2JeeYUMi8GM6TYTTknoEbniDz69OoSvv0=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
		private static          RSACryptoServiceProvider _rsa;

		private static void CreateNewProvider()
		{
			_rsa = new RSACryptoServiceProvider();
			_rsa.FromXmlString(PublicKey);
		}

		// RSA 암호화
		public static LogicPacket.IBuffer RSAEncrypt(LogicPacket.IBuffer inbuf)
		{
			if (_rsa == null)
				CreateNewProvider();
			ArraySegment<byte> body          = new ArraySegment<byte>(inbuf.Array, inbuf.BodyOffset, inbuf.BodyCount);
			byte[]             encryptBuffer = new byte[inbuf.Count * 2];
			_rsa!.TryEncrypt(body, encryptBuffer, RSAEncryptionPadding.Pkcs1, out var bufferUsed);
			var ret = LogicPacket.MakeVariableHeaderPacket(LogicPacket.ClientMessageType.Normal, bufferUsed, out var _);
			Buffer.BlockCopy(encryptBuffer, 0, ret.Array, ret.BodyOffset, bufferUsed);
			return ret;
		}
#endregion RSA
	}
}
