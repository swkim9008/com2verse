/*===============================================================
* Product:		Com2Verse
* File Name:	IAudioDevice.cs
* Developer:	urun4m0r1
* Date:			2022-04-04 10:31
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

namespace Com2Verse.Communication
{
	/// <inheritdoc />
	/// <summary>
	/// Interface for audio device such as microphone, speaker, etc.
	/// </summary>
	public interface IAudioDevice : IDevice
	{
		/// <summary>
		/// Get system default audio device.
		/// You are not allowed to change this value.
		/// </summary>
		DeviceInfo SystemDefault { get; }

		/// <summary>
		/// Get the current system audio device's <see cref="SystemVolume"/>.
		/// </summary>
		SystemVolume SystemVolume { get; }
	}
}
