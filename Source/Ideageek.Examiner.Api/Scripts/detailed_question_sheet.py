import argparse
import base64
import io
import json
import textwrap
from pathlib import Path
from typing import Any, Dict, List

from PIL import Image, ImageDraw, ImageFont

PAGE_WIDTH = 2480
PAGE_HEIGHT = 3508
MARGIN_X = 120
MARGIN_Y = 140
LINE_SPACING = 42
QUESTION_GAP = 40
HEADER_FONT_SIZE = 54
BODY_FONT_SIZE = 40
SUB_FONT_SIZE = 34
LINE_PEN_WIDTH = 3
QUESTION_TO_LINES_GAP = 80
LINE_GAP = 56


def load_font(size: int) -> ImageFont.FreeTypeFont | ImageFont.ImageFont:
    candidates = [
        "arial.ttf",
        "DejaVuSans.ttf",
        "LiberationSans-Regular.ttf",
        "C:\\Windows\\Fonts\\arial.ttf",
    ]
    for path in candidates:
        try:
            return ImageFont.truetype(path, size)
        except OSError:
            continue
    return ImageFont.load_default()


def wrap_text(text: str, width: int, font: ImageFont.ImageFont) -> List[str]:
    if not text:
        return [""]
    # Rough width estimation using average character width to avoid measuring every word.
    avg_char_width = max(font.getlength("n"), 1)
    max_chars = max(int(width // avg_char_width), 1)
    return textwrap.wrap(text, width=max_chars) or [text]


def render_sheet(payload: Dict[str, Any], sheet_label: str) -> List[Image.Image]:
    exam_name = payload.get("examName") or "Exam"
    exam_id = payload.get("examId") or "N/A"
    questions = payload.get("questions") or []

    header_font = load_font(HEADER_FONT_SIZE)
    body_font = load_font(BODY_FONT_SIZE)
    sub_font = load_font(SUB_FONT_SIZE)
    content_width = PAGE_WIDTH - (2 * MARGIN_X)

    def start_page() -> Dict[str, Any]:
        image = Image.new("RGB", (PAGE_WIDTH, PAGE_HEIGHT), color="white")
        draw = ImageDraw.Draw(image)
        draw.text((MARGIN_X, MARGIN_Y), exam_name, font=header_font, fill="black")
        draw.text((MARGIN_X, MARGIN_Y + HEADER_FONT_SIZE + 10), f"Exam ID: {exam_id}", font=sub_font, fill="black")
        draw.text(
            (PAGE_WIDTH - MARGIN_X - 300, MARGIN_Y),
            f"Sheet: {sheet_label.title()}",
            font=sub_font,
            fill="black",
        )
        start_y = MARGIN_Y + HEADER_FONT_SIZE + SUB_FONT_SIZE + 40
        return {"image": image, "draw": draw, "current_y": start_y}

    def compute_diagram_height(box_size: str, lines: int, current_y: int) -> int:
        factor = 0.45
        if "full" in box_size:
            factor = 0.95
        elif "three" in box_size or "3/4" in box_size:
            factor = 0.8
        elif "half" in box_size:
            factor = 0.65
        elif "quarter" in box_size or "1/4" in box_size:
            factor = 0.4

        target_height = max(int(PAGE_HEIGHT * factor) - (2 * MARGIN_Y), 600, lines * (LINE_SPACING + LINE_GAP))
        max_available = max(PAGE_HEIGHT - current_y - MARGIN_Y - QUESTION_GAP, 400)
        return min(target_height, max_available)

    pages: List[Image.Image] = []
    page = start_page()

    ordered = sorted(questions, key=lambda q: q.get("questionNumber") or q.get("number") or q.get("id") or 0)
    for idx, question in enumerate(ordered):
        qnum = question.get("questionNumber") or question.get("number") or (idx + 1)
        text = question.get("text") or ""
        lines = max(int(question.get("lines") or 1), 1)
        marks = question.get("marks") or 0
        q_type = question.get("type")
        is_diagram = bool(
            question.get("isDiagram")
            or question.get("diagram")
            or q_type == "diagram"
            or q_type == 2
        )
        box_size_raw = question.get("boxSize")
        box_size = str(box_size_raw or "").lower()

        wrapped = wrap_text(text, content_width, body_font)
        question_line_height = BODY_FONT_SIZE + 6
        question_text_height = len(wrapped) * (BODY_FONT_SIZE + 4)
        answer_height = (
            compute_diagram_height(box_size, lines, int(page["current_y"])) if is_diagram else lines * (LINE_SPACING + LINE_GAP)
        )
        total_needed = question_line_height + question_text_height + QUESTION_TO_LINES_GAP + answer_height + QUESTION_GAP

        if page["current_y"] + total_needed > PAGE_HEIGHT - MARGIN_Y:
            pages.append(page["image"])
            page = start_page()
            answer_height = compute_diagram_height(box_size, lines, int(page["current_y"])) if is_diagram else answer_height

        draw = page["draw"]
        current_y = int(page["current_y"])

        # Question line with marks on the right
        question_text = f"Q.{qnum}."
        marks_text = f"(Total Marks: {marks:02d})"

        draw.text((MARGIN_X, current_y), question_text, font=body_font, fill="black")
        marks_text_width = sub_font.getlength(marks_text)
        marks_x = PAGE_WIDTH - MARGIN_X - marks_text_width
        draw.text((marks_x, current_y), marks_text, font=sub_font, fill="black")

        # Box for obtained marks, next to total marks
        box_width = 180
        box_height = SUB_FONT_SIZE + 40
        box_x0 = marks_x - box_width - 12
        box_y0 = current_y - 4
        box_x1 = box_x0 + box_width
        box_y1 = box_y0 + box_height
        draw.rectangle([(box_x0, box_y0), (box_x1, box_y1)], outline="black", width=2)
        draw.text((box_x0, box_y0 - SUB_FONT_SIZE - 4), "Obtained", font=sub_font, fill="black")

        current_y += question_line_height

        # Question body
        for line in wrapped:
            draw.text((MARGIN_X, current_y), line, font=body_font, fill="black")
            current_y += BODY_FONT_SIZE + 4

        current_y += QUESTION_TO_LINES_GAP

        # Answer area
        if is_diagram:
            rect_height = answer_height
            rect_y0 = current_y
            rect_y1 = current_y + rect_height
            draw.rectangle(
                [(MARGIN_X, rect_y0), (PAGE_WIDTH - MARGIN_X, rect_y1)],
                outline="black",
                width=LINE_PEN_WIDTH,
            )
            current_y = rect_y1 + QUESTION_GAP
        else:
            for _ in range(lines):
                draw.line(
                    [(MARGIN_X, current_y), (PAGE_WIDTH - MARGIN_X, current_y)],
                    fill="black",
                    width=LINE_PEN_WIDTH,
                )
                current_y += LINE_SPACING + LINE_GAP

            current_y += QUESTION_GAP

        page["current_y"] = current_y

    pages.append(page["image"])
    return pages


def image_to_base64(image: Image.Image) -> str:
    buf = io.BytesIO()
    image.save(buf, format="PNG")
    return base64.b64encode(buf.getvalue()).decode("ascii")


def load_payload(source: str) -> Dict[str, Any]:
    path = Path(source)
    if path.exists():
        with path.open("r", encoding="utf-8-sig") as handle:
            return json.load(handle)
    return json.loads(source)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Generate detailed question sheet PNG from JSON payload.")
    parser.add_argument("--json", "-j", required=True, help="Inline JSON or path to JSON file containing questions.")
    parser.add_argument("--sheet", "-s", default="question", help="Label for the sheet (question/answer).")
    parser.add_argument("--output", "-o", default="", help="Optional path to save the PNG.")
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    payload = load_payload(args.json)
    images = render_sheet(payload, args.sheet)

    encoded_images = [image_to_base64(img) for img in images]
    result: Dict[str, Any] = {
        "image_base64": encoded_images[0],
        "images": encoded_images,
        "images_base64": encoded_images,
        "sheet": args.sheet,
        "question_count": len(payload.get("questions") or []),
        "page_count": len(encoded_images),
    }

    if args.output:
        out_path = Path(args.output)
        out_path.parent.mkdir(parents=True, exist_ok=True)
        images[0].save(out_path, format="PNG")
        result["saved_paths"] = [str(out_path)]

    print(json.dumps(result, ensure_ascii=False))


if __name__ == "__main__":
    main()
