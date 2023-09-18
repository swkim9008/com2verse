/*===============================================================
* Product:		Com2Verse
* File Name:	AudioRecordManager.cs
* Developer:	ydh
* Date:			2023-03-21 11:00
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Com2Verse.Chat;
using Com2Verse.Communication.Unity;
using Com2Verse.Extension;
using Com2Verse.HttpHelper;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.Organization;
using Com2Verse.StorageApi;
using Com2Verse.UI;
using Com2Verse.Utils;
using Com2Verse.WebApi.Service;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using User = Com2Verse.Network.User;

namespace Com2Verse.AudioRecord
{
	public sealed class AudioRecordManager : Singleton<AudioRecordManager>, IDisposable
	{
		[UsedImplicitly]
		private AudioRecordManager() { }

		private int _clockTimeSec;
		private int _samplesPerSec;
		private bool _isCreateDateTimeSort;

		private AudioRecordController _controller;
		private AudioRecordPopupController _popUpController;

		private GameObject _audioRecordObj;

		private readonly string _audioRecordPopUp = "UI_AudioRecordPopUp";
		private readonly string _aAudioRecord = "UI_AudioRecord";
		private readonly string _filePath = "/Com2verse/AudioRecord";
		private readonly int _maxUploadCount = 3;
		private readonly int _recordTimeSec = 30;

		private void ControllerInitialized()
		{
			_controller ??= new AudioRecordController();
			_controller.Initialize(_audioRecordObj);

			_popUpController ??= new AudioRecordPopupController();
			_popUpController.Initialize();
			_popUpController.RecordCancelAction += AudioPopUpDisable;
			_popUpController.RecordFinshAction += AudioRecordFinsh;
			_popUpController.RecordingAction += AudioRecording;
		}

		public void OpenAudioRecord(string objectId)
		{
			UIManager.Instance.CreatePopup(_aAudioRecord, view =>
			{
				view.Show();
				_isCreateDateTimeSort = true;
				if (_audioRecordObj.IsUnityNull())
				{
					_audioRecordObj = new GameObject("AudioRecord");
					GameObject.DontDestroyOnLoad(_audioRecordObj);
				}

				if (_controller == null)
					ControllerInitialized();

				_controller?.ViewInitialize(view, view.ViewModelContainer.GetViewModel<AudioRecordViewModel>());
				_controller?.AudioRecordRefresh(AudioRecordExit, AudioRecordPopUpOpen, AudioRecordSort, objectId);

				GetAudioRecordDataAsync().Forget();

				ChatManager.Instance.OnAudioRecord -= OnAudioRecord;
				ChatManager.Instance.OnAudioRecord += OnAudioRecord;
			}).Forget();
		}

		private async UniTask GetAudioRecordDataAsync()
		{
			var response = await Api.VoiceBoard.PostVoiceBoardGetVoiceBoard(new Components.GetVoiceBoardRequest() { ObjectId = _controller.ObjectId.ToString()});
			if (response == null || response.Value == null)
			{
				NetworkUIManager.Instance.ShowWebApiErrorMessage(0);
			}
			else if (response.Value.Code == Components.OfficeHttpResultCode.Success)
			{
				_controller.RemoveAllItem();
				_controller.HasItem(response.Value.Data.Length > 0);

				var maxUploadCheckCount = 0;
				for (var i = 0; i < response.Value.Data.Length; i++)
				{
					var info = new AudioRecordInfo();
					info.AccountId = response.Value.Data[i].AccountId;

					var employee = await DataManager.Instance.GetMemberAsync(info.AccountId);
					info.RecordName = employee.Member.MemberName;

					info.IsMine = info.AccountId == User.Instance.CurrentUserData.ID;
					info.BoardSeq = response.Value.Data[i].BoardSeq;

					var splitStr = response.Value.Data[i].FilePath.Split('/');

					info.FileName = splitStr.Last();
					info.FilePath = response.Value.Data[i].FilePath;
					info.ObjectId = response.Value.Data[i].ObjectId;
					info.RecommendAvailable = response.Value.Data[i].RecommendAvailable == "Y";
					info.RecommendCount = response.Value.Data[i].RecommendCount;
					info.CreateDateTime = response.Value.Data[i].CreateDateTime;
					info.ObjectId = response.Value.Data[i].ObjectId;

					var item = new AudioRecordItemViewModel(info);
					item.AudioRecordPlayAction = AudioPlay;
					item.AudioRecordDeleteAction = AudioRecordItemDelete;
					item.AudioRecordStopAction  = AudioStop;
					item.AudioRecordSelectAction = AudioItemSelect;
					item.AudioRecordLikeAction = AudioRecordItemLikeSet;
					item.IsPlayActive = false;

					_controller.AddRecordItem(item);

					if (info.IsMine)
						maxUploadCheckCount++;
				}

				_controller.MaxUpload(maxUploadCheckCount >= _maxUploadCount);
			}
			else
			{
				NetworkUIManager.Instance.ShowWebApiErrorMessage(Components.OfficeHttpResultCode.Fail);
			}
		}

		private void AudioPlay(long boardSeq) => _controller.AudioPlay(boardSeq);
		public void AudioPause() => _controller.AudioPause();

		private void AudioStop(long boardSeq)
		{
			_controller.AudioStop();
			_controller.AudioStopReset(boardSeq);
		}

		private void RecordStart()
		{
			_popUpController.ClockTimeSec = 0;
			_popUpController.RecordFillamount(0);

			AudioRecordAsync().Forget();
		}

		private void RecordStop() => _popUpController.Recording = false;
		private void RecordSavePopUp() => _popUpController.RecrodSavePopUp();

		private void RecordStopAndSave()
		{
			RecordStop();
			RecordSavePopUp();
			_popUpController.StopRecordAnimation();
		}

		private void RecordExit()
		{
			_controller.RecordToggleOnOff(true);

			AudioPopUpDisable();

			_popUpController.ExitButtonClick = true;
			_popUpController.Recording = false;
		}


		private async UniTask AudioRecordAsync()
		{
			if (!DeviceManager.Instance.AudioRecorder.Current.IsAvailable)
			{
				_popUpController.AudioRecordPopUpViewModel.IsDisConnectMic = true;
				return;
			}

			var wasRunning = ModuleManager.Instance.Voice.IsRunning;
			ModuleManager.Instance.Voice.IsRunning = false;

			await UniTaskHelper.WaitUntil(() => ModuleManager.Instance.Voice.AudioSource.IsUnityNull());
			
			if (_popUpController.AudioRecordPopUpViewModel != null)
			{
				_popUpController.IsRecordBtnState(false);
				_popUpController.IsRecActive(true);
			}

			_popUpController.Recording = true;
			_popUpController.AudioRecordPopUpViewModel.RecordAnimationEnable = true;
			await _popUpController.AudioRecordAsync(DeviceManager.Instance.AudioRecorder.CurrentUnityDevice);
			
			ModuleManager.Instance.Voice.IsRunning = wasRunning;
		}

		private void AudioRecordFinsh(AudioClip mirClip)
		{
			_popUpController.StopRecordAnimation();

			var popupAddress = "UI_Popup_YN_Title";
			UIManager.Instance.CreatePopup(popupAddress, view =>
			{
				view.Show();

				var commonPopupViewModel = view.ViewModelContainer.GetViewModel<CommonPopupYesNoViewModel>();
				commonPopupViewModel.GuiView = view;
				commonPopupViewModel.Context = Localization.Instance.GetString("UI_Office_Voicemessage_System_0005");
				commonPopupViewModel.Title = Localization.Instance.GetString("UI_Common_Btn_OK");
				commonPopupViewModel.Yes = Localization.Instance.GetString("UI_Office_Voicemessage_Btn_0002");
				commonPopupViewModel.No = Localization.Instance.GetString("UI_Common_Btn_Cancel");
				commonPopupViewModel.OnYesEvent = view =>
				{
					float[] samples = new float[mirClip.samples * mirClip.channels];

					mirClip.GetData(samples, 0);

					AudioClip clip = AudioClip.Create("NewClip", samples.Length / _recordTimeSec * (int)_popUpController.ClockTimeSec, mirClip.channels, mirClip.frequency, false);

					clip.SetData(samples, 0);

					UploadAudioAsync(clip).Forget();
				};

				view.OnClosedEvent += guiView => RecordExit();
			});
		}

		private async UniTask UploadAudioAsync(AudioClip clip)
		{
			var bytes = StorageApi.Util.GetBytes(clip);

			if (!StorageApi.Util.TryGenerateHash(bytes, out var hash)) return;

			var fileName      = $"{_popUpController.CreateName(User.Instance.CurrentUserData.ID + ServerTime.Time.ToString())}.wav";
			var uploadRequest = new Components.UploadFileRequest();
			uploadRequest.GroupId = DataManager.Instance.GroupID;
			uploadRequest.Path = _filePath;
			uploadRequest.FileName = fileName;
			uploadRequest.Md5 = hash;
			uploadRequest.Size = bytes.Length;
			uploadRequest.DirectoryPathType = Components.DirectoryPathUserType.VoiceBoard;
			var response = await Api.FileStorage.PostFileStorageUploadFile(uploadRequest);
			if (response == null)
			{
				UploadFailPopup(true);
				return;
			}

			if (response.StatusCode == HttpStatusCode.OK && response.Value.Code == Components.OfficeHttpResultCode.Success)
			{
				var fileUpload = await Helper.WebRequestUploadFileAsync(response.Value.Data.Url, fileName, bytes);
				if (fileUpload.StatusCode == HttpStatusCode.OK)
				{
					var urlRequest = new Components.GetFileDownloadUrlRequest();
					urlRequest.CallbackKey = response.Value.Data.CallbackKey;
					urlRequest.Path = response.Value.Data.Path;
					urlRequest.FileName = response.Value.Data.File;
					urlRequest.GroupId = DataManager.Instance.GroupID;
					urlRequest.DirectoryPathType = Components.DirectoryPathUserType.VoiceBoard;
					var responseUrl = await Api.FileStorage.PostFileStorageGetFileDownloadUrl(urlRequest);
					if (responseUrl == null)
					{
						UploadFailPopup(true);
						return;
					}

					if (responseUrl.StatusCode == HttpStatusCode.OK && responseUrl.Value.Code == Components.OfficeHttpResultCode.Success)
					{
						var enrollVoiceBoardPost = await Api.VoiceBoard.PostVoiceBoardEnrollVoiceBoardPost(
							new Components.EnrollVoiceBoardPostRequest()
							{
								FilePath = responseUrl.Value.Data.DownloadUrl,
								ObjectId = _controller.ObjectId,
								DirectoryPath = responseUrl.Value.Data.DirectoryPath,
								FileName = responseUrl.Value.Data.FileName
							});

						if (enrollVoiceBoardPost == null)
						{
							UploadFailPopup(true);
							return;
						}

						if (enrollVoiceBoardPost.StatusCode == HttpStatusCode.OK)
						{
							if (enrollVoiceBoardPost.Value.Code == Components.OfficeHttpResultCode.Success)
							{
								ChatManager.Instance.BroadcastCustomNotify(ChatManager.CustomDataType.AUDIO_RECORD);
								GetAudioRecordDataAsync().Forget();
								_popUpController.Hide();
							}
						}
						else
						{
							UploadFail($"PostVoiceBoardEnrollVoiceBoardPost fail.");
						}
					}
					else
					{
						UploadFail($"Url get fail");
					}
				}
				else
				{
					UploadFail($"file web upload fail.");
				}
			}
			else
			{
				UploadFail($"PostFileStorageGetFileDownloadUrl fail");
			}
		}

		private void UploadFail(string logMent)
		{
			RecordExit();
			C2VDebug.Log(logMent);
			NetworkUIManager.Instance.ShowWebApiErrorMessage(Components.OfficeHttpResultCode.Fail);
		}

		private void UploadFailPopup(bool isNull)
		{
			RecordExit();
			NetworkUIManager.Instance.ShowWebApiErrorMessage((isNull) ? 0 : Components.OfficeHttpResultCode.Fail);
		}

		private void OnAudioRecord(long id)
		{
			OnAudioRecordAsync(id).Forget();
		}

		private async UniTask OnAudioRecordAsync(long id)
		{
			var memberModel = await DataManager.Instance.GetMemberAsync(id);
			if (memberModel == null)
			{
				return;
			}

			ChatManager.Instance.SendSystemMessage(string.Format(Localization.Instance.GetString("UI_Chat_System_Voice_Chat_Msg"), memberModel.Member.MemberName));
		}

		private void AudioRecording()
		{
			_popUpController.RecordingTime(_popUpController.ClockTimeSec < 10
				? $"00 : 0{Math.Floor(_popUpController.ClockTimeSec):0.}"
				: $"00 : {Math.Floor(_popUpController.ClockTimeSec):0.}");

			_popUpController.RecordFillamount(Mathf.Lerp(0f, 1, _popUpController.ClockTimeSec / 30));
		}

		public void Dispose()
		{
			_controller = null;
			_popUpController = null;

			if (!_audioRecordObj.IsUnityNull())
			{
				UnityEngine.Object.Destroy(_audioRecordObj);
				_audioRecordObj = null;
			}
		}

		private void AudioPopUpDisable()
		{
			_popUpController?.AudioRecordPopUpViewModel?.Registerer?.HideComplete();
		}

		private void AudioRecordExit()
		{
			_controller.AudioStop();
			_controller.SelectAudioBoardSeq = -1;
			RecordExit();

			_popUpController.AudioRecordPopUpViewModel = null;
			_popUpController.AudioRecordPopUpView = null; 
			ChatManager.Instance.OnAudioRecord -= OnAudioRecord;
		}

		private void AudioRecordPopUpOpen()
		{
			if (_popUpController.AudioRecordPopUpViewModel == null)
			{
				UIManager.Instance.CreatePopup(_audioRecordPopUp, view =>
				{
					view.Show();

					var popup = view.ViewModelContainer.GetViewModel<AudioRecordPopupViewModel>();
					popup.Refresh();
					popup.RecordStartAction = RecordStart;
					popup.RecordStopAction = RecordStopAndSave;
					popup.RecordExitAction = RecordExit;

					_popUpController.ViewInitialize(view, popup);
					_popUpController.ExitButtonClick = false;
				}).Forget();
			}
			else
			{
				_popUpController.ViewRefreshView();
			}

			_controller?.AudioStop();
			_controller?.AudioStopReset(_controller.SelectAudioBoardSeq);
		}

		private void AudioRecordItemLikeSet(long boardSequence, bool like)
		{
			_controller.AudioRecordItemLike(boardSequence, like).Forget();
		}

		private void AudioRecordItemDelete(long boardSequence)
		{
			UIManager.Instance.ShowPopupYesNo(Localization.Instance.GetString("UI_Common_Btn_OK"), Localization.Instance.GetString("UI_Office_Voicemessage_Popup_System_0002"), view =>
			{
				AudioRecordItemDeleteAsync(boardSequence).Forget();
			}, null, null, Localization.Instance.GetString("UI_Office_Voicemessage_Popup_System_0003"), Localization.Instance.GetString("UI_Common_Btn_Cancel"));
		}

		private async UniTask AudioRecordItemDeleteAsync(long boardSequence)
		{
			var respnse = await Api.VoiceBoard.PostVoiceBoardDeleteVoiceBoardPost(new Components.DeleteVoiceBoardPostRequest()
			{
				ObjectId = _controller.ObjectId.ToString(),
				BoardSeq = boardSequence
			});

			if (respnse.Value?.Code == Components.OfficeHttpResultCode.Success)
			{
				_controller.AudioRecordItemDelete(boardSequence);
				_controller.MaxUpload(false);
			}
		}

		private bool _isAniPlay = false;
		private void AudioItemSelect(long boardSeq)
		{
			if (boardSeq == _controller.SelectAudioBoardSeq || _isAniPlay)
				return;

			_isAniPlay = true;

			_controller.AudioItemSelect(boardSeq, (boardSeq) =>
			{
				if (_controller.SelectAudioBoardSeq != -1)
					AudioSelectReturn(_controller.SelectAudioBoardSeq);

				_controller.SelectAudioBoardSeq = boardSeq;
				_isAniPlay = false;
				_controller.AudioPlayTokenReset(true);
			});
		}

		private void AudioSelectReturn(long boardSeq)
		{
			_controller.AudioSelectReturn(boardSeq);
		}

		private void AudioRecordSort()
		{
			_controller.AudioStop();
			_controller.SelectAudioBoardSeq = -1;
			_controller.PlayItemViewModel = null;

			List<AudioRecordItemViewModel> sortList;
			_isCreateDateTimeSort = !_isCreateDateTimeSort;
			if(_isCreateDateTimeSort)
				sortList = new(_controller.GetCollection().Value.OrderByDescending(data => data.AudioRecordInfo.CreateDateTime));
			else
				sortList = new(_controller.GetCollection().Value.OrderByDescending(data => data.AudioRecordInfo.RecommendCount));

			_controller.RemoveAllItem();
			foreach (var value in sortList)
			{
				AudioRecordItemViewModel item = new AudioRecordItemViewModel(value.AudioRecordInfo);
				item.AudioRecordPlayAction =  AudioPlay;
				item.AudioRecordDeleteAction = AudioRecordItemDelete;
				item.AudioRecordStopAction  = AudioStop;
				item.AudioRecordSelectAction = AudioItemSelect;
				item.AudioRecordLikeAction = AudioRecordItemLikeSet;
				item.IsPlayActive = false;
				_controller.AddRecordItem(item);
			}
		}

#region Cheat
		public void LocalSave(string filename)
		{
			_popUpController.AudioClipLocalSave(filename);
		}
#endregion Cheat
	}
}
