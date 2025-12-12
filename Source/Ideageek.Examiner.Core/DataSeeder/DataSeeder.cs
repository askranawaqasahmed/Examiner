using System;
using System.Collections.Generic;
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

    private static readonly QuestionSeedDefinition[] QuestionBank = new[]
    {
        new QuestionSeedDefinition(
            Guid.Parse("86442efe-23cc-40b0-add3-7a781d28fc9a"),
            1,
            "What is the largest ocean on Earth?",
            "C",
            new[]
            {
                new QuestionSeedOption("A", "Atlantic Ocean", 1),
                new QuestionSeedOption("B", "Indian Ocean", 2),
                new QuestionSeedOption("C", "Pacific Ocean", 3),
                new QuestionSeedOption("D", "Arctic Ocean", 4)
            }),
        new QuestionSeedDefinition(
            Guid.Parse("1205d9ef-bc41-48b5-a5cd-c59d2c61d84a"),
            2,
            "Which part of the plant is responsible for making food through photosynthesis?",
            "B",
            new[]
            {
                new QuestionSeedOption("A", "Roots", 1),
                new QuestionSeedOption("B", "Leaves", 2),
                new QuestionSeedOption("C", "Stem", 3),
                new QuestionSeedOption("D", "Flowers", 4)
            }),
        new QuestionSeedDefinition(
            Guid.Parse("624c2156-e4db-4b93-9081-d87e061236c4"),
            3,
            "In which year did World War II end?",
            "C",
            new[]
            {
                new QuestionSeedOption("A", "1943", 1),
                new QuestionSeedOption("B", "1944", 2),
                new QuestionSeedOption("C", "1945", 3),
                new QuestionSeedOption("D", "1946", 4)
            }),
        new QuestionSeedDefinition(
            Guid.Parse("a64b6784-828d-4224-98a7-33b08af3f32b"),
            4,
            "What is the capital city of Australia?",
            "C",
            new[]
            {
                new QuestionSeedOption("A", "Sydney", 1),
                new QuestionSeedOption("B", "Melbourne", 2),
                new QuestionSeedOption("C", "Canberra", 3),
                new QuestionSeedOption("D", "Brisbane", 4)
            }),
        new QuestionSeedDefinition(
            Guid.Parse("c2ba3196-3565-41a3-8b3c-b16df6d6147a"),
            5,
            "What do we call animals that eat only plants?",
            "B",
            new[]
            {
                new QuestionSeedOption("A", "Carnivores", 1),
                new QuestionSeedOption("B", "Herbivores", 2),
                new QuestionSeedOption("C", "Omnivores", 3),
                new QuestionSeedOption("D", "Insectivores", 4)
            }),
        new QuestionSeedDefinition(
            Guid.Parse("be389bde-f372-45f9-82a8-218d5506cd5f"),
            6,
            "Which river is the longest in the world?",
            "B",
            new[]
            {
                new QuestionSeedOption("A", "Amazon River", 1),
                new QuestionSeedOption("B", "Nile River", 2),
                new QuestionSeedOption("C", "Yangtze River", 3),
                new QuestionSeedOption("D", "Mississippi River", 4)
            }),
        new QuestionSeedDefinition(
            Guid.Parse("4d88738e-74c4-46ed-a5e1-9fa0a855eb9f"),
            7,
            "What is the process by which water changes from liquid to gas called?",
            "B",
            new[]
            {
                new QuestionSeedOption("A", "Condensation", 1),
                new QuestionSeedOption("B", "Evaporation", 2),
                new QuestionSeedOption("C", "Precipitation", 3),
                new QuestionSeedOption("D", "Freezing", 4)
            }),
        new QuestionSeedDefinition(
            Guid.Parse("c7e2d77e-80e3-47e0-bdb3-1207e25c5c5f"),
            8,
            "Who was the first person to walk on the Moon?",
            "B",
            new[]
            {
                new QuestionSeedOption("A", "Buzz Aldrin", 1),
                new QuestionSeedOption("B", "Neil Armstrong", 2),
                new QuestionSeedOption("C", "Yuri Gagarin", 3),
                new QuestionSeedOption("D", "John Glenn", 4)
            }),
        new QuestionSeedDefinition(
            Guid.Parse("86f0f6aa-2e8a-44ef-afe3-830bb7a4b45e"),
            9,
            "How many continents are there on Earth?",
            "C",
            new[]
            {
                new QuestionSeedOption("A", "Five", 1),
                new QuestionSeedOption("B", "Six", 2),
                new QuestionSeedOption("C", "Seven", 3),
                new QuestionSeedOption("D", "Eight", 4)
            }),
        new QuestionSeedDefinition(
            Guid.Parse("039269e9-7a2f-4f4f-ab38-53d9ded45074"),
            10,
            "Is Diamond the hardest natural substance on Earth?",
            "T",
            new[]
            {
                new QuestionSeedOption("T", "True", 1),
                new QuestionSeedOption("F", "False", 2)
            })
    };

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
        var existingQuestionIds = await _queryFactory.Query("Question")
            .Where("ExamId", examId)
            .Select("Id")
            .GetAsync<Guid>();

        if (existingQuestionIds.Any())
        {
            await _queryFactory.Query("QuestionOption")
                .WhereIn("QuestionId", existingQuestionIds)
                .DeleteAsync();
        }
        await _queryFactory.Query("Question").Where("ExamId", examId).DeleteAsync();

        foreach (var question in QuestionBank)
        {
            var optionMap = question.Options.ToDictionary(o => o.Key, o => o.Text);

            await _queryFactory.Query("Question").InsertAsync(new
            {
                Id = question.Id,
                ExamId = examId,
                QuestionNumber = question.QuestionNumber,
                Text = question.Text,
                CorrectOption = question.CorrectOption
            });

            foreach (var option in question.Options)
            {
                await _queryFactory.Query("QuestionOption").InsertAsync(new
                {
                    Id = Guid.NewGuid(),
                    QuestionId = question.Id,
                    Key = option.Key,
                    Text = option.Text,
                    Order = option.Order
                });
            }
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

    private sealed record QuestionSeedDefinition(Guid Id, int QuestionNumber, string Text, string CorrectOption, QuestionSeedOption[] Options);
    private sealed record QuestionSeedOption(string Key, string Text, int Order);
}
