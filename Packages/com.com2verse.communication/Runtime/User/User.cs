/*===============================================================
* Product:		Com2Verse
* File Name:	User.cs
* Developer:	urun4m0r1
* Date:			2022-04-07 19:26
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Diagnostics;
using Com2Verse.Logger;
using Cysharp.Text;

namespace Com2Verse.Communication
{
	/// <summary>
	/// 채널에 속한 사용자의 정보를 저장하는 구조체.
	/// <br/>모든 속성은 불변이며, 읽기 전용입니다.
	/// </summary>
	public readonly struct User
	{
		/// <summary>
		/// 기본 값 <see cref="Uid"/>로 생성된 <see cref="User"/> 구조체를 반환합니다.<br/>
		/// 다음 코드와 동일한 결과를 반환합니다.
		/// <code>new User(default)</code>
		/// </summary>
		public static User Default { get; } = new(default);

		/// <summary>
		/// 사용자의 고유 ID를 반환합니다.
		/// <br/>채널과 관련없이 모든 사용자는 고유한 ID를 가지고 있습니다.
		/// </summary>
		public Uid Uid { get; }

		/// <summary>
		/// 사용자의 이름을 반환합니다.
		/// <br/>최종 사용자에게 표시하기 적절하지 않은 값입니다.
		/// </summary>
		public string? Name { get; }

		/// <summary>
		/// 유저 정보를 초기화합니다.
		/// </summary>
		/// <param name="uid">사용자의 고유 ID</param>
		///	<param name="name">사용자의 이름.</param>
		/// <param name="skipValidation">유효성 검사를 건너뜁니다.</param>
		/// <exception cref="ArgumentException"><paramref name="uid"/>가 유효하지 않은 경우</exception>
		public User(Uid uid, string? name = nameof(User), bool skipValidation = false)
		{
			Uid  = uid;
			Name = name;

			if (skipValidation || Uid.IsValid())
				return;

			throw new ArgumentException("Invalid Uid", nameof(uid));
		}

#region Debug
		/// <summary>
		/// 간략한 디버그 정보를 반환합니다.
		/// </summary>
		/// <returns>
		/// Name (Uid) 형식의 문자열을 반환합니다.
		/// <br/>ex) urun4m0r1 (1234567890)
		/// </returns>
		[DebuggerHidden, StackTraceIgnore]
		public string GetInfoText()
		{
			return ZString.Format(
				"{0} ({1})"
			  , Name, Uid.ToString());
		}
#endregion // Debug
	}
}
