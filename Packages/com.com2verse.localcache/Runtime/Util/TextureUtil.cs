/*===============================================================
* Product:		Com2Verse
* File Name:	TextureUtil.cs
* Developer:	jhkim
* Date:			2023-03-22 19:37
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.IO;
using Com2Verse.Extension;
using Com2Verse.Logger;
using UnityEngine;

namespace Com2Verse
{
	public static class TextureUtil
	{
#region Variables
		private static readonly byte[] PNGHeader = new byte[] {0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A};
		private static readonly byte[] JPGHeader = new byte[] {0xFF, 0xD8, 0xFF};
		private static readonly byte[] HeaderBuffer = new byte[Mathf.Max(PNGHeader.Length, JPGHeader.Length)];
#endregion // Variables

#region Public Methods
		public static bool CheckFileIsValidTexture(string filePath)
		{
			if (!File.Exists(filePath)) return false;

			try
			{
				var fileSize = new FileInfo(filePath).Length;
				if (fileSize < HeaderBuffer.Length) return false;

				using var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				Array.Clear(HeaderBuffer, 0, HeaderBuffer.Length);
				fs.Read(HeaderBuffer, 0, HeaderBuffer.Length);
				return IsValidTexture(HeaderBuffer);
			}
			catch (IOException) { }

			return false;
		}

		public static (int, int) GetTextureSize(string filePath)
		{
			if (!File.Exists(filePath)) return (-1, -1);

			try
			{
				var fileSize = new FileInfo(filePath).Length;
				if (fileSize < HeaderBuffer.Length) return (-1, -1);

				using var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				Array.Clear(HeaderBuffer, 0, HeaderBuffer.Length);
				fs.Read(HeaderBuffer, 0, HeaderBuffer.Length);
				fs.Seek(0, SeekOrigin.Begin);
				return GetTextureSize(HeaderBuffer, fs);
			}
			catch (IOException) { }

			return (-1, -1);
		}

		public static (int, int) GetTextureSize(byte[] bytes)
		{
			if (IsPng(bytes)) GetPngSize(bytes);
			if (IsJpg(bytes)) GetJpgSize(bytes);

			return (-1, -1);
		}
		public static bool IsValidTexture(byte[] bytes) => IsPng(bytes) || IsJpg(bytes);

		public static bool IsValidTexture(Texture2D? texture)
		{
			if (texture.IsUnityNull()) return false;

			return texture!.width > 8 && texture.height > 8;
		}

		public static int GetBitsPerPixel(TextureFormat format)
		{
			switch (format)
			{
				case TextureFormat.R8:
				case TextureFormat.BC7:
				case TextureFormat.Alpha8:
					return 8;
				case TextureFormat.RGBA4444:
				case TextureFormat.ARGB4444:
				case TextureFormat.RGB565:
					return 16;
				case TextureFormat.RGB24:
					return 24;
				case TextureFormat.RGBA32:
				case TextureFormat.ARGB32:
				case TextureFormat.BGRA32:
					return 32;
				case TextureFormat.RGBA64:
					return 64;
				default:
					return 0;
			}
		}

		public static Texture2D? ChangeFormat(this Texture2D oldTexture, TextureFormat newFormat)
		{
			var newTex = new Texture2D(oldTexture.width, oldTexture.height, newFormat, false);

			var oldPixels = oldTexture.GetPixels();
			if (oldPixels == null)
				return null;

			newTex.SetPixels(oldPixels);
			newTex.Apply();

			return newTex;
		}
#endregion // Public Methods

#region Texture Validation
		private static bool IsPng(byte[] bytes)
		{
			if (bytes.Length < PNGHeader.Length) return false;

			for (int i = 0; i < PNGHeader.Length; ++i)
				if (PNGHeader[i] != bytes[i])
					return false;

			return true;
		}

		private static bool IsJpg(byte[] bytes)
		{
			if (bytes.Length < JPGHeader.Length) return false;

			for (int i = 0; i < JPGHeader.Length; ++i)
				if (JPGHeader[i] != bytes[i])
					return false;

			return true;
		}
#endregion // Texture Validation

#region Texture Size
		private static (int, int) GetTextureSize(byte[] headerBuffer, Stream stream)
		{
			if (IsPng(headerBuffer)) return GetPngSize(stream);
			if (IsJpg(headerBuffer)) return GetJpgSize(stream);

			return (-1, -1);
		}

		private static (int, int) GetPngSize(Stream stream)
		{
			try
			{
				var sizeBytes = new byte[4];
				stream.Seek(16, SeekOrigin.Begin);

				stream.Read(sizeBytes, 0, sizeBytes.Length);
				Array.Reverse(sizeBytes);
				var width = BitConverter.ToInt32(sizeBytes, 0);

				stream.Read(sizeBytes, 0, sizeBytes.Length);
				Array.Reverse(sizeBytes);
				var height = BitConverter.ToInt32(sizeBytes, 0);

				return (width, height);
			}
			catch (IOException e)
			{
				C2VDebug.LogWarning(e);
				return (-1, -1);
			}
		}

		private static (int, int) GetPngSize(byte[] bytes)
		{
			if (bytes.Length < 24) return (-1, -1);

			var span = bytes.AsSpan();
			var widthBytes = span.Slice(16, 4);
			var heightBytes = span.Slice(20, 4);
			widthBytes.Reverse();
			heightBytes.Reverse();

			var width = BitConverter.ToInt32(widthBytes);
			var height = BitConverter.ToInt32(heightBytes);
			return (width, height);
		}

		private static (int, int) GetJpgSize(Stream stream)
		{
			var header = new byte[2];
			var sizeBytes = new byte[4];
			var sizeBuffer = new byte[2];

			stream.Read(header, 0, 2);
			if (header[0] != 0xFF || header[1] != 0xD8)
				return (-1, -1);

			try
			{
				while (stream.Position < stream.Length)
				{
					stream.Read(header, 0, header.Length);
					if (header[0] != 0xFF)
						return (-1, -1);

					var segmentId = header[1];
					if (segmentId is >= 0xC0 and <= 0xC3)
					{
						stream.Seek(3, SeekOrigin.Current);

						stream.Read(sizeBytes, 0, sizeBytes.Length);
						var height = (sizeBytes[0] << 8) | sizeBytes[1];
						var width = (sizeBytes[2] << 8) | sizeBytes[3];

						return (width, height);
					}

					stream.Read(sizeBuffer, 0, sizeBuffer.Length);
					var size = (sizeBuffer[0] << 8) | sizeBuffer[1];
					stream.Seek(size - 2, SeekOrigin.Current);
				}
			}
			catch (Exception e)
			{
				C2VDebug.LogWarning(e);
			}

			return (-1, -1);
		}

		private static (int, int) GetJpgSize(byte[] bytes)
		{
			if (bytes.Length < 2) return (-1, -1);
			if (bytes[0] != 0xFF || bytes[1] != 0xD8) return (-1, -1);

			var idx = 2;
			while (idx < bytes.Length)
			{
				if (bytes[idx] != 0xFF) return (-1, -1);

				var segmentId = bytes[idx + 1];
				idx += 2;
				if (segmentId is >= 0xC0 and <= 0xC3)
				{
					idx += 3;
					var height = (bytes[idx] << 8) | bytes[idx + 1];
					var width = (bytes[idx + 2] << 8) | bytes[idx + 3];
					return (width, height);
				}

				var size = (bytes[idx] << 8) | bytes[idx + 1];
				idx = idx + size - 2;
			}

			return (-1, -1);
		}
#endregion // Texture Size
	}
}
