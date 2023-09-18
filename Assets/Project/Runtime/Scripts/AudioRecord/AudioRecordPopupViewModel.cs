/*===============================================================
* Product:		Com2Verse
* File Name:	AudioRecordRecordPopupViewModel.cs
* Developer:	ydh
* Date:			2023-03-21 14:10
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.UI;

namespace Com2Verse.AudioRecord
{
	[ViewModelGroup("AudioRecord")]
	public sealed class AudioRecordPopupViewModel : ViewModel
	{
		private bool _isRecordBtnState;
		public bool IsRecordBtnState
		{
			get => _isRecordBtnState;
			set => SetProperty(ref _isRecordBtnState, value);
		}
		
		private string _recordingTime;
		public string RecordingTime
		{
			get => _recordingTime;
			set=> SetProperty(ref _recordingTime, value);
		}
		
		private float _fillAmount;
		public float FillAmount
		{
			get => _fillAmount;
			set=> SetProperty(ref _fillAmount, value);
		}

		private bool _recActive;
		public bool RecActive
		{
			get => _recActive;
			set => SetProperty(ref _recActive, value);
		}
		
		private AnimationPropertyExtensions _animationPropertyExtensions;
		public AnimationPropertyExtensions AnimationPropertyExtensions
		{
			get => _animationPropertyExtensions;
			set => _animationPropertyExtensions = value;
		}

		private bool _recordAnimationEnable;
		public bool RecordAnimationEnable
		{
			get => _recordAnimationEnable;
			set => SetProperty(ref _recordAnimationEnable, value);
		}

		private bool _isDisConnectMic;

		public bool IsDisConnectMic
		{
			get => _isDisConnectMic;
			set => SetProperty(ref _isDisConnectMic, value);
		}

		private StackRegisterer _registerer;

		public StackRegisterer Registerer
		{
			get => _registerer;
			set
			{
				_registerer = value;
				_registerer.WantsToQuit += Hide;
			}
		}

		private void Hide()
		{
			RecordExitAction?.Invoke();
			_registerer.HideComplete();
		}

		public CommandHandler Command_RecordStart { get; }
		public CommandHandler Command_RecordStop { get; }
		public CommandHandler Command_RecordExit { get; }

		public Action RecordStartAction { get; set; }
		public Action RecordStopAction { get; set; }
		public Action RecordExitAction { get; set; }
		
		public AudioRecordPopupViewModel()
		{
			Command_RecordStart = new CommandHandler(OnCommand_RecordStart);
			Command_RecordStop = new CommandHandler(OnCommand_RecordStop);
			Command_RecordExit = new CommandHandler(OnCommand_RecordExit);
		}

		private void OnCommand_RecordStart() => RecordStartAction?.Invoke();
		private void OnCommand_RecordStop()  => RecordStopAction?.Invoke();
		private void OnCommand_RecordExit()  => RecordExitAction?.Invoke();

		public void Refresh()
		{
			IsRecordBtnState = true;
			IsDisConnectMic = false;
		}
	}
}