/*===============================================================
* Product:		Com2Verse
* File Name:	MiceExtensions_TextureCache.cs
* Developer:	sprite
* Date:			2023-04-20 21:05
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using Com2Verse.Utils;
using System;
using Cysharp.Threading.Tasks;

namespace Com2Verse.Mice
{
    public static partial class TextureExtensions
    {
        /// <summary>
        /// 텍스쳐를 URL로부터 로딩하거나 캐싱된 텍스쳐를 반환한다.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="url"></param>
        /// <param name="defaultAsset"></param>
        /// <param name="completed"></param>
        /// <returns></returns>
        public static Texture GetOrDownloadTexture(this Texture texture, string url, Action<Texture> onCompleted = null)
        {
            TextureCache.Instance
                .GetOrDownloadTextureAsync(url)
                .ContinueWith(tex => { texture = tex; onCompleted?.Invoke(tex); })
                .Forget();

            return texture;
        }
    }
}
