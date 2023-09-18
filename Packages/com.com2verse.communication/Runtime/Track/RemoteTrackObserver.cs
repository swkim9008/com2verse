/*===============================================================
* Product:		Com2Verse
* File Name:	RemoteTrackObserver.cs
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
	/// WebRTC 통신량 최적화를 위해, 트랙을 관찰중인 GameObject의 상태를 지정할 수 있는 컴포넌트입니다.<br />
	/// <br />
	/// 눈에 보이지 않거나 들을 필요가 없는 트랙의 경우, Subscribe를 하지 않음으로서 네트워크 트래픽을 절약할 수 있습니다.<br />
	/// 따라서 해당 컴포넌트를 통해 필요 없는 트랙의 Subscribe를 해제할 수 있습니다.<br />
	/// 활성화 되어있는 GameObject가 단 하나라도 있으면 Subscribe를 하고, 모두 비활성화 되어있으면 Subscribe를 해제합니다.
	/// </summary>
	[AddComponentMenu("[Communication]/[Communication] Remote Track Observer")]
	public sealed class RemoteTrackObserver : MonoBehaviour, IRemoteTrackObserver
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

				UnregisterObserver();

				_user = value;

				if (isActiveAndEnabled)
					RegisterObserver();
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

		private void OnEnable()  => RegisterObserver();
		private void OnDisable() => UnregisterObserver();

		private void RegisterObserver()
		{
			if (User == null)
				return;

			if (User is not ISubscribableRemoteUser remoteUser)
			{
				LogTypeMismatch();
				return;
			}

			LogSuccess();
			remoteUser.TryAddObserver(_trackType, this);
		}

		private void UnregisterObserver()
		{
			if (User == null)
				return;

			if (User is not ISubscribableRemoteUser remoteUser)
			{
				LogTypeMismatch();
				return;
			}

			LogSuccess();
			remoteUser.RemoveObserver(_trackType, this);
		}

#region Debug
		[Conditional(C2VDebug.VerboseDefinition), DebuggerHidden, StackTraceIgnore]
		private void LogTypeMismatch([CallerMemberName] string? caller = null)
		{
			var message = ZString.Format(
				"Target not implementing {0}. Operation ignored."
			  , nameof(ISubscribableRemoteUser));

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
			var observerPath = transform.IsUnityNull()
				? "null"
				: transform.GetFullPathInHierachy();

			return ZString.Format(
				"[{0}]\n: Type = {1}\n: ObserverPath = \"{2}\""
			  , GetLogCategory(), _trackType, observerPath);
		}
#endregion // Debug
	}
}
