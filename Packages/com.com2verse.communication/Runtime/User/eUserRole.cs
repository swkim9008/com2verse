/*===============================================================
 * Product:		Com2Verse
 * File Name:	eUserRole.cs
 * Developer:	urun4m0r1
 * Date:		2023-01-26 16:08
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

namespace Com2Verse.Communication
{
	/// <summary>
	/// 유저의 채널 역할을 나타내는 열거형입니다.
	/// </summary>
	public enum eUserRole
	{
		/// <summary>
		/// 정의되지 않은 역할입니다.
		/// </summary>
		UNDEFINED,

		/// <summary>
		/// 기본 역할입니다.
		/// </summary>
		DEFAULT,

		/// <summary>
		/// P2P 통신에서의 호스트 역할입니다.
		/// </summary>
		HOST,

		/// <summary>
		/// P2P 통신에서의 게스트 역할입니다.
		/// </summary>
		GUEST,

		/// <summary>
		/// 해당 역할로 지정된 Peer 끼리는 서로 Peer 정보를 공유하지 않습니다. 미디어 서버 전용 기능입니다.
		/// </summary>
		AUDIENCE,
	}
}
