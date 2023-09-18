/*===============================================================
* Product:		Com2Verse
* File Name:	Channel.cs
* Developer:	urun4m0r1
* Date:			2022-08-05 14:09
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Com2Verse.Logger;
using Cysharp.Text;
using MediaSdkUser = Com2Verse.Solution.UnityRTCSdk.User;

namespace Com2Verse.Communication.MediaSdk
{
	/// <summary>
	/// MediaSdk 채널에 접속한 유저들을 관리하는 클래스입니다.<br/>
	/// <see cref="Dispose"/>메서드 호출 시, <see cref="EventDispatcher"/>의 콜백을 해제합니다.<br/>
	/// 이후 모든 유저에 대해 <see cref="UserLeft"/>이벤트를 발생시킵니다.
	/// </summary>
	public sealed class ChannelUserManager : IDisposable
	{
		public event Action<User, MediaSdkUser>? UserJoined;
		public event Action<User, MediaSdkUser>? UserLeft;

		public event Action<User, MediaSdkUser, MediaSdkUser>? UserUpdated;

		public event Action<string, string>? DisconnectRequest;

		public IReadOnlyDictionary<Uid, (User, MediaSdkUser)> Users => _users;

		public ChannelInfo               ChannelInfo     { get; }
		public RtcChannelEventDispatcher EventDispatcher { get; }

		private readonly Dictionary<Uid, (User, MediaSdkUser)> _users = new(UidComparer.Default);

		public ChannelUserManager(ChannelInfo channelInfo, RtcChannelEventDispatcher eventDispatcher)
		{
			ChannelInfo     = channelInfo;
			EventDispatcher = eventDispatcher;

			RegisterChannelCallbacks();
		}

		private void RegisterChannelCallbacks()
		{
			UnRegisterChannelCallbacks();

			EventDispatcher.UserJoined        += OnUserJoined;
			EventDispatcher.UserLeft          += OnUserLeft;
			EventDispatcher.UserUpdated       += OnUserUpdated;
			EventDispatcher.DisconnectRequest += OnDisconnectRequest;
		}

		private void UnRegisterChannelCallbacks()
		{
			EventDispatcher.UserJoined        -= OnUserJoined;
			EventDispatcher.UserLeft          -= OnUserLeft;
			EventDispatcher.UserUpdated       -= OnUserUpdated;
			EventDispatcher.DisconnectRequest -= OnDisconnectRequest;
		}

		private void OnUserJoined(MediaSdkUser mediaSdkUser)
		{
			if (!mediaSdkUser.TryParseUser(out var user, ChannelInfo.SkipRemoteUserIdValidation))
			{
				LogError($"Failed to parse user from media sdk user. / {mediaSdkUser.GetInfoText()}");
				return;
			}

			if (TryFindRemoteUser(user, out _))
			{
				LogError($"User already exists with same uid, operation ignored. / {mediaSdkUser.GetInfoText()}");
				return;
			}

			_users.Add(user.Uid, (user, mediaSdkUser));
			UserJoined?.Invoke(user, mediaSdkUser);
			Log(mediaSdkUser.GetInfoText());
		}

		private void OnUserLeft(MediaSdkUser mediaSdkUser)
		{
			if (!mediaSdkUser.TryParseUser(out var user, ChannelInfo.SkipRemoteUserIdValidation))
			{
				LogError($"Failed to parse user from media sdk user. / {mediaSdkUser.GetInfoText()}");
				return;
			}

			if (!TryFindRemoteUser(user, out _))
			{
				LogError($"User not found, operation ignored. / {mediaSdkUser.GetInfoText()}");
				return;
			}

			_users.Remove(user.Uid);
			UserLeft?.Invoke(user, mediaSdkUser);
			Log(mediaSdkUser.GetInfoText());
		}

		private void OnUserUpdated(MediaSdkUser mediaSdkUser)
		{
			if (!mediaSdkUser.TryParseUser(out var user, ChannelInfo.SkipRemoteUserIdValidation))
			{
				LogError($"Failed to parse user from media sdk user. / {mediaSdkUser.GetInfoText()}");
				return;
			}

			if (!TryFindRemoteUser(user, out var foundUser))
			{
				LogError($"User not found, operation ignored. / {mediaSdkUser.GetInfoText()}");
				return;
			}

			_users[user.Uid] = (user, mediaSdkUser);
			UserUpdated?.Invoke(user, foundUser, mediaSdkUser);
			Log(ZString.Format("{0} -> {1}", foundUser.GetInfoText(), mediaSdkUser.GetInfoText()));
		}

		private void OnDisconnectRequest(string channelId, string reason)
		{
			DisconnectRequest?.Invoke(channelId, reason);
		}

		private bool TryFindRemoteUser(User user, out MediaSdkUser mediaSdkUser)
		{
			var result = Users.TryGetValue(user.Uid, out var value);
			mediaSdkUser = value.Item2;
			return result;
		}

#region IDisposable
		private bool _disposed;

		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;

			UnRegisterChannelCallbacks();

			foreach (var user in _users)
			{
				UserLeft?.Invoke(user.Value.Item1, user.Value.Item2);
				Log(user.Value.Item1.GetInfoText());
			}

			_users.Clear();
		}
#endregion // IDisposable

#region Debug
		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		private void Log(string message, [CallerMemberName] string? caller = null)
		{
			C2VDebug.LogMethod(GetLogCategory(), FormatMessage(message), caller);
		}

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		private void LogWarning(string message, [CallerMemberName] string? caller = null)
		{
			C2VDebug.LogWarningMethod(GetLogCategory(), FormatMessage(message), caller);
		}

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		private void LogError(string message, [CallerMemberName] string? caller = null)
		{
			C2VDebug.LogErrorMethod(GetLogCategory(), FormatMessage(message), caller);
		}

		[DebuggerHidden, StackTraceIgnore]
		private string GetLogCategory()
		{
			var className   = GetType().Name;
			var channelInfo = ChannelInfo.GetInfoText();

			return ZString.Format(
				"{0}: {1}"
			  , className, channelInfo);
		}

		[DebuggerHidden, StackTraceIgnore]
		private string FormatMessage(string message)
		{
			var channelInfo = ChannelInfo.GetDebugInfo();

			return ZString.Format(
				"{0}\n----------\n{1}\n----------\n{2}"
			  , message, GetDebugInfo(), channelInfo);
		}

		[DebuggerHidden, StackTraceIgnore]
		public string GetDebugInfo()
		{
			return ZString.Format(
				"[{0}]\n: UserCount = {1}"
			  , GetLogCategory(), Users.Count);
		}
#endregion // Debug
	}
}
