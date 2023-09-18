# Http Helper
## [Unreleased]
- 인증 테스트 필요 (기본, 토큰 기반)
## [1.0.5] - 2023-05-18
### Added
- 인증 만료시 이벤트 호출하도록 수정
- 요청에 대한 결과로 ResponseBase<T> 타입 리턴하도록 수정

## [1.0.4] - 2023-02-09
### Added
- 네트워크 요청 순서대로 처리 적용
- 요청 중 취소 처리
### Fixed
- 동시 다운로드 이슈 수정

## [1.0.3] - 2023-02-07
### Changed
- GET, POST, PUT, DELETE, MESSAGE 요청 구조 개선 (Non-Callback)
- Wrapper 클래스 구조로 변경 (GET, POST, PUT, DELETE, MESSAGE, Request, Settings, Auth, Debug)
- Sample 추가 (동시 다운로드 테스트)

## [1.0.2] - 2023-02-07
### Added
- 각 요청에 대한 관리 적용
  - 최대 연결 수 제한
  - 요청 가능할 때 대기중 요청 연결
- 콜백 지원 추가
  - 상태별 콜백
    - 다운로드 중
    - 실패
    - 완료
    - 마무리
- 다운로드 핸들러 추가
  - 다운로드 요청 및 취소
  - 
- 테스트 추가 (콜백 적용)

## [1.0.1] - 2023-02-02
### Added
- 웹 요청 빌더 추가 (HttpRequestBuilder)
- HttpRequestMessage 요청 타입 추가
- 파파고 API 테스트 추가

## [1.0.0] - 2023-02-02
### Added
- REST API 요청 추가 (GET, POST, PUT, DELETE)
- 인증 추가 (Basic, Token)
- 테스트 추가