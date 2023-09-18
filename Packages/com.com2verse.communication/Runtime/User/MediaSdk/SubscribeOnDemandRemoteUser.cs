/*===============================================================
 * Product:		Com2Verse
 * File Name:	SubscribeOnlyRemoteUser.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-20 18:28
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MediaSdkUser = Com2Verse.Solution.UnityRTCSdk.User;

namespace Com2Verse.Communication.MediaSdk
{
	/// <inheritdoc cref="IPublishRequestableRemoteUser"/>
	/// <summary>
	/// <see cref="SubscribeOnlyRemoteUser"/>의 <see cref="IPublishRequestableRemoteUser"/> 구현체입니다.
	/// <br/>
	/// <br/>즉, 원격 Peer를 대상으로 <see cref="IRemoteMediaTrack"/>의 Publish를 요청 가능하며,
	/// <br/>추가된 트랙의 Subscribe 제어가 가능한 <see cref="IRemoteUser"/>입니다.
	/// </summary>
	internal class SubscribeOnDemandRemoteUser : SubscribeOnlyRemoteUser, IPublishRequestableRemoteUser
	{
		private readonly Dictionary<eTrackType, RemoteTrackPublishRequestSender> _requestSenders = new();

		private RemoteTrackPublishRequestSenderFactory? _requestSenderFactory;

		public SubscribeOnDemandRemoteUser(ChannelInfo channelInfo, User user, eUserRole role, MediaSdkUser mediaSdkUser)
			: base(channelInfo, user, role, mediaSdkUser) { }

		/// <summary>
		/// <see cref="RemoteTrackPublishRequestSenderFactory"/>를 할당합니다.
		/// <br/><see cref="CommunicationUser.Dispose"/>시 팩토리에서 생성된 모든 <see cref="RemoteTrackPublishRequestSender"/>의 자원을 해제합니다.
		/// </summary>
		public void AssignRequestSenderFactory(RemoteTrackPublishRequestSenderFactory requestSenderFactory)
		{
			if (_requestSenderFactory != null)
				throw new InvalidOperationException("RequestSenderFactory is already assigned.");

			_requestSenderFactory = requestSenderFactory;
		}

		public bool ContainsPublishRequest(IRemoteTrackPublishRequester requester, eTrackType trackType)
		{
			if (!TryGetRequestSender(trackType, out var requestSender))
				return false;

			return requestSender.RequestHandler.IsRegistered(requester);
		}

		public bool TryAddPublishRequest(IRemoteTrackPublishRequester requester, eTrackType trackType)
		{
			if (!TryGetRequestSender(trackType, out var requestSender))
			{
				if (_requestSenderFactory == null)
					return false;

				requestSender = _requestSenderFactory.Create(trackType);
				_requestSenders.Add(trackType, requestSender);
			}

			return requestSender.RequestHandler.Register(requester);
		}

		public bool RemovePublishRequest(IRemoteTrackPublishRequester requester, eTrackType trackType)
		{
			if (!TryGetRequestSender(trackType, out var requestSender))
				return false;

			return requestSender.RequestHandler.Unregister(requester);
		}

		private bool TryGetRequestSender(eTrackType trackType, [NotNullWhen(true)] out RemoteTrackPublishRequestSender? requestSender)
		{
			return _requestSenders.TryGetValue(trackType, out requestSender);
		}

#region IDisposable
		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				foreach (var requestSender in _requestSenders.Values)
				{
					requestSender.Dispose();
					requestSender.RequestHandler.Dispose();
				}

				_requestSenders.Clear();
			}

			base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable
	}
}
