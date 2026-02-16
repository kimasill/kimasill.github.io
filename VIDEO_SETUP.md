# 동영상 설정 가이드

## 1. 메인 페이지 상단 배경 영상 (Jack Bromhead 스타일)

**위치:** `index.html` → `#lead` 요소의 `data-lead-video` 속성

```html
<div id="lead" class="theme-dark" data-lead-video="여기에_유튜브_비디오_ID">
```

- **유튜브 비디오 ID**: `https://www.youtube.com/watch?v=dQw4w9WgXcQ` 에서 `dQw4w9WgXcQ` 부분
- 빈 값(`""`)이면 배경 영상 없음 (기본 그라데이션만 표시)
- 자동재생, 음소거, 무한루프로 전체 화면 배경에 재생됩니다.

---

## 2. 프로젝트 상세 페이지 동영상 (Dean Tate 스타일)

**위치:** `projects/*.html` → `PROJ_DATA.video`

다음 형식 모두 지원합니다:

```javascript
// 유튜브 전체 URL
video:'https://www.youtube.com/watch?v=VIDEO_ID'

// 유튜브 짧은 URL
video:'https://youtu.be/VIDEO_ID'

// 비디오 ID만
video:'VIDEO_ID'

// 로컬 MP4 (기존 방식)
video:'../videos/프로젝트명.mp4'
```

---

## 3. 메인 페이지 카드 썸네일

**위치:** `index.html` → 각 `work-card` 내부

- `poster="images/썸네일.jpg"`: 카드에 보이는 정지 이미지
- `data-src="videos/xxx.mp4"`: 호버 시 재생되는 동영상 (로컬 MP4)
- 유튜브 썸네일 사용 시: `poster="https://img.youtube.com/vi/VIDEO_ID/maxresdefault.jpg"` 로 설정 가능
