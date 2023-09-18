/*===============================================================
 * Product:		Com2Verse
 * File Name:	IAudioSourcePipeline.cs
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
	/// 입력받은 오디오를 가공해 출력할 오디오 소스를 만들어주는 파이프라인.
	/// </summary>
	public interface IAudioSourcePipeline : IAudioSourceProvider
	{
		IAudioSourceProvider? Target { get; set; }
	}
}
