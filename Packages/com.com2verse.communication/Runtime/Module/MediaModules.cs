/*===============================================================
 * Product:		Com2Verse
 * File Name:	MediaModules.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-14 22:38
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.Extension;
using Com2Verse.Sound;
using Com2Verse.Utils;
using UnityEngine;

namespace Com2Verse.Communication
{
	/// <summary>
	/// 모듈 상태를 관리하는 클래스입니다.
	/// </summary>
	public sealed class MediaModules : IDisposable
	{
		/// <summary>
		/// 모듈의 컨텐츠가 변경되었을 때 발생하는 이벤트입니다.
		/// </summary>
		public event Action<eTrackType, bool>? ModuleContentChanged;

		/// <summary>
		/// 모듈 컨텐츠와 별개로 관리되는 트랙 접속 상태가 변경되었을 때 발생하는 이벤트입니다.
		/// </summary>
		public event Action<eTrackType, bool>? ConnectionTargetChanged;

		private readonly Func<eTrackType, Audio?>? _getAudio;
		private readonly Func<eTrackType, Video?>? _getVideo;

		public Audio? GetAudio(eTrackType trackType) => _getAudio?.Invoke(trackType);
		public Video? GetVideo(eTrackType trackType) => _getVideo?.Invoke(trackType);

		public bool IsConnectionTarget(eTrackType trackType)
		{
			var blockers = GetConnectionBlockers(trackType);
			return blockers == null || !blockers.IsAnyItemExists;
		}

		private readonly Dictionary<eTrackType, ObservableHashSet<object>> _connectionBlockers = new();

		/// <summary>
		/// 모듈 상태를 관리하는 클래스를 초기화합니다.
		/// <br/><see cref="IDisposable"/>을 구현하고 있기 때문에 반드시 <see cref="Dispose"/>를 호출해야 합니다.
		/// </summary>
		public MediaModules(Func<eTrackType, Audio?>? getAudio, Func<eTrackType, Video?>? getVideo)
		{
			_getAudio = getAudio;
			_getVideo = getVideo;

			foreach (var trackType in EnumUtility.Foreach<eTrackType>())
			{
				var audio = GetAudio(trackType);
				if (audio != null)
					audio.AudioSourceChanged += OnAudioSourceChanged(trackType);

				var video = GetVideo(trackType);
				if (video != null)
					video.TextureChanged += OnVideoTextureChanged(trackType);
			}
		}

		/// <summary>
		/// 모든 이벤트를 해제하고, 사용된 모든 리소스를 해제합니다.
		/// </summary>
		public void Dispose()
		{
			foreach (var trackType in EnumUtility.Foreach<eTrackType>())
			{
				var audio = GetAudio(trackType);
				if (audio != null)
					audio.AudioSourceChanged -= OnAudioSourceChanged(trackType);

				var video = GetVideo(trackType);
				if (video != null)
					video.TextureChanged -= OnVideoTextureChanged(trackType);
			}
		}

		private Action<MetaverseAudioSource?> OnAudioSourceChanged(eTrackType trackType)
		{
			return value => ModuleContentChanged?.Invoke(trackType, !value.IsUnityNull());
		}

		private Action<Texture?> OnVideoTextureChanged(eTrackType trackType)
		{
			return value => ModuleContentChanged?.Invoke(trackType, !value.IsUnityNull());
		}

		/// <summary>
		/// 해당 타입의 모듈이 활성화되어 있는지 확인합니다.
		/// </summary>
		public bool IsModuleContentAvailable(eTrackType trackType)
		{
			var audio = GetAudio(trackType);
			if (audio != null)
				return !audio.AudioSource.IsUnityNull();

			var video = GetVideo(trackType);
			if (video != null)
				return !video.Texture.IsUnityNull();

			return false;
		}

		private ObservableHashSet<object> GetOrCreateConnectionBlockers(eTrackType trackType)
		{
			var blockers = GetConnectionBlockers(trackType);
			if (blockers == null)
			{
				blockers = new ObservableHashSet<object>();
				_connectionBlockers.Add(trackType, blockers);
				blockers.ItemExistenceChanged += OnConnectionBlockingStateChanged(trackType);
			}

			return blockers;
		}

		private Action<bool> OnConnectionBlockingStateChanged(eTrackType trackType)
		{
			return value => ConnectionTargetChanged?.Invoke(trackType, !value);
		}

		private ObservableHashSet<object>? GetConnectionBlockers(eTrackType trackType)
		{
			return _connectionBlockers.TryGetValue(trackType, out var blockers) ? blockers : null;
		}

		public bool ContainsConnectionBlocker(eTrackType trackType, object blocker)
		{
			return GetConnectionBlockers(trackType)?.Contains(blocker) ?? false;
		}

		public bool TryAddConnectionBlocker(eTrackType trackType, object blocker)
		{
			return GetOrCreateConnectionBlockers(trackType).TryAdd(blocker);
		}

		public bool RemoveConnectionBlocker(eTrackType trackType, object blocker)
		{
			return GetConnectionBlockers(trackType)?.Remove(blocker) ?? false;
		}
	}
}
