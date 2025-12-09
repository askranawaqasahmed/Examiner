# Examiner Setup Guide

1. **Restore tools**
   ```bash
   dotnet restore Ideageek.Examiner.sln
   ```
2. **Create the SQL Server database**
   - Open SQL Server Management Studio or `sqlcmd` pointing to your target instance.
   - Run `Source/Ideageek.Examiner.Api/Scripts/CreateDatabase.sql` to create `Examiner`, tables, and seed demo data.
3. **Update connection string if needed**
   - Edit `Source/Ideageek.Examiner.Api/appsettings*.json` and change `ConnectionStrings.ExaminerDb` to point at your SQL Server, e.g. `Server=.;Database=Examiner;User Id=sa;Password=123;TrustServerCertificate=True;MultipleActiveResultSets=true`.
4. **Run the API**
   ```bash
   dotnet run --project Source/Ideageek.Examiner.Api/Ideageek.Examiner.Api.csproj
   ```
5. **Validate dummy sheet endpoints**
   - `GET /api/question-sheets/dummy/pdf` downloads the OMR sheet; accepts the same optional query values.
   - `POST /api/question-sheets/dummy/pdf/evaluate-upload` uses Swagger’s built-in file picker (multipart/form-data). Mark bubbles digitally with an `X` or `✓` so the lightweight reader can detect selections automatically.

Swagger UI is available at `/swagger` for easy testing.
