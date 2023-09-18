/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingMinutesDetailPopupViewModel.cs
* Developer:	ksw
* Date:			2023-05-12 16:42
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Com2Verse.Logger;
using Com2Verse.Network;
using Com2Verse.WebApi.Service;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.Device;
using UnityEngine.Networking;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;

namespace Com2Verse.UI
{
	public sealed class MeetingMinutesDetailPopupViewModel : ViewModel
	{
		private StackRegisterer _sttGUIViewRegister;

		public StackRegisterer SttGUIViewRegister
		{
			get => _sttGUIViewRegister;
			set
			{
				_sttGUIViewRegister             =  value;
				_sttGUIViewRegister.WantsToQuit += OnClosePopup;
			}
		}
		// ReSharper disable InconsistentNaming
		private enum STTLanguage
		{
			DEFAULT = -1,
			KO      = 0,
			EN      = 1,
		}

		private enum LanguageKey
		{
			@default = -1,
			ko       = 0,
			en       = 1,
		}
		// ReSharper restore InconsistentNaming

		private struct TextFileInfo
		{
			public STTLanguage LanguageType;
			public string      SoundFileName;
			public string      TextFileName;
			public bool        InProgress;
		}

		private GUIView     _guiView;
		private bool        _setActive;
		private bool        _isConverting;
		private bool        _isConverted;
		private int         _convertLanguage;
		private STTLanguage _currentSelectLanguage;

		private List<TMP_Dropdown.OptionData> _meetingMinutesLanguageOptions;
		private TMP_Dropdown.DropdownEvent    _dropDownEventOfLanguage;

		private MeetingInfoType _meetingInfo;

		private List<string>                       _soundFileInfo      = new();
		private Dictionary<string, List<FileInfo>> _fileInfoDictionary = new();

		private bool _fileDownloading;

		private string _savePath;

		public CommandHandler CloseMeetingMinutesPopup   { get; }
		public CommandHandler VoiceFileDownload          { get; }
		public CommandHandler MeetingMinutesFileDownload { get; }
		public CommandHandler MeetingMinutesConvert      { get; }

		public MeetingMinutesDetailPopupViewModel()
		{
			CloseMeetingMinutesPopup   = new CommandHandler(OnClosePopup);
			VoiceFileDownload          = new CommandHandler(OnVoiceFileDownload);
			MeetingMinutesFileDownload = new CommandHandler(OnMeetingMinutesFileDownload);
			MeetingMinutesConvert      = new CommandHandler(OnMeetingMinutesConvert);
			
			_meetingMinutesLanguageOptions = new List<TMP_Dropdown.OptionData>();

			IsConverting = false;
			IsConverted  = false;

			_savePath = Path.Combine(Application.persistentDataPath, "Downloads");
		}

		public bool SetActive
		{
			get => _setActive;
			set => SetProperty(ref _setActive, value);
		}

		/// <summary>
		/// STT 변환 중
		/// </summary>
		public bool IsConverting
		{
			get => _isConverting;
			set => SetProperty(ref _isConverting, value);
		}

		/// <summary>
		/// STT 변환 완료
		/// </summary>
		public bool IsConverted
		{
			get => _isConverted;
			set => SetProperty(ref _isConverted, value);
		}

		public List<TMP_Dropdown.OptionData> MeetingMinutesLanguageOptions
		{
			get => _meetingMinutesLanguageOptions;
			set => SetProperty(ref _meetingMinutesLanguageOptions, value);
		}

		public TMP_Dropdown.DropdownEvent DropdownEventOfLanguage
		{
			get => _dropDownEventOfLanguage;
			set => _dropDownEventOfLanguage = value;
		}

		public int ConvertLanguage
		{
			get => _convertLanguage;
			set => SetProperty(ref _convertLanguage, value);
		}

		public void Initialize(GUIView guiView, MeetingInfoType meetingInfo)
		{
			_guiView               = guiView;
			SetActive              = true;
			IsConverting           = false;
			IsConverted            = false;
			ConvertLanguage        = 0;
			_currentSelectLanguage = STTLanguage.DEFAULT;
			_meetingInfo           = meetingInfo;
			_fileDownloading       = false;
			SetDropdownOption();
			RegisterDropdownAddListener();
			GetRecordFileInfo();
		}

		private void OnClosePopup()
		{
			SetActive = false;
			_dropDownEventOfLanguage.RemoveAllListeners();
			_guiView.Hide();
		}

		private void GetRecordFileInfo()
		{
			_soundFileInfo.Clear();
			_fileInfoDictionary.Clear();
			Commander.Instance.RequestRecordInfoAsync(_meetingInfo.MeetingId, response =>
			{
				foreach (var soundFile in response.Value.Data.SoundFiles)
				{
					_soundFileInfo.Add(soundFile);
				}

				if (response.Value.Data.FileInfos == null)
				{
					IsConverting = false;
					IsConverted  = false;
					return;
				}

				// 미디어 서버에서 FileInfos가 불필요하게 Array 형태로 넘어와서 처리함
				// FileInfos의 길이는 무조건 1이고, 값이 없어도 빈 값으로 채워서 넘어옴
				foreach (var fileInfo in response.Value.Data.FileInfos)
				{
					// 아직 변환 요청을 하지 않은 경우
					if (fileInfo == null)
					{
						IsConverting = false;
						IsConverted  = false;
					}
					else
					{
						_fileInfoDictionary = JsonConvert.DeserializeObject<Dictionary<string, List<FileInfo>>>(fileInfo.ToString());
					}
				}

				SetConvertingUI();
				if (IsConverting)
				{
					ConvertingStateCheck().Forget();
				}
			}, error =>
			{
				C2VDebug.LogError("RequestRecordInfoAsync Failed");
				OnClosePopup();
			}).Forget();
		}

		/// <summary>
		/// 음성 파일 다운로드
		/// </summary>
		private void OnVoiceFileDownload()
		{
			if (_fileDownloading)
				return;
			FileDownload(_soundFileInfo.ToArray());
		}

		/// <summary>
		/// STT 변환 파일 다운로드
		/// </summary>
		private void OnMeetingMinutesFileDownload()
		{
			if (_fileDownloading)
				return;
			List<string> fileNames = new();
			foreach (var fileInfo in _fileInfoDictionary)
			{
				var languageType = (LanguageKey)Enum.Parse(typeof(LanguageKey), fileInfo.Key);
				if ((int)languageType == (int)_currentSelectLanguage)
				{
					foreach (var file in fileInfo.Value)
					{
						if (string.IsNullOrWhiteSpace(file.ResultFileName))
							continue;
						fileNames.Add(file.ResultFileName);
					}
				}
			}

			if (fileNames.Count != 0)
			{
				FileDownload(fileNames.ToArray());
			}
		}

		private void FileDownload(string[] fileNames)
		{
			_fileDownloading = true;
			Commander.Instance.RequestFileDownloadURL(_meetingInfo.MeetingId, fileNames, response =>
			{
				//FileDownload(response.Value.Data.SttFileDownloadInfo).Forget();
				foreach (var fileDownloadInfo in response.Value.Data.SttFileDownloadInfo)
				{
					// 간혹 FileDownloadUrl이 공백으로 넘어올 때가 있음
					if (string.IsNullOrWhiteSpace(fileDownloadInfo.FileDownloadUrl))
						continue;
					Application.OpenURL(fileDownloadInfo.FileDownloadUrl);
					_fileDownloading = false;
				}
			}, error =>
			{
				C2VDebug.LogError("RequestFileDownloadURL Failed");
				_fileDownloading = false;
				OnClosePopup();
			}).Forget();
		}
		
		private async UniTask FileDownload(Components.SttFileDownloadInfo[] downloadInfos)
		{
			foreach (var info in downloadInfos)
			{
				using (UnityWebRequest request = UnityWebRequest.Get(info.FileDownloadUrl))
				{
					await request.SendWebRequest();
					if (request.result == UnityWebRequest.Result.Success)
					{
						var localPath = request.uri.LocalPath;
						var split     = localPath.Split('/');
						var fileName  = split[split.Length - 1];

						var saveFilePath = Path.Combine(_savePath, fileName);

						File.WriteAllBytes(saveFilePath, request.downloadHandler.data);
						C2VDebug.Log("File downloaded and saved: " + _savePath);
					}
					else
					{
						C2VDebug.Log("Download Error : " + request.error);
					}
				}
			}

			_fileDownloading = false;
		}

		/// <summary>
		/// STT 변환
		/// 응답으로 변환 요청한 언어에 대한 것만 응답해줌
		/// </summary>
		private void OnMeetingMinutesConvert()
		{
			Commander.Instance.RequestTranscriptionAsync(_meetingInfo.MeetingId, (int)_currentSelectLanguage, response =>
			{
				IsConverting = true;

				var            key       = (LanguageKey)_currentSelectLanguage;
				List<FileInfo> fileInfos = new();
				foreach (var fileInfo in response.Value.Data.FileInfos)
				{
					var data = JsonConvert.DeserializeObject<FileInfo>(fileInfo.ToString());
					fileInfos.Add(data);
				}
				_fileInfoDictionary.Add(key.ToString(), fileInfos);
				ConvertingStateCheck().Forget();
			}).Forget();
		}

		private async UniTask ConvertingStateCheck()
		{
			if (!SetActive)
				return;
			await UniTask.Delay(10000);
			GetRecordFileInfo();
		}

		private void SetDropdownOption()
		{
			_meetingMinutesLanguageOptions.Clear();
			var languageOptionData = new TMP_Dropdown.OptionData(Localization.Instance.GetString("UI_ConnectingApp_Detail_Stt_LanguageDefaultAuto_Text"));
			_meetingMinutesLanguageOptions.Add(languageOptionData);
			languageOptionData = new TMP_Dropdown.OptionData("한국어");
			_meetingMinutesLanguageOptions.Add(languageOptionData);
			languageOptionData = new TMP_Dropdown.OptionData("English");
			_meetingMinutesLanguageOptions.Add(languageOptionData);
		}
		private void RegisterDropdownAddListener()
		{
			_dropDownEventOfLanguage.RemoveAllListeners();
			_dropDownEventOfLanguage.AddListener((index) =>
			{
				ConvertLanguage        = index;
				_currentSelectLanguage = (STTLanguage)(index - 1);

				GetRecordFileInfo();
			});
		}

		private void SetConvertingUI()
		{
			if (_fileInfoDictionary.Count == 0)
				return;
			foreach (var fileInfo in _fileInfoDictionary)
			{
				var languageType = (LanguageKey)Enum.Parse(typeof(LanguageKey), fileInfo.Key);
				if ((int)languageType == (int)_currentSelectLanguage)
				{
					// 어느 한 파일이 회의록 작성에 실패했을 경우
					if (_soundFileInfo.Count != fileInfo.Value.Count)
					{
						IsConverted  = false;
						IsConverting = false;
						return;
					}
					foreach (var file in fileInfo.Value)
					{
						// 하나라도 변환중일 경우 변환중 UI 출력
						if (file.InProgress)
						{
							IsConverting = true;
							IsConverted  = false;
							return;
						}
					}
					// 모두 변환되어 다운로드 가능한 상태인 경우
					IsConverted  = true;
					IsConverting = false;
					return;
				}
			}
			// Dictionary에 선택한 언어에 대한 파일 정보가 없는 경우
			IsConverted  = false;
			IsConverting = false;
		}
	}

	public class FileInfo
	{
		[JsonProperty("requestFileName")]
		public string RequestFileName { get; set; }

		[JsonProperty("resultFileName")]
		public string ResultFileName  { get; set; }

		[JsonProperty("inProgress")]
		public bool   InProgress    { get; set; }
	}
}