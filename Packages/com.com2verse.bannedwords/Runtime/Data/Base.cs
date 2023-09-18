/*===============================================================
* Product:		Com2Verse
* File Name:	Base.cs
* Developer:	jhkim
* Date:			2023-03-10 18:30
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Text;
using Com2Verse.LocalCache;
using Cysharp.Threading.Tasks;

namespace Com2Verse.BannedWords
{
	[Serializable]
	public struct AppDefine
	{
		public string AppId;
		public string Game;
		public string Revision;
		public bool IsStaging;

		private static AppDefine _default = new AppDefine
		{
			AppId = "com.com2us.aaa",
			Game = "default",
			Revision = "15",
			IsStaging = false,
		};

		public static AppDefine Default => _default;
	}

	internal abstract class Base
	{
		internal virtual string MakeFileName(AppDefine appDefine) => string.Empty;

#region Cache
		internal async UniTask<string> LoadCacheAsync(AppDefine appDefine)
		{
			if (!IsCached(appDefine)) return string.Empty;

			var fileName = MakeFileName(appDefine);
			Cache.SetCacheType(eCacheType.DEFAULT);
			using var pool = await Cache.LoadBytesAsync(fileName);
			var data = Encoding.UTF8.GetString(pool.Data);

			if (Cache.UseEncryption)
				return data;

			data = data.Trim('\0');
			var jsonBytes = Convert.FromBase64String(data);
			return Encoding.UTF8.GetString(jsonBytes);
		}

		internal async UniTask SaveCacheAsync(AppDefine appDefine, string text)
		{
			var fileName = MakeFileName(appDefine);
			var bytes = Encoding.UTF8.GetBytes(text);
			Cache.SetCacheType(eCacheType.DEFAULT);
			await Cache.SaveBytesAsync(fileName, bytes);
		}

		internal bool IsCached(AppDefine appDefine)
		{
			var fileName = MakeFileName(appDefine);
			Cache.SetCacheType(eCacheType.DEFAULT);
			return Cache.IsExist(fileName);
		}

		internal void RemoveCache(AppDefine appDefine)
		{
			var fileName = MakeFileName(appDefine);
			Cache.SetCacheType(eCacheType.DEFAULT);
			Cache.PurgeCache(fileName);
		}
#endregion // Cache

#region Util
		internal bool IsValidAppDefine(AppDefine appDefine) => !string.IsNullOrWhiteSpace(appDefine.AppId) && !string.IsNullOrWhiteSpace(appDefine.Game) && !string.IsNullOrWhiteSpace(appDefine.Revision);
#endregion // Util
	}
}
