/*===============================================================
* Product:		Com2Verse
* File Name:	FileHandleManager.cs
* Developer:	jhkim
* Date:			2023-08-16 19:51
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using Com2Verse.Logger;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Com2Verse.Utils
{
	public static class FileHandleManager
	{
		private static readonly Dictionary<string, FileStream> FileStreams;
		[NotNull] private static readonly HashSet<string> UsingStreams = new();
		static FileHandleManager()
		{
			FileStreams = new Dictionary<string, FileStream>();

			Application.quitting -= Dispose;
			Application.quitting += Dispose;
		}

		public static bool WriteToStream(string filePath, string data)
		{
			if (!TryLoadStream(filePath, out var stream)) return false;
			if (!(stream.CanSeek && stream.CanWrite)) return false;

			if (UsingStreams.Contains(filePath)) return false;
			UsingStreams.Add(filePath);

			try
			{
				stream.Seek(0, SeekOrigin.Begin);
				stream.SetLength(0);

				var sw = new StreamWriter(stream);
				sw.Write(data);
				sw.Flush();
			}
			finally
			{
				UsingStreams.Remove(filePath);
			}

			return true;
		}
		public static async UniTask WriteToStreamAsync(string filePath, string data)
		{
			if (!TryLoadStream(filePath, out var stream)) return;
			if (!(stream.CanSeek && stream.CanWrite)) return;

			await UniTask.WaitUntil(() => !UsingStreams.Contains(filePath));
			UsingStreams.Add(filePath);

			try
			{
				stream.Seek(0, SeekOrigin.Begin);
				stream.SetLength(0);

				var sw = new StreamWriter(stream);
				await sw.WriteAsync(data)!;
				await sw.FlushAsync()!;
			}
			finally
			{
				UsingStreams.Remove(filePath);
			}
		}

		public static string ReadFromStream(string filePath)
		{
			if (!TryLoadStream(filePath, out var stream)) return string.Empty;

			stream.Seek(0, SeekOrigin.Begin);
			var sr = new StreamReader(stream);
			return sr.ReadToEnd();
		}

		public static async UniTask<string> ReadFromStreamAsync(string filePath)
		{
			if (!TryLoadStream(filePath, out var stream)) return string.Empty;

			stream.Seek(0, SeekOrigin.Begin);

			var sr = new StreamReader(stream);
			return await sr.ReadToEndAsync();
		}
		public static async UniTask<bool> DeleteFileAsync(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath)) return true;

			await CloseStreamAsync(filePath);
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
				return true;
			}

			return false;
		}
		private static bool TryLoadStream(string filePath, out FileStream stream)
		{
			stream = null;

			if (string.IsNullOrWhiteSpace(filePath)) return false;

			CreateDir(filePath);

			var fileInfo = new FileInfo(filePath);
			try
			{
				if (FileStreams.ContainsKey(filePath))
				{
					stream = FileStreams[filePath];
					if (IsValidStream(stream))
						return true;
				}

				stream = fileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);

				FileStreams.Add(filePath, stream);
				if (IsValidStream(stream))
					return true;

				CloseStreamAsync(filePath).Forget();
			}
			catch (SecurityException e)
			{
				PrintException(e);
			}
			catch (FileNotFoundException e)
			{
				PrintException(e);
			}
			catch (UnauthorizedAccessException e)
			{
				PrintException(e);
			}
			catch (DirectoryNotFoundException e)
			{
				PrintException(e);
			}
			catch (IOException e)
			{
				PrintException(e);
			}
			catch (ArgumentNullException e)
			{
				PrintException(e);
			}
			catch (ArgumentException e)
			{
				PrintException(e);
			}

			return false;

			bool IsValidStream(Stream s) => s.CanRead && s.CanWrite;
			void PrintException(Exception e) => C2VDebug.LogWarningCategory(nameof(FileHandleManager), $"Try Load File failed...\n{e}");
		}
		private static void CloseStream(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath)) return;

			if (FileStreams.TryGetValue(filePath, out var stream))
			{
				stream.Dispose();
				FileStreams.Remove(filePath);
			}
		}
		private static async UniTask CloseStreamAsync(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath)) return;

			if (FileStreams.TryGetValue(filePath, out var stream))
			{
				await stream.DisposeAsync();
				FileStreams.Remove(filePath);
			}
		}
		private static async void Dispose()
		{
			var keys = FileStreams.Keys.ToArray();
			foreach (var key in keys)
				await CloseStreamAsync(key);

			FileStreams.Clear();
		}
		private static void CreateDir(string filePath)
		{
			var dir = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);
		}
#if UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Clear() => Dispose();
#endif // UNITY_EDITOR
	}
}
