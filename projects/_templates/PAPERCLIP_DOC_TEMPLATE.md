# Paperclip 문서형 포트폴리오 페이지 — 템플릿 스펙

`projects/paperclip*.html` 시리즈와 동일한 **구조·클래스·자산 경로**로 새 문서 페이지를 만들 때 이 파일을 따른다. 스타일은 전부 `css/portfolio.css`의 **`.paperclip-doc` 네임스페이스**와 공통 프로젝트 레이아웃에 의존한다.

---

## 1. 자산·의존성

| 항목 | 경로 (`projects/` 기준) |
|------|-------------------------|
| Bootstrap | `../css/bootstrap.min.css` |
| 사이트 공통 | `../css/styles.css` |
| **필수 (Paperclip 전용 스타일)** | `../css/portfolio.css` |
| Font Awesome | `../libs/font-awesome/css/font-awesome.min.css` |
| 파비콘 | `../favicon.ico` |
| 테마·언어 토글 | `../js/portfolio.js` (jQuery 필요) |
| jQuery | `https://ajax.googleapis.com/ajax/libs/jquery/1.12.4/jquery.min.js` |
| Mermaid (선택) | `https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.min.js` |

**본문 루트 클래스:** `body.proj-detail-page`  
**메인 래퍼:** `main.project-page.proj-with-section-lines.paperclip-doc`

섹션 구분선을 쓰려면 `proj-with-section-lines`를 유지한다 (`portfolio.css`에서 인접 섹션 border 처리).

---

## 2. 페이지 두 가지 유형

### A. 허브 페이지 (`paperclip.html`)

- 상단: **다크 히어로** `section.proj-hero.proj-hero-dark`
  - 내부: `div.proj-hero-inner` → `h1` + `div.proj-hero-split`
  - 왼쪽: `div.proj-hero-media.paperclip-hero-thumb` + 이미지
  - 오른쪽: `div.proj-hero-meta` + `p.proj-meta-line` (역할·스택 등)
- 그 다음: **로컬 내비** (아래 2번과 동일). 허브에서는 해당 링크에 `class="is-active"`를 **개요**에만 붙인다.

### B. 서브 페이지 (`paperclip-*.html` 단일 주제)

- 상단 히어로 대신 **미니 히어로** `section.paperclip-mini-hero`
  - `div.proj-container-wide` 안에 `div.paperclip-mini-hero-icon` (Font Awesome) + `div.paperclip-mini-hero-text`
  - `h1`, `p.paperclip-mini-hero-desc`, `a.paperclip-mini-hero-back.btn-link` (허브로 복귀)
- 이후 **로컬 내비**에서 현재 페이지만 `class="is-active"`

---

## 3. 글로벌 헤더 (모든 Paperclip 페이지 공통)

- `header.proj-header`
  - `div.proj-header-top`: 브랜드 `a.proj-header-brand` → `../index.html`
  - `div.header-controls`: `#lang-toggle`, `#theme-toggle` (portfolio.js와 연동)
- `nav.proj-header-nav`: 다른 프로젝트 링크 + Paperclip 항목
  - Paperclip 시리즈에서 이 프로젝트가 활성일 때:  
    `a href="paperclip.html" class="active proj-header-nav-orch"`  
    긴 제목은 `proj-header-nav-orch`로 작은 폰트·줄바꿈 허용.

---

## 4. 로컬 내비 (문서 허브 내 이동)

```html
<div class="proj-container-wide paperclip-local-nav-wrap">
  <nav class="paperclip-local-nav" aria-label="(시리즈명) 섹션">
    <a href="paperclip.html" class="is-active">개요</a>
    <a href="paperclip-server-webhook.html">서버 · 웹훅</a>
    <!-- … -->
  </nav>
</div>
```

- 현재 페이지의 `<a>`에만 `is-active` (중복 금지).

---

## 5. 본문 섹션 리듬 (번갈아 배경)

| 용도 | 섹션 클래스 | 비고 |
|------|-------------|------|
| 밝은 띠 | `section.proj-intro` | 기본 흰 배경 |
| 번갈 띠 | `section.proj-overview.proj-block-alt` | 약간 다른 배경 (`--alt` 계열과 조화) |

콘텐츠 폭: `div.proj-container-wide` (max-width 1280px, 좌우 패딩).

---

## 6. 타이포·텍스트 유틸

| 클래스 | 용도 |
|--------|------|
| `h2.proj-section-title` | 대제목 |
| `h3.proj-block-title` | 소제목 |
| `p.paperclip-lead` | 도입 강조 단락 |
| `p.paperclip-muted` | 보조 설명 (필요 시 `style="max-width:52em"`) |
| `code.paperclip-code` | 인라인 코드·환경변수명 |
| `a.proj-detail-accent-link` | 본문 강조 링크 (파란 톤) |
| `a.btn-link` | 보조 링크·뒤로가기 |

---

## 7. 블록 컴포넌트 (복붙용 개요)

### Before / After (`paperclip-evolution` + `paperclip-ba`)

- 제목: `p.paperclip-evolution-title` (+ 선택 `i.fa`)
- 그리드: `div.paperclip-ba` → `div.paperclip-ba-col.paperclip-ba-before` | `div.paperclip-ba-arrow` | `div.paperclip-ba-col.paperclip-ba-after`
- 내부 라벨: `span.paperclip-ba-label`

### 의사결정 콜아웃

- `div.paperclip-decision` + `span.paperclip-decision-label` + `p`

### 타임라인 단계

- `ol.paperclip-journey` + `li.paperclip-journey-step`

### 인사이트 한 줄

- `div.paperclip-insight` (`strong`으로 강조)

### 정량 칩

- `div.paperclip-metrics` + `div.paperclip-metric` + `span.paperclip-metric-value` / `span.paperclip-metric-desc`

### 참조 카드 (업스트림)

- `aside.paperclip-ref-card` → `div.paperclip-ref-card-inner`
- 키커: `p.paperclip-ref-card-kicker`, 제목: `p.paperclip-ref-card-title`, 설명: `p.paperclip-ref-card-desc`
- 링크 버튼: `a.paperclip-ref-pill`

### 인라인 레퍼런스

- `aside.paperclip-ref-inline`

### 목록

- `ul.paperclip-list` / 번호: `ol.paperclip-list.paperclip-list--steps`
- TOC: `ul.paperclip-toc` + `span.paperclip-toc-desc`

### 트러블슈팅 / 임팩트

- `ul.paperclip-trouble-list` + `li.paperclip-trouble-item` + `p.paperclip-trouble-prob` / `p.paperclip-trouble-sol`
- `ul.paperclip-impact-list` + `li` (`strong`으로 라벨)

### 표

- `table.table.paperclip-table.paperclip-table--stack`
- 비교 2열: `paperclip-table--compare`
- **행 블록 구분(카드형 행·헤더 구분선):** `paperclip-table--stack-blocks` — 2열(`td`만)·3열 모두 가능. 기술 스택처럼 첫 열이 `th[scope=row]`이면 `paperclip-table--clean`도 함께 사용 가능.

### 코드 블록

- `pre.paperclip-pre`

### 스크린샷

- 단일: `figure.paperclip-figure` + `img` + `figcaption`
- 2열: `div.paperclip-figure-grid` 안에 `figure.paperclip-figure` 두 개

### Mermaid

- 래퍼: `div.paperclip-mermaid-wrap` (시퀀스는 `paperclip-mermaid-wrap--sequence` 추가)
- `div.mermaid#고유id` + `script type="text/plain" id="…-source"`에 다이어그램 소스
- 페이지 하단 인라인 스크립트: `mermaid.initialize({ theme: document.body.getAttribute('data-theme') === 'dark' ? 'dark' : 'default', … })` 후 `mermaid.run({ nodes: [...] })`  
  참고: `paperclip.html`, `paperclip-obsidian.html`, `paperclip-server-webhook.html`

---

## 8. 푸터·스크립트 (하단 공통)

```html
<footer>
  <div class="container"><p style="margin:0">© 2026 KIMASILL</p></div>
</footer>
<script src="https://ajax.googleapis.com/ajax/libs/jquery/1.12.4/jquery.min.js"></script>
<!-- Mermaid 쓰는 페이지만 -->
<script src="https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.min.js"></script>
<script src="../js/portfolio.js"></script>
<!-- 필요 시 페이지 전용 인라인 스크립트 -->
```

---

## 9. 하단 CTA (선택)

```html
<section class="proj-footer-cta">
  <a href="paperclip.html" class="btn-link">← 개요</a>
  &nbsp;·&nbsp;
  <a href="paperclip-다음.html" class="btn-link">다음: … →</a>
</section>
```

---

## 10. 경로 규칙

- HTML 파일은 `projects/`에 둔다고 가정.
- 이미지: `../images/(프로젝트폴더)/파일명` — Paperclip은 `../images/paperclip/`.
- 파일명은 **ASCII** 권장 (GitHub Pages·URL 호환).

---

## 11. 새 시리즈를 만들 때 체크리스트

1. `body.proj-detail-page` + `main.project-page.proj-with-section-lines.paperclip-doc`
2. `portfolio.css` 로드 (Paperclip 스타일 포함)
3. 헤더·글로벌 내비에서 시리즈 대표 링크에 `active` / `proj-header-nav-orch` 처리
4. 허브 vs 서브에 맞게 히어로(다크) 또는 미니 히어로 선택
5. 로컬 내비 항목·`is-active` 일치
6. 섹션 `proj-intro` / `proj-overview.proj-block-alt` 교차
7. Mermaid 사용 시 다크 테마 연동 스크립트 포함
8. 스크린샷은 `figure` + `loading="lazy"` `decoding="async"`

---

## 12. 참고 원본 파일

| 파일 | 역할 |
|------|------|
| `projects/paperclip.html` | 허브·히어로·Mermaid·스택 표·메트릭 |
| `projects/paperclip-server-webhook.html` | 미니 히어로·Mermaid flow |
| `projects/paperclip-gitlab.html` | Before/After·표·이미지 그리드 |
| `projects/paperclip-obsidian.html` | 미니 히어로·복수 Mermaid |
| `projects/paperclip-token.html` | 미니 히어로·메트릭·종합 표 |

스타터 HTML: 같은 폴더의 `paperclip-doc-hub-starter.html`, `paperclip-doc-subpage-starter.html`.
