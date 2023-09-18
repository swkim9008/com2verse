/*===============================================================
* Product:		Com2Verse
* File Name:	TutorialTooltipManagerViewModel.cs
* Developer:	urun4m0r1
* Date:			2022-11-08 17:35
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Com2Verse.UI
{
	[Serializable]
	public class TutorialTooltipInfo
	{
		[JsonProperty] public bool IsMeetingRoomRoomInfoFinished { get; set; }
		[JsonProperty] public bool IsMeetingRoomUserTypeFinished { get; set; }
	}

	[UsedImplicitly, ViewModelGroup("Communication")]
	public class TutorialTooltipManagerViewModel : ViewModelBase
	{
		[UsedImplicitly] public CommandHandler FinishMeetingRoomRoomInfo { get; }
		[UsedImplicitly] public CommandHandler FinishMeetingRoomUserType { get; }

		private TutorialTooltipInfo? _tutorialTooltipInfo;

		private readonly string _savePath = typeof(TutorialTooltipInfo).FullName;

		public TutorialTooltipManagerViewModel()
		{
			FinishMeetingRoomRoomInfo = new CommandHandler(() => IsMeetingRoomRoomInfoFinished = true);
			FinishMeetingRoomUserType = new CommandHandler(() => IsMeetingRoomUserTypeFinished = true);

			LoadTutorialTooltipStatusAsync().Forget();
		}

		public void ResetSavedData()
		{
			_tutorialTooltipInfo = new TutorialTooltipInfo();
			SaveTutorialTooltipStatusAsync().Forget();
			NotifyTutorialTooltipStatusChanged();
		}

		private async UniTask LoadTutorialTooltipStatusAsync()
		{
			_tutorialTooltipInfo = await LocalSave.Temp.LoadJsonAsync<TutorialTooltipInfo>(_savePath);
			if (_tutorialTooltipInfo == null)
			{
				_tutorialTooltipInfo = new TutorialTooltipInfo();
				await SaveTutorialTooltipStatusAsync();
			}

			NotifyTutorialTooltipStatusChanged();
		}

		private void NotifyTutorialTooltipStatusChanged()
		{
			InvokePropertyValueChanged(nameof(IsMeetingRoomRoomInfoFinished), IsMeetingRoomRoomInfoFinished);
			InvokePropertyValueChanged(nameof(IsMeetingRoomUserTypeFinished), IsMeetingRoomUserTypeFinished);
		}

		private async UniTask SaveTutorialTooltipStatusAsync()
		{
			await LocalSave.Temp.SaveJsonAsync(_savePath, _tutorialTooltipInfo);
		}

#region ViewModelProperties
		public bool IsMeetingRoomRoomInfoFinished
		{
			get => _tutorialTooltipInfo?.IsMeetingRoomRoomInfoFinished ?? false;
			set
			{
				if (_tutorialTooltipInfo == null || _tutorialTooltipInfo.IsMeetingRoomRoomInfoFinished == value)
					return;

				_tutorialTooltipInfo.IsMeetingRoomRoomInfoFinished = value;
				InvokePropertyValueChanged(nameof(IsMeetingRoomRoomInfoFinished), value);
				SaveTutorialTooltipStatusAsync().Forget();
			}
		}

		public bool IsMeetingRoomUserTypeFinished
		{
			get => _tutorialTooltipInfo?.IsMeetingRoomUserTypeFinished ?? false;
			set
			{
				if (_tutorialTooltipInfo == null || _tutorialTooltipInfo.IsMeetingRoomUserTypeFinished == value)
					return;

				_tutorialTooltipInfo.IsMeetingRoomUserTypeFinished = value;
				InvokePropertyValueChanged(nameof(IsMeetingRoomUserTypeFinished), value);
				SaveTutorialTooltipStatusAsync().Forget();
			}
		}
#endregion // ViewModelProperties
	}
}
