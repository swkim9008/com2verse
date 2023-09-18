/*===============================================================
 * Product:		Com2Verse
 * File Name:	ILocalUser.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-09 15:01
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;

namespace Com2Verse.Communication
{
	/// <inheritdoc cref="ICommunicationUser"/>
	/// <summary>
	/// 자기 자신을 나타내는 <see cref="ICommunicationUser"/>입니다.
	/// <br/>
	/// <br/>해당 인터페이스는 다음과 같은 하위 인터페이스를 가집니다.
	/// <list type="bullet">
	/// <item><see cref="IPublishableLocalUser"/></item>
	/// </list>
	/// </summary>
	public interface ILocalUser : ICommunicationUser { }

	/// <inheritdoc cref="ILocalUser"/>
	/// <summary>
	/// WebRTC 트랙을 Publish 할 수 있는 <see cref="ILocalUser"/>입니다.
	/// <br/>해당 인터페이스는 다음과 같은 하위 인터페이스를 가집니다.
	/// <list type="bullet">
	/// <item><see cref="IChannelLocalUser"/></item>
	/// <item><see cref="IPeerLocalUser"/></item>
	/// </list>
	/// </summary>
	public interface IPublishableLocalUser : ILocalUser, IViewModelUser
	{
		AudioPublishSettings? GetAudioPublishSettings(eTrackType trackType);
		VideoPublishSettings? GetVideoPublishSettings(eTrackType trackType);
	}

	/// <inheritdoc cref="IPublishableLocalUser"/>
	/// <summary>
	/// <see cref="ILocalMediaTrack"/>의 Publish 대상이 미디어 서버인 경우 사용하는 <see cref="IPublishableLocalUser"/>입니다.
	/// <br/>
	/// <br/><see cref="ePublishTarget"/>이 <see cref="ePublishTarget.CHANNEL"/>인 경우 사용됩니다.
	/// </summary>
	public interface IChannelLocalUser : IPublishableLocalUser
	{
		/// <summary>
		/// 미디어 서버로 전송되는 트랙들을 관리하는 매니저입니다.
		/// </summary>
		ILocalTrackManager? ChannelTrackManager { get; }
	}

	/// <inheritdoc cref="IPublishableLocalUser"/>
	/// <summary>
	/// <see cref="ILocalMediaTrack"/>의 Publish 대상이 개별 상대 Peer인 경우 사용하는 <see cref="IPublishableLocalUser"/>입니다.
	/// <br/>
	/// <br/><see cref="ePublishTarget"/>이 <see cref="ePublishTarget.PEER"/>인 경우 사용됩니다.
	/// </summary>
	public interface IPeerLocalUser : IPublishableLocalUser
	{
		event Action<IPublishableRemoteUser, ILocalTrackManager>? PeerAdded;
		event Action<IPublishableRemoteUser, ILocalTrackManager>? PeerRemoved;

		/// <summary>
		/// 피어별로 관리되는 트랙들을 관리하는 매니저의 목록입니다.
		/// </summary>
		IReadOnlyDictionary<IPublishableRemoteUser, ILocalTrackManager>? PeerMap { get; }
	}
}
