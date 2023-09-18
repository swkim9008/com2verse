/*===============================================================
* Product:		Com2Verse
* File Name:	IChannel.cs
* Developer:	urun4m0r1
* Date:			2022-04-14 11:21
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
	/// event listener는 event가 등록된 스레드에서만 암시적으로 이벤트가 호출된다.<br/>
	/// 따라서 Invoke() 시점의 context가 이벤트 등록(+=, -=) 시점의 context와 다를 경우,<br/>
	/// 해당 callback이 event delegate 목록에 존재하더라도, 실제 event는 암시적으로 호출되지 않는다.<br/>
	/// 따라서 해당 인터페이스의 event를 사용할때는 충분히 주의가 필요하다.<br/>
	/// </summary>
	public interface IChannel
	{
		IConnectionController Connector { get; }

		event Action<IChannel, eConnectionState>? ConnectionChanged;

		event Action<IChannel, ICommunicationUser>? UserJoin;
		event Action<IChannel, ICommunicationUser>? UserLeft;

		event Action<IChannel, ICommunicationUser?, ICommunicationUser?>? HostChanged;
		event Action<IChannel, ILocalUser?, ILocalUser?>?                 SelfChanged;

		ChannelInfo Info { get; }

		IReadOnlyDictionary<Uid, ICommunicationUser> ConnectedUsers { get; }

		ICommunicationUser? Host { get; }
		ILocalUser?         Self { get; }

		string GetDebugInfo();
	}

	public interface IUserEventInvoker
	{
		void RaiseUserJoinedEvent(ICommunicationUser user);
		void RaiseUserLeftEvent(ICommunicationUser   user);
	}
}
