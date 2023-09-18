/*===============================================================
* Product:		Com2Verse
* File Name:	ConvertExtensions.cs
* Developer:	jhkim
* Date:			2023-06-28 17:54
* History:		
* Documents:	
* Copyright ⓒ Com2Verse. All rights reserved.
 ================================================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using Com2Verse.WebApi.Service;
using Google.Protobuf.Collections;
using Protocols;
using Protocols.OfficeMeeting;
using MeetingInfoType = Com2Verse.WebApi.Service.Components.MeetingEntity;

// TODO : NEW_ORGANIZATION 임시 파일
namespace Com2Verse.Organization
{
	public static class ConvertExtensions
	{
		public static WebApi.Service.Components.AuthorityCode Convert(this AuthorityCode code) => code switch
		{
			AuthorityCode.None => Components.AuthorityCode.AuthorityCodeNone,
			AuthorityCode.Organizer => Components.AuthorityCode.Organizer,
			AuthorityCode.Presenter => Components.AuthorityCode.Presenter,
			AuthorityCode.Participant => Components.AuthorityCode.Participant,
			_ => Components.AuthorityCode.AuthorityCodeNone,
		};

		public static AuthorityCode Convert(this WebApi.Service.Components.AuthorityCode code) => code switch
		{
			Components.AuthorityCode.AuthorityCodeNone => AuthorityCode.None,
			Components.AuthorityCode.Organizer => AuthorityCode.Organizer,
			Components.AuthorityCode.Presenter => AuthorityCode.Presenter,
			Components.AuthorityCode.Participant => AuthorityCode.Participant,
			_ => AuthorityCode.None,
		};

		public static WebApi.Service.Components.AttendanceCode Convert(this AttendanceCode code) => code switch
		{
			AttendanceCode.None => Components.AttendanceCode.AttendanceCodeNone,
			AttendanceCode.Join => Components.AttendanceCode.Join,
			AttendanceCode.Viewer => Components.AttendanceCode.Viewer,
			AttendanceCode.JoinRequest => Components.AttendanceCode.JoinRequest,
			AttendanceCode.JoinReceive => Components.AttendanceCode.JoinReceive,
			_ => Components.AttendanceCode.AttendanceCodeNone,
		};

		public static AttendanceCode Convert(this WebApi.Service.Components.AttendanceCode code) => code switch
		{
			Components.AttendanceCode.AttendanceCodeNone => AttendanceCode.None,
			Components.AttendanceCode.Join => AttendanceCode.Join,
			Components.AttendanceCode.Viewer => AttendanceCode.Viewer,
			Components.AttendanceCode.JoinRequest => AttendanceCode.JoinRequest,
			Components.AttendanceCode.JoinReceive => AttendanceCode.JoinReceive,
			_ => AttendanceCode.None,
		};

		public static WebApi.Service.Components.MemberType Convert(this MemberType type) => type switch
		{
			MemberType.None => Components.MemberType.MemberTypeNone,
			MemberType.CompanyEmployee => Components.MemberType.CompanyEmployee,
			MemberType.OutsideParticipant => Components.MemberType.OutsideParticipant,
			_ => Components.MemberType.MemberTypeNone,
		};

		public static MemberType Convert(this WebApi.Service.Components.MemberType type) => type switch 
		{
			Components.MemberType.MemberTypeNone => MemberType.None,
			Components.MemberType.CompanyEmployee => MemberType.CompanyEmployee,
			Components.MemberType.OutsideParticipant => MemberType.OutsideParticipant,
			_ => MemberType.None,
		};

		public static WebApi.Service.Components.MeetingMemberEntity[] Convert(this RepeatedField<MeetingUserInfo> users)
		{
			var result = new Components.MeetingMemberEntity[users.Count];
			for (int i = 0; i < users.Count; ++i)
			{
				var user = users[i];
				var member = new Components.MeetingMemberEntity
				{
					AttendanceCode = user.AttendanceCode.Convert(),
					AuthorityCode = user.AuthorityCode.Convert(),
					AccountId = user.AccountId,
					MemberName = user.EmployeeName,
					IsEnter = user.IsEnter,
					MemberType = user.MemberType.Convert(),
				};

				result[i] = member;
			}
			return result;
		}

		public static WebApi.Service.Components.MeetingStatus Convert(this MeetingStatus status) => status switch
		{
			MeetingStatus.None => Components.MeetingStatus.MeetingStatusNone,
			MeetingStatus.MeetingBeforeStart => Components.MeetingStatus.MeetingBeforeStart,
			MeetingStatus.MeetingReadyTime => Components.MeetingStatus.MeetingReadyTime,
			MeetingStatus.MeetingOngoing => Components.MeetingStatus.MeetingOngoing,
			MeetingStatus.MeetingPassed => Components.MeetingStatus.MeetingPassed,
			MeetingStatus.MeetingExpired => Components.MeetingStatus.MeetingExpired,
			MeetingStatus.MeetingCancelAfterDelete => Components.MeetingStatus.MeetingCancelAfterDelete,
			_ => Components.MeetingStatus.MeetingStatusNone,
		};

		public static MeetingStatus Convert(this WebApi.Service.Components.MeetingStatus status) => status switch
		{
			Components.MeetingStatus.MeetingStatusNone => MeetingStatus.None,
			Components.MeetingStatus.MeetingBeforeStart => MeetingStatus.MeetingBeforeStart,
			Components.MeetingStatus.MeetingReadyTime => MeetingStatus.MeetingReadyTime,
			Components.MeetingStatus.MeetingOngoing => MeetingStatus.MeetingOngoing,
			Components.MeetingStatus.MeetingPassed => MeetingStatus.MeetingPassed,
			Components.MeetingStatus.MeetingExpired => MeetingStatus.MeetingExpired,
			Components.MeetingStatus.MeetingCancelAfterDelete => MeetingStatus.MeetingCancelAfterDelete,
			_ => MeetingStatus.None,
		};

		private static DateTime Convert(this ProtoDateTime protpDateTime) => new DateTime(protpDateTime.Year, protpDateTime.Month, protpDateTime.Day, protpDateTime.Hour, protpDateTime.Minute, protpDateTime.Second);

		public static WebApi.Service.Components.MeetingEntity Convert(this MeetingInfo info)
		{
			long.TryParse(info.CreateUserNo, out var accountId);

			var entity = new Components.MeetingEntity
			{
				MeetingId = info.MeetingId,
				ChannelId = info.ChannelId,
				FieldId = info.FieldId,
				CompanyCode = info.CompanyCode,
				MeetingName = info.MeetingName,
				MaxUsersLimit = info.MaxUsersLimit,
				MeetingDescription = info.MeetingDescription,
				MeetingMembers = info.MeetingUserInfo.Convert(),
				StartDateTime = info.StartDateTime.Convert(),
				EndDateTime = info.EndDateTime.Convert(),
				UpdateDateTime = info.EndDateTime.Convert(), // 임시로 종료 시간 세팅
				CancelYn = info.CancelYN,
				ChatNoteYn = info.ChatNoteYN,
				VoiceRecordYn = info.VoiceRecordYN,
				MeetingNoteYn = info.MeetNoteYN,
				MeetingStatus = info.MeetingStatus.Convert(),
				PublicYn = info.PublicYN,
			};
			return entity;
		}

		public static Protocols.OfficeMeeting.Direction ToProtocolType(this WebApi.Service.Components.Direction direction) => direction switch
		{
			Components.Direction.DirectionNone => Direction.None,
			Components.Direction.Bilateral => Direction.Bilateral,
			Components.Direction.Incoming => Direction.Incomming,
			Components.Direction.Outgoing => Direction.Outgoing,
			_ => Direction.None,
		};

		public static Protocols.OfficeMeeting.ChannelType ToProtocolType(this WebApi.Service.Components.ChannelType channelType) => channelType switch
		{
			Components.ChannelType.ChannelTypeNone => ChannelType.None,
			Components.ChannelType.Meeting => ChannelType.Meeting,
			Components.ChannelType.SmallTalk => ChannelType.SmallTalk,
			Components.ChannelType.P2PCall => ChannelType.P2PCall,
			Components.ChannelType.TeamworkCall => ChannelType.TeamworkCall,
			Components.ChannelType.PartyTalk => ChannelType.PartyTalk,
			_ => ChannelType.None,
		};
	}
}
