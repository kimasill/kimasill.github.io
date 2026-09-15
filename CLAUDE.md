# kimasill.github.io

GitHub Pages로 배포되는 정적 포트폴리오 사이트입니다.

## 구조

- `index.html`: 사이트 진입점(홈)
- `css/`, `js/`, `images/`, `libs/`: 스타일·스크립트·에셋
- `Portfolio/`: 마크다운 원본(`SUNG_HYEON_KIM_Portfolio.md` 등), PDF·워크스페이스, 빌드 산출물이 있을 수 있음
- `export_portfolio_md_to_pdf.py`: 포트폴리오 MD → PDF 변환 스크립트(필요 시)

## 작업 시 참고

- 배포는 저장소 기본 브랜치(보통 `main`)의 루트가 Pages 소스인 경우가 많습니다. 경로나 CNAME 변경 시 GitHub Pages 설정과 일치하는지 확인하세요.
- 언어·테마 토글 등 클라이언트 동작은 `js/` 쪽 스크립트와 연동됩니다. UI 변경 시 관련 CSS(`css/portfolio.css` 등)와 함께 수정하는 편이 안전합니다.

## 개인 설정(커밋하지 않음)

- 루트의 `CLAUDE.local.md`에 본인만의 지시를 두면 `CLAUDE.md`와 함께 로드됩니다(저장소에는 올리지 마세요).

## CLEARIX 페이지 (비밀번호 보호)

- `projects/clearix.html`은 빌드 산출물(AES-256-GCM 암호문, 이미지 인라인). 직접 편집하지 말 것.
- 원본: `_private/clearix/clearix.html` + `_private/clearix/images/` (gitignore, 저장소에 없음 — 별도 백업 필요).
- 빌드: `node scripts/build-clearix-gate.mjs --password <pw>` (`src="../images/clearix/이름"` 참조를 자동 인라인).
- `_private/clearix/README.md`에 자료 목록과 교체용 이미지 안내가 있음.
