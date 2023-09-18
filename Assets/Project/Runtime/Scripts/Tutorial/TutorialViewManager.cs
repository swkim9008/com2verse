/*===============================================================
* Product:		Com2Verse
* File Name:	TutorialViewManager.cs
* Developer:	ydh
* Date:			2023-05-02 14:27
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using Com2Verse.Builder;
using Com2Verse.Data;
using Com2Verse.EventTrigger;
using Com2Verse.Extension;
using Com2Verse.Network;
using Com2Verse.Office;
using Com2Verse.Sound;
using Com2Verse.UI;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Localization = Com2Verse.UI.Localization;

namespace Com2Verse.Tutorial
{
	public sealed partial class TutorialManager
	{
		private int _tutorialMaxPageCount = 0;
		private int _tutorialNowPageCount = 1;
		
		private void OnShowBotPopUp(string title, string ment, string image, int maxCount)
		{
			if (_tutorialView == null)
			{
				UIManager.Instance.CreatePopup(TUTORIAL_POPUP, guiView =>
				{
					guiView.Show();
					_tutorialView = guiView;
					_viewModel = _tutorialView.ViewModelContainer.GetViewModel<TutorialViewModel>();
					_viewModel.TutorialCanvasAlpha = 0f;
					CanvasAlpha(_viewModel).Forget();
					_viewModel.PageCount = $"{_tutorialNowPageCount} / {_tutorialMaxPageCount}";
					SetPopUp(title, ment, image);
				}).Forget();
			}
			else
			{
				if (_viewModel.TutorialCanvasAlpha == 0)
					CanvasAlpha(_viewModel);
				
				SetPopUp(title, ment, image);
			}
		}

		private async UniTask CanvasAlpha(TutorialViewModel viewModel)
		{
			float duration = 0.04f;
			while (true)
			{
				viewModel.TutorialCanvasAlpha += duration;
				if (viewModel.TutorialCanvasAlpha >= 1)
				{
					viewModel.TutorialCanvasAlpha = 1;
					return;
				}

				await UniTask.Delay(1);
			}
		}

		private void SetPopUp(string title, string ment, string image)
		{
			_viewModel?.SetTextureType(image);
			_viewModel.TutorialTitle = title;
			_controller.PlayTutorialMent(ment ?? "");
		}

		private void SetDesc(string desc)
		{
			_nowStepDesc = desc;
			_viewModel.ChatBotDesc = _nowStepDesc;
		}

		public void OnLanguageChanged()
		{
			if (!string.IsNullOrWhiteSpace(_controller.DescKey))
				_viewModel.ChatBotDesc = Localization.Instance.GetTutorialString(_controller.DescKey);
		}

		private void PlayBefore() { }

		private void PlayAfter() { }

		[Serializable()]
		public class TutorialRun
		{
			[JsonProperty("TutorialRecord")] 
			public List<string> TutorialRecord { get; set; }
		}

		public async UniTask LoadTutorialLocalLoad()
		{
			_tutorialRun = await LocalSave.Temp.LoadJsonAsync<TutorialRun>(typeof(TutorialRun).FullName);
			if (_tutorialRun == null)
			{
				_tutorialRun = new TutorialRun();
				await LoadTutorialLocalSave();
			}
		}

		private async UniTask LoadTutorialLocalSave()
		{
			await LocalSave.Temp.SaveJsonAsync(typeof(TutorialRun).FullName, _tutorialRun);
		}

		private bool TutorialPlayCheck(int groupId)
		{
			if (_tutorialRun.TutorialRecord == null)
				return false;

			for (int i = 0; i < _tutorialRun.TutorialRecord.Count; ++i)
			{
				if (_tutorialRun.TutorialRecord[i] == $"{User.Instance.CurrentUserData.ID}_{groupId}")
				{
					return true;
				}
			}
			
			return false;
		}

		public async UniTask TutorialPlay(eTutorialGroup groupId, bool Forced = false)
		{
			if (!_tutorialInfoTable.Datas.ContainsKey(groupId) || _open)
				return;
			
			if (!Forced)
			{
				if (TutorialPlayCheck((int)groupId) || !_nowGroupId.Equals(0))
					return;
			}

			_nowStepId  = _tutorialInfoTable.Datas[groupId].StartStepID;
			_nowGroupId = (int)groupId;
			_open       = true;

			SoundManager.Instance.PlayUISound("SE_Common_Tutorial_01.wav");

			if (_tutorialRun.TutorialRecord == null)
				_tutorialRun.TutorialRecord = new List<string>();
			
			_tutorialRun.TutorialRecord.TryAdd($"{User.Instance.CurrentUserData.ID}_{(int)groupId}");
			LoadTutorialLocalSave().Forget();
			
			_onTutorialOpened?.Invoke();

			_tutorialNowPageCount = 1;
			_tutorialMaxPageCount = TutorialMaxStepCount(groupId);
			while (true)
			{
				ControllerReset();
 
				if (!_tutorialStepTable.Datas.ContainsKey(_nowStepId))
					break;
				
				var stringId = _controller.DescKey = _tutorialStepTable.Datas[_nowStepId].StringID;
				var imageRes = _tutorialStepTable.Datas[_nowStepId].ImageRes;

				if (imageRes != null) imageRes = $"{imageRes}.png";

				_controller.PlayTutorial( Localization.Instance.GetTutorialString(_tutorialInfoTable.Datas[groupId].TutorialTitle), Localization.Instance.GetTutorialString(stringId), imageRes, _tutorialMaxPageCount);

				await UniTask.WaitUntil(() => _viewModel != null, cancellationToken: _cancellationToken.Token).SuppressCancellationThrow();

				if (_tutorialStepTable.Datas[_nowStepId].NextStep == 0)
				{
					BtnsSet(true, false);

					var response = await UniTask.WaitUntil(() => _controller.ClickCloseBtn || _controller.PrevBtnClick, cancellationToken: _cancellationToken.Token).SuppressCancellationThrow();
					if (response) return;
					if (BtnsClick())
						continue;

					break;
				}

				BtnsSet(_nowStepId != _tutorialInfoTable.Datas[(eTutorialGroup)_nowGroupId].StartStepID, true);
				
				var result = await UniTask.WaitUntil(()=>  _controller.NextBtnClick || _controller.PrevBtnClick, cancellationToken: _cancellationToken.Token).SuppressCancellationThrow();
				if (result) return;
				if (BtnsClick())
					continue;

				_nowStepId = _tutorialStepTable.Datas[_nowStepId].NextStep;
				Reset();
			}

			_nowGroupId = 0;
		}

		private int TutorialMaxStepCount(eTutorialGroup groupId)
		{
			int maxCnt = 1;
			var step = _tutorialInfoTable.Datas[groupId].StartStepID;
			while (true)
			{
				if (_tutorialStepTable.Datas[step].NextStep != 0)
				{
					step = _tutorialStepTable.Datas[step].NextStep;
					maxCnt++;
				}
				else
					break;
			}

			return maxCnt;
		}
		
		private void TutorialZoneCheck(ServerZone zone)
		{
			foreach (var tutorialdata in _tutorialTriggerZoneTable)
			{
				var values = tutorialdata.Value.RequiredValue.Split(',');
				for (int i = 0; i < values.Length; i++)
				{
					if (values[i].Equals(zone.ZoneId.ToString()) && OptionalCheck(tutorialdata.Value.OptionalValueType, tutorialdata.Value.OptionalValue))
					{
						TutorialPlay(tutorialdata.Key).Forget();
						return;
					}
				}
			}
		}

		public void TutorialSpaceCheck(string spaceCode)
		{
			if(string.IsNullOrEmpty(spaceCode))
				return;

			foreach (var tutorialdata in _tutorialTriggerSpaceTable)
			{
				var values = tutorialdata.Value.RequiredValue.Split(',');
				for (int i = 0; i < values.Length; i++)
				{
					if (values[i].Equals(spaceCode) && OptionalCheck(tutorialdata.Value.OptionalValueType, tutorialdata.Value.OptionalValue))
					{
						TutorialPlay(tutorialdata.Key).Forget();
						return;
					}
				}
			}
		}

		public void TutorialObjectInteractionCheck(TriggerInEventParameter triggerInEventParameter)
		{
			if (triggerInEventParameter == null || triggerInEventParameter.ParentMapObject.InteractionValues == null)
				return;

			foreach (var data in triggerInEventParameter.ParentMapObject.InteractionValues)
			{
				foreach (var tutorialdata in _tutorialInteractionObjectCheck)
				{
					var values = tutorialdata.Value.RequiredValue.Split(',');
					for (int i = 0; i < values.Length; i++)
					{
						if (values[i].Equals(data.InteractionLinkId.ToString()) && OptionalCheck(tutorialdata.Value.OptionalValueType, tutorialdata.Value.OptionalValue))
						{
							TutorialPlay(tutorialdata.Key).Forget();
							return;
						}
					}
				}
			}
		}

		private bool OptionalCheck(eTutorialValue type, string optionalValue)
		{
			switch (type)
			{
				case eTutorialValue.NONE:
					return true;
				case eTutorialValue.SERVICE_ID:
					if (User.Instance.CurrentServiceType == Convert.ToInt64(optionalValue))
						return true;
					break;
				case eTutorialValue.BUILDING_ID:
					if (OfficeService.Instance.IsSameBuilding(Convert.ToInt64(optionalValue)))
						return true;
					break;
				case eTutorialValue.SPACE_ID:
					if (SpaceManager.Instance.SpaceID == optionalValue)
						return true;
					break;
				case eTutorialValue.SPACE_CODE:
					if (Convert.ToInt32(CurrentScene.SpaceCode) == Convert.ToInt32(optionalValue))
						return true;
					break;
				case eTutorialValue.NOT_SERVICE_ID:
					if (User.Instance.CurrentServiceType != Convert.ToInt64(optionalValue))
						return true;
					break;
				case eTutorialValue.NOT_BUILDING_ID:
					if (!OfficeService.Instance.IsSameBuilding(Convert.ToInt64(optionalValue)))
						return true;
					break;
				case eTutorialValue.NOT_SPACE_ID:
					if (SpaceManager.Instance.SpaceID != optionalValue)
						return true;
					break;
				case eTutorialValue.NOT_SPACE_CODE:
					if (Convert.ToInt32(CurrentScene.SpaceCode) != Convert.ToInt32(optionalValue))
						return true;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
			
			return false;
		}

		private bool BtnsClick()
		{
			if (_controller.NextBtnClick)
			{
				if (_tutorialStepTable.Datas[_nowStepId].NextStep == 0)
					return true; 

				if (!_tutorialStepTable.Datas.ContainsKey(_tutorialStepTable.Datas[_nowStepId].NextStep))
					return false;

				_nowStepId = _tutorialStepTable.Datas[_nowStepId].NextStep;
				
				_viewModel.ChatBotDesc = _tutorialStepTable.Datas[_nowStepId].StringID == null ? string.Empty : " ";
				_viewModel.PageCount = $"{++_tutorialNowPageCount} / {_tutorialMaxPageCount}";
				return true;
			}

			if (_controller.PrevBtnClick)
			{
				foreach (var value in _tutorialStepTable.Datas.Values)
				{
					if (_tutorialStepTable.Datas[_nowStepId].StepID == value.NextStep)
					{
						_nowStepId = value.StepID;
						_viewModel.ChatBotDesc = _tutorialStepTable.Datas[_nowStepId].StringID == null ? string.Empty : " ";
						break;
					}
				}
				_viewModel.PageCount = $"{--_tutorialNowPageCount} / {_tutorialMaxPageCount}";
				return true;
			}

			return false;
		}

		private void BtnsSet(bool prevBtn, bool nextBtn)
		{
			_viewModel.IsPreviousButtonEnabled = prevBtn;
			_viewModel.IsNextButtonEnabled = nextBtn;
		}

		public void TutorialClose()
		{
			_tutorialView?.Hide();
			TutorialCloseAction();

			_open = false;
			
			if(_viewModel != null)
				_viewModel.TutorialCanvasAlpha = 0;
		}

		private void TutorialCloseAction()
		{
			_controller.ClickCloseBtn = true;
			_nowGroupId = 0;
			_tutorialView = null;
			_onTutorialClosed?.Invoke();
		}

		public void StopTutorial()
		{
			if (_nowGroupId == 0)
				return;

			_controller.SkipPopupOpen = false;
			Reset();
			ControllerReset();
			TutorialCloseAction();
		}
		
#region Cheat
		public void TutorialClear()
		{
			_tutorialRun.TutorialRecord = null;
			LocalSave.Temp.Delete(typeof(TutorialRun).FullName);
		}
#endregion Cheat
	}
}