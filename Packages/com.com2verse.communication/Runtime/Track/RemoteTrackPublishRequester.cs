/*===============================================================
* Product:		Com2Verse
* File Name:	RemoteTrackPublishRequester.cs
* Developer:	urun4m0r1
* Date:			2022-08-19 17:05
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Cysharp.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.Communication
{
	/// <summary>
	/// WebRTC 통신량 최적화를 위해, RemoteUser에게 Publish 요청 상태를 지정할 수 있는 컴포넌트입니다.<br/>
	/// <br/>
	/// P2P 통신 환경에서는 Subscribe를 하지 않아도 상대가 Publish를 하고 있으면, 네트워크 트래픽이 발생합니다.<br/>
	/// 따라서 해당 컴포넌트를 통해 RemoteUser 에게 Publish 자체를 하지 않도록 요청할 수 있습니다.<br/>
	/// GameObject의 활성화 상태에 따라 자동으로 Publish 요청 상태를 변경합니다.
	/// </summary>
	[AddComponentMenu("[Communication]/[Communication] Remote Track Publish Requester")]
	public sealed class RemoteTrackPublishRequester : MonoBehaviour, IRemoteTrackPublishRequester
	{
#region InspectorFields
		[SerializeField] private eTrackType _trackType = eTrackType.UNKNOWN;
#endregion // InspectorFields

		private ICommunicationUser? _user;

#region ViewModelProperties
		[UsedImplicitly] // Setter used by view model.
		public ICommunicationUser? User
		{
			get => _user;
			set
			{
				if (User == value)
					return;

				UnregisterPublishRequester();

				_user = value;

				if (isActiveAndEnabled)
					RegisterPublishRequester();
			}
		}

		/// <summary>
		/// 상태를 초기화하고 이벤트를 발생시킵니다.
		/// </summary>
		public void Clear()
		{
			User = null;
		}
#endregion // ViewModelProperties

		private void OnEnable()  => RegisterPublishRequester();
		private void OnDisable() => UnregisterPublishRequester();

		private void RegisterPublishRequester()
		{
			if (User == null)
				return;

			if (User is not IPublishRequestableRemoteUser remoteUser)
			{
				LogTypeMismatch();
				return;
			}

			LogSuccess();
			remoteUser.TryAddPublishRequest(this, _trackType);
		}

		private void UnregisterPublishRequester()
		{
			if (User == null)
				return;

			if (User is not IPublishRequestableRemoteUser remoteUser)
			{
				LogTypeMismatch();
				return;
			}

			LogSuccess();
			remoteUser.RemovePublishRequest(this, _trackType);
		}

#region Debug
		[Conditional(C2VDebug.VerboseDefinition), DebuggerHidden, StackTraceIgnore]
		private void LogTypeMismatch([CallerMemberName] string? caller = null)
		{
			var message = ZString.Format(
				"Target not implementing {0}. Operation ignored."
			  , nameof(IPublishRequestableRemoteUser));

			C2VDebug.LogWarningMethod(GetLogCategory(), FormatMessage(message), caller);
		}

		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, StackTraceIgnore]
		private void LogSuccess([CallerMemberName] string? caller = null)
		{
			C2VDebug.LogCategory(GetLogCategory(), FormatMessage(caller ?? "null"));
		}

		[DebuggerHidden, StackTraceIgnore]
		private string GetLogCategory()
		{
			var className   = GetType().Name;
			var channelInfo = User?.ChannelInfo.GetInfoText() ?? "null";
			var targetInfo  = User?.GetInfoText()             ?? "null";

			return ZString.Format(
				"{0} ({1}): {2} / {3}"
			  , className, _trackType, channelInfo, targetInfo);
		}

		[DebuggerHidden, StackTraceIgnore]
		private string FormatMessage(string message)
		{
			var targetInfo  = User?.GetDebugInfo()             ?? "TargetInfo = null";
			var channelInfo = User?.ChannelInfo.GetDebugInfo() ?? "ChannelInfo = null";

			return ZString.Format(
				"{0}\n----------\n{1}\n----------\n{2}\n----------\n{3}"
			  , message, GetDebugInfo(), targetInfo, channelInfo);
		}

		[DebuggerHidden, StackTraceIgnore]
		public string GetDebugInfo()
		{
			var requesterPath = transform.IsUnityNull()
				? "null"
				: transform.GetFullPathInHierachy();

			return ZString.Format(
				"[{0}]\n: Type = {1}\n: RequesterPath = \"{2}\""
			  , GetLogCategory(), _trackType, requesterPath);
		}
#endregion // Debug
	}
}
