/*===============================================================
* Product:		Com2Verse
* File Name:	AudioRecordController.cs
* Developer:	ydh
* Date:			2023-03-22 11:19
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.Sound;
using Com2Verse.UI;
using Com2Verse.Utils;
using Com2Verse.WebApi.Service;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace Com2Verse.AudioRecord
{
	public sealed class AudioRecordController : IDisposable
	{
		private MetaverseAudioSource _audio;
		private AudioClip _clip;
		
		private GUIView _audioRecordView;
		public AudioRecordItemViewModel PlayItemViewModel { get; set; }
		private AudioRecordViewModel _audioRecordViewModel;
		public string ObjectId { get; private set; }
		public long SelectAudioBoardSeq { get; set; }
		private string _streamingUrl;
		
		private CancellationTokenSource _audioPlayToken;
		#region Cache
		private static readonly string AudioRecordRoot = DirectoryUtil.GetTempPath("AudioRecord");
		private static readonly string AudioRecordCacheRoot = $"{AudioRecordRoot}Cache";
		private static readonly string AudioRecordAudioRoot = $"{AudioRecordRoot}Audio";
		private static readonly string AudioRecordCacheInfoPath = $"{AudioRecordCacheRoot}/Info.json";

		[Serializable]
		class AudioRecordCacheData
		{
			public List<AudioRecordCacheInfo> Infos;

			public AudioRecordCacheData()
			{
				Infos = new();
			}
		}

		[Serializable]
		class AudioRecordCacheInfo
		{
			public string FileName { get; set; }
			public DateTime LastAccess { get; set; }
			public string GetFilePath() => $"{AudioRecordRoot}/{FileName}.wav";
		}

		private void SaveAudioRecordCache()
		{
			if (!Directory.Exists(AudioRecordCacheRoot))
				Directory.CreateDirectory(AudioRecordCacheRoot);

			var json = JsonConvert.SerializeObject(_audioRecordCache);
			File.WriteAllText(AudioRecordCacheInfoPath, json);
		}

		private AudioRecordCacheData LoadOrCreateAudioRecordCache()
		{
			var audioRecordCache = LoadAudioRecordCache();
			return audioRecordCache ?? new AudioRecordCacheData();
		}

		private static AudioRecordCacheData LoadAudioRecordCache()
		{
			if (!Directory.Exists(AudioRecordCacheRoot))
				Directory.CreateDirectory(AudioRecordCacheRoot);

			if (!File.Exists(AudioRecordCacheInfoPath)) return null;
			var json = File.ReadAllText(AudioRecordCacheInfoPath);
			return JsonConvert.DeserializeObject<AudioRecordCacheData>(json);
		} 

		private void AddAudioRecordCache(AudioRecordCacheInfo newInfo)
		{
			//파일 네임이 같고, 멘트가 다를 경우 지우고 추가.
			RemoveAudioRecordCache(newInfo.FileName);
			_audioRecordCache.Infos.Add(newInfo);
		}

		private void RemoveAudioRecordCache(string fileName)
		{
			var findIdx = FindAudioRecordCacheIndex(fileName);
			if (findIdx < 0) return;

			_audioRecordCache.Infos.RemoveAt(findIdx);
		}

		private void ClearAudioRecordCache()
		{
			var now = DateTime.Now;
			for (int i = _audioRecordCache.Infos.Count - 1; i >= 0; --i)
			{
				var filePath = _audioRecordCache.Infos[i].GetFilePath();
				var monthTimeSpan = now.Month - _audioRecordCache.Infos[i].LastAccess.Month;
				var dayTimeSpan = now.Day - _audioRecordCache.Infos[i].LastAccess.Day;
				if (dayTimeSpan != 0 || monthTimeSpan != 0)
				{
					if (File.Exists(filePath))
					{
						File.Delete(filePath);
						_audioRecordCache.Infos.RemoveAt(i);
					}
				}
			}

			SaveAudioRecordCache();
		}

		private static AudioRecordCacheData _audioRecordCache;
		private static int FindAudioRecordCacheIndex(string fileName) => _audioRecordCache.Infos.FindIndex(info => info.FileName.Equals(fileName ));

#endregion Cache

		public void Initialize(GameObject _audioRecordObj)
		{
			_audioRecordCache = LoadOrCreateAudioRecordCache();
			if (_audioRecordCache != null)
				ClearAudioRecordCache();
			
			if (_audio.IsReferenceNull())
			{
				_audio = MetaverseAudioSource.CreateNew(_audioRecordObj);
				_audio.TargetMixerGroup = (int)SoundSystem.eAudioMixerGroup.AUDIORECORD;	
			}
		}

		public void ViewInitialize(GUIView view, AudioRecordViewModel viewModel)
		{
			_audioRecordView = view;
			_audioRecordViewModel = viewModel;
		}

		public void AudioRecordRefresh(Action exitAction, Action popUpOpen, Action audioRecordSort, string objectId)
		{
			_audioRecordViewModel?.Refresh(exitAction, popUpOpen, audioRecordSort);
			PlayItemViewModel = null;
			ObjectId = objectId;
		}
		
		private async UniTask AudioStreaming(long boardSeq)
		{
			using var request = UnityWebRequestMultimedia.GetAudioClip(_streamingUrl, AudioType.WAV);
			if (request?.downloadHandler == null)
				return;
			
			(request.downloadHandler as DownloadHandlerAudioClip).streamAudio = true;

			await request.SendWebRequest();
			var audioClip = DownloadHandlerAudioClip.GetContent(request);

			switch (audioClip.loadState)
			{
				case AudioDataLoadState.Loaded:
					C2VDebug.Log($"AudioStreaming LOADED - {audioClip.length}");
					_clip = audioClip;
					
					AudioRecordSetClipAndPlay(boardSeq);
					AudioStateChange(boardSeq, true);
					break;
				case AudioDataLoadState.Failed:	
					UIManager.Instance.SendToastMessage("UI_Office_Voicemessage_Today_Error", 3f, UIManager.eToastMessageType.WARNING);
					C2VDebug.Log("AudioStreaming FAILED");
					break;
				case AudioDataLoadState.Unloaded:
					UIManager.Instance.SendToastMessage("UI_Office_Voicemessage_Today_Error", 3f, UIManager.eToastMessageType.WARNING);
					C2VDebug.Log("AudioStreaming UNLOADED");
					break;
			}
		}
		
		private void AudioRecordSetClipAndPlay(long boardSeq)
		{
			_audio.Stop();
			_audio.Volume = 1;
			_audio.Loop = false;
			_audio.SetClip(_clip);
			_audio.Play();
			
			AudioFinishAsync(boardSeq).Forget();
		}
		
		private void AudioStateChange(long boardSeq, bool onOff)
		{
			foreach (var item in _audioRecordViewModel.AudioRecordItemCollection.Value)
			{
				if (item.AudioRecordInfo.BoardSeq == boardSeq)
				{
					if (onOff)
					{
						item.PlayImageFillAmount = 0;
						item.IsPlayActive = true;

						AudioPlaying(boardSeq).Forget();
					}

					break;
				}
			}
		}

		private async UniTask AudioPlaying(long boardSeq)
		{
			while (AudioPlaying())
			{
				await UniTask.Delay(1, cancellationToken: _audioPlayToken.Token).SuppressCancellationThrow();

				SetFillAmount(boardSeq);
			}

			AudioStopReset(boardSeq);
		}
		
		public void AudioStopReset(long boardSeq)
		{
			AudioSelectReturn(boardSeq);
			SelectAudioBoardSeq = -1;
		}
		
		private async UniTask AudioFinishAsync(long boardSeq)
		{
			await UniTask.WaitUntil(() => !_audio.IsPlaying);

			 foreach (var item in _audioRecordViewModel.AudioRecordItemCollection.Value)
			{
				if (item.AudioRecordInfo.BoardSeq == boardSeq)
				{
					item.StateText = Localization.Instance.GetString("UI_Office_Voicemessage_System_0004");
					item.PlayImageFillAmount = 1;
					break;
				}
			}
		}
		
		public void AudioPlay(long boardSeq) => AudioStreaming(boardSeq).Forget();
		public void AudioPause() => _audio.Pause();
		public void AudioStop() => _audio.Stop();
		private bool AudioPlaying() => _audio.IsPlaying;
		public void RecordToggleOnOff(bool value) => _audioRecordViewModel.RecordButtonToggleOn = value;

		public void Hide()
		{
			_audioRecordView.Hide();
			PlayItemViewModel = null;
		}
		
		public void  AudioItemSelect(long boardSeq, Action<long> action)
		{
			foreach (var item in _audioRecordViewModel.AudioRecordItemCollection.Value)
			{
				if (item.AudioRecordInfo.BoardSeq == boardSeq)
				{
					AudioItemSelectAsync(item, action).Forget();
					break;
				}
			}
		}

		private async UniTask AudioItemSelectAsync(AudioRecordItemViewModel item, Action<long> action)
		{
			_streamingUrl = item.AudioRecordInfo.FilePath;
				
			if (PlayItemViewModel != null)
				PlayItemViewModel.IsPlayActive = false;
				
			item.AnimationPropertyExtensions.SetFuntion("UI_AudioRecord_PlayOpen", item.AudioStateEvent);
			item.AnimationPropertyExtensions.AnimationPlay = true;
			item.StateText = Localization.Instance.GetString("UI_Office_Voicemessage_System_0003");
			item.PlayImageFillAmount = 0;
				
			AudioPause();
			action?.Invoke(item.AudioRecordInfo.BoardSeq);
		}
		
		public void AudioSelectReturn(long boardSeq)
		{
			foreach (var item in _audioRecordViewModel.AudioRecordItemCollection.Value)
			{
				if (item.AudioRecordInfo.BoardSeq == boardSeq)
				{
					item.AnimationPropertyExtensions.SetFuntion("UI_AudioRecord_PlayClose", null);
					item.AnimationPropertyExtensions.AnimationPlay = true;
					break;
				}
			}
		}

		public void AddRecordItem(AudioRecordItemViewModel item) => _audioRecordViewModel.AudioRecordItemCollection.AddItem(item);
		private void RemoveItem(AudioRecordItemViewModel viewModel) => _audioRecordViewModel.AudioRecordItemCollection.RemoveItem(viewModel);
		public void RemoveAllItem() => _audioRecordViewModel.AudioRecordItemCollection.Reset();
		public void HasItem(bool value) => _audioRecordViewModel.AudioRecordItemCollectionIsIn = value;
		
		private void SetFillAmount(long boardSeq)
		{
			if (!_audio.IsPlaying)
				return;
			
			if (PlayItemViewModel == null || PlayItemViewModel.AudioRecordInfo.BoardSeq != boardSeq)
			{
				foreach (var item in _audioRecordViewModel.AudioRecordItemCollection.Value)
				{
					if (item.AudioRecordInfo.BoardSeq == boardSeq)
					{
						PlayItemViewModel = item;
						break;
					}
				}	
			}

			PlayItemViewModel.PlayImageFillAmount = Mathf.Lerp(0f, 1, _audio.AudioSource.time / _clip.length);
		}

		public void AudioRecordItemDelete(long boardSequence)
		{
			foreach (var item in _audioRecordViewModel.AudioRecordItemCollection.Value)
			{
				if (item.AudioRecordInfo.BoardSeq == boardSequence)
				{
					RemoveItem(item);
					break;
				}
			}

			_audioRecordViewModel.AudioRecordItemCollectionIsIn = _audioRecordViewModel.AudioRecordItemCollection.CollectionCount > 0;
		}

		public async UniTask AudioRecordItemLike(long boardSequence, bool like)
		{
			foreach (var item in _audioRecordViewModel.AudioRecordItemCollection.Value)
			{
				if (item.AudioRecordInfo.BoardSeq == boardSequence)
				{
					if (like)
					{
						var respnse = await Api.VoiceBoard.PostVoiceBoardRecommendVoiceBoardPost(new Components.RecommendVoiceBoardPostRequest() {BoardSeq = boardSequence});
						if (respnse.Value.Code == Components.OfficeHttpResultCode.Success)
						{
							item.AudioRecordInfo.RecommendCount = respnse.Value.Data;
							item.AudioRecordInfo.RecommendAvailable = !like;
						}
					}
					else
					{
						var respnse = await Api.VoiceBoard.PostVoiceBoardCancelRecommendVoiceBoardPost(new Components.RecommendVoiceBoardPostRequest() {BoardSeq = boardSequence });
						if (respnse.Value.Code == Components.OfficeHttpResultCode.Success)
						{
							item.AudioRecordInfo.RecommendCount = respnse.Value.Data;
							item.AudioRecordInfo.RecommendAvailable = !like;
						}
					}

					item.ItemInfoRefresh();
					return;
				}
			}
		}
		
		public Collection<AudioRecordItemViewModel> GetCollection() => _audioRecordViewModel.AudioRecordItemCollection;

		public void AudioPlayTokenReset(bool isvalidate)
		{
			if (_audioPlayToken != null)
			{
				_audioPlayToken.Cancel();
				_audioPlayToken = null;
			}

			if(isvalidate)
				_audioPlayToken = new CancellationTokenSource();
		}
		
		public void Dispose()
		{
			_clip = null;
			_audio = null;

			AudioPlayTokenReset(false);
		}

		public void MaxUpload(bool value)
		{
			_audioRecordViewModel.MaxUploadToggleEnable = !value;
			_audioRecordViewModel.RecordButtonToggleOn = !value;
		}
	}
}