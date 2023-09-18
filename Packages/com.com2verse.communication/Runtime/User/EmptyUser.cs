/*===============================================================
* Product:		Com2Verse
* File Name:	EmptyUser.cs
* Developer:	urun4m0r1
* Date:			2022-11-14 14:59
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

namespace Com2Verse.Communication
{
	/// <inheritdoc cref="CommunicationUser"/>
	/// <summary>
	/// <see cref="CommunicationUser" />의 비어있는 구현입니다.
	/// <br/>유저 자체에 대한 메타 정보는 필요하나, 음성, 카메라, 화면 등의 기능은 필요하지 않은 경우에 사용합니다.
	/// <br/>
	/// <br/>해당 클래스는 다음과 같은 하위 구현을 가집니다.
	/// <list type="bullet">
	/// <item><see cref="EmptyLocalUser"/></item>
	/// <item><see cref="EmptyRemoteUser"/></item>
	/// </list>
	/// </summary>
	public abstract class EmptyUser : CommunicationUser
	{
		protected EmptyUser(ChannelInfo channelInfo, User user, eUserRole role) : base(channelInfo, user, role) { }
	}

	/// <inheritdoc cref="EmptyUser"/>
	/// <summary>
	/// <see cref="EmptyUser" />의 <see cref="ILocalUser"/> 구현입니다.
	/// </summary>
	public sealed class EmptyLocalUser : EmptyUser, ILocalUser
	{
		public EmptyLocalUser(ChannelInfo channelInfo) : base(channelInfo, channelInfo.LoginUser, channelInfo.UserRole) { }
	}

	/// <inheritdoc cref="EmptyUser"/>
	/// <summary>
	/// <see cref="EmptyUser" />의 <see cref="IRemoteUser"/> 구현입니다.
	/// </summary>
	public sealed class EmptyRemoteUser : EmptyUser, IRemoteUser
	{
		public EmptyRemoteUser(ChannelInfo channelInfo, User user, eUserRole role) : base(channelInfo, user, role) { }
	}
}
