/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingRoomMyProfileViewModel.cs
* Developer:	ksw
* Date:			2023-05-12 14:42
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/


using System;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Com2Verse.Organization;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com2Verse.UI
{
	public sealed class MeetingRoomProfileViewModel : ViewModel
	{
		private string  _meetingRoomUserProfileName;
		private string  _meetingRoomUserProfilePositionLevelDept;
		private string  _meetingRoomUserProfileAffiliation;
		private string  _meetingRoomUserProfileTelNo;
		private string  _meetingRoomUserProfileMail;
		private string  _meetingRoomUserProfileWork;
		private Texture _meetingRoomUserProfileIcon;
		private bool    _meetingRoomUserProfileIsOpen;

		private GUIView _guiView;

		public GUIView GUIView
		{
			get => _guiView;
			set => SetProperty(ref _guiView, value);
		}

		public CommandHandler CommandProfileClose { get; }

		public MeetingRoomProfileViewModel()
		{
			CommandProfileClose = new CommandHandler(CloseProfilePopup);
		}

		public async UniTask SetSelfProfile()
		{
			var memberModel = await DataManager.Instance.GetMyselfAsync();
			if (memberModel == null)
				return;

			MeetingRoomUserProfileName              = memberModel.Member.MemberName;
			MeetingRoomUserProfilePositionLevelDept = memberModel.GetPositionLevelTeamStr();
			MeetingRoomUserProfileAffiliation       = memberModel.Affiliation;
			MeetingRoomUserProfileTelNo             = memberModel.GetFormattedTelNo();
			MeetingRoomUserProfileMail              = memberModel.Member.MailAddress;
			MeetingRoomUserProfileWork              = memberModel.Member.Task;
			await Util.DownloadTexture(memberModel.Member.PhotoPath, (success, texture) => { MeetingRoomUserProfileIcon = texture; });
		}

		public async UniTask SetProfile(MemberModel memberModel)
		{
			if (memberModel == null)
				return;

			MeetingRoomUserProfileName              = memberModel.Member.MemberName;
			MeetingRoomUserProfilePositionLevelDept = memberModel.GetPositionLevelTeamStr();
			MeetingRoomUserProfileAffiliation       = memberModel.Affiliation;
			MeetingRoomUserProfileTelNo             = memberModel.GetFormattedTelNo();
			MeetingRoomUserProfileMail              = memberModel.Member.MailAddress;
			MeetingRoomUserProfileWork              = memberModel.Member.Task;
			await Util.DownloadTexture(memberModel.Member.PhotoPath, (success, texture) => { MeetingRoomUserProfileIcon = texture; });
		}

		public void SetGuestProfile(string userName)
		{
			MeetingRoomUserProfileName              = userName;
			MeetingRoomUserProfilePositionLevelDept = String.Empty;
			MeetingRoomUserProfileAffiliation       = String.Empty;
			MeetingRoomUserProfileTelNo             = String.Empty;
			MeetingRoomUserProfileMail              = String.Empty;
			MeetingRoomUserProfileWork              = String.Empty;
			MeetingRoomUserProfileIcon              = null;
		}

		private void CloseProfilePopup()
		{
			GUIView.Hide();
		}

		public string MeetingRoomUserProfileName
		{
			get => _meetingRoomUserProfileName;
			private set => SetProperty(ref _meetingRoomUserProfileName, value);
		}

		public string MeetingRoomUserProfilePositionLevelDept
		{
			get => _meetingRoomUserProfilePositionLevelDept;
			private set => SetProperty(ref _meetingRoomUserProfilePositionLevelDept, value);
		}

		public string MeetingRoomUserProfileAffiliation
		{
			get => _meetingRoomUserProfileAffiliation;
			private set => SetProperty(ref _meetingRoomUserProfileAffiliation, value);
		}

		public string MeetingRoomUserProfileTelNo
		{
			get => _meetingRoomUserProfileTelNo;
			private set => SetProperty(ref _meetingRoomUserProfileTelNo, value);
		}

		public string MeetingRoomUserProfileMail
		{
			get => _meetingRoomUserProfileMail;
			private set => SetProperty(ref _meetingRoomUserProfileMail, value);
		}

		public string MeetingRoomUserProfileWork
		{
			get => _meetingRoomUserProfileWork;
			private set => SetProperty(ref _meetingRoomUserProfileWork, value);
		}

		public Texture MeetingRoomUserProfileIcon
		{
			get => _meetingRoomUserProfileIcon;
			private set => SetProperty(ref _meetingRoomUserProfileIcon, value);
		}
	}
}
