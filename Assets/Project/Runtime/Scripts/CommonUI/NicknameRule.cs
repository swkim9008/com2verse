/*===============================================================
* Product:		Com2Verse
* File Name:	NicknameRule.cs
* Developer:	eugene9721
* Date:			2023-03-17 18:14
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System.Text.RegularExpressions;
using Com2Verse.InputSystem;
using Com2Verse.Logger;
using Com2Verse.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Com2Verse
{
	public static class NicknameRule
	{
		public enum eErrorReason
		{
			NO_ERROR,
			TOO_SHORT,
			TOO_LONG,
			USE_SPECIAL_CHARACTERS,
			USE_CONSONANT_VOWEL,
			USE_WHITE_SPACE,
			DUPLICATE_NICKNAME,
			BANNED_WORD,
		}

		private const string ErrorSpecialCharacterKey = "UI_Nickname_Toast_Msg_SpecialCharacter";
		private const string ErrorSpaceKey            = "UI_Nickname_Toast_Msg_Space";
		private const string ErrorIncompleteKey       = "UI_Nickname_Toast_Msg_Incomplete";
		private const string ErrorProhibitedKey       = "UI_Nickname_Toast_Msg_Prohibited";
		private const string ErrorMinimumKey          = "UI_Nickname_Toast_Msg_Minimum";
		private const string ErrorDuplicateKey        = "UI_Nickname_Toast_Msg_Duplicate";

		private const string PopupResName = "UI_Popup_NicknameRule";

		private const string NickNamePattern       = @"^[0-9a-zA-Z가-힣]+$";
		private const string ConsonantVowelPattern = @"[ㄱ-ㅎㅏ-ㅣ]+";
		private const string WhiteSpacePattern      = @"\s+";

		private const int MinLength = 2;
		private const int MaxLength = 12;

		private static bool _isOnNicknameRulePopup;
		private static GUIView? _lastView;
		private static string _lastInputString = string.Empty;

		private static OverrideInputCompositionString? _overrideInput = null;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			_lastView              = null;
			_isOnNicknameRulePopup = false;
			_lastInputString       = string.Empty;
			_overrideInput         = null;
		}

		public static void ShowNicknameRulePopup()
		{
			if (_isOnNicknameRulePopup) return;
			_isOnNicknameRulePopup = true;

			UIManager.Instance.CreatePopup(PopupResName, (guiView) =>
			{
				guiView.NeedDimmedPopup = true;
				guiView.Show();
				_lastView = guiView;

				var viewModel = ViewModelManager.InstanceOrNull?.Get<NicknameRuleViewModel>();
				if (viewModel != null)
					viewModel.InputString = string.Empty;

				_overrideInput = InputSystemManager.Instance.SetOverrideInput<OverrideInputCompositionString>();
			}).Forget();
		}

		public static void HideNicknameRulePopup()
		{
			if (_lastView != null)
				_lastView.Hide();
			_lastView = null;
			_isOnNicknameRulePopup = false;

			InputSystemManager.InstanceOrNull?.SetOverrideInput<BaseInput>();
			_overrideInput = null;
		}

		public static void SetShowCompositionString(bool value)
		{
			if (_overrideInput != null)
				_overrideInput.EnableCompositionString = value;
		}

		public static bool IsTooLong(string? nickname)
		{
			var length = Utils.Util.GetAsciiLength(nickname);
			return length > MaxLength;
		}

		public static bool IsFull(string? nickname)
		{
			var length = Utils.Util.GetAsciiLength(nickname);
			return length >= MaxLength;
		}

		/// <summary>
		/// 서버 연결 없이 닉네임 규칙만을 체크하는 함수
		/// 닉네임 확인 요청을 보내기 전, 1차적인 규칙체크를 위해 사용
		/// </summary>
		/// <param name="nickname">체크할 닉네임</param>
		/// <param name="ruleErrorReason">룰 규칙 위반 내용</param>
		/// <returns></returns>
		public static bool IsValid(string nickname, out eErrorReason ruleErrorReason)
		{
			_lastInputString = nickname;
			ruleErrorReason = eErrorReason.NO_ERROR;

			// 1. 최소 글자 수 미충족 ( 문자열이 없는 경우 )
			if (string.IsNullOrEmpty(nickname))
			{
				ruleErrorReason = eErrorReason.TOO_SHORT;
				return false;
			}

			var length = Utils.Util.GetAsciiLength(nickname);

			// 1. 최소 글자 수 미충족
			if (length < MinLength)
			{
				ruleErrorReason = eErrorReason.TOO_SHORT;
				return false;
			}

			// 3. 최대 글자수 초과
			if (length > MaxLength)
			{
				ruleErrorReason = eErrorReason.TOO_LONG;
				return false;
			}
			var consonantVowelMatch = Regex.Match(nickname, ConsonantVowelPattern);

			// 2. 완성되지 않은 문자 사용
			if (consonantVowelMatch.Success)
			{
				ruleErrorReason = eErrorReason.USE_CONSONANT_VOWEL;
				return false;
			}

			var patternMatch    = Regex.Match(nickname, NickNamePattern);
			var whiteSpaceMatch = Regex.Match(nickname, WhiteSpacePattern);

			// 3. 띄어쓰기 사용
			if (whiteSpaceMatch.Success)
			{
				ruleErrorReason = eErrorReason.USE_WHITE_SPACE;
				return false;
			}

			// 4. 특수 문자 사용
			if (!patternMatch.Success)
			{
				ruleErrorReason = eErrorReason.USE_SPECIAL_CHARACTERS;
				return false;
			}

			return true;
		}

		public static async UniTask<eErrorReason> IsValidAsync(string nickName)
		{
			if (IsValid(nickName, out var result))
			{
				if (await BannedWords.BannedWords.HasBannedWordAsync(nickName))
					result = eErrorReason.BANNED_WORD;
			}
			return result;
		}

		private static void OnServerError()
		{
			var viewModel = ViewModelManager.Instance.Get<NicknameRuleViewModel>();
			viewModel?.OnErrorResponse();
		}

		public static void OnTimeoutError()
		{
			OnServerError();

			var message = $"{Localization.Instance.GetString("UI_Error_Timeout_Popup_Desc")}\n[Timeout]";
			UIManager.Instance.ShowPopupCommon(message);
			C2VDebug.LogError(message);
		}

		public static void OnNicknameDuplicateErrorOnServer()
		{
			OnServerError();
			ShowNicknameErrorToastMessage(eErrorReason.DUPLICATE_NICKNAME);
		}

		public static bool OnNicknameRuleErrorOnServer(Protocols.ErrorCode errorCode)
		{
			OnServerError();

			switch (errorCode)
			{
				case Protocols.ErrorCode.AvatarNicknameBasicRule:
					if (!IsValid(_lastInputString, out var reason))
					{
						ShowNicknameErrorToastMessage(reason);
						return true;
					}

					// 기존 입력 스트링으론 알 수 없는 에러 발생
					ShowNicknameErrorToastMessage(eErrorReason.BANNED_WORD);
					return true;
				case Protocols.ErrorCode.AvatarNicknameBannedword:
					ShowNicknameErrorToastMessage(eErrorReason.BANNED_WORD);
					return true;
				case Protocols.ErrorCode.AvatarNicknameDuplicated:
					ShowNicknameErrorToastMessage(eErrorReason.DUPLICATE_NICKNAME);
					return true;
			}

			return false;
		}

		public static void ShowNicknameErrorToastMessage(eErrorReason reason)
		{
			switch (reason)
			{
				case eErrorReason.TOO_SHORT:
					UIManager.Instance.SendToastMessage(Localization.Instance.GetString(ErrorMinimumKey), toastMessageType: UIManager.eToastMessageType.WARNING);
					break;
				case eErrorReason.USE_SPECIAL_CHARACTERS:
					UIManager.Instance.SendToastMessage(Localization.Instance.GetString(ErrorSpecialCharacterKey), toastMessageType: UIManager.eToastMessageType.WARNING);
					break;
				case eErrorReason.USE_CONSONANT_VOWEL:
					UIManager.Instance.SendToastMessage(Localization.Instance.GetString(ErrorIncompleteKey), toastMessageType: UIManager.eToastMessageType.WARNING);
					break;
				case eErrorReason.USE_WHITE_SPACE:
					UIManager.Instance.SendToastMessage(Localization.Instance.GetString(ErrorSpaceKey), toastMessageType: UIManager.eToastMessageType.WARNING);
					break;
				case eErrorReason.DUPLICATE_NICKNAME:
					UIManager.Instance.SendToastMessage(Localization.Instance.GetString(ErrorDuplicateKey), toastMessageType: UIManager.eToastMessageType.WARNING);
					break;
				case eErrorReason.BANNED_WORD:
				case eErrorReason.TOO_LONG:
					UIManager.Instance.SendToastMessage(Localization.Instance.GetString(ErrorProhibitedKey), toastMessageType: UIManager.eToastMessageType.WARNING);
					break;
			}
		}
	}
}
