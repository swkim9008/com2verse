/*===============================================================
* Product:		Com2Verse
* File Name:	eTrackType.cs
* Developer:	urun4m0r1
* Date:			2022-10-14 16:45
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

namespace Com2Verse.Communication
{
	/// <summary>
	/// <see cref="IMediaTrack"/>의 종류를 나타내는 열거형입니다.
	/// </summary>
	public enum eTrackType
	{
		/// <summary>
		/// 알 수 없는 종류의 트랙입니다. 예외 처리를 위해 사용하십시오.
		/// </summary>
		UNKNOWN = 0,

		/// <summary>
		/// 음성 트랙입니다. (마이크)
		/// </summary>
		VOICE = 1,

		/// <summary>
		/// 화상 트랙입니다. (웹캠)
		/// </summary>
		CAMERA = 2,

		/// <summary>
		/// 화면 트랙입니다. (화면 공유)
		/// </summary>
		SCREEN = 3,

		/// <summary>
		/// 오디오 트랙입니다. (파일)
		/// </summary>
		AUDIO = 4,

		/// <summary>
		/// 비디오 트랙입니다. (파일)
		/// </summary>
		VIDEO = 5,
	}
}
