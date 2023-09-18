/*===============================================================
* Product:		Com2Verse
* File Name:	MiceUISurveyPopupViewModel.cs
* Developer:	ikyoung
* Date:			2023-07-18 15:11
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Com2Verse.Logger;
using Com2Verse.Mice;

namespace Com2Verse.UI
{
	[ViewModelGroup("Mice")]
	public sealed class MiceUISurveyPopupViewModel : ViewModel
	{
		public bool DontShowAgain { get; set; }
		public string SurveyUrl { get; private set; }
		public long SurveyNo { get; private set; }
		
		public CommandHandler<bool> OnDontShowAgainClick { get; }
		public CommandHandler OnYesClick { get; }
		public CommandHandler OnNoClick  { get; }

		public MiceUISurveyPopupViewModel()
		{
			OnDontShowAgainClick = new CommandHandler<bool>(OnDontShowAgainClicked);
			OnYesClick = new CommandHandler(OnYesClicked);
			OnNoClick = new CommandHandler(OnNoClicked);
		}

		public void Init(string surveyUrl, long surveyNo)
		{
			SurveyUrl = surveyUrl;
			SurveyNo = surveyNo;
		}

		private void OnYesClicked()
		{
			Application.OpenURL(SurveyUrl);
			MiceInfoManager.Instance.MyUserInfo.CompleteSurvey(SurveyNo);
		}

		private void OnNoClicked()
		{
			if (DontShowAgain)
			{
				MiceInfoManager.Instance.MyUserInfo.CompleteSurvey(SurveyNo);
			}
		}

		private void OnDontShowAgainClicked(bool isActive)
		{
			DontShowAgain = isActive;
		}
	}
}
