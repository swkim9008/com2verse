/*===============================================================
* Product:		Com2Verse
* File Name:	AudioRecordItem.cs
* Developer:	ydh
* Date:			2023-03-20 14:47
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.UI;
using UnityEngine.Events;

namespace Com2Verse.AudioRecord
{
	[ViewModelGroup("AudioRecord")]
	public sealed class AudioRecordItemViewModel : ViewModel, IDisposable
	{
		public AudioRecordInfo AudioRecordInfo { get; set; }
		
		private bool _likeActive;
		public bool LikeActive
		{
			get => _likeActive;
			set => SetProperty(ref _likeActive, value);
		}
		
		private string _name;
		public string Name
		{
			get => _name;
			set => SetProperty(ref _name, value);
		}

		private string _itemLikeCount;

		public string ItemLikeCount
		{
			get => _itemLikeCount;
			set => SetProperty(ref _itemLikeCount, value);
		}
		
		private string _signUpTime;
		public string SignUpTime
		{
			get => _signUpTime;
			set => SetProperty(ref _signUpTime, value);
		}

		private bool _isMine;

		public bool IsMine
		{
			get => _isMine;
			set => SetProperty(ref _isMine, value);
		}

		private bool _isPlayActive;
		public bool IsPlayActive
		{
			get => _isPlayActive;
			set => SetProperty(ref _isPlayActive, value);
		}

		private string _stateText;
		public string StateText
		{
			get => _stateText;
			set => SetProperty(ref _stateText, value);
		}

		private float _playImageFillAmount;
		public float PlayImageFillAmount
		{
			get => _playImageFillAmount;
			set => SetProperty(ref _playImageFillAmount, value);
		}
		
		private bool _like;
		public Action<long> AudioRecordPlayAction { get; set; }
		public Action<long> AudioRecordStopAction { get; set; }
		public Action<long> AudioRecordDeleteAction { get; set; }
		public Action<long> AudioRecordSelectAction { get; set; }
		public Action<long, bool> AudioRecordLikeAction { get; set; }
		public UnityEvent AudioStateEvent { get; set; }

		public CommandHandler Command_Like { get; }
		public CommandHandler Command_Delete { get; }
		public CommandHandler Command_AudioStop { get; }
		public CommandHandler Command_AudioSelect { get; }

		private AnimationPropertyExtensions _animationPropertyExtensions;
		public AnimationPropertyExtensions AnimationPropertyExtensions
		{
			get => _animationPropertyExtensions;
			set => _animationPropertyExtensions = value;
		}

		public AudioRecordItemViewModel(AudioRecordInfo info)
		{
			AudioRecordInfo = info;
			ItemInfoRefresh();
			
			PlayImageFillAmount = 0;
			_like = !AudioRecordInfo.RecommendAvailable;
			
			Command_Like = new CommandHandler(OnCommand_Like);
			Command_Delete = new CommandHandler(OnCommand_Delete);
			Command_AudioStop = new CommandHandler(OnCommand_AudioStop);
			Command_AudioSelect = new  CommandHandler(OnCommand_AudioSelect);
			
			AudioStateEvent ??= new UnityEvent();
			AudioStateEvent.AddListener(OnCommand_AudioStart);
		}

		public void ItemInfoRefresh()
		{
			Name = AudioRecordInfo.RecordName;
			var time = DateTime.Now - AudioRecordInfo.CreateDateTime.ToLocalTime();
			SignUpTime = TimeConversion(time);
			IsMine = AudioRecordInfo.IsMine;
			LikeActive = AudioRecordInfo.RecommendAvailable;
			ItemLikeCount = AudioRecordInfo.RecommendCount.ToString();
		}

		private string TimeConversion(TimeSpan time)
		{
			if(time.TotalHours < 1)
				return string.Format(Localization.Instance.GetString("UI_Office_Voicemessage_Time_System_0002"), time.Minutes);
			else
				return string.Format(Localization.Instance.GetString("UI_Office_Voicemessage_Time_System_0001"), time.Hours, time.Minutes);
		}

		private void OnCommand_AudioStart() => AudioRecordPlayAction?.Invoke(AudioRecordInfo.BoardSeq);
		private void OnCommand_Delete() => AudioRecordDeleteAction?.Invoke(AudioRecordInfo.BoardSeq);
		private void OnCommand_AudioStop() => AudioRecordStopAction?.Invoke(AudioRecordInfo.BoardSeq);
		private void OnCommand_Like() => AudioRecordLikeAction?.Invoke(AudioRecordInfo.BoardSeq, _like = !_like);
		private void OnCommand_AudioSelect() => AudioRecordSelectAction?.Invoke(AudioRecordInfo.BoardSeq);

		public void Dispose()
		{
			if (AudioStateEvent != null)
				AudioStateEvent = null;
			
			AudioRecordPlayAction = null;
			AudioRecordStopAction = null;
			AudioRecordDeleteAction = null;
			AudioRecordSelectAction = null;
			AudioRecordLikeAction = null;
			AudioStateEvent = null;
		}
	}
}