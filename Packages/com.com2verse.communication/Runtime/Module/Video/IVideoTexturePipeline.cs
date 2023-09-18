/*===============================================================
 * Product:		Com2Verse
 * File Name:	IVideoTexturePipeline.cs
 * Developer:	urun4m0r1
 * Date:		2023-06-16 16:29
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

namespace Com2Verse.Communication
{
	/// <summary>
	/// 입력받을 텍스쳐를 가공해 출력할 텍스쳐를 만들어주는 파이프라인.
	/// </summary>
	public interface IVideoTexturePipeline : IVideoTextureProvider
	{
		IVideoTextureProvider? Target { get; set; }
	}
}
