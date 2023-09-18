# Organization
- 조직도 및 팀워크와 관련한 코드를 포함한 패키지 입니다.
- 검색 유틸리티 Trie
  - 일반 검색, 초성 검색, 중간부터 검색을 지원합니다.
- 계층트리 [HierarchyTree](https://jira.com2us.com/wiki/pages/viewpage.action?pageId=294809806)
  - 노드단위로 계층 트리를 구성합니다.
  - 각 노드를 Fold/UnFold하는 기능
  - 노드 검색
  - 노드 순회 (Forward, BackWard, Ascent, Descent)
## [1.0.2] - 2023-01-11
### Added
- Com2Verse.Logger 패키지 추가
### Removed
- External UberLogger 제거 

## [1.0.1] - 2023-01-06
### Added
- Organization 코드 추가
- TeamWork 코드 추가

## [1.0.0] - 2022-12-22
### Added
- HierarchyTree 추가
- Trie 추가
- 테스트 코드 추가
### Fixed
- HierarchyTree의 BackwardEnumerator가 제대로 작동하지 않는 문제 수정