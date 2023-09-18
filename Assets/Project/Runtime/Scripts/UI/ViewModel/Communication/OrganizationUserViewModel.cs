/*===============================================================
* Product:		Com2Verse
* File Name:	CommunicationUserViewModel.cs
* Developer:	urun4m0r1
* Date:			2022-06-20 12:45
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

#nullable enable

using System;
using System.Linq;
using System.Net;
using Com2Verse.Data;
using Com2Verse.Logger;
using Com2Verse.MeetingReservation;
using Com2Verse.Network;
using Com2Verse.Organization;
using Com2Verse.WebApi.Service;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Com2Verse.UI
{
	[UsedImplicitly, ViewModelGroup("Communication")]
	public sealed class OrganizationUserViewModel : ViewModelBase
	{
		public long Uid { get; }

		public event Action<OrganizationUserViewModel>? EmployeePayloadChanged;
		public event Action<OrganizationUserViewModel>? NameChanged;

		private MemberModel? _memberModel;

		private string? _userName;
		private string? _nameLevelDeptInfo;
		private string? _positionLevelDeptInfo;

		private Texture? _userProfileIcon;

		public OrganizationUserViewModel(long uid) => Uid = uid;

		public async UniTask FetchOrganizationInfo()
		{
			if (CurrentScene.SpaceCode is eSpaceCode.MEETING)
			{
				await TryUpdateMeetingInfo();

				// [게스트] 4. 이 유저가 게스트인 경우
				if (MeetingReservationProvider.IsGuest(Uid))
				{
					await FetchGuestInfo();
				}
				// [게스트] 5. 이 유저는 게스트가 아니지만, 내가 게스트인 경우 (= 다른 회사의 조직도를 받아올 수 없는 경우)
				else if (MeetingReservationProvider.IsGuest())
				{
					await FetchOtherGroupMemberInfo();
				}
				// [게스트] 6. 이 유저는 게스트도 아니고, 내가 게스트도 아닌 경우 (= 우리 조직도를 받아올 수 있는 경우)
				else
				{
					await FetchSelfGroupMemberInfo();
				}
			}
			else
			{
				await FetchSelfGroupMemberInfo();
			}
		}

		private async UniTask TryUpdateMeetingInfo()
		{
			var meetingInfo = MeetingReservationProvider.EnteredMeetingInfo;
			if (meetingInfo == null)
			{
				C2VDebug.LogErrorMethod(nameof(OrganizationUserViewModel), "EnteredMeetingInfo is null");
				return;
			}

			var response = await Commander.Instance.RequestMeetingInfoAsync(meetingInfo.MeetingId);
			if (response.StatusCode is not HttpStatusCode.OK || response.Value?.Code is not Components.OfficeHttpResultCode.Success)
			{
				C2VDebug.LogErrorMethod(nameof(OrganizationUserViewModel), $"RequestMeetingInfoAsync failed: {response.StatusCode}, {response.Value?.Code}");
				return;
			}

			MeetingReservationProvider.SetMeetingInfo(response.Value?.Data!);
		}

		private async UniTask FetchGuestInfo()
		{
			var meetingMember = MeetingReservationProvider.EnteredMeetingInfo?.MeetingMembers?.FirstOrDefault(x => x.AccountId == Uid);
			if (meetingMember == null)
			{
				C2VDebug.LogErrorMethod(nameof(OrganizationUserViewModel), $"MeetingMember is null: {Uid.ToString()}");
				return;
			}

			var guestName = meetingMember.MemberName;

			UserName              = ZString.Format("{0} (G)", guestName);
			NameLevelDeptInfo     = ZString.Format("{0} (G)", guestName);
			PositionLevelDeptInfo = "Guest";

			await UniTask.CompletedTask;
		}

		private async UniTask FetchOtherGroupMemberInfo()
		{
			var meetingMember = MeetingReservationProvider.EnteredMeetingInfo?.MeetingMembers?.FirstOrDefault(x => x.AccountId == Uid);
			if (meetingMember == null)
			{
				C2VDebug.LogErrorMethod(nameof(OrganizationUserViewModel), $"MeetingMember is null: {Uid.ToString()}");
				return;
			}

			var memberName   = meetingMember.MemberName;
			var levelName    = "-";          // FIXME: [게스트] meetingInfo.LevelName;
			var teamName     = "-";          // FIXME: [게스트] meetingInfo.TeamName;
			var positionName = "-";          // FIXME: [게스트] meetingInfo.PositionName;
			var profileUrl   = string.Empty; // FIXME: [게스트] meetingInfo.ProfileUrl;

			UserName              = memberName;
			NameLevelDeptInfo     = ZString.Format("{0}/{1}/{2}", memberName,   levelName, teamName);
			PositionLevelDeptInfo = ZString.Format("{0}/{1}/{2}", positionName, levelName, teamName);

			await Util.DownloadTexture(profileUrl, OnProfileIconLoaded);
		}


		private async UniTask FetchSelfGroupMemberInfo()
		{
			MemberModel = await DataManager.Instance.GetMemberAsync(Uid);
			if (MemberModel?.Member == null)
			{
				C2VDebug.LogErrorMethod(nameof(OrganizationUserViewModel), $"MemberModel is null: {Uid.ToString()}");
				return;
			}

			var memberName = MemberModel.Member.MemberName;
			var levelName  = MemberModel.Member.Level;
			var teamName   = MemberModel.TeamName;

			UserName              = memberName;
			NameLevelDeptInfo     = ZString.Format("{0}/{1}/{2}", memberName, levelName, teamName);
			PositionLevelDeptInfo = MemberModel.GetPositionLevelTeamStr();

			await Util.DownloadTexture(MemberModel.Member.PhotoPath, OnProfileIconLoaded);
		}

		private void OnProfileIconLoaded(bool success, Texture texture)
		{
			if (!success)
				return;

			UserProfileIcon = texture;
		}

#region ViewModelProperties
		public MemberModel? MemberModel
		{
			get => _memberModel;
			private set
			{
				SetProperty(ref _memberModel, value);
				EmployeePayloadChanged?.Invoke(this);
			}
		}

		public string? UserName
		{
			get => _userName;
			private set
			{
				SetProperty(ref _userName, value);
				NameChanged?.Invoke(this);
			}
		}

		public string? NameLevelDeptInfo
		{
			get => _nameLevelDeptInfo;
			private set => SetProperty(ref _nameLevelDeptInfo, value);
		}

		public string? PositionLevelDeptInfo
		{
			get => _positionLevelDeptInfo;
			private set => SetProperty(ref _positionLevelDeptInfo, value);
		}

		public Texture? UserProfileIcon
		{
			get => _userProfileIcon;
			private set
			{
				SetProperty(ref _userProfileIcon, value);
				InvokePropertyValueChanged(nameof(UserProfileIconExists), UserProfileIconExists);
			}
		}

		public bool UserProfileIconExists => UserProfileIcon != null;
#endregion // ViewModelProperties
	}
}
