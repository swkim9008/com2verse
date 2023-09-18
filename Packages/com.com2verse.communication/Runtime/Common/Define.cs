/*===============================================================
 * Product:		Com2Verse
 * File Name:	Define.cs
 * Developer:	urun4m0r1
 * Date:		2023-01-04 18:34
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using UnityEngine.Experimental.Rendering;

namespace Com2Verse.Communication
{
	internal static class Define
	{
		public static readonly int DefaultTableIndex = 1;

		public static GraphicsFormat WebRtcSupportedGraphicsFormat => GraphicsFormat.B8G8R8A8_SRGB;

		public static readonly int PublishDelay = 500;

		/// <summary>
		/// <see cref="IRemoteUser"/>가 생성되기 전에 <see cref="IRemoteMediaTrack"/> 관련 이벤트가 발생하는 경우 유저 생성까지 대기하는 시간.
		/// </summary>
		public static readonly TimeSpan RemoteUserCreationAwaitTimeout = TimeSpan.FromSeconds(5);

		/// <summary>
		/// Failed to present D3D11 swapchain due to device removed. 에러 발생 방지를 위해 RenderTexture 재생성을 지연시키는 시간
		/// </summary>
		public static readonly int RenderTextureReCreateDelay = 1000;

		public static class Device
		{
			public static readonly int RefreshInterval = 1000;
		}

		public static class Audio
		{
			public static readonly int DefaultLoopbackDelay = 1000;
			public static readonly int DelayTolerance       = 100;
		}

		public static class VoiceDetection
		{
			public static readonly int UpdateInterval = 100;
			public static readonly int SampleLength   = 256;
		}
	}
}
