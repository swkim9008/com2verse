/*===============================================================
* Product:		Com2Verse
* File Name:	AssetSystemUtility.cs
* Developer:	tlghks1009
* Date:			2023-02-09 18:02
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using Com2Verse.Logger;

namespace Com2VerseEditor.AssetSystem
{
	public static class AssetExternalDatabase
	{
		public static (string[], string[]) FindAssets(string path)
		{
			try
			{
				var guids = new List<string>();
				var assetNames = new List<string>();
				var filePaths = Directory.GetFiles(path);

				foreach (var filePath in filePaths)
				{
					if (filePath.EndsWith(".meta", StringComparison.Ordinal))
					{
						foreach (var line in File.ReadLines(filePath))
						{
							if (line.StartsWith("guid", StringComparison.Ordinal))
							{
								var split = line.Split(' ');
								var guid = split[1];

								guids.Add(guid);
								assetNames.Add(Path.GetFileNameWithoutExtension(filePath));
								break;
							}
						}
					}
				}

				return (guids.ToArray(), assetNames.ToArray());
			}
			catch (Exception e)
			{
				C2VDebug.LogError($"[AssetExternalDatabase] {e.Message}");
				return (new string[] { }, new string[] { });
			}
		}


		public static string FindDirectory(string path, string search)
		{
			try
			{
				foreach (var directory in Directory.GetDirectories(path!))
				{
					if (directory.Contains(search))
					{
						return directory;
					}
				}

				return string.Empty;
			}
			catch (Exception e)
			{
				C2VDebug.LogError($"[AssetExternalDatabase] {e.Message}");
				return string.Empty;
			}
		}
	}
}
