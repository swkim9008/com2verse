/*===============================================================
 * Product:		Com2Verse
 * File Name:	ITrackManager.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-14 22:38
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;

namespace Com2Verse.Communication
{
	/// <summary>
	/// <see cref="eTrackType"/>의 종류에 따라 <see cref="IMediaTrack"/>을 관리하는 인터페이스.<br/>
	/// 내부적으로 <see cref="IMediaTrack"/>의 생성/소멸을 관리하며, 이를 외부로 알리는 역할을 한다.
	/// </summary>
	public interface ITrackManager<T> where T : IMediaTrack
	{
		event Action<eTrackType, T>? TrackAdded;
		event Action<eTrackType, T>? TrackRemoved;
		event Action<eTrackType, T>? TrackUpdated;

		IReadOnlyDictionary<eTrackType, T> Tracks { get; }
	}

	public interface ILocalTrackManager : ITrackManager<ILocalMediaTrack> { }

	/// <summary>
	/// <see cref="ITrackManager{T}"/> 인터페이스를 상속받는 인터페이스.<br/>
	/// 추가적으로 <see cref="IRemoteMediaTrack"/>의 <see cref="IRemoteMediaTrack.Observers"/>를 관리하는 역할을 한다.
	/// </summary>
	/// <inheritdoc cref="ITrackManager{T}"/>
	public interface IRemoteTrackManager : ITrackManager<IRemoteMediaTrack>
	{
		bool ContainsObserver(eTrackType trackType, IRemoteTrackObserver observer);
		bool TryAddObserver(eTrackType   trackType, IRemoteTrackObserver observer);
		bool RemoveObserver(eTrackType   trackType, IRemoteTrackObserver observer);
	}

	internal interface IPeerTrackManagersContainer
	{
		IPeerTrackManagers? PeerTrackManagers { get; }
	}

	internal interface IPeerTrackManagers
	{
		event Action<IPublishableRemoteUser, ILocalTrackManager> PeerAdded;
		event Action<IPublishableRemoteUser, ILocalTrackManager> PeerRemoved;

		IReadOnlyDictionary<IPublishableRemoteUser, ILocalTrackManager> PeerMap { get; }

		bool ContainsPeer(IPublishableRemoteUser target);
		bool TryAddPeer(IPublishableRemoteUser   target);
		bool RemovePeer(IPublishableRemoteUser   target);
	}
}
