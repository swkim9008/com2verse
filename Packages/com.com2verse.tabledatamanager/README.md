# TableDataManager
컴투버스 테이블 데이터를 관리하는 패키지입니다.

# 사용방법
1. TableData 프로젝트를 이용해 CSV를 변환, bytes 파일을 생성한다. (Assets/Deploy/Runtime/Data)
2. 변환된 bytes파일 중 필요한 파일만을 복사하여 컴투버스 패키지 내 특정 폴더에 넣어둔다. (Assets/External/LocalTable)
3. 이제 플레이를 하면 기존 테이블보다 넣어둔 파일을 우선하여 읽게 된다.

- 필수 패키지 Dependency
    - MetaverseCore : https://meta-bitbucket.com2us.com/scm/c2verse/com.com2verse.core.git
    - UniTask : https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
