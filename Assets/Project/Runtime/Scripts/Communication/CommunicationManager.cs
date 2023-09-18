/*===============================================================
* Product:		Com2Verse
* File Name:	CommunicationManager.cs
* Developer:	urun4m0r1
* Date:			2023-01-02 18:38
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Communication.Unity;
using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.SoundSystem;
using Com2Verse.Utils;
using JetBrains.Annotations;

namespace Com2Verse.Communication
{
	public sealed class CommunicationManager : Singleton<CommunicationManager>
	{
		public event Action<eCommunicationType>? CommunicationTypeChanged;
		public event Action<int>?                CommunicationTypeChangedInt;

		private eCommunicationType _communicationType = eCommunicationType.DEFAULT;

		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private CommunicationManager()
		{
			AudioMixerGroupIndex.Mute        = eAudioMixerGroup.MUTE.CastInt();
			AudioMixerGroupIndex.LocalVoice  = eAudioMixerGroup.LOCAL_VOICE.CastInt();
			AudioMixerGroupIndex.RemoteVoice = eAudioMixerGroup.REMOTE_VOICE.CastInt();
		}

		public eCommunicationType CommunicationType
		{
			get => _communicationType;
			set => ChangeCommunicationType(value);
		}

		public int CommunicationTypeInt
		{
			get => CommunicationType.CastInt();
			set => CommunicationType = value.CastEnum<eCommunicationType>();
		}

		public void ChangeCommunicationTypeInt(int type)
		{
			ChangeCommunicationType(type.CastEnum<eCommunicationType>());
		}

		public void ChangeCommunicationType(eCommunicationType type)
		{
			if (_communicationType == type)
				return;

			C2VDebug.LogMethod(nameof(CommunicationManager), $"[{CommunicationType}] -> [{type}]");

			_communicationType = type;
			CommunicationTypeChanged?.Invoke(type);
			CommunicationTypeChangedInt?.Invoke(type.CastInt());

			UpdateModuleSettings(type);
		}

		private void UpdateModuleSettings(eCommunicationType type)
		{
			if (type is eCommunicationType.DEFAULT)
				return;

			ModuleManager.Instance.VoiceSettings.ChangeSettings(type, eAudioType.DEFAULT);
			ModuleManager.Instance.CameraSettings.ChangeSettings(type, eVideoType.DEFAULT);
			ModuleManager.Instance.ScreenSettings.ChangeSettings(type, eVideoType.SCREEN);

			ModuleManager.Instance.VoicePublishSettings.ChangeSettings(type, eAudioType.DEFAULT);
			ModuleManager.Instance.CameraPublishSettings.ChangeSettings(type, eVideoType.DEFAULT);
			ModuleManager.Instance.ScreenPublishSettings.ChangeSettings(type, eVideoType.SCREEN);
		}
	}
}
