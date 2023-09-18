/*===============================================================
* Product:		Com2Verse
* File Name:	MeetingRoomReservationManager.cs
* Developer:	tlghks1009
* Date:			2022-08-29 10:20
* History:
* Documents:
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using Com2Verse.Communication;
using Com2Verse.Network;
using Com2Verse.Organization;
using Com2Verse.UI;
using Com2Verse.WebApi.Service;
using Cysharp.Threading.Tasks;
using MemberIdType = System.Int64;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;
using MeetingUserType = Com2Verse.WebApi.Service.Components.MeetingMemberEntity;
using AuthorityCode = Com2Verse.WebApi.Service.Components.AuthorityCode;
using User = Com2Verse.Network.User;

namespace Com2Verse.MeetingReservation
{
    public static class MeetingReservationProvider
    {
        /// <summary>
        /// 예약 캘린더
        /// </summary>
        public static MeetingCalendar MeetingCalendar { get; } = new();

        /// <summary>
        /// 현재 회의실에 입장 된 상태의 MeetingInfo
        /// </summary>
        public static MeetingInfoType EnteredMeetingInfo { get; private set; }

        /// <summary>
        /// 커넥팅 최대 참여자 수
        /// </summary>
        public static int MaxNumberOfParticipants = 30;
        /// <summary>
        /// 커넥팅 예약 변경 가능 시간
        /// </summary>
        public static readonly int AdmissionTime = 10;

        /// <summary>
        /// 참여중인 RoomId
        /// </summary>
        public static string RoomId;

        /// <summary>
        /// 현재 커넥팅이 녹음중인지
        /// </summary>
        public static bool IsRecording = false;

        /// <summary>
        /// MeetingInfo 정보가 변경되었을 때 호출해주는 콜백
        /// </summary>
        public static event Action OnMeetingInfoChanged;

        public static event Action OnDisconnectRequestFromMediaChannel;

        public static List<Components.MeetingTemplate> MeetingTemplates = new();

        public static void SetMeetingInfo(MeetingInfoType enteredMeetingInfo)
        {
            EnteredMeetingInfo      = enteredMeetingInfo;
            MaxNumberOfParticipants = EnteredMeetingInfo.MaxUsersLimit;
            OnMeetingInfoChanged?.Invoke();
        }

        public static void RemoveMeetingInfo()
        {
            EnteredMeetingInfo                  = null;
            OnMeetingInfoChanged                = null;
            OnDisconnectRequestFromMediaChannel = null;
            RoomId                              = string.Empty;
            IsRecording                         = false;
            if (User.Instance.CurrentUserData is OfficeUserData userData)
                userData.GuestName = string.Empty;
        }

        public static void SetMeetingTemplates(Components.MeetingTemplate[] meetingTemplates)
        {
            MeetingTemplates.Clear();
            foreach (var meetingTemplate in meetingTemplates)
            {
                MeetingTemplates.Add(meetingTemplate);
            }
        }

        public static bool IsGuest()
        {
            return IsGuest(User.Instance.CurrentUserData.ID);
        }

        public static bool IsGuest(long uid)
        {
            if (EnteredMeetingInfo == null)
                return false;
            foreach (var meetingUserInfo in EnteredMeetingInfo.MeetingMembers)
            {
                if (meetingUserInfo.AccountId == uid)
                {
                    return meetingUserInfo.MemberType == Components.MemberType.OutsideParticipant;
                }
            }

            return false;
        }

        public static bool IsOrganizer(long                         accountId)        => IsOrganizer(EnteredMeetingInfo?.MeetingMembers?.ToList(), accountId);
        public static bool IsOrganizer(IEnumerable<MeetingUserType> meetingUserInfos) => User.InstanceExists && IsOrganizer(meetingUserInfos, User.Instance.CurrentUserData.ID);

        public static bool IsOrganizer(IEnumerable<MeetingUserType> meetingUserInfos, long accountId)
        {
            if (!GetAuthorityCode(meetingUserInfos, accountId, out var authorityCode))
                return false;

            return authorityCode is AuthorityCode.Organizer;
        }

        public static string GetOrganizerName()
        {
            foreach (var member in EnteredMeetingInfo.MeetingMembers)
            {
                if (member.AuthorityCode == AuthorityCode.Organizer)
                {
                    return member.MemberName;
                }
            }

            return string.Empty;
        }

        public static string GetMemberName(long accountId)
        {
            foreach (var member in EnteredMeetingInfo.MeetingMembers)
            {
                if (member.AccountId == accountId)
                {
                    return member.MemberName;
                }
            }

            return string.Empty;
        }

        public static long GetOrganizerID()
        {
            foreach (var member in EnteredMeetingInfo.MeetingMembers)
            {
                if (member.AuthorityCode == AuthorityCode.Organizer)
                {
                    return member.AccountId;
                }
            }

            return 0;
        }

        public static void DisconnectRequestFromMediaChannel()
        {
            OnDisconnectRequestFromMediaChannel?.Invoke();
        }

        public static bool GetAuthorityCode(long accountId, out AuthorityCode authorityCode) => GetAuthorityCode(EnteredMeetingInfo?.MeetingMembers?.ToList(), accountId, out authorityCode);
        public static bool GetAuthorityCode(IEnumerable<MeetingUserType> meetingUserInfos, out AuthorityCode authorityCode) => GetAuthorityCode(meetingUserInfos, User.Instance.CurrentUserData.ID, out authorityCode);

        public static bool GetAuthorityCode(IEnumerable<MeetingUserType> meetingUserInfos, long accountId, out AuthorityCode authorityCode)
        {
            if (meetingUserInfos == null)
            {
                authorityCode = AuthorityCode.AuthorityCodeNone;
                return false;
            }

            foreach (var meetingUserInfo in meetingUserInfos)
            {
                if (meetingUserInfo?.AccountId == accountId)
                {
                    authorityCode = meetingUserInfo.AuthorityCode;
                    return true;
                }
            }

            authorityCode = AuthorityCode.AuthorityCodeNone;
            return false;
        }

        public static bool IsAvailableVoiceData()
        {
            foreach (var channel in ChannelManager.Instance.JoiningChannels.Values)
            {
                var selfTrackManager = (channel.Self as IChannelLocalUser).ChannelTrackManager;

                if (selfTrackManager.Tracks.ContainsKey(eTrackType.VOICE))
                    return true;

                foreach (var user in channel.ConnectedUsers.Values)
                {
                    var remoteTrackManager = (user as ISubscribableRemoteUser)?.SubscribeTrackManager;

                    if (remoteTrackManager != null && remoteTrackManager.Tracks.ContainsKey(eTrackType.VOICE))
                        return true;
                }
            }

            return false;
        }

        public static string GetOrganizerName(MeetingInfoType meetingInfo)
        {
            var memberId = GetMemberId(meetingInfo);

            return DataManager.Instance.GetMember(memberId)?.Member?.MemberName;
        }

        public static string GetMemberNameInOrganization(MemberIdType memberId)
        {
            var memberModel = DataManager.Instance.GetMember(memberId);
            return memberModel == null ? string.Empty : memberModel.Member?.MemberName;
        }

        public static async UniTask<MemberModel> GetOrganizerMemberModel(MemberIdType memberId)
        {
            var memberModel = await DataManager.Instance.GetMemberAsync(memberId);
            return memberModel;
        }

        private static MemberIdType GetMemberId(MeetingInfoType meetingInfo)
        {
            foreach (var meetingUserInfo in meetingInfo.MeetingMembers)
            {
                if (meetingUserInfo.AuthorityCode == Components.AuthorityCode.Organizer)
                    return meetingUserInfo.AccountId;
            }

            return default;
        }

        public static void OpenMeetingReservationPopup(MeetingInfoType meetingInfo, Action closeCallback, Action<MeetingInfoType> reservationResponse, Action<MeetingInfoType> reservationChangeResponse)
        {
            UIManager.Instance.CreatePopup("UI_ConnectingApp_Reservation", (guiView) =>
            {
                guiView.Show();

                var reservationViewModel = guiView.ViewModelContainer.GetViewModel<MeetingReservationViewModel>();
                reservationViewModel.Set(meetingInfo, closeCallback, reservationResponse, reservationChangeResponse, guiView).Forget();

            }).Forget();
        }

        public static void OpenMeetingReservationInquiryPopup(Action<MeetingInfoType> onClickDetailPage, Action inviteCallback)
        {
            UIManager.Instance.CreatePopup("UI_ConnectingApp_Reservation_Inquiry", (guiView) =>
            {
                guiView.Show();
                var meetingInquiryListViewModel = guiView.ViewModelContainer.GetViewModel<MeetingInquiryListViewModel>();
                meetingInquiryListViewModel.Set(onClickDetailPage, inviteCallback, guiView);
            }).Forget();
        }


        public static void OpenParticipantManagementPopup(MeetingInfoType meetingInfo)
        {
            UIManager.Instance.CreatePopup("UI_Repository_Popup_Management", (guiView) =>
            {
                guiView.Show();

                var meetingEmployeeListViewModel = guiView.ViewModelContainer.GetViewModel<MeetingEmployeeListViewModel>();
                meetingEmployeeListViewModel.Set(meetingInfo);
            }).Forget();
        }

        public static void OpenMeetingReservationCreditPopup(int numberOfConsumed, Action<MeetingReservationCreditViewModel.ePaymentType> closeCallback)
        {
            UIManager.Instance.CreatePopup("UI_Credit_Payment_Popup", (guiView) =>
            {
                guiView.Show();

                var reservationViewModel = guiView.ViewModelContainer.GetViewModel<MeetingReservationCreditViewModel>();
                reservationViewModel.SetCreditPopup(numberOfConsumed, closeCallback, guiView);

            }).Forget();
        }
    }
}
