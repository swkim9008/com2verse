/*===============================================================
 * Product:		Com2Verse
 * File Name:	CommunicationSpeakerManager.cs
 * Developer:	urun4m0r1
 * Date:		2023-07-26 16:15
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.Communication;
using Com2Verse.Extension;
using Com2Verse.Utils;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	public class CommunicationSpeakerManager : Singleton<CommunicationSpeakerManager>
	{
		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly]
		private CommunicationSpeakerManager() { }

		public event Action<IReadOnlyList<IViewModelUser>>? UserListChanged;
		public event Action<IReadOnlyList<IViewModelUser>>? SpeakerListChanged;

		public IReadOnlyList<IViewModelUser> Users    => _users;
		public IReadOnlyList<IViewModelUser> Speakers => _speakers;

		private readonly List<IViewModelUser> _users    = new();
		private readonly List<IViewModelUser> _speakers = new();

		public int GetUserIndexOf(IViewModelUser?    user) => user == null ? -1 : _users.IndexOf(user);
		public int GetSpeakerIndexOf(IViewModelUser? user) => user == null ? -1 : _speakers.IndexOf(user);

		public bool TryAddUser(IViewModelUser? user)
		{
			var speechDetector = user?.Voice?.SpeechDetector;
			if (user == null || speechDetector == null)
				return false;

			if (!_users.TryAdd(user))
				return false;

			speechDetector.SpeakerTypeChanged += OnSpeakerTypeChanged(user);
			OnSpeakerTypeChanged(user)(speechDetector.SpeakerType);

			UserListChanged?.Invoke(_users);
			return true;
		}

		public bool RemoveUser(IViewModelUser? user)
		{
			var speechDetector = user?.Voice?.SpeechDetector;
			if (user == null || speechDetector == null)
				return false;

			if (!_users.TryRemove(user))
				return false;

			speechDetector.SpeakerTypeChanged -= OnSpeakerTypeChanged(user);
			OnSpeakerTypeChanged(user)(eSpeakerType.NONE);

			UserListChanged?.Invoke(_users);
			return true;
		}

		private Action<eSpeakerType> OnSpeakerTypeChanged(IViewModelUser user) => speakerType =>
		{
			if (speakerType.IsFilterMatch(eSpeakerType.SPEAKER, eFlagMatchType.CONTAINS))
			{
				if (!_speakers.TryAdd(user))
					return;

				SpeakerListChanged?.Invoke(_speakers);
			}
			else
			{
				if (!_speakers.TryRemove(user))
					return;

				SpeakerListChanged?.Invoke(_speakers);
			}
		};
	}
}
