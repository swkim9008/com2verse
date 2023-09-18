/*===============================================================
* Product:		Com2Verse
* File Name:	CommunicationUser.cs
* Developer:	urun4m0r1
* Date:			2022-04-07 19:26
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Com2Verse.Logger;
using Cysharp.Text;

namespace Com2Verse.Communication
{
	/// <inheritdoc cref="ICommunicationUser"/>
	/// <summary>
	/// <see cref="ICommunicationUser"/>의 기본 구현입니다.
	/// <br/><see cref="IDisposable"/>을 구현하고 있기 때문에 반드시 <see cref="Dispose"/>를 호출해야 합니다.
	/// <br/>
	/// <br/>해당 클래스는 다음과 같은 하위 구현을 가집니다.
	/// <list type="bullet">
	/// <item><see cref="EmptyUser"/></item>
	/// <item><see cref="PublishableLocalUser"/></item>
	/// <item><see cref="MediaSdk.SubscribeOnlyRemoteUser"/></item>
	/// <item><see cref="Cheat.DummyUser"/></item>
	/// </list>
	/// </summary>
	public abstract class CommunicationUser : ICommunicationUser, IDisposable
	{
		public ChannelInfo ChannelInfo { get; }
		public User        User        { get; }
		public eUserRole   Role        { get; }

		/// <summary>
		/// <see cref="CommunicationUser"/>의 새 인스턴스를 초기화합니다.
		/// </summary>
		/// <param name="channelInfo">유저가 속한 채널의 정보</param>
		/// <param name="user">유저의 정보</param>
		/// <param name="role">유저의 역할</param>
		protected CommunicationUser(ChannelInfo channelInfo, User user, eUserRole role)
		{
			ChannelInfo = channelInfo;
			User        = user;
			Role        = role;
		}

#region IDisposable
		private bool _disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				// 현재 정리할 관리 리소스가 없습니다.
			}

			// Uncomment this line in inherited class to implement standard disposing pattern.
			// base.Dispose(disposing);

			_disposed = true;
		}
#endregion // IDisposable

#region Debug
		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		protected virtual void Log(string message, [CallerMemberName] string? caller = null)
		{
			C2VDebug.LogMethod(GetLogCategory(), FormatMessage(message), caller);
		}

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		protected virtual void LogWarning(string message, [CallerMemberName] string? caller = null)
		{
			C2VDebug.LogWarningMethod(GetLogCategory(), FormatMessage(message), caller);
		}

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		protected virtual void LogError(string message, [CallerMemberName] string? caller = null)
		{
			C2VDebug.LogErrorMethod(GetLogCategory(), FormatMessage(message), caller);
		}

		[DebuggerHidden, StackTraceIgnore]
		protected virtual string GetLogCategory()
		{
			var className   = GetType().Name;
			var channelInfo = ChannelInfo.GetInfoText();
			var userInfo    = GetInfoText();

			return ZString.Format(
				"{0}: {1} / {2}"
			  , className, channelInfo, userInfo);
		}

		[DebuggerHidden, StackTraceIgnore]
		protected virtual string FormatMessage(string message)
		{
			return ZString.Format(
				"{0}\n----------\n{1}"
			  , message, GetDebugInfo());
		}

		[DebuggerHidden, StackTraceIgnore]
		public virtual string GetDebugInfo()
		{
			return ZString.Format(
				"[{0}]\n: Role = {1}"
			  , GetLogCategory(), Role);
		}

		[DebuggerHidden, StackTraceIgnore]
		public virtual string GetInfoText()
		{
			var userInfo          = User.GetInfoText();
			var isSelf            = this is ILocalUser;
			var isP2PChannelHost  = this.IsP2PChannelHost();
			var isP2PChannelGuest = this.IsP2PChannelGuest();

			var format = (isSelf, isP2PChannelHost, isP2PChannelGuest) switch
			{
				(_, true, true)       => throw new InvalidOperationException("P2P Channel Host and Guest cannot be same user."),
				(true, true, false)   => "\"{0}\" (Self, Host)",
				(true, false, true)   => "\"{0}\" (Self, Guest)",
				(true, false, false)  => "\"{0}\" (Self)",
				(false, true, false)  => "\"{0}\" (Host)",
				(false, false, true)  => "\"{0}\" (Guest)",
				(false, false, false) => "\"{0}\"",
			};

			return ZString.Format(format, userInfo);
		}
#endregion // Debug
	}
}
