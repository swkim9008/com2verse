/*===============================================================
* Product:    Com2Verse
* File Name:  ProjectManager.cs
* Developer:  jehyun
* Date:       2022-03-10 12:59
* History:
* Documents:
* Copyright ⓒ Com2us. All rights reserved.
 ================================================================*/
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Com2Verse.Avatar;
using Com2Verse.AvatarAnimation;
using Com2Verse.BannedWords;
using Com2Verse.CameraSystem;
using Com2Verse.Chat;
using Com2Verse.Communication;
using Com2Verse.Communication.Unity;
using Com2Verse.Contents;
using Com2Verse.Data;
using Com2Verse.Deeplink;
using Com2Verse.Director;
using Com2Verse.EventTrigger;
using Com2Verse.InputSystem;
using Com2Verse.Interaction;
using Com2Verse.Logger;
using Com2Verse.Mice;
using Com2Verse.Network;
using Com2Verse.Office;
using Com2Verse.PlayerControl;
using Com2Verse.Option;
using Com2Verse.Organization;
using Com2Verse.Pathfinder;
using Com2Verse.PlatformControl;
using Com2Verse.Project.CameraSystem;
using Com2Verse.Project.InputSystem;
using Com2Verse.Rendering;
using Com2Verse.SmallTalk;
using Com2Verse.Tutorial;
using Com2Verse.UI;
using Com2Verse.Utils;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Vuplex.WebView;
using Localization = Com2Verse.UI.Localization;
using User = Com2Verse.Network.User;


namespace Com2Verse
{
	public sealed class ProjectManager : MonoSingleton<ProjectManager>
	{
		private readonly Dictionary<string, UniTask> _initTasks = new();

		public bool IsInitializing { get; private set; }
		public bool IsInitialized  { get; private set; }
		public int TotalTaskCount    => _initTasks.Count;
		public int CompleteTaskCount { get; private set; }

#region Initialize
		public async UniTask TryInitializeAsync()
		{
			if (IsInitialized || IsInitializing)
				return;

			IsInitializing = true;
			{
				await InitializeAsync();
			}
			IsInitializing = false;
			IsInitialized  = true;
		}

		private async UniTask InitializeAsync()
		{
			_initTasks.Clear();

			AddTask(ZString.Concat("Step 00 - ", nameof(InitializeTableDataAsync)),      UniTask.Defer(InitializeTableDataAsync));
			AddTask(ZString.Concat("Step 01 - ", nameof(InitializeLocalization)),        UniTask.Defer(InitializeLocalization));
			AddTask(ZString.Concat("Step 02 - ", nameof(InitializeStaticClasses)),       UniTask.Defer(InitializeStaticClasses));
			AddTask(ZString.Concat("Step 03 - ", nameof(InitializePlatformController)),  UniTask.Defer(InitializePlatformController));
			AddTask(ZString.Concat("Step 04 - ", nameof(InitializeWebView)),             UniTask.Defer(InitializeWebView));
			AddTask(ZString.Concat("Step 05 - ", nameof(InitializeSingletonAsync)),      UniTask.Defer(InitializeSingletonAsync));
			AddTask(ZString.Concat("Step 06 - ", nameof(InitializeMonoSingletonAsync)),  UniTask.Defer(InitializeMonoSingletonAsync));
			AddTask(ZString.Concat("Step 07 - ", nameof(InitializeSystemViewListAsync)), UniTask.Defer(InitializeSystemViewListAsync));
			AddTask(ZString.Concat("Step 08 - ", nameof(InitializeAvatarTableAsync)),    UniTask.Defer(InitializeAvatarTableAsync));
			AddTask(ZString.Concat("Step 09 - ", nameof(InitializeConfig)),              UniTask.Defer(InitializeConfig));

			await StartTasks();
		}

		private async UniTask InitializeSystemViewListAsync()
		{
			await UIManager.Instance.LoadSystemViewListAsync();
			await UniTaskHelper.DelayFrame(1);
		}

		private async UniTask InitializeAvatarTableAsync()
		{
			AvatarTable.LoadTable();
			await UniTaskHelper.DelayFrame(1);
		}


		private static async UniTask InitializeLocalization()
		{
			Localization.Instance.LoadTable();
			await UniTaskHelper.DelayFrame(1);
		}


		private static async UniTask InitializeStaticClasses()
		{
			PropertyPathGenerator.Initialize();
			await UniTaskHelper.DelayFrame(1);
		}

		private static async UniTask InitializePlatformController()
		{
			ScreenSize.Instance.Initialize();
			PlatformController.Instance.CreateController();
			PlatformController.Instance.InitializeApplicationController();
			await UniTaskHelper.DelayFrame(1);
		}

		private static async UniTask InitializeWebView()
		{
			StandaloneWebView.SetChromiumLogLevel(ChromiumLogLevel.Disabled);
			Web.ClearAllData();
			await UniTaskHelper.DelayFrame(1);
		}

		private static async UniTask InitializeTableDataAsync()
		{
			// Table
			await TableDataManager.Instance.InitializeAsync();
		}

		private static async UniTask InitializeSingletonAsync()
		{
			// System
			SceneManager.Instance.TryInitialize();
			InputSystemManagerHelper.Initialize();

			// Data
			GeneralData.Initialize();
			OptionController.Instance.ApplyAll();

			// Event & Interaction
			TriggerEventManager.Instance.Initialize();
			InteractionManager.Instance.Initialize();

			// Network
			NetworkUIManager.Instance.Initialize();
			ChatManager.Instance.Initialize();
			NoticeManager.Instance.Initialize();
			ServiceManager.Instance.Initialize();
			User.Instance.Initialize();

			// Communication
			CommunicationManager.TryCreateInstance();
			VoiceDetectionManager.Instance.TryInitialize();
			VideoResolution.Instance.TryInitialize();
			AudioQuality.Instance.TryInitialize();
			await DeviceManager.Instance.TryInitializeAsync();
			await BlobManager.Instance.TryInitializeAsync();
			SmallTalkDistance.Instance.TryInitialize();
			AuditoriumController.CreateInstance();

			// Modules
			AvatarMediator.Instance.Initialize();
			OfficeInfo.Instance.Initialize();

			// View
			await CameraManager.Instance.TryInitializeAsync();
			CameraMediator.Initialize();
			AnimationManager.Instance.Initialize();

			// Office
			OfficeService.Instance.Initialize();

			// Mice
			MiceInfoManager.Instance.Initialize();
			MiceService.Instance.Initialize();

			// EscapeManager
			UIStackManager.Instance.Initialize();

			// SeatManager
			SeatManager.Instance.Initialize();

			// Zone
			ZoneManager.Instance.Initialize();

			// Client Path Finding
			ClientPathFinding.Instance.Initialize();

			// TutorialManager
			TutorialManager.Instance.Initialize();

			// AdsObject
			AdsObject.Initialize();

			// Banned Words
			LoadBannedWords();

			// Organization
			DataManager.Initialize();

			// Reddot
			Mice.RedDotManager.TryCreateInstance();

			// Contents
			NpcManager.Instance.Initialize();
		}

		private static async UniTask InitializeMonoSingletonAsync()
		{
			// Network
			ModeManager.Instance.Initialize();
			UserDirector.Instance.Initialize();
			TagProcessorManager.Instance.Initialize();

			// Graphics
			GraphicsSettingsManager.Instance.Initialize();
			MaterialEffectManager.Instance.Initialize();
			ShaderKeywordsManager.Instance.Initialize();

			// Contents
			PlayContentsManager.Instance.Initialize();

#if ENABLE_CHEATING
			// Cheat
			Cheat.CheatManager.Instance.enabled = true;
#endif // ENABLE_CHEATING

			// Deeplink
			DeeplinkParser.Initialize();
			DeeplinkManager.Initialize();

			await UniTask.CompletedTask;
		}

		private static async UniTask InitializeConfig()
		{
			ChatManager.Instance.SetChatUrl(Configurator.Instance.Config?.WorldChat);
			WebApi.Service.Api.ApiUrl = Configurator.Instance.Config?.OfficeServerAddress;
			await UniTask.Yield();
		}
#endregion // Initialize

#region PlayerLoop
		private void Update()
		{
			if (!IsInitialized)
				return;

			InputSystemManager.Instance.OnUpdate();
			UIManager.Instance.OnUpdate();
			CameraManager.Instance.OnUpdate();
			ChatManager.Instance.OnUpdate();
			ViewModelManager.Instance.OnUpdate();
			PlayerController.Instance.OnUpdate();
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			PlatformController.Instance.OnUpdateApplicationController();
#endif
		}

		private void LateUpdate()
		{
			if (!IsInitialized)
				return;

			CameraManager.Instance.OnLateUpdate();
			MapController.Instance.OnLateUpdate();
		}
#endregion // PlayerLoop

#region Debug
		[Conditional(C2VDebug.LogDefinition), DebuggerHidden, DebuggerStepThrough]
		private static void LogMessage(string msg, [CallerMemberName] string caller = null)
		{
			C2VDebug.LogMethod(nameof(ProjectManager), msg, caller);
		}
#endregion // Debug

		private void AddTask(string message, UniTask task) => _initTasks.Add(message, task);

		private async UniTask StartTasks()
		{
			CompleteTaskCount = 0;

			foreach (var kvp in _initTasks)
			{
				var message = kvp.Key;
				var task    = kvp.Value;

				LogMessage(message);
				await task;

				CompleteTaskCount++;
			}
		}

#region Banned Words
		private static void LoadBannedWords()
		{
			var tableBlackList = TableDataManager.Instance.Get<TableBlackList>();
			if (tableBlackList != null)
			{
				var blackListItems = tableBlackList.Datas
				                                   .Select(item => BannedWordsInfo.WordInfo.Create(word: item.Word, lang: item.Lang, country: item.Country, usage: item.Usage.ToString().ToLower()))
				                                   .GroupBy(item => item.Lang);
				Add(FilterList.BlackList, blackListItems);
			}

			var tableWhiteList = TableDataManager.Instance.Get<TableWhiteList>();
			if (tableWhiteList != null)
			{
				var whiteListItems = tableWhiteList.Datas
				                                   .Select(item => BannedWordsInfo.WordInfo.Create(word: item.Word, lang: item.Lang, country: item.Country, usage: item.Usage.ToString().ToLower()))
				                                   .GroupBy(item => item.Lang);
				Add(FilterList.WhiteList, whiteListItems);
			}


			void Add(IFilterList filterList, IEnumerable<IGrouping<string, BannedWordsInfo.WordInfo>> groups)
			{
				if (groups == null) return;
				if (filterList == null) return;
				filterList.Clear();

				foreach (var group in groups)
				{
					if (group == null || string.IsNullOrWhiteSpace(group.Key)) continue;

					filterList.Add(group.Key, group.AsEnumerable());
				}
			}
		}
#endregion // Banned Words
	}
}