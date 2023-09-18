/*===============================================================
 * Product:		Com2Verse
 * File Name:	DefaultRemoteTrackPublishRequestHandler.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-14 22:38
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Utils;

namespace Com2Verse.Communication
{
	/// <inheritdoc />
	/// <summary>
	/// 등록된 <see cref="IRemoteTrackPublishRequester"/>가 존재하는지 여부를 검사해 <see cref="Requested"/> 이벤트를 발생시키는 클래스
	/// </summary>
	public sealed class DefaultRemoteTrackPublishRequestHandler : IRemoteTrackPublishRequestHandler
	{
		public event Action<bool>? Requested;

		private readonly ObservableHashSet<IRemoteTrackPublishRequester> _requesters = new();

		public DefaultRemoteTrackPublishRequestHandler()
		{
			_requesters.ItemExistenceChanged += OnItemExistenceChanged;
		}

		private void OnItemExistenceChanged(bool isAnyItemExists)
		{
			Requested?.Invoke(isAnyItemExists);
		}

		public void Dispose()
		{
			if (IsRequested)
				Requested?.Invoke(false);
		}

		public bool IsRequested => _requesters.IsAnyItemExists;

		public bool IsRegistered(IRemoteTrackPublishRequester requester) => _requesters.Contains(requester);
		public bool Register(IRemoteTrackPublishRequester     requester) => _requesters.TryAdd(requester);
		public bool Unregister(IRemoteTrackPublishRequester   requester) => _requesters.Remove(requester);

		public void Reset() => _requesters.Clear();
	}
}
