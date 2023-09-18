/*===============================================================
* Product:		Com2Verse
* File Name:	ScreenShareViewModel.cs
* Developer:	urun4m0r1
* Date:			2022-09-23 13:22
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Linq;
using Com2Verse.Chat;
using Com2Verse.Communication.Unity;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using Com2Verse.ScreenShare;
using Com2Verse.Utils;
using Com2Verse.WebApi.Service;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("ScreenShare")]
	public class ScreenShareViewModel : ViewModelBase, IDisposable
	{
		[UsedImplicitly] public CommandHandler StartShare { get; }
		[UsedImplicitly] public CommandHandler StopShare  { get; }

		public ScreenShareViewModel()
		{
			StartShare = new CommandHandler(() => OnScreenShareToggled(true));
			StopShare  = new CommandHandler(() => OnScreenShareToggled(false));

			var controller = ScreenCaptureManager.Instance.Controller;
			controller.CapturedImageChanged += OnCapturedImageChanged;
			OnCapturedImageChanged(controller.CurrentScreen?.Screen);

			ChatManager.Instance.OnAuthorityChangedNotify += OnAuthorityChangedNotify;
		}

		private void OnAuthorityChangedNotify(long accountId, int oldAuthority, int newAuthority)
		{
			if (accountId != Network.User.Instance.CurrentUserData.ID)
				return;

			var isShareFeatureAvailable = Define.ShareFeatureAvailableAuthorities.Contains((Components.AuthorityCode)newAuthority);
			if (isShareFeatureAvailable)
				return;

			if (ScreenCaptureManager.InstanceOrNull?.Controller.IsCaptureRequestedOrCapturing is null or false)
				return;

			OnScreenShareToggled(false);
			UIManager.Instance.ShowPopupCommon(Define.String.UI_MeetingRoomSharing_Popup_NoAuthority);
		}

		public void Dispose()
		{
			var module = ModuleManager.InstanceOrNull;
			if (module != null)
				module.Screen.IsRunning = false;

			var controller = ScreenCaptureManager.InstanceOrNull?.Controller;
			if (controller != null)
			{
				controller.CapturedImageChanged -= OnCapturedImageChanged;
				controller.Dispose();
			}

			var chatManager = ChatManager.InstanceOrNull;
			if (chatManager != null)
				chatManager.OnAuthorityChangedNotify -= OnAuthorityChangedNotify;
		}

		private void OnCapturedImageChanged(Texture2D? texture)
		{
			if (texture.IsUnityNull())
			{
				var module = ModuleManager.InstanceOrNull;
				if (module != null)
					module.Screen.IsRunning = false;
			}
			else
			{
				ModuleManager.Instance.Screen.IsRunning = true;
			}
		}

		private void OnScreenShareToggled(bool isSharing)
		{
			if (isSharing && !CheckShareFeatureEnabled())
			{
				IsListEnabled = false;
				UpdateDisposedState();
				return;
			}

			var captureManager = ScreenCaptureManager.InstanceOrNull;
			var wasSharing     = captureManager?.Controller.IsCaptureRequested ?? false;

			if (isSharing && captureManager?.RemoteSharingUsers.Count > 0)
			{
				var isSharingCaptured = isSharing;
				UIManager.Instance.ShowPopupYesNo(
					null!, Define.String.UI_MeetingRoomSharing_Popup_ChoiceSharing,
					okAction: _ => ExecuteScreenShare(wasSharing, isSharingCaptured));
			}
			else
			{
				ExecuteScreenShare(wasSharing, isSharing);
			}
		}

		private void ExecuteScreenShare(bool wasSharing, bool isSharing)
		{
			var shareTarget = ViewModelManager.Instance.GetOrAdd<ScreenShareLayoutViewModel>().SelectedViewModel?.Value;
			if (isSharing && shareTarget != null)
			{
				var reason = wasSharing
					? eScreenShareSignal.CAPTURE_CHANGED_BY_USER
					: eScreenShareSignal.CAPTURE_STARTED_BY_USER;

				var controller = ScreenCaptureManager.Instance.Controller;
				controller.Initialize();
				controller.StartCapture(shareTarget, reason);
			}
			else
			{
				ScreenCaptureManager.InstanceOrNull?.Controller.StopCapture(eScreenShareSignal.CAPTURE_STOPPED_BY_USER);
			}

			IsListEnabled = false;
			UpdateDisposedState();
		}

		public event Action<bool>? ListToggled;

		private bool _isListEnabled;

		public bool IsListEnabled
		{
			get => _isListEnabled;
			set
			{
				var wasEnabled = _isListEnabled;
				if (wasEnabled == value)
					return;

				_isListEnabled = value;
				ListToggled?.Invoke(value);

				UpdateList();
			}
		}

		private void UpdateList()
		{
			if (IsListEnabled && CheckShareFeatureEnabled())
			{
				var controller = ScreenCaptureManager.Instance.Controller;
				controller.Initialize();
				controller.RequestAllMetadata();
			}

			UpdateDisposedState();
		}

		private void UpdateDisposedState()
		{
			var controller = ScreenCaptureManager.InstanceOrNull?.Controller;
			if (controller == null)
				return;

			if (!IsListEnabled)
			{
				controller.DestroyAllMetadata();

				if (!controller.IsCaptureRequested)
				{
					controller.Dispose();
				}
			}
		}

		private bool CheckShareFeatureEnabled(bool showPopup = true)
		{
			var hasAuthorityCode = MeetingReservationProvider.GetAuthorityCode(Network.User.Instance.CurrentUserData.ID, out var authorityCode);
			var isShareFeatureAvailable = hasAuthorityCode
				? Define.ShareFeatureAvailableAuthorities.Contains(authorityCode)
				: Define.AllowShareFeatureOnNullAuthority;

			if (!isShareFeatureAvailable)
			{
				if (showPopup)
				{
					var authorityCodeName = authorityCode.ToString();
					UIManager.Instance.ShowPopupCommon(Define.String.UI_MeetingRoomSharing_Popup_NoAuthority);
					C2VDebug.LogWarningMethod(nameof(ScreenShareViewModel), $"Your user authority ({authorityCodeName}) does not allow screen sharing.");
				}

				IsListEnabled = false;
				return false;
			}

			return true;
		}
	}
}
