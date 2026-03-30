# -*- coding: utf-8 -*-
"""SUNG_HYEON_KIM_Portfolio.md -> SUNG_HYEON_KIM_Portfolio.pdf (Playwright/Chromium)."""

from __future__ import annotations

import base64
import re
import ssl
import urllib.parse
import urllib.request
from io import BytesIO
from pathlib import Path

import markdown
from PIL import Image
from playwright.sync_api import sync_playwright

IMAGE_MAX_WIDTH = 640

ROOT = Path(__file__).resolve().parent
MD_NAME = "SUNG_HYEON_KIM_Portfolio.md"
PDF_NAME = "SUNG_HYEON_KIM_Portfolio.pdf"


def build_css() -> str:
    font_stack = "'Malgun Gothic', '맑은 고딕', 'Noto Sans KR', sans-serif"
    mono_stack = "'D2Coding', 'Consolas', 'Courier New', monospace"
    return f"""
@page {{
    size: A4;
    margin: 2cm 1.8cm 2.2cm 1.8cm;
}}

html {{
    font-family: {font_stack};
    font-size: 10pt;
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
}}
body {{
    font-family: {font_stack};
    font-size: 10pt;
    line-height: 1.65;
    color: #1a1a1a;
    margin: 0;
    padding: 0;
}}

/* ── Headings ── */
h1 {{
    font-family: {font_stack};
    font-size: 20pt;
    font-weight: 700;
    color: #111;
    margin: 0 0 0.4em 0;
    padding-bottom: 0.25em;
    border-bottom: 2.5px solid #333;
    break-before: page;
    break-after: avoid;
}}
h1:first-of-type {{
    break-before: avoid;
}}
h2 {{
    font-family: {font_stack};
    font-size: 14pt;
    font-weight: 700;
    color: #222;
    margin: 1.2em 0 0.45em 0;
    padding-bottom: 0.18em;
    border-bottom: 1.2px solid #ddd;
    break-after: avoid;
}}
h3 {{
    font-family: {font_stack};
    font-size: 12pt;
    font-weight: 700;
    color: #222;
    margin: 1.1em 0 0.35em 0;
    break-after: avoid;
}}
h4 {{
    font-family: {font_stack};
    font-size: 10.5pt;
    font-weight: 700;
    color: #333;
    margin: 0.9em 0 0.3em 0;
    break-after: avoid;
}}

/* ── Body ── */
p {{
    margin: 0.35em 0;
    orphans: 3;
    widows: 3;
}}
ul, ol {{
    margin: 0.3em 0 0.45em 0;
    padding-left: 1.5em;
}}
li {{
    margin: 0.2em 0;
    line-height: 1.55;
}}
hr {{
    border: none;
    border-top: 1px solid #e0e0e0;
    margin: 1em 0;
    break-after: avoid;
}}
a {{
    color: #1a5fb4;
    text-decoration: none;
}}
b, strong {{
    font-weight: 700;
}}
em {{
    color: #444;
}}

/* ── Tables ── */
table {{
    border-collapse: collapse;
    width: 100%;
    margin: 0.45em 0 0.7em 0;
    font-size: 9pt;
    line-height: 1.5;
    break-inside: avoid;
}}
th, td {{
    font-family: {font_stack};
    border: 1px solid #d0d0d0;
    padding: 5px 8px;
    text-align: left;
    vertical-align: top;
}}
th {{
    background: #f5f5f5;
    font-weight: 700;
    color: #222;
}}

/* ── Code ── */
code, kbd, samp, tt {{
    font-family: {mono_stack};
    font-size: 8.5pt;
    background: #f0f0f0;
    padding: 1px 4px;
    border-radius: 3px;
}}
pre {{
    font-family: {mono_stack};
    font-size: 7.5pt;
    line-height: 1.45;
    background: #f7f7f7;
    border: 1px solid #e0e0e0;
    border-left: 3px solid #4a90d9;
    padding: 8px 10px;
    margin: 0.4em 0 0.6em 0;
    white-space: pre-wrap;
    word-wrap: break-word;
    break-inside: avoid;
    border-radius: 2px;
}}
pre code {{
    background: transparent;
    padding: 0;
    font-size: inherit;
    border-radius: 0;
}}

/* ── Images ── */
img {{
    display: block;
    max-width: 100%;
    height: auto;
    margin: 0.5em auto 0.35em auto;
}}
.pdf-keep {{
    break-inside: avoid;
    margin: 0.5em auto 0.35em auto;
}}
.pdf-page-break {{
    break-before: page;
}}
"""


def _iri_to_uri(url: str) -> str:
    parts = urllib.parse.urlsplit(url)
    path = urllib.parse.quote(urllib.parse.unquote(parts.path), safe="/%")
    return urllib.parse.urlunsplit(
        (parts.scheme, parts.netloc, path, parts.query, parts.fragment)
    )


def fetch_image_data_uri(url: str) -> str:
    ctx = ssl.create_default_context()
    fetch_url = _iri_to_uri(url)
    req = urllib.request.Request(
        fetch_url,
        headers={"User-Agent": "Mozilla/5.0 (compatible; PortfolioPDF/1.0)"},
    )
    with urllib.request.urlopen(req, timeout=45, context=ctx) as resp:
        raw = resp.read()
    ctype = resp.headers.get_content_type() or "image/png"
    if ";" in ctype:
        ctype = ctype.split(";")[0].strip()

    im = Image.open(BytesIO(raw))
    if im.mode in ("RGBA", "P", "LA"):
        im = im.convert("RGBA")
    else:
        im = im.convert("RGB")
    w, h = im.size
    if w > IMAGE_MAX_WIDTH:
        nh = max(1, int(h * (IMAGE_MAX_WIDTH / w)))
        im = im.resize((IMAGE_MAX_WIDTH, nh), Image.Resampling.LANCZOS)

    buf = BytesIO()
    if im.mode == "RGBA":
        im.save(buf, format="PNG", optimize=True)
        out_ctype = "image/png"
    else:
        im.save(buf, format="JPEG", quality=88, optimize=True)
        out_ctype = "image/jpeg"

    b64 = base64.b64encode(buf.getvalue()).decode("ascii")
    return f"data:{out_ctype};base64,{b64}"


def embed_remote_images(html: str) -> str:
    cache: dict[str, str] = {}

    def repl(m: re.Match[str]) -> str:
        full = m.group(0)
        url = m.group(1)
        if not url.startswith("https://"):
            return full
        if url not in cache:
            try:
                cache[url] = fetch_image_data_uri(url)
                print(f"  [OK] {url[:80]}")
            except Exception as e:
                print(f"  [WARN] {url[:80]} -> {e}")
                return full
        return full.replace(url, cache[url], 1)

    return re.sub(r'src="(https://[^"]+)"', repl, html)


def postprocess_pdf_html(body: str) -> str:
    body = re.sub(
        r"<p>\s*(<a[^>]*>\s*<img[^>]+/?>\s*</a>)\s*</p>",
        r'<div class="pdf-keep">\1</div>',
        body,
        flags=re.IGNORECASE,
    )
    body = re.sub(
        r"<p>\s*(<img[^>]+/?>)\s*</p>",
        r'<div class="pdf-keep">\1</div>',
        body,
        flags=re.IGNORECASE,
    )
    return body


def insert_major_section_breaks(html: str) -> str:
    marker = '<div class="pdf-page-break">'
    needles = (
        "<h2>1. Dawnstar</h2>",
        "<h2>2. Pickpacker</h2>",
        "<h2>3. DevCom Tycoon (GameTycoon)</h2>",
        "<h2>기술 스택 요약</h2>",
    )
    for n in needles:
        wrapped = f"{marker}{n}</div>"
        if n in html and wrapped not in html:
            html = html.replace(n, wrapped, 1)
    return html


def md_to_html(md_text: str) -> str:
    body = markdown.markdown(
        md_text,
        extensions=["extra", "tables"],
    )
    body = postprocess_pdf_html(body)
    print("Embedding remote images...")
    body = embed_remote_images(body)
    body = insert_major_section_breaks(body)
    css = build_css()
    return f"""<!DOCTYPE html>
<html lang="ko">
<head>
<meta charset="utf-8" />
<title>SUNG HYEON KIM — Portfolio</title>
<style type="text/css">{css}</style>
</head>
<body>
{body}
</body>
</html>
"""


def main() -> None:
    md_path = ROOT / MD_NAME
    pdf_path = ROOT / PDF_NAME
    if not md_path.is_file():
        raise SystemExit(f"Not found: {md_path}")

    print(f"Reading: {md_path}")
    md_text = md_path.read_text(encoding="utf-8")
    html = md_to_html(md_text)

    html_debug = ROOT / "debug_output.html"
    html_debug.write_text(html, encoding="utf-8")
    print(f"HTML saved: {html_debug}")

    print("Generating PDF with Playwright (Chromium)...")
    with sync_playwright() as pw:
        browser = pw.chromium.launch()
        page = browser.new_page()
        page.set_content(html, wait_until="networkidle")
        page.pdf(
            path=str(pdf_path),
            format="A4",
            margin={
                "top": "2cm",
                "right": "1.8cm",
                "bottom": "2.2cm",
                "left": "1.8cm",
            },
            print_background=True,
            display_header_footer=True,
            header_template='<span></span>',
            footer_template=(
                '<div style="width:100%;text-align:center;font-size:8pt;'
                "font-family:'Malgun Gothic',sans-serif;color:#999;\">"
                '<span class="pageNumber"></span>'
                "</div>"
            ),
        )
        browser.close()

    size_kb = pdf_path.stat().st_size / 1024
    print(f"Done: {pdf_path} ({size_kb:.0f} KB)")


if __name__ == "__main__":
    main()
