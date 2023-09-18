/*===============================================================
* Product:		Com2Verse
* File Name:	AudioRecordPopupController.cs
* Developer:	ydh
* Date:			2023-05-30 12:39
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Security.Cryptography;
using System.Threading;
using Com2Verse.Communication.Unity;
using Com2Verse.UI;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.AudioRecord
{
	public sealed class AudioRecordPopupController : IDisposable
	{
		private CancellationTokenSource _tokenSource;
		private HashAlgorithm _hashAlgorithm;

		public float ClockTimeSec { get; set; }
		private readonly int _recordTimeMaxTime = 30;

		public bool Recording { get; set; }

		public bool ExitButtonClick { get; set; }
		//public bool RecordingOk { get; set; } //변수명 좀더 생각

		public event Action<AudioClip> RecordFinshAction;
		public event Action RecordCancelAction;
		public event Action RecordingAction;

		private AudioClip _clip;

		public GUIView AudioRecordPopUpView { get; set; }
		public AudioRecordPopupViewModel AudioRecordPopUpViewModel { get; set; }

		private static readonly string AudioRecordRoot = DirectoryUtil.GetTempPath("AudioRecord");

		public void Initialize()
		{
			_tokenSource ??= new CancellationTokenSource();
			_hashAlgorithm ??= HashAlgorithm.Create();
		}

		public void ViewInitialize(GUIView view, AudioRecordPopupViewModel viewModel)
		{
			AudioRecordPopUpView ??= view;
			AudioRecordPopUpViewModel ??= viewModel;
		}

		public void ViewRefreshView()
		{
			AudioRecordPopUpViewModel?.Refresh();
			AudioRecordPopUpView?.Show();
		}

#region  Record
	    public async UniTask AudioRecordAsync(string deviceName)
	    {
		    _clip = MicrophoneProxy.Instance.Start(this, deviceName, true, _recordTimeMaxTime, ModuleManager.Instance.VoiceSettings.Frequency);

	        while (ClockTimeSec < _recordTimeMaxTime && Recording)
	        {
	            await UniTask.DelayFrame(1, cancellationToken: _tokenSource.Token);

	            ClockTimeSec += Time.deltaTime;

	            RecordingAction?.Invoke();
	        }

	        ClockTimeSec = ClockTimeSec > _recordTimeMaxTime ? _recordTimeMaxTime : (int)Math.Ceiling(ClockTimeSec);

	        MicrophoneProxy.InstanceOrNull?.End(this, deviceName);

	        Recording = false;
	        IsRecActive(false);

	        if (ExitButtonClick)
	        {
		        ExitButtonClick = false;
		        //RecordCancelAction?.Invoke();
	        }
			else
		        RecordFinshAction?.Invoke(_clip);
	    }

	    public void RecrodSavePopUp()
	    {
		    if (_clip == null)
			    return;

		    RecordFinshAction?.Invoke(_clip);
	    }

	    public string CreateName(string name) => Convert.ToBase64String(_hashAlgorithm.ComputeHash(System.Text.Encoding.UTF8.GetBytes($"{name}"))).Replace('/', '!');
#endregion Record

		public void RecordFillamount(float value) => AudioRecordPopUpViewModel.FillAmount = value;
		public void IsRecordBtnState(bool value) => AudioRecordPopUpViewModel.IsRecordBtnState = value;
		public void IsRecActive(bool value) => AudioRecordPopUpViewModel.RecActive = value;
		public void RecordingTime(string value) => AudioRecordPopUpViewModel.RecordingTime = value;
		public void Hide() => AudioRecordPopUpView?.Hide();

		public void StopRecordAnimation()
		{
			AudioRecordPopUpViewModel.AnimationPropertyExtensions.AnimationRewind();
			AudioRecordPopUpViewModel.AnimationPropertyExtensions.AnimationStop();
			AudioRecordPopUpViewModel.RecordAnimationEnable = false;
		}

		public void Dispose()
		{
			_clip = null;
			_hashAlgorithm = null;

			RecordFinshAction = null;
			RecordCancelAction = null;
			RecordingAction = null;
			AudioRecordPopUpView = null;
			AudioRecordPopUpViewModel = null;

			if (_tokenSource != null)
			{
				_tokenSource?.Cancel();
				_tokenSource?.Dispose();
				_tokenSource = null;
			}
		}

#region Cheat
		public async UniTask AudioClipLocalSave(string filename)
		{
			if (_clip == null)
				return;
		    
			float[] samples = new float[_clip.samples * _clip.channels];

			_clip.GetData(samples, 0);

			AudioClip clip = AudioClip.Create("NewClip", samples.Length / 30 * (int)ClockTimeSec, _clip.channels, _clip.frequency, false);

			clip.SetData(samples, 0);

			await UniTask.WaitUntil(()=> SavWav.Save($"{AudioRecordRoot}/{filename}", clip));
		}		
#endregion Cheat
	}
}
