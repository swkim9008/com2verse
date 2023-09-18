/*===============================================================
* Product:		Com2Verse
* File Name:	DeviceModuleManager.cs
* Developer:	urun4m0r1
* Date:			2022-08-05 16:07
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Communication.Unity;
using Com2Verse.Utils;
using JetBrains.Annotations;

namespace Com2Verse.Communication
{
	public sealed class DeviceModuleManager : Singleton<DeviceModuleManager>
	{
		private readonly ObservableHashSet<VoiceUI>  _voiceUIs  = new();
		private readonly ObservableHashSet<CameraUI> _cameraUIs = new();

		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private DeviceModuleManager()
		{
			_voiceUIs.ItemExistenceChanged  += OnVoiceUIsExistenceChanged;
			_cameraUIs.ItemExistenceChanged += OnCameraUIsExistenceChanged;
		}

		private void OnVoiceUIsExistenceChanged(bool isAnyItemExists)
		{
			if (isAnyItemExists)
			{
				DeviceManager.Instance.AudioRecorder.UseAutoRefresh = true;
				DeviceManager.Instance.AudioPlayer.UseAutoRefresh   = true;
				ModuleManager.Instance.Voice.Input.IsRunning        = true;
			}
			else
			{
				var module = ModuleManager.InstanceOrNull;
				if (module != null)
				{
					module.Voice.Input.IsRunning = false;
				}

				var device = DeviceManager.InstanceOrNull;
				if (device != null)
				{
					device.AudioRecorder.UseAutoRefresh = false;
					device.AudioPlayer.UseAutoRefresh   = false;
				}
			}
		}

		private void OnCameraUIsExistenceChanged(bool isAnyItemExists)
		{
			if (isAnyItemExists)
			{
				DeviceManager.Instance.VideoRecorder.UseAutoRefresh = true;
				ModuleManager.Instance.Camera.Input.IsRunning       = true;
			}
			else
			{
				var module = ModuleManager.InstanceOrNull;
				if (module != null)
				{
					module.Camera.Input.IsRunning = false;
				}

				var device = DeviceManager.InstanceOrNull;
				if (device != null)
				{
					device.VideoRecorder.UseAutoRefresh = false;
				}
			}
		}

		public void TryAddVoiceUI(VoiceUI   voiceUI)  => _voiceUIs.TryAdd(voiceUI);
		public void RemoveVoiceUI(VoiceUI   voiceUI)  => _voiceUIs.Remove(voiceUI);
		public void TryAddCameraUI(CameraUI cameraUI) => _cameraUIs.TryAdd(cameraUI);
		public void RemoveCameraUI(CameraUI cameraUI) => _cameraUIs.Remove(cameraUI);
	}
}
