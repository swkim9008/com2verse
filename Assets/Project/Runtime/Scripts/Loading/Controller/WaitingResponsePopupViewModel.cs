/*===============================================================
* Product:		Com2Verse
* File Name:	WaitingResponsePopupViewModel.cs
* Developer:	tlghks1009
* Date:			2022-11-04 13:07
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

namespace Com2Verse.UI
{
	[ViewModelGroup("CommonPopup")]
	public sealed class WaitingResponsePopupViewModel : ViewModelBase
	{
		private bool _isVisibleLoadingView;


		public override void OnInitialize()
		{
			base.OnInitialize();

			IsVisibleLoadingView = false;
		}

		public bool IsVisibleLoadingView
		{
			get => _isVisibleLoadingView;
			set
			{
				_isVisibleLoadingView = value;
				base.InvokePropertyValueChanged(nameof(IsVisibleLoadingView), value);
			}
		}
	}
}
