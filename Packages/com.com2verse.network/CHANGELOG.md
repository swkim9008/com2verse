# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.1] - 2023-05-10
### Added
- Send 추가 ( notification type)

## [1.1.0] - 2023-01-17
### Changed
- 패키지 폴더 구조 변경 (Assets/Deploy -> Packages/xxx)

## [1.0.1] - 2023-01-13
### Added
- SocketBase, ClientSocket
  - 소켓 방식 전환으로 인한 재추가
- PacketBufferController
  - 소켓 버퍼 재사용 및 이어받기 기능을 담당하는 클래스
### Removed
- Dealer
  - 소켓 방식 전환으로 인해 사용하지 않게 되어 삭제
- NetMQ
  - NetMQ 패키지 참조 삭제

## [1.0.0] - 2023-01-11
### Added
- Logger
  - UberLogger 패키지로 참조
- Protocols
  - Dll 포함한 패키지로 참조
- Assembly References 추가
### Removed
- SocketBase, ClientSocket
  - 현재 쓰이지 않고 있어 삭제

## [0.0.1] - 2023-01-05
### Added
- IMessageProcessor
  - Message 파싱을 위한 인터페이스
- ClientSocket
  - C# 소켓을 컴투버스 특화로 래핑한 소켓 클래스(메세지 핸들러 포함)
- NetworkManager
  - 소켓과 메시지에 대한 관리를 담은 싱글톤 클래스 
- SocketBase
  - C# 소켓을 단순 래핑한 ClientSocket의 베이스 클래스
- Dealer
  - NetMQ소켓을 래핑한 클래스
