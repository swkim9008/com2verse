/*===============================================================
* Product:		Com2Verse
* File Name:	MyPadEscapeViewModel.cs
* Developer:	mikeyid77
* Date:			2023-05-04 18:32
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.Network;
using Com2Verse.PlayerControl;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Protocols.CommonLogic;
using UnityEngine;

namespace Com2Verse.UI
{
	[ViewModelGroup("MyPad")]
	public sealed class MyPadEscapeViewModel : CommonPopupYesNoViewModel
	{
		private TimeSpan _cooldown;
		// TODO: 6월까지는 쿨타임 없음
		private TimeSpan _escapeTimer = new(0, 0, 0);
		private TimeSpan _second = new(0, 0, 1);

		private string         _topContext;
		private string         _highlight;
		private bool           _canEscapeTrigger = true;
		public  CommandHandler EscapeButtonClicked { get; }

		public MyPadEscapeViewModel()
		{
			Context = MyPadString.EscapePopupCanEscape;
			EscapeButtonClicked = new CommandHandler(OnEscapeButtonClicked);
		}

		public string TopContext
		{
			get => _topContext;
			set => SetProperty(ref _topContext, value);
		}

		public string Highlight
		{
			get => _highlight;
			set => SetProperty(ref _highlight, value);
		}
		
		public bool CanEscapeTrigger
		{
			get => _canEscapeTrigger;
			set => SetProperty(ref _canEscapeTrigger, value);
		}

		private void OnEscapeButtonClicked()
		{
			Network.CommonLogic.PacketReceiver.Instance.EscapeAcceptableCheckResponse += OnEscapeAcceptableCheckResponse;
			Commander.Instance.EscapeAcceptableCheckRequest();
		}

		private void OnEscapeAcceptableCheckResponse(EscapeAcceptableCheckResponse response)
		{
			Network.CommonLogic.PacketReceiver.Instance.EscapeAcceptableCheckResponse -= OnEscapeAcceptableCheckResponse;
			if (response.IsSuccess)
				GoToEscapeProcess();
			else
				UIManager.Instance.ShowPopupYesNo(Localization.Instance.GetString("UI_Common_Notice_Popup_Title"),
				                                  Localization.Instance.GetString("UI_AccessDelay_Notice_Popup_Desc"),
				                                  _ => GoToEscapeProcess(),
				                                  yes: Localization.Instance.GetString("UI_Common_Btn_Move"),
				                                  no: Localization.Instance.GetString("UI_Common_Btn_Cancel"));
		}

		private void GoToEscapeProcess()
		{
			CanEscapeTrigger = false;
			OnTimeUpdate().Forget();
			PlayerController.Instance.SetStopAndCannotMove(true);
			User.Instance.DiscardPacketBeforeStandBy();
			Commander.Instance.EscapeUserRequest();
		}
		
		private async UniTask OnTimeUpdate()
		{
			_cooldown = _escapeTimer;
			float currentTime = 1;
			while (!CanEscapeTrigger)
			{
				currentTime -= Time.deltaTime;
				if (currentTime < 0)
				{
					_cooldown -= _second;
					var format = _cooldown.ToString(@"mm\:ss");
					var context = ZString.Format(MyPadString.EscapePopupCantEscape, format);
					Context = context.Replace(@"\n", Environment.NewLine);
					Highlight = format;
					currentTime = 1;
				}

				if (_cooldown == TimeSpan.Zero)
				{
					Context = MyPadString.EscapePopupCanEscape;
					CanEscapeTrigger = true;
					break;
				}
				
				await UniTask.Delay(_second);
			}
			
			await UniTask.Yield();
		}
	}
}
