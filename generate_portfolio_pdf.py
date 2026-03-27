from pathlib import Path

from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import mm
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import HRFlowable, KeepTogether, PageBreak, Paragraph, SimpleDocTemplate, Spacer


ROOT = Path(__file__).resolve().parent
OUTPUT_DIR = ROOT / "pdf"
OUTPUT_PATH = OUTPUT_DIR / "SUNG_HYEON_KIM_Portfolio.pdf"

WEB_PORTFOLIO = "https://kimasill.github.io/"


def register_fonts() -> tuple[str, str]:
    # Prefer Noto Sans KR because some PDF viewers render Malgun subsets unreliably.
    noto = Path(r"C:\Windows\Fonts\NotoSansKR-VF.ttf")
    malgun = Path(r"C:\Windows\Fonts\malgun.ttf")
    malgun_bold = Path(r"C:\Windows\Fonts\malgunbd.ttf")

    if noto.exists():
        pdfmetrics.registerFont(TTFont("NotoSansKR", str(noto)))
        return "NotoSansKR", "NotoSansKR"

    if malgun.exists():
        pdfmetrics.registerFont(TTFont("MalgunGothic", str(malgun)))
        if malgun_bold.exists():
            pdfmetrics.registerFont(TTFont("MalgunGothic-Bold", str(malgun_bold)))
            return "MalgunGothic", "MalgunGothic-Bold"
        return "MalgunGothic", "MalgunGothic"

    return "Helvetica", "Helvetica-Bold"


FONT_NAME, FONT_BOLD = register_fonts()


def make_styles():
    base = getSampleStyleSheet()
    styles = {}

    styles["title"] = ParagraphStyle(
        "title",
        parent=base["Title"],
        fontName=FONT_BOLD,
        fontSize=24,
        leading=30,
        textColor=colors.HexColor("#1f1a17"),
        spaceAfter=8,
    )
    styles["subtitle"] = ParagraphStyle(
        "subtitle",
        parent=base["Normal"],
        fontName=FONT_NAME,
        fontSize=11.5,
        leading=18,
        textColor=colors.HexColor("#5b524c"),
        spaceAfter=18,
    )
    styles["name"] = ParagraphStyle(
        "name",
        parent=base["Heading1"],
        fontName=FONT_BOLD,
        fontSize=28,
        leading=34,
        textColor=colors.HexColor("#7a4f32"),
        alignment=TA_CENTER,
        spaceAfter=8,
    )
    styles["heading"] = ParagraphStyle(
        "heading",
        parent=base["Heading2"],
        fontName=FONT_BOLD,
        fontSize=15.5,
        leading=21,
        textColor=colors.HexColor("#1f1a17"),
        spaceBefore=4,
        spaceAfter=10,
    )
    styles["subheading"] = ParagraphStyle(
        "subheading",
        parent=base["Heading3"],
        fontName=FONT_BOLD,
        fontSize=11.5,
        leading=16,
        textColor=colors.HexColor("#7a4f32"),
        spaceBefore=2,
        spaceAfter=4,
    )
    styles["project"] = ParagraphStyle(
        "project",
        parent=base["Heading3"],
        fontName=FONT_BOLD,
        fontSize=13.5,
        leading=19,
        textColor=colors.HexColor("#1f1a17"),
        spaceBefore=4,
        spaceAfter=2,
    )
    styles["project_meta"] = ParagraphStyle(
        "project_meta",
        parent=base["BodyText"],
        fontName=FONT_NAME,
        fontSize=9.7,
        leading=14,
        textColor=colors.HexColor("#5b524c"),
        spaceAfter=6,
    )
    styles["body"] = ParagraphStyle(
        "body",
        parent=base["BodyText"],
        fontName=FONT_NAME,
        fontSize=10.3,
        leading=16.5,
        textColor=colors.HexColor("#1f1a17"),
        spaceAfter=6,
    )
    styles["meta"] = ParagraphStyle(
        "meta",
        parent=base["BodyText"],
        fontName=FONT_NAME,
        fontSize=10,
        leading=15,
        textColor=colors.HexColor("#5b524c"),
        spaceAfter=4,
    )
    styles["bullet"] = ParagraphStyle(
        "bullet",
        parent=base["BodyText"],
        fontName=FONT_NAME,
        fontSize=10.1,
        leading=15.5,
        leftIndent=14,
        firstLineIndent=-9,
        bulletIndent=0,
        textColor=colors.HexColor("#1f1a17"),
        spaceAfter=4,
    )
    styles["link"] = ParagraphStyle(
        "link",
        parent=base["BodyText"],
        fontName=FONT_NAME,
        fontSize=9.4,
        leading=13.5,
        textColor=colors.HexColor("#7a4f32"),
        spaceAfter=3,
    )
    return styles


STYLES = make_styles()


PROJECTS = [
    {
        "title": "Dawnstar",
        "meta": "Unity / C# / .NET / 1인 / 6개월",
        "summary": [
            "세계관, 클라이언트, 서버, DB 구조, 경제 시스템, 퀘스트, 전투, 파티 플레이를 직접 설계한 2D 다크 판타지 MMORPG 프로젝트입니다.",
            "인증 서버와 게임 서버를 분리하고 CommonDB를 통해 토큰과 상태를 공유했으며, Protocol Buffers 기반 패킷 파이프라인으로 반복적인 직렬화 문제를 줄였습니다.",
            "플레이어 주변 객체만 선택적으로 동기화하는 Interest Management 구조와 룸 단위 TaskQueue 흐름을 적용해 상태 충돌과 불필요한 네트워크 비용을 줄였습니다.",
        ],
        "links": [
            "https://kimasill.github.io/projects/dawnstar.html",
            "https://kimasill.github.io/projects/dawnstar-process.html",
        ],
    },
    {
        "title": "Pickpacker",
        "meta": "Unreal Engine 5 / 1인 / 개발 중",
        "summary": [
            "지하 세계 탐험과 물류 처리 루프를 결합한 협동 멀티플레이 프로젝트로, 게임플레이 프로그래밍, AI 설계, 멀티플레이 시스템을 모두 담당했습니다.",
            "주문 태그, 레시피, 배치 규칙을 데이터 중심 구조로 정리해 신규 아이템과 목적지, 퀘스트 조건을 코드 수정 없이 확장할 수 있도록 설계했습니다.",
            "서버 권한 기반 상태 판정, OnRep 기반 IK 동기화, 자유 배치 및 스태킹 구조를 구현했고, 렌더링 병목을 프로파일링해 드로우 콜과 Scene Capture 비용을 크게 줄였습니다.",
        ],
        "links": [
            "https://kimasill.github.io/projects/pickpacker.html",
            "https://kimasill.github.io/projects/pickpacker-process.html",
        ],
    },
    {
        "title": "PCGLevelGenerator",
        "meta": "Unreal Engine 5.6+ / PCG / 1인 / 2개월",
        "summary": [
            "강한 랜덤성을 가진 던전 레벨을 빠르게 생성하기 위해 만든 UE5 플러그인입니다.",
            "Grid 기반 그래프 구조와 MST 알고리즘을 활용해 방 연결을 보장하고, 다층 던전, 계단 규칙, 구조 생성, 오브젝트 배치, DataAsset 기반 설정 체계를 구축했습니다.",
            "PCG 노드만으로 해결하기 어려운 수학적 규칙은 C++과 PCGEx 커스텀 로직으로 보완해, 기획자와 아티스트가 에디터 안에서 바로 결과를 확인할 수 있는 제작 파이프라인을 완성했습니다.",
        ],
        "links": [
            "https://kimasill.github.io/projects/pcg-dungeon.html",
        ],
    },
    {
        "title": "kCars Classification",
        "meta": "Python / PyTorch / YOLOv5 / 개인 프로젝트",
        "summary": [
            "AI-Hub 데이터셋과 직접 수집한 크롤링 이미지를 결합해 35,000장 규모의 데이터셋을 구축하고, 74개 클래스를 대상으로 차량 분류 모델을 학습했습니다.",
            "데이터 편향을 분석하고 강한 증강의 한계를 검증한 뒤, 색상·밝기·구도 변화 중심의 현실적인 증강 전략을 설계해 정확도를 60.91%에서 93.88%까지 개선했습니다.",
        ],
        "links": [
            "https://kimasill.github.io/projects/car-classification.html",
        ],
    },
    {
        "title": "DevCom Tycoon",
        "meta": "Java / Swing / 핵심 2인",
        "summary": [
            "Java Swing만으로 화면 렌더링, 상점, 인벤토리, 개발 진행, 경제 구조, 아이템 배치, 직원 행동을 갖춘 경영 시뮬레이션을 제작했습니다.",
            "팝업 UI를 조작하는 동안에도 게임 상태가 지속적으로 갱신되도록 백그라운드 스레드와 독립적인 시뮬레이션 루프를 구성했습니다.",
        ],
        "links": [
            "https://kimasill.github.io/projects/devcomtycoon.html",
        ],
    },
    {
        "title": "Startup Camp - 기상천외라이더",
        "meta": "팀장 / 3인 / 2021",
        "summary": [
            "기상·기후 데이터를 활용한 오프라인 연계 배달 플랫폼 아이템으로 창업 지원사업에 참여했습니다.",
            "시장 분석, 수익 구조 설계, 발표 자료 작성, 리스크 관리, 멘토링 대응, 팀 일정 조율을 담당하며 시스템 사고를 사업 구조와 운영 플로우 설계에 적용했습니다.",
        ],
        "links": [
            "https://kimasill.github.io/projects/startup.html",
        ],
    },
]


def footer(canvas, doc):
    canvas.saveState()
    canvas.setStrokeColor(colors.HexColor("#d8cec3"))
    canvas.line(doc.leftMargin, 10 * mm, A4[0] - doc.rightMargin, 10 * mm)
    canvas.setFont(FONT_NAME, 8.5)
    canvas.setFillColor(colors.HexColor("#5b524c"))
    canvas.drawString(doc.leftMargin, 6.5 * mm, f"SUNG HYEON KIM Portfolio  |  Page {canvas.getPageNumber()}")
    canvas.drawRightString(A4[0] - doc.rightMargin, 6.5 * mm, WEB_PORTFOLIO)
    canvas.restoreState()


def paragraph(text, style="body"):
    return Paragraph(text, STYLES[style])


def bullet(text):
    return Paragraph(f"• {text}", STYLES["bullet"])


def link(url):
    return Paragraph(f'<link href="{url}">{url}</link>', STYLES["link"])


def build_story():
    story = []

    story.append(Spacer(1, 24 * mm))
    story.append(Paragraph("SUNG HYEON KIM", STYLES["name"]))
    story.append(Paragraph("프로젝트 포트폴리오", STYLES["title"]))
    story.append(
        Paragraph(
            "게임플레이 시스템, 멀티플레이 구조, UE5 기반 절차적 레벨 생성, "
            "성능 최적화, 데이터 기반 설계 경험을 중심으로 정리한 문서형 포트폴리오입니다.",
            STYLES["subtitle"],
        )
    )
    story.append(HRFlowable(width="100%", color=colors.HexColor("#7a4f32"), thickness=1.2, spaceBefore=4, spaceAfter=12))
    story.append(paragraph("<b>Email</b>  kibbel1998@gmail.com", "meta"))
    story.append(paragraph('<b>GitHub</b>  <link href="https://github.com/kimasill">https://github.com/kimasill</link>', "meta"))
    story.append(
        paragraph(
            '<b>LinkedIn</b>  <link href="https://www.linkedin.com/in/sung-hyeon-kim-a64b2922a/">'
            "linkedin.com/in/sung-hyeon-kim-a64b2922a</link>",
            "meta",
        )
    )
    story.append(paragraph(f'<b>Web Portfolio</b>  <link href="{WEB_PORTFOLIO}">{WEB_PORTFOLIO}</link>', "meta"))
    story.append(Spacer(1, 12 * mm))
    story.append(paragraph("한 줄 소개", "heading"))
    story.append(
        paragraph(
            "플레이 경험을 시스템과 구조로 구현하고, 기술적 문제를 끝까지 파고들어 실제 결과로 연결하는 게임 개발자입니다."
        )
    )
    story.append(Spacer(1, 10 * mm))
    story.append(paragraph("메인 웹 포트폴리오", "heading"))
    story.append(link(WEB_PORTFOLIO))
    story.append(link(f"{WEB_PORTFOLIO}#work"))
    story.append(PageBreak())

    story.append(paragraph("소개", "heading"))
    story.append(
        paragraph(
            "Unreal Engine 5 기반 프로젝트에서는 협동 멀티플레이, AI 감시 시스템, 데이터 기반 게임플레이, "
            "성능 최적화, 절차적 레벨 생성 플러그인까지 폭넓게 다뤄 왔습니다. 또한 Unity와 .NET 기반 MMORPG, "
            "Java Swing 기반 경영 시뮬레이션, Python 기반 딥러닝 분류 모델 등 서로 다른 환경에서도 "
            "시스템을 빠르게 이해하고 문제를 구조적으로 해결해 왔습니다."
        )
    )
    story.append(
        paragraph(
            "저는 단순히 기능을 구현하는 개발자가 아니라, 기획 의도와 플레이 감각, 그리고 이를 안정적으로 "
            "뒷받침하는 기술 구조를 함께 설계하는 개발자입니다. 새로운 기술을 빠르게 학습하고, 병목과 충돌의 원인을 "
            "끝까지 파고들며, 다양한 직군과 협업 가능한 형태로 정리해 내는 것을 강점으로 삼고 있습니다."
        )
    )
    story.append(Spacer(1, 4 * mm))
    story.append(paragraph("핵심 역량", "heading"))
    story.append(paragraph("Gameplay / System", "subheading"))
    for item in [
        "게임플레이 프로그래밍, AI 설계, 상호작용 시스템 구현",
        "서버 권한 기반 멀티플레이 구조 및 동기화 설계",
        "데이터 에셋 기반 확장 가능한 시스템 설계",
    ]:
        story.append(bullet(item))
    story.append(Spacer(1, 2 * mm))
    story.append(paragraph("Optimization / Pipeline", "subheading"))
    for item in [
        "UE5 프로파일링 기반 드로우 콜 및 렌더링 병목 개선",
        "PCG와 C++ 커스텀 로직을 활용한 제작 파이프라인 구축",
        "TaskQueue, Interest Management 등 구조적 안정성 설계",
    ]:
        story.append(bullet(item))

    story.append(Spacer(1, 4 * mm))
    story.append(paragraph("대표 링크", "heading"))
    story.append(paragraph("웹 포트폴리오와 주요 작업 목록은 아래 링크에서 바로 확인할 수 있습니다.", "body"))
    story.append(link(WEB_PORTFOLIO))
    story.append(link(f"{WEB_PORTFOLIO}#work"))

    story.append(PageBreak())
    story.append(paragraph("주요 프로젝트", "heading"))

    for project in PROJECTS[:3]:
        block = []
        block.append(Paragraph(project["title"], STYLES["project"]))
        block.append(Paragraph(project["meta"], STYLES["project_meta"]))
        for text in project["summary"]:
            block.append(bullet(text))
        block.append(Spacer(1, 1.5 * mm))
        block.append(Paragraph("관련 링크", STYLES["subheading"]))
        for url in project["links"]:
            block.append(link(url))
        block.append(Spacer(1, 4 * mm))
        block.append(HRFlowable(width="100%", color=colors.HexColor("#d8cec3"), thickness=0.7, spaceBefore=0, spaceAfter=6))
        story.append(KeepTogether(block))

    story.append(PageBreak())
    story.append(paragraph("추가 프로젝트 및 작업", "heading"))

    for project in PROJECTS[3:]:
        block = []
        block.append(Paragraph(project["title"], STYLES["project"]))
        block.append(Paragraph(project["meta"], STYLES["project_meta"]))
        for text in project["summary"]:
            block.append(bullet(text))
        block.append(Spacer(1, 1.5 * mm))
        block.append(Paragraph("관련 링크", STYLES["subheading"]))
        for url in project["links"]:
            block.append(link(url))
        block.append(Spacer(1, 4 * mm))
        block.append(HRFlowable(width="100%", color=colors.HexColor("#d8cec3"), thickness=0.7, spaceBefore=0, spaceAfter=6))
        story.append(KeepTogether(block))

    story.append(paragraph("마무리", "heading"))
    story.append(
        paragraph(
            "저는 플레이 경험을 중심으로 시스템을 설계하고, 기술적 문제를 구조적으로 해결하며, "
            "제작 효율까지 개선하는 개발자입니다. 이 PDF는 요약본이며, 더 자세한 전투 구조, 개발 과정, "
            "영상, 문서는 아래 웹 포트폴리오에서 확인하실 수 있습니다."
        )
    )
    story.append(link(WEB_PORTFOLIO))

    return story


def main():
    OUTPUT_DIR.mkdir(exist_ok=True)
    doc = SimpleDocTemplate(
        str(OUTPUT_PATH),
        pagesize=A4,
        leftMargin=18 * mm,
        rightMargin=18 * mm,
        topMargin=18 * mm,
        bottomMargin=16 * mm,
        title="SUNG HYEON KIM Portfolio",
        author="SUNG HYEON KIM",
    )
    story = build_story()
    doc.build(story, onFirstPage=footer, onLaterPages=footer)
    print(OUTPUT_PATH)


if __name__ == "__main__":
    main()
