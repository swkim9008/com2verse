/*===============================================================
 * Product:		Com2Verse
 * File Name:	IRemoteTrackPublishRequestHandler.cs
 * Developer:	urun4m0r1
 * Date:		2023-02-22 17:51
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;

namespace Com2Verse.Communication
{
	/// <summary>
	/// 등록된 <see cref="IRemoteTrackPublishRequester"/>들의 조건을 검사해 <see cref="Requested"/> 이벤트를 발생시키는 인터페이스<br/>
	/// <see cref="IDisposable.Dispose()"/> 호출시 조건이 충족되지 않은 상태로 간주되어 <see cref="Requested"/> 이벤트가 발생할 수 있다.
	/// </summary>
	public interface IRemoteTrackPublishRequestHandler : IDisposable
	{
		/// <summary>
		/// 조건이 충족 여부가 변경되었을때 발생하는 이벤트
		/// </summary>
		event Action<bool>? Requested;

		/// <summary>
		/// 현재 조건이 충족되었는지 여부
		/// </summary>
		bool IsRequested { get; }

		/// <summary>
		/// <see cref="requester"/>가 등록되어있는지 여부
		/// </summary>
		bool IsRegistered(IRemoteTrackPublishRequester requester);

		/// <summary>
		/// <see cref="requester"/>를 등록한다
		/// </summary>
		bool Register(IRemoteTrackPublishRequester requester);

		/// <summary>
		/// <see cref="requester"/>를 등록해제한다
		/// </summary>
		bool Unregister(IRemoteTrackPublishRequester requester);

		/// <summary>
		/// 등록된 모든 <see cref="IRemoteTrackPublishRequester"/>를 등록해제한다
		/// </summary>
		void Reset();
	}
}
