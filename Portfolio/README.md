# Game Programming Portfolio

이 폴더는 프로젝트 전체 원본을 그대로 담는 저장소가 아니라, 면접/검토용으로 핵심 시스템만 빠르게 이해할 수 있도록 정리한 패키지입니다.

## AutoGen 멀티 에이전트 (`autogen_portfolio/`)

원고 규칙·경로 일관성 검토를 AutoGen(AgentChat)으로 돌리려면 `autogen_portfolio/README.md`를 참고해 가상환경에 의존성을 설치한 뒤 `portfolio_agents.py`를 실행합니다. `OPENAI_API_KEY`가 필요합니다.

## 요약 PDF (`SUNG_HYEON_KIM_Portfolio.pdf`)

`SUNG_HYEON_KIM_Portfolio.md`를 원고로 하여 PDF를 다시 만들 때는 이 폴더에서 다음을 실행합니다.

```bash
python build_portfolio_pdf.py
```

필요 패키지: `markdown`, `xhtml2pdf`, `Pillow` (`pip install -r requirements-portfolio-pdf.txt`). 스크립트가 웹 이미지를 받아 최대 너비 640px로 줄인 뒤 PDF에 넣어 파일 크기와 레이아웃을 맞춥니다.

## 구성

- `Dawnstar/README.md`
  - Unity 클라이언트 + C# 전용 서버 기반 2D MMO 프로젝트
  - 로그인/캐릭터/맵 전환/퀘스트/아이템/파티/상호작용 등 게임 콘텐츠 흐름 중심으로 정리
- `Dawnstar/Source/README.md`
  - `Dawnstar` 대표 코드 파일 안내
- `Pickpacker/README.md`
  - Unreal Engine 기반 협동 멀티플레이 프로젝트
  - 게임 진행, 상호작용/인벤토리, AI, 엔딩 분기, 세션 처리 중심으로 정리
- `Pickpacker/Source/README.md`
  - `Pickpacker` 대표 코드 파일 안내
- `GameTycoon/README.md`
  - Java Swing 기반 게임회사 타이쿤 (`DevGameTycoon`) — 시간 루프, 직원 이동, 개발·출시·매출, UI 갱신, 세이브
- `GameTycoon/Source/README.md`
  - `GameTycoon` 대표 코드 파일 안내

## 빠르게 보고 싶은 포인트

### Dawnstar

- `Dawnstar/Source/01_ClientSession_LoginLobby.cs`
  - 로그인 이후 캐릭터 생성, 복원, 최초 진입까지 이어지는 플레이어 라이프사이클
- `Dawnstar/Source/02_GameRoom_MapRespawnSequence.cs`
  - 포탈 이동, 리스폰, 필드/던전 룸 분기, 파티 던전 진입
- `Dawnstar/Source/03_Quest_Realization_Progression.cs`
  - 퀘스트 진행과 스탯 성장 시스템
- `Dawnstar/Source/04_ItemEconomy_CraftingEnhancement.cs`
  - 상점, 소비, 제작, 강화, 인챈트가 연결된 아이템 경제 루프
- `Dawnstar/Source/05_WorldInteraction_Persistence.cs`
  - 문/트리거/상자 같은 월드 오브젝트와 상태 저장
- `Dawnstar/Source/06_PartyMatching_PartyFlow.cs`
  - 파티 결성, 파티 정보 동기화, 4인 매칭 처리
- `Dawnstar/Source/07_Sync_Interest_TaskQueue.cs`
  - TaskQueue·GameRoom·InterestManagement **전체 발췌**, 확장 지점은 InterestManagement 주석

### Pickpacker

- `Pickpacker/Source/01_GameFlow_PickpackerGameMode.cpp`
  - 웨이브 주문, 제출 판정, 크레딧 반영
- `Pickpacker/Source/02_Interaction_Inventory.cpp`
  - 상호작용 타게팅, 아이템 수집, 인벤토리 저장
- `Pickpacker/Source/03_AI_Architecture.cpp`
  - AI 공통 레이어, 시야 설정, 순찰 포인트 생성
- `Pickpacker/Source/04_Escape_EndingFlow.cpp`
  - 월드 플래그 기반 엔딩 분기
- `Pickpacker/Source/05_MultiplayerSessions.cpp`
  - 세션 생성, 검색, 참가

### GameTycoon (DevGameTycoon)

- `GameTycoon/Source/01_GameBoard_GameLoop.java`
  - 메인 스레드 시간 흐름, 메뉴·팝업 시 정지, 맵 스크롤·그리드 배치
- `GameTycoon/Source/02_Developer_Movement.java`
  - 직원 이동·스프라이트 애니메이션
- `GameTycoon/Source/03_DevGame.java`
  - 개발 진행도 계산
- `GameTycoon/Source/04_Game.java`
  - 출시 가격·판매 감쇠
- `GameTycoon/Source/05_Company.java`
  - 개발 시작·매출·경영비·세이브/로드
- `GameTycoon/Source/06_GameUpdater_Events.java`
  - 상태 표시·진행 바·개발 이벤트

## 프로젝트별 이동

- [Dawnstar 개요](./Dawnstar/README.md)
- [Dawnstar 소스 하이라이트](./Dawnstar/Source/README.md)
- [Pickpacker 개요](./Pickpacker/README.md)
- [Pickpacker 소스 하이라이트](./Pickpacker/Source/README.md)
- [GameTycoon 개요](./GameTycoon/README.md)
- [GameTycoon 소스 하이라이트](./GameTycoon/Source/README.md)
