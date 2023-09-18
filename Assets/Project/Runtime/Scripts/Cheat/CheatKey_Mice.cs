/*===============================================================
* Product:		Com2Verse
* File Name:	CheatKey_Mice.cs
* Developer:	wlemon
* Date:			2023-04-06 12:55
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Com2Verse.Avatar;
using Com2Verse.Data;
using Com2Verse.Mice;
using Com2Verse.Network;
using Com2Verse.Rendering;
using Com2Verse.UI;
using Com2Verse.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Vuplex.WebView;
using System;
using System.Net;
using Com2Verse.Logger;

#if ENABLE_CHEATING
namespace Com2Verse.Cheat
{
	[SuppressMessage("ReSharper", "UnusedMember.Local")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static partial class CheatKey
	{
		[MetaverseCheat("Cheat/Mice/CaptureBusinessCardImage")]
		private static async UniTask CaptureBusinessCardImage()
		{
			var avatarInfo = AvatarInfo.GetTestInfo();
			var texture = await BusinessCardRT.CreateAsync(avatarInfo);
			var imageBytes = texture.EncodeToPNG();
			
			var assetDirectory = Application.dataPath + "/_Output";
			if (!System.IO.Directory.Exists(assetDirectory))
				System.IO.Directory.CreateDirectory(assetDirectory);
			
			System.IO.File.WriteAllBytes($"{assetDirectory}/BusinessCardImage.png", imageBytes);
#if UNITY_EDITOR
			UnityEditor.AssetDatabase.Refresh();
#endif
		}

		[MetaverseCheat("Cheat/Mice/ShowBusinessCardBook")]
		private static void ShowBusinessCardBook()
		{
			MiceBusinessCardBookViewModel.ShowView();
		}
		
		[MetaverseCheat("Cheat/Mice/StartMiceService")]
		private static void StartMiceService()
		{
			Protocols.DestinationLogicalAddress.SetServerID(Protocols.ServerType.Logic, (long)eServiceID.WORLD);
			User.Instance.ChangeUserData((long)eServiceID.WORLD);
			Commander.Instance.RequestServiceChange(MiceService.MICE_CONVENTIONCENTER_ID);
		}
		[MetaverseCheat("Cheat/Mice/StopMiceService")]
		private static void StopMiceService()
		{
			Commander.Instance.LeaveBuildingRequest();
		}

		[MetaverseCheat("Cheat/Mice/EnterMiceLobby")]
		private static void EnterMiceLobby()
		{
			MiceService.Instance.RequestEnterMiceLobby().Forget();
		}

		[MetaverseCheat("Cheat/Mice/EnterMiceLounge")]
		private static void EnterMiceLounge(string eventID)
		{
			Process().Forget();

			async UniTask Process()
			{
				var eventIDLong = long.Parse(eventID);
				var response = await MiceWebClient.Test.LoungePost_EventId_AccountId(eventIDLong, User.Instance.CurrentUserData.ID);
				if (response.Result.HttpStatusCode == HttpStatusCode.OK)
					await MiceService.Instance.RequestEnterMiceLounge(eventIDLong.ToMiceEventID());
			}
		}
		
		[MetaverseCheat("Cheat/Mice/EnterMiceFreeLounge")]
		private static void EnterMiceFreeLounge(string eventID)
		{
			MiceService.Instance.RequestEnterMiceFreeLounge(long.Parse(eventID).ToMiceEventID()).Forget();
		}
		[MetaverseCheat("Cheat/Mice/EnterMiceHall")]
		private static void EnterMiceHall(string sessionID)
		{
			Process().Forget();
			
			async UniTask Process()
			{
				var sessionIDLong = long.Parse(sessionID);
				var response = await MiceWebClient.Test.HallPost_SessionId_AccountId(sessionIDLong, User.Instance.CurrentUserData.ID);
				if (response.Result.HttpStatusCode == HttpStatusCode.OK)
					MiceService.Instance.RequestEnterMiceHall(long.Parse(sessionID).ToMiceSessionID()).Forget();
			}
		}
		[MetaverseCheat("Cheat/Mice/SpaceTeleport(f-0300~0302)")]
		private static void TeleportMice(string spaceId = "f-0301")
		{
			Commander.Instance.TeleportUserSpaceRequest(spaceId);
		}

		[MetaverseCheat("Cheat/Mice/ChangeMiceState")]
		private static void ChangeMiceState(string index)
		{
			eMiceServiceState stateType = (eMiceServiceState)int.Parse(index);
			MiceService.Instance.ChangeCurrentState(stateType);
		}
		[MetaverseCheat("Cheat/Mice/UserInteractionState")]
		private static void ChangeMiceUserInteractionState(string interactionTypeIndex)
		{
			eMiceUserInteractionState stateType = (eMiceUserInteractionState)int.Parse(interactionTypeIndex);
			MiceService.Instance.SetUserInteractionState(stateType);
		}
		[MetaverseCheat("Cheat/Mice/CreateDummyAvatar")]
		private static void CreateDummyAvatar(string columns = "1", string rows = "1", string spacingX = "1.0", string spacingY = "1.0", string yaw     = "0.0")
		{
			var columnsInt      = int.Parse(columns);
			var rowsInt         = int.Parse(rows);
			var spacingXFloat   = float.Parse(spacingX);
			var spacingYFloat   = float.Parse(spacingY);
			var yawFloat        = float.Parse(yaw);
			var offsetXFloat    = -((float)columnsInt * 0.5f * spacingXFloat);
			var offsetYFloat    = -((float)rowsInt    * 0.5f * spacingYFloat);
			var dummyAvatarRoot = GameObject.Find("DummyAvatarRoot");
			if (dummyAvatarRoot != null)
			{
				GameObject.DestroyImmediate(dummyAvatarRoot);
			}

			dummyAvatarRoot = new GameObject("DummyAvatarRoot");
			if (User.InstanceExists)
			{
				dummyAvatarRoot.transform.position = User.Instance.CharacterObject.transform.position;
			}
			CreateAll().Forget();

			async UniTask CreateAll()
			{
				for (int row = 0; row < rowsInt; row++)
				{
					for (int column = 0; column < columnsInt; column++)
					{
						await Create(column, row, spacingXFloat, spacingYFloat, offsetXFloat, offsetYFloat, yawFloat);
					}
				}
			}

			async UniTask Create(int column, int row, float spacingX, float spacingY, float offsetX, float offsetY, float yaw)
			{
				var avatarInfo = AvatarInfo.CreateTestInfo(true, false);
				var avatar     = await AvatarCreator.CreateAvatarAsync(avatarInfo, eAnimatorType.WORLD, Vector3.zero, (int)Define.eLayer.CHARACTER);
				avatar.transform.parent = dummyAvatarRoot.transform;
				avatar.transform.SetLocalPositionAndRotation(new Vector3(column * spacingX + offsetX, 0.0f, row * spacingY + offsetY), Quaternion.Euler(0.0f, yaw, 0.0f));
				avatar.transform.localScale = Vector3.one;
				avatar.gameObject.SetActive(true);
				avatar.GetComponent<GhostAvatarController>().IsTestAvatar    = true;
				avatar.GetComponent<GhostAvatarController>().ModelTypeString = avatarInfo.AvatarType.ToString();
			}
		}

		[MetaverseCheat("Cheat/Mice/CreateSeatDummyAvatar")]
		private static void CreateSeatDummyAvatar(string fillRate = "1.0")
		{
			var controller = GameObject.FindObjectOfType<MiceSeatController>();
			if (controller == null) return;

			controller.CreateDummyAvatars(float.Parse(fillRate));
		}

        [MetaverseCheat("Cheat/Mice/ShowUIPopupSessionList")]
        public static async UniTask ShowUIPopupSessionList()
        {
            await MiceService.Instance.ShowUIPopupSessionList();
        }

        [MetaverseCheat("Cheat/Mice/ShowUIPopupSessionInfo")]
        public static async UniTask ShowUIPopupSessionInfo()
        {
            await MiceService.Instance.ShowUIPopupSessionInfo();
        }

        [MetaverseCheat("Cheat/Mice/ShowUIPopupPrizeInfo")]
        public static async UniTask ShowUIPopupPrizeInfo()
        {
			await MiceUIGachaMachineGiftInfoViewModel.ShowView(new List<PrizeInfo>());
        }


        [MetaverseCheat("Cheat/Mice/ShowParticipantList")]
        private static UniTask ShowParticipantList()
			=> MiceUIParticipantListViewModel.ShowView();

        [UnityEngine.Scripting.Preserve]
        [Cheat.MetaverseCheat("Cheat/Mice/SessionQuestionList/Show")]
        private static UniTask MiceTest_SessionQuestionList_Show()
           => MiceUISessionQuestionListViewModel.ShowView();


        [UnityEngine.Scripting.Preserve]
        [Cheat.MetaverseCheat("Cheat/Mice/SessionQuestionList/Write")]
        private static async UniTask SessionQuestionListWrite(string questionTitle = "질문 제목", string questionDescription = "질문 내용")
        {
            var result = await MiceWebClient.Question.WritePost(new MiceWebClient.Entities.QuestionCreateRequest()
            {
                QuestionTitle = questionTitle,
                QuestionDescription = questionDescription,
                SessionId = MiceService.Instance.SessionID
            });

            if (result)
            {
                (
                    $"정상적으로 질문을 올렸습니다.\r\n" +
                    $"(번호:{result.Data.QuestionSeq} 시간:{result.Data.CreateDateTime})"
                )
                .ShowAsNotice();
            }
            else
            {
                (
                    $"실패 했네요?\r\n" +
                    $"코드:{result.Result.HttpStatusCode}(Mice:{result.Result.MiceStatusCode})\r\n" +
                    $"이유:{result.Result.Reason}"
                )
                .ShowAsNotice();
            }
        }

        [Cheat.MetaverseCheat("Cheat/Mice/OperatorCameraSequence")]
        private static void OperatorCameraSequence()
        {
	        if (MiceService.Instance.IsPlayingOperatorCameraSequence)
		        MiceService.Instance.StopOperatorCameraSequence();
	        else
		        MiceService.Instance.StartOperatorCameraSequence();
        }

        [Cheat.MetaverseCheat("Cheat/Mice/OperatorQuestionerSequence")]
        private static void OperatorQuestionerSequence()
        {
	        if (MiceService.Instance.IsPlayingOperatorQuestionerSequence)
		        MiceService.Instance.StopOperatorQuestionerSequence();
	        else
		        MiceService.Instance.StartOperatorQuestionerSequence();
        }

        [MetaverseCheat("Cheat/Mice/ShowConferencePhotoShoot")]
        private static UniTask ShotConferencePhotoShoot()
            => MiceUIConferencePhotoShootViewModel.ShowView();

    }
}
#endif

#if ENABLE_CHEATING
namespace Com2Verse.Cheat
{
	public static partial class CheatKey
	{
		private const string URL_SOURCE_HLS = "https://test-streams.mux.dev/x36xhzz/x36xhzz.m3u8";
		private const string URL_SOURCE_OGG = "https://download.blender.org/peach/trailer/trailer_400p.ogg";
		private const string URL_SOURCE_MP4 = "file:///C:/Users/user/Downloads/_battle01.mp4";

		[MetaverseCheat("Cheat/Mice/Test WebView Video")]
		private static void TestWebViewVideo(string type = "hls")
		{
			WebHelper.TrySetAutoplayEnabled(true);

			var isHLS = string.Compare(type, "hls", true) == 0;
			string url;
			switch (type)
			{
				default:
				case "hls": isHLS = true; url = MiceService.GetHLSVideoWebPage(URL_SOURCE_HLS, MiceService.MEDIA_TYPE_CUSTOM_HLS, false, true); break;
				case "ogg": url = MiceService.GetHLSVideoWebPage(URL_SOURCE_OGG, MiceService.MEDIA_TYPE_VIDEO_OGG, true, true); break;
				case "mp4": url = MiceService.GetHLSVideoWebPage(URL_SOURCE_MP4, MiceService.MEDIA_TYPE_VIDEO_MP4, true, true); break;
			}

			UIManager.Instance.ShowPopupVideoWebView(new Vector2(1280, 720), url, !isHLS);
		}

		[MetaverseCheat("Cheat/Mice/ThrowIfCancellationRequested Test")]
		private static async UniTask TestThrowIfCancellationRequested()
		{
			System.Threading.CancellationTokenSource cts = new();
			var token = cts.Token;

			try
			{
				Logger.C2VDebug.LogCategory("Mice", "Begin Test.");

				UniTask.Void(async () =>
				{
					await UniTask.Delay(2000);

					Logger.C2VDebug.LogCategory("Mice", "Cancel!");

					cts.Cancel();
				});

				Logger.C2VDebug.LogCategory("Mice", "Wait for 5 seconds");

				await UniTask.Delay(5000).WithCancellationToken(token);

				Logger.C2VDebug.LogCategory("Mice", "Test Completed.");
			}
			catch (Exception e)
			{
				Logger.C2VDebug.LogErrorCategory("Mice", e);
			}
			finally
			{
				cts.Dispose();
			}
		}

        [MetaverseCheat("Cheat/Mice/Test PostCode WebView")]
        private static async UniTask TestPostCodeWebView()
        {
			var data = await UIManager.Instance.ShowPopupPostCodeWebView();

			C2VDebug.LogCategory("TestPostCode", $"(valid:{data.isValid}) postcode:'{data.postcode}', address:'{data.address}', extra:'{data.extraAddress}'");
		}


  //      [MetaverseCheat("Cheat/Mice/Test Prize Recipient Info")]
  //      private static async UniTask TestPrizeRecipientInfo()
  //      {
		//	await MiceUIPrizeDrawingMachineInputInfoViewModel.ShowView();
		//}

        [MetaverseCheat("Cheat/Mice/Test MiceWebView")]
        private static void TestMiceWebView()
        {
			UIManager.Instance
				.CreatePopup
				(
					"UI_Popup_MiceWebView",
					guiView =>
					{
						guiView.Show();
					}
				)
				.Forget();
        }

        [MetaverseCheat("Cheat/Mice/Apply Staff Fashion Items")]
        private static void ApplyStaffFashionItems()
        {
	        MiceService.Instance.ApplyStaffFashionItems();
        }
	}
}
#endif

#if ENABLE_CHEATING
namespace Com2Verse.Cheat
{
	public static partial class CheatKey
	{
		[MetaverseCheat("Cheat/Mice/ShowUIPopupMiceApp")]
		private static void ShowUIPopupMiceApp()
		{
			UIManager.Instance
				.CreatePopup
				(
					"UI_Popup_MiceApp",
					guiView =>
					{
						guiView.Show();
						
						var viewModel = guiView.ViewModelContainer.GetViewModel<MiceAppViewModel>();
						viewModel.SetViewMode(MiceAppViewModel.eViewMode.DEFAULT);
					}
				)
				.Forget();
		}
	}
}
#endif



