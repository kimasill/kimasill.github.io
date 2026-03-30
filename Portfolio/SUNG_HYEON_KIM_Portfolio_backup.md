# SUNG HYEON KIM PORTFOLIO

## Intro

### Identity

저는 서버와 클라이언트 전반의 구조적 이해와 경험을 바탕으로, 기획 의도에 맞는 플레이 경험을 구현하고 개선하는 게임 프로그래머입니다.

**성과 요약**

- **Dawnstar** (2D MMORPG): 로그인~게임 서버·DB·퀘스트·전투까지 1인으로 클라–서버 전 구간 구현.
- **Pickpacker** (UE5): 협동 멀티플레이 시스템 개발, 성능 최적화 - 프로파일링 기준 드로우 콜 **약 11,061 → 약 3,200**(약 **71%** 감소), FPS **약 25 → 약 100**.
- **DevCom Tycoon**: Java Swing 기반 멀티쓰레드 활용, 게임 루프·UI 동기화.

### Keywords

Unity · C# / .NET · MMORPG 서버 · UE5 · C++ · 멀티플레이 시스템 · AI 구조 · PCG · 성능 최적화

Contacts · [kibbel1998@gmail.com](mailto:kibbel1998@gmail.com) · [GitHub](https://github.com/kimasill) · [LinkedIn](https://www.linkedin.com/in/sung-hyeon-kim-a64b2922a/)

---

본 문서는 핵심 문제 해결 사례와 아키텍처 위주로 요약했습니다. 영상·진행 기록·상세 페이지는 [웹 포트폴리오](https://kimasill.github.io/) · [작업 목록](https://kimasill.github.io/#work)에서, 근거 코드는 아래에 적은 `Portfolio/` 패키지 경로에서 확인할 수 있습니다.

---

## Project Overview

### 1. Summary

플레이 경험을 중심에 두고 시스템·네트워크·툴을  설계·구현하는 것을 목표로 한 프로젝트 모음입니다.

MMORPG 서버 주도 콘텐츠, UE5 협동 멀티플레이 시스템, 자체 멀티 쓰레드 타이쿤·PCG·ML·창업 과제 등 범위는 다르지만, 공통으로 구조와 플레이 루프를 맞추는 데 집중했습니다. 기술적 문제와 해결 과정도 담았습니다.

### 2. Project List

| 프로젝트 | 요약 |
| --- | --- |
| DAWNSTAR | 2D MMORPG — 인증(로그인) 서버·게임 서버·DB·콘텐츠(성장·전투·파티 등) 클라이언트–서버 전 구간 구현 |
| PICKPACKER | UE5 협동 멀티플레이 — Online Subsystem 기반 멀티플레이 세션(생성·검색·참가), 게임 진행·AI |
| DevCom Tycoon | Java Swing 타이쿤 — 시뮬레이션 루프·UI 스레드 동기화·경제 로직 |
| 기타 | PCG 던전 플러그인, 차량 분류 모델(YOLO 기반 딥러닝), 창업 캠프, Tab Game 등 시스템 설계·프로토타이핑·실험 |

### 3. Skills

| 구분 | 내용 |
| --- | --- |
| 클라이언트·게임플레이 | 입력 처리, 상호작용 로직, AI 및 전투 시스템 설계, UI 구현 |
| 네트워크·동기화 | C# 전용 게임 서버(MMO): Protobuf·Interest Management·TaskQueue · UE5: 데디케이티드 서버 기준 RPC·Replication·서버 권한(Authority) |
| 시스템 설계·구조화 | 데이터 기반 확장, 맵·세션 인스턴스·경제·퀘스트 루프, 멀티플레이 세션(검색·참가)·엔딩 분기 |
| 최적화·파이프라인 | UE5 프로파일링 기반 성능 분석, 드로우 콜 최적화, PCG 활용 및 C++ 기반 게임플레이 시스템 구현 |

---

# 핵심 프로젝트

아래 세 작업은 `Portfolio/`에 대표 소스를 두었습니다. 문서는 Overview → Role → Core Implementation → Problem Solving → Result 순으로 읽을 수 있습니다. 코드는 대표 분기만 발췌했고, 전문은 각 패키지 경로에서 확인할 수 있습니다.

---

## 1. Dawnstar

<a href="https://kimasill.github.io/projects/dawnstar.html" title="Dawnstar 프로젝트 페이지" target="_blank" rel="noopener noreferrer"><img src="https://kimasill.github.io/images/dawnstar/DawnstarTitleImg.png" alt="Dawnstar 타이틀" width="640" /></a>

링크 · [프로젝트 페이지](https://kimasill.github.io/projects/dawnstar.html) · [상세 개발 과정 (dawnstar-process)](https://kimasill.github.io/projects/dawnstar-process.html) · 코드 패키지: `Dawnstar/README.md`, `Dawnstar/Source/`

### Overview

| 항목 | 내용 |
| --- | --- |
| 장르 | 2D 다크 판타지 MMORPG |
| 엔진·스택 | Unity · C# · .NET · SQL Server |
| 기간·규모 | 1인 · 2024.08 ~ 2025.02 |

### Role

- 클라이언트·서버·DB·콘텐츠(퀘스트·전투·경제·파티·월드 디자인) 설계·구현.

#### Visual: Network & Game Server

<img src="https://kimasill.github.io/images/dawnstar/%EB%84%A4%ED%8A%B8%EC%9B%8C%ED%81%AC%20%EC%95%84%ED%82%A4%ED%85%8D%EC%B2%98.png" alt="Dawnstar 네트워크 아키텍처" width="640" />

<img src="https://kimasill.github.io/images/dawnstar/%EA%B2%8C%EC%9E%84%EC%84%9C%EB%B2%84%EC%95%84%ED%82%A4%ED%85%8D%EC%B2%98.png" alt="Dawnstar 게임 서버 아키텍처" width="640" />

### Core Implementation

#### 1. Login & Lobby – 로그인 후 상태 한 축으로 묶기

로그인 직후 로비·캐릭터 목록과 `ServerState`가 어긋나면, 입장·복원·퀘스트·인벤 복구까지 이어지는 흐름이 중간에 끊긴다.

인벤토리 초기화가 되지 않거나나 맵·세션 상태가 꼬이면 유저 입장에서는 진행 불가에 가깝다. 원인도 클라·서버·DB 중 어디인지 파악하기 어렵게 디버깅 지점이 흩어진다.

토큰으로 계정을 조회한 뒤 `S_Login`에 로비 캐릭터 정보를 한 번에 실어 보내고 세션을 `Lobby`로 옮겼다. 응답을 여러 번 쪼개지 않고, 패킷 한 번에 스냅샷과 상태 전이를 묶는 쪽을 택했다.

로그인 직후 꼬이는 문제는 결국 `loginResponse` 에 담아 보냄으로써 해결되었다.

`Dawnstar/Source/01_ClientSession_LoginLobby.cs`

```csharp
// 토큰으로 계정 조회 후 로비 캐릭터 목록을 패킷에 담고 세션 상태를 Lobby로 전이
S_Login loginResponse = new S_Login { LoginOk = 1 };
foreach (PlayerDb player in findAccount.Players)
{
    LobbyPlayerInfo summary = ToLobbyPlayerInfo(player);
    LobbyPlayers.Add(summary);
    loginResponse.Players.Add(summary);
}
Send(loginResponse);
ServerState = PlayerServerState.ServerStateLobby;
```

#### 2. Map & Dungeon – 맵 전환 시 서버·클라 판정 일치

맵 ID만 갱신하고 HP·좌표·Idle을 서버와 맞추지 않으면 전투·파티 동기화가 한 번에 무너진다.

처음에는 맵 전환만 빠르게 붙였다가 불일치가 잦아서, `LeaveGame`으로 정리한 뒤 `EnterGame`으로 같은 룸에 다시 넣는 순서를 고정했다. 이후에는 `Leave -> Enter` 시퀀스 하나만 추적해도 재현 구간이 금방 드러났다.

`Dawnstar/Source/02_GameRoom_MapRespawnSequence.cs`

```csharp
// LeaveGame으로 정리한 뒤 Spot이면 가까운 포탈로 보정하고, 체력·Idle을 맞춘 다음 동일 룸에 재입장
LeaveGame(player.Id);
if (respawnType == RespawnType.Spot)
    TryMoveToNearestPortal(player);
RestorePlayerStateAfterRespawn(player);
EnterGame(player, randPos: false);
```

#### 3. Quest & Progression – DB·메모리 퀘스트 단일 소스

DB에 있는 퀘스트 행과 메모리 진행 정보가 어긋나면 완료·보상·진행도 판정이 흔들린다. 이미 받은 보상이 사라지거나 조건을 채워도 완료가 안 될수있기 때문이다.

클라이언트 쪽 카운터만 올리는 식도 잠깐 검토했으나, 소유자(`OwnerDbId`)·템플릿 기준으로 `QuestDb`를 읽고 메모리 Progress를 그에 맞추는 편이 분기가 늘어날 때 추적이 쉬웠다.

`Dawnstar/Source/03_Quest_Realization_Progression.cs`

```csharp
// 소유자 기준으로 퀘스트 DB 행 조회 후 Progress·메모리 Quest와 동기화
using (AppDbContext db = new AppDbContext())
{
    List<QuestDb> quests = db.Quests
        .Where(q => q.OwnerDbId == player.PlayerDbId && (questId == 0 || q.TemplateId == questId))
        .OrderByDescending(q => q.QuestDbId)
        .ToList();
    QuestDb questDb = quests.FirstOrDefault();
}
```

#### 4. Item Economy – 경제 경로 트랜잭션 통합
<img src="https://kimasill.github.io/images/dawnstar/재련.PNG" alt="Dawnstar 네트워크 아키텍처" width="640" />

골드·인벤·DB 갱신이 함수마다 분산되지 않게 하기위해서, 비용 검증 뒤 인벤과 DB 반영을 `DbTransaction` 으로 묶어, 상점·강화·제작 어디서든 성공/실패가 같은 경로에 남게 했다.

`Dawnstar/Source/04_ItemEconomy_CraftingEnhancement.cs`

```csharp
// 골드·슬롯 검증 후 인벤·DB 보상은 DbTransaction 경로로 통일
player.Gold -= (int)totalCost;
DbTransaction.RewardPlayer(player, itemData, count, this);
```

#### 5. World Interaction – 타입별 상호작용 스폰

`InteractionType`별로 `Door`, `Trigger` 등 구체 타입으로 분기해 인스턴스를 만든다. 직렬화 데이터를 단일 클래스에 우겨 넣으면 스키마가 바뀔 때마다 서버가 같이 깨져서, 팩토리 패턴에 가깝게 구현했다.

`Dawnstar/Source/05_WorldInteraction_Persistence.cs`

```csharp
// InteractionData → Door / Trigger
switch (data.interactionType)
{
    case InteractionType.Door:
        interaction = new Door((DoorData)data);
        break;
    case InteractionType.Trigger:
        interaction = new Trigger((TriggerData)data);
        break;
    default:
        return null;
}
```

#### 6. Party Matching – 매칭·맵 입장 한 번에 묶기
<img src="https://kimasill.github.io/images/dawnstar/파티.PNG" alt="Dawnstar 네트워크 아키텍처" width="640" />

대기열만 쌓이고 파티 생성·맵 이동이 없으면 유저는 “매칭 중” UI에 갇히고 던전은 시작되지 않는다.

맵별/채널 별로 4명이 차면 파티를 만들고 `JoinParty` 뒤 `EnterMap`까지 한 틱에서 처리했다. “매칭 완료”와 “같은 맵 배치”를 나누면 중간에 인원이 흩어지는 경우가 있어, 한 번에 묶었다.

결국 유저 기준으로는 “매칭됐다”보다 “같이 들어갔다”가 보여야 했다. 4명이 찼을 때 바로 같은 맵으로 넘기는 방식이 그 요구에 가장 가까웠다.

`Dawnstar/Source/06_PartyMatching_PartyFlow.cs`

```csharp
// 맵별 대기 4명 충족 시 파티 생성·Join 후 EnterMap
if (!_waitingLists.TryGetValue(mapId, out var queue) || queue.Count < 4)
    return;
List<ClientSession> matched = queue.GetRange(0, 4);
queue.RemoveRange(0, 4);
var newParty = new Party(PartySystem.Instance.CreateParty().PartyId);
foreach (var session in matched)
    session.JoinParty(newParty);
EnterMap(newParty, mapId);
```

#### 7. Sync & Interest – 시야·장비 동기화 부하 분산

시야에 플레이어가 새로 잡힐 때마다 장비 풀 동기화와 시야 갱신을 즉시 전부 밀어 넣으면, 맵이 붐빌 때 틱당 작업이 폭증하고 전 유저의 체감 동기화가 함께 나빠진다.

이벤트마다 스로틀을 새로 박는 대신, 기존 `Room`의 지연 큐에 `EnqueueAfter`로 장비(예: **100ms**)·시야(예: **500ms**)를 넣었다. 기존 코드를 크게 건드리지 않아도 돼서 이쪽을 먼저 썼다.

가시 범위·순서 같은 MMO식 골격은 유지하고 부하를 완충하려는 의도로 작업했다. 로컬에서 더미 세션 10대를 같은 Zone에 몰아넣고 틱 시간을 찍어 봤을 때, 즉시 전송 대비 지연 큐 쪽이 틱 스파이크가 뚜렷하게 낮았고, 이 값(100ms / 500ms)을 기준으로 고정했다.

`Dawnstar/Source/07_Sync_Interest_TaskQueue.cs`

```csharp
// 시야에 다른 플레이어가 새로 잡히면 장비 목록 동기화를 지연 큐로 처리
Owner.Room.EnqueueAfter(100, Owner.Room.HandleEquippedItemList, Owner, player, true);
// …
// 스폰·디스폰 반영 후 다음 시야 갱신을 주기적으로 예약
Owner.Room.EnqueueAfter(500, Update);
```

#### Visual: Packet Pipeline

<img src="https://kimasill.github.io/images/dawnstar/%ED%8C%A8%ED%82%B7%20%EC%95%84%ED%82%A4%ED%85%8D%EC%B2%98.png" alt="Dawnstar 패킷 처리 아키텍처" width="640" />

*Protocol.proto·PacketGenerator·하이브리드 직렬화 파이프라인*

### Problem Solving

- 패킷 파이프라인 — 수동 직렬화와 `PacketGenerator`를 거쳐 최종 기준을 `Protocol.proto`에 두었다. 스키마가 바뀔 때 손볼 곳을 프로토콜 정의와 자동 생성 핸들러로 묶어 `Write/Read` 분기 누락 위험을 낮췄다.
- 동시성·동기화 — MMO 기본 틀은 유지하고, 부하 대응은 지연 큐와 시야 갱신 주기로 풀었다.
- 세계 일관성 — 맵만 바꾸는 수준에서 끝내지 않았다. 맵 상태, 상호작용, 파티가 서버·DB와 같은 순서로 맞물리게 시퀀스를 쪼개 두었고, 상태가 어긋났을 때도 어느 단계에서 틀어졌는지 바로 좁혀갈 수 있게 했다.

### Result

- MMORPG 의 핵심요소 전반을 구현 하였으며 지속적인 업데이트가 가능한 구조를 완성함.
- 대표 소스: `Dawnstar/Source/README.md` (`01_`~`07_*.cs`).

---

## 2. Pickpacker

<a href="https://kimasill.github.io/projects/pickpacker.html" title="Pickpacker 프로젝트 페이지" target="_blank" rel="noopener noreferrer"><img src="https://kimasill.github.io/images/Pickpacker/PickPackerCover.png" alt="Pickpacker 타이틀" width="640" /></a>

링크 · [프로젝트 페이지](https://kimasill.github.io/projects/pickpacker.html) · [진행·구조 (pickpacker-process)](https://kimasill.github.io/projects/pickpacker-process.html) · 코드 패키지: `Pickpacker/README.md`, `Pickpacker/Source/`

### Overview

| 항목 | 내용 |
| --- | --- |
| 장르 | 지하·열차 배경 협동 멀티플레이(물류·주문·탈출) |
| 엔진·스택 | Unreal Engine 5 · C++ · Online Subsystem |
| 기간·규모 | 1인 · 개발 중 · PC / Steam 목표 |

### Role

- 게임 플레이 루프·상호작용·시네마틱·인벤토리·AI·멀티플레이(Online Subsystem 세션).
- 주문·레시피·배치 규칙을 데이터 중심으로 정리, 콘텐츠 확장 시 코드 수정 최소화.

### Core Implementation

#### 1. GameFlow – 게임 진행 흐름에서 스트리밍 먼저 고정

에디터와 달리 패키지 빌드에서 스트리밍 레벨이 늦게 붙으면 매치 시작 시점에 액터 참조가 비어 크래시 등 각종 문제가 발생했다. 서버 권한(`HasAuthority`)에서 `SetShouldBeLoaded`·`SetShouldBeVisible` 후 `FlushLevelStreaming`으로 반영해, 레벨이 실제로 붙은 뒤에만 이후 로직을 시작하게 했다.

`Pickpacker/Source/01_GameFlow_PickpackerGameMode.cpp`

```cpp
if (LevelShortName.Equals(ToLoad.ToString(), ESearchCase::IgnoreCase) ||
    LevelName.Contains(ToLoad.ToString(), ESearchCase::IgnoreCase))
{
    StreamingLevel->SetShouldBeLoaded(true);
    StreamingLevel->SetShouldBeVisible(true);
    break;
}
UGameplayStatics::FlushLevelStreaming(World);
```

#### 2. GameMode & GameState – 주문·크레딧 서버 권한

태그·수량·포장 조건과 서버 `GameState`가 어긋나면 협동 주문이 클라 추측에만 의존한다.

크레딧·이력이 팀원마다 다르면 협동 목표와 게임오버 판정도 함께 흔들린다.

활성 주문 소모는 서버에서만 적용하고 완료 시 `HandleOrderSuccess` 후 `SyncOrdersToGameState`로 반영했다. 크레딧·이력도 서버 델타만 `CreditHistory`·`OnCreditsChanged`로 맞춘다.

멀티플레이 구현에서 각자 상태를 처리하고 동기화 하는것보다 서버에 요청하는 구조로 작동하는게 디버깅 비용이 적었다. 팀원 간 주문·잔액 불일치도 결국 `GameState` 한곳만 보면 원인이 드러났다.

`Pickpacker/Source/01_GameFlow_PickpackerGameMode.cpp`

```cpp
Order.SubmittedQuantity += UnitsToApply;
if (Order.SubmittedQuantity >= Order.RequiredQuantity)
{
    Order.bCompleted = true;
    HandleOrderSuccess(Order);
}

SyncOrdersToGameState();

TeamCredits = FMath::Max(0, TeamCredits + Delta);
CreditHistory.Add(Transaction);
OnCreditsChanged.Broadcast(TeamCredits, TeamCredits - OldCredits);
```

#### 3. Interaction & Inventory – 트레이스 보정·협동 인벤
<img src="https://kimasill.github.io/images/Pickpacker/상호작용.png" alt="Pickpacker 초기 프로파일링" width="640" />

Parcel이 카메라 앞을 가리면 타깃이 안 잡히고, 클라이언트를 기반으로 상호작용을 진행하면 서버와 어긋나 유실·중복처럼 보인다. 이런식으로 특히 상호작용 부분에서 서버와 클라이언트 상태가 다르게 동작하는 부분이 많았다.

`CarriedParcel`과 부착 부모를 `AddIgnoredActor`로 트레이스에서 빼고, 권한이 없으면 `Server_CollectItem`으로 보내 `CollectedItems`는 서버에서만 갱신하게 했다.

`Pickpacker/Source/02_Interaction_Inventory.cpp`

```cpp
// 상호작용 트레이스
if (IsValid(CarriedParcel))
{
    InteractionParams.AddIgnoredActor(CarriedParcel);
    if (AActor* Carrier = CarriedParcel->GetAttachParentActor())
    {
        InteractionParams.AddIgnoredActor(Carrier);
    }
}

// 인벤 수집·서버 권한
if (!GetOwner() || !GetOwner()->HasAuthority())
{
    Server_CollectItem(Item);
    return true;
}
CollectedItems.Add(Item);
Item->SetActorHiddenInGame(true);
```

#### 4. AI – BT·블랙보드 일원화·순찰 클램프

BT·블랙보드 초기화가 제각각이면 AI를 추가할 때마다 세팅 누락으로 깨진다. 초기화 문제로 순찰 지점이 지정된 순찰 박스 밖으로 나가면 내비 실패 → 유닛 정체 → 프레임 저하로 이어진다. `RunBehaviorTreeWithBlackboard`로 블랙보드를 주입한 뒤 BT를 시작하고, 순찰은 박스 내 랜덤 샘플 후 최소·최대 반경으로 클램프해서 이 두 가지를 같이 최적화했다.

`Pickpacker/Source/03_AI_Architecture.cpp`

```cpp
if (!UseBlackboard(BBToUse, BlackboardComp)) { return false; }
RunBehaviorTree(BTAsset);
```
<img src="https://kimasill.github.io/images/Pickpacker/Drone.png" alt="Pickpacker 드론" width="640" />

*Drone정찰/Perception처리테스트*
#### 5. Escape & Ending – 플래그 변동 시 엔딩 재평가

월드 플래그나 탈출 인원이 바뀐 뒤 엔딩을 다시 평가하지 않으면 조건을 채워도 분기가 빠진다.

탈출 조건을 만족해도 엔딩 실행이 되지 않으면 세션 전체가 마비된다.

`SetWorldFlag`·승인 플레이어 갱신 시 `EvaluateEndings`를 호출하고 `EndingData`를 `IsEndingConditionMet`로 순회했다. 엔딩을 데이터 에셋으로 빼 두면 기획·밸런스 수정 때 코드 재컴파일을 줄이기 편할것이라고 생각했다.

`Pickpacker/Source/04_Escape_EndingFlow.cpp`

```cpp
if (FWorldFlagEntry* Entry = FindWorldFlagEntry(Flag))
{
    Entry->Value = Value;
}
// …
EvaluateEndings();

for (const UDA_EndingData* EndingData : EndingDataAssets)
{
    if (EndingData && IsEndingConditionMet(EndingData))
    {
        StartEnding(EndingData);
        return;
    }
}
```

#### 6. Multiplayer Sessions – LAN·온라인 세션 분기 한곳에

LAN·온라인에서 세션 설정·검색 쿼리가 달라 분기가 흩어지면, 친구 방이 안 보이는 식으로 멀티 자체가 동작하지 않았다.

`bIsLANMatch`·`bIsLanQuery`로 로컬 서브시스템을 구분하고 `SEARCH_LOBBIES` / `SEARCH_PRESENCE`를 전처리 분기로 맞춰 플랫폼별 예외를 세션 모듈 한곳에 모았다.

`Pickpacker/Source/05_MultiplayerSessions.cpp`

```cpp
LastSessionSettings->bIsLANMatch = IOnlineSubsystem::Get()->GetSubsystemName() == "NULL";
// MatchType, SessionVisibility 설정 …
LastSessionSearch->bIsLanQuery = IOnlineSubsystem::Get()->GetSubsystemName() == "NULL";
#if defined(SEARCH_LOBBIES)
    LastSessionSearch->QuerySettings.Set(SEARCH_LOBBIES, true, EOnlineComparisonOp::Equals);
#else
    LastSessionSearch->QuerySettings.Set(SEARCH_PRESENCE, true, EOnlineComparisonOp::Equals);
#endif
```


### Problem Solving

#### Visual: Rendering & Optimization

웹 [pickpacker-process](https://kimasill.github.io/projects/pickpacker-process.html)과 동일 출처.

<img src="https://kimasill.github.io/images/Pickpacker/%EC%B5%9C%EC%A0%81%ED%99%94.png" alt="Pickpacker 초기 프로파일링" width="640" />

*초기 프로파일링 — 드로우 콜·CPU 병목 확인*

<img src="https://kimasill.github.io/images/Pickpacker/%EC%B5%9C%EC%A0%81%ED%99%943.png" alt="Pickpacker 씬 캡처·드로우 콜 최적화 후" width="640" />

*씬 캡처 틱·빈도 조정 후 (아래 표와 동일 출처)*

- **프로파일링·측정** — GPU 약 **9 ms** 수준. 드로우 콜 과다로 CPU 병목 시 FPS **약 25**까지 하락(개발 빌드·프로파일러, 웹 프로세스와 동일 출처).
- **HISM·씬 캡처** — 정적 메시 HISM 병합으로 평균 FPS **약 43**까지 상승. 병목은 Scene Capture 과다 호출로 판정.  
  매 프레임 캡처 → 이동 시만 캡처로 전환, 드로우 콜 **약 10,518 → 4,600**, FPS **약 94**.  
  루멘 설정과 라이팅 최적화 조정 후 드로우 콜 **약 3,200**(초기 **약 11,061** 대비 **약 71%** 감소), Prims **약 400K**, FPS **약 100** 부근.  
  드로우 콜 전체 **약 70%** 수준으로 축소, 밀집 구간에서도 협동 플레이 가능한 프레임 예산(상세는 웹과 동일).
- 상호작용 — Parcel이 시야를 가리는 등 예외를 트레이스·채널 설계에 반영.

| 최적화 단계 (요약) | FPS (대략) | Draw Calls | Prims |
| --- | --- | --- | --- |
| 초기 (CPU 병목) | **~25** | **~11,061** | **~1,581K** |
| Step 3 (씬 캡처 틱 조정 후) | **~100** | **~3,200** | **~400K** |

### Result

- 서버 권한·복제 기준으로 루프 전체가 연결된 협동 게임임을 보여 줌. 
- 대표 소스: `Pickpacker/Source/README.md` (`01_`~`05_*.cpp`; Core 항목 1·2는 동일 파일 `01_GameFlow_PickpackerGameMode.cpp` 참조).

---

## 3. DevCom Tycoon (GameTycoon)

<a href="https://kimasill.github.io/projects/devcomtycoon.html" title="DevCom Tycoon 프로젝트 페이지" target="_blank" rel="noopener noreferrer"><img src="https://kimasill.github.io/images/DevComTycoon/Title.png" alt="DevCom Tycoon 타이틀" width="640" /></a>

링크 · [프로젝트 페이지](https://kimasill.github.io/projects/devcomtycoon.html) · 코드 패키지: `GameTycoon/README.md`, `GameTycoon/Source/`

### Overview

| 항목 | 내용 |
| --- | --- |
| 장르 | 게임 회사 경영 시뮬레이션(타이쿤) |
| 엔진·스택 | Java · Java Swing |
| 기간·규모 | 핵심 2인 · 약 2개월(2020) · 팀명 게발게발 |

### Role

- 핵심 프로그래밍·시스템 설계 공동(핵심 2인). 본인: 메인 시간 루프·일시정지, 직원 이동·애니메이션, 개발·출시·매출 경제, UI 갱신 동기화, 세이브/로드 등 타이쿤 전반 (`01`~`06` 참고).


#### Visual: Game Architecture

웹 [devcomtycoon](https://kimasill.github.io/projects/devcomtycoon.html)과 동일 자산.

<img src="https://kimasill.github.io/images/DevComTycoon/dev%EA%B2%8C%EC%9E%84%EC%95%84%ED%82%A4%ED%85%8D%EC%B2%98.png" alt="DevCom Tycoon 게임 아키텍처" width="640" />

### Core Implementation

대표 소스는 `GameTycoon/Source/`의 `01_`~`06_*.java`이며, 원본 트리(`GameTycoon/src/system/...`)와 대응되는 발췌이다.

#### 1. Game Loop – 시간 흐름을 한 스레드에 고정

실제 시간 흐름과 백그라운드 작업 진행을 구현해야 했기 때문에 해당 작업을 위해 멀티쓰레드 구조를 구축했다.

매출과 개발 진행 시간 흐름이 다른 경우를 보면 날짜가 진행되지 않는다거나 정산이 두 번 되는 식의 버그가 난다.

`GameBoard` 스레드에서 시간을 돌리고, 메뉴가 떠 있거나 `POPUP_LAYER`에 컴포넌트가 있을 때는 시뮬을 멈춘다. 스윙에서 기본적으로 작동되는 UI 팝업 후에도 시간이 가는 구조를 막기위해 UI 상태를 루프 조건에 넣었다.

하루가 두 번 지나가거나 팝업 중 정산이 되는 문제는 이 조건 하나로 설명된다. 코드에서 `24`시간 루프와 `POPUP_LAYER` 체크를 볼 수 있다.

`GameTycoon/Source/01_GameBoard_GameLoop.java`

```java
if (isRun && !menu.isVisible()
        && layeredPane.getComponentCountInLayer(JLayeredPane.POPUP_LAYER) == 0) {
    if (++hour == 24) {
        com.setTime(1);
        com.sellGame();
        if (com.getProjectCount() != 0) {
            com.progressProject();
        }
        hour = 0;
    }
}
```

#### 2. Developer Movement – 이동·애니메이션 동기

엔진 없이 스프라이트를 그릴 때 경로·애니메이션과 프레임 타이밍이 어긋나면 화면이 깜빡인다. 직원 움직임이 순간이동·잔상처럼 보이지 않아야 했다.

전용 스레드에서 `frameNum`·이동·목표 좌표를 갱신하고 `repaint`로 맞췄다. `Developer` 쪽 구현은 `Thread.sleep(20)` 기준으로 돌고, 4틱마다 프레임을 넘기는 방식이라 왜 그렇게 보이는지 설명하기도 쉬웠다.

<img src="https://kimasill.github.io/images/DevComTycoon/TitleImg.png" alt="DevCom Tycoon 개발 화면 스크린샷" width="640" />


`GameTycoon/Source/02_Developer_Movement.java`

```java
thread = new Thread(this);
thread.start();
// run() 안에서 frameNum·좌표 갱신 후 draw.repaint();
```

#### 3. Economy Loop – 돈의 출처를 한 곳에 모으기

타이쿤 경제 구조의 최적화를 위해 경제 시스템 구조를 단순화 하려 했다.  `DevGame`에 진행도를 두고 `Company`가 시간·자금·프로젝트 슬롯을 한데 묶게 해서, 경제 시스템 관련 버그는 회사와 프로젝트 객체만 따라가면 정리가 됐다.

`GameTycoon/Source/03_DevGame.java`, `05_Company.java`

```java
// DevGame — 팀 능력 합산 후 진행도 가산
public void addProgress() {
    if (this.progress >= 100) return;
    int devSpeed = 10000 / game.getInterest();
    int totalDevAbility = 0;
    for (Developer dev : team) {
        if (dev == null) break;
        if (dev.work()) totalDevAbility += dev.getAbility();
    }
    if (totalDevAbility == 0) return;
    devSpeed *= (1 + (totalDevAbility) / 100);
    this.progress += (float) devSpeed / 100;
}

// Company — 초기 자금·규칙·프로젝트 슬롯
public Company(String companyName, Developer defaultDev, int time, Rule rule) {
    this.companyName = companyName;
    this.projectCount = 0;
    this.devList.add(defaultDev);
    this.money = 200;
    this.rule = rule != null ? rule : new Rule();
    // …
    gameUpdater.signal(this);
}
```

#### 4. Simulation & UI – UI 갱신·시뮬레이션 진행 분리

스윙 UI와 시뮬레이션의 스레드가 뒤섞이면 팝업을 띄운 뒤에도 시간이 흐르는 경우가 나타났다. 설정 창을 연 사이에 하루가 지나가 버리면 플레이어는 일시정지했다고 착각한 채 패배한다.

`GameUpdater`에서 `wait`/`notify`로 UI 갱신 시점을 끊고 상태바·진행바를 맞췄다. 1번의 레이어 정지와 같이 쓰면 시뮬이 도는 타이밍이 분명해진다. 세이브는 `Serializable`와 `ObjectOutputStream`으로 진행 단위를 저장·로드했다.

UI 갱신 경로가 한곳으로 모였고, 팝업 관련 버그도 재현 지점이 훨씬 또렷해졌다.

`GameTycoon/Source/06_GameUpdater_Events.java`

```java
synchronized (this) {
    this.wait();
    statusBarUpdate();
    progressBarUpdate();
}
// signal()에서 notify()로 깨움
```

#### Visual: Class Diagram & In-Game Screen

<img src="https://kimasill.github.io/images/DevComTycoon/dev%ED%81%B4%EB%9E%98%EC%A스%EB%8B%A4%EC%9D%B4%EC%96%B4%EA%B7%B8%EB%9E%A8.png" alt="DevCom Tycoon 클래스 다이어그램" width="640" />

<img src="https://kimasill.github.io/images/DevComTycoon/%EA%B0%9C%EB%B0%9C%ED%99%94%EB%A9%B4.png" alt="DevCom Tycoon 개발 화면 스크린샷" width="640" />

### Problem Solving

- 팝업 표시 중에도 시뮬이 돌아가는 전형적 버그를 레이어 기준 정지로 차단.
- 엔진 없이 규칙을 코드로 고정, 동작 예측 가능성 우선.

### Result

- 상용 엔진 없이 완성된 싱글플레이 경제 루프 구현 입증.
- 대표 소스: `GameTycoon/Source/README.md` (`01_`~`06_*.java`).

---

# 기타 프로젝트 (요약)

아래는 한 줄 요약·역할·링크만 정리한 표입니다. 깊은 설명·대표 소스는 웹 또는 별도 문서를 참고하시면 됩니다.

| 프로젝트 | 한 줄 | 역할·비고 | 링크 |
| --- | --- | --- | --- |
| PCG Level Generator | UE5 PCG·그래프·MST 기반 던전 자동 생성 플러그인 | 1인 · PCG 그래프·C++/PCGEx 보완 | [웹](https://kimasill.github.io/projects/pcg-dungeon.html) |
| kCars Classification | YOLOv5 기반 국산 차량 74종 분류(데이터·학습·논문) | 1인 · 데이터·모델·평가 | [웹](https://kimasill.github.io/projects/car-classification.html) |
| Startup Camp | 기상·기후 연계 배달 플랫폼 창업 캠프 | 팀장 · 기획·사업계획·피치·리딩 | [웹](https://kimasill.github.io/projects/startup.html) |
| Tab Game | 클릭커·보스·페이즈 전투(WinForms, 48h) | 1인 · 기획·코드·그래픽 | [웹](https://kimasill.github.io/projects/tab-game.html) |

---

## 기술 스택 요약

요약 목록은 다음과 같습니다.

- 엔진·런타임: Unity · Unreal Engine 5 · Java Swing · .NET
- 언어: C# · C++ · Java · Python
- 네트워크·동기화: C# 전용 게임 서버(MMO) · Protobuf · Interest Management · TaskQueue · UE Replication · RPC · Online Subsystem
- 데이터·도구: EF Core · SQL Server · PCG / PCGEx · PyTorch · YOLOv5

---

## 마무리

이 문서는 제작의도와 기준을 전달하기 위해 제작되었습니다. 영상·스크린샷·세부 제작 과정은 웹 포트폴리오에서 확인 가능합니다.

웹 · https://kimasill.github.io/

---

## 부록 — 파일·폴더

| 파일 | 역할 |
| --- | --- |
| `SUNG_HYEON_KIM_Portfolio.md` | 본 문서 (편집·PDF 생성 원고) |
| `README.md` | 패키지 목차·대표 소스 하이라이트 |
| `build_portfolio_pdf.py` | Markdown → PDF 빌드 스크립트 (원격 이미지 임베드) |


프로젝트별 (로컬 검증 시): Dawnstar `Dawnstar/README.md`, `Dawnstar/Source/README.md` · Pickpacker `Pickpacker/README.md`, `Pickpacker/Source/README.md` · GameTycoon `GameTycoon/README.md`, `GameTycoon/Source/README.md`

이미지 출처: 본문 삽입 그림은 모두 `https://kimasill.github.io/images/` 하위 파일이며, 상세 설명·추가 스크린은 각 프로젝트 웹 페이지와 동일합니다.
