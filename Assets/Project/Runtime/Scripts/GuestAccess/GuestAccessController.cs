/*===============================================================
* Product:		Com2Verse
* File Name:	GuestAccessController.cs
* Developer:	jhkim
* Date:			2023-06-15 20:06
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using Com2Verse.BannedWords;
using Com2Verse.Chat;
using Com2Verse.Communication;
using Com2Verse.InputSystem;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Com2Verse.PlayerControl;
using Com2Verse.UI;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using BannedWord = Com2Verse.BannedWords.BannedWords;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;
using ResponseRoomJoin = Com2Verse.HttpHelper.ResponseBase<Com2Verse.WebApi.Service.Components.RoomJoinResponseResponseFormat>;
using User = Com2Verse.Network.User;

namespace Com2Verse.GuestAccess
{
	public sealed class GuestAccessController
	{
		private enum eValidationType
		{
			NOT_GUEST,
			NO_TIME_TO_ENTER,
			INCORRECT_SPACE_CODE,
		}

#region Variables
		private static class StringKey
		{
			public static readonly string ErrorNotGuest = "UI_Guest_GuestObj_NotGuestPopup_Text";
			public static readonly string ErrorNoTimeToEnter = "UI_Guest_Login_NotTimeToEnter_Toast";
			public static readonly string ErrorIncorrentSpaceCode = "UI_Guest_Login_IncorrectSpaceCode_Toast";

			public static readonly string ErrorNickNameTooShort = "UI_Guest_Login_NicknameRule_Minimum_Toast";
			public static readonly string ErrorNickNameIncomplete = "UI_Guest_Login_NicknameRule_Incomplete_Toast";
			public static readonly string ErrorNickNameWhiteSpace = "UI_Guest_Login_NicknameRule_Space_Toast";
			public static readonly string ErrorNickNameSpecialChar = "UI_Guest_Login_NicknameRule_SpecialCharacter_Toast";
			public static readonly string ErrorNickNameBannedWord = "UI_Guest_Login_NicknameRule_Prohibited_Toast";
		}

		public static readonly int MinNickNameBytes = 2;
		public static readonly int MaxNickNameBytes = 12;

		private bool _requestLock = false;
		private Action _completeEnter = null;
#endregion // Variables

#region Binding Events
		public async UniTask RequestEnterAsync(string code, string nickName, Action completeAction)
		{
			if (_requestLock) return;
			_requestLock = true;

			_completeEnter = completeAction;
			var validationResult = await IsValidNickNameAsync(nickName);
			if (validationResult != NicknameRule.eErrorReason.NO_ERROR)
			{
				ShowToastMessage(validationResult);
				_requestLock = false;
				return;
			}

			SendRequestGuestJoinAsync(code, nickName);
		}
#endregion // Binding Events

#region Validation
		public int GetBytes(string nickName) => string.IsNullOrWhiteSpace(nickName) ? 0 : Utils.Util.GetAsciiLength(nickName);

		private async UniTask<NicknameRule.eErrorReason> IsValidNickNameAsync(string nickName)
		{
			if (NicknameRule.IsValid(nickName, out var result))
			{
				if (await HasBannedWordAsync(nickName))
					result = NicknameRule.eErrorReason.BANNED_WORD;
			}
			return result;
		}

		private static async UniTask<bool> HasBannedWordAsync(string nickName)
		{
			await PrepareBannedWordAsync();
			return BannedWord.IsReady && BannedWord.HasBannedWords(nickName);

			async UniTask PrepareBannedWordAsync()
			{
				if (!BannedWord.IsReady)
				{
					var available = await BannedWord.CheckAndUpdateAsync(AppDefine.Default);
					if (available)
					{
						await BannedWord.LoadAsync(AppDefine.Default);
						BannedWord.SetLanguageAll();
						BannedWord.SetCountryAll();
						BannedWord.SetUsageName();
					}
				}
			}
		}
#endregion // Validation

#region Network
		private async void SendRequestGuestJoinAsync(string code, string nickName)
		{
			NetworkUIManager.Instance.OnFieldChangeEvent += ClearEvent;

			UIManager.Instance.ShowWaitingResponsePopup();
			// 이동중 OSR/Input 막기
			PlayerController.Instance.SetStopAndCannotMove(true);
			User.Instance.DiscardPacketBeforeStandBy();
			
			await Commander.Instance.RequestGuestCheckAsync(code,
			                                                response =>
			                                                {
				                                                SendRequestMeetingInfoAsync(nickName, response.Value.Data.MeetingId);
			                                                },
			                                                error =>
			                                                {
				                                                ClearEvent();
			                                                });
		}

		private async void SendRequestMeetingInfoAsync(string nickName, long meetingId)
		{
			await Commander.Instance.RequestMeetingInfoAsync(meetingId,
			                                                 response =>
			                                                 {
				                                                 SendRequestGuestRoomJoin(nickName, meetingId, response.Value.Data);
			                                                 },
			                                                 error =>
			                                                 {
				                                                 ClearEvent();
			                                                 });
		}

		private async void SendRequestGuestRoomJoin(string nickName, long meetingId, MeetingInfoType meetingInfo)
		{
			await Commander.Instance.RequestGuestRoomJoinAsync(meetingId, nickName,
			                                                   response =>
			                                                   {
				                                                   ResponseGuestRoomJoin(response, meetingInfo, nickName);
			                                                   },
			                                                   error =>
			                                                   {
				                                                   ClearEvent();
			                                                   });
		}

		private void ResponseGuestRoomJoin(ResponseRoomJoin response, MeetingInfoType meetingInfo, string nickname)
		{
			MeetingReservationProvider.RoomId = response.Value.Data.RoomId;
			ChatManager.Instance.SetAreaMove(response.Value.Data.GroupId);

			MeetingReservationProvider.SetMeetingInfo(meetingInfo);
			var extraInfo = new ExtraInfo
			{
				Uid      = User.Instance.CurrentUserData.ID.ToString()!,
				Job      = "Guest",
				Name     = ZString.Format("{0} (G)", nickname),
				Position = "",
				Team     = "",
				Token    = "",
			};
			ChannelManagerHelper.AddChannel(response.Value.Data, MeetingReservationProvider.DisconnectRequestFromMediaChannel, nickname, extraInfo.ToString());
			_completeEnter?.Invoke();
		}

		private void ClearEvent()
		{
			NetworkUIManager.Instance.OnFieldChangeEvent -= ClearEvent;

			UIManager.Instance.HideWaitingResponsePopup();
			PlayerController.Instance.SetStopAndCannotMove(false);
			User.Instance.RestoreStandBy();
			_requestLock = false;
		}
#endregion // Network

#region Toast
		private void ShowToastMessage(eValidationType type)
		{
			var stringKey = string.Empty;
			var toastType = UIManager.eToastMessageType.WARNING;

			switch (type)
			{
				case eValidationType.NOT_GUEST:
					stringKey = StringKey.ErrorNotGuest;
					break;
				case eValidationType.NO_TIME_TO_ENTER:
					stringKey = StringKey.ErrorNoTimeToEnter;
					break;
				case eValidationType.INCORRECT_SPACE_CODE:
					stringKey = StringKey.ErrorIncorrentSpaceCode;
					break;
				default:
					break;
			}

			if (string.IsNullOrWhiteSpace(stringKey)) return;

			ShowToastMessage(toastType, Localization.Instance.GetString(stringKey));
		}
		private void ShowToastMessage(NicknameRule.eErrorReason error)
		{
			var stringKey = string.Empty;
			var toastType = UIManager.eToastMessageType.WARNING;

			switch (error)
			{
				case NicknameRule.eErrorReason.TOO_SHORT:
					stringKey = StringKey.ErrorNickNameTooShort;
					break;
				case NicknameRule.eErrorReason.TOO_LONG:
					break;
				case NicknameRule.eErrorReason.USE_SPECIAL_CHARACTERS:
					stringKey = StringKey.ErrorNickNameSpecialChar;
					break;
				case NicknameRule.eErrorReason.USE_CONSONANT_VOWEL:
					stringKey = StringKey.ErrorNickNameIncomplete;
					break;
				case NicknameRule.eErrorReason.USE_WHITE_SPACE:
					stringKey = StringKey.ErrorNickNameWhiteSpace;
					break;
				case NicknameRule.eErrorReason.DUPLICATE_NICKNAME:
					break;
				case NicknameRule.eErrorReason.BANNED_WORD:
					stringKey = StringKey.ErrorNickNameBannedWord;
					break;
				case NicknameRule.eErrorReason.NO_ERROR:
				default:
					break;
			}

			if (string.IsNullOrWhiteSpace(stringKey)) return;

			ShowToastMessage(toastType, Localization.Instance.GetString(stringKey));
		}
		private void ShowToastMessage(UIManager.eToastMessageType type, string message)
		{
			UIManager.Instance.SendToastMessage(message, toastMessageType: type);
		}
#endregion // Toast
	}
}
