using System;
using System.Collections.Generic;
using Newtonsoft.Json;

#nullable enable
namespace Com2Verse.Mice
{
    public static partial class MiceWebClient
    {
		public enum HttpStatusCode
		{
			Continue = 100,
			SwitchingProtocols = 101,
			Processing = 102,
			EarlyHints = 103,
			OK = 200,
			Created = 201,
			Accepted = 202,
			NonAuthoritativeInformation = 203,
			NoContent = 204,
			ResetContent = 205,
			PartialContent = 206,
			MultiStatus = 207,
			AlreadyReported = 208,
			IMUsed = 226,
			Ambiguous = 300,
			Moved = 301,
			Redirect = 302,
			RedirectMethod = 303,
			NotModified = 304,
			UseProxy = 305,
			Unused = 306,
			RedirectKeepVerb = 307,
			PermanentRedirect = 308,
			BadRequest = 400,
			Unauthorized = 401,
			PaymentRequired = 402,
			Forbidden = 403,
			NotFound = 404,
			MethodNotAllowed = 405,
			NotAcceptable = 406,
			ProxyAuthenticationRequired = 407,
			RequestTimeout = 408,
			Conflict = 409,
			Gone = 410,
			LengthRequired = 411,
			PreconditionFailed = 412,
			RequestEntityTooLarge = 413,
			RequestUriTooLong = 414,
			UnsupportedMediaType = 415,
			RequestedRangeNotSatisfiable = 416,
			ExpectationFailed = 417,
			MisdirectedRequest = 421,
			UnprocessableEntity = 422,
			Locked = 423,
			FailedDependency = 424,
			UpgradeRequired = 426,
			PreconditionRequired = 428,
			TooManyRequests = 429,
			RequestHeaderFieldsTooLarge = 431,
			UnavailableForLegalReasons = 451,
			InternalServerError = 500,
			NotImplemented = 501,
			BadGateway = 502,
			ServiceUnavailable = 503,
			GatewayTimeout = 504,
			HttpVersionNotSupported = 505,
			VariantAlsoNegotiates = 506,
			InsufficientStorage = 507,
			LoopDetected = 508,
			NotExtended = 510,
			NetworkAuthenticationRequired = 511
		}

		public enum MiceType
		{
			Lobby = 0,
			EventLounge = 1,
			Conference = 2,
			ConferenceSession = 3,
			Exhibition = 4,
			EventFreeLounge = 5
		}

		public enum eMiceAccountCardExchangeCode
		{
			NONE = 0,
			MYSELF = 1,
			UNKNOWN = 2,
			FOLLOW = 3,
			MUTUAL_FOLLOW = 4
		}

		public enum eMiceAuthorityCode
		{
			NORMAL = 1,
			SPEAKER = 2,
			STAFF = 3,
			OPERATOR = 4
		}

		public enum eMiceBannerCode
		{
			NONE = 0,
			ALL = 1,
			MOBILE = 2,
			PC = 3
		}

		public enum eMiceEventDisplayStateType
		{
			OPEN = 1,
			CLOSED = 2
		}

		public enum eMiceEventSellStateCode
		{
			OPEN = 1,
			CLOSED = 2
		}

		public enum eMiceEventSpaceStateType
		{
			NOT_YET = 1,
			SPACE_CREATE_READY = 2,
			REMOVE = 3
		}

		public enum eMiceEventStateType
		{
			OPENED = 1,
			IN_PROGRESS = 2,
			END = 3,
			DELETE = 4,
			TEMP = 5
		}

		public enum eMiceHttpErrorCode
		{
			OK = 0,
			NO_LOUNGE = 1,
			NOT_YET_OPENED = 2,
			NO_SESSION = 3,
			NEED_PERSONAL_INFO = 4,
			NEED_TICKET = 5,
			ALREADY_HAS_TICKET = 6,
			NEED_COUPON = 7,
			UNKNOWN_COUPON = 8,
			NO_COUPON = 9,
			ALREADY_USED_COUPON = 10,
			DELETED_COUPON = 11,
			NO_ARTICLE = 12,
			NOT_MY_ARTICLE = 13,
			EXCESS_LIMIT = 14,
			INVALID_ACCOUNT_ID = 15,
			NAME_REQUIRED = 16,
			NOT_YOUR_INFO = 17,
			NOT_PUBLIC_ACCOUNT = 18,
			CANNOT_EXCHANGE_CARD = 19,
			ALREADY_EXCHANGE_CARD = 20,
			CANNOT_FIND_ACCOUNT_CARD = 21,
			CANNOT_GET_UPLOAD_URL = 22,
			CANNOT_FIND_ARTICLE = 23,
			CANNOT_UPLOAD_FILE = 24,
			PRIZE_EVENT_CANNOT_FIND = 25,
			PRIZE_NEED_TICKET = 26,
			PRIZE_NOT_IN_TIME = 27,
			PRIZE_PERSONAL_INFO_NEED = 28,
			PRIZE_TRY_COUNT_NONE = 29,
			PRIZE_TRY_NONE = 30,
			PRIZE_RECEIVE_STATUS_INVALID = 31,
			PRIZE_PERSONAL_NEED_NAME = 32,
			PRIZE_PERSONAL_NEED_PHONE = 33,
			PRIZE_PERSONAL_NEED_EMAIL = 34,
			PRIZE_PERSONAL_NEED_ADDRESS = 35,
			NO_EVENT = 36,
			TICKET_USER_GO_TO_NORMAL = 37,
			NO_STREAMING_URL = 38,
			NO_GROUP_INFO = 39,
			HAVE_NO_TICKET = 40,
			FAIL_UPDATE_QUESTION = 41,
			UNKNOWN = 42,
			NOT_YET_SESSION_OPENED = 43
		}

		public enum eMiceLangCode
		{
			KO = 0,
			EN = 1
		}

		public enum eMiceLoungeType
		{
			LOUNGE_NONE = 0,
			LOUNGE_NORMAL = 1,
			LOUNGE_FREE = 2
		}

		public enum eMiceMappingType
		{
			EVENT = 1,
			PROGRAM = 2,
			SESSION = 3,
			LOUNGE = 4
		}

		public enum eMicePackageSellStateType
		{
			NONE = 0,
			SALE = 1,
			STOP_SELLING = 2,
			ALL = 3
		}

		public enum eMicePackageType
		{
			SELECT = 1,
			ENTRANCE_ALL_IN_EVENT = 2,
			ENTRANCE_ALL_IN_PROGRAM = 3,
			ENTRANCE_TO_SESSION = 4
		}

		public enum eMicePrizePrivacyAgreeTypeCode
		{
			NONE = 0,
			PRIVACY_TYPE_1 = 1,
			PRIVACY_TYPE_2 = 2,
			PRIVACY_TYPE_3 = 3,
			PRIVACY_TYPE_4 = 4
		}

		public enum eMicePrizeReceiveTypeCode
		{
			RECEIVE_NONE = 0,
			RECEIVE_PHONE = 1,
			RECEIVE_EMAIL = 2,
			RECEIVE_ADDRESS = 4,
			RECEIVE_NAME = 8
		}

		public enum eMiceProgramType
		{
			FAIR = 1,
			EXHIBITION = 2,
			CONFERENCE = 3,
			SPECIAL_MEED_UP = 4,
			OTHER = 5
		}

		public enum eMiceSessionEndType
		{
			FORCED_EXIT = 1,
			CONTINUE = 2
		}

		public enum eMiceSessionStateCode
		{
			READY = 1,
			PROGRESS = 2,
			REST = 3,
			QNA = 4,
			ENTER = 5,
			FINISH = 6
		}

		public enum eMiceSessionType
		{
			SELECT = 1,
			LIVE_STREAMING = 2,
			VOD_STREAMING = 3,
			VOD_PLAY = 4,
			IMAGE_COURTESY = 5,
			VOICE_COURTESY = 7
		}

		public enum eMiceStaffType
		{
			SPEAKER = 1,
			MODERATOR = 2
		}

		public enum eMiceSurveyTypeCode
		{
			SURVEY_NONE = 0,
			SURVEY_NORMAL = 1,
			SURVEY_SATISFY = 2
		}

		public enum eMiceTicketEndType
		{
			SPECIFY_TIME = 1,
			END_EVENT = 2,
			FIRST_COME_FIRST_SERVED = 3
		}

		public enum ePlatformHttpResultCode
		{
			Success = 200,
			Fail = 500
		}

		public static partial class Entities
		{
			public partial class AccountDetailEntity
			{
				[JsonProperty("detailSeq")] public int? DetailSeq { get; set; }
				[JsonProperty("accountId")] public long AccountId { get; set; }
				[JsonProperty("detailName")] public string? DetailName { get; set; }
				[JsonProperty("detailValue")] public string? DetailValue { get; set; }
			}

			public partial class AccountEntity
			{
				[JsonProperty("accountId")] public long AccountId { get; set; }
				[JsonProperty("nickname")] public string? Nickname { get; set; }
				[JsonProperty("domainId")] public int DomainId { get; set; }
				[JsonProperty("surname")] public string? Surname { get; set; }
				[JsonProperty("givenName")] public string? GivenName { get; set; }
				[JsonProperty("middleName")] public string? MiddleName { get; set; }
				[JsonProperty("photoPath")] public string? PhotoPath { get; set; }
				[JsonProperty("photoUrl")] public string? PhotoUrl { get; set; }
				[JsonProperty("photoThumbnailUrl")] public string? PhotoThumbnailUrl { get; set; }
				[JsonProperty("telNo")] public string? TelNo { get; set; }
				[JsonProperty("companyName")] public string? CompanyName { get; set; }
				[JsonProperty("mailAddress")] public string? MailAddress { get; set; }
				[JsonProperty("classCode")] public int ClassCode { get; set; }
				[JsonProperty("isPublic")] public bool IsPublic { get; set; }
				[JsonProperty("exchangeCode")] public eMiceAccountCardExchangeCode ExchangeCode { get; set; }
				[JsonProperty("details")] public IEnumerable<AccountDetailEntity>? Details { get; set; }
				[JsonProperty("surveyAgrees")] public IEnumerable<long>? SurveyAgrees { get; set; }
			}

			public partial class AccountInfo
			{
				[JsonProperty("accountId")] public long AccountId { get; set; }
				[JsonProperty("surname")] public string? Surname { get; set; }
				[JsonProperty("givenName")] public string? GivenName { get; set; }
				[JsonProperty("middleName")] public string? MiddleName { get; set; }
				[JsonProperty("telNo")] public string? TelNo { get; set; }
				[JsonProperty("companyName")] public string? CompanyName { get; set; }
				[JsonProperty("mailAddress")] public string? MailAddress { get; set; }
				[JsonProperty("isPublic")] public bool IsPublic { get; set; }
				[JsonProperty("details")] public IEnumerable<AccountDetailEntity>? Details { get; set; }
			}

			public partial class AddGroupAreaRequest
			{
				[JsonProperty("groupName")] public string? GroupName { get; set; }
				[JsonProperty("serviceType")] public string? ServiceType { get; set; }
				[JsonProperty("areaName")] public string? AreaName { get; set; }
			}

			public partial class AnalyzeFileResult
			{
				[JsonProperty("Md5")] public string? Md5 { get; set; }
				[JsonProperty("Size")] public long Size { get; set; }
				[JsonProperty("FileName")] public string? FileName { get; set; }
			}

			public partial class BannerDisplayEntity
			{
				[JsonProperty("displaySeq")] public long DisplaySeq { get; set; }
				[JsonProperty("bannerId")] public long BannerId { get; set; }
				[JsonProperty("pageNo")] public int PageNo { get; set; }
				[JsonProperty("displayContents")] public string? DisplayContents { get; set; }
				[JsonProperty("linkAddress")] public string? LinkAddress { get; set; }
				[JsonProperty("displayStartDateTime")] public DateTime DisplayStartDateTime { get; set; }
				[JsonProperty("displayEndDateTime")] public DateTime DisplayEndDateTime { get; set; }
				[JsonProperty("isAlwaysDisplay")] public bool IsAlwaysDisplay { get; set; }
			}

			public partial class BannerEntity
			{
				[JsonProperty("bannerId")] public long BannerId { get; set; }
				[JsonProperty("bannerCode")] public int BannerCode { get; set; }
				[JsonProperty("location")] public string? Location { get; set; }
				[JsonProperty("locationDescription")] public string? LocationDescription { get; set; }
				[JsonProperty("sizeX")] public double SizeX { get; set; }
				[JsonProperty("sizeY")] public double SizeY { get; set; }
				[JsonProperty("bannerDisplays")] public IEnumerable<BannerDisplayEntity>? BannerDisplays { get; set; }
			}

			public partial class CheckResult
			{
				[JsonProperty("result")] public bool Result { get; set; }
				[JsonProperty("reason")] public string? Reason { get; set; }
				[JsonProperty("miceStatusCode")] public eMiceHttpErrorCode MiceStatusCode { get; set; }
			}

			public partial class CollectorModel
			{
				[JsonProperty("activeSec")] public long ActiveSec { get; set; }
				[JsonProperty("lastEnterDateTime")] public DateTime LastEnterDateTime { get; set; }
				[JsonProperty("enterCount")] public int EnterCount { get; set; }
			}

			public partial class CommunicationUser
			{
				[JsonProperty("userId")] public string? UserId { get; set; }
				[JsonProperty("userRole")] public int UserRole { get; set; }
				[JsonProperty("pushType")] public int PushType { get; set; }
				[JsonProperty("hivePlayerId")] public int HivePlayerId { get; set; }
			}

			public partial class DeleteGroupResponse
			{
				[JsonProperty("groupId")] public string? GroupId { get; set; }
			}

			public partial class DeleteGroupResponseResponseFormat
			{
				[JsonProperty("code")] public ePlatformHttpResultCode Code { get; set; }
				[JsonProperty("msg")] public string? Msg { get; set; }
				[JsonProperty("data")] public DeleteGroupResponse? Data { get; set; }
			}

			public partial class EventCoverImage
			{
				[JsonProperty("imageHorizonUrl")] public string? ImageHorizonUrl { get; set; }
				[JsonProperty("imageVerticalUrl")] public string? ImageVerticalUrl { get; set; }
			}

			public partial class EventDetailPage
			{
				[JsonProperty("detailPage")] public string? DetailPage { get; set; }
			}

			public partial class EventEntity
			{
				[JsonProperty("eventId")] public long EventId { get; set; }
				[JsonProperty("eventName")] public string? EventName { get; set; }
				[JsonProperty("eventDescription")] public string? EventDescription { get; set; }
				[JsonProperty("offlineLocation")] public string? OfflineLocation { get; set; }
				[JsonProperty("offlineDetailLocation")] public string? OfflineDetailLocation { get; set; }
				[JsonProperty("stateCode")] public eMiceEventStateType StateCode { get; set; }
				[JsonProperty("spaceStateCode")] public eMiceEventSpaceStateType SpaceStateCode { get; set; }
				[JsonProperty("displayStateCode")] public eMiceEventDisplayStateType DisplayStateCode { get; set; }
				[JsonProperty("sellStateCode")] public eMiceEventSellStateCode SellStateCode { get; set; }
				[JsonProperty("startDatetime")] public DateTime StartDatetime { get; set; }
				[JsonProperty("endDatetime")] public DateTime EndDatetime { get; set; }
				[JsonProperty("displayStartDateTime")] public DateTime DisplayStartDateTime { get; set; }
				[JsonProperty("displayEndDateTime")] public DateTime DisplayEndDateTime { get; set; }
				[JsonProperty("sellStartDatetime")] public DateTime SellStartDatetime { get; set; }
				[JsonProperty("sellEndDatetime")] public DateTime SellEndDatetime { get; set; }
				[JsonProperty("loungeEnterDatetime")] public DateTime LoungeEnterDatetime { get; set; }
				[JsonProperty("loungeStartDateTime")] public DateTime LoungeStartDateTime { get; set; }
				[JsonProperty("loungeEndDateTime")] public DateTime LoungeEndDateTime { get; set; }
				[JsonProperty("eventCoverImage")] public EventCoverImage? EventCoverImage { get; set; }
				[JsonProperty("eventDetailPage")] public EventDetailPage? EventDetailPage { get; set; }
				[JsonProperty("eventPopUpBanner")] public EventPopUpBanner? EventPopUpBanner { get; set; }
				[JsonProperty("miceTicketEndType")] public eMiceTicketEndType MiceTicketEndType { get; set; }
				[JsonProperty("updateDatetime")] public DateTime UpdateDatetime { get; set; }
				[JsonProperty("surveys")] public IEnumerable<SurveyEventEntity>? Surveys { get; set; }
				[JsonProperty("lounges")] public IEnumerable<LoungeEntity>? Lounges { get; set; }
				[JsonProperty("programs")] public IEnumerable<ProgramEntity>? Programs { get; set; }
			}

			public partial class EventPopUpBanner
			{
				[JsonProperty("imageUrl")] public string? ImageUrl { get; set; }
				[JsonProperty("linkUrl")] public string? LinkUrl { get; set; }
			}

			public partial class FileDownloadResponse
			{
				[JsonProperty("url")] public string? Url { get; set; }
			}

			public partial class FileDownloadResponseResponseFormat
			{
				[JsonProperty("code")] public ePlatformHttpResultCode Code { get; set; }
				[JsonProperty("msg")] public string? Msg { get; set; }
				[JsonProperty("data")] public FileDownloadResponse? Data { get; set; }
			}

			public partial class GetUserListByGroupResponse
			{
				[JsonProperty("groupCount")] public int GroupCount { get; set; }
				[JsonProperty("users")] public IEnumerable<SimpleCommunicationUser>? Users { get; set; }
			}

			public partial class GetUserListByGroupResponseResponseFormat
			{
				[JsonProperty("code")] public ePlatformHttpResultCode Code { get; set; }
				[JsonProperty("msg")] public string? Msg { get; set; }
				[JsonProperty("data")] public GetUserListByGroupResponse? Data { get; set; }
			}

			public partial class GroupInfo
			{
				[JsonProperty("groupId")] public string? GroupId { get; set; }
				[JsonProperty("groupName")] public string? GroupName { get; set; }
				[JsonProperty("areaName")] public string? AreaName { get; set; }
			}

			public partial class GroupInfoResponseData
			{
				[JsonProperty("groupId")] public string? GroupId { get; set; }
				[JsonProperty("groupName")] public string? GroupName { get; set; }
				[JsonProperty("groupType")] public int GroupType { get; set; }
				[JsonProperty("createUserId")] public string? CreateUserId { get; set; }
				[JsonProperty("createDatetime")] public long CreateDatetime { get; set; }
				[JsonProperty("updateDatetime")] public long UpdateDatetime { get; set; }
				[JsonProperty("serviceType")] public int ServiceType { get; set; }
				[JsonProperty("areaName")] public string? AreaName { get; set; }
				[JsonProperty("groupCount")] public int GroupCount { get; set; }
				[JsonProperty("users")] public IEnumerable<CommunicationUser>? Users { get; set; }
				[JsonProperty("groupActions")] public IEnumerable<int>? GroupActions { get; set; }
			}

			public partial class GroupInfoResponseDataResponseFormat
			{
				[JsonProperty("code")] public ePlatformHttpResultCode Code { get; set; }
				[JsonProperty("msg")] public string? Msg { get; set; }
				[JsonProperty("data")] public GroupInfoResponseData? Data { get; set; }
			}

			public partial class LoungeEntity
			{
				[JsonProperty("loungeNo")] public long LoungeNo { get; set; }
				[JsonProperty("eventId")] public long EventId { get; set; }
				[JsonProperty("loungeName")] public string? LoungeName { get; set; }
				[JsonProperty("spaceId")] public string? SpaceId { get; set; }
				[JsonProperty("maxUserCount")] public int MaxUserCount { get; set; }
				[JsonProperty("templateId")] public long TemplateId { get; set; }
				[JsonProperty("loungeType")] public eMiceLoungeType LoungeType { get; set; }
			}

			public partial class MainNoticeResult
			{
				[JsonProperty("boardSeq")] public int BoardSeq { get; set; }
				[JsonProperty("articleTitle")] public string? ArticleTitle { get; set; }
			}

			public partial class MoveUserAreaRequest
			{
				[JsonProperty("groupId")] public string? GroupId { get; set; }
				[JsonProperty("userId")] public string? UserId { get; set; }
				[JsonProperty("userName")] public string? UserName { get; set; }
			}

			public partial class NoticeBoardEntity
			{
				[JsonProperty("boardSeq")] public int BoardSeq { get; set; }
				[JsonProperty("articleType")] public int ArticleType { get; set; }
				[JsonProperty("articleTitle")] public string? ArticleTitle { get; set; }
				[JsonProperty("articleDescription")] public string? ArticleDescription { get; set; }
				[JsonProperty("isMain")] public bool IsMain { get; set; }
				[JsonProperty("updateDatetime")] public DateTime UpdateDatetime { get; set; }
				[JsonProperty("createDatetime")] public DateTime CreateDatetime { get; set; }
			}

			public partial class NoticeInfo
			{
				[JsonProperty("totalCount")] public int TotalCount { get; set; }
				[JsonProperty("notices")] public IEnumerable<NoticeBoardEntity>? Notices { get; set; }
			}

			public partial class PackageEntity
			{
				[JsonProperty("packageId")] public string? PackageId { get; set; }
				[JsonProperty("eventId")] public long EventId { get; set; }
				[JsonProperty("packageName")] public string? PackageName { get; set; }
				[JsonProperty("packageType")] public eMicePackageType PackageType { get; set; }
				[JsonProperty("ticketInfoList")] public IEnumerable<TicketInfoEntity>? TicketInfoList { get; set; }
				[JsonProperty("maxPackageCount")] public int MaxPackageCount { get; set; }
				[JsonProperty("price")] public long Price { get; set; }
				[JsonProperty("authorityCode")] public eMiceAuthorityCode AuthorityCode { get; set; }
				[JsonProperty("stateCode")] public eMicePackageSellStateType StateCode { get; set; }
				[JsonProperty("paymentPath")] public string? PaymentPath { get; set; }
			}

			public partial class Participant
			{
				[JsonProperty("accountId")] public long AccountId { get; set; }
				[JsonProperty("nickname")] public string? Nickname { get; set; }
				[JsonProperty("photoPath")] public string? PhotoPath { get; set; }
				[JsonProperty("photoThumbnailUrl")] public string? PhotoThumbnailUrl { get; set; }
				[JsonProperty("companyName")] public string? CompanyName { get; set; }
				[JsonProperty("isPublic")] public bool IsPublic { get; set; }
			}

			public partial class PrizeInfoEntity
			{
				[JsonProperty("prizeId")] public long PrizeId { get; set; }
				[JsonProperty("tryCount")] public int TryCount { get; set; }
				[JsonProperty("myTryCount")] public int MyTryCount { get; set; }
				[JsonProperty("personalInfoNeeded")] public bool PersonalInfoNeeded { get; set; }
				[JsonProperty("winPrizeItemId")] public long? WinPrizeItemId { get; set; }
				[JsonProperty("winPrizeItemSeq")] public int? WinPrizeItemSeq { get; set; }
				[JsonProperty("winReceiveType")] public eMicePrizeReceiveTypeCode WinReceiveType { get; set; }
				[JsonProperty("winPrivacyAgreeType")] public eMicePrizePrivacyAgreeTypeCode WinPrivacyAgreeType { get; set; }
				[JsonProperty("startDateTime")] public DateTime StartDateTime { get; set; }
				[JsonProperty("endDateTime")] public DateTime EndDateTime { get; set; }
			}

			public partial class PrizeItem
			{
				[JsonProperty("prizeItemId")] public long PrizeItemId { get; set; }
				[JsonProperty("prizeId")] public long PrizeId { get; set; }
				[JsonProperty("itemName")] public string? ItemName { get; set; }
				[JsonProperty("itemPhoto")] public string? ItemPhoto { get; set; }
				[JsonProperty("itemQuantity")] public int ItemQuantity { get; set; }
				[JsonProperty("receiveType")] public eMicePrizeReceiveTypeCode ReceiveType { get; set; }
				[JsonProperty("privacyAgreeType")] public eMicePrizePrivacyAgreeTypeCode PrivacyAgreeType { get; set; }
				[JsonProperty("prizeItemIdSeq")] public int? PrizeItemIdSeq { get; set; }
			}

			public partial class PrizePersonalInfo
			{
				[JsonProperty("prizeId")] public long PrizeId { get; set; }
				[JsonProperty("prizeItemId")] public long PrizeItemId { get; set; }
				[JsonProperty("prizeItemSeq")] public int PrizeItemSeq { get; set; }
				[JsonProperty("winnerName")] public string? WinnerName { get; set; }
				[JsonProperty("phoneNum")] public string? PhoneNum { get; set; }
				[JsonProperty("email")] public string? Email { get; set; }
				[JsonProperty("address")] public string? Address { get; set; }
			}

			public partial class ProblemDetails
			{
				[JsonProperty("type")] public string? Type { get; set; }
				[JsonProperty("title")] public string? Title { get; set; }
				[JsonProperty("status")] public int? Status { get; set; }
				[JsonProperty("detail")] public string? Detail { get; set; }
				[JsonProperty("instance")] public string? Instance { get; set; }
			}

			public partial class ProgramCoverImage
			{
				[JsonProperty("imageHorizonUrl")] public string? ImageHorizonUrl { get; set; }
				[JsonProperty("imageVerticalUrl")] public string? ImageVerticalUrl { get; set; }
			}

			public partial class ProgramEntity
			{
				[JsonProperty("programId")] public long ProgramId { get; set; }
				[JsonProperty("eventId")] public long EventId { get; set; }
				[JsonProperty("programName")] public string? ProgramName { get; set; }
				[JsonProperty("isSessionAuto")] public bool IsSessionAuto { get; set; }
				[JsonProperty("description")] public string? Description { get; set; }
				[JsonProperty("updateDatetime")] public DateTime UpdateDatetime { get; set; }
				[JsonProperty("surveys")] public IEnumerable<SurveyProgramEntity>? Surveys { get; set; }
				[JsonProperty("miceProgramType")] public eMiceProgramType MiceProgramType { get; set; }
				[JsonProperty("programCoverImage")] public ProgramCoverImage? ProgramCoverImage { get; set; }
				[JsonProperty("sessions")] public IEnumerable<SessionEntity>? Sessions { get; set; }
			}

			public partial class ProgramSurvey
			{
				[JsonProperty("programId")] public long ProgramId { get; set; }
				[JsonProperty("programName")] public string? ProgramName { get; set; }
				[JsonProperty("programSurveys")] public IEnumerable<SurveyProgramEntity>? ProgramSurveys { get; set; }
			}

			public partial class QuestionCreateRequest
			{
				[JsonProperty("sessionId")] public long SessionId { get; set; }
				[JsonProperty("questionTitle")] public string? QuestionTitle { get; set; }
				[JsonProperty("questionDescription")] public string? QuestionDescription { get; set; }
			}

			public partial class QuestionResult
			{
				[JsonProperty("questionSeq")] public int QuestionSeq { get; set; }
				[JsonProperty("sessionId")] public long SessionId { get; set; }
				[JsonProperty("questionTitle")] public string? QuestionTitle { get; set; }
				[JsonProperty("questionDescription")] public string? QuestionDescription { get; set; }
				[JsonProperty("viewCount")] public int ViewCount { get; set; }
				[JsonProperty("likeCount")] public int LikeCount { get; set; }
				[JsonProperty("createDateTime")] public DateTime CreateDateTime { get; set; }
				[JsonProperty("accountId")] public long AccountId { get; set; }
				[JsonProperty("nickName")] public string? NickName { get; set; }
				[JsonProperty("companyName")] public string? CompanyName { get; set; }
				[JsonProperty("photoPath")] public string? PhotoPath { get; set; }
				[JsonProperty("photoThumbnailUrl")] public string? PhotoThumbnailUrl { get; set; }
				[JsonProperty("isMine")] public bool IsMine { get; set; }
				[JsonProperty("isLikeClicked")] public bool IsLikeClicked { get; set; }
			}

			public partial class RequestPrizeItemSeq
			{
				[JsonProperty("itemId")] public long ItemId { get; set; }
				[JsonProperty("count")] public int Count { get; set; }
				[JsonProperty("startSeq")] public int StartSeq { get; set; }
			}

			public partial class SessionAttachmentFile
			{
				[JsonProperty("fileName")] public string? FileName { get; set; }
				[JsonProperty("fileUrl")] public string? FileUrl { get; set; }
			}

			public partial class SessionCoverImage
			{
				[JsonProperty("imageHorizonUrl")] public string? ImageHorizonUrl { get; set; }
				[JsonProperty("imageVerticalUrl")] public string? ImageVerticalUrl { get; set; }
			}

			public partial class SessionEntity
			{
				[JsonProperty("sessionId")] public long SessionId { get; set; }
				[JsonProperty("programId")] public long ProgramId { get; set; }
				[JsonProperty("eventId")] public long EventId { get; set; }
				[JsonProperty("sessionName")] public string? SessionName { get; set; }
				[JsonProperty("miceSessionType")] public eMiceSessionType MiceSessionType { get; set; }
				[JsonProperty("parentSessionId")] public long ParentSessionId { get; set; }
				[JsonProperty("sessionDescription")] public string? SessionDescription { get; set; }
				[JsonProperty("hallName")] public string? HallName { get; set; }
				[JsonProperty("hallStartDatetime")] public DateTime HallStartDatetime { get; set; }
				[JsonProperty("hallEndDateTime")] public DateTime HallEndDateTime { get; set; }
				[JsonProperty("startDatetime")] public DateTime StartDatetime { get; set; }
				[JsonProperty("endDatetime")] public DateTime EndDatetime { get; set; }
				[JsonProperty("miceSessionEndType")] public eMiceSessionEndType MiceSessionEndType { get; set; }
				[JsonProperty("maxMemberCount")] public int MaxMemberCount { get; set; }
				[JsonProperty("spaceId")] public string? SpaceId { get; set; }
				[JsonProperty("templateId")] public long TemplateId { get; set; }
				[JsonProperty("stateCode")] public eMiceSessionStateCode StateCode { get; set; }
				[JsonProperty("isQuestion")] public bool IsQuestion { get; set; }
				[JsonProperty("useMobileChat")] public bool UseMobileChat { get; set; }
				[JsonProperty("questionCount")] public int QuestionCount { get; set; }
				[JsonProperty("eventPopUpBanner")] public EventPopUpBanner? EventPopUpBanner { get; set; }
				[JsonProperty("sessionCoverImage")] public SessionCoverImage? SessionCoverImage { get; set; }
				[JsonProperty("sessionAttachmentFiles")] public IEnumerable<SessionAttachmentFile>? SessionAttachmentFiles { get; set; }
				[JsonProperty("updateDatetime")] public DateTime UpdateDatetime { get; set; }
				[JsonProperty("sessionStaffs")] public IEnumerable<SessionStaffEntity>? SessionStaffs { get; set; }
			}

			public partial class SessionQuestionEntity
			{
				[JsonProperty("questionSeq")] public int QuestionSeq { get; set; }
				[JsonProperty("sessionId")] public long SessionId { get; set; }
				[JsonProperty("accountId")] public long AccountId { get; set; }
				[JsonProperty("questionTitle")] public string? QuestionTitle { get; set; }
				[JsonProperty("questionDescription")] public string? QuestionDescription { get; set; }
				[JsonProperty("answer")] public string? Answer { get; set; }
				[JsonProperty("updateDateTime")] public DateTime UpdateDateTime { get; set; }
				[JsonProperty("createDateTime")] public DateTime CreateDateTime { get; set; }
			}

			public partial class SessionStaffEntity
			{
				[JsonProperty("staffId")] public long StaffId { get; set; }
				[JsonProperty("sessionId")] public long SessionId { get; set; }
				[JsonProperty("staffName")] public string? StaffName { get; set; }
				[JsonProperty("domainName")] public string? DomainName { get; set; }
				[JsonProperty("staffDescription")] public string? StaffDescription { get; set; }
				[JsonProperty("staffType")] public eMiceStaffType StaffType { get; set; }
				[JsonProperty("photoUrl")] public string? PhotoUrl { get; set; }
			}

			public partial class SimpleCommunicationUser
			{
				[JsonProperty("userId")] public string? UserId { get; set; }
				[JsonProperty("userRole")] public int UserRole { get; set; }
				[JsonProperty("groupId")] public string? GroupId { get; set; }
			}

			public partial class StreamingStatusEntity
			{
				[JsonProperty("IsPlay")] public int IsPlay { get; set; }
				[JsonProperty("Url")] public string? Url { get; set; }
				[JsonProperty("SourceType")] public int SourceType { get; set; }
			}

			public partial class SurveyEventEntity
			{
				[JsonProperty("surveyNo")] public long SurveyNo { get; set; }
				[JsonProperty("eventId")] public long EventId { get; set; }
				[JsonProperty("surveyPath")] public string? SurveyPath { get; set; }
			}

			public partial class SurveyInfo
			{
				[JsonProperty("eventName")] public string? EventName { get; set; }
				[JsonProperty("eventSurveys")] public IEnumerable<SurveyEventEntity>? EventSurveys { get; set; }
				[JsonProperty("programSurveys")] public IEnumerable<ProgramSurvey>? ProgramSurveys { get; set; }
			}

			public partial class SurveyProgramEntity
			{
				[JsonProperty("surveyNo")] public long SurveyNo { get; set; }
				[JsonProperty("programId")] public long ProgramId { get; set; }
				[JsonProperty("surveyCode")] public eMiceSurveyTypeCode SurveyCode { get; set; }
				[JsonProperty("surveyPath")] public string? SurveyPath { get; set; }
			}

			public partial class TicketEntity
			{
				[JsonProperty("paymentNo")] public int PaymentNo { get; set; }
				[JsonProperty("accountId")] public long AccountId { get; set; }
				[JsonProperty("sessionId")] public long SessionId { get; set; }
				[JsonProperty("packageId")] public string? PackageId { get; set; }
				[JsonProperty("isBlock")] public bool IsBlock { get; set; }
				[JsonProperty("paymentType")] public int PaymentType { get; set; }
				[JsonProperty("price")] public long Price { get; set; }
				[JsonProperty("authorityCode")] public eMiceAuthorityCode AuthorityCode { get; set; }
				[JsonProperty("couponNo")] public string? CouponNo { get; set; }
				[JsonProperty("createDatetime")] public DateTime CreateDatetime { get; set; }
				[JsonProperty("updateDatetime")] public DateTime UpdateDatetime { get; set; }
			}

			public partial class TicketInfoDetailEntity
			{
				[JsonProperty("ticketId")] public string? TicketId { get; set; }
				[JsonProperty("eventId")] public long EventId { get; set; }
				[JsonProperty("programId")] public long ProgramId { get; set; }
				[JsonProperty("sessionId")] public long SessionId { get; set; }
				[JsonProperty("ticketName")] public string? TicketName { get; set; }
				[JsonProperty("onlineCode")] public int OnlineCode { get; set; }
				[JsonProperty("createUserId")] public long CreateUserId { get; set; }
				[JsonProperty("updateUserId")] public long UpdateUserId { get; set; }
				[JsonProperty("createDatetime")] public DateTime CreateDatetime { get; set; }
				[JsonProperty("updateDatetime")] public DateTime UpdateDatetime { get; set; }
				[JsonProperty("programName")] public string? ProgramName { get; set; }
				[JsonProperty("programType")] public eMiceProgramType ProgramType { get; set; }
				[JsonProperty("sessionName")] public string? SessionName { get; set; }
				[JsonProperty("stateCode")] public eMiceSessionStateCode StateCode { get; set; }
				[JsonProperty("startDateTime")] public DateTime StartDateTime { get; set; }
				[JsonProperty("endDateTime")] public DateTime EndDateTime { get; set; }
			}

			public partial class TicketInfoEntity
			{
				[JsonProperty("ticketId")] public string? TicketId { get; set; }
				[JsonProperty("eventId")] public long EventId { get; set; }
				[JsonProperty("programId")] public long ProgramId { get; set; }
				[JsonProperty("sessionId")] public long SessionId { get; set; }
				[JsonProperty("ticketName")] public string? TicketName { get; set; }
				[JsonProperty("onlineCode")] public int OnlineCode { get; set; }
				[JsonProperty("createUserId")] public long CreateUserId { get; set; }
				[JsonProperty("updateUserId")] public long UpdateUserId { get; set; }
				[JsonProperty("createDatetime")] public DateTime CreateDatetime { get; set; }
				[JsonProperty("updateDatetime")] public DateTime UpdateDatetime { get; set; }
			}

			public partial class UserPackageInfo
			{
				[JsonProperty("eventId")] public long EventId { get; set; }
				[JsonProperty("eventName")] public string? EventName { get; set; }
				[JsonProperty("eventStateCode")] public eMiceEventStateType EventStateCode { get; set; }
				[JsonProperty("eventImage")] public string? EventImage { get; set; }
				[JsonProperty("startDatetime")] public DateTime StartDatetime { get; set; }
				[JsonProperty("endDatetime")] public DateTime EndDatetime { get; set; }
				[JsonProperty("packageId")] public string? PackageId { get; set; }
				[JsonProperty("packageName")] public string? PackageName { get; set; }
				[JsonProperty("packageType")] public eMicePackageType PackageType { get; set; }
				[JsonProperty("ticketInfoList")] public IEnumerable<TicketInfoDetailEntity>? TicketInfoList { get; set; }
				[JsonProperty("price")] public long Price { get; set; }
				[JsonProperty("authorityCode")] public eMiceAuthorityCode AuthorityCode { get; set; }
			}

		}

    }
}
