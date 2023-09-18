/*===============================================================
 * Product:		Com2Verse
 * File Name:	IAudioSourceProvider.cs
 * Developer:	urun4m0r1
 * Date:		2023-06-16 15:58
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Sound;

namespace Com2Verse.Communication
{
	/// <summary>
	/// 오디오 소스를 제공하는 모듈.
	/// </summary>
	public interface IAudioSourceProvider : IModule, IVolume
	{
		event Action<MetaverseAudioSource?>? AudioSourceChanged;

		MetaverseAudioSource? AudioSource { get; }
	}
}
