/*===============================================================
* Product:		Com2Verse
* File Name:	TrackPublisherViewModel.cs
* Developer:	urun4m0r1
* Date:			2022-06-20 12:45
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Communication;
using Com2Verse.Communication.Unity;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("Communication")]
	public sealed class TrackPublisherViewModel : ViewModelBase, IDisposable
	{
		private IPublishableLocalUser? _user;

		[UsedImplicitly] public CommandHandler<bool> SetVoiceState  { get; }
		[UsedImplicitly] public CommandHandler<bool> SetCameraState { get; }
		[UsedImplicitly] public CommandHandler<bool> SetScreenState { get; }

		[UsedImplicitly] public CommandHandler ToggleVoiceState  { get; }
		[UsedImplicitly] public CommandHandler ToggleCameraState { get; }
		[UsedImplicitly] public CommandHandler ToggleScreenState { get; }

		public TrackPublisherViewModel()
		{
			SetVoiceState  = new CommandHandler<bool>(OnSetVoiceState);
			SetCameraState = new CommandHandler<bool>(OnSetCameraState);
			SetScreenState = new CommandHandler<bool>(OnSetScreenState);

			ToggleVoiceState  = new CommandHandler(OnToggleVoiceState);
			ToggleCameraState = new CommandHandler(OnToggleCameraState);
			ToggleScreenState = new CommandHandler(OnToggleScreenState);

			ChannelManager.Instance.ViewModelUserAdded   += OnUserAdded;
			ChannelManager.Instance.ViewModelUserRemoved += OnUserRemoved;

			foreach (var user in ChannelManager.Instance.GetViewModelUsers())
			{
				RegisterCurrentUser(user);
			}

			DeviceManager.Instance.AudioRecorder.DeviceChanged += OnAudioRecorderDeviceChanged;
			DeviceManager.Instance.VideoRecorder.DeviceChanged += OnVideoRecorderDeviceChanged;
		}

		public void Dispose()
		{
			var channelManager = ChannelManager.InstanceOrNull;
			if (channelManager != null)
			{
				channelManager.ViewModelUserAdded   -= OnUserAdded;
				channelManager.ViewModelUserRemoved -= OnUserRemoved;

				foreach (var user in channelManager.GetViewModelUsers())
				{
					UnregisterCurrentUser(user);
				}
			}

			var deviceManager = DeviceManager.InstanceOrNull;
			if (deviceManager != null)
			{
				deviceManager.AudioRecorder.DeviceChanged -= OnAudioRecorderDeviceChanged;
				deviceManager.VideoRecorder.DeviceChanged -= OnVideoRecorderDeviceChanged;
			}
		}

		/// <summary>
		/// 오디오 장치 변경 시 Publish 타깃에 반영
		/// </summary>
		private void OnAudioRecorderDeviceChanged(DeviceInfo prevDevice, int prevIndex, DeviceInfo device, int index)
		{
			var willPublish   = !prevDevice.IsAvailable && device.IsAvailable;
			var willUnpublish = !device.IsAvailable;
			if (willPublish) _user?.Modules.RemoveConnectionBlocker(eTrackType.VOICE, this);
			if (willUnpublish) _user?.Modules.TryAddConnectionBlocker(eTrackType.VOICE, this);
		}

		/// <summary>
		/// 카메라 장치 변경 시 Publish 타깃에 반영
		/// </summary>
		private void OnVideoRecorderDeviceChanged(DeviceInfo prevDevice, int prevIndex, DeviceInfo device, int index)
		{
			var willPublish   = !prevDevice.IsAvailable && device.IsAvailable;
			var willUnpublish = !device.IsAvailable;
			if (willPublish) _user?.Modules.RemoveConnectionBlocker(eTrackType.CAMERA, this);
			if (willUnpublish) _user?.Modules.TryAddConnectionBlocker(eTrackType.CAMERA, this);
		}


		private void OnUserAdded(IChannel channel, IViewModelUser user)
		{
			RegisterCurrentUser(user);
		}

		private void OnUserRemoved(IChannel channel, IViewModelUser user)
		{
			UnregisterCurrentUser(user);
		}

		private void RegisterCurrentUser(IViewModelUser user)
		{
			if (user is not IPublishableLocalUser localUser)
				return;

			_user = localUser;

			localUser.Modules.ConnectionTargetChanged += OnLocalConnectionTargetChanged;
			localUser.Modules.ModuleContentChanged    += OnLocalModuleContentChanged;

			if (!DeviceManager.Instance.AudioRecorder.Current.IsAvailable)
				localUser.Modules.TryAddConnectionBlocker(eTrackType.VOICE, this);

			if (!DeviceManager.Instance.VideoRecorder.Current.IsAvailable)
				localUser.Modules.TryAddConnectionBlocker(eTrackType.CAMERA, this);

			InvokePropertyValueChanged(nameof(IsVoiceConnectionTarget),  IsVoiceConnectionTarget);
			InvokePropertyValueChanged(nameof(IsCameraConnectionTarget), IsCameraConnectionTarget);
			InvokePropertyValueChanged(nameof(IsScreenConnectionTarget), IsScreenConnectionTarget);

			InvokePropertyValueChanged(nameof(IsVoiceEnabled),  IsVoiceEnabled);
			InvokePropertyValueChanged(nameof(IsCameraEnabled), IsCameraEnabled);
			InvokePropertyValueChanged(nameof(IsScreenEnabled), IsScreenEnabled);
		}

		private void UnregisterCurrentUser(IViewModelUser user)
		{
			if (user is not IPublishableLocalUser localUser)
				return;

			_user = null;

			localUser.Modules.ConnectionTargetChanged -= OnLocalConnectionTargetChanged;
			localUser.Modules.ModuleContentChanged    -= OnLocalModuleContentChanged;

			localUser.Modules.RemoveConnectionBlocker(eTrackType.VOICE,  this);
			localUser.Modules.RemoveConnectionBlocker(eTrackType.CAMERA, this);
			localUser.Modules.RemoveConnectionBlocker(eTrackType.SCREEN, this);

			InvokePropertyValueChanged(nameof(IsVoiceConnectionTarget),  IsVoiceConnectionTarget);
			InvokePropertyValueChanged(nameof(IsCameraConnectionTarget), IsCameraConnectionTarget);
			InvokePropertyValueChanged(nameof(IsScreenConnectionTarget), IsScreenConnectionTarget);

			InvokePropertyValueChanged(nameof(IsVoiceEnabled),  IsVoiceEnabled);
			InvokePropertyValueChanged(nameof(IsCameraEnabled), IsCameraEnabled);
			InvokePropertyValueChanged(nameof(IsScreenEnabled), IsScreenEnabled);
		}

		private void OnLocalConnectionTargetChanged(eTrackType trackType, bool isConnectionTarget)
		{
			NotifyTrackStatusChanged(trackType, isConnectionTarget);
		}

		private void OnLocalModuleContentChanged(eTrackType trackType, bool isAvailable)
		{
			NotifyTrackStatusChanged(trackType, isAvailable);
		}

		private void NotifyTrackStatusChanged(eTrackType trackType, bool value)
		{
			switch (trackType)
			{
				case eTrackType.VOICE:
					InvokePropertyValueChanged(nameof(IsVoiceConnectionTarget), IsVoiceConnectionTarget);
					InvokePropertyValueChanged(nameof(IsVoiceEnabled),          IsVoiceEnabled);
					break;
				case eTrackType.CAMERA:
					InvokePropertyValueChanged(nameof(IsCameraConnectionTarget), IsCameraConnectionTarget);
					InvokePropertyValueChanged(nameof(IsCameraEnabled),          IsCameraEnabled);
					break;
				case eTrackType.SCREEN:
					InvokePropertyValueChanged(nameof(IsScreenConnectionTarget), IsScreenConnectionTarget);
					InvokePropertyValueChanged(nameof(IsScreenEnabled),          IsScreenEnabled);
					break;
			}
		}

		public void OnSetVoiceState(bool value)
		{
			SetConnectionTarget(eTrackType.VOICE, value);
		}

		public void OnToggleVoiceState()
		{
			SetConnectionTarget(eTrackType.VOICE, !GetIsModuleConnectionTarget(eTrackType.VOICE));
		}

		public void OnSetCameraState(bool value)
		{
			SetConnectionTarget(eTrackType.CAMERA, value);
		}

		public void OnToggleCameraState()
		{
			SetConnectionTarget(eTrackType.CAMERA, !GetIsModuleConnectionTarget(eTrackType.CAMERA));
		}

		public void OnSetScreenState(bool value)
		{
			SetConnectionTarget(eTrackType.SCREEN, value);
		}

		public void OnToggleScreenState()
		{
			SetConnectionTarget(eTrackType.SCREEN, !GetIsModuleConnectionTarget(eTrackType.SCREEN));
		}

		public void SetConnectionTarget(eTrackType trackType, bool value)
		{
			if (trackType is eTrackType.VOICE && DeviceManager.InstanceOrNull?.AudioRecorder.Current.IsAvailable is null or false)
				return;

			if (trackType is eTrackType.CAMERA && DeviceManager.InstanceOrNull?.VideoRecorder.Current.IsAvailable is null or false)
				return;

			if (value)
				_user?.Modules.RemoveConnectionBlocker(trackType, this);
			else
				_user?.Modules.TryAddConnectionBlocker(trackType, this);
		}

		private bool GetIsModuleConnectionTarget(eTrackType trackType)
		{
			return _user != null && _user.Modules.IsConnectionTarget(trackType);
		}

		private bool GetIsModuleEnabled(eTrackType trackType)
		{
			return _user != null && _user.Modules.IsConnectionTarget(trackType);
		}

#region ViewModelProperties
		[UsedImplicitly] public bool IsVoiceConnectionTarget  => GetIsModuleConnectionTarget(eTrackType.VOICE);
		[UsedImplicitly] public bool IsCameraConnectionTarget => GetIsModuleConnectionTarget(eTrackType.CAMERA);
		[UsedImplicitly] public bool IsScreenConnectionTarget => GetIsModuleConnectionTarget(eTrackType.SCREEN);

		[UsedImplicitly] public bool IsVoiceEnabled  => GetIsModuleEnabled(eTrackType.VOICE);
		[UsedImplicitly] public bool IsCameraEnabled => GetIsModuleEnabled(eTrackType.CAMERA);
		[UsedImplicitly] public bool IsScreenEnabled => GetIsModuleEnabled(eTrackType.SCREEN);
#endregion // ViewModelProperties
	}
}
