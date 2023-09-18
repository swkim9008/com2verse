# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.1] - 2023-02-03

### Changed

- 네트워크 모듈과의 종속성 추가

## [1.1.0] - 2023-01-12

### Changed

- 패키지 구조 변경

## [1.0.0] - 2023-01-12

### Changed

- 팀 공유 설정 적용
- 로거 변경

## [0.0.1] - 2023-01-03
### Added
- BaseMapObject
  - 모든 오브젝트의 베이스
- IObjectCreator
  - 오브젝트 생성 로직을 담는 클래스의 인터페이스
- MapController
  - 오브젝트들을 관리하는 싱글턴 클래스
- ServerTime
  - 오브젝트 동기화의 기준이 되는 시간을 관리
