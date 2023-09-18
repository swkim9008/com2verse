/*===============================================================
* Product:		Com2Verse
* File Name:	DeviceManager.cs
* Developer:	urun4m0r1
* Date:			2022-08-05 16:07
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using JetBrains.Annotations;

namespace Com2Verse.Communication.Unity
{
	public sealed class DeviceManager : BaseDeviceManager<DeviceManager, AudioRecorderDevice, AudioPlayerDevice, VideoRecorderDevice>, IDisposable
	{
		public override AudioRecorderDevice AudioRecorder { get; }
		public override AudioPlayerDevice   AudioPlayer   { get; }
		public override VideoRecorderDevice VideoRecorder { get; }

		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private DeviceManager()
		{
			AudioRecorder = new AudioRecorderDevice();
			AudioPlayer   = new AudioPlayerDevice();
			VideoRecorder = new VideoRecorderDevice();

			RegisterDeviceSaveEvent();
		}

		public void Dispose()
		{
			AudioRecorder.Dispose();
			AudioPlayer.Dispose();
			VideoRecorder.Dispose();
		}
	}
}
