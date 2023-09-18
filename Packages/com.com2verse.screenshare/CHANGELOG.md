# Com2verse.ScreenShare

- uWindowCapture 기반 screencapture 관리 모듈입니다.

## [1.3.0] - 2023-04-24

### Changed

- ScreenList 변경 콜백 제거
- ScreenList 추가/제거 콜백 추가
- ScreenList 프로퍼티 추가

## [1.2.1] - 2023-02-22

### Changed

- ScreenCapture 패키지 v1.1.1 -> v1.1.2
  > - Dll Initalize/Finalize 반복시 Crash 문제 수정 
  > - 화면 공유중 해당 Window 사이즈 조정시 간헐적 Crash 문제 수정

## [1.2.0] - 2023-02-03

### Added

- 썸네일 로딩 표시 추가
- 썸네일 최소화 화면 표시 추가
- 썸네일 타이틀/아이콘 갱신 기능 추가
- 공유중 화면 최소화시 중지 기능 추가
- 공유 시작시 최소화된 화면 크기 복구 기능 추가
- 화면 캡쳐 설정 변경 치트키 추가
- SDK 에러 발생시 로그 기능 추가

### Changed

- 공유 권한 박탈 검자 로직을 패키지 내부로 이동
- 모호한 공개 API 프로퍼티/메서드명 수정
- ScreenCaptureController 클래스 partial 분리

### Fixed

- 공유 도중 창 전환 속도 개선
- 화면 공유 시작 조건 검사 강화
- 썸네일 텍스쳐 색 공간 오류 수정 (UNorm -> sRGB)
- InvokePropertyValueChanged 에 [CanBeNull] 어노테이션 추가
- ScreenInfoViewModel 최적화
- GC Alloc 최적화
- 화면공유 기능을 사용중이지 않을때도 메모리를 점유하는 문제 수정
- Failed to present D3D11 swapchain due to device removed. 에러 수정

## [1.1.3] - 2023-01-30

### Changed

- ScreenCapture 패키지 v1.0.20 -> v1.1.0

## [1.1.2] - 2023-01-17

### Changed

- 의존 패키지 버전 명시

## [1.1.1] - 2023-01-17

### Changed

- 의존 DLL 샘플로 이동

## [1.1.0] - 2023-01-17

### Changed

- 패키지 구조 변경 (Assets/Deploy -> Packages/xxx)

## [1.0.0] - 2023-01-11

### Added

- 프로젝트 생성
