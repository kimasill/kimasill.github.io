"""
Portfolio HTML → PDF converter.

가로(A4 landscape) 슬라이드형 포트폴리오 HTML을 그대로 PDF로 렌더링합니다.
브라우저 인쇄와 동일하게 @page / @media print 규칙을 적용합니다.
"""
from __future__ import annotations

import asyncio
from pathlib import Path

from playwright.async_api import async_playwright

ROOT = Path(__file__).resolve().parent
HTML_PATH = ROOT / "Portfolio" / "_build" / "SUNG_HYEON_KIM_Portfolio.html"
OUT_PATH = ROOT / "Portfolio" / "SUNG_HYEON_KIM_Portfolio.pdf"


async def _generate_pdf(html_path: Path, out_path: Path) -> None:
    async with async_playwright() as p:
        browser = await p.chromium.launch()
        page = await browser.new_page()
        await page.goto(html_path.as_uri(), wait_until="networkidle")
        await page.emulate_media(media="print")
        await page.pdf(
            path=str(out_path),
            format="A4",
            landscape=True,
            print_background=True,
            margin={"top": "0", "right": "0", "bottom": "0", "left": "0"},
            display_header_footer=False,
        )
        await browser.close()


def main() -> None:
    if not HTML_PATH.exists():
        raise FileNotFoundError(f"HTML not found: {HTML_PATH}")

    print(f"Source HTML: {HTML_PATH}")
    asyncio.run(_generate_pdf(HTML_PATH, OUT_PATH))
    print(f"PDF generated: {OUT_PATH}")


if __name__ == "__main__":
    main()
