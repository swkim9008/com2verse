/*===============================================================
* Product:		Com2Verse
* File Name:	MicePlayingCutScene.cs
* Developer:	ikyoung
* Date:			2023-07-11 14:13
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/


using System;
using System.Threading;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Network;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.Mice
{
	public sealed class MicePlayingCutScene : MiceServiceState
	{
		private MiceCutSceneManager     _cutSceneManager = default;
		private CancellationTokenSource _cts             = default;
		private GameObject              _screenCover     = default;

		public MicePlayingCutScene()
		{
			ServiceStateType = eMiceServiceState.PLAYING_CUTSCENE;
		}

		public override void OnStart(eMiceServiceState prevStateType)
		{
			base.OnStart(prevStateType);
			UIStackManager.Instance.AddByName(nameof(MicePlayingCutScene), () => { }, eInputSystemState.UI, true);

			_cutSceneManager = MiceCutSceneManager.Instance;
			if (_cutSceneManager.IsUnityNull())
			{
				MiceService.Instance.ChangeCurrentState(eMiceServiceState.SESSION_PLAYING);
				return;
			}

			_cutSceneManager.gameObject.SetActive(true);
			if (!MiceEffectManager.Instance.IsUnityNull())
			{
				MiceEffectManager.Instance!.SetActive(false);
			}

			RunCutScene().Forget();
		}

		public override void OnStop()
		{
			base.OnStop();
			UIStackManager.Instance.RemoveByName(nameof(MicePlayingCutScene));

			if (!_cutSceneManager.IsUnityNull())
			{
				if (_cutSceneManager.IsPlaying())
				{
					_cutSceneManager.Stop();
					MiceService.Instance.ChangeCamera(0);
				}
				_cutSceneManager.gameObject.SetActive(false);
			}
			_cutSceneManager = null;
			_cts?.Cancel();

			if (!MiceEffectManager.Instance.IsUnityNull())
				MiceEffectManager.Instance!.SetActive(true);
		}

		public override bool MiceUIShouldVisible()
		{
			return false;
		}

		private async UniTask RunCutScene()
		{
			try
			{
				_cts = new CancellationTokenSource();

				var cancellationToken = _cts.Token;
				User.Instance.CharacterObject.ForceSetUpdateEnable(false);

				var parameter = new MiceCutSceneController.Parameter();
				parameter.SlotBindings = new[]
				{
					new MiceCutSceneController.Parameter.SlotBinding()
					{
						Type   = ConvertTo(User.Instance.AvatarInfo.AvatarType),
						Target = User.Instance.CharacterObject.gameObject,
					}
				};

				var skipIntro    = MiceService.Instance.IsCurrentEventIntroSkip;
				var skipTutorial = false;
				if (!skipIntro)
				{
					_cutSceneManager.Play(MiceCutSceneController.eType.INTRO, parameter);
					await UniTask.WaitUntil(predicate: () => !_cutSceneManager.IsPlaying(), cancellationToken: cancellationToken);
					if (!_cutSceneManager.IsSkipped) MiceService.Instance.SetCurrentEventIntroSkip();
					MiceService.Instance.ChangeCamera(0);

					var showTutorialSkipPopup = true;
					UIManager.Instance.ShowPopupYesNoCancel(Data.Localization.eKey.UI_Common_Notice_Popup_Title.ToLocalizationString(),
					                                        Data.Localization.eKey.MICE_UI_SessionHall_Tutorial_Popup_Desc.ToLocalizationString(),
					                                        (guiView) => { showTutorialSkipPopup = false; },
					                                        (guiView) =>
					                                        {
						                                        showTutorialSkipPopup = false;
						                                        skipTutorial          = true;
					                                        },
					                                        (guiView) => { showTutorialSkipPopup = false; },
					                                        yes: Data.Localization.eKey.MICE_UI_Common_Btn_Ok.ToLocalizationString(),
					                                        no: Data.Localization.eKey.MICE_UI_SessionHall_Btn_Skip.ToLocalizationString(),
					                                        allowCloseArea: false);

					await UniTask.WaitUntil(() => !showTutorialSkipPopup, cancellationToken: cancellationToken);
				}

				if (!skipTutorial)
				{
					_cutSceneManager.Play(MiceCutSceneController.eType.TUTORIAL, parameter);
					await UniTask.WaitUntil(predicate: () => !_cutSceneManager.IsPlaying(), cancellationToken: cancellationToken);
					MiceService.Instance.ChangeCamera(0);
				}
				User.Instance.CharacterObject.ForceSetUpdateEnable(true);

				MiceService.Instance.ChangeCurrentState(eMiceServiceState.SESSION_PLAYING);
			}
			catch (Exception e) { }
			finally
			{
				_cts.Dispose();
				_cts = null;
			}
		}

		private MiceCutSceneSlot.eType ConvertTo(eAvatarType type)
		{
			return type switch
			{
				eAvatarType.PC01_M => MiceCutSceneSlot.eType.PC_M,
				eAvatarType.PC01_W => MiceCutSceneSlot.eType.PC_W,
				_                  => MiceCutSceneSlot.eType.NONE
			};
		}
	}
}
