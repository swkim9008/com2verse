/*===============================================================
 * Product:		Com2Verse
 * File Name:	IVideoTextureProvider.cs
 * Developer:	urun4m0r1
 * Date:		2023-06-16 15:58
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using UnityEngine;

namespace Com2Verse.Communication
{
	/// <summary>
	/// 비디오 텍스쳐를 제공하는 모듈.
	/// </summary>
	public interface IVideoTextureProvider : IModule
	{
		event Action<Texture?>? TextureChanged;

		Texture? Texture { get; }
	}
}
