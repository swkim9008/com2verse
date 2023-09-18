/*===============================================================
 * Product:		Com2Verse
 * File Name:	SceneModuleViewModel.cs
 * Developer:	urun4m0r1
 * Date:		2022-12-13 14:57
 * History:
 * Documents:
 * Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Com2Verse.Communication;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.SmallTalk;
using Com2Verse.SmallTalk.SmallTalkObject;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("SceneModule")]
	public class SceneModuleViewModel : ViewModelBase, IDisposable
	{
		private bool _isWorldModule;
		private bool _isOfficeModule;
		private bool _isMiceModule;
		private bool _isVoiceModule;
		private bool _isCameraModule;
		private bool _isScreenModule;

		public SceneModuleViewModel()
		{
			SceneManager.Instance.CurrentSceneChanged += OnCurrentSceneChanged;
			OnCurrentSceneChanged(SceneEmpty.Empty, SceneManager.Instance.CurrentScene);

			SmallTalkDistance.Instance.StateChanged += OnSmallTalkDistanceStateChanged;
			OnSmallTalkDistanceStateChanged(SmallTalkDistance.Instance.IsEnabled);

			SmallTalkObjectManager.Instance.ConnectionChanged += OnTriggerSmallTalkConnectionChanged;
			OnTriggerSmallTalkConnectionChanged(SmallTalkObjectManager.Instance.IsConnected);

			AuditoriumController.Instance.SpeakerConnectionChanged += OnAuditoriumSpeakerConnectionChanged;
			OnAuditoriumSpeakerConnectionChanged(AuditoriumController.Instance.CurrentGroupChannel?.IsSpeech ?? false);
		}

		public void Dispose()
		{
			var sceneManager = SceneManager.InstanceOrNull;
			if (sceneManager != null)
			{
				sceneManager.CurrentSceneChanged -= OnCurrentSceneChanged;
			}

			var smallTalkDistance = SmallTalkDistance.InstanceOrNull;
			if (smallTalkDistance != null)
			{
				smallTalkDistance.StateChanged -= OnSmallTalkDistanceStateChanged;
			}

			var triggerSmallTalk = SmallTalkObjectManager.InstanceOrNull;
			if (!triggerSmallTalk.IsUnityNull())
			{
				triggerSmallTalk!.ConnectionChanged -= OnTriggerSmallTalkConnectionChanged;
			}

			var auditoriumController = AuditoriumController.InstanceOrNull;
			if (auditoriumController != null)
			{
				auditoriumController.SpeakerConnectionChanged -= OnAuditoriumSpeakerConnectionChanged;
			}
		}

		private void OnCurrentSceneChanged(SceneBase prevScene, SceneBase currentScene)
		{
			IsWorldModule  = CurrentScene.ServiceType is eServiceType.WORLD;
			IsOfficeModule = CurrentScene.ServiceType is eServiceType.OFFICE;
			IsMiceModule   = CurrentScene.ServiceType is eServiceType.MICE;
			IsVoiceModule  = CurrentScene.UseVoiceModule;
			IsCameraModule = CurrentScene.UseCameraModule;
			IsScreenModule = CurrentScene.UseScreenModule;

			InvokePropertyValueChanged(nameof(ShowVoiceModule),  ShowVoiceModule);
			InvokePropertyValueChanged(nameof(ShowCameraModule), ShowCameraModule);
			InvokePropertyValueChanged(nameof(ShowScreenModule), ShowScreenModule);
		}

		private void OnSmallTalkDistanceStateChanged(bool isEnabled)
		{
			InvokePropertyValueChanged(nameof(ShowVoiceModule),  ShowVoiceModule);
			InvokePropertyValueChanged(nameof(ShowCameraModule), ShowCameraModule);
			InvokePropertyValueChanged(nameof(ShowScreenModule), ShowScreenModule);
		}

		private void OnTriggerSmallTalkConnectionChanged(bool isConnected)
		{
			InvokePropertyValueChanged(nameof(ShowVoiceModule),  ShowVoiceModule);
			InvokePropertyValueChanged(nameof(ShowCameraModule), ShowCameraModule);
			InvokePropertyValueChanged(nameof(ShowScreenModule), ShowScreenModule);
		}

		private void OnAuditoriumSpeakerConnectionChanged(bool isConnected)
		{
			InvokePropertyValueChanged(nameof(ShowVoiceModule),  ShowVoiceModule);
			InvokePropertyValueChanged(nameof(ShowCameraModule), ShowCameraModule);
			InvokePropertyValueChanged(nameof(ShowScreenModule), ShowScreenModule);
		}

#region ViewModelProperties
		/// <summary>
		/// 월드 모듈을 사용하는지 여부
		/// </summary>
		public bool IsWorldModule
		{
			get => _isWorldModule;
			private set => UpdateProperty(ref _isWorldModule, value);
		}

		/// <summary>
		/// 오피스 모듈을 사용하는지 여부
		/// </summary>
		public bool IsOfficeModule
		{
			get => _isOfficeModule;
			private set => UpdateProperty(ref _isOfficeModule, value);
		}

		/// <summary>
		/// MICE 모듈을 사용하는지 여부
		/// </summary>
		public bool IsMiceModule
		{
			get => _isMiceModule;
			private set => UpdateProperty(ref _isMiceModule, value);
		}

		/// <summary>
		/// 음성 모듈을 사용하는지 여부
		/// </summary>
		public bool IsVoiceModule
		{
			get => _isVoiceModule;
			private set => UpdateProperty(ref _isVoiceModule, value);
		}

		/// <summary>
		/// 카메라 모듈을 사용하는지 여부
		/// </summary>
		public bool IsCameraModule
		{
			get => _isCameraModule;
			private set => UpdateProperty(ref _isCameraModule, value);
		}

		/// <summary>
		/// 화면공유 모듈을 사용하는지 여부
		/// </summary>
		public bool IsScreenModule
		{
			get => _isScreenModule;
			private set => UpdateProperty(ref _isScreenModule, value);
		}

		/// <summary>
		/// 음성 모듈 UI를 보여줄지 여부
		/// </summary>
		public bool ShowVoiceModule => IsVoiceModule && (IsDistanceSmallTalk || IsTriggerSmallTalk || IsMeetingRoom || IsAuditoriumSpeaker);

		/// <summary>
		/// 카메라 모듈 UI를 보여줄지 여부
		/// </summary>
		public bool ShowCameraModule => IsCameraModule && (IsDistanceSmallTalk || IsTriggerSmallTalk || IsMeetingRoom || IsAuditoriumSpeaker);

		/// <summary>
		/// 화면공유 모듈 UI를 보여줄지 여부
		/// </summary>
		public bool ShowScreenModule => IsScreenModule && (IsDistanceSmallTalk || IsTriggerSmallTalk || IsMeetingRoom || IsAuditoriumSpeaker);
#endregion // ViewModelProperties

		private void UpdateProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "") where T : unmanaged, IConvertible
		{
			if (EqualityComparer<T>.Default.Equals(storage, value))
				return;

			SetProperty(ref storage, value, propertyName);
		}

		private bool IsDistanceSmallTalk => SmallTalkDistance.InstanceOrNull?.IsEnabled ?? false;

		private bool IsTriggerSmallTalk
		{
			get
			{
				var triggerSmallTalk = SmallTalkObjectManager.InstanceOrNull;
				if (!triggerSmallTalk.IsUnityNull() && triggerSmallTalk!.IsConnected)
					return true;

				return false;
			}
		}

		private bool IsMeetingRoom => CurrentScene.CommunicationType is eSpaceOptionCommunication.MEETING;

		private bool IsAuditoriumSpeaker => AuditoriumController.InstanceOrNull?.CurrentGroupChannel?.IsSpeech ?? false;
	}
}
