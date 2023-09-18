/*===============================================================
 * Product:		Com2Verse
 * File Name:	IRemoteUser.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-09 15:01
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using MediaSdkUser = Com2Verse.Solution.UnityRTCSdk.User;

namespace Com2Verse.Communication
{
	/// <inheritdoc cref="ICommunicationUser"/>
	/// <summary>
	/// 원격 사용자를 나타내는 <see cref="ICommunicationUser"/>입니다.
	/// <br/>
	/// <br/>해당 인터페이스는 다음과 같은 하위 인터페이스를 가집니다.
	/// <list type="bullet">
	/// <item><see cref="IPublishableRemoteUser"/></item>
	/// <item><see cref="ISubscribableRemoteUser"/></item>
	/// <item><see cref="IPublishRequestableRemoteUser"/></item>
	/// </list>
	/// </summary>
	public interface IRemoteUser : ICommunicationUser { }

	/// <inheritdoc cref="IRemoteUser"/>
	/// <summary>
	/// 나의 <see cref="ILocalMediaTrack"/>을 상대 Peer로 직접 Publish 가능한 <see cref="IRemoteUser"/>입니다.
	/// <br/>미디어 서버의 경우 사용되지 않습니다.
	/// <br/>
	/// <br/><see cref="eRemoteBehaviour"/>가 <see cref="eRemoteBehaviour.PUBLISH_ONLY"/>
	/// <br/>혹은 <see cref="eRemoteBehaviour.FULL"/>인 경우 사용됩니다.
	/// </summary>
	public interface IPublishableRemoteUser : IRemoteUser
	{
		/// <summary>
		/// 미디어 서버에서 수신된 <see cref="MediaSdkUser"/>입니다.
		/// </summary>
		/// <remarks>
		/// FIXME: 제거 예정
		/// </remarks>
		MediaSdkUser MediaSdkUser { get; }
	}

	/// <inheritdoc cref="IRemoteUser"/>
	/// <summary>
	/// Subscribe 가능한 <see cref="IRemoteMediaTrack"/>를 나에게 Publish 하는 <see cref="IRemoteUser"/>입니다.
	/// <br/>미디어 서버와 P2P 모두 사용됩니다.
	/// <br/>
	/// <br/><see cref="eRemoteBehaviour"/>가 <see cref="eRemoteBehaviour.SUBSCRIBE_ONLY"/>, <see cref="eRemoteBehaviour.SUBSCRIBE_ON_DEMAND"/>
	/// <br/>혹은 <see cref="eRemoteBehaviour.FULL"/>인 경우 사용됩니다.
	/// </summary>
	public interface ISubscribableRemoteUser : IRemoteUser, IViewModelUser
	{
		/// <summary>
		/// 미디어 서버에서 수신된 <see cref="MediaSdkUser"/>입니다.
		/// </summary>
		/// <remarks>
		/// FIXME: 제거 예정
		/// </remarks>
		MediaSdkUser MediaSdkUser { get; }

		/// <summary>
		/// 원격 사용자로부터 수신된 트랙들을 관리하는 매니저입니다.
		/// </summary>
		IRemoteTrackManager? SubscribeTrackManager { get; }

		/// <summary>
		/// 원격 사용자로부터 수신된 트랙에 관찰자가 등록되어 있는지 확인합니다.
		/// </summary>
		bool ContainsObserver(eTrackType trackType, IRemoteTrackObserver observer);

		/// <summary>
		/// 원격 사용자로부터 수신된 트랙을 사용하기 위해 관찰자를 등록합니다.
		/// </summary>
		bool TryAddObserver(eTrackType trackType, IRemoteTrackObserver observer);

		/// <summary>
		/// 원격 사용자로부터 수신된 트랙의 관찰자를 제거합니다.
		/// </summary>
		bool RemoveObserver(eTrackType trackType, IRemoteTrackObserver observer);
	}

	/// <inheritdoc cref="IRemoteUser"/>
	/// <summary>
	/// 상대에게 <see cref="IRemoteMediaTrack"/>을 나에게 Publish 해 달라 요청할 수 있는 <see cref="IRemoteUser"/>입니다.
	/// <br/>미디어 서버의 경우 사용되지 않습니다.
	/// <br/>
	/// <br/><see cref="eRemoteBehaviour"/>가 <see cref="eRemoteBehaviour.SUBSCRIBE_ON_DEMAND"/>
	/// <br/>혹은 <see cref="eRemoteBehaviour.FULL"/>인 경우 사용됩니다.
	/// </summary>
	public interface IPublishRequestableRemoteUser : IRemoteUser, IViewModelUser
	{
		/// <summary>
		/// 미디어 서버에서 수신된 <see cref="MediaSdkUser"/>입니다.
		/// </summary>
		/// <remarks>
		/// FIXME: 제거 예정
		/// </remarks>
		MediaSdkUser MediaSdkUser { get; }

		/// <summary>
		/// 원격 사용자에게 트랙 발행 요청을 등록했는지 확인합니다.
		/// </summary>
		bool ContainsPublishRequest(IRemoteTrackPublishRequester requester, eTrackType trackType);

		/// <summary>
		/// 원격 사용자에게 트랙 발행을 요청하기 위해 요청자를 등록합니다.
		/// </summary>
		bool TryAddPublishRequest(IRemoteTrackPublishRequester requester, eTrackType trackType);

		/// <summary>
		/// 원격 사용자 트랙 발행 요청자 목록에서 요청자를 제거합니다.
		/// </summary>
		bool RemovePublishRequest(IRemoteTrackPublishRequester requester, eTrackType trackType);
	}
}
