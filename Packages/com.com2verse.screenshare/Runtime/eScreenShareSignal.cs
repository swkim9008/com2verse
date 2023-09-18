/*===============================================================
 * Product:		Com2Verse
 * File Name:	eScreenShareSignal.cs
 * Developer:	urun4m0r1
 * Date:		2023-01-09 15:11
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

namespace Com2Verse.ScreenShare
{
	public enum eScreenShareSignal
	{
		NONE,

		IGNORE,

		/// <summary>
		/// 유저에 의해 화면 공유 시작시
		/// </summary>
		CAPTURE_STARTED_BY_USER,

		/// <summary>
		/// 유저에 의해 공유 화면 변경시
		/// </summary>
		CAPTURE_CHANGED_BY_USER,

		/// <summary>
		/// 유저에 의해 화면 공유 중지시
		/// </summary>
		CAPTURE_STOPPED_BY_USER,

		/// <summary>
		/// 시스템에 의해 화면 공유가 중지된 경우
		/// </summary>
		CAPTURE_STOPPED_BY_SYSTEM,

		/// <summary>
		/// 공유 대상 화면이 제거되어 화면 공유가 중지된 경우
		/// </summary>
		CAPTURE_STOPPED_BY_SCREEN_REMOVED,

		/// <summary>
		/// 화면 정보를 받아오지 못해 화면 공유가 중지된 경우
		/// </summary>
		CAPTURE_STOPPED_BY_VISIBILITY,

		/// <summary>
		/// 원격 유저에 의해 화면 공유가 중지된 경우
		/// </summary>
		CAPTURE_STOPPED_BY_REMOTE,

		/// <summary>
		/// 원격 유저가 화면 공유를 시작한 경우
		/// </summary>
		RECEIVE_STARTED_BY_REMOTE,

		/// <summary>
		/// 원격 유저가 화면 공유를 중지한 경우
		/// </summary>
		RECEIVE_STOPPED_BY_REMOTE,
	}
}
