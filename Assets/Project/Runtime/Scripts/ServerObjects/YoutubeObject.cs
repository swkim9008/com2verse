/*===============================================================
* Product:		Com2Verse
* File Name:	YoutubeObject.cs
* Developer:	jhkim
* Date:			2023-05-23 20:41
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Extension;
using Com2Verse.SoundSystem;
using Com2Verse.WebView;
using Cysharp.Threading.Tasks;
using UnityEngine;
using SoundManager = Com2Verse.SoundSystem.SoundManager;

namespace Com2Verse.Network
{
	public sealed class YoutubeObject : MonoBehaviour, IDisposable
	{
#region Variables
		[SerializeField] private Renderer _renderer;
		private YoutubeWebController Controller { get; set; }
		private Material _originalMaterial = null;
		private bool _isCreating = false;
#endregion // Variables

#region Properties
		public bool HasController => Controller != null;
#endregion // Properties

#region Initialize
		public void Create(string videoId, bool mute = true, Action onCreated = null)
		{
			if (HasController || _isCreating) return;

			_isCreating = true;

			KeepOriginalMaterial();

			RegisterAudioEvents();

			YoutubeWebController.CreateAsync(GetSetting(), videoId, _renderer, controller =>
			{
				Controller = controller;
				UpdateAudioVolume();
				onCreated?.Invoke();
				_isCreating = false;
			}).Forget();

			YoutubeWebModel.Settings GetSetting()
			{
				var setting = mute ? YoutubeWebModel.Settings.Default : YoutubeWebModel.Settings.NoMuteDefault;
				setting.Volume = GetAudioVolume();
				return setting;
			}
		}
#endregion // Initialize

#region Public Functions
		public void SetRenderer(Renderer render)  => _renderer = render;

		public void Play(string videoId, bool mute = true)
		{
			KeepOriginalMaterial();
			Controller?.PlayAsync(videoId, _renderer, UpdateAudioVolume);
			Controller?.SetMuteAsync(mute);
		}
		public void Pause() => Controller?.PauseAsync().Forget();
		public void Stop()
		{
			RecoverMaterial();
			_originalMaterial = null;
			Pause();
		}
		public void Resume()                => Controller?.ResumeAsync().Forget();
		public void SetVolume(float volume) => Controller?.SetVolumeAsync(volume).Forget();
#endregion // Public Functions

#region Material
		private void KeepOriginalMaterial()
		{
			if (_originalMaterial.IsReferenceNull())
				_originalMaterial = _renderer.material;
		}
		private void RecoverMaterial()
		{
			if (!_renderer.IsUnityNull() && !_originalMaterial.IsReferenceNull())
				_renderer.material = _originalMaterial;
		}
#endregion // Material

#region Audio Events
		private void RegisterAudioEvents()
		{
			UnregisterAudioEvents();
			SoundManager.OnVolumeChanged += OnVolumeChanged;
		}

		private void UnregisterAudioEvents()
		{
			SoundManager.OnVolumeChanged -= OnVolumeChanged;
		}

		private void OnVolumeChanged(int mixerGroup, float _)
		{
			if (!Enum.TryParse<eAudioMixerGroup>(Convert.ToString(mixerGroup), out var value)) return;

			switch (value)
			{
				case eAudioMixerGroup.MASTER:
				case eAudioMixerGroup.SFX:
				case eAudioMixerGroup.BGM:
				{
					UpdateAudioVolume();
				}
					break;
			}
		}

		private void UpdateAudioVolume()
		{
			var volume = GetAudioVolume();
			SetVolume(volume);
		}

		private float GetAudioVolume()
		{
			var masterVolume = Sound.SoundManager.Instance.GetVolume((int) eAudioMixerGroup.MASTER);
			var bgmVolume = Sound.SoundManager.Instance.GetVolume((int) eAudioMixerGroup.BGM);
			return masterVolume * bgmVolume;
		}
#endregion // Audio Events

#region Dispose
		public void OnDestroy()
		{
			Dispose();
		}

		public void Dispose()
		{
			UnregisterAudioEvents();

			Controller?.Dispose();
			Controller = null;

			RecoverMaterial();
			_originalMaterial = null;
		}
#endregion // Dispose
	}
}
