# Examiner Setup Guide

1. **Restore tools**
   ```bash
   dotnet restore Ideageek.Examiner.sln
   ```
2. **Create the SQL Server database**
   - Open SQL Server Management Studio or `sqlcmd` pointing to your target instance.
   - Run `Source/Ideageek.Examiner.Api/Scripts/CreateDatabase.sql` to create `Examiner`, tables, and seed demo data (schools, classes, questions, sheet template, and users).
3. **Update connection string and JWT secret**
   - Edit `Source/Ideageek.Examiner.Api/appsettings*.json` and change `ConnectionStrings.ExaminerDb` to point at your SQL Server, e.g. `Server=.;Database=Examiner;User Id=sa;Password=123;TrustServerCertificate=True;MultipleActiveResultSets=true`.
   - Replace `Jwt:SigningKey` with a strong secret (minimum 32 chars) for production.
4. **Run the API**
   ```bash
   dotnet run --project Source/Ideageek.Examiner.Api/Ideageek.Examiner.Api.csproj
   ```
5. **Auth**
   - Seeded accounts (from the SQL script and data seeder):
     - Superadmin: `superadmin@examiner.com` / `SuperAdmin@123`
     - Students: `STD-0001` / `STD-0001`, `STD-0002` / `STD-0002`
   - Obtain a JWT via `POST /api/auth/login` with the credentials above, then call other endpoints with `Authorization: Bearer <token>`.
6. **Question sheet helpers**
   - For Python-based image/text generation, `GET /api/question-sheets/template/{examId}` still returns the questions, options map, and metadata that drive the script.
   - Generate resources via `GET /api/question-sheets/generate-question-sheet/{examId}` and `/generate-answer-sheet/{examId}`; both call the Python script, save the PNG to `wwwroot/Documents/Exam`, and persist the file name against the exam.
   - To calculate scored results from an uploaded answer sheet image, `POST /api/question-sheets/{examId}/calculate-score` accepts `multipart/form-data` with `studentId` (text) and `answerSheet` (file). The service saves the upload, passes it to the Python `scoreCheck` mode (`--scanned-sheet`), and returns counts/details for correct/wrong answers.
   - Legacy dummy OMR helpers still exist (`GET /api/question-sheets/dummy/pdf` and `POST /api/question-sheets/dummy/pdf/evaluate-upload`).

Swagger UI is available at `/swagger` for easy testing.

7. **API contract notes**
   - `GET /api/classes`, `GET /api/exams`, and `GET /api/students` now include descriptive names (`SchoolName` and `ClassName`) in addition to the existing identifiers.
   - Update/delete operations are exposed as `POST /api/{resource}/{id}/update` and `POST /api/{resource}/{id}/delete`, so consumers should stop calling PUT or DELETE verbs on those routes.
