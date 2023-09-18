# Com2Verse.Communication

- MediaSdk 기반 유니티 WebRTC 채널 관리 모듈입니다.

## [1.4.0] - 2023-03-06

### Changed

- RemoteUser를 기능에 따라 클래스를 분리
- LocalUser를 기능에 따라 클래스를 분리
- Channel-User-Track 간 강한 의존성 분리
- 트랙/모듈 공통 연결상태 관리 추상 클래스 추가
- MediaTrack/Audio-Video 모듈간 상호 의존성 제거
- Channel 에서 트랙 관리 클래스 분리
- Publish request/Subscribe 제어 컴포넌트 중복 호출 제거

## [1.3.0] - 2023-02-10

### Changed

- OfflineLayout 관련 코드 분리
- PublishRequest 인자로 IPublicationTarget을 받게 수정
- ISubscriptionObserver 인터페이스 콜백으로 트랙 종류를 전달
- Pub/Sub 판단 공통 로직을 ObservableHashSet 클래스로 분리
- ICommunication.cs 내부 클래스 파일 분리
- Communication (1.2.0 -> 1.3.0)

## [1.2.0] - 2023-02-08

### Changed

- Communication 패키지 API에서 MediaSdk 의존성 제거
- ICommunicationUser 인터페이스에서 유틸리티성 API 를 헬퍼 클래스로 추출

## [1.1.3] - 2023-02-03

### Changed

- ScreenShare 패키지 업데이트 (1.1.3 -> 1.2.0)

## [1.1.2] - 2023-01-30

### Changed

- MediaSdk 업데이트 (v0.7.7 -> v0.7.8)
- ScreenShare 패키지 업데이트 (1.1.1 -> 1.1.3)

## [1.1.1] - 2023-01-17

### Changed

- ScreenShare 패키지 업데이트 (1.1.0 -> 1.1.1)

## [1.1.0] - 2023-01-17

### Changed

- 패키지 구조 변경 (Assets/Deploy -> Packages/com.com2verse.communication)

## [1.0.1] - 2023-01-13

### Changed

- TableData 패키지 적용

## [1.0.0] - 2023-01-11

### Added

- 프로젝트 생성
