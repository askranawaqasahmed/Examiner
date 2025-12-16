
import argparse
import base64
import io
import json
import random
import textwrap
from pathlib import Path
from typing import Any, Dict, List, Tuple

try:
    import cv2
except Exception:
    cv2 = None
import numpy as np
import qrcode
from PIL import Image, ImageDraw, ImageFont
IMAGE_SIZE = (2480, 3508)  # width, height
BACKGROUND_COLOR = 255
SCHOOL_NAME = "Sample Academy"
HEADER_TITLE = "Examiner OMR Demo"
EXAM_ID = "EXAM-001"
STUDENT_ID = "12345"
HEADER_TITLE_POS = (180, 120)
HEADER_FONT_SIZE = 64
BODY_FONT_SIZE = 42
SMALL_FONT_SIZE = 32

QUESTION_START_Y = 500
LINE_SPACING = 180
BUBBLE_RADIUS = 35
OPTION_SPACING_X = 150
QUESTION_NUMBER_OFFSET_X = 150
FIRST_OPTION_X = 350
DEFAULT_OPTIONS_ORDER = ["A", "B", "C", "D"]
QUESTION_TEXT_X = 280
TEXT_WRAP_WIDTH = 55
OPTION_COLUMN_GAP = 80
OPTION_WRAP_WIDTH = 30


def build_option_labels(options_per_question: int) -> List[str]:
    if options_per_question <= 0:
        raise SystemExit("template.optionsPerQuestion must be a positive integer.")
    labels: List[str] = []
    for idx in range(options_per_question):
        if idx < 26:
            labels.append(chr(ord("A") + idx))
        else:
            labels.append(str(idx + 1))
    return labels


def coerce_options(
    raw_options: Any, fallback_order: List[str] | None
) -> Tuple[List[str], Dict[str, str]]:
    options_order: List[str] = []
    options_map: Dict[str, str] = {}

    def option_text(value: Any) -> str:
        if isinstance(value, dict):
            if "text" in value:
                return str(value["text"])
            return str(value.get("value") or value.get("label") or value)
        return str(value)

    if isinstance(raw_options, dict):
        options_order = list(raw_options.keys())
        options_map = {k: option_text(v) for k, v in raw_options.items()}
    elif isinstance(raw_options, list):
        options_order = build_option_labels(len(raw_options))
        options_map = {label: option_text(val) for label, val in zip(options_order, raw_options)}

    if not options_order:
        options_order = fallback_order or DEFAULT_OPTIONS_ORDER
        options_map = {label: f"Option {label}" for label in options_order}

    return options_order, options_map


def load_font(size: int) -> ImageFont.FreeTypeFont | ImageFont.ImageFont:
    # Try a few common fonts; fallback to default if unavailable.
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


def build_qr(student_id: str, exam_id: str, size: int = 300) -> Image.Image:
    data = f"student={student_id};exam={exam_id}"
    qr = qrcode.QRCode(
        version=1,
        error_correction=qrcode.constants.ERROR_CORRECT_M,
        box_size=10,
        border=2,
    )
    qr.add_data(data)
    qr.make(fit=True)
    img = qr.make_image(fill_color="black", back_color="white").convert("L")
    return img.resize((size, size), resample=Image.NEAREST)


def draw_centered_text(draw: ImageDraw.ImageDraw, text: str, center: Tuple[int, int], font: ImageFont.ImageFont) -> None:
    bbox = draw.textbbox((0, 0), text, font=font)
    w = bbox[2] - bbox[0]
    h = bbox[3] - bbox[1]
    draw.text((center[0] - w // 2, center[1] - h // 2), text, fill=0, font=font)


def compute_bubble_positions(questions: List[Dict], fallback_order: List[str]) -> List[Dict]:
    positions: List[Dict] = []
    for idx, question in enumerate(questions):
        options_order = question.get("options_order") or fallback_order or DEFAULT_OPTIONS_ORDER
        available_width = IMAGE_SIZE[0] - FIRST_OPTION_X - 200
        option_spacing = OPTION_SPACING_X if len(options_order) > 1 else 0
        if len(options_order) > 1:
            option_spacing = min(OPTION_SPACING_X, max(90, available_width // (len(options_order) - 1)))

        y = QUESTION_START_Y + idx * LINE_SPACING
        bubbles: Dict[str, Tuple[int, int]] = {}
        for option_index, option in enumerate(options_order):
            x = FIRST_OPTION_X + option_index * option_spacing
            bubbles[option] = (x, y)
        positions.append({"question_id": idx + 1, "bubbles": bubbles})
    return positions


def build_default_questions(num_questions: int, options_order: List[str]) -> List[Dict]:
    questions: List[Dict] = []
    for idx in range(num_questions):
        q_id = idx + 1
        options = {opt: f"Option {opt}{q_id}" for opt in options_order}
        correct = options_order[0] if options_order else "A"
        questions.append(
            {
                "id": q_id,
                "number": q_id,
                "questionNumber": q_id,
                "text": f"Dummy Question {q_id}",
                "options": options,
                "options_order": options_order,
                "correct": correct,
            }
        )
    return questions


def load_exam_payload(raw_input: str, base_dir: Path) -> Any:
    candidate_paths = [Path(raw_input), base_dir / raw_input]
    for path in candidate_paths:
        if path.exists():
            with path.open("r", encoding="utf-8-sig") as handle:
                return json.load(handle)
    try:
        return json.loads(raw_input)
    except json.JSONDecodeError as exc:
        raise SystemExit(
            f"Could not parse exam input. Provide a JSON string or path to a JSON file. Error: {exc}"
        ) from exc


def build_run_config(payload: Any) -> Tuple[List[Dict], List[str], Dict[str, str], List[str] | None]:
    # Figure out template defaults up-front
    template = payload.get("template", {}) if isinstance(payload, dict) else {}
    template_options_count = int(template.get("optionsPerQuestion", 0)) if template else 0
    template_options_order = build_option_labels(template_options_count) if template_options_count > 0 else None

    def normalize_questions(raw_questions: List[Dict]) -> List[Dict]:
        normalized: List[Dict] = []
        for idx, qraw in enumerate(raw_questions):
            q_id = qraw.get("id") or idx + 1
            question_number = qraw.get("questionNumber") or qraw.get("number") or q_id
            text = qraw.get("text", f"Question {question_number}")
            options_order, options_map = coerce_options(
                qraw.get("options") or qraw.get("option") or {}, template_options_order
            )
            normalized.append(
                {
                    "id": q_id,
                    "number": question_number,
                    "questionNumber": question_number,
                    "text": text,
                    "options": options_map,
                    "options_order": options_order,
                    "correct": qraw.get("correct"),
                }
            )
        return normalized

    # New exam spec format with explicit questions array
    if isinstance(payload, dict) and "questions" in payload:
        questions_raw = payload.get("questions") or []
        questions = normalize_questions(questions_raw)

        if not questions:
            question_count = int(payload.get("questionCount", 0))
            if question_count <= 0:
                raise SystemExit("questionCount must be a positive integer.")
            fallback_order = template_options_order or DEFAULT_OPTIONS_ORDER
            questions = build_default_questions(question_count, fallback_order)

        options_order = template_options_order or (questions[0].get("options_order") if questions else DEFAULT_OPTIONS_ORDER)
        meta = {
            "exam_name": payload.get("examName", HEADER_TITLE),
            "exam_id": payload.get("examId", EXAM_ID),
            "template_name": template.get("name", "Template"),
        }
        correct_answers = [q.get("correct") for q in questions]
        if not any(correct_answers):
            correct_answers = None
        return questions, options_order, meta, correct_answers

    # Legacy exam spec (questionCount + template only)
    if isinstance(payload, dict) and payload.get("questionCount") is not None:
        question_count = int(payload.get("questionCount", 0))
        if question_count <= 0:
            raise SystemExit("questionCount must be a positive integer.")

        options_order = template_options_order or DEFAULT_OPTIONS_ORDER
        questions = build_default_questions(question_count, options_order)
        meta = {
            "exam_name": payload.get("examName", HEADER_TITLE),
            "exam_id": payload.get("examId", EXAM_ID),
            "template_name": template.get("name", "Template"),
        }
        return questions, options_order, meta, None

    # List-of-questions format (legacy questions.json)
    if isinstance(payload, list):
        questions = normalize_questions(payload)
        options_order = questions[0].get("options_order") if questions else DEFAULT_OPTIONS_ORDER
        meta = {
            "exam_name": HEADER_TITLE,
            "exam_id": EXAM_ID,
            "template_name": "questions.json",
        }
        correct_answers = [q.get("correct") for q in questions]
        if not any(correct_answers):
            correct_answers = None
        return questions, options_order, meta, correct_answers

    raise SystemExit(
        "Unsupported exam payload. Provide either an object with questionCount/template(+questions) or a list of questions."
    )


def derive_question_numbers(questions: List[Dict]) -> List[int]:
    numbers: List[int] = []
    for idx, question in enumerate(questions):
        candidate = question.get("questionNumber") or question.get("number") or idx + 1
        try:
            numbers.append(int(candidate))
        except (TypeError, ValueError):
            numbers.append(idx + 1)
    return numbers


def generate_sheet(
    questions: List[Dict],
    header_options_order: List[str],
    exam_name: str,
    exam_id: str,
    template_name: str,
    student_id: str,
    image_path: Path | None = None,
    fill_random: bool = False,
) -> Tuple[List[str], List[Dict], Image.Image]:
    positions = compute_bubble_positions(questions, header_options_order)
    image = Image.new("L", IMAGE_SIZE, BACKGROUND_COLOR)
    draw = ImageDraw.Draw(image)
    header_font = load_font(HEADER_FONT_SIZE)
    body_font = load_font(BODY_FONT_SIZE)
    small_font = load_font(SMALL_FONT_SIZE)

    # Header block
    header_bottom = 420
    draw.rectangle(
        [(120, 60), (IMAGE_SIZE[0] - 120, header_bottom)], outline=0, width=4
    )
    draw.text((HEADER_TITLE_POS[0], HEADER_TITLE_POS[1] - 70), SCHOOL_NAME, fill=0, font=body_font)
    draw.text(HEADER_TITLE_POS, exam_name, fill=0, font=header_font)
    draw.text((HEADER_TITLE_POS[0], HEADER_TITLE_POS[1] + 80), f"Exam ID: {exam_id}", fill=0, font=body_font)
    draw.text((HEADER_TITLE_POS[0], HEADER_TITLE_POS[1] + 140), f"Student ID: {student_id}", fill=0, font=body_font)
    draw.text(
        (HEADER_TITLE_POS[0], HEADER_TITLE_POS[1] + 200),
        f"Template: {template_name} ({len(header_options_order)} options)",
        fill=0,
        font=body_font,
    )

    # QR code on the right
    qr_img = build_qr(student_id, exam_id, size=320)
    qr_pos = (IMAGE_SIZE[0] - qr_img.width - 180, 80)
    image.paste(qr_img, qr_pos)
    draw.text((qr_pos[0], qr_pos[1] + qr_img.height + 10), "Scan for exam + student", fill=0, font=small_font)
    # Separator line to keep header distinct from questions
    draw.line([(120, header_bottom), (IMAGE_SIZE[0] - 120, header_bottom)], fill=0, width=3)

    student_answers: List[str] = []
    for question, position in zip(questions, positions):
        y = list(position["bubbles"].values())[0][1]
        display_number = question.get("questionNumber", question.get("number", question["id"]))
        draw.text((QUESTION_NUMBER_OFFSET_X, y - BUBBLE_RADIUS), f"Q{display_number})", fill=0, font=body_font)

        options_order = question.get("options_order") or header_options_order

        chosen = random.choice(options_order) if fill_random else ""
        student_answers.append(chosen)

        for option in options_order:
            x, y_center = position["bubbles"][option]
            bbox = [
                x - BUBBLE_RADIUS,
                y_center - BUBBLE_RADIUS,
                x + BUBBLE_RADIUS,
                y_center + BUBBLE_RADIUS,
            ]
            draw.ellipse(bbox, outline=0, width=3)
            draw_centered_text(draw, option, (x, y_center), body_font)
            if fill_random and option == chosen:
                draw.ellipse(bbox, fill=0, outline=0)

    if image_path:
        image.save(image_path)
    return student_answers, positions, image


def generate_question_sheet(
    questions: List[Dict],
    header_options_order: List[str],
    exam_name: str,
    exam_id: str,
    template_name: str,
    student_id: str,
    image_path: Path | None = None,
) -> Tuple[List[Dict], Image.Image]:
    image = Image.new("L", IMAGE_SIZE, BACKGROUND_COLOR)
    draw = ImageDraw.Draw(image)
    header_font = load_font(HEADER_FONT_SIZE)
    body_font = load_font(BODY_FONT_SIZE)
    small_font = load_font(SMALL_FONT_SIZE)

    header_bottom = 420
    draw.rectangle(
        [(120, 60), (IMAGE_SIZE[0] - 120, header_bottom)], outline=0, width=4
    )
    draw.text((HEADER_TITLE_POS[0], HEADER_TITLE_POS[1] - 70), SCHOOL_NAME, fill=0, font=body_font)
    draw.text(HEADER_TITLE_POS, exam_name, fill=0, font=header_font)
    draw.text((HEADER_TITLE_POS[0], HEADER_TITLE_POS[1] + 80), f"Exam ID: {exam_id}", fill=0, font=body_font)
    draw.text((HEADER_TITLE_POS[0], HEADER_TITLE_POS[1] + 140), f"Student ID: {student_id}", fill=0, font=body_font)
    draw.text(
        (HEADER_TITLE_POS[0], HEADER_TITLE_POS[1] + 200),
        f"Template: {template_name} ({len(header_options_order)} options)",
        fill=0,
        font=body_font,
    )
    draw.text(
        (HEADER_TITLE_POS[0], HEADER_TITLE_POS[1] + 260),
        "Question Sheet (with text + options)",
        fill=0,
        font=small_font,
    )

    qr_img = build_qr(student_id, exam_id, size=320)
    qr_pos = (IMAGE_SIZE[0] - qr_img.width - 180, 80)
    image.paste(qr_img, qr_pos)
    draw.text((qr_pos[0], qr_pos[1] + qr_img.height + 10), "Scan for exam + student", fill=0, font=small_font)
    draw.line([(120, header_bottom), (IMAGE_SIZE[0] - 120, header_bottom)], fill=0, width=3)

    records: List[Dict] = []

    current_y = QUESTION_START_Y
    line_height = max(BODY_FONT_SIZE, SMALL_FONT_SIZE) + 6
    usable_width = IMAGE_SIZE[0] - QUESTION_TEXT_X - 240
    column_width = (usable_width - OPTION_COLUMN_GAP) // 2
    left_column_x = QUESTION_TEXT_X
    right_column_x = left_column_x + column_width + OPTION_COLUMN_GAP

    for question in questions:
        display_number = question.get("questionNumber", question.get("number", question["id"]))
        draw.text((QUESTION_NUMBER_OFFSET_X, current_y), f"Q{display_number})", fill=0, font=body_font)

        options_order = question.get("options_order") or header_options_order

        # Only render question text + options (no bubbles) on the question sheet.
        question_text_lines = textwrap.wrap(str(question.get("text", "")), width=TEXT_WRAP_WIDTH)
        option_values = []
        for label in options_order:
            opt_val = question.get("options", {}).get(label, "")
            if isinstance(opt_val, dict) and "text" in opt_val:
                option_values.append(str(opt_val["text"]))
            elif isinstance(opt_val, dict):
                option_values.append(str(opt_val.get("value") or opt_val.get("label") or ""))
            else:
                option_values.append(str(opt_val))
        option_blocks: List[List[str]] = []
        for idx, value in enumerate(option_values):
            heading = f"Option {idx + 1}: "
            wrapped = textwrap.wrap(value, width=OPTION_WRAP_WIDTH) or [""]
            block_lines: List[str] = []
            for line_idx, line in enumerate(wrapped):
                prefix = heading if line_idx == 0 else " " * len(heading)
                block_lines.append(prefix + line)
            option_blocks.append(block_lines)

        text_y = current_y
        for line in question_text_lines:
            draw.text((QUESTION_TEXT_X, text_y), line, fill=0, font=body_font)
            text_y += line_height

        option_lines: List[str] = []
        for block_idx in range(0, len(option_blocks), 2):
            left_block = option_blocks[block_idx]
            right_block = option_blocks[block_idx + 1] if block_idx + 1 < len(option_blocks) else []
            max_lines = max(len(left_block), len(right_block))
            for row in range(max_lines):
                left_line = left_block[row] if row < len(left_block) else ""
                right_line = right_block[row] if row < len(right_block) else ""
                if left_line:
                    draw.text((left_column_x, text_y), left_line, fill=0, font=small_font)
                if right_line:
                    draw.text((right_column_x, text_y), right_line, fill=0, font=small_font)
                combined_line = left_line
                if right_line:
                    combined_line = (combined_line + "    " if combined_line else "") + right_line
                if combined_line:
                    option_lines.append(combined_line)
                elif left_line or right_line:
                    option_lines.append(left_line or right_line)
                else:
                    option_lines.append("")
                text_y += line_height

        records.append(
            {
                "question_id": question["id"],
                "options_order": options_order,
                "question_text": question.get("text", ""),
                "options_text": option_lines,
            }
        )

        current_y = text_y + 20  # margin between questions

    if image_path:
        image.save(image_path)
    return records, image


def detect_answers(
    image_source: Path | Image.Image | np.ndarray,
    questions: List[Dict],
    header_options_order: List[str],
    region_size: int = 20,
    question_numbers: List[int] | None = None,
) -> Tuple[List[str], List[Dict]]:
    def load_grayscale(source: Path | Image.Image | np.ndarray) -> np.ndarray:
        if isinstance(source, np.ndarray):
            return source
        if isinstance(source, Image.Image):
            return np.array(source.convert("L"))
        # Prefer OpenCV if available and functional; otherwise Pillow fallback.
        if cv2 is not None:
            image_cv = cv2.imread(str(source), cv2.IMREAD_GRAYSCALE)
            if image_cv is not None:
                return image_cv
        with Image.open(source) as img:
            return np.array(img.convert("L"))

    image = load_grayscale(image_source)
    if image is None or not hasattr(image, "shape"):
        raise FileNotFoundError(f"Unable to load image from {image_source}")

    positions = compute_bubble_positions(questions, header_options_order)
    detected_answers: List[str] = []
    per_question: List[Dict] = []

    half_region = region_size // 2
    height, width = image.shape

    for idx, (question, position) in enumerate(zip(questions, positions)):
        options_order = question.get("options_order") or header_options_order
        intensities: Dict[str, float] = {}
        for option in options_order:
            coord = position["bubbles"].get(option)
            if coord is None:
                continue
            x, y = coord
            x0 = max(0, x - half_region)
            x1 = min(width, x + half_region)
            y0 = max(0, y - half_region)
            y1 = min(height, y + half_region)
            region = image[y0:y1, x0:x1]
            avg_intensity = float(region.mean()) if region.size else 255.0
            intensities[option] = avg_intensity
        detected_option = min(intensities, key=intensities.get)
        detected_answers.append(detected_option)
        per_question.append(
            {
                "question_id": position["question_id"],
                "question_number": question_numbers[idx] if question_numbers and idx < len(question_numbers) else position["question_id"],
                "intensities": intensities,
                "detected": detected_option,
            }
        )

    return detected_answers, per_question


def evaluate(
    correct_answers: List[str], detected_answers: List[str], question_numbers: List[int] | None = None
) -> Tuple[int, int, List[Dict]]:
    results: List[Dict] = []
    correct_count = 0
    for idx, correct in enumerate(correct_answers):
        detected = detected_answers[idx]
        is_correct = correct == detected
        correct_count += 1 if is_correct else 0
        results.append(
            {
                "question_id": idx + 1,
                "question_number": question_numbers[idx] if question_numbers and idx < len(question_numbers) else idx + 1,
                "correct": correct,
                "detected": detected,
                "is_correct": is_correct,
            }
        )
    wrong_count = len(correct_answers) - correct_count
    return correct_count, wrong_count, results


def image_to_base64(image: Image.Image, fmt: str = "PNG") -> str:
    buffer = io.BytesIO()
    image.save(buffer, format=fmt)
    return base64.b64encode(buffer.getvalue()).decode("ascii")


def load_image_from_base64(value: str) -> Image.Image:
    decoded = base64.b64decode(value)
    buffer = io.BytesIO(decoded)
    return Image.open(buffer).convert("L")


def load_image_from_path(path: Path) -> Image.Image:
    with path.open("rb") as handle:
        return Image.open(handle).convert("L")


def extract_student_responses(payload: Any) -> List[str] | None:
    if not isinstance(payload, dict):
        return None
    candidate_keys = (
        "responses",
        "studentResponses",
        "student_answers",
        "answers",
        "selectedAnswers",
        "selected_answers",
    )
    for key in candidate_keys:
        raw = payload.get(key)
        if isinstance(raw, list) and raw:
            return [str(item) for item in raw]
    return None


def extract_scanned_sheet(payload: Any) -> str | None:
    if not isinstance(payload, dict):
        return None
    candidate_keys = (
        "sheetBase64",
        "answerSheetBase64",
        "scannedSheet",
        "scanned_sheet",
        "sheet",
    )
    for key in candidate_keys:
        value = payload.get(key)
        if isinstance(value, str) and value.strip():
            return value
    return None


def attach_detection_evaluation(
    result: Dict[str, Any],
    detected_answers: List[str],
    per_question: List[Dict],
    correct_answers: List[str] | None,
    question_numbers: List[int] | None,
) -> None:
    result["detected_answers"] = detected_answers
    result["detection_details"] = per_question
    if correct_answers and len(correct_answers) == len(detected_answers):
        correct_count, wrong_count, evaluation_rows = evaluate(
            correct_answers, detected_answers, question_numbers
        )
        result["evaluation"] = {
            "correct_count": correct_count,
            "wrong_count": wrong_count,
            "details": evaluation_rows,
        }
    elif correct_answers:
        result["evaluation_error"] = "Answer key count does not match detected answers; skipping score."
    else:
        result["evaluation_error"] = "No answer key provided; detection results only."


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Generate an OMR-style answer sheet from a JSON payload.")
    parser.add_argument(
        "--input",
        "-i",
        default="questions.json",
        help="Path to a JSON file (used when `--json` is omitted).",
    )
    parser.add_argument(
        "--json",
        "-j",
        default=None,
        help="Inline JSON string or path to a JSON file. Overrides --input when provided.",
    )
    parser.add_argument(
        "--mode",
        "-m",
        choices=["answerSheet", "questionSheet", "scoreCheck"],
        default="answerSheet",
        help="Choose: `answerSheet`, `questionSheet`, or `scoreCheck` (default).",
    )
    parser.add_argument(
        "--output",
        "-o",
        default="",
        help="Optional path to save the answer sheet PNG (base64 is always returned).",
    )
    parser.add_argument(
        "--questions-output",
        "-q",
        default="",
        help="Optional path to save the question sheet PNG (with text + options). Use empty string to skip.",
    )
    parser.add_argument(
        "--student-id",
        default=STUDENT_ID,
        help="Student ID to print and encode in the QR code.",
    )
    parser.add_argument(
        "--scanned-sheet",
        "-s",
        default="",
        help="Optional path to a scanned answer sheet image (used for scoring).",
    )
    parser.add_argument(
        "--fill-random",
        action="store_true",
        help="Fill one random bubble per question (useful for testing detection).",
    )
    parser.add_argument(
        "--detect",
        action="store_true",
        help="Run detection against the generated sheet (works best with --fill-random).",
    )
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    base_dir = Path(__file__).resolve().parent

    def resolve_path(value: str) -> Path | None:
        if not value:
            return None
        resolved = Path(value)
        return resolved if resolved.is_absolute() else base_dir / resolved

    scanned_sheet_path = resolve_path(args.scanned_sheet)
    payload_source = args.json or args.input
    payload = load_exam_payload(payload_source, base_dir)

    if scanned_sheet_path:
        if not scanned_sheet_path.exists():
            raise SystemExit(f"Scanned sheet file not found: {scanned_sheet_path}")
        scanned_sheet_base64 = image_to_base64(load_image_from_path(scanned_sheet_path))
        if isinstance(payload, dict):
            payload["scannedSheet"] = scanned_sheet_base64

    questions, options_order, meta, correct_answers = build_run_config(payload)
    question_numbers = derive_question_numbers(questions)

    if not questions:
        raise SystemExit("No questions provided in the input.")

    result: Dict[str, Any] = {
        "mode": args.mode,
        "exam": {
            "name": meta["exam_name"],
            "id": meta["exam_id"],
            "template": meta["template_name"],
            "options_per_question": len(options_order),
        },
        "question_count": len(questions),
        "student_id": args.student_id,
    }

    if args.mode == "answerSheet":
        answer_output = resolve_path(args.output)
        student_answers, _, sheet_image = generate_sheet(
            questions,
            options_order,
            meta["exam_name"],
            meta["exam_id"],
            meta["template_name"],
            args.student_id,
            image_path=answer_output,
            fill_random=args.fill_random,
        )
        result["image_base64"] = image_to_base64(sheet_image)
        result["student_answers"] = student_answers
        if answer_output:
            result.setdefault("saved_paths", []).append(str(answer_output))
        if args.fill_random:
            result["simulated_answers"] = student_answers
        if args.detect:
            detected_answers, per_question = detect_answers(
                sheet_image, questions, options_order, question_numbers=question_numbers
            )
            attach_detection_evaluation(
                result, detected_answers, per_question, correct_answers, question_numbers
            )
    elif args.mode == "questionSheet":
        question_output = resolve_path(args.questions_output)
        records, question_image = generate_question_sheet(
            questions,
            options_order,
            meta["exam_name"],
            meta["exam_id"],
            meta["template_name"],
            args.student_id,
            image_path=question_output,
        )
        result["records"] = records
        result["image_base64"] = image_to_base64(question_image)
        if question_output:
            result.setdefault("saved_paths", []).append(str(question_output))
    elif args.mode == "scoreCheck":
        scanned_base64 = extract_scanned_sheet(payload)
        responses = extract_student_responses(payload)
        if scanned_base64:
            sheet_image = load_image_from_base64(scanned_base64)
            detected_answers, per_question = detect_answers(
                sheet_image, questions, options_order, question_numbers=question_numbers
            )
            attach_detection_evaluation(
                result, detected_answers, per_question, correct_answers, question_numbers
            )
        elif responses and correct_answers:
            result["responses"] = responses
            if len(responses) == len(correct_answers):
                correct_count, wrong_count, evaluation_rows = evaluate(
                    correct_answers, responses, question_numbers
                )
                result["evaluation"] = {
                    "correct_count": correct_count,
                    "wrong_count": wrong_count,
                    "details": evaluation_rows,
                }
            else:
                result["evaluation_error"] = "Number of responses does not match the answer key."
        elif responses:
            result["responses"] = responses
            if correct_answers:
                result["evaluation_error"] = "Cannot evaluate responses; length mismatch with answer key."
            else:
                result["message"] = "Responses captured but no answer key provided."
        else:
            result["message"] = "Provide scanned sheet base64 (sheetBase64/answerSheetBase64) or responses for scoring."
    print(json.dumps(result, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
