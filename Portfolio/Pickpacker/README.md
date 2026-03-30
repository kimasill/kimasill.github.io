# Pickpacker

## 프로젝트 한 줄 소개

`Pickpacker`는 주문 처리, 아이템 수집/포장, AI 순찰과 추적, 탈출 엔딩, 온라인 세션 참가까지 하나의 루프로 연결한 Unreal Engine 기반 협동 멀티플레이 게임입니다.

이 섹션에서는 "기능을 많이 만들었다"보다 "시스템을 어떻게 구조화했고, 서버 권한과 상태 동기화를 어떻게 설계했는가"를 보여주는 구현 위주로 정리했습니다.

## 제가 보여주고 싶은 역량

- 서버 권한 기반 게임 진행 구조 설계
- 상호작용과 인벤토리의 멀티플레이 동기화
- 재사용 가능한 AI 공통 레이어 설계
- 데이터 기반 엔딩 조건 처리
- 온라인 세션 생성/검색/참가 흐름 구현

## 핵심 구현 1. 주문 시스템과 게임 진행 루프

중심은 `APickpackerGameMode`와 `APickpackerGameState`입니다. 매치 시작 시 스트리밍 레벨을 먼저 로드해 패키징 빌드에서 참조 실패를 막고, 게임 시작 후에는 주문 웨이브를 생성하며, 제출된 Parcel의 태그/수량/포장 여부를 기준으로 주문을 충족시킵니다. 이 과정에서 크레딧 변화와 게임오버 조건도 같은 흐름 안에서 처리합니다.

단순 스폰 형태가 아니라 `웨이브 -> 활성 주문 -> 제출 판정 -> 보상/패널티 -> 종료 조건`까지 이어지는 루프 전체를 서버 권한 기준으로 설계했습니다. 주문 충족 시 태그별 소모량을 따로 계산해 중복 반영을 방지했으며, `GameState`에 활성 주문과 크레딧을 복제해 팀원 간 UI와 세션 상태를 일관되게 유지합니다.

대표 코드: `Pickpacker/Source/01_GameFlow_PickpackerGameMode.cpp`

## 핵심 구현 2. 상호작용 + 인벤토리 네트워크 설계

`UInteractionComponent`는 화면 중앙 기준 트레이스를 수행하되, 들고 있는 Parcel이 시야를 가리는 상황까지 고려해 별도 채널과 스윕, 선반 우선순위, 히스테리시스까지 적용했습니다. `UPlayerInventoryComponent`는 아이템 수집과 슬롯 저장을 서버 RPC로 통일하고, 복제 콜백에서 UI를 갱신합니다.

단순 아이템 획득이 아니라 로컬 입력 감지와 서버 권한 처리를 분리하여 설계했습니다. 월드 오브젝트를 숨기고 충돌과 물리 판정을 제어해 실제 플레이 루프와 맞물리게 했고, `CarriedParcel`과 `CollectedItems` 상태를 복제해 다른 위치의 팀원 시점에서도 상호작용 동작이 어긋나지 않도록 했습니다.

대표 코드: `Pickpacker/Source/02_Interaction_Inventory.cpp`

## 핵심 구현 3. 확장 가능한 AI 구조

AI는 개별 몬스터마다 코드를 중복하기보다 공통 컨트롤러, 시야 컴포넌트, 패트롤 포인트 계산 유틸리티를 분리했습니다. `APPAIControllerBase`가 블랙보드와 비헤이비어 트리 초기화를 공통 처리하고, `UPPSightPerceptionComponent`가 시야 파라미터를 모듈화하며, `PPPatrolBoundsLibrary`가 볼륨 기반 순찰 좌표를 계산합니다.

적 유닛 종류가 늘어나더라도 공통 초기화 코드를 재사용할 수 있게 구성했습니다. 감지 범위와 순찰 가능 영역을 액터와 컴포넌트 단위 변수로 분리하여 다루며, 시스템 단위를 잘게 쪼개어 언리얼 엔진의 BT(Behavior Tree) Task와 결합하기 쉬운 형태로 설계했습니다.

대표 코드: `Pickpacker/Source/03_AI_Architecture.cpp`

## 핵심 구현 4. 월드 상태 기반 엔딩 분기

`UEscapeProgressComponent`는 월드 플래그와 탈출 인원 수를 복제 상태로 보관하고, 값이 바뀔 때마다 엔딩 데이터 에셋을 순회하며 조건을 평가합니다. 이 방식 덕분에 엔딩 분기 로직이 하드코딩된 if-else 덩어리가 아니라 데이터 기반 규칙으로 정리됩니다.

단순 하드코딩된 조건문 대신 본질적인 월드 플래그 값과 인가된 플레이어 수를 분리하여 엔딩 조건을 다양하게 확장할 수 있게 만들었습니다. 진행 흐름 중에 조건이 달라질 때마다 즉시 재평가하는 이벤트 중심 방식을 따르며, 기획 쪽에서 데이터 에셋만 수정해도 엔딩 조합을 즉각 조정할 수 있습니다.

대표 코드: `Pickpacker/Source/04_Escape_EndingFlow.cpp`

## 핵심 구현 5. 온라인 세션 처리

`UMultiplayerSessionsSubsystem`에서는 세션 생성, 검색, 참가를 하나의 서브시스템으로 묶었습니다. LAN/온라인 분기, Presence/Lobbies 설정, 세션 가시성, 세션 제목 인코딩, 기존 세션 파기 후 재생성까지 실사용 이슈를 고려해 구현했습니다.

언리얼 Online Subsystem 기능들을 멀티플레이 프로젝트 흐름에 맞춰 래핑했습니다. Steam 서비스와 Null(구내 LAN) 환경 상의 패킷 및 환경 차이와 검색 호환성 조건을 하나로 묶었고, 단계별로 디버깅 로그를 남겨 향후 발생 가능한 세션 탐색 문제를 역추적할 수 있게 구성했습니다.

대표 코드: `Pickpacker/Source/05_MultiplayerSessions.cpp`

## 어떤 역량이 가장 잘 드러나는가

이 프로젝트에서 가장 강하게 드러나는 역량은 `물리적 게임 진행 흐름 설계`와 `멀티플레이 권한 시스템 동기화 구현`입니다. 주문, 크레딧, 탈출, 엔딩, 세션 처리까지 서로 다른 카테고리가 서버 권한과 상태 복제라는 하나의 잣대를 중심으로 모여 있기 때문에, 단순히 기능을 나열한 것이 아니라 "협동 게임 루프를 유기적으로 맞춘 구조파악 능력"을 보여주기에 적합합니다.

## 함께 보면 좋은 원본 파일

- `Source/Blaster/GameMode/PickpackerGameMode.cpp`
- `Source/Blaster/GameState/PickpackerGameState.cpp`
- `Source/Blaster/Components/InteractionComponent.cpp`
- `Source/Blaster/Components/PlayerInventoryComponent.cpp`
- `Source/Blaster/Components/EscapeProgressComponent.cpp`
- `Source/Blaster/AI/PPAIControllerBase.cpp`
- `Source/Blaster/AI/PPPatrolBoundsLibrary.cpp`
- `Source/Blaster/AI/UPPSightPerceptionComponent.cpp`
- `Plugins/MultiplayerSessions/Source/MultiplayerSessions/Private/MultiplayerSessionsSubsystem.cpp`
