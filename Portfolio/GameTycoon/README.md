# GameTycoon (DevGameTycoon)

원본 소스는 `GameTycoon1207` 저장소의 `GameTycoon/` 이클립스 프로젝트에 있습니다.

## 프로젝트 한 줄 소개

`GameTycoon`은 Java Swing으로 구현한 게임회사 경영 시뮬레이션입니다. 플레이어는 CEO가 되어 자본으로 직원을 고용하고 사무실에 오브젝트를 배치하며, 장르·주제 등을 조합해 게임을 개발·출시하고 매출과 평판으로 회사를 성장시킵니다.

이 포트폴리오에서는 `단일 스레드 기반 게임 시간 루프`, `메뉴 일시정지`, `직원 이동·스프라이트`, `개발 진행도와 출시·매출`, `UI 갱신 동기화`, `저장/불러오기`처럼 타이쿤 루프를 뒷받침하는 구현 위주로 정리했습니다.

## 제가 보여주고 싶은 역량

- Swing 환경에서 게임 시간 흐름을 직접 설계하고, UI 열림과 연동해 일시정지 처리
- 그리드 매핑 기반 맵 배치와 마우스 상호작용
- 개발 프로젝트 진행도, 팀 능력치, 흥미도에 따른 시뮬레이션 로직
- 출시 후 일일 매출·인기도 연동 등 경제 루프 구현
- `ObjectOutputStream` 기반 세이브/로드와 UI 갱신 시그널 패턴

## 핵심 구현 1. 메인 게임 루프와 메뉴 일시정지

`GameBoard`는 하루 24시간을 게임 틱으로 나누고, 메뉴가 보이거나 팝업 레이어에 창이 뜨면 시간이 흐르지 않게 했습니다. 하루가 지나면 정산·직원 복귀·프로젝트 진행·파산 검사·상점/고용 목록 갱신 주기 등이 같은 루프에서 이어집니다.

`JLayeredPane.POPUP_LAYER`의 유무로 "대화 중에는 시간 정지"라는 규칙을 직관적으로 제어했습니다. 점심시간 직원 휴식 로직을 시간 흐름과 맞물려 동일 루프에 두었으며, 한 틱 안에서 여러 시스템이 의도한 순서대로 갱신되게끔 설계했습니다.

대표 코드: `GameTycoon/Source/01_GameBoard_GameLoop.java` (전체 `GameBoard.java` 대응)

## 핵심 구현 2. 직원 이동과 도트 스프라이트 애니메이션

`Developer`는 목적지까지 이동할 때 별도 스레드를 띄우고, 프레임 카운터로 스프라이트 인덱스를 바꿉니다. 엘리베이터 구간, 퇴근·출근, 책상 복귀 등은 좌표와 방향으로 분기합니다.

상용 엔진 없이 스레드와 sleep 루프를 활용해 프레임 레이트 단위의 애니메이션을 구현했습니다. 목적지 도착 등 이동 종료 후 휴식이나 체력 회복 로직이 발동하며, 이 상태 변화가 즉시 `repaint` 갱신으로 이어져 자연스럽게 스크린에 반영되도록 연결했습니다.

대표 코드: `GameTycoon/Source/02_Developer_Movement.java` (전체 `Developer.java` 대응)

## 핵심 구현 3. 개발 진행도와 출시·매출 경제

`DevGame`은 팀 능력과 게임 흥미도로 진행 속도를 계산하고, `Game`은 주제·장르·평점·인기도에 따라 가격과 판매 감쇠를 반영합니다. `Company`는 출시 시 리뷰 점수를 매기고, 일일 `sellGame`으로 매출을 누적합니다.

프로젝트 슬롯, 개발 자금 검증, 중복 제목 방지 등의 검사를 `startDev` 진입점에 모아 데이터 무결성을 챙겼습니다. 개발 시뮬레이션의 진행도 상승부터 상용화 후 일일 패키지 매출 증가까지, 경제 관련 변동이 단일 `Company` 모델 하나로 일관되게 수렴하는 유기적 루프를 형성했습니다.

대표 코드: `GameTycoon/Source/03_DevGame.java`, `GameTycoon/Source/04_Game.java`, `GameTycoon/Source/05_Company.java`

## 핵심 구현 4. 상태 표시줄·진행 바와 이벤트 탭

`GameUpdater`는 `wait`/`notify`로 회사 상태가 바뀔 때만 상단 자금·날짜와 출시 중인 프로젝트 진행 바를 갱신합니다. 진행도가 일정 간격을 지나면 확률적으로 개발 이벤트 탭을 띄웁니다.

Java Swing 프레임워크의 UI 스레드와 내부 게임 시뮬레이션 로직 사이에 시그널을 중계하는 얇은 레이어를 두어 비동기 스레드 이슈를 차단했습니다. 진행 바의 비율에 따라 확률적 랜덤 이벤트를 발생시키는 등, 반복되기 쉬운 턴제 요소에 동적인 변주를 더했습니다.

대표 코드: `GameTycoon/Source/06_GameUpdater_Events.java`

## 핵심 구현 5. 세이브/불러오기

`Company`를 `Serializable`로 두고 `save/` 폴더에 `ObjectOutputStream`으로 직렬화합니다. 불러오기 후 `gameUpdater.signal`로 UI를 다시 맞춥니다.

복잡한 파일 입출력 로직 대신 게임 세션을 구성하는 전체 상태를 하나의 `Company` 객체 트리로 직렬화하여 확실하고 유실 없는 세이브 시스템을 구축했습니다. 클래스 필드가 나중에 추가될 경우를 대비해 누락 시 기본값 우회 로직을 적용, 하위 호환성까지 대응했습니다.

대표 코드: `GameTycoon/Source/05_Company.java` (`saveFile` / `loadFile`)

## 어떤 역량이 가장 잘 드러나는가

이 프로젝트에서 가장 돋보이는 역량은 상용 게임 엔진 없이 UI 툴킷만으로 복잡한 타이쿤 게임 루프와 내부 경제, 캐릭터 행동을 단일 실행 흐름으로 안정감 있게 묶어낸 로우레벨 구조 설계 관점입니다. 단순한 기능 구현을 넘어, 플레이어가 이해하기 쉬운 핵심 시뮬레이션 규칙을 코드로 단단하게 고정하는 데 집중했습니다.

## 함께 보면 좋은 원본 파일

원본 저장소는 `GameTycoon1207` 등 별도 워크스페이스에 두고, 아래는 그중 `GameTycoon/` 소스 기준 예시입니다.

- `GameTycoon/src/system/GameBoard.java`
- `GameTycoon/src/system/Struct/Company.java`
- `GameTycoon/src/system/Struct/Developer.java`
- `GameTycoon/src/system/Struct/DevGame.java`
- `GameTycoon/src/system/Struct/Game.java`
- `GameTycoon/src/system/UI/GameUpdater.java`
- `GameTycoon/src/system/Tab/GameLaunchTab.java`
- `GameTycoon/src/system/Tab/DevEventTab.java`
