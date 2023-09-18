/*===============================================================
* Product:		Com2Verse
* File Name:	TTSController.cs
* Developer:	ydh
* Date:			2023-01-03 14:03
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using Com2Verse.AssetSystem;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.Sound;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Com2Verse.TTS
{
	public sealed class TTSController : IDisposable
	{
		private CancellationTokenSource _tokenSource;
		private MetaverseAudioSource _audio = null;
		public AudioClip Clip = null;
		
		private HashAlgorithm _hashAlgorithm = null;

		public event Action OnInitialized;
		public event Action OnInitializedAIBot;
		public event Action OnShowBotPopUp;
		public event Action OnHideBotPopUp;
		public event Action<string> SetChatBotDesc;
		public event Action<string> PlayBefore;
		public event Action<string> PlayAfter;
		
		private readonly string _audioPath = DirectoryUtil.GetTempPath("TTS", "Audio");
		
		public HashSet<string> StaticAudiosState = new ();
		public string NowPlayAudioMentKey;
		private StaticAudioInfo[] StaticAudioInfos;
		public Dictionary<string, AudioClip> PlayStaticAudios = new();
		public List<string> AlReadyStaticAudios = new();
		public bool TTSReady { get; set; }

		public struct StaticAudioInfo
		{
			public string Key { get; set; }
			public string Gender { get; set; }
			public string VoiceType { get; set; }
			public string Ment { get; set; }
			public string MentKey { get; set; }
			public string GetClipName() => $"{Key.ToString()}_{GetGenderCode()}_{VoiceType}";
			private string GetGenderCode() => Gender;
		}
		
#region Cache
		private static readonly string TTSRoot = DirectoryUtil.GetTempPath("TTS");
		private static readonly string TTSCacheRoot = $"{TTSRoot}Cache";
		private static readonly string TTSAudioRoot = $"{TTSRoot}Audio";
		private static readonly string TTSCacheInfoPath = $"{TTSCacheRoot}/Info.json";

		[Serializable]
		class TTSCacheData
		{
			public List<TTSCacheInfo> Infos;
			public TTSCacheData()
			{
				Infos = new();
			}
		}
		[Serializable]
		class TTSCacheInfo
		{
			public string FileName { get; set; }
			public string Ment{ get; set; }
			public DateTime LastAccess{ get; set; }
			public string CountryVoiceType{ get; set; }

			public string GetFilePath() => $"{TTSAudioRoot}/{FileName}.mp3";
		}
		
		private void SaveTTSCache()
		{
			if (!Directory.Exists(TTSCacheRoot))
				Directory.CreateDirectory(TTSCacheRoot);

			var json = JsonConvert.SerializeObject(_ttsCache);
			File.WriteAllText(TTSCacheInfoPath, json);
		}

		private TTSCacheData LoadOrCreateTTSCache()
		{
			var ttsCache = LoadTTSCache();
			return ttsCache ?? new TTSCacheData();
		}
		private static TTSCacheData LoadTTSCache()
		{
			if (!Directory.Exists(TTSCacheRoot))
				Directory.CreateDirectory(TTSCacheRoot);

			if (!File.Exists(TTSCacheInfoPath)) return null;
			var json = File.ReadAllText(TTSCacheInfoPath);
			return JsonConvert.DeserializeObject<TTSCacheData>(json);
		}

		private void AddTTSCache(TTSCacheInfo newInfo)
		{
			//파일 네임이 같고, 멘트가 다를 경우 지우고 추가.
			RemoveTTSCache(newInfo.FileName);
			_ttsCache.Infos.Add(newInfo);
		}

		private void RemoveTTSCache(string fileName)
		{
			var findIdx = FindTTSCacheIndex(fileName);
			if (findIdx < 0) return;

			_ttsCache.Infos.RemoveAt(findIdx);
		}

		private void ClearTTSCache()
		{
			var ttsCache = LoadOrCreateTTSCache();
			var now = DateTime.Now;
			foreach (var info in ttsCache.Infos)
			{
				var filePath = info.GetFilePath();
				var timeSpan = now - info.LastAccess;
				if (timeSpan.Days > 10)
				{
					if (File.Exists(filePath))
						File.Delete(filePath);
				}
			}
			SaveTTSCache();
		}

		private static TTSCacheData _ttsCache;
		private static int FindTTSCacheIndex(string fileName) => _ttsCache.Infos.FindIndex(info => info.FileName.Equals(fileName));
		
#endregion Cache

#region Initialize
		public void InitializeAsync()
		{
			OnInitialized?.Invoke();
			
			_tokenSource = new CancellationTokenSource();
			
			_hashAlgorithm ??= HashAlgorithm.Create();
			
			_ttsCache = LoadOrCreateTTSCache();
			if (_ttsCache != null)
				ClearTTSCache();
		}

		public void AudioLoad(GameObject _ttsObj, int mixerGroup)
		{
			if (_audio == null)
			{
				_audio = MetaverseAudioSource.CreateNew(_ttsObj);
				_audio.TargetMixerGroup = mixerGroup;
			}
		}
		
		public void InitAIBot()
		{
			TTSReady = true;
			OnInitializedAIBot?.Invoke();
		}
#endregion Initialize

#region ChatBot
		private void ShowBotPopup()
		{
			OnShowBotPopUp?.Invoke();
		}

		private void HideBotPopup()
		{
			OnHideBotPopUp?.Invoke();
		}
#endregion ChatBot

#region PlayTTS
		public async UniTask PlayTTSAsync(string audioType, int time = 0, Action onAction = null)
		{
			var endTime = Time.realtimeSinceStartup + time;
			await UniTask.WaitUntil(() => endTime < Time.realtimeSinceStartup, cancellationToken: _tokenSource.Token);
					
			await ActiveAudioFadeOutAsync();
					
			await SetStaticAudioAsync(audioType);

			Clip = null;
			if (PlayStaticAudios.TryGetValue(audioType, out Clip) && !AlReadyStaticAudios.Contains(audioType))
			{
				AlReadyStaticAudios.Add(audioType);
				PlayClip();
				
				PlayBefore?.Invoke(audioType);
				
				await UniTask.WaitUntil(() => !_audio.IsReferenceNull() && !_audio.IsPlaying, cancellationToken: _tokenSource.Token);

				PlayAfter?.Invoke(audioType);

				onAction?.Invoke();
			}
		}
		
		public void PlayTTS(string ment, Action endAction = null)
		{
			var fileName =System.Convert.ToBase64String(_hashAlgorithm.ComputeHash(System.Text.Encoding.UTF8.GetBytes($"{ment}_{Secretary.CurrentLanuageName}"))).Replace('/', '!');
            			 
			var cacheFile = _ttsCache.Infos.Find((data) => fileName == data.FileName);
			if (cacheFile == null)
				TTSNetWork(ment, fileName, endAction);
			else
			{
				if (cacheFile.Ment == ment)
					PlayTTSAsync(ment, fileName, endAction).Forget();
				else
					TTSNetWork(ment, fileName, endAction);
			}
		}

		private void TTSNetWork(string ment, string fileName, Action endAction = null)
		{
			TTS.TTSSecretary.TTS(ment, _audioPath, fileName, () =>
			{
				PlayTTSAsync(ment, fileName, endAction).Forget();
				AddTTSCache(new TTSCacheInfo()
				{
					Ment = ment,
					FileName = fileName,
					LastAccess = MetaverseWatch.NowDateTime,
					CountryVoiceType = Secretary.CurrentLanuageName
				});
				SaveTTSCache();
			});
		}
		

		private async UniTask PlayTTSAsync(string ment, string name, Action endAction = null)
		{
			try
			{
				var uwr = UnityWebRequestMultimedia.GetAudioClip($"{_audioPath}/{name}.mp3", AudioType.UNKNOWN);
				await uwr.SendWebRequest();
			
				Clip = DownloadHandlerAudioClip.GetContent(uwr);
			
				PlayClip();
				ShowBotPopup();
				SetDynamicDesc(ment);

				await UniTask.WaitUntil(() => !_audio.IsReferenceNull() && !_audio.IsPlaying, cancellationToken: _tokenSource.Token);
				if (endAction != null)
					endAction();
			
				HideBotPopup();
			}
			catch (Exception e)
			{
				C2VDebug.LogError($"Get TTS Audio Failed..\n{e}");
			}
		}
#endregion PlayTTS

		public void SetStaticAudioInfos(StaticAudioInfo[] infos)
		{
			StaticAudioInfos = infos;
		}

		private async UniTask SetStaticAudioAsync(string audioKey)
		{
			StaticAudioInfo audioInfo = new StaticAudioInfo();
			foreach (var audioValue in StaticAudioInfos)
			{
				if (audioValue.Key == audioKey)
				{
					audioInfo = audioValue;
					break;
				}
			}

			var loadFinsh = false;
			var clipName = $"{audioInfo.GetClipName()}.mp3";

			C2VAddressables.LoadAssetAsync<AudioClip>(clipName).OnCompleted += (operationHandle) =>
			{
				var loadedAsset = operationHandle.Result;
				if (loadedAsset.IsReferenceNull())
					C2VDebug.Log("Clip is None.");
				else
					PlayStaticAudios.TryAdd(audioKey, loadedAsset);

				loadFinsh = true;
			};

			await UniTask.WaitUntil(()=>loadFinsh, cancellationToken: _tokenSource.Token);
		}

		private async UniTask ActiveAudioFadeOutAsync()
		{
			if(_audio.IsReferenceNull())
				return;
			
			if (_audio.IsPlaying)
			{
				await VolumeDownUntilZeroAsync();
			}
		}

		private async UniTask VolumeDownUntilZeroAsync()
		{
			if (_audio.IsReferenceNull())
				return;

			var time = 0f;
			var fadeTime = 1f;
			while (_audio.Volume > 0)
			{
				time += Time.deltaTime / fadeTime;
				_audio.Volume  = Mathf.Lerp(1, 0, time);
				await UniTask.Yield();
			}
		}

		private void PlayClip()
		{
			_audio.Stop();
			_audio.Volume = 1;
			_audio.Loop = false;
			_audio.SetClip(Clip);
			_audio.Play();
		}
		
		public void Pause() => _audio?.Pause();
		
		public void UnPause() => _audio?.UnPause();
		private void Play() => _audio?.Play();
		private void SetClip(AudioClip clip) => _audio?.SetClip(clip);

		public void SetClipPlay(AudioClip clip)
		{
			if (_audio != null)
			{
				SetClip(clip);
				Play();
			}
		}

		public bool Mute
		{
			get {  return _audio.Mute; }
			set { _audio.Mute = value; }
		}
		
		public bool IsPlaying() => _audio.IsPlaying;

		public bool Loop
		{
			get { return _audio.Loop; }
			set { _audio.Loop = value; }
		}

		public void StopTTS()
		{
			ActiveAudioFadeOutAsync().Forget();
			HideBotPopup();
		}

		public void ResetStaticAudio() => StaticAudiosState.Clear();

		public void Dispose()
		{
			_audio = null;
			Clip = null;
			_hashAlgorithm = null;
			
			StaticAudiosState.Clear();
			PlayStaticAudios.Clear();
			_tokenSource?.Cancel();
			_tokenSource?.Dispose();
			_tokenSource = null;
		}

		public void SetStaticDesc(string Key)
		{
			for (int i = 0; i < StaticAudioInfos.Length; ++i)
			{
				if (StaticAudioInfos[i].Key == Key)
				{
					SetChatBotDesc?.Invoke(StaticAudioInfos[i].Ment);
					NowPlayAudioMentKey = StaticAudioInfos[i].MentKey;
					return;
				}
			}
		}
		
		private void SetDynamicDesc(string Ment)
		{
			SetChatBotDesc?.Invoke(Ment);
		}
	}
}
