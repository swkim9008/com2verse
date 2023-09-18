/*===============================================================
* Product:		Com2Verse
* File Name:	ScreenCaptureManager.cs
* Developer:	urun4m0r1
* Date:			2022-10-17 17:50
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.Solution.ScreenCapture;
using JetBrains.Annotations;
using RemoteUser = System.Object;

namespace Com2Verse.ScreenShare
{
	public class ScreenCaptureManager : Singleton<ScreenCaptureManager>, IDisposable
	{
		public event Action<eScreenShareSignal>? ScreenShareSignalChanged;

		public IReadOnlyList<RemoteUser> RemoteSharingUsers => _remoteSharingUsers;

		public ScreenCaptureController Controller { get; }

		private readonly MgrScreenCapture _screenCaptureManager = new();
		private readonly List<RemoteUser> _remoteSharingUsers   = new();

		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private ScreenCaptureManager()
		{
			Controller = new ScreenCaptureController(_screenCaptureManager);
		}

		public void Dispose()
		{
			Controller.Dispose();
			_screenCaptureManager.Dispose();
		}

		public void AddRemoteUser(RemoteUser user)
		{
			if (_remoteSharingUsers.Contains(user))
				return;

			_remoteSharingUsers.Add(user);

			if (Controller.IsCaptureRequestedOrCapturing)
				SendSignal(eScreenShareSignal.CAPTURE_STOPPED_BY_REMOTE);

			SendSignal(eScreenShareSignal.RECEIVE_STARTED_BY_REMOTE);
		}

		public void RemoveRemoteUser(RemoteUser user)
		{
			if (!_remoteSharingUsers.Contains(user))
				return;

			_remoteSharingUsers.Remove(user);

			SendSignal(eScreenShareSignal.RECEIVE_STOPPED_BY_REMOTE);
		}

		internal void SendSignal(eScreenShareSignal type)
		{
			ScreenShareSignalChanged?.Invoke(type);
		}
	}
}
