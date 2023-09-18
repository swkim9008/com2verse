/*===============================================================
* Product:		Com2Verse
* File Name:	TutorialController.cs
* Developer:	ydh
* Date:			2023-04-07 10:18
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Threading;

namespace Com2Verse.Tutorial
{
	public sealed class TutorialController : IDisposable
	{
		private CancellationTokenSource _tokenSource;
		public event Action<string, string, string, int> OnShowBotPopUp;
		public event Action<string> SetChatBotDesc;
		public event Action PlayBefore;
		public event Action PlayAfter;
		
		public string DescKey {get; set;}
		public bool ClickCloseBtn {get; set;}
		public bool SkipPopupOpen {get; set;}
		public bool NextBtnClick {get; set;}
		public bool PrevBtnClick {get; set;}

#region Initialize
		public void Initialize()
		{
			_tokenSource = new CancellationTokenSource();
		}
#endregion Initialize

#region PlayTutorialMent
		public void PlayTutorialMent(string ment, Action endAction = null)
		{
			PlayBefore?.Invoke();
			
			SetDynamicDesc(ment);

			if (endAction != null)
				endAction();
			
			PlayAfter?.Invoke();
		}
#endregion PlayTutorialMent
		public void Dispose()
		{
			_tokenSource?.Cancel();
			_tokenSource?.Dispose();
			_tokenSource = null;
		}
		
		private void SetDynamicDesc(string Ment)
		{
			SetChatBotDesc?.Invoke(Ment);
		}

		public void PlayTutorial(string title, string ment, string image, int maxCount)
		{
			OnShowBotPopUp?.Invoke(title, ment, image, maxCount);
		}
	}
}