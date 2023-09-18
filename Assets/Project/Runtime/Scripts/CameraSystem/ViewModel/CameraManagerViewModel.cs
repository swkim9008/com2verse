/*===============================================================
* Product:		Com2Verse
* File Name:	CameraManagerViewModel.cs
* Developer:	eugene9721
* Date:			2022-10-14 17:34
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.CameraSystem;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Project.CameraSystem;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("Camera")]
	public sealed class CameraManagerViewModel : ViewModelBase, IDisposable
	{
		[UsedImplicitly] public CommandHandler<int> EnableCullingMaskLayer  { get; }
		[UsedImplicitly] public CommandHandler<int> DisableCullingMaskLayer { get; }

		[UsedImplicitly] public CommandHandler SetFollowCamera  { get; }
		[UsedImplicitly] public CommandHandler SetForwardCamera { get; }
		[UsedImplicitly] public CommandHandler SetFixedCamera   { get; }

		[UsedImplicitly] public CommandHandler<string> SetFixedCameraWithKey { get; }

		[UsedImplicitly] public CommandHandler CycleMeetingRoomView { get; }

		[UsedImplicitly] public CommandHandler SetMeetingRoomQuarterView { get; }
		[UsedImplicitly] public CommandHandler SetMeetingRoomScreenView  { get; }
		[UsedImplicitly] public CommandHandler SetMyDeskView             { get; }

		private bool _isAvatarHudEnabled;

		private bool _isFollowCamera = true;
		private bool _isForwardCamera;
		private bool _isFixedCamera;

		private string? _fixedCameraKey;

		public CameraManagerViewModel()
		{
			EnableCullingMaskLayer  = new CommandHandler<int>(OnEnableCullingMaskLayer);
			DisableCullingMaskLayer = new CommandHandler<int>(OnDisableCullingMaskLayer);

			CycleMeetingRoomView = new CommandHandler(OnCycleMeetingRoomView);

			SetFollowCamera  = new CommandHandler(() => IsFollowCamera  = true);
			SetForwardCamera = new CommandHandler(() => IsForwardCamera = true);
			SetFixedCamera   = new CommandHandler(() => IsFixedCamera   = true);

			SetFixedCameraWithKey = new CommandHandler<string>(value => FixedCameraKey = value);

			SetMeetingRoomQuarterView = new CommandHandler(() => IsMeetingRoomQuarterView = true);
			SetMeetingRoomScreenView  = new CommandHandler(() => IsMeetingRoomScreenView  = true);
			SetMyDeskView             = new CommandHandler(() => IsMyDeskView             = true);

			CameraManager.Instance.OnCameraStateChange += OnCameraStateChange;
			RefreshViewModel();
		}

		private void OnCycleMeetingRoomView()
		{
			// ScreenView -> FollowCamera -> ScreenView -> ...
			if (IsMeetingRoomScreenView)
			{
				IsFollowCamera = true;
			}
			else if (IsFollowCamera)
			{
				IsMeetingRoomScreenView  = true;
			}
		}

		public void Dispose()
		{
			var cameraManager = CameraManager.InstanceOrNull;
			if (cameraManager != null)
			{
				cameraManager.OnCameraStateChange -= OnCameraStateChange;
			}
		}

		private void OnCameraStateChange(CameraBase? prevCamera, CameraBase nextCamera)
		{
			RefreshViewModel();
		}

#region StateChange
		public void RefreshViewModel()
		{
			ResetCameraState();

			switch (CameraManager.Instance.CurrentState)
			{
				case eCameraState.FOLLOW_CAMERA:
					_isFollowCamera = true;
					break;
				case eCameraState.FORWARD_CAMERA:
					_isForwardCamera = true;
					break;
				case eCameraState.FIXED_CAMERA:
					_isFixedCamera  = true;
					_fixedCameraKey = FixedCameraManager.Instance.CurrentJigKey;
					break;
			}

			if (_isFollowCamera || _isForwardCamera || _isFixedCamera)
			{
				InvokeAllPropertyValueChanged();
			}
			else
			{
				RestoreFallbackCamera();
			}
		}

		private void ResetCameraState()
		{
			_isFollowCamera  = false;
			_isForwardCamera = false;
			_isFixedCamera   = false;

			_fixedCameraKey = null;
		}

		private void InvokeAllPropertyValueChanged()
		{
			InvokePropertyValueChanged(nameof(IsFollowCamera), IsFollowCamera);
			InvokePropertyValueChanged(nameof(IsForwardCamera), IsForwardCamera);
			InvokePropertyValueChanged(nameof(IsFixedCamera), IsFixedCamera);

			InvokePropertyValueChanged(nameof(FixedCameraKey), FixedCameraKey);

			InvokePropertyValueChanged(nameof(IsMeetingRoomQuarterView), IsMeetingRoomQuarterView);
			InvokePropertyValueChanged(nameof(IsMeetingRoomScreenView),  IsMeetingRoomScreenView);
			InvokePropertyValueChanged(nameof(IsMyDeskView),             IsMyDeskView);
		}

		public void RestoreFallbackCamera()
		{
			IsFollowCamera = true;
		}
#endregion // StateChange

#region ViewModelProperties
		public bool IsAvatarHudEnabled
		{
			get => _isAvatarHudEnabled;
			private set => SetProperty(ref _isAvatarHudEnabled, value);
		}

		public bool IsFollowCamera
		{
			get => _isFollowCamera;
			set
			{
				ResetCameraState();
				_isFollowCamera = value;
				NotifyPropertyValueChanged();
			}
		}

		public bool IsForwardCamera
		{
			get => _isForwardCamera;
			set
			{
				ResetCameraState();
				_isForwardCamera = value;
				NotifyPropertyValueChanged();
			}
		}

		public bool IsFixedCamera
		{
			get => _isFixedCamera;
			set
			{
				ResetCameraState();
				_isFixedCamera  = value;
				_fixedCameraKey = null;
				NotifyPropertyValueChanged();
			}
		}

		public string? FixedCameraKey
		{
			get => _fixedCameraKey;
			set
			{
				ResetCameraState();
				_isFixedCamera  = true;
				_fixedCameraKey = value;
				NotifyPropertyValueChanged();
			}
		}

		private void NotifyPropertyValueChanged()
		{
			UpdateCameraState();
			InvokeAllPropertyValueChanged();
		}

		private void UpdateCameraState()
		{
			if (CameraManager.Instance.MainCamera.IsUnityNull())
				return;

			if (IsFollowCamera)
			{
				OnSetFollowCamera();
			}
			else if (IsForwardCamera)
			{
				OnSetForwardCamera();
			}
			else if (IsFixedCamera)
			{
				OnSetFixedCamera();
			}
			else
			{
				RestoreFallbackCamera();
			}
		}
#endregion // ViewModelProperties

#region ViewModelProperties - JigKey
		public bool IsMeetingRoomQuarterView
		{
			get => FixedCameraKey == CameraJigKey.MeetingQuarterView;
			set => ChangeCameraKeyIfTrue(value, CameraJigKey.MeetingQuarterView);
		}

		public bool IsMeetingRoomScreenView
		{
			get => FixedCameraKey == CameraJigKey.MeetingScreenView;
			set => ChangeCameraKeyIfTrue(value, CameraJigKey.MeetingScreenView);
		}

		public bool IsMyDeskView
		{
			get => FixedCameraKey == CameraJigKey.MyDeskView;
			set => ChangeCameraKeyIfTrue(value, CameraJigKey.MyDeskView);
		}

		private void ChangeCameraKeyIfTrue(bool value, string cameraKey)
		{
			if (value)
			{
				FixedCameraKey = cameraKey;
			}
			else
			{
				IsFixedCamera = false;
			}
		}
#endregion // ViewModelProperties - JigKey

#region CameraController
		private void OnEnableCullingMaskLayer(int layerIndex)
		{
			CameraManager.Instance.OnCullingMaskLayer(layerIndex);
		}

		private void OnDisableCullingMaskLayer(int layerIndex)
		{
			CameraManager.Instance.OffCullingMaskLayer(layerIndex);
		}

		private void OnSetFollowCamera()
		{
			CameraManager.Instance.ChangeState(eCameraState.FOLLOW_CAMERA);
		}

		private void OnSetForwardCamera()
		{
			CameraManager.Instance.ChangeState(eCameraState.FORWARD_CAMERA);
		}

		private void OnSetFixedCamera()
		{
			CameraManager.Instance.ChangeState(eCameraState.FIXED_CAMERA, FixedCameraKey);
		}
#endregion // CameraController
	}
}
