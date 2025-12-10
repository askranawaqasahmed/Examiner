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
   - For Python-based image generation, use `GET /api/question-sheets/template/{examId}` to fetch questions, answers, options-per-question count, and template metadata as JSON.
   - Legacy dummy OMR endpoints remain: `GET /api/question-sheets/dummy/pdf` and `POST /api/question-sheets/dummy/pdf/evaluate-upload` (multipart `file`).

Swagger UI is available at `/swagger` for easy testing.
