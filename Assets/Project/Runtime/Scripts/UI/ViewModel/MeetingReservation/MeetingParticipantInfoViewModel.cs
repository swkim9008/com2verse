/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingParticipantInfoViewModel.cs
* Developer:	tlghks1009
* Date:			2022-09-12 16:21
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using Com2Verse.Organization;
using Cysharp.Threading.Tasks;
using Protocols.OfficeMeeting;
using UnityEngine;
using MeetingUserType = Com2Verse.WebApi.Service.Components.MeetingMemberEntity;

namespace Com2Verse.UI
{
	[ViewModelGroup("MeetingReservation")]
	public sealed class MeetingParticipantInfoViewModel : ViewModelBase
	{
		private Texture _profileTexture;
		private string _profileName;
		private bool _setActiveCounter;
		private string _invitedUserCount;
		private bool _isOrganizer;

		public MeetingParticipantInfoViewModel(MeetingUserType meetingUserInfo)
		{
			var memberModel = DataManager.Instance.GetMember(meetingUserInfo.AccountId);
			if (memberModel != null)
			{
				if (!string.IsNullOrEmpty(memberModel.Member.PhotoPath))
				{
					Util.DownloadTexture(memberModel.Member.PhotoPath,(success, texture) => ProfileTexture = texture).Forget();
				}

				ProfileName = memberModel.Member.MemberName;
			}

			SetActiveCounter = false;
		}


		public Texture ProfileTexture
		{
			get => _profileTexture;
			set => SetProperty(ref _profileTexture, value);
		}

		public string ProfileName
		{
			get => _profileName;
			set => SetProperty(ref _profileName, value);
		}

		public bool SetActiveCounter
		{
			get => _setActiveCounter;
			set => SetProperty(ref _setActiveCounter, value);
		}

		public bool IsOrganizer
		{
			get => _isOrganizer;
			set
			{
				_isOrganizer = value;
				InvokePropertyValueChanged(nameof(IsOrganizer), value);
			}
		}

		public string InvitedUserCount
		{
			get => _invitedUserCount;
			set
			{
				_invitedUserCount = value;
				SetActiveCounter = true;
				base.InvokePropertyValueChanged(nameof(InvitedUserCount), value);
			}
		}
	}
}
