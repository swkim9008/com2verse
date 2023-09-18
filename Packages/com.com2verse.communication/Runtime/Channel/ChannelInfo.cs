/*===============================================================
* Product:		Com2Verse
* File Name:	ChannelInfo.cs
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
using UnityEngine;

namespace Com2Verse.Communication
{
	/// <summary>
	/// 채널 정보를 저장하는 레코드.<br/>
	/// 모든 속성은 불변이며, 읽기 전용입니다.<br/>
	/// </summary>
	[Serializable]
	public sealed record ChannelInfo(string ChannelId, eChannelType ChannelType, eChannelDirection ChannelDirection, User LoginUser, eUserRole UserRole, bool SkipRemoteUserIdValidation = false)
	{
		/// <summary>
		/// 채널의 고유 ID를 반환합니다.
		/// </summary>
		[field: SerializeField]
		public string ChannelId { get; private set; } = ChannelId;

		/// <summary>
		/// 채널의 통신 타입을 반환합니다.
		/// </summary>
		[field: SerializeField]
		public eChannelType ChannelType { get; private set; } = ChannelType;

		/// <summary>
		/// 채널의 통신 방향을 반환합니다.
		/// </summary>
		[field: SerializeField]
		public eChannelDirection ChannelDirection { get; private set; } = ChannelDirection;

		/// <summary>
		/// 채널에 로그인한 사용자(본인)의 정보를 반환합니다.
		/// </summary>
		[field: SerializeField]
		public User LoginUser { get; private set; } = LoginUser;

		/// <summary>
		/// <see cref="LoginUser"/>의 채널 역할을 반환합니다.
		/// </summary>
		[field: SerializeField]
		public eUserRole UserRole { get; private set; } = UserRole;

		/// <summary>
		/// 원격 사용자의 ID 유효성 검사를 건너뜁니다.
		/// </summary>
		public bool SkipRemoteUserIdValidation { get; private set; } = SkipRemoteUserIdValidation;

		/// <summary>
		/// 서버, P2P 등 채널의 퍼블리싱 방식을 반환합니다.
		/// </summary>
		public ePublishTarget PublishTarget { get; } = PublishTargetConverter.GetPublishTarget(ChannelType, ChannelDirection);

		/// <summary>
		/// P2P 채널의 호스트 여부를 반환합니다.
		/// </summary>
		public bool IsP2PChannelHost { get; } = RemoteBehaviourConverter.IsP2PChannelHost(ChannelType, ChannelDirection, UserRole);

		/// <summary>
		/// P2P 채널의 게스트 여부를 반환합니다.
		/// </summary>
		public bool IsP2PChannelGuest { get; } = RemoteBehaviourConverter.IsP2PChannelGuest(ChannelType, ChannelDirection, UserRole);

		/// <summary>
		/// 해당 유저 권한의 원격 유저 역할을 반환합니다.
		/// </summary>
		public eRemoteBehaviour GetRemoteBehaviour(eUserRole userRole) => RemoteBehaviourConverter.GetRemoteBehaviour(ChannelType, ChannelDirection, userRole);
#region Debug
		[DebuggerHidden, StackTraceIgnore]
		private string GetLogCategory()
		{
			var className = GetType().Name;

			return ZString.Format(
				"{0}: {1}"
			  , className, GetInfoText());
		}

		/// <summary>
		/// 상세 디버그 정보를 반환합니다.
		/// </summary>
		[DebuggerHidden, StackTraceIgnore]
		public string GetDebugInfo()
		{
			var loginUserInfo = LoginUser.GetInfoText();

			return ZString.Format(
				"[{0}]\n: ChannelType = {1}\n: ChannelDirection = {2}\n: PublishTarget = {3}\n: LoginUser = \"{4}\"\n: UserRole = {5}"
			  , GetLogCategory(), ChannelType, ChannelDirection, PublishTarget, loginUserInfo, UserRole);
		}

		/// <summary>
		/// 간략한 디버그 정보를 반환합니다.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// 동시에 호스트이며 게스트인 경우 발생합니다. 모든 실행 경로에서 발생할 수 없습니다.
		/// </exception>
		[DebuggerHidden, StackTraceIgnore]
		public string GetInfoText()
		{
			var format = (IsP2PChannelHost, IsP2PChannelGuest) switch
			{
				(true, true)   => throw new InvalidOperationException("P2P Channel Host and Guest cannot be same user."),
				(true, false)  => "\"{0}\" (Host)",
				(false, true)  => "\"{0}\" (Guest)",
				(false, false) => "\"{0}\"",
			};

			return ZString.Format(format, ChannelId);
		}
#endregion // Debug
	}
}
