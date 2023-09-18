/*===============================================================
* Product:		Com2Verse
* File Name:	Utils.cs
* Developer:	wlemon
* Date:			2023-04-20 17:27
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace Com2Verse.Mice
{
	public static partial class Utils
	{
		public static async UniTask<bool> UploadFile(string url, byte[] fileBytes, string md5, string contentType)
		{
			const string contentMD5          = "Content-MD5";
			const string formSectionName     = "File";
			const string formSectionFileName = "Default";

			var formData = new List<IMultipartFormSection>();
			formData.Add(new MultipartFormFileSection(formSectionName, fileBytes, formSectionFileName, contentType));

			var request = UnityWebRequest.Post(url, formData);
			request.SetRequestHeader(contentMD5, md5);
			
			var result = await request.SendWebRequest();
			return result.isDone;
		}

        public static void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            UnityEngine.Application.Quit();
#endif // UNITY_EDITOR
        }

        public static string GenerateUniquePath(string path)
        {
	        if (!System.IO.File.Exists(path)) return path;

	        var directory   = System.IO.Path.GetDirectoryName(path);
	        var fileName    = System.IO.Path.GetFileNameWithoutExtension(path);
	        var extension   = System.IO.Path.GetExtension(path);
	        var index       = 1;
	        var newFilePath = path;
	        while (true)
	        {
		        newFilePath = System.IO.Path.Combine(directory, $"{fileName} ({index}){extension}");
		        if (!System.IO.File.Exists(newFilePath)) break;

		        index++;
	        }

	        return newFilePath;
        }
	}
}

namespace Vuplex.WebView
{
	public static class WebHelper
	{
        public static bool TrySetAutoplayEnabled(bool enabled)
		{
			bool result = true;
			try
			{
				Web.SetAutoplayEnabled(enabled);
			}
			catch (System.Exception e) // Ignore any exceptions...
			{
				Com2Verse.Logger.C2VDebug.LogWarningCategory("Web.SetAutoplayEnabled", e.Message);
				result = false;
			}

			return result;
        }
    }
}
