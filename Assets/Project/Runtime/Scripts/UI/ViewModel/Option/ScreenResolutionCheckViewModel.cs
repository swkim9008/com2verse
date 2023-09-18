/*===============================================================
* Product:		Com2Verse
* File Name:	ScreenResolutionCheckViewModel.cs
* Developer:	mikeyid77
* Date:			2023-06-26 16:13
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Logger;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.UI
{
	public sealed class ScreenResolutionCheckViewModel : CommonPopupYesNoViewModel
	{
		private const int MaxQueueCount = 9999;

		private StackRegisterer _guiViewRegisterer  = null;
		private bool            _confirmTrigger     = false;
		private bool            _cancelTrigger      = false;
		private uint            _timer      = 0;
		private Action          _cancelAction = null;
		
		public CommandHandler OkButtonClicked { get; }
		public CommandHandler CancelButtonClicked { get; }

		public StackRegisterer GuiViewRegisterer
		{
			get => _guiViewRegisterer;
			set
			{
				_guiViewRegisterer             =  value;
				_guiViewRegisterer.WantsToQuit += OnCancelButtonClicked;
			}
		}

		private uint Timer
		{
			get => _timer;
			set
			{
				_timer = value;
				Context = Localization.Instance.GetString("UI_Setting_Resolution_Resolution_Popup_Desc", value.ToString());
			}
		}

		public ScreenResolutionCheckViewModel()
		{
			OkButtonClicked = new CommandHandler(OnOkButtonClicked);
			CancelButtonClicked = new CommandHandler(OnCancelButtonClicked);
		}
		
		public void SetPopup(Action cancel)
		{
			Timer = 10;
			_confirmTrigger = false;
			_cancelTrigger = false;
			_cancelAction = cancel;
			OnTimeUpdate().Forget();
		}

		private void OnOkButtonClicked()
		{
			_confirmTrigger = true;
			_cancelAction = null;
			_guiViewRegisterer.HideComplete();
		}

		private void OnCancelButtonClicked()
		{
			_cancelTrigger = true;
			_cancelAction?.Invoke();
			_cancelAction = null;
			_guiViewRegisterer.HideComplete();
		}

		private async UniTask OnTimeUpdate()
		{
			float currentTime = 1;
			while (!_confirmTrigger)
			{
				currentTime -= Time.deltaTime;
				if (currentTime < 0)
				{
					// TODO : 서버에서 대기인원 수 받는 플로우
					// ...
					Timer -= 1;
					currentTime = 1;
				}

				if (_cancelTrigger || Timer <= 0)
				{
					C2VDebug.LogCategory("GraphicOption", $"Canceled");
					OnCancelButtonClicked();
					break;
				}

				await UniTask.Yield();
			}

			await UniTask.Yield();
		}
	}
}
