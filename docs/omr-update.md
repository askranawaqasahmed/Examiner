# OMR Image Flow

## Summary
- Added `QuestionSheetImageExporter` to render the same OMR layout directly as PNG or TIFF using SkiaSharp and ImageSharp so the user can download raster sheets instead of PDFs.
- Shared layout and detection constants live in `OmrLayout`/`OmrDetectionConfig`, keeping both PDF (`PdfOmrReader`) and raster (`ImageOmrReader`) readers in sync with how bubbles are located and marked.
- Introduced `ImageOmrReader` to extract metadata from the QR payload and analyse uploaded PNG/TIFF files with OpenCvSharp, reporting which bubbles are filled; it is now DI-registered alongside the PDF reader.
- `QuestionSheetHandler` and `QuestionSheetController` expose the new endpoints (`GET /api/question-sheets/dummy/image` and `POST /api/question-sheets/dummy/image/evaluate-upload`) while keeping the existing PDF workflow intact.

## New APIs
- `GET /api/question-sheets/dummy/image?format={png|tiff}` — returns the rasterized OMR sheet with generated QR, student info, and 10 questions. Defaults to PNG when `format` is missing/invalid.
- `POST /api/question-sheets/dummy/image/evaluate-upload` — accepts a multipart `file` upload (Swagger shows a `file` content-type field) and uses `ImageOmrReader` to detect filled bubbles, returning the same evaluation DTO as the PDF endpoint.

## Testing
- `dotnet build Ideageek.Examiner.sln -c Debug -o bin-temp` passes with existing warnings (`NETSDK1194` from solution-level `--output` and several `NU190x` advisories for `SixLabors.ImageSharp 3.1.4`, which is required to encode TIFFs from the PNG output).
- If needed, run `OMR_DEBUG=1 dotnet run --project bin-temp/PdfReaderCli/PdfReaderCli.csproj` (new helper CLI) to inspect detection ratios or the generated debug PNG in `bin-temp/omr-debug/page1.png`.

## Notes
- TIFF output is derived from the PNG snapshot by re-encoding with ImageSharp because SkiaSharp does not expose `SKEncodedImageFormat.Tiff`.
- The raster reader relies on the same QR payload format as the PDF reader, so both flows continue to share the answer sheet metadata pipeline.
- The QR payload now uses Pascal-case property names (`SheetCode`, `StudentNumber`, `ExamId`, `QuestionCount`) so ZXing can deserialize the metadata record and the `/dummy/image/evaluate-upload` call consistently finds the sheet details.
- When the uploaded image carries valid QR metadata, the handler now auto-creates the `AnswerSheet` row (using the embedded exam + student) before running the evaluation so the endpoint works even if the sheet was produced externally.
- The QR metadata reader now tolerates older camel-case payloads (`sheetCode`, `studentNumber`, `examId`, `questionCount`) via case-insensitive JSON parsing, so legacy sheets won’t be rejected.
