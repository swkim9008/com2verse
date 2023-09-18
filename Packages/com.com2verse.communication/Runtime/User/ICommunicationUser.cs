/*===============================================================
* Product:		Com2Verse
* File Name:	ICommunicationUser.cs
* Developer:	urun4m0r1
* Date:			2022-11-14 15:01
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

namespace Com2Verse.Communication
{
	/// <summary>
	/// CommunicationViewModel 에서 내부 값으로 사용할 View 전용 User
	/// </summary>
	public interface IViewModelUser : ICommunicationUser
	{
		/// <summary>
		/// 미디어 트랙 모듈을 반환합니다.
		/// </summary>
		MediaModules Modules { get; }

		/// <summary>
		/// 유저의 음성 정보를 반환합니다.
		/// </summary>
		Audio? Voice => Modules.GetAudio(eTrackType.VOICE);

		/// <summary>
		/// 유저의 카메라 정보를 반환합니다.
		/// </summary>
		Video? Camera => Modules.GetVideo(eTrackType.CAMERA);

		/// <summary>
		/// 유저의 화면 정보를 반환합니다.
		/// </summary>
		Video? Screen => Modules.GetVideo(eTrackType.SCREEN);
	}

	/// <summary>
	/// <see cref="Com2Verse.Communication"/>의 모든 사용자를 나타내는 인터페이스입니다.
	/// <br/>
	/// <br/>해당 인터페이스는 다음과 같은 하위 인터페이스를 가집니다.
	/// <list type="bullet">
	/// <item><see cref="ILocalUser"/></item>
	/// <item><see cref="IRemoteUser"/></item>
	/// </list>
	/// </summary>
	public interface ICommunicationUser
	{
		/// <summary>
		/// 유저가 속한 채널의 불변 정보를 반환합니다.
		/// </summary>
		ChannelInfo ChannelInfo { get; }

		/// <summary>
		/// 유저의 불변 정보를 반환합니다.
		/// </summary>
		User User { get; }

		/// <summary>
		/// 유저의 채널 역할을 반환합니다.
		/// </summary>
		/// <remarks>
		/// TODO: <see cref="Communication.User"/> 구조체 내부로 이동 예정
		/// </remarks>
		eUserRole Role { get; }

		/// <summary>
		/// 자세한 디버그 정보를 반환합니다.
		/// </summary>
		string GetDebugInfo();

		/// <summary>
		/// 간략한 디버그 정보를 반환합니다.
		/// </summary>
		string GetInfoText();
	}
}
