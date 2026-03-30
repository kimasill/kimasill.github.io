# SUNG HYEON KIM — GAME PROGRAMMING PORTFOLIO (PDF)

> **한 줄 소개**: 플레이 경험을 중심에 두고, 서버/클라이언트/툴을 “끝까지 연결되는 시스템”으로 설계·구현합니다.

---

## 0. 프로필 (Cover)

- **이름**: 김성현 (SUNG HYEON KIM)
- **이메일**: [kibbel1998@gmail.com](mailto:kibbel1998@gmail.com)
- **전화번호**: TODO
- **핵심 링크**: [GitHub](https://github.com/kimasill) · [웹 포트폴리오](https://kimasill.github.io/) · [LinkedIn](https://www.linkedin.com/in/sung-hyeon-kim-a64b2922a/)

**핵심 성과 (요약)**  
- **DawnStar (2D MMORPG)**: 로그인→게임 서버→DB→퀘스트/전투/파티까지 1인 구현  
- **Pickpacker (UE5 협동 멀티플레이)**: 드로우 콜 **~11,061 → ~3,200(약 71%↓)**, FPS **~25 → ~100**  
- **DevGameTycoon (Java Swing)**: 게임 루프·UI 동기화·경제 시뮬레이션 구현

---

## 1. 기술 스택 (Skills)

### Frontend / Gameplay (Client)
- Unity, Unreal Engine 5, Java Swing
- 입력/상호작용, UI, 전투/AI 구조 설계 및 구현

### Backend / Network (Server)
- C#/.NET, Protobuf, Interest Management, TaskQueue
- 서버 권한과 상태 일관성을 기준으로 “플레이 루프 전체”를 연결

### Database / Data
- SQL Server, EF Core
- 데이터 기반 확장(레시피/룰/엔딩 조건 등) 및 디버깅 가능한 파이프라인 구성

### Performance / Tools
- UE 프로파일링 기반 병목 분석 및 최적화(드로우 콜/씬 캡처/렌더링 비용)

---

# 2. 프로젝트 경험 (Selected Projects)

> PDF에는 **코드 스니펫을 넣지 않습니다.**  
> 자세한 구현/트러블슈팅/코드 스니펫은 각 프로젝트 GitHub `README.md`로 연결됩니다.

---

## 2-1. DawnStar (2D MMORPG)

<img src="https://kimasill.github.io/images/dawnstar/DawnstarTitleImg.png" alt="DawnStar Title" width="640" />

### 프로젝트 개요
| 항목 | 내용 |
| --- | --- |
| 기간 | 2024.08 ~ 2025.02 |
| 인원 | 1인 |
| 역할 | 클라이언트/서버/DB/콘텐츠 전 구간 설계·구현 |
| 스택 | Unity · C# · .NET · SQL Server |

**한 줄 요약**: 로그인 이후 “캐릭터 라이프사이클”과 MMO 콘텐츠 루프(퀘스트/경제/파티/동기화)를 서버 흐름 안에서 완결되게 만든 프로젝트.

**바로가기**: [GitHub](https://github.com/kimasill/DawnStar) · [프로젝트 페이지](https://kimasill.github.io/projects/dawnstar.html)

### 주요 성과·트러블슈팅 (요약)
- 로그인 이후 상태(로비/캐릭터/세션)를 **스냅샷 중심**으로 묶어 복원 실패를 줄임
- 맵 이동/리스폰/던전 입장을 **고정 시퀀스**로 두어 재현·디버깅 비용 절감
- 상점/제작/강화/인챈트의 경제 경로를 **트랜잭션**으로 통합해 일관된 성공/실패 보장
- Interest/TaskQueue 타이밍 조정으로 **틱 스파이크** 완충(체감 동기화 품질 유지)

### 시각 자료
<img src="https://kimasill.github.io/images/dawnstar/%EA%B2%8C%EC%9E%84%EC%84%9C%EB%B2%84%EC%95%84%ED%82%A4%ED%85%8D%EC%B2%98.png" alt="DawnStar Game Server Architecture" width="640" />

---

## 2-2. Pickpacker (UE5 협동 멀티플레이)

<img src="https://kimasill.github.io/images/Pickpacker/PickPackerCover.png" alt="Pickpacker Cover" width="640" />

### 프로젝트 개요
| 항목 | 내용 |
| --- | --- |
| 기간 | 개발 중 |
| 인원 | 1인 |
| 역할 | 게임 루프·상호작용·인벤토리·AI·세션(Online Subsystem) |
| 스택 | Unreal Engine 5 · C++ · Online Subsystem |

**한 줄 요약**: 주문/크레딧/탈출/엔딩을 “서버 권한·복제” 기준으로 정렬한 협동 멀티플레이 게임.

**바로가기**: [프로젝트 페이지](https://kimasill.github.io/projects/pickpacker.html)

### 주요 성과·트러블슈팅 (요약)
- 서버 권한 기반 주문/크레딧/게임오버로 **팀 상태 일관성** 확보
- 상호작용 트레이스 예외(들고 있는 오브젝트 가림)를 **채널/무시 목록 설계**로 보완
- 엔딩 분기를 **데이터 에셋** 기반으로 구성해 기획 변경 비용 감소
- 렌더링 최적화: 드로우 콜 **~11,061 → ~3,200(약 71%↓)**, FPS **~25 → ~100**

### 시각 자료
<img src="https://kimasill.github.io/images/Pickpacker/%EC%B5%9C%EC%A0%81%ED%99%943.png" alt="Pickpacker Optimization" width="640" />

---

## 2-3. DevGameTycoon (Java Swing)

<img src="https://kimasill.github.io/images/DevComTycoon/Title.png" alt="DevGameTycoon Title" width="640" />

### 프로젝트 개요
| 항목 | 내용 |
| --- | --- |
| 기간 | 약 2개월 (2020) |
| 인원 | 핵심 2인 |
| 역할 | 메인 시간 루프·일시정지, 직원 이동·애니메이션, 개발/출시/매출, UI 동기화, 세이브/로드 |
| 스택 | Java · Java Swing |

**한 줄 요약**: 상용 엔진 없이 UI 툴킷만으로 “시간 루프 + 경제 시뮬레이션 + UI 동기화”를 안정적으로 구성한 타이쿤 게임.

**바로가기**: [GitHub](https://github.com/kimasill/DevGameTycoon) · [프로젝트 페이지](https://kimasill.github.io/projects/devcomtycoon.html)

### 주요 성과·트러블슈팅 (요약)
- 메뉴/팝업 상태에서 **시간 정지** 규칙을 루프 조건에 포함해 정산/시간 버그 차단
- 직원 이동/스프라이트 애니메이션을 스레드 기반으로 구현(깜빡임/순간이동 방지)
- 개발 진행/출시/일 매출을 단일 회사 모델로 수렴시켜 디버깅 가능성 확보
- 세이브/로드로 세션 단위 상태를 안정적으로 직렬화

### 시각 자료
<img src="https://kimasill.github.io/images/DevComTycoon/dev%ED%81%B4%EB%9E%98%EC%8A%A4%EB%8B%A4%EC%9D%B4%EC%96%B4%EA%B7%B8%EB%9E%A8.png" alt="DevGameTycoon Class Diagram" width="640" />

---

## 3. 기타 프로젝트 (Summary)

| 프로젝트 | 한 줄 | 링크 |
| --- | --- | --- |
| PCG Dungeon Generator | UE5 PCG/PCGEx 기반 던전 자동 생성 플러그인 | [GitHub](https://github.com/kimasill/DungeonGenerator) |
| kCars Classification | YOLOv5 기반 국산 차량 분류 | [GitHub](https://github.com/kimasill/kCarsClassificationModule) |
| Startup Camp | 기상·기후 연계 배달 플랫폼 창업 캠프 | [웹](https://kimasill.github.io/projects/startup.html) |
| Tab Game | 클릭커·보스·페이즈 전투(WinForms, 48h) | [GitHub](https://github.com/kimasill/TabGame) |

---

## 4. 학력 및 기타 경험

- TODO