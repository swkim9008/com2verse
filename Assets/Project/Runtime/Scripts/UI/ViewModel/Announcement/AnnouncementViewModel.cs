/*===============================================================
* Product:		Com2Verse
* File Name:	AnnouncementViewModel.cs
* Developer:	ksw
* Date:			2023-05-23 16:41
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/


using Com2Verse.UI;

namespace Com2Verse
{
	public sealed class AnnouncementViewModel : ViewModelBase
	{
		private string _message;
		private float  _textMoveSpeed;
		private bool   _isLoop;
		private bool   _isPlay;

		public string Message
		{
			get => _message;
			set => SetProperty(ref _message, value);
		}

		public float TextMoveSpeed
		{
			get => _textMoveSpeed;
			set => SetProperty(ref _textMoveSpeed, value);
		}

		public bool IsLoop
		{
			get => _isLoop;
			set => SetProperty(ref _isLoop, value);
		}

		public bool IsPlay
		{
			get => _isPlay;
			set => SetProperty(ref _isPlay, value);
		}
	}
}
