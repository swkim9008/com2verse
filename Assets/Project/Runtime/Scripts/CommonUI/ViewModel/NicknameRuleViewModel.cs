/*===============================================================
* Product:		Com2Verse
* File Name:	NicknameRuleViewModel.cs
* Developer:	eugene9721
* Date:			2023-03-31 16:17
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using Com2Verse.Avatar;
using Com2Verse.Extension;
using Com2Verse.Loading;
using Com2Verse.Logger;
using Com2Verse.Network;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Protocols.GameLogic;

namespace Com2Verse.UI
{
	[ViewModelGroup("CommonUI")]
	public sealed class NicknameRuleViewModel : ViewModelBase
	{
		private const string NeedNicknameCheckMessageKey = "UI_Nickname_Toast_Msg_ClickCheckBtn";
		private const string CanCreateNicknameMessageKey = "UI_Nickname_Toast_Msg_Creatable";

		private const string InputPopupMessageKey = "UI_Nickname_Popup_Msg";
		// private const string DecidePopupMessageKey = "UI_Nickname_Popup_Msg_Decide";
#region Fields
		private string _inputString = string.Empty;
		private bool   _isAvailableNickname;
#endregion Fields

#region Command Properties
		[UsedImplicitly] public CommandHandler CancelButtonClicked        { get; }
		[UsedImplicitly] public CommandHandler ConfirmButtonClicked       { get; }
		[UsedImplicitly] public CommandHandler CheckNicknameButtonClicked { get; }
#endregion Command Properties

#region Properties
		[UsedImplicitly]
		public string InputString
		{
			get => _inputString;
			set
			{
				SetProperty(ref _inputString, NicknameRule.IsTooLong(value) ? _inputString : value);
				NicknameRule.SetShowCompositionString(!NicknameRule.IsFull(_inputString));
				IsAvailableNickname = false;

				InvokePropertyValueChanged(nameof(CanClickCheckDuplicateButton), CanClickCheckDuplicateButton);
			}
		}

		[UsedImplicitly]
		public bool IsAvailableNickname
		{
			get => _isAvailableNickname;
			set => SetProperty(ref _isAvailableNickname, value);
		}

		[UsedImplicitly]
		public bool CanClickCheckDuplicateButton => !string.IsNullOrEmpty(_inputString);

		[UsedImplicitly]
		public string PopupContentString => Localization.Instance.GetString(InputPopupMessageKey);
#endregion Properties
		public NicknameRuleViewModel()
		{
			ConfirmButtonClicked       = new CommandHandler(OnConfirmButtonClicked);
			CancelButtonClicked        = new CommandHandler(OnCancelButtonClicked);
			CheckNicknameButtonClicked = new CommandHandler(OnCheckNicknameButtonClicked);
		}

		private void OnConfirmButtonClicked()
		{
			if (IsAvailableNickname)
				RequestCreateAvatar();
			else
				UIManager.Instance.SendToastMessage(Localization.Instance.GetString(NeedNicknameCheckMessageKey), toastMessageType: UIManager.eToastMessageType.WARNING);
		}

		private void OnCancelButtonClicked()
		{
			OnCancelInInputStage();
		}

		private void OnCheckNicknameButtonClicked()
		{
			OnCheckNicknameAsync().Forget();
		}

		private async UniTask OnCheckNicknameAsync()
		{
			LoadingManager.Instance.Show(this);
			var reason = await NicknameRule.IsValidAsync(_inputString);
			if (reason != NicknameRule.eErrorReason.NO_ERROR)
			{
				NicknameRule.ShowNicknameErrorToastMessage(reason);
				LoadingManager.InstanceOrNull?.Hide(this);
				return;
			}

			PacketReceiver.Instance.OnCheckNicknameResponseEvent += OnResponseCheckNickname;
			Commander.Instance.CheckNicknameRequest(_inputString);
		}

		private void OnResponseCheckNickname(CheckNicknameResponse checkNicknameResponse)
		{
			PacketReceiver.Instance.OnCheckNicknameResponseEvent -= OnResponseCheckNickname;
			LoadingManager.InstanceOrNull?.Hide(this);
			IsAvailableNickname = true;

			UIManager.Instance.SendToastMessage(Localization.Instance.GetString(CanCreateNicknameMessageKey), toastMessageType: UIManager.eToastMessageType.NORMAL);
		}

		private void RequestCreateAvatar()
		{
			var avatar = AvatarMediator.Instance.AvatarCloset.CurrentAvatar;
			if (avatar.IsUnityNull() || avatar!.Info == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "Avatar is null");
				return;
			}

			PacketReceiver.Instance.OnCreateAvatarResponseEvent += OnResponseCreateAvatar;
			LoadingManager.Instance.Show(this);
			Commander.Instance.RequestCreateAvatar(avatar.Info.AvatarType, avatar.Info, _inputString);
		}

		private void OnResponseCreateAvatar(CreateAvatarResponse avatarResponse)
		{
			LoadingManager.InstanceOrNull?.Hide(this);
			PacketReceiver.Instance.OnCreateAvatarResponseEvent -= OnResponseCreateAvatar;

			var avatarMediator = AvatarMediator.Instance;
			var avatarCloset   = avatarMediator.AvatarCloset;
			var avatar         = avatarCloset.CurrentAvatar;
			if (avatar.IsUnityNull() || avatar!.Info == null || avatarResponse.CreateAvatar == null)
			{
				C2VDebug.LogErrorCategory(GetType().Name, "Avatar is null");
				return;
			}

			avatar.Info.Set(0, avatarResponse.CreateAvatar);
			avatarCloset.SetAvatarInfo(avatar.Info);
			avatarCloset.CurrentAvatar = avatar;

			avatarMediator.SetUserAvatar(avatarResponse.CreateAvatar);

			NicknameRule.HideNicknameRulePopup();
			LoginManager.Instance.CheckLoginQueue(AvatarCreateManager.Instance.ReadyToEnter, true);
		}

		public void OnErrorResponse()
		{
			IsAvailableNickname = false;
			LoadingManager.InstanceOrNull?.Hide(this);
		}

		private void OnCancelInInputStage()
		{
			NicknameRule.HideNicknameRulePopup();
		}
	}
}
