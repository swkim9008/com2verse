/*===============================================================
* Product:    Com2Verse
* File Name:  GameLogicProcessor.cs
* Developer:  haminjeong
* Date:       2022-05-09 14:38
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Contents;
using Com2Verse.Loading;
using Com2Verse.Logger;
using Com2Verse.UI;
using JetBrains.Annotations;

namespace Com2Verse.Network
{
	[UsedImplicitly]
	[Channel(Protocols.Channels.GameLogic)]
	public sealed class GameLogicProcessor : BaseMessageProcessor
	{
		public override void Initialize()
		{
			SetMessageProcessCallback((int) Protocols.GameLogic.MessageTypes.LoginCom2VerseResponse,
			                          (payload) => Protocols.GameLogic.LoginCom2verseResponse.Parser.ParseFrom(payload),
			                          UI.PacketReceiver.Instance.OnLoginCom2VerseResponse);

			SetMessageProcessCallback((int) Protocols.GameLogic.MessageTypes.CreateAvatarResponse,
			                          (payload) => Protocols.GameLogic.CreateAvatarResponse.Parser.ParseFrom(payload),
			                          UI.PacketReceiver.Instance.OnCreateAvatarResponse);

			SetMessageProcessCallback((int)Protocols.GameLogic.MessageTypes.EnterWorldResponse,
			                          (payload) => Protocols.GameLogic.EnterWorldResponse.Parser.ParseFrom(payload),
			                          UI.PacketReceiver.Instance.OnEnterWorldResponse);

			SetMessageProcessCallback((int)Protocols.GameLogic.MessageTypes.EnterPlazaResponse,
			                          (payload) => Protocols.GameLogic.EnterPlazaResponse.Parser.ParseFrom(payload),
			                          UI.PacketReceiver.Instance.OnEnterPlazaResponse);

			SetMessageProcessCallback((int) Protocols.GameLogic.MessageTypes.StandInTriggerNotify,
			                          (payload) => Protocols.GameLogic.StandInTriggerNotify.Parser.ParseFrom(payload),
			                          UI.PacketReceiver.Instance.OnStandInTriggerNotify);

			SetMessageProcessCallback((int) Protocols.GameLogic.MessageTypes.GetoffTriggerNotify,
			                          (payload) => Protocols.GameLogic.GetOffTriggerNotify.Parser.ParseFrom(payload),
			                          UI.PacketReceiver.Instance.OnGetOffTriggerNotify);

			SetMessageProcessCallback((int) Protocols.GameLogic.MessageTypes.UsePortalResponse,
			                          (payload) => Protocols.GameLogic.UsePortalResponse.Parser.ParseFrom(payload),
			                          UI.PacketReceiver.Instance.OnUsePortalResponse);
			SetMessageProcessCallback((int)Protocols.GameLogic.MessageTypes.TeleportUserResponse,
			                          (payload) => Protocols.GameLogic.TeleportUserResponse.Parser.ParseFrom(payload),
			                          _ => { });

			SetMessageProcessCallback((int)Protocols.GameLogic.MessageTypes.CheckNicknameResponse,
			                          (payload) => Protocols.GameLogic.CheckNicknameResponse.Parser.ParseFrom(payload),
			                          UI.PacketReceiver.Instance.OnCheckNicknameResponse);

			SetMessageProcessCallback((int) Protocols.GameLogic.MessageTypes.MyGuestBookNotify,
			                          payload => Protocols.GameLogic.MyGuestBookNotify.Parser.ParseFrom(payload),
			                          BoardManager.Instance.OnMyGuestBookNotify);
			SetMessageProcessCallback((int) Protocols.GameLogic.MessageTypes.RegisterGuestBookResponse,
			                          payload => Protocols.GameLogic.RegisterGuestBookResponse.Parser.ParseFrom(payload),
			                          BoardManager.Instance.OnRegisterGuestBookResponse);
			SetMessageProcessCallback((int)Protocols.GameLogic.MessageTypes.ReadAiBotResponse,
			                          payload => Protocols.GameLogic.ReadAIBotResponse.Parser.ParseFrom(payload),
			                          BoardManager.Instance.OnReadAIBotResponse);

			// Service change
			SetMessageProcessCallback((int)Protocols.GameLogic.MessageTypes.ServiceChangeResponse,
			                          payload => Protocols.GameLogic.ServiceChangeResponse.Parser.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is Protocols.GameLogic.ServiceChangeResponse response)
					                          GameLogic.PacketReceiver.Instance.RaiseServiceChangeResponse(response);
			                          });
			SetMessageProcessCallback((int)Protocols.GameLogic.MessageTypes.LeaveBuildingResponse,
			                          payload => Protocols.GameLogic.LeaveBuildingResponse.Parser.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is Protocols.GameLogic.LeaveBuildingResponse response)
					                          GameLogic.PacketReceiver.Instance.RaiseLeaveBuildingResponse(response);
			                          });
			SetMessageProcessCallback((int)Protocols.GameLogic.MessageTypes.EnterChattingAreaNotify,
			                          payload => Protocols.GameLogic.EnterChattingAreaNotify.Parser.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is Protocols.GameLogic.EnterChattingAreaNotify response)
					                          GameLogic.PacketReceiver.Instance.RaiseEnterChattingAreaNotify(response);
			                          });
			SetMessageProcessCallback((int)Protocols.GameLogic.MessageTypes.ExitChattingAreaNotify,
			                          payload => Protocols.GameLogic.ExitChattingAreaNotify.Parser.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is Protocols.GameLogic.ExitChattingAreaNotify response)
					                          GameLogic.PacketReceiver.Instance.RaiseExitChattingAreaNotify(response);
			                          });

			// Smalltalk object
			SetMessageProcessCallback((int)Protocols.GameLogic.MessageTypes.EnterObjectInteractionSmallTalkNotify,
			                          payload => Protocols.GameLogic.EnterObjectInteractionSmallTalkNotify.Parser.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is Protocols.GameLogic.EnterObjectInteractionSmallTalkNotify response)
					                          GameLogic.PacketReceiver.Instance.RaiseObjectSmallTalkNotify(response);
			                          });
			SetMessageProcessCallback((int)Protocols.GameLogic.MessageTypes.ObjectInteractionEnterFailNotify,
			                          payload => Protocols.GameLogic.ObjectInteractionEnterFailNotify.Parser.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is Protocols.GameLogic.ObjectInteractionEnterFailNotify response)
					                          GameLogic.PacketReceiver.Instance.RaiseObjectInteractionEnterFailNotify(response);
			                          });

			// Audience
			SetMessageProcessCallback((int)Protocols.GameLogic.MessageTypes.EnterAudioMuxMicAreaNotify,
			                          payload => Protocols.GameLogic.EnterAudioMuxMicAreaNotify.Parser.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is Protocols.GameLogic.EnterAudioMuxMicAreaNotify response)
					                          GameLogic.PacketReceiver.Instance.RaiseEnterAudioMuxMicAreaNotify(response);
			                          });
			SetMessageProcessCallback((int)Protocols.GameLogic.MessageTypes.EnterAudioMuxCrowdAreaNotify,
			                          payload => Protocols.GameLogic.EnterAudioMuxCrowdAreaNotify.Parser.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is Protocols.GameLogic.EnterAudioMuxCrowdAreaNotify response)
					                          GameLogic.PacketReceiver.Instance.RaiseEnterAudioMuxCrowdAreaNotify(response);
			                          });
			
			// Maze
			SetMessageProcessCallback((int)Protocols.GameLogic.MessageTypes.EnterMazeResponse,
			                          payload => Protocols.GameLogic.EnterMazeResponse.Parser.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is Protocols.GameLogic.EnterMazeResponse response)
					                          C2VDebug.Log("EnterMazeResponse");
			                          });
			SetMessageProcessCallback((int)Protocols.GameLogic.MessageTypes.ExitMazeResponse,
			                          payload => Protocols.GameLogic.ExitMazeResposne.Parser.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is Protocols.GameLogic.ExitMazeResposne response)
				                          {
					                          C2VDebug.Log("ExitMazeResponse");
					                          if (PlayContentsManager.Instance.CurrentContents is not MazeController mazeController) return;
					                          mazeController.OnResultResponse(response);
				                          }
			                          });
			SetMessageProcessCallback((int)Protocols.GameLogic.MessageTypes.EscapeMazeResponse,
			                          payload => Protocols.GameLogic.EscapeMazeResponse.Parser.ParseFrom(payload),
			                          static message =>
			                          {
				                          if (message is Protocols.GameLogic.EscapeMazeResponse response)
				                          {
					                          C2VDebug.Log("EscapeMazeResponse");
					                          if (PlayContentsManager.Instance.CurrentContentsType != PlayContentsManager.eContentsType.MAZE) return;
					                          PlayContentsManager.Instance.ContentsEnd();
				                          }
			                          });

			// Advertisement
			SetMessageProcessCallback((int)Protocols.GameLogic.MessageTypes.MyPadAdvertisementNotify,
			                          (payload) => Protocols.GameLogic.MyPadAdvertisementNotify.Parser.ParseFrom(payload),
			                          UI.PacketReceiver.Instance.OnMyPadAdvertisementNotify);

			// Textboard
			SetMessageProcessCallback((int)Protocols.GameLogic.MessageTypes.UpdateTextBoardResponse,
			                          (payload) => Protocols.GameLogic.UpdateTextBoardResponse.Parser.ParseFrom(payload),
			                          static message => { });
		}

		public override void ErrorProcess(Protocols.Channels channel, int command, Protocols.ErrorCode errorCode)
		{
			UIManager.Instance.HideWaitingResponsePopup();
			switch (command)
			{
				case (int)Protocols.GameLogic.MessageTypes.ErrorCodeNotify:
				{
					var message = NetworkUIManager.Instance.GetProtocolErrorContext(errorCode);
					UIManager.Instance.ShowPopupCommon(message, allowCloseArea: false, onShowAction: (guiView) =>
					{
						void OnClosedAction(GUIView view)
						{
							guiView.OnClosedEvent -= OnClosedAction;
							if (errorCode is Protocols.ErrorCode.UserKick or Protocols.ErrorCode.UserAllKick)
							{
#if UNITY_EDITOR
								UnityEditor.EditorApplication.ExitPlaymode();
#else
								UnityEngine.Application.Quit((int)errorCode);
#endif
							}
							else
							{
								LoadingManager.Instance.ChangeScene<SceneLogin>();
							}
						}

						NetworkManager.Instance.Disconnect(true);
						guiView.OnClosedEvent += OnClosedAction;
					});
					return;
				}
				case (int)Protocols.GameLogic.MessageTypes.LoginCom2VerseResponse:
				{
					C2VDebug.LogError($"LoginOfficeResponse : {errorCode}");
					if (errorCode == Protocols.ErrorCode.ExpiredToken)
					{
						var message = NetworkUIManager.Instance.GetProtocolErrorContext(errorCode);
						NetworkManager.Instance.TokenExpired = true;
						UIManager.Instance.ShowPopupCommon(message, async () =>
						{
							await LoginManager.Instance.TryRefreshToken(() =>
							{
								NetworkManager.Instance.TokenExpired = false;
								NetworkManager.Instance.ResendMessage?.Invoke();
							});
						});
					}
					else
						base.ErrorProcess(channel, command, errorCode);
					return;
				}
				case (int)Protocols.GameLogic.MessageTypes.CreateAvatarResponse:
					var isShownToastPopup = NicknameRule.OnNicknameRuleErrorOnServer(errorCode);
					if (isShownToastPopup) return;
					break;
				case (int)Protocols.GameLogic.MessageTypes.CheckNicknameResponse:
					NicknameRule.OnNicknameDuplicateErrorOnServer();
					return;
			}
			base.ErrorProcess(channel, command, errorCode);
		}
	}
}
