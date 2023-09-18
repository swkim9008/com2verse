/*===============================================================
* Product:		Com2Verse
* File Name:	VoiceQualitySelectViewModel.cs
* Developer:	haminjeong
* Date:			2023-08-18 15:48
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Com2Verse.Data;
using Com2Verse.Option;
using JetBrains.Annotations;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("Communication")]
	public sealed class VoiceQualitySelectViewModel : ViewModelBase
	{
		[UsedImplicitly] public CommandHandler       SelectVoiceQualityHigh { get; }
		[UsedImplicitly] public CommandHandler       SelectVoiceQualityLow  { get; }
		[UsedImplicitly] public CommandHandler       ConfirmHandler         { get; }

		public Action OnConfirmAction;

		private bool _isHighQuality;
		private bool _isLowQuality;
		private int  _useVoiceRecordingQuality;

		public VoiceQualitySelectViewModel()
		{
			SelectVoiceQualityHigh = new CommandHandler(() => UseVoiceRecordingQuality = (int)eSoundQualityType.HIGH_QUALITY);
			SelectVoiceQualityLow  = new CommandHandler(() => UseVoiceRecordingQuality = (int)eSoundQualityType.LOW_QUALITY);
			ConfirmHandler         = new CommandHandler(OnConfirmButton);
			
			InitVariables();
		}

		public void InitVariables()
		{
			var option = OptionController.Instance.GetOption<DeviceOption>();
			if (option != null)
			{
				UseVoiceRecordingQuality = option.UseVoiceRecordingQuality;
			}
		}

		private void OnConfirmButton()
		{
			var audioRecordingQuality = ViewModelManager.Instance.Get<AudioRecordingQualityViewModel>();
			if (audioRecordingQuality != null)
				audioRecordingQuality.UseVoiceRecordingQuality = UseVoiceRecordingQuality-1;
			OnConfirmAction?.Invoke();
		}

#region ViewModelProperties
		public bool IsHighQuality
		{
			get => _isHighQuality;
			set => UpdateProperty(ref _isHighQuality, value);
		}

		public bool IsLowQuality
		{
			get => _isLowQuality;
			set => UpdateProperty(ref _isLowQuality, value);
		}
		
		public int UseVoiceRecordingQuality
		{
			get => _useVoiceRecordingQuality;
			set
			{
				var prevValue = _useVoiceRecordingQuality;
				if (prevValue == value)
					return;
				SetProperty(ref _useVoiceRecordingQuality, value);

				var option = OptionController.Instance.GetOption<DeviceOption>();
				if (option != null)
				{
					option.UseVoiceRecordingQuality = value;
					option.SaveData();
				}

				IsLowQuality  = _useVoiceRecordingQuality == (int)eSoundQualityType.LOW_QUALITY;
				IsHighQuality = _useVoiceRecordingQuality == (int)eSoundQualityType.HIGH_QUALITY;
			}
		}
#endregion // ViewModelProperties

		private void UpdateProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "") where T : unmanaged, IConvertible
		{
			if (EqualityComparer<T>.Default.Equals(storage, value))
				return;

			SetProperty(ref storage, value, propertyName);
		}
	}
}

