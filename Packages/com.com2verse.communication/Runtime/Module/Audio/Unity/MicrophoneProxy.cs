/*===============================================================
 * Product:		Com2Verse
 * File Name:	MicrophoneProxy.cs
 * Developer:	urun4m0r1
 * Date:		2023-06-16 12:45
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.Communication.Unity
{
	public class MicrophoneProxy : Singleton<MicrophoneProxy>
	{
		private readonly List<(object, string)> _requesters = new();

		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private MicrophoneProxy() { }

		public bool IsRecording(object requester, string deviceName)
		{
			return _requesters.Contains((requester, deviceName));
		}

		public AudioClip? Start(object requester, string deviceName, bool loop, int lengthSec, int frequency)
		{
			if (_requesters.Contains((requester, deviceName)))
				throw new InvalidOperationException("Already started");

			_requesters.Add((requester, deviceName));
			return Microphone.Start(deviceName, loop, lengthSec, frequency);
		}

		public void End(object requester, string deviceName)
		{
			_requesters.Remove((requester, deviceName));

			if (_requesters.Count == 0)
				Microphone.End(deviceName);
		}
	}
}
