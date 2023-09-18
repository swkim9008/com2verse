/*===============================================================
* Product:		Com2Verse
* File Name:	Util.cs
* Developer:	jhkim
* Date:			2022-07-20 18:16
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.Organization
{
	public sealed class Util
	{
		[Obsolete("DownloadTexture(string url, Action<bool, Texture> onLoaded) 사용")]
		public static async UniTask DownloadTexture(MonoBehaviour mono, string url, Action<bool, Texture> onLoaded)
		{
			await DownloadTexture(url, onLoaded);
		}

		public static async UniTask DownloadTexture([CanBeNull] string url, [CanBeNull] Action<bool, Texture> onLoaded)
		{
			await TextureCache.Instance.GetOrDownloadTextureAsync(url, onLoaded);
		}
	}
}
