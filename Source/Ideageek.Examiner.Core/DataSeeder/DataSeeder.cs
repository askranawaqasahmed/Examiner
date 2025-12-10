using System.Linq;
using Ideageek.Examiner.Core.Entities;
using Ideageek.Examiner.Core.Helpers;
using Microsoft.Extensions.Logging;
using SqlKata.Execution;

namespace Ideageek.Examiner.Core.DataSeeder;

public class DataSeeder : IDataSeeder
{
    private static readonly Guid DefaultDemoExamId = Guid.Parse("A9E7DC13-C9F7-44B0-9D13-148771AB0B1B");
    private const string SuperAdminUsername = "superadmin@examiner.com";
    private const string SuperAdminPassword = "SuperAdmin@123";

    private readonly QueryFactory _queryFactory;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(QueryFactory queryFactory, ILogger<DataSeeder> logger)
    {
        _queryFactory = queryFactory;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Examiner data seeder starting");
            await EnsureSuperAdminAsync();
            var schoolId = await EnsureSchoolAsync();
            var classes = await EnsureClassesAsync(schoolId);
            var students = await EnsureStudentsAsync(schoolId, classes);
            var examId = await EnsureExamAsync(schoolId, classes["Grade 8 - A"]);
            await EnsureQuestionsAsync(examId);
            await EnsureTemplateAsync(examId);
            _logger.LogInformation("Examiner data seeder completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Data seeding failed");
            throw;
        }
    }

    private async Task<Guid> EnsureSchoolAsync()
    {
        var school = await _queryFactory.Query("School").Where("Code", "DEMO-SCH").FirstOrDefaultAsync<School>();
        if (school is not null)
        {
            return school.Id;
        }

        var schoolId = Guid.NewGuid();
        await _queryFactory.Query("School").InsertAsync(new
        {
            Id = schoolId,
            Name = "Demo School",
            Code = "DEMO-SCH",
            Address = "Seeded by Examiner",
            CreatedAt = DateTime.UtcNow
        });

        return schoolId;
    }

    private async Task<Dictionary<string, Guid>> EnsureClassesAsync(Guid schoolId)
    {
        var classes = new Dictionary<string, (string Name, string Section)>
        {
            ["Grade 8 - A"] = ("Grade 8", "A"),
            ["Grade 9 - A"] = ("Grade 9", "A")
        };

        var result = new Dictionary<string, Guid>();
        foreach (var entry in classes)
        {
            var existing = await _queryFactory.Query("Class")
                .Where("SchoolId", schoolId)
                .Where("Name", entry.Value.Name)
                .Where("Section", entry.Value.Section)
                .FirstOrDefaultAsync<ClassEntity>();

            if (existing is not null)
            {
                result[entry.Key] = existing.Id;
                continue;
            }

            var id = Guid.NewGuid();
            await _queryFactory.Query("Class").InsertAsync(new
            {
                Id = id,
                SchoolId = schoolId,
                Name = entry.Value.Name,
                Section = entry.Value.Section,
                CreatedAt = DateTime.UtcNow
            });

            result[entry.Key] = id;
        }

        return result;
    }

    private async Task<Dictionary<string, Guid>> EnsureStudentsAsync(Guid schoolId, IReadOnlyDictionary<string, Guid> classes)
    {
        var students = new List<(string Key, string Number, string First, string Last, Guid ClassId)>
        {
            ("Ali", "STD-0001", "Ali", "Khan", classes["Grade 8 - A"]),
            ("Sara", "STD-0002", "Sara", "Ahmed", classes["Grade 9 - A"])
        };

        var result = new Dictionary<string, Guid>();
        foreach (var student in students)
        {
            var existing = await _queryFactory.Query("Student")
                .Where("StudentNumber", student.Number)
                .FirstOrDefaultAsync<Student>();

            if (existing is not null)
            {
                result[student.Key] = existing.Id;
                await EnsureStudentUserAsync(existing.Id, existing.StudentNumber);
                continue;
            }

            var id = Guid.NewGuid();
            await _queryFactory.Query("Student").InsertAsync(new
            {
                Id = id,
                SchoolId = schoolId,
                ClassId = student.ClassId,
                StudentNumber = student.Number,
                FirstName = student.First,
                LastName = student.Last,
                CreatedAt = DateTime.UtcNow
            });

            result[student.Key] = id;
            await EnsureStudentUserAsync(id, student.Number);
        }

        return result;
    }

    private async Task EnsureSuperAdminAsync()
    {
        var user = await _queryFactory.Query("UserAccount")
            .Where("Username", SuperAdminUsername)
            .FirstOrDefaultAsync<UserAccount>();

        if (user is not null)
        {
            return;
        }

        await _queryFactory.Query("UserAccount").InsertAsync(new
        {
            Id = Guid.NewGuid(),
            Username = SuperAdminUsername,
            PasswordHash = PasswordHasher.Hash(SuperAdminPassword),
            Role = "SuperAdmin",
            CreatedAt = DateTime.UtcNow
        });
    }

    private async Task EnsureStudentUserAsync(Guid studentId, string studentNumber)
    {
        var existing = await _queryFactory.Query("UserAccount")
            .Where("StudentId", studentId)
            .FirstOrDefaultAsync<UserAccount>();

        if (existing is not null)
        {
            return;
        }

        await _queryFactory.Query("UserAccount").InsertAsync(new
        {
            Id = Guid.NewGuid(),
            Username = studentNumber,
            PasswordHash = PasswordHasher.Hash(studentNumber),
            Role = "Student",
            StudentId = studentId,
            CreatedAt = DateTime.UtcNow
        });
    }

    private async Task<Guid> EnsureExamAsync(Guid schoolId, Guid classId)
    {
        var exam = await _queryFactory.Query("Exam")
            .Where("Id", DefaultDemoExamId)
            .FirstOrDefaultAsync<Exam>();

        if (exam is not null)
        {
            return exam.Id;
        }

        await _queryFactory.Query("Exam").InsertAsync(new
        {
            Id = DefaultDemoExamId,
            SchoolId = schoolId,
            ClassId = classId,
            Name = "Demo MCQ Test",
            Subject = "Mathematics",
            TotalMarks = 10,
            QuestionCount = 10,
            ExamDate = DateTime.UtcNow.Date,
            CreatedAt = DateTime.UtcNow
        });

        return DefaultDemoExamId;
    }

    private async Task EnsureQuestionsAsync(Guid examId)
    {
        var count = await _queryFactory.Query("Question").Where("ExamId", examId).CountAsync<int>();
        if (count > 0)
        {
            return;
        }

        var questions = Enumerable.Range(1, 10).Select(number => new Dictionary<string, object>
        {
            ["Id"] = Guid.NewGuid(),
            ["ExamId"] = examId,
            ["QuestionNumber"] = number,
            ["Text"] = $"Question {number}",
            ["OptionA"] = "Option A",
            ["OptionB"] = "Option B",
            ["OptionC"] = "Option C",
            ["OptionD"] = "Option D",
            ["CorrectOption"] = number % 2 == 0 ? "B" : "A"
        }).ToList();

        foreach (var row in questions)
        {
            await _queryFactory.Query("Question").InsertAsync(row);
        }
    }

    private async Task EnsureTemplateAsync(Guid examId)
    {
        var count = await _queryFactory.Query("QuestionSheetTemplate")
            .Where("ExamId", examId)
            .Where("Name", "Default OMR Template")
            .CountAsync<int>();

        if (count > 0)
        {
            return;
        }

        await _queryFactory.Query("QuestionSheetTemplate").InsertAsync(new
        {
            Id = Guid.NewGuid(),
            ExamId = examId,
            Name = "Default OMR Template",
            Description = "Template used for demo 10-question sheet.",
            IsDefault = true,
            CreatedAt = DateTime.UtcNow
        });
    }
}
