# 로컬 캐시
## [Unreleased]
- 이어받기
- 전역설정

## [1.0.1] - 2023-03-10
### Added
- 캐시 파일에 암/복호화 적용

## [1.0.0] - 2023-01-05
### Changed
- 패키지 설정 변경

## [0.0.7] - 2022-11-01
### Fixed
- 텍스쳐 로드시 생기는 부하를 개선합니다.
- 텍스쳐 로드 방식을 수정합니다. (UnityWebRequestTexture)

## [0.0.6] - 2022-10-27
### Fixed
- 다운로드 받은 텍스쳐 품질을 유지하기 위해 MipMap을 사용하지 않도록 처리합니다.

## [0.0.5] - 2022-10-19
### Fixed
- 텍스쳐 로드 방식을 변경합니다.
  (FreeImage 라이브러리 사용 -> Unity Texture2D.LoadImage)
## [0.0.4] - 2022-09-29
### Added
- 텍스쳐 로드 비동기 처리를 위해 FreeImage 라이브러리 적용 (현재는 Windows 환경에서만 동작)
### Fixed
- 동일 URL 중복 요청 확인 조건이 동일 URL + 저장할 파일 이름 중복 확인으로 변경됩니다.
## [0.0.3] - 2022-09-27
### Fixed
- 동일 URL 요청이 중복으로 오는 경우 하나의 다운로드만 활성화되고 다운로드 진행에 대한 콜백을 공유합니다.
- 임시파일 -> 캐시파일로 복사하는 처리 개선
- 캐시파일 로드 전 파일이 있는지 확인

## [0.0.2] - 2022-09-19
### Added
- API 구성
  - 비동기 로드
    - LoadBytesAsync
    - LoadTexture2DAsync
  - 캐시 관리
    - PurgeCache
    - PurgeTemp
    - DeleteAllTemp
    - SetCacheType
  - 디버그
    - PrintInfo
- 콜백을 제공하는 Request API (빌더 방식)
  - 캐시 로드 콜백
  - 스트리밍 다운로드 콜백
  - 다운로드 요청 콜백
- HttpClient 관리
- 다운로드 요청 풀링 및 동시 다운로드 제한 (기본값 30개)