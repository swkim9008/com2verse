/*===============================================================
* Product:		Com2Verse
* File Name:	ToastMessageViewModel.cs
* Developer:	tlghks1009
* Date:			2022-08-25 14:41
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.UI
{
	public sealed class ToastMessageViewModel : ViewModelBase
	{
		private string _message;

		private bool _isVisibleNormal;
		private bool _isVisibleWarning;

		public string Message
		{
			get => _message;
			set => base.SetProperty(ref _message, value);
		}


		public bool IsVisibleNormal
		{
			get => _isVisibleNormal;
			set => base.SetProperty(ref _isVisibleNormal, value);
		}


		public bool IsVisibleWarning
		{
			get => _isVisibleWarning;
			set => base.SetProperty(ref _isVisibleWarning, value);
		}
	}
}
