using Ideageek.Examiner.Api.Helpers;
using Ideageek.Examiner.Api.Options;
using Ideageek.Examiner.Api.Swagger;
using Ideageek.Examiner.Core.BackgroundServices;
using Ideageek.Examiner.Core.DataSeeder;
using Ideageek.Examiner.Core.Dependencies;
using Ideageek.Examiner.Core.Helpers;
using Ideageek.Examiner.Core.Options;
using Ideageek.Examiner.Core.Repositories;
using Ideageek.Examiner.Core.Repositories.Interfaces;
using Ideageek.Examiner.Core.Services;
using Ideageek.Examiner.Core.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SqlKata.Compilers;
using SqlKata.Execution;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var connectionString = builder.Configuration.GetConnectionString("ExaminerDb")
    ?? throw new InvalidOperationException("Connection string 'ExaminerDb' is missing.");

builder.Services.Configure<DatabaseOptions>(options => options.ConnectionString = connectionString);
builder.Services.Configure<DefaultSheetOptions>(builder.Configuration.GetSection("DefaultSheet"));
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddSingleton<SqlServerCompiler>();
builder.Services.AddScoped<QueryFactory>(sp =>
{
    var factory = sp.GetRequiredService<IDbConnectionFactory>();
    var connection = factory.CreateOpenConnectionAsync().GetAwaiter().GetResult();
    var compiler = sp.GetRequiredService<SqlServerCompiler>();
    var logger = sp.GetRequiredService<ILogger<QueryFactory>>();
    var queryFactory = new QueryFactory(connection, compiler)
    {
        Logger = compiled => logger.LogDebug(compiled.Sql)
    };
    return queryFactory;
});

// Dependencies
builder.Services.AddSingleton<IQrCodeGenerator, QrCodeGenerator>();

builder.Services.AddScoped<ISchoolRepository, SchoolRepository>();
builder.Services.AddScoped<IClassRepository, ClassRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IExamRepository, ExamRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IAnswerSheetRepository, AnswerSheetRepository>();

builder.Services.AddScoped<ISchoolService, SchoolService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IAnswerSheetService, AnswerSheetService>();
builder.Services.AddScoped<IQuestionSheetService, QuestionSheetService>();
builder.Services.AddScoped<IEvaluationService, EvaluationService>();

builder.Services.AddScoped<IDataSeeder, DataSeeder>();
builder.Services.AddHostedService<DataSeedingBackgroundService>();

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
    options.OperationFilter<FormFileUploadOperationFilter>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
