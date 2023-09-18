/*===============================================================
* Product:		Com2Verse
* File Name:	Util.cs
* Developer:	jhkim
* Date:			2023-06-09 13:13
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.IO;
using Com2Verse.Logger;
using UnityEngine;

namespace Com2Verse.StorageApi
{
	public static class Util
	{
#region Path
		public static string GetValidStoragePath(string path)
		{
			var result = path;
			if (string.IsNullOrWhiteSpace(result)) return string.Empty;

			if (path.Length >= 1 && path[0] != '/') result = $"/{result}";
			if (path[^1] != '/') result = $"{result}/";
			return result;
		}
#endregion // Path

#region Data
		public static byte[] GetBytes(AudioClip audioClip)
		{
			if (audioClip == null) return null;

			var samples = new float[audioClip.samples];
			audioClip.GetData(samples, 0);

			var ms = new MemoryStream();

			// SavWav - https://gist.github.com/JT5D/5974837
			Int16[] intData = new Int16[samples.Length];
			var bytesData = new Byte[samples.Length * 2];
			int rescaleFactor = 32767; //to convert float to Int16

			for (int i = 0; i < samples.Length; i++)
			{
				intData[i] = (short) (samples[i] * rescaleFactor);
				var byteArr = new Byte[2];
				byteArr = BitConverter.GetBytes(intData[i]);
				byteArr.CopyTo(bytesData, i * 2);
			}

			ms.Write(bytesData, 0, bytesData.Length);
			AppendHeader(ms, audioClip);

			return ms.ToArray();
		}

		// SavWav - https://gist.github.com/JT5D/5974837
		private static void AppendHeader(Stream stream, AudioClip clip)
		{
			if (stream is not {CanSeek: true} || !stream.CanWrite) return;

			var hz = clip.frequency;
			var channels = clip.channels;
			var samples = clip.samples;

			stream.Seek(0, SeekOrigin.Begin);

			var riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
			stream.Write(riff, 0, 4);

			var chunkSize = BitConverter.GetBytes(stream.Length - 8);
			stream.Write(chunkSize, 0, 4);

			var wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
			stream.Write(wave, 0, 4);

			var fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
			stream.Write(fmt, 0, 4);

			var subChunk1 = BitConverter.GetBytes(16);
			stream.Write(subChunk1, 0, 4);

			var one = 1;

			var audioFormat = BitConverter.GetBytes(one);
			stream.Write(audioFormat, 0, 2);

			var numChannels = BitConverter.GetBytes(channels);
			stream.Write(numChannels, 0, 2);

			var sampleRate = BitConverter.GetBytes(hz);
			stream.Write(sampleRate, 0, 4);

			var byteRate = BitConverter.GetBytes(hz * channels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2
			stream.Write(byteRate, 0, 4);

			var blockAlign = (ushort) (channels * 2);
			stream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

			var bps = 16;
			var bitsPerSample = BitConverter.GetBytes(bps);
			stream.Write(bitsPerSample, 0, 2);

			var dataString = System.Text.Encoding.UTF8.GetBytes("data");
			stream.Write(dataString, 0, 4);

			var subChunk2 = BitConverter.GetBytes(samples * channels * 2);
			stream.Write(subChunk2, 0, 4);
		}
#endregion // Data

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
				C2VDebug.LogWarning($"Hash 생성 실패...\n{e}");
				return false;
			}
		}

		public static bool TryGenerateHash(byte[] bytes, out string hashStr)
		{
			hashStr = string.Empty;
			if (bytes == null)
			{
				C2VDebug.LogWarning("Hash 생성 실패");
				return false;
			}

			var hash = System.Security.Cryptography.MD5.Create().ComputeHash(bytes);
			hashStr = Convert.ToBase64String(hash);
			return true;
		}
#endregion // Hash
	}
}
