using System.Collections.Generic;
using Com2Verse.BuildHelper;
using Com2Verse.Data;
using Com2Verse.EventTrigger;
using Com2Verse.Extension;
using Com2Verse.Option;
using Com2Verse.Project.InputSystem;
using Com2Verse.TTS;
using Google.Protobuf;
using JetBrains.Annotations;
using Protocols.GameLogic;

namespace Com2Verse.UI
{
	public class BoardManager : Singleton<BoardManager>
	{
		/// <summary>
		/// Singleton Instance Creation
		/// </summary>
		[UsedImplicitly] private BoardManager() { }

		public BoardViewModel BoardViewModel { get; private set; }

		public string BoardURI
		{
			get
			{
				switch (AppInfo.Instance.Data.Environment)
				{
					case eBuildEnv.DEV:
						return "http://10.36.0.97:31504";
					case eBuildEnv.DEV_INTEGRATION:
					case eBuildEnv.QA:
						return "https://qa-service.com2verse.com/today";
					case eBuildEnv.STAGING:
					case eBuildEnv.PRODUCTION:
						return "https://service.com2verse.com/today";
				}
				return string.Empty;
			}
		}

		public const int LengthLimit = 100;
		public readonly string PostPrefabName = "UI_Board_Post";

		public string TodayTopic
		{
			get
			{
				var language = OptionController.Instance.GetOption<LanguageOption>().GetLanguage();

				if (_todayTopicContainer.TryGetValue((int)language, out string todayTopic))
				{
					return todayTopic;
				}

				return string.Empty;
			}
		}
		public string PrivateAccessToken { get; private set; }

		private Dictionary<int, string> _todayTopicContainer = new ();

		public void OnRegisterGuestBookResponse(IMessage message)
		{
			//NetworkUIManager.Instance.GetLogicTypeProcessor<BoardReadProcessor>().BaseInterction();
		}
		
		public void OnMyGuestBookNotify(IMessage message)
		{
			if (message is MyGuestBookNotify myGuestBookNotify)
			{
				_todayTopicContainer.Clear();
				
				_todayTopicContainer.Add((int) Localization.eLanguage.KOR, myGuestBookNotify.TodayTopic);
				_todayTopicContainer.Add((int) Localization.eLanguage.ENG, myGuestBookNotify.TodayEnTopic);
				
				NetworkUIManager.Instance.OnResponseStandInTriggerNotify(new StandInTriggerNotify()
				{
					LogicType = myGuestBookNotify.LogicType,
					ObjectId = -1,
					TriggerId = myGuestBookNotify.TriggerId
				});

				PrivateAccessToken = myGuestBookNotify.PrivateAccessToken;
			}
		}

		public void OnReadAIBotResponse(IMessage message)
		{
			if (message is ReadAIBotResponse readAIBotResponse)
			{
				if (!string.IsNullOrEmpty(readAIBotResponse.GuestBook))
				{
				}
			}
		}

		public void Initialize()
		{
			if (BoardViewModel != null)
			{
				BoardViewModel.ForceRelease();
			}
			BoardViewModel = new BoardViewModel();
		}

		public void Finalize()
		{
			BoardViewModel?.ForceRelease();
			BoardViewModel = null;
		}

		public bool PostPoUpAnimationPlaying(string animationName)
		{
			if (BoardViewModel.AnimationPropertyExtensions.IsReferenceNull())
				return false;
			
			BoardViewModel.AnimationPropertyExtensions.AnimationName = animationName;
			if (BoardViewModel.AnimationPropertyExtensions.AnimationPlay)
				return true;

			return false;
		}
	}
}
