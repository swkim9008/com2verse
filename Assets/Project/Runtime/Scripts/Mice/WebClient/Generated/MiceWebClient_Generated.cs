using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

#nullable enable
namespace Com2Verse.Mice
{
    public static partial class MiceWebClient
    {
		public static class Event
		{
			/// <summary>
			/// 전체 event list와 program, session 전체 정보를 내려 받는다. 이때 인증을 가지고 들어올시에 유저 정보가 없다면 정보를 생성한다.
			/// </summary>
			public static ArraySupport.Response<Entities.EventEntity> EventGet(eMiceLangCode? lang = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("lang", () => lang!.Value.ToString(), () => lang.HasValue)
				);
				return ArraySupport.GET<Entities.EventEntity>($"{MiceWebClient.REST_API_URL}/api/Event{query}");
			}
			/// <summary>
			/// 주어진 event 정보를 가져온다.
			/// </summary>
			public static Response<Entities.EventEntity> EventGet_EventId(long eventId, eMiceLangCode? lang = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("lang", () => lang!.Value.ToString(), () => lang.HasValue)
				);
				return MiceWebClient.GET<Entities.EventEntity>($"{MiceWebClient.REST_API_URL}/api/Event/{eventId}{query}");
			}
			/// <summary>
			/// 주어진 event의 program 정보를 가져온다.
			/// </summary>
			public static ArraySupport.Response<Entities.ProgramEntity> ProgramGet_EventId(long eventId, eMiceLangCode? lang = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("lang", () => lang!.Value.ToString(), () => lang.HasValue)
				);
				return ArraySupport.GET<Entities.ProgramEntity>($"{MiceWebClient.REST_API_URL}/api/Event/{eventId}/program{query}");
			}
			/// <summary>
			/// 주어진 program 정보를 가져온다.
			/// </summary>
			public static Response<Entities.ProgramEntity> ProgramGet_ProgramId(long programId, eMiceLangCode? lang = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("lang", () => lang!.Value.ToString(), () => lang.HasValue)
				);
				return MiceWebClient.GET<Entities.ProgramEntity>($"{MiceWebClient.REST_API_URL}/api/Event/program/{programId}{query}");
			}
			/// <summary>
			/// 주어진 program 정보와 설문조사 등을 가져온다.
			/// </summary>
			public static Response<Entities.ProgramEntity> OnlyGet_ProgramId(long programId, eMiceLangCode? lang = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("lang", () => lang!.Value.ToString(), () => lang.HasValue)
				);
				return MiceWebClient.GET<Entities.ProgramEntity>($"{MiceWebClient.REST_API_URL}/api/Event/program/{programId}/only{query}");
			}
			/// <summary>
			/// 주어진 program의 session 정보를 가져온다.
			/// </summary>
			public static ArraySupport.Response<Entities.SessionEntity> SessionGet_ProgramId(long programId, eMiceLangCode? lang = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("lang", () => lang!.Value.ToString(), () => lang.HasValue)
				);
				return ArraySupport.GET<Entities.SessionEntity>($"{MiceWebClient.REST_API_URL}/api/Event/program/{programId}/session{query}");
			}
			/// <summary>
			/// 주어진 program의 session 정보를 가져온다.
			/// </summary>
			public static Response<Entities.SessionEntity> SessionGet_SessionId(long sessionId, eMiceLangCode? lang = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("lang", () => lang!.Value.ToString(), () => lang.HasValue)
				);
				return MiceWebClient.GET<Entities.SessionEntity>($"{MiceWebClient.REST_API_URL}/api/Event/session/{sessionId}{query}");
			}
			/// <summary>
			/// 해당 이벤트의 상품 리스트를 가져온다.
			/// </summary>
			public static ArraySupport.Response<Entities.PackageEntity> PackagesGet_EventId(long eventId, eMiceLangCode? lang = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("lang", () => lang!.Value.ToString(), () => lang.HasValue)
				);
				return ArraySupport.GET<Entities.PackageEntity>($"{MiceWebClient.REST_API_URL}/api/Event/{eventId}/packages{query}");
			}
			/// <summary>
			/// 해당 이벤트의 설문 조사 리스틀 가져온다.
			/// </summary>
			public static Response<Entities.SurveyInfo> SurveysGet_EventId(long eventId, eMiceLangCode? lang = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("lang", () => lang!.Value.ToString(), () => lang.HasValue)
				);
				return MiceWebClient.GET<Entities.SurveyInfo>($"{MiceWebClient.REST_API_URL}/api/Event/{eventId}/surveys{query}");
			}
			/// <summary>
			/// 일반 라운지 입장 (가능 여부 체크) API
			/// <para>참조: https://jira.com2us.com/wiki/pages/viewpage.action?pageId=331474087</para>
			/// </summary>
			public static Response<Entities.CheckResult> EnterLoungePost_EventId(long eventId, eMiceLangCode? lang = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("lang", () => lang!.Value.ToString(), () => lang.HasValue)
				);
				return MiceWebClient.POST<Entities.CheckResult>($"{MiceWebClient.REST_API_URL}/api/Event/{eventId}/EnterLounge{query}");
			}
			/// <summary>
			/// 체험 라운지 입장 (가능 여부 체크) API
			/// </summary>
			public static Response<Entities.CheckResult> EnterFreeLoungePost_EventId(long eventId, eMiceLangCode? lang = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("lang", () => lang!.Value.ToString(), () => lang.HasValue)
				);
				return MiceWebClient.POST<Entities.CheckResult>($"{MiceWebClient.REST_API_URL}/api/Event/{eventId}/EnterFreeLounge{query}");
			}
			/// <summary>
			/// 쿠폰 사용 가능 여부를 체크한다.
			/// <para>noCoupon: 쿠폰없음,alreadyUsed: 이미사용,deleted:삭제됨</para>
			/// </summary>
			public static Response<Entities.CheckResult> CheckPost_EventId(long eventId, string? payload = null) => MiceWebClient.POST<Entities.CheckResult>($"{MiceWebClient.REST_API_URL}/api/Event/{eventId}/Coupon/Check", !string.IsNullOrEmpty(payload) ? payload : "");
			/// <summary>
			/// 쿠폰을 통해 티켓을 구매한다.
			/// <para>쿠폰 번호를 함께 넣어준다.</para>
			/// </summary>
			public static ArraySupport.Response<Entities.TicketEntity> TicketPost_EventId(long eventId, string? payload = null) => ArraySupport.POST<Entities.TicketEntity>($"{MiceWebClient.REST_API_URL}/api/Event/{eventId}/Ticket", !string.IsNullOrEmpty(payload) ? payload : "");
			/// <summary>
			/// 쿠폰이 필요없는 티켓을 발급받는다.
			/// <para>패키지 id ( ex) tf-001 ) 를 받는다 .</para>
			/// </summary>
			public static ArraySupport.Response<Entities.TicketEntity> NoCouponPost_EventId(long eventId, string? payload = null) => ArraySupport.POST<Entities.TicketEntity>($"{MiceWebClient.REST_API_URL}/api/Event/{eventId}/Ticket/NoCoupon", !string.IsNullOrEmpty(payload) ? payload : "");
			/// <summary>
			/// 홀(Session) 입장 가능 여부 체크
			/// <para>티켓이 반드시 필요하며 세션의 상태에 때라 일반 유저는 입장이 안될 수 있다.</para>
			/// </summary>
			public static Response<Entities.CheckResult> EnterHallPost_SessionId(long sessionId) => MiceWebClient.POST<Entities.CheckResult>($"{MiceWebClient.REST_API_URL}/api/Event/Session/{sessionId}/EnterHall");
			/// <summary>
			/// 스트리밍 url을 얻는다.
			/// </summary>
			public static Response<Entities.StreamingStatusEntity> StreamingGet_SessionId(long sessionId) => MiceWebClient.GET<Entities.StreamingStatusEntity>($"{MiceWebClient.REST_API_URL}/api/Event/Streaming/{sessionId}");
			/// <summary>
			/// 그룹(채팅) 정보를 얻는다.
			/// <para>miceType</para>
			/// <para>- Lobby : 0</para>
			/// <para>- Lounge : 1</para>
			/// <para>- Session : 3</para>
			/// <para>- Free Lounge : 5</para>
			/// </summary>
			public static Response<Entities.GroupInfo> GroupGet_MiceType_RoomId(MiceType miceType, long roomId) => MiceWebClient.GET<Entities.GroupInfo>($"{MiceWebClient.REST_API_URL}/api/Event/Group/{miceType}/{roomId}");
			/// <summary>
			/// 유저를 해당 채팅 그룹으로 이동시킨다.
			/// <para>miceType</para>
			/// <para>- Lobby : 0</para>
			/// <para>- Lounge : 1</para>
			/// <para>- Session : 3</para>
			/// <para>- Free Lounge : 5</para>
			/// </summary>
			public static Response<Entities.GroupInfo> MovePost_MiceType_RoomId(MiceType miceType, long roomId) => MiceWebClient.POST<Entities.GroupInfo>($"{MiceWebClient.REST_API_URL}/api/Event/Group/Move/{miceType}/{roomId}");
			/// <summary>
			/// 유저를 해당 채팅 그룹에서 내보낸다.
			/// <para>miceType</para>
			/// <para>- Lobby : 0</para>
			/// <para>- Lounge : 1</para>
			/// <para>- Session : 3</para>
			/// <para>- Free Lounge : 5</para>
			/// </summary>
			public static Response<Entities.CheckResult> DeletePost(MiceType? miceType = null, long? roomId = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("miceType", () => miceType!.Value.ToString(), () => miceType.HasValue),
					("roomId", () => roomId!.Value.ToString(), () => roomId.HasValue)
				);
				return MiceWebClient.POST<Entities.CheckResult>($"{MiceWebClient.REST_API_URL}/api/Event/Group/Delete{query}");
			}
			/// <summary>
			/// 주어진 session의 상태를 가져온다.
			/// </summary>
			public static Response StateGet_SessionId(long sessionId) => MiceWebClient.GET($"{MiceWebClient.REST_API_URL}/api/Event/Session/{sessionId}/State");
		}

		public static class Notice
		{
			/// <summary>
			/// 베너 가져오기
			/// </summary>
			public static ArraySupport.Response<Entities.BannerEntity> BannersGet(eMiceBannerCode? bannerCode = null, eMiceLangCode? lang = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("bannerCode", () => bannerCode!.Value.ToString(), () => bannerCode.HasValue),
					("lang", () => lang!.Value.ToString(), () => lang.HasValue)
				);
				return ArraySupport.GET<Entities.BannerEntity>($"{MiceWebClient.REST_API_URL}/api/Notice/Banners{query}");
			}
			/// <summary>
			/// 공지사항 전체 보기
			/// </summary>
			public static ArraySupport.Response<Entities.NoticeBoardEntity> NoticeGet(eMiceLangCode? lang = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("lang", () => lang!.Value.ToString(), () => lang.HasValue)
				);
				return ArraySupport.GET<Entities.NoticeBoardEntity>($"{MiceWebClient.REST_API_URL}/api/Notice{query}");
			}
			/// <summary>
			/// 공지사항 페이지 보기
			/// </summary>
			public static Response<Entities.NoticeInfo> NoticeGet_Skip_Limit(int skip, int limit, eMiceLangCode? lang = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("lang", () => lang!.Value.ToString(), () => lang.HasValue)
				);
				return MiceWebClient.GET<Entities.NoticeInfo>($"{MiceWebClient.REST_API_URL}/api/Notice/{skip}/{limit}{query}");
			}
			/// <summary>
			/// 공지사항 클릭시
			/// </summary>
			public static Response<Entities.NoticeBoardEntity> DetailsGet_BoardSeq(int boardSeq, eMiceLangCode? lang = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("lang", () => lang!.Value.ToString(), () => lang.HasValue)
				);
				return MiceWebClient.GET<Entities.NoticeBoardEntity>($"{MiceWebClient.REST_API_URL}/api/Notice/Details/{boardSeq}{query}");
			}
			/// <summary>
			/// 상단 메인 공지사항 뿌려줄 것
			/// </summary>
			public static ArraySupport.Response<Entities.MainNoticeResult> MainGet(eMiceLangCode? lang = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("lang", () => lang!.Value.ToString(), () => lang.HasValue)
				);
				return ArraySupport.GET<Entities.MainNoticeResult>($"{MiceWebClient.REST_API_URL}/api/Notice/Main{query}");
			}
		}

#if ENV_DEV || ENABLE_CHEATING
		public static class OperatorTest
		{
			/// <summary>
			/// 프로그램단위로 세션의 상태 변경
			/// <para>program id = 프로그램 id,  IsAllCancel = 0 인경우 시작, 1 인경우 종료</para>
			/// </summary>
			public static Response ProgramStatusChangePost_ProgramId_IsAllCancel(long programId, int isAllCancel) => MiceWebClient.POST($"{MiceWebClient.REST_API_URL}/dev/OperatorTest/ProgramStatusChange/{programId}/{isAllCancel}");
			/// <summary>
			/// 진행중인 세션을 종료하고 다음세션을 시작한다.
			/// <para>playSessionId : 시작할 세션의 Id  finishSessionId : 종료할 세션의 Id</para>
			/// <para>IsSessionEnter  - 1인경우 다음세션 즉시 시작(티켓이 있거나 관리자는 다음세션으로 이동)  - 0 인경우 다음세션이 바로 시작하지 않으므로 일괄 라운지로 이동.</para>
			/// </summary>
			public static Response SessionChangePost_PlaySessionId_FinishSessionId_IsSessionEnter(long playSessionId, long finishSessionId, int isSessionEnter) => MiceWebClient.POST($"{MiceWebClient.REST_API_URL}/dev/OperatorTest/SessionChange/{playSessionId}/{finishSessionId}/{isSessionEnter}");
			/// <summary>
			/// 진행중인 세션의 상태 변경
			/// <para>IsSessionEnter 가 0 인경우 세션이 종료되므로 세션에 남아있는 유저들은 모두 라운지로 퇴장한다.</para>
			/// </summary>
			public static Response SessionStatusChangePost_SessionId_IsSessionEnter(long sessionId, int isSessionEnter) => MiceWebClient.POST($"{MiceWebClient.REST_API_URL}/dev/OperatorTest/SessionStatusChange/{sessionId}/{isSessionEnter}");
			/// <summary>
			/// 스트리밍 관련 : 시작 또는 재시작
			/// <para>tick 은 재생 시작 시간.  sourceType은 뭔지 모르지만 int Body에는 스트리밍할 url 을 담는다.</para>
			/// </summary>
			public static Response StreamingStartPost_SessionId_SourceType_Tick(long sessionId, int sourceType, long tick, string? payload = null) => MiceWebClient.POST($"{MiceWebClient.REST_API_URL}/dev/OperatorTest/StreamingStart/{sessionId}/{sourceType}/{tick}", !string.IsNullOrEmpty(payload) ? payload : "");
			/// <summary>
			/// 스트리밍 관련 : 종료
			/// <para>커맨드 관련 input value 는 StreamingStart 참고</para>
			/// </summary>
			public static Response StreamingEndPost_SessionId_SourceType_Tick(long sessionId, int sourceType, long tick, string? payload = null) => MiceWebClient.POST($"{MiceWebClient.REST_API_URL}/dev/OperatorTest/StreamingEnd/{sessionId}/{sourceType}/{tick}", !string.IsNullOrEmpty(payload) ? payload : "");
			/// <summary>
			/// 사용자내보내기
			/// </summary>
			public static Response UserKickOutPost_AccountId(long accountId) => MiceWebClient.POST($"{MiceWebClient.REST_API_URL}/dev/OperatorTest/UserKickOut/{accountId}");
			/// <summary>
			/// 사용자 마이크 ON/OFF 
			/// </summary>
			public static Response UserMicOnOffPost_SessionId_EnableAccountId_DisableAccountId(long sessionId, long enableAccountId, long disableAccountId) => MiceWebClient.POST($"{MiceWebClient.REST_API_URL}/dev/OperatorTest/UserMicOnOff/{sessionId}/{enableAccountId}/{disableAccountId}");
			/// <summary>
			/// 질문상태변경
			/// <para>surveyNo = 질문 번호  IsProgress = 0 인경우 그 질문이 종료되었다는 알림. 1인경우 질문이 시작되었다는 알림</para>
			/// </summary>
			public static Response QuestionStatusChangePost_SurveyNo_SessionId_IsProgress(long surveyNo, long sessionId, int isProgress) => MiceWebClient.POST($"{MiceWebClient.REST_API_URL}/dev/OperatorTest/QuestionStatusChange/{surveyNo}/{sessionId}/{isProgress}");
			/// <summary>
			/// 이팩트 : 시작
			/// <para>이팩트 리스트 : https://docs.google.com/spreadsheets/d/1U0M-rHokDZDP18d8agZyyJehw_0L1o-FwrHiIlXG47M/edit?usp=sharing</para>
			/// </summary>
			public static Response EffectOnPost_SessionId_EffectType_EffectCode(long sessionId, string effectType, long effectCode) => MiceWebClient.POST($"{MiceWebClient.REST_API_URL}/dev/OperatorTest/EffectOn/{sessionId}/{effectType}/{effectCode}");
			/// <summary>
			/// 이팩트 : 종료
			/// <para>이팩트 리스트 : https://docs.google.com/spreadsheets/d/1U0M-rHokDZDP18d8agZyyJehw_0L1o-FwrHiIlXG47M/edit?usp=sharing</para>
			/// </summary>
			public static Response EffectOffPost_SessionId_EffectType_EffectCode(long sessionId, string effectType, long effectCode) => MiceWebClient.POST($"{MiceWebClient.REST_API_URL}/dev/OperatorTest/EffectOff/{sessionId}/{effectType}/{effectCode}");
			/// <summary>
			/// 이벤트 데이터 변경 알림
			/// </summary>
			public static Response EventDataChangePost_EventId(long eventId) => MiceWebClient.POST($"{MiceWebClient.REST_API_URL}/dev/OperatorTest/EventDataChange/{eventId}");
			/// <summary>
			/// 이벤트 공간 알림. spacestatecode 2가 생성 3이 삭제 2만 쓰세요.
			/// </summary>
			public static Response EventSpaceStateChangePost_EventId_SpaceStateCode(long eventId, int SpaceStateCode) => MiceWebClient.POST($"{MiceWebClient.REST_API_URL}/dev/OperatorTest/EventSpaceStateChange/{eventId}/{SpaceStateCode}");
		}
#else
		public static class OperatorTest
		{
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response ProgramStatusChangePost_ProgramId_IsAllCancel(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response SessionChangePost_PlaySessionId_FinishSessionId_IsSessionEnter(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response SessionStatusChangePost_SessionId_IsSessionEnter(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response StreamingStartPost_SessionId_SourceType_Tick(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response StreamingEndPost_SessionId_SourceType_Tick(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response UserKickOutPost_AccountId(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response UserMicOnOffPost_SessionId_EnableAccountId_DisableAccountId(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response QuestionStatusChangePost_SurveyNo_SessionId_IsProgress(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response EffectOnPost_SessionId_EffectType_EffectCode(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response EffectOffPost_SessionId_EffectType_EffectCode(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response EventDataChangePost_EventId(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response EventSpaceStateChangePost_EventId_SpaceStateCode(params object[] unused) => default;
		}
#endif

		public static class Participant
		{
			/// <summary>
			/// 참가자 리스트에 계정을 넣어준다, 로직서버에선 사용하지 않으며 웹이나 모바일 앱이 사용한다. 유효시간은 5분이며 이동이 없더라도 5분마다 한번씩 호출해 준다.
			/// <para>miceType</para>
			/// <para>- Lobby : 0</para>
			/// <para>- Lounge : 1</para>
			/// <para>- Session : 3</para>
			/// <para>- Free Lounge : 5</para>
			/// </summary>
			public static Response ParticipantPost_MiceType_RoomId(MiceType miceType, long roomId) => MiceWebClient.POST($"{MiceWebClient.REST_API_URL}/api/Participant/Participant/{miceType}/{roomId}");
			/// <summary>
			/// 참가자 리스트를 가져온다. 라운지와 Session에서만 동작한다.
			/// <para>miceType</para>
			/// <para>- Lobby : 0</para>
			/// <para>- Lounge : 1</para>
			/// <para>- Session : 3</para>
			/// <para>- Free Lounge : 5</para>
			/// </summary>
			public static ArraySupport.Response<Entities.Participant> ParticipantGet_MiceType_RoomId(MiceType miceType, long roomId) => ArraySupport.GET<Entities.Participant>($"{MiceWebClient.REST_API_URL}/api/Participant/Participant/{miceType}/{roomId}");
			/// <summary>
			/// 참가자 인원수 를 가져온다. 라운지와 Session에서만 동작한다.
			/// <para>miceType</para>
			/// <para>- Lobby : 0</para>
			/// <para>- Lounge : 1</para>
			/// <para>- Session : 3</para>
			/// <para>- Free Lounge : 5</para>
			/// </summary>
			public static Response CountGet_MiceType_RoomId(MiceType miceType, long roomId) => MiceWebClient.GET($"{MiceWebClient.REST_API_URL}/api/Participant/Participant/Count/{miceType}/{roomId}");
			/// <summary>
			/// 참가자 리스트를 가져온다. 라운지와 Session에서만 동작한다. SKip Limit가 적용된다.
			/// <para>miceType</para>
			/// <para>- Lobby : 0</para>
			/// <para>- Lounge : 1</para>
			/// <para>- Session : 3</para>
			/// <para>- Free Lounge : 5</para>
			/// </summary>
			public static ArraySupport.Response<Entities.Participant> ParticipantGet_MiceType_RoomId_Skip_Limit(MiceType miceType, long roomId, int skip, int limit) => ArraySupport.GET<Entities.Participant>($"{MiceWebClient.REST_API_URL}/api/Participant/Participant/{miceType}/{roomId}/{skip}/{limit}");
			/// <summary>
			/// 참가자 닉네임으로 검색한다.
			/// <para>miceType</para>
			/// <para>- Lobby : 0</para>
			/// <para>- Lounge : 1</para>
			/// <para>- Session : 3</para>
			/// <para>- Free Lounge : 5</para>
			/// </summary>
			public static ArraySupport.Response<Entities.Participant> ParticipantGet_MiceType_RoomId_Query(MiceType miceType, long roomId, string query) => ArraySupport.GET<Entities.Participant>($"{MiceWebClient.REST_API_URL}/api/Participant/Participant/{miceType}/{roomId}/{query}");
		}

		public static class Prize
		{
			/// <summary>
			/// 뽑기 이벤트 정보 (시도횟수) 를 가져온다. 티켓이 없으면 오류를 낸다. 완료 되지 않은 당첨 오류를 낼수도 있다.
			/// </summary>
			public static Response<Entities.PrizeInfoEntity> InfoGet_PrizeId(long prizeId) => MiceWebClient.GET<Entities.PrizeInfoEntity>($"{MiceWebClient.REST_API_URL}/api/Prize/Info/{prizeId}");
			/// <summary>
			/// 경품 리스트를 본다.
			/// </summary>
			public static ArraySupport.Response<Entities.PrizeItem> ItemsGet_PrizeId(long prizeId) => ArraySupport.GET<Entities.PrizeItem>($"{MiceWebClient.REST_API_URL}/api/Prize/Items/{prizeId}");
			/// <summary>
			/// 뽑기를 진행한다.
			/// </summary>
			public static Response<Entities.PrizeItem> TryPost(long? payload = null) => MiceWebClient.POST<Entities.PrizeItem>($"{MiceWebClient.REST_API_URL}/api/Prize/Try", payload.HasValue ? payload.ToString() : "");
			/// <summary>
			/// 당첨 정보를 입력한다.
			/// </summary>
			public static Response<Entities.CheckResult> PersonalInfoPost(Entities.PrizePersonalInfo? payload = null) => MiceWebClient.POST<Entities.CheckResult>($"{MiceWebClient.REST_API_URL}/api/Prize/PersonalInfo", JsonConvert.SerializeObject(payload));
		}

		public static class Question
		{
			/// <summary>
			/// 검색
			/// <para>offset : 받을 리스트의 개수 [필수]</para>
			/// <para>pagesize : 해당 페이지에 받은 게시물 개수 [필수]</para>
			/// <para>searchCategory : 검색 유형 뭘 적어도 현재는 content임</para>
			/// <para>- content : 내용으로 검색</para>
			/// <para>- author : 글쓴이로 검색</para>
			/// <para>keyword : 검색어</para>
			/// <para>pageSortType : 정렬유형</para>
			/// <para>- date: 생성일자 desc</para>
			/// <para>- dateAsc: 생성일자 asc</para>
			/// <para>- likeCount : 좋아요 순 desc</para>
			/// <para>- likeCountAsc : 좋아요 순 asc</para>
			/// <para>- viewCount : 조회 수 순 desc</para>
			/// <para>- viewCountAsc : 조회 수 순 asc</para>
			/// </summary>
			public static ArraySupport.Response<Entities.QuestionResult> QuestionGet_SessionId(long sessionId, int? offset = null, int? pageSize = null, string? pageSortType = null, string? keyword = null, string? searchCategory = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("offset", () => offset!.Value.ToString(), () => offset.HasValue),
					("pageSize", () => pageSize!.Value.ToString(), () => pageSize.HasValue),
					("pageSortType", () => pageSortType!, () => !string.IsNullOrEmpty(pageSortType)),
					("keyword", () => keyword!, () => !string.IsNullOrEmpty(keyword)),
					("searchCategory", () => searchCategory!, () => !string.IsNullOrEmpty(searchCategory))
				);
				return ArraySupport.GET<Entities.QuestionResult>($"{MiceWebClient.REST_API_URL}/api/Question/{sessionId}{query}");
			}
			/// <summary>
			/// 좋아요 누름
			/// </summary>
			public static Response<Entities.QuestionResult> LikePost_QuestionSeq(int questionSeq) => MiceWebClient.POST<Entities.QuestionResult>($"{MiceWebClient.REST_API_URL}/api/Question/{questionSeq}/Like");
			/// <summary>
			/// 질문 게시물 작성
			/// <para>결과 값은 다음과 같을수 있다.</para>
			/// <para>- CheckResult: Result=false, Reason=excessLimit - 질문수 초과</para>
			/// <para>- 또는 정상 처리되었 다면 Question 객체</para>
			/// </summary>
			public static Response<Entities.QuestionResult> WritePost(Entities.QuestionCreateRequest? payload = null) => MiceWebClient.POST<Entities.QuestionResult>($"{MiceWebClient.REST_API_URL}/api/Question/Write", JsonConvert.SerializeObject(payload));
			/// <summary>
			/// 질문 삭제
			/// </summary>
			public static Response DeletePost_QuestionSeq(int questionSeq) => MiceWebClient.POST($"{MiceWebClient.REST_API_URL}/api/Question/{questionSeq}/Delete");
			/// <summary>
			/// 게시물 업데이트
			/// </summary>
			public static Response<Entities.CheckResult> UpdatePost_QuestionSeq(int questionSeq, Entities.SessionQuestionEntity? payload = null) => MiceWebClient.POST<Entities.CheckResult>($"{MiceWebClient.REST_API_URL}/api/Question/{questionSeq}/Update", JsonConvert.SerializeObject(payload));
			/// <summary>
			/// 특정 게시물 조회
			/// </summary>
			public static Response<Entities.QuestionResult> DetailsGet_QuestionSeq(int questionSeq) => MiceWebClient.GET<Entities.QuestionResult>($"{MiceWebClient.REST_API_URL}/api/Question/{questionSeq}/details");
			/// <summary>
			/// 특정 세션의 특정 유저의 질문 목록 전체 보기
			/// </summary>
			public static ArraySupport.Response<Entities.QuestionResult> QuestionGet_SessionId_AccountId(long sessionId, long accountId) => ArraySupport.GET<Entities.QuestionResult>($"{MiceWebClient.REST_API_URL}/api/Question/{sessionId}/{accountId}");
			/// <summary>
			/// 특정 세션의 나의 질문과 다른 사용자의 질문 보기
			/// </summary>
			public static ArraySupport.Response<Entities.QuestionResult> MineGet_SessionId_Skip_Limit(long sessionId, int skip, int limit) => ArraySupport.GET<Entities.QuestionResult>($"{MiceWebClient.REST_API_URL}/api/Question/Mine/{sessionId}/{skip}/{limit}");
		}

#if ENV_DEV || ENABLE_CHEATING
		public static class Test
		{
			/// <summary>
			/// 사용된 쿼리를 리턴한다.
			/// </summary>
			public static Response QueriesGet() => MiceWebClient.GET($"{MiceWebClient.REST_API_URL}/dev/Test/queries");
			/// <summary>
			/// 오퍼레이터 명령을 Publish 한다.
			/// </summary>
			public static Response OperatorCommandPost() => MiceWebClient.POST($"{MiceWebClient.REST_API_URL}/dev/Test/OperatorCommand");
			/// <summary>
			/// 전체 event list를 내려 받는다. 인증 체크를 하지 않는다. 가급적 사용하지 말자. 테스트용으로 만든 것이다.
			/// </summary>
			public static ArraySupport.Response<Entities.EventEntity> EventsGet() => ArraySupport.GET<Entities.EventEntity>($"{MiceWebClient.REST_API_URL}/dev/Test/Events");
			/// <summary>
			/// 라운지 입장 가능 여부 체크 API, Test용 이며 모든 티켓을 발급한 후 무조건 진입 시킨다.
			/// </summary>
			public static Response<Entities.CheckResult> EnterLoungePost_EventId_AuthorityCode(long eventId, int authorityCode, long? payload = null) => MiceWebClient.POST<Entities.CheckResult>($"{MiceWebClient.REST_API_URL}/dev/Test/Events/{eventId}/EnterLounge/{authorityCode}", payload.HasValue ? payload.ToString() : "");
			/// <summary>
			/// 라운지 입장 가능 여부 체크 API, Test용 이며 세션id의 티켓을 발급한 후 무조건 진입 시킨다.
			/// </summary>
			public static Response<Entities.CheckResult> SessionIdPost_EventId_AuthorityCode_SessionId(long eventId, int authorityCode, long sessionId) => MiceWebClient.POST<Entities.CheckResult>($"{MiceWebClient.REST_API_URL}/dev/Test/Events/{eventId}/EnterLounge/{authorityCode}/SessionId/{sessionId}");
			/// <summary>
			/// 생성된 모든 유저에게 해당 이벤트의 모든 세션 티켓을 발급한다. 물론 테스트용이다.
			/// </summary>
			public static Response<Entities.CheckResult> GiveTicketsToAllUsersPost_EventId(long eventId) => MiceWebClient.POST<Entities.CheckResult>($"{MiceWebClient.REST_API_URL}/dev/Test/Events/{eventId}/GiveTicketsToAllUsers");
			/// <summary>
			/// GetUserListByGroup
			/// </summary>
			public static Response<Entities.GetUserListByGroupResponseResponseFormat> GetUserListByGroupPost(string? groupId = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("groupId", () => groupId!, () => !string.IsNullOrEmpty(groupId))
				);
				return MiceWebClient.POST<Entities.GetUserListByGroupResponseResponseFormat>($"{MiceWebClient.REST_API_URL}/dev/Test/GetUserListByGroup{query}");
			}
			/// <summary>
			/// DeleteGroupArea
			/// </summary>
			public static Response<Entities.DeleteGroupResponseResponseFormat> DeleteGroupAreaPost(string? groupId = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("groupId", () => groupId!, () => !string.IsNullOrEmpty(groupId))
				);
				return MiceWebClient.POST<Entities.DeleteGroupResponseResponseFormat>($"{MiceWebClient.REST_API_URL}/dev/Test/DeleteGroupArea{query}");
			}
			/// <summary>
			/// MoveUserToArea
			/// </summary>
			public static Response<Entities.CheckResult> MoveUserToAreaPost(Entities.MoveUserAreaRequest? payload = null) => MiceWebClient.POST<Entities.CheckResult>($"{MiceWebClient.REST_API_URL}/dev/Test/MoveUserToArea", JsonConvert.SerializeObject(payload));
			/// <summary>
			/// DeleteUserFromArea
			/// </summary>
			public static Response<Entities.CheckResult> DeleteUserFromAreaPost(string? userId = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("userId", () => userId!, () => !string.IsNullOrEmpty(userId))
				);
				return MiceWebClient.POST<Entities.CheckResult>($"{MiceWebClient.REST_API_URL}/dev/Test/DeleteUserFromArea{query}");
			}
			/// <summary>
			/// GetOrCreateGroupArea
			/// </summary>
			public static Response<Entities.GroupInfoResponseDataResponseFormat> GetOrCreateGroupAreaPost(Entities.AddGroupAreaRequest? payload = null) => MiceWebClient.POST<Entities.GroupInfoResponseDataResponseFormat>($"{MiceWebClient.REST_API_URL}/dev/Test/GetOrCreateGroupArea", JsonConvert.SerializeObject(payload));
			/// <summary>
			/// DB 로직 검증용.
			/// </summary>
			public static Response DBExecuteGet() => MiceWebClient.GET($"{MiceWebClient.REST_API_URL}/dev/Test/DBExecute");
			/// <summary>
			/// 주어진 템플릿에 배치된 오브젝트 정보를 가져온다.
			/// <para>라운지: 10400201000, 홀: 10400301000</para>
			/// </summary>
			public static Response TemplateGet_TemplateId(long templateId) => MiceWebClient.GET($"{MiceWebClient.REST_API_URL}/dev/Test/ServerObjects/Template/{templateId}");
			/// <summary>
			/// 주어진 템플릿에 배치된 오브젝트 정보를 가져온다.
			/// </summary>
			public static Response ServerObjectsGet(long? templateId = null, eMiceMappingType? mappingType = null, long? mappingId = null, long? loungeId = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("templateId", () => templateId!.Value.ToString(), () => templateId.HasValue),
					("mappingType", () => mappingType!.Value.ToString(), () => mappingType.HasValue),
					("mappingId", () => mappingId!.Value.ToString(), () => mappingId.HasValue),
					("loungeId", () => loungeId!.Value.ToString(), () => loungeId.HasValue)
				);
				return MiceWebClient.GET($"{MiceWebClient.REST_API_URL}/dev/Test/ServerObjects{query}");
			}
			/// <summary>
			/// 현재 전체 계장 사용자를 해당 룸의 참가자로 넣는다.
			/// </summary>
			public static Response MakeParticipantGet_MiceType_RoomId(MiceType miceType, long roomId) => MiceWebClient.GET($"{MiceWebClient.REST_API_URL}/dev/Test/MakeParticipant/{miceType}/{roomId}");
			/// <summary>
			/// Exception을 Throw 한다.
			/// </summary>
			public static Response ThrowGet() => MiceWebClient.GET($"{MiceWebClient.REST_API_URL}/dev/Test/Throw");
			/// <summary>
			/// DB를 지운후에 더미데이터 초기화 한다
			/// </summary>
			public static Response MakeDummyGet() => MiceWebClient.GET($"{MiceWebClient.REST_API_URL}/dev/Test/MakeDummy");
			/// <summary>
			/// 파일정보 분석
			/// </summary>
			public static Response<Entities.AnalyzeFileResult> AnalyzeFilePost(object? payload = null) => MiceWebClient.POST<Entities.AnalyzeFileResult>($"{MiceWebClient.REST_API_URL}/dev/Test/AnalyzeFile", payload?.ToString() ?? "");
			/// <summary>
			/// 이벤트 전체 동접 보기
			/// </summary>
			public static Response CUGet_EventId(long eventId) => MiceWebClient.GET($"{MiceWebClient.REST_API_URL}/dev/Test/CU/{eventId}");
			/// <summary>
			/// 이벤트 전체 DAU 보기
			/// </summary>
			public static Response DAUGet_EventId(long eventId) => MiceWebClient.GET($"{MiceWebClient.REST_API_URL}/dev/Test/DAU/{eventId}");
			/// <summary>
			/// 라운지 입장 가능 하도록 Set
			/// </summary>
			public static Response LoungePost_EventId_AccountId(long eventId, long accountId) => MiceWebClient.POST($"{MiceWebClient.REST_API_URL}/dev/Test/Enable/Enter/Lounge/{eventId}/{accountId}");
			/// <summary>
			/// 홀 입장 가능 하도록 Set
			/// </summary>
			public static Response HallPost_SessionId_AccountId(long sessionId, long accountId) => MiceWebClient.POST($"{MiceWebClient.REST_API_URL}/dev/Test/Enable/Enter/Hall/{sessionId}/{accountId}");
			/// <summary>
			/// 파일업로드
			/// </summary>
			public static Response<Entities.FileDownloadResponseResponseFormat> UploadFileFullPost(object? payload = null) => MiceWebClient.POST<Entities.FileDownloadResponseResponseFormat>($"{MiceWebClient.REST_API_URL}/dev/Test/UploadFileFull", payload?.ToString() ?? "");
			/// <summary>
			/// PUSH 를 보낸다.
			/// </summary>
			public static Response SendPushPost_PlayerId(string playerId, string? payload = null) => MiceWebClient.POST($"{MiceWebClient.REST_API_URL}/dev/Test/SendPush/{playerId}", !string.IsNullOrEmpty(payload) ? payload : "");
			/// <summary>
			/// 
			/// </summary>
			public static Response<Entities.CollectorModel> GetSnapShotPost(MiceType? type = null, long? eventId = null, long? accountId = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("type", () => type!.Value.ToString(), () => type.HasValue),
					("eventId", () => eventId!.Value.ToString(), () => eventId.HasValue),
					("accountId", () => accountId!.Value.ToString(), () => accountId.HasValue)
				);
				return MiceWebClient.POST<Entities.CollectorModel>($"{MiceWebClient.REST_API_URL}/dev/Test/Log/GetSnapShot{query}");
			}
			/// <summary>
			/// 
			/// </summary>
			public static Response GetSnapShot2Post(MiceType? type = null, long? eventId = null, long? accountId = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("type", () => type!.Value.ToString(), () => type.HasValue),
					("eventId", () => eventId!.Value.ToString(), () => eventId.HasValue),
					("accountId", () => accountId!.Value.ToString(), () => accountId.HasValue)
				);
				return MiceWebClient.POST($"{MiceWebClient.REST_API_URL}/dev/Test/Log/GetSnapShot2{query}");
			}
			/// <summary>
			/// 
			/// </summary>
			public static Response SetLogPost(MiceType? type = null, long? eventId = null, long? accountId = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("type", () => type!.Value.ToString(), () => type.HasValue),
					("eventId", () => eventId!.Value.ToString(), () => eventId.HasValue),
					("accountId", () => accountId!.Value.ToString(), () => accountId.HasValue)
				);
				return MiceWebClient.POST($"{MiceWebClient.REST_API_URL}/dev/Test/Log/SetLog{query}");
			}
			/// <summary>
			/// 
			/// </summary>
			public static Response CollectLogPost(MiceType? type = null, long? eventId = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("type", () => type!.Value.ToString(), () => type.HasValue),
					("eventId", () => eventId!.Value.ToString(), () => eventId.HasValue)
				);
				return MiceWebClient.POST($"{MiceWebClient.REST_API_URL}/dev/Test/Log/CollectLog{query}");
			}
			/// <summary>
			/// 
			/// </summary>
			public static Response EventTotalPost(long? eventId = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("eventId", () => eventId!.Value.ToString(), () => eventId.HasValue)
				);
				return MiceWebClient.POST($"{MiceWebClient.REST_API_URL}/dev/Test/HiveLog/EventTotal{query}");
			}
			/// <summary>
			/// 
			/// </summary>
			public static Response MakeQueryPost_TotalCount_StartSeq(int totalCount, int startSeq, Entities.RequestPrizeItemSeq? payload = null) => MiceWebClient.POST($"{MiceWebClient.REST_API_URL}/dev/Test/Prize/MakeQuery/{totalCount}/{startSeq}", JsonConvert.SerializeObject(payload));
			/// <summary>
			/// 사전 질문 list 를 받는다.
			/// <para>offset : 받을 list 중 첫번째 게시물의 시작 번호,</para>
			/// <para>offset : 받을 리스트의 개수</para>
			/// <para>pagesize : 해당 페이지에 받은 게시물 개수</para>
			/// <para>page 정렬 유형</para>
			/// <para>- date: 생성일자 desc</para>
			/// <para>- dateAsc: 생성일자 asc</para>
			/// <para>- likeCount : 좋아요 순 desc</para>
			/// <para>- likeCountAsc : 좋아요 순 asc</para>
			/// <para>- viewCount : 조회 수 순 desc</para>
			/// <para>- viewCountAsc : 조회 수 순 asc</para>
			/// </summary>
			public static ArraySupport.Response<Entities.QuestionResult> SampleGet_SessionId(long sessionId, string? pageSortType = null, int? offset = null, int? pageSize = null, string? keyword = null, string? searchCategory = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("pageSortType", () => pageSortType!, () => !string.IsNullOrEmpty(pageSortType)),
					("offset", () => offset!.Value.ToString(), () => offset.HasValue),
					("pageSize", () => pageSize!.Value.ToString(), () => pageSize.HasValue),
					("keyword", () => keyword!, () => !string.IsNullOrEmpty(keyword)),
					("searchCategory", () => searchCategory!, () => !string.IsNullOrEmpty(searchCategory))
				);
				return ArraySupport.GET<Entities.QuestionResult>($"{MiceWebClient.REST_API_URL}/dev/Test/Question/Sample/{sessionId}{query}");
			}
		}
#else
		public static class Test
		{
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response QueriesGet(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response OperatorCommandPost(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static ArraySupport.Response<Entities.EventEntity> EventsGet(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response<Entities.CheckResult> EnterLoungePost_EventId_AuthorityCode(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response<Entities.CheckResult> SessionIdPost_EventId_AuthorityCode_SessionId(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response<Entities.CheckResult> GiveTicketsToAllUsersPost_EventId(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response<Entities.GetUserListByGroupResponseResponseFormat> GetUserListByGroupPost(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response<Entities.DeleteGroupResponseResponseFormat> DeleteGroupAreaPost(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response<Entities.CheckResult> MoveUserToAreaPost(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response<Entities.CheckResult> DeleteUserFromAreaPost(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response<Entities.GroupInfoResponseDataResponseFormat> GetOrCreateGroupAreaPost(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response DBExecuteGet(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response TemplateGet_TemplateId(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response ServerObjectsGet(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response MakeParticipantGet_MiceType_RoomId(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response ThrowGet(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response MakeDummyGet(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response<Entities.AnalyzeFileResult> AnalyzeFilePost(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response CUGet_EventId(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response DAUGet_EventId(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response LoungePost_EventId_AccountId(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response HallPost_SessionId_AccountId(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response<Entities.FileDownloadResponseResponseFormat> UploadFileFullPost(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response SendPushPost_PlayerId(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response<Entities.CollectorModel> GetSnapShotPost(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response GetSnapShot2Post(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response SetLogPost(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response CollectLogPost(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response EventTotalPost(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response MakeQueryPost_TotalCount_StartSeq(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static ArraySupport.Response<Entities.QuestionResult> SampleGet_SessionId(params object[] unused) => default;
		}
#endif

#if ENV_DEV || ENABLE_CHEATING
		public static class Token
		{
			/// <summary>
			/// JWT 토큰을 인증 백엔드 API로 가져온다.
			/// </summary>
			public static Response GenerateGet_Did_Pid(string did, long pid) => MiceWebClient.GET($"{MiceWebClient.REST_API_URL}/dev/Token/generate/{did}/{pid}");
			/// <summary>
			/// JWT 토큰을 인증 백엔드 API로 Refresh한다.
			/// </summary>
			public static Response RefreshGet_C2vRefreshToken(string c2vRefreshToken) => MiceWebClient.GET($"{MiceWebClient.REST_API_URL}/dev/Token/refresh/{c2vRefreshToken}");
			/// <summary>
			/// JWT 토큰으로 인증한다.
			/// <para>/api/Token/generate 로 생성된 토큰을 넣고 테스트 한다.</para>
			/// </summary>
			public static Response AuthGet() => MiceWebClient.GET($"{MiceWebClient.REST_API_URL}/dev/Token/auth");
			/// <summary>
			/// 컴투버스 아이디로 token 뽑기
			/// </summary>
			public static Response C2vauthGet_Id(string id) => MiceWebClient.GET($"{MiceWebClient.REST_API_URL}/dev/Token/c2vauth/{id}");
			/// <summary>
			/// 
			/// </summary>
			public static Response ClaimGet() => MiceWebClient.GET($"{MiceWebClient.REST_API_URL}/dev/Token/claim");
			/// <summary>
			/// JWK 키를 가져온다.
			/// </summary>
			public static Response KeyGet() => MiceWebClient.GET($"{MiceWebClient.REST_API_URL}/dev/Token/jwk/key");
		}
#else
		public static class Token
		{
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response GenerateGet_Did_Pid(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response RefreshGet_C2vRefreshToken(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response AuthGet(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response C2vauthGet_Id(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response ClaimGet(params object[] unused) => default;
			[System.Obsolete("MUST BE USED ONLY IN 'ENV_DEV || ENABLE_CHEATING' ENVIRONMENT.", true)]
			public static Response KeyGet(params object[] unused) => default;
		}
#endif

		public static class User
		{
			/// <summary>
			/// 유저의 MICE 정보를 체크해 본다 정보가 필요하면 생성한다.
			/// </summary>
			public static Response<Entities.AccountEntity> CheckPost() => MiceWebClient.POST<Entities.AccountEntity>($"{MiceWebClient.REST_API_URL}/api/User/Account/Check");
			/// <summary>
			/// 나의 Account 정보를 가져온다.
			/// </summary>
			public static Response<Entities.AccountEntity> AccountGet() => MiceWebClient.GET<Entities.AccountEntity>($"{MiceWebClient.REST_API_URL}/api/User/Account");
			/// <summary>
			/// 나의 Account 정보를 수정한다. 이름은 필수항목이다.
			/// detail 이나 config는 다른 API 로 수정한다.
			/// 만약 public을 false로 했을 경우 교환했던 명함은 삭제되며 복구 되지 않는다.
			/// </summary>
			public static Response<Entities.AccountEntity> AccountPost(Entities.AccountInfo? payload = null) => MiceWebClient.POST<Entities.AccountEntity>($"{MiceWebClient.REST_API_URL}/api/User/Account", JsonConvert.SerializeObject(payload));
			/// <summary>
			/// 계정 탈퇴를 한다.
			/// </summary>
			public static Response<Entities.CheckResult> AccountDelete() => MiceWebClient.DELETE<Entities.CheckResult>($"{MiceWebClient.REST_API_URL}/api/User/Account");
			/// <summary>
			/// 나의 Account 의 PlayerId 를 저장한다.
			/// </summary>
			public static Response PidPost(string? payload = null) => MiceWebClient.POST($"{MiceWebClient.REST_API_URL}/api/User/Account/Pid", !string.IsNullOrEmpty(payload) ? payload : "");
			/// <summary>
			/// 나의 모든 티켓 정보를 가져온다.
			/// </summary>
			public static ArraySupport.Response<Entities.TicketEntity> TicketsGet() => ArraySupport.GET<Entities.TicketEntity>($"{MiceWebClient.REST_API_URL}/api/User/Account/Tickets");
			/// <summary>
			/// 나의 모든 패키지 정보를 가져온다.
			/// </summary>
			public static ArraySupport.Response<Entities.UserPackageInfo> PackagesGet(eMiceLangCode? lang = null)
			{
				var query = MiceWebClient.ConvertNullableParametersToQuery
				(
					("lang", () => lang!.Value.ToString(), () => lang.HasValue)
				);
				return ArraySupport.GET<Entities.UserPackageInfo>($"{MiceWebClient.REST_API_URL}/api/User/Account/Packages{query}");
			}
			/// <summary>
			/// 주어진 accountId로 Account 정보를 가져온다. 명함 비공개 상태면 오류를 낸다.
			/// </summary>
			public static Response<Entities.AccountEntity> AccountGet_TargetAccountId(long targetAccountId) => MiceWebClient.GET<Entities.AccountEntity>($"{MiceWebClient.REST_API_URL}/api/User/Account/{targetAccountId}");
			/// <summary>
			/// 명함을 교환한다.
			/// </summary>
			public static Response<Entities.CheckResult> ExchangePost(long? payload = null) => MiceWebClient.POST<Entities.CheckResult>($"{MiceWebClient.REST_API_URL}/api/User/Account/Card/Exchange", payload.HasValue ? payload.ToString() : "");
			/// <summary>
			/// 내가 교환한 명함 리스트를 본다.
			/// </summary>
			public static ArraySupport.Response<Entities.AccountEntity> CardGet() => ArraySupport.GET<Entities.AccountEntity>($"{MiceWebClient.REST_API_URL}/api/User/Account/Card");
			/// <summary>
			/// 내가 교환한 명함 하나를 삭제한다.
			/// </summary>
			public static Response<Entities.CheckResult> CardDelete_TargetAccountId(long targetAccountId) => MiceWebClient.DELETE<Entities.CheckResult>($"{MiceWebClient.REST_API_URL}/api/User/Account/Card/{targetAccountId}");
			/// <summary>
			/// 내가 교환한 명함 여러개를 삭제한다.
			/// </summary>
			public static Response<Entities.CheckResult> DeletePost(long? payload = null) => MiceWebClient.POST<Entities.CheckResult>($"{MiceWebClient.REST_API_URL}/api/User/Account/Card/Delete", payload.HasValue ? payload.ToString() : "");
			/// <summary>
			/// 명함 사진 등록 신 버전
			/// </summary>
			public static Response<Entities.FileDownloadResponseResponseFormat> RegisterCardPhotoNewPost(object? payload = null) => MiceWebClient.POST<Entities.FileDownloadResponseResponseFormat>($"{MiceWebClient.REST_API_URL}/api/User/Account/RegisterCardPhotoNew", payload?.ToString() ?? "");
			/// <summary>
			/// 주어진 SurveyNo 팝업을 다시 보지 않기 한다.
			/// </summary>
			public static Response<Entities.CheckResult> AgreePost_SurveyNo(long surveyNo) => MiceWebClient.POST<Entities.CheckResult>($"{MiceWebClient.REST_API_URL}/api/User/Account/Survey/Agree/{surveyNo}");
			/// <summary>
			/// 해당 행사의 티켓을 가지고 있는지 여부
			/// </summary>
			public static Response<Entities.CheckResult> TicketGet_EventId(long eventId) => MiceWebClient.GET<Entities.CheckResult>($"{MiceWebClient.REST_API_URL}/api/User/Account/Event/Ticket/{eventId}");
			/// <summary>
			/// LIAPP 를 위한 userKey를 발급한다.
			/// </summary>
			public static Response KeyGet() => MiceWebClient.GET($"{MiceWebClient.REST_API_URL}/api/User/Account/LIAPP/Key");
			/// <summary>
			/// userKey와 token으로 보안 앱 체크를 한다.
			/// </summary>
			public static Response<Entities.CheckResult> KeyPost(string? payload = null) => MiceWebClient.POST<Entities.CheckResult>($"{MiceWebClient.REST_API_URL}/api/User/Account/LIAPP/Key", !string.IsNullOrEmpty(payload) ? payload : "");
		}
    }
}
