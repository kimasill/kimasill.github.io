# DawnStar

## 프로젝트 한 줄 소개

`DawnStar`는 Unity 클라이언트와 C# 전용 서버로 구성한 2D MMO 프로젝트입니다. 이 포트폴리오에서는 `캐릭터 진입 흐름`, `퀘스트/성장`, `아이템 경제`, `월드 상호작용`, `파티 매칭과 던전 진입`, `동기화·부하(TaskQueue·Interest Management)` 같은 게임 콘텐츠·서버 구조를 중심으로 정리했습니다.

## 제가 보여주고 싶은 역량

- 로그인 이후 로비/캐릭터/월드 진입 흐름 설계
- 퀘스트와 스탯 성장 시스템 구현
- 상점, 소비, 제작, 강화, 인챈트까지 이어지는 아이템 루프 설계
- 문/트리거/상자 같은 월드 상호작용과 영속 상태 저장
- 파티 결성, 파티 매칭, 던전 입장 흐름 연결
- 게임 세션(룸) 단위 작업 큐와 플레이어 주변 가시 범위 동기화

## 핵심 구현 1. 로그인, 캐릭터 생성, 월드 진입 플로우

`ClientSession_preGame`에는 단순 로그인 승인만 있는 것이 아니라, 토큰 검증 이후 로비 캐릭터 목록 구성, 캐릭터 생성, 최초 접속 처리, 퀘스트/인벤토리 복원, 현재 맵 상태 반영, 그리고 실제 게임 룸 입장까지 한 흐름으로 이어집니다.

단순한 로그인 성공 응답에서 그치지 않고, `계정 인증 -> 보유 캐릭터 조회 -> 월드 진입`까지의 전체 루프를 유기적으로 구성했습니다. 첫 접속 유저와 기존 플레이어를 분기하여 시작 맵과 상태를 다르게 세팅하며, 퀘스트·인벤토리·맵 정보를 DB에서 읽어들여 메모리 속 플레이어 객체로 온전히 복원해냅니다.

대표 코드: `Dawnstar/Source/01_ClientSession_LoginLobby.cs`

## 핵심 구현 2. 맵 전환, 리스폰, 던전 입장 흐름

`GameRoom_Sequence`에서는 포탈 기반 맵 이동, 가장 가까운 포탈을 이용한 스팟 리스폰, 필드/던전 타입에 따른 룸 생성 분기, 그리고 파티 단위 던전 진입을 처리합니다. 단순 좌표 변경이 아니라 `플레이어 상태 변경 -> 맵 정보 저장 -> 룸 이동 -> 맵별 상호작용/상자 상태 갱신`까지 연결됩니다.

필드 맵과 던전 인스턴스를 서로 다른 생성 규칙으로 분리 적용했습니다. 리스폰 시 포탈 데이터 기반의 최단 거리 검색을 통해 보정 위치를 산출하며, 모든 맵 전환이 단순한 클라이언트 렌더링 연출에 머물지 않고 서버 핵심 로직 상의 실제 룸 이동 및 DB 업데이트로 완벽히 직결되도록 설계했습니다.

대표 코드: `Dawnstar/Source/02_GameRoom_MapRespawnSequence.cs`

## 핵심 구현 3. 퀘스트와 성장(Realization) 시스템

`GameRoom_Quest`에서는 퀘스트 시작, 진행도 갱신, 완료 처리뿐 아니라 스탯 포인트 소비와 `Realization` 성장도 함께 관리합니다. 특히 성장 단계 수치를 단순 누적하지 않고, 조건 충족 시 특수 스탯을 적용하는 방식으로 설계해 플레이 선택이 누적 성장에 반영되도록 만들었습니다.

퀘스트의 시작부터 진행 단계, 완료까지의 흐름이 메모리 상의 트래킹뿐만 아니라 DB 기록 및 클라이언트 패킷 전송과 맞물려 있습니다. 진행 중인 퀘스트와 수행 완료된 퀘스트를 엄격히 구분하여 중복 수령 등 어뷰징을 차단하고, 획득한 포인트 투자가 즉시 스탯 로직 변화로 매핑되도록 처리했습니다.

대표 코드: `Dawnstar/Source/03_Quest_Realization_Progression.cs`

## 핵심 구현 4. 상점, 소비, 제작, 강화, 인챈트 아이템 루프

`GameRoom_Item`은 단순 인벤토리 추가/삭제를 넘어서, 상점 구매/판매, 소비 아이템 사용, 제작 재료 차감, 강화 재료 검사, 인챈트 옵션 누적 등 하나의 아이템 경제 루프를 담당합니다. 아이템 사용이 곧바로 체력 회복이나 스킬 발동으로 연결되고, 강화/인챈트는 재료 소비와 DB 저장을 함께 처리합니다.

단편적인 인벤토리 조작을 넘어 상점 매매, 아이템 제작, 장비 강화 과정이 하나의 거대한 경제 시스템 하에서 유기적으로 맞물리게 통합했습니다. 파편화된 로직 대신 재료와 슬롯 공간 검증, 옵션 중복 판정 등의 예외 사항들을 꼼꼼히 거치며, 아이템 소모 및 상태 변화의 최종 결과물을 DB 트랜잭션과 패킷으로 일괄 보장합니다.

대표 코드: `Dawnstar/Source/04_ItemEconomy_CraftingEnhancement.cs`

## 핵심 구현 5. 월드 상호작용과 상태 영속화

`Interaction`, `Door`, `Trigger`는 맵 데이터 기반 상호작용 오브젝트 레이어입니다. 문은 키 아이템이나 트리거 조건으로 열리고, 트리거는 다른 상호작용 오브젝트의 상태를 바꿉니다. 또한 `UpdateMapChests`, `UpdateMapInteractions`를 통해 열어 둔 상자와 완료된 상호작용 상태를 플레이어 맵 정보에 다시 반영합니다.

스크립트에 이벤트 로직을 하드코딩하는 대신 데이터 테이블을 바탕으로 문과 트리거 등의 오브젝트를 스폰하고 동작 타입을 다형성을 이용해 분기했습니다. 요구 키 아이템 소모 및 연계 트리거 상태 등 조건 만족 여부를 서버 규칙 하에 통제하며, 상호작용 성공 여부가 휘발성 연출에 그치지 않고 지속적인 맵 상태로서 영속성을 가지도록 짰습니다.

대표 코드: `Dawnstar/Source/05_WorldInteraction_Persistence.cs`

## 핵심 구현 6. 파티 결성, 파티 매칭, 던전 진입

`ClientSession_party`, `Party`, `PartyMatchingSystem`은 MMO 협동 콘텐츠 흐름을 보여주는 영역입니다. 플레이어가 직접 파티를 만들거나, 매칭 대기열에 들어가 4인 파티를 구성한 뒤, 같은 파티 단위로 던전에 이동하도록 구현했습니다.

논리적 세션 단위의 파티 소속 관리 구조와, 물리적 월드 위 플레이어 액터와의 상태를 서로 유기적으로 묶어 냈습니다. 조인과 이탈 등 멤버 변동 이벤트가 발생할 때마다 파티 전체 정보 패킷을 브로드캐스팅해 클라이언트들의 동기화를 맞추고, 자동 매칭이 성사되면 파티원 전원을 동시에 다루는 통합 이동 시퀀스를 구현했습니다.

대표 코드: `Dawnstar/Source/06_PartyMatching_PartyFlow.cs`

## 핵심 구현 7. 동기화·부하: TaskQueue·Interest Management

순서·가시 범위를 맞추는 **기본 골격**(`TaskQueue`·`Zone`·Interest diff·`Broadcast` 등)은 일반적인 MMO 서버 설계 위에 두고, 본 프로젝트에서는 **장비 동기화 지연 큐**·**시야 갱신 주기**처럼 실제 부하와 플레이 품질에 맞춘 타이밍을 추가했습니다. 포트폴리오 패키지 `07` 파일에는 **TaskQueue·GameRoom 발췌·InterestManagement 전체**를 두었고, **확장·조정 지점**은 `InterestManagement.Update` 주석으로 표시합니다. PDF 원고(`SUNG_HYEON_KIM_Portfolio.md`)에는 필요 시 확장 부분만 인용해도 됩니다.

공용 기반의 MMO 서버를 구동하는 데서 멈추지 않고, 체감 랙과 틱의 퍼포먼스 한계를 분석해 **본 게임의 규칙 강도에 맞게 확장시킨 동기화 타이밍 최적화** 노력이 드러나 있습니다. 틱 병목에 치명적인 전체 루프를 손보는 대신 장비 패킷의 전송 지연과 시각적 갱신 주기를 제어해, 실제 플레이 감각과 서버 생존력을 맞교환한 디테일을 코드로 입증합니다.

대표 코드: `Dawnstar/Source/07_Sync_Interest_TaskQueue.cs`

## 어떤 역량이 가장 잘 드러나는가

이 프로젝트에서 가장 강하게 드러나는 역량은 `MMO 콘텐츠를 서버 흐름 안에 녹여내는 능력`입니다. 단순한 서버 인프라 구현보다, 로그인 이후의 캐릭터 라이프사이클, 퀘스트와 성장, 아이템 소비와 강화, 파티 결성, 던전 진입, 월드 상호작용 상태, 그리고 룸 단위 작업 순서·주변 위주 동기화까지 실제 플레이 루프가 이어지도록 만들었다는 점이 핵심입니다.

## 함께 보면 좋은 원본 파일

- `Server/Server/Session/ClientSession_preGame.cs`
- `Server/Server/Session/ClientSession_party.cs`
- `Server/Server/Game/Room/GameRoom_Sequence.cs`
- `Server/Server/Game/Room/GameRoom_Quest.cs`
- `Server/Server/Game/Room/GameRoom_Item.cs`
- `Server/Server/Game/Room/GameRoom.cs`
- `Server/Server/Game/Room/InterestManagement.cs`
- `Server/Server/Game/Job/TaskQueue.cs`
- `Server/Server/Game/Interactions/Interaction.cs`
- `Server/Server/Game/Interactions/Door.cs`
- `Server/Server/Game/Interactions/Trigger.cs`
- `Server/Server/Game/Contents/Party.cs`
- `Server/Server/Game/Contents/PartyMatchingSystem.cs`
- `Server/Server/DB/DbTransaction.cs`
