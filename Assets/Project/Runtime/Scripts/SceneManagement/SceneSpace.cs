/*===============================================================
* Product:		Com2Verse
* File Name:	SceneSpace.cs
* Developer:	urun4m0r1
* Date:			2023-06-01 18:30
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Collections.Generic;
using Com2Verse.Ambience.Runtime.DitherOverride;
using Com2Verse.Avatar;
using Com2Verse.Banner;
using Com2Verse.CameraSystem;
using Com2Verse.Chat;
using Com2Verse.Communication;
using Com2Verse.Data;
using Com2Verse.Extension;
using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using Com2Verse.Mice;
using Com2Verse.Network;
using Com2Verse.Pathfinder;
using Com2Verse.Rendering.World;
using Com2Verse.SmallTalk;
using Com2Verse.SoundSystem;
using Com2Verse.Tutorial;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using RuntimeObjectManager = Com2Verse.LruObjectPool.RuntimeObjectManager;
using User = Com2Verse.Network.User;

namespace Com2Verse
{
	/// <summary>
	/// TODO: 씬 로딩 이후에 SceneProperty 값 수정 기능 추가 예정
	/// </summary>
	public sealed class SceneSpace : SceneBase
	{
		// FIXME: 임시 처리, 서버에서 받아오는 값으로 변경해야 함
		private static readonly string LightSceneAddressableName = "World_LightSettingScene";

		protected override eMainCameraType MainCameraType => eMainCameraType.BACKGROUND;

		private readonly SceneInstanceContainer _lightSceneInstance = new();

		private static WorldRenderPositionHandler? _worldRenderPositionHandler;

		private bool _prevDitherValue;

		protected override void OnLoadingCompleted()
		{
			if (CommunicationType is eSpaceOptionCommunication.SMALL_TALK_DISTANCE_ALL or eSpaceOptionCommunication.SMALL_TALK_DISTANCE_VOICE)
				SmallTalkDistance.InstanceOrNull?.Enable(SmallTalk.Define.TableIndex.Default);

			if (CommunicationType is eSpaceOptionCommunication.SMALL_TALK_AREA_ALL or eSpaceOptionCommunication.SMALL_TALK_AREA_VOICE)
				SmallTalkDistance.InstanceOrNull?.Enable(SmallTalk.Define.TableIndex.AreaBased);

			User.Instance.SetUserSessionTimeoutTime(SceneProperty.SessionTimeout);
			ChatManager.Instance.ConnectChatUI();
			BoardManager.Instance.Initialize();
			SoundManager.PlayBgm(SceneProperty);

			if (CommunicationType is eSpaceOptionCommunication.MEETING)
			{
				MeetingRoomExtraTimeReminder.Start();
			}

			if (SpaceCode != null)
			{
				TutorialManager.Instance.TutorialSpaceCheck(Convert.ToInt32(SpaceCode).ToString());
			}

			if (IsWorldScene)
			{
				Sound.SoundManager.Instance.PlayUISound("SE_PC01_Entry_02_2.wav");
			}

			_prevDitherValue = DitherOverride.Get();
			DitherOverride.Set(true);
		}

		protected override void OnExitScene(SceneBase nextScene)
		{
			SmallTalkDistance.InstanceOrNull?.Disable();
			ChannelManager.InstanceOrNull?.LeaveAllChannels();
			UserFunctionUI.RemoveGUIView();

			CameraManager.Instance.ChangeTarget(null!);
			CameraManager.Instance.CleanupComponents();
			MapController.Instance.CleanObjects();
			ChatManager.Instance.ResetProperties();

			if (_worldRenderPositionHandler != null)
			{
				if (!WorldRenderRuntimeManager.Instance.IsUnityNull())
					WorldRenderRuntimeManager.Instance!.Clear();
				UnityEngine.Object.Destroy(_worldRenderPositionHandler);
			}

			_lightSceneInstance.UnloadAsync().Forget();

			CommunicationManager.Instance.ChangeCommunicationType(eCommunicationType.DEFAULT);
			ChannelManager.Instance.LeaveAllChannels();

			BoardManager.Instance.Finalize();

			if (CommunicationType is eSpaceOptionCommunication.MEETING)
			{
				MeetingReservationProvider.RemoveMeetingInfo();
				MeetingRoomExtraTimeReminder.End();
			}

			ClientPathFinding.Instance.ClearGraph();

			// TODO: 커스터마이즈 종료시 실행 타이밍 확인 필요
			DitherOverride.Set(_prevDitherValue);
			AvatarMediator.Instance.AvatarCloset.Clear();

			AvatarCreatorQueue.Instance.Clear();
			RuntimeObjectManager.Instance.Clear();

			BannerManager.Instance.ResetBanner();
		}

		protected override void RegisterLoadingTasks(Dictionary<string, UniTask> loadingTasks)
		{
			if (IsWorldScene)
			{
				loadingTasks.Add("WorldLightConfigure", UniTask.Defer(WaitForWorldLightConfig));
			}

			if (CommunicationType is eSpaceOptionCommunication.MEETING)
			{
				loadingTasks.Add("OrganizationData", UniTask.Defer(LoadOrganizationDataAsync));
			}

			if (SpaceType == eSpaceType.MICE)
			{
				loadingTasks.Add("PrepareService", UniTask.Defer(PrepareService));
			}

			loadingTasks.Add("UserStandBy",           UniTask.Defer(WaitForUserStandbyAsync));

			if (IsWorldScene)
			{
				loadingTasks.Add("WorldUpdatorCheckUser", UniTask.Defer(WaitForUserBaseWorldUpdator));
				loadingTasks.Add("WorldRenderingInit", UniTask.Defer(WaitForWorldRenderFirstLoadingComplete));
			}

			loadingTasks.Add("UICanvasRoot", UniTask.Defer(LoadUICanvasRootAsync));

			if (CommunicationType is eSpaceOptionCommunication.MEETING)
			{
				loadingTasks.Add("ChannelJoin",   UniTask.Defer(WaitForChannelJoinAsync));
			}

			loadingTasks.Add("MyPadSpriteAtlas",    UniTask.Defer(LoadMyPadSpriteAtlasAsync));
			loadingTasks.Add("MyPadTableData",      UniTask.Defer(LoadMyPadTableDataAsync));
			loadingTasks.Add("BannerTableData",     UniTask.Defer(LoadBannerTableDataAsync));
			loadingTasks.Add("NotificationManager", UniTask.Defer(InitializeNotificationManagerAsync));

			if (ClientPathFinding.Instance.enabled)
			{
				loadingTasks.Add("NavMesh", UniTask.Defer(LoadNavmeshAsync));
			}

			if (SpaceType == eSpaceType.MICE)
			{
				loadingTasks.Add("StartService", UniTask.Defer(StartService));
			}

			loadingTasks.Add("CreateCharacter", UniTask.Defer(WaitForCreateUserCharacter));
		}

		private async UniTask PrepareService()
		{
			await MiceService.Instance.OnPrepareService(SpaceTemplate?.ID ?? -1);
			await UniTask.CompletedTask;
		}

		private async UniTask StartService()
		{
			await MiceService.Instance.OnStartService(SpaceTemplate?.ID ?? -1);
			await UniTask.CompletedTask;
		}

		private async UniTask WaitForChannelJoinAsync()
		{
			ChannelManager.Instance.LeaveAllChannels();

			var meetingInfo = MeetingReservationProvider.EnteredMeetingInfo;
			CommunicationManager.Instance.ChangeCommunicationType(eCommunicationType.MEETING_ROOM_ROUND);
			ViewModelManager.Instance.GetOrAdd<MeetingRoomInfoViewModel>().Initialize(meetingInfo);

			ChannelManager.Instance.JoinAllChannels();
			await UniTask.CompletedTask;
		}

		private async UniTask WaitForUserBaseWorldUpdator()
		{
			static bool CheckUser()
			{
				var usercharacter = User.Instance.CharacterObject;
				if (usercharacter != null)
				{
					_worldRenderPositionHandler = usercharacter.gameObject.AddComponent<WorldRenderPositionHandler>();
					if (!WorldRenderRuntimeManager.Instance.IsUnityNull())
						WorldRenderRuntimeManager.Instance.Init(WorldRenderRuntimeManager.DefaultRenderGrade, WorldRenderRuntimeManager.DefaultRenderGradeMaskPath);
				}
				else
					return false;

				return true;
			}

			await UniTask.WaitUntil(static () => CheckUser());
		}

		private async UniTask WaitForWorldRenderFirstLoadingComplete()
		{
			await UniTask.WaitUntil(() => WorldRenderRuntimeManager.Instance.FirstLoadingComplete);
		}

		private async UniTask WaitForWorldLightConfig()
		{
			await _lightSceneInstance.LoadAsync(LightSceneAddressableName, LoadSceneMode.Additive);

			try
			{
				if (_lightSceneInstance.AddressableSceneInstance == null) return;
				UnityEngine.SceneManagement.SceneManager.SetActiveScene(_lightSceneInstance.AddressableSceneInstance.Value
				                                                                           .Scene);
			}
			catch (Exception e)
			{
				C2VDebug.LogError($"WorldLightConfig SetActiveScene Failed-> {e.Message}");
			}

			await UniTask.Delay(100);
			LightProbes.TetrahedralizeAsync();
		}

		private async UniTask LoadMyPadSpriteAtlasAsync()
		{
			bool spriteAtlasLoaded = false;
			SpriteAtlasManager.Instance.LoadSpriteAtlasAsync("Atlas_MyPad", (handle) => { spriteAtlasLoaded = true; }, true);
			await UniTask.WaitUntil(() => spriteAtlasLoaded);
		}

		private async UniTask LoadMyPadTableDataAsync()
		{
			MyPadManager.Instance.LoadTable();
			await UniTask.Yield();
		}

		private async UniTask LoadBannerTableDataAsync()
		{
			BannerManager.Instance.ShowBanner(IsWorldScene);
			await UniTask.Yield();
		}

		public override void OnEscapeAction()
		{
			if (SpaceCode == eSpaceCode.MICE_CONFERENCE_HALL)
			{
				MiceService.Instance.ShowHallExitPopup();
				return;
			}

			MyPadManager.Instance.CheckOpenMyPad();
		}
	}
}
