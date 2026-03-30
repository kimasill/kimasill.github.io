# DawnStar Source Highlights

각 파일은 `Server/Server/` 쪽 구현과 대응하는 대표 코드입니다.

- `01_ClientSession_LoginLobby.cs`
  - 로그인, 캐릭터 생성, 최초 접속 처리, 퀘스트/인벤토리 복원, 게임 룸 입장 흐름
- `02_GameRoom_MapRespawnSequence.cs`
  - 리스폰, 포탈 이동, 맵 타입 분기, 던전 입장 처리
- `03_Quest_Realization_Progression.cs`
  - 퀘스트 시작/완료, 진행도 저장, 성장 포인트와 특수 스탯 적용
- `04_ItemEconomy_CraftingEnhancement.cs`
  - 상점, 소비 아이템, 제작, 강화, 인챈트 흐름
- `05_WorldInteraction_Persistence.cs`
  - 문/트리거 상호작용과 상자/상호작용 상태 복원
- `06_PartyMatching_PartyFlow.cs`
  - 파티 생성, 참가/탈퇴, 4인 매칭, 파티 단위 던전 이동
- `07_Sync_Interest_TaskQueue.cs`
  - `TaskQueue`·`GameRoom`(Broadcast·GetAdjacentZone 등)·`InterestManagement` 전체 발췌. **확장·조정 지점**은 `InterestManagement.Update` 내 주석(장비 지연 동기화, 시야 갱신 주기). PDF 원고에는 확장 부분만 인용 가능

원본은 아래 경로에서 확인할 수 있습니다.

- `Server/Server/`
- `Server/Server/Game/`
- `Server/Server/Session/`
- `Server/Server/Game/Job/TaskQueue.cs`
- `Server/Server/Game/Room/InterestManagement.cs`
